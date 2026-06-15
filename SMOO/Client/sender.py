import socket
import struct
import sys
import threading
import time
import uuid

# A python client maintained by claude for rapid testing of new features added to the sever

# EventType enum order (matches server's EventType enum)
EVENT_JOIN_STAGE      = 0
EVENT_LEAVE_STAGE     = 1
EVENT_CHANGE_COSTUME  = 2
EVENT_GAME_SYNC       = 3

EVENT_NAMES = {
    EVENT_JOIN_STAGE:     "JoinStage",
    EVENT_LEAVE_STAGE:    "LeaveStage",
    EVENT_CHANGE_COSTUME: "ChangeCostume",
    EVENT_GAME_SYNC:      "GameSync",
}

EVENT_HEADER_FORMAT = "<HB"  # EventType(ushort) + PlayerSlot(byte)
EVENT_HEADER_SIZE   = struct.calcsize(EVENT_HEADER_FORMAT)  # 3 bytes

MAGIC   = 0x534D4F4F  # "SMOO"
VERSION = 1
FLAGS   = 0
PORT    = 5001

# PacketType enum order (matches server's PacketType enum)
PTYPE_CONNECT          = 0
PTYPE_CONNECT_ACK      = 1
PTYPE_CONNECT_SYN_ACK  = 2
PTYPE_DISCONNECT       = 3
PTYPE_PLAYER_JOIN_ROOM = 4
PTYPE_HEALTH_CHECK     = 5
PTYPE_PING             = 6
PTYPE_ACK              = 7
PTYPE_CHAT_MESSAGE         = 8
PTYPE_CHAT_MESSAGE_REQUEST = 9
PTYPE_EVENT                = 10

PTYPE_NAMES = {
    PTYPE_CONNECT:          "Connect",
    PTYPE_CONNECT_ACK:      "ConnectAck",
    PTYPE_CONNECT_SYN_ACK:  "ConnectSynAck",
    PTYPE_DISCONNECT:       "Disconnect",
    PTYPE_PLAYER_JOIN_ROOM: "PlayerJoinRoom",
    PTYPE_HEALTH_CHECK:     "HealthCheck",
    PTYPE_PING:             "Ping",
    PTYPE_ACK:              "Ack",
    PTYPE_CHAT_MESSAGE:         "ChatMessage",
    PTYPE_CHAT_MESSAGE_REQUEST: "ChatMessageRequest",
    PTYPE_EVENT:                "Event",
}

# Magic(4) Type(1) Flags(1) Version(1) RoomId(2) Seq(2) — PayloadSize is not in the header, it is derived from received byte count
HEADER_FORMAT = "<IBBBHH"
HEADER_SIZE   = struct.calcsize(HEADER_FORMAT)

# Types the server sends reliably (server expects an Ack back)
SERVER_RELIABLE_PTYPES = {PTYPE_CONNECT_ACK, PTYPE_PLAYER_JOIN_ROOM, PTYPE_DISCONNECT, PTYPE_CHAT_MESSAGE}

# Max sizes matching server constants (for malformed packet construction)
MAX_CHAT_MESSAGE_LENGTH = 512
MAX_PLAYER_NAME_LENGTH  = 50

_room_id = None  # None means not connected

def _require_room():
    if _room_id is None:
        print("  connect first")
        return None
    return _room_id

def build_packet(ptype, room_id, payload=b"", seq=0):
    header = struct.pack(HEADER_FORMAT, MAGIC, ptype, FLAGS, VERSION, room_id, seq)
    return header + payload

def parse_header(data):
    if len(data) < HEADER_SIZE:
        return None
    magic, ptype, flags, version, room_id, seq = struct.unpack_from(HEADER_FORMAT, data)
    if magic != MAGIC:
        return None
    return {"type": ptype, "flags": flags, "version": version, "room_id": room_id, "seq": seq}

def decode_payload(ptype, data, header):
    raw = data[HEADER_SIZE:]

    seq_str = f"seq={header['seq']} " if ptype in SERVER_RELIABLE_PTYPES else ""

    if ptype == PTYPE_CONNECT_ACK and len(raw) >= 18:
        session_id = uuid.UUID(bytes_le=bytes(raw[0:16]))
        room_size = raw[16]
        active_players = raw[17]
        players = []
        offset = 18
        for _ in range(active_players):
            if offset + 2 > len(raw):
                break
            player_index = raw[offset]
            name_len = raw[offset + 1]
            name = raw[offset + 2:offset + 2 + name_len].decode("utf-8", errors="replace")
            offset += 2 + name_len
            body_len = raw[offset]
            body = raw[offset + 1:offset + 1 + body_len].decode("utf-8", errors="replace")
            offset += 1 + body_len
            cap_len = raw[offset]
            cap = raw[offset + 1:offset + 1 + cap_len].decode("utf-8", errors="replace")
            offset += 1 + cap_len
            players.append(f"[{player_index}]{name!r} body={body!r} cap={cap!r}")
        players_str = " players=" + ",".join(players) if players else ""
        return f"{seq_str}RoomSize={room_size} OtherPlayers={active_players} SessionId={session_id}{players_str}"
    elif ptype == PTYPE_PLAYER_JOIN_ROOM and len(raw) >= 2:
        player_slot = raw[0]
        name_len = raw[1]
        name = raw[2:2 + name_len].decode("utf-8", errors="replace")
        offset = 2 + name_len
        body_len = raw[offset]
        body = raw[offset + 1:offset + 1 + body_len].decode("utf-8", errors="replace")
        offset += 1 + body_len
        cap_len = raw[offset]
        cap = raw[offset + 1:offset + 1 + cap_len].decode("utf-8", errors="replace")
        return f"{seq_str}slot={player_slot} name={name!r} body={body!r} cap={cap!r}"
    elif ptype == PTYPE_CONNECT and len(raw) >= 1:
        name_len = raw[0]
        name = raw[1:1 + name_len].decode("utf-8", errors="replace") if name_len > 0 else ""
        return f"name={name!r}"
    elif ptype == PTYPE_CONNECT_SYN_ACK:
        return f"AckedSeq={header['seq']}"
    elif ptype == PTYPE_ACK:
        return f"AckedSeq={header['seq']}"
    elif ptype == PTYPE_DISCONNECT and len(raw) >= 1:
        return f"{seq_str}slot={raw[0]} left"
    elif ptype == PTYPE_EVENT and len(raw) >= EVENT_HEADER_SIZE:
        event_type, player_slot = struct.unpack_from(EVENT_HEADER_FORMAT, raw)
        event_data = raw[EVENT_HEADER_SIZE:]
        event_name = EVENT_NAMES.get(event_type, f"Unknown({event_type})")
        if event_type == EVENT_GAME_SYNC:
            return None  # high-frequency, suppress real-time output
        return f"{event_name} slot={player_slot} ({len(event_data)} data bytes)"
    elif ptype == PTYPE_PING:
        return f"echo={raw.decode('utf-8', errors='replace')}" if raw else "(empty)"
    elif ptype == PTYPE_CHAT_MESSAGE and len(raw) >= 3:
        player_slot = raw[0]
        msg_len = struct.unpack_from("<H", raw, 1)[0]
        msg = raw[3:3 + msg_len].decode("utf-8", errors="replace")
        return f"{seq_str}slot={player_slot} msg={msg!r}"
    return f"({len(raw)} payload bytes)"

def send(sock, packet, server):
    sock.sendto(packet, server)
    header = parse_header(packet)
    if header:
        ptype_name = PTYPE_NAMES.get(header["type"], f"Unknown({header['type']})")
        print(f"  >> {ptype_name} to {server} | {packet.hex(' ')}")
    else:
        print(f"  >> {len(packet)} bytes to {server}")

def receive_loop(sock, server):
    global _room_id
    while True:
        try:
            data, sender = sock.recvfrom(1024)
            header = parse_header(data)
            if header:
                ptype = header["type"]
                ptype_name = PTYPE_NAMES.get(ptype, f"Unknown({ptype})")
                payload_str = decode_payload(ptype, data, header)
                if payload_str is not None:
                    print(f"\n  << {ptype_name} from {sender} | room={header['room_id']} | {payload_str}")
                    print("> ", end="", flush=True)

                if ptype == PTYPE_CONNECT_ACK:
                    _room_id = header["room_id"]
                    syn_ack = build_packet(PTYPE_CONNECT_SYN_ACK, _room_id, seq=header["seq"])
                    send(sock, syn_ack, server)
                elif ptype in SERVER_RELIABLE_PTYPES:
                    ack = build_packet(PTYPE_ACK, header["room_id"], seq=header["seq"])
                    send(sock, ack, server)
            else:
                print(f"\n  << {len(data)} bytes from {sender} (unrecognised) | {data.hex(' ')}")
                print("> ", end="", flush=True)
        except OSError:
            break

# --- packet builders ---

def packet_connect():
    room_id = int(input("room id: "))
    name = input("player name: ").encode("utf-8")
    if not (1 <= len(name) <= 30):
        print(f"  name must be 1–30 bytes (got {len(name)})")
        return None
    payload = bytes([len(name)]) + name
    return build_packet(ptype=PTYPE_CONNECT, room_id=room_id, payload=payload)

def packet_disconnect():
    global _room_id
    room_id = _require_room()
    if room_id is None:
        return None
    _room_id = None
    return build_packet(ptype=PTYPE_DISCONNECT, room_id=room_id)

def packet_health_check():
    room_id = _require_room()
    if room_id is None:
        return None
    payload = input("payload: ").encode("utf-8")
    return build_packet(ptype=PTYPE_HEALTH_CHECK, room_id=room_id, payload=payload)

def packet_ping():
    return build_packet(ptype=PTYPE_PING, room_id=0)

def packet_chat_message():
    room_id = _require_room()
    if room_id is None:
        return None
    message = input("message: ").encode("utf-8")
    if len(message) == 0:
        print("  message cannot be empty")
        return None
    return build_packet(ptype=PTYPE_CHAT_MESSAGE_REQUEST, room_id=room_id, payload=struct.pack("<H", len(message)) + message)

def packet_game_sync():
    room_id = _require_room()
    if room_id is None:
        return None
    try:
        x = float(input("x: "))
        y = float(input("y: "))
        z = float(input("z: "))
    except ValueError:
        print("  invalid float")
        return None
    event_header = struct.pack(EVENT_HEADER_FORMAT, EVENT_GAME_SYNC, 0)
    position     = struct.pack("<fff", x, y, z)
    quat_identity = struct.pack("<ffff", 0.0, 0.0, 0.0, 1.0)
    return build_packet(ptype=PTYPE_EVENT, room_id=room_id, payload=event_header + position + quat_identity)

def malformed_packet(sock, server):
    room_id = _require_room()
    if room_id is None:
        return
    print("  1. oversized payload            — triggers MaxPayloadSize gate in Room.ProcessAsync")
    print("  2. undersized payload           — triggers MinPayloadSize gate in Room.ProcessAsync")
    print("  3. string length > max allowed  — triggers StreamStringView max check (InvalidDataException)")
    print("  4. string length > actual bytes — triggers StreamStringView remaining check (InvalidDataException)")
    choice = input("  > ").strip()

    if choice == "1":
        # Payload exceeds ChatMessageRequest MaxPayloadSize (ushort prefix 2 + MaxChatMessageLength 512 = 514)
        junk = b"A" * (MAX_CHAT_MESSAGE_LENGTH + 10)
        payload = struct.pack("<H", len(junk)) + junk
        print(f"  sending ChatMessageRequest with {len(payload)}-byte payload (max allowed: {MAX_CHAT_MESSAGE_LENGTH + 2})")
        send(sock, build_packet(PTYPE_CHAT_MESSAGE_REQUEST, room_id, payload), server)

    elif choice == "2":
        # Payload below ChatMessageRequest MinPayloadSize (2 bytes for the ushort length prefix)
        print("  sending ChatMessageRequest with empty payload (min required: 2)")
        send(sock, build_packet(PTYPE_CHAT_MESSAGE_REQUEST, room_id, b""), server)

    elif choice == "3":
        # String length prefix claims more than MaxChatMessageLength — hits the first check in StreamStringView.Deserialize
        claimed = MAX_CHAT_MESSAGE_LENGTH + 1
        payload = struct.pack("<H", claimed) + b"hello"
        print(f"  sending ChatMessageRequest with length prefix={claimed} (max allowed: {MAX_CHAT_MESSAGE_LENGTH})")
        send(sock, build_packet(PTYPE_CHAT_MESSAGE_REQUEST, room_id, payload), server)

    elif choice == "4":
        # String length prefix is valid but claims more bytes than are present — hits the remaining bytes check
        claimed = 100
        actual  = b"short"
        payload = struct.pack("<H", claimed) + actual
        print(f"  sending ChatMessageRequest with length prefix={claimed} but only {len(actual)} bytes of data")
        send(sock, build_packet(PTYPE_CHAT_MESSAGE_REQUEST, room_id, payload), server)

    else:
        print("  invalid choice")

# --- menu ---

PACKETS = [
    ("connect",          packet_connect),
    ("disconnect",       packet_disconnect),
    ("health check",     packet_health_check),
    ("ping",             packet_ping),
    ("chat message",     packet_chat_message),
    ("send game sync",   packet_game_sync),
    ("malformed packet", None),
]

def main():
    host = sys.argv[1] if len(sys.argv) > 1 else "127.0.0.1"
    server = (host, PORT)
    print(f"connecting to {host}:{PORT}")

    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.bind(("0.0.0.0", 0))

    receiver = threading.Thread(target=receive_loop, args=(sock, server), daemon=True)
    receiver.start()

    while True:
        print()
        for i, (name, _) in enumerate(PACKETS):
            print(f"  {i + 1}. {name}")
        print("  q. quit")

        choice = input("\n> ").strip().lower()
        if choice in ("q", "quit", "exit"):
            break
        if not choice.isdigit() or not (1 <= int(choice) <= len(PACKETS)):
            print(f"enter a number between 1 and {len(PACKETS)}")
            continue
        idx = int(choice) - 1
        name, builder = PACKETS[idx]
        if name == "malformed packet":
            malformed_packet(sock, server)
        else:
            packet = builder()
            if packet is not None:
                send(sock, packet, server)

    sock.close()

if __name__ == "__main__":
    main()
