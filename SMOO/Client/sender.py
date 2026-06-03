import socket
import struct
import sys
import threading
import time

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
PTYPE_PLAYER_INPUT     = 5
PTYPE_HEALTH_CHECK     = 6
PTYPE_PING             = 7
PTYPE_ACK              = 8

PTYPE_NAMES = {
    PTYPE_CONNECT:          "Connect",
    PTYPE_CONNECT_ACK:      "ConnectAck",
    PTYPE_CONNECT_SYN_ACK:  "ConnectSynAck",
    PTYPE_DISCONNECT:       "Disconnect",
    PTYPE_PLAYER_JOIN_ROOM: "PlayerJoinRoom",
    PTYPE_PLAYER_INPUT:     "PlayerInput",
    PTYPE_HEALTH_CHECK:     "HealthCheck",
    PTYPE_PING:             "Ping",
    PTYPE_ACK:              "Ack",
}

HEADER_FORMAT = "<IBBBHH"   # Magic(4) Type(1) Flags(1) Version(1) RoomId(2) PayloadSize(2)
HEADER_SIZE   = struct.calcsize(HEADER_FORMAT)
SEQ_SIZE      = 2  # ushort sequence number prepended to server-reliable payloads

# Types the server sends reliably (each has a leading seq ushort in the payload)
SERVER_RELIABLE_PTYPES = {PTYPE_CONNECT_ACK, PTYPE_PLAYER_JOIN_ROOM}

def build_packet(ptype, room_id, payload=b""):
    header = struct.pack(HEADER_FORMAT, MAGIC, ptype, FLAGS, VERSION, room_id, len(payload))
    return header + payload

def parse_header(data):
    if len(data) < HEADER_SIZE:
        return None
    magic, ptype, flags, version, room_id, payload_size = struct.unpack_from(HEADER_FORMAT, data)
    if magic != MAGIC:
        return None
    return {"type": ptype, "flags": flags, "version": version, "room_id": room_id, "payload_size": payload_size}

def decode_payload(ptype, data):
    # Read everything after the header; don't trust PayloadSize for variable-length packets
    raw = data[HEADER_SIZE:]

    seq_str = ""
    rest = raw
    if ptype in SERVER_RELIABLE_PTYPES and len(raw) >= SEQ_SIZE:
        seq = struct.unpack_from("<H", raw, 0)[0]
        seq_str = f"seq={seq} "
        rest = raw[SEQ_SIZE:]

    if ptype == PTYPE_CONNECT_ACK and len(rest) >= 2:
        room_size = struct.unpack_from("<H", rest, 0)[0]
        return f"{seq_str}RoomSize={room_size}"
    elif ptype == PTYPE_PLAYER_JOIN_ROOM and len(rest) >= 1:
        name_len = rest[0]
        name = rest[1:1 + name_len].decode("utf-8", errors="replace") if name_len > 0 else ""
        return f"{seq_str}Player={name!r}"
    elif ptype == PTYPE_CONNECT and len(raw) >= 1:
        name_len = raw[0]
        name = raw[1:1 + name_len].decode("utf-8", errors="replace") if name_len > 0 else ""
        return f"name={name!r}"
    elif ptype == PTYPE_CONNECT_SYN_ACK and len(raw) >= 2:
        seq = struct.unpack_from("<H", raw, 0)[0]
        return f"AckedSeq={seq}"
    elif ptype == PTYPE_ACK and len(raw) >= 2:
        seq = struct.unpack_from("<H", raw, 0)[0]
        return f"AckedSeq={seq}"
    elif ptype == PTYPE_PING:
        return f"echo={raw.decode('utf-8', errors='replace')}" if raw else "(empty)"
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
    while True:
        try:
            data, sender = sock.recvfrom(1024)
            header = parse_header(data)
            if header:
                ptype = header["type"]
                ptype_name = PTYPE_NAMES.get(ptype, f"Unknown({ptype})")
                payload_str = decode_payload(ptype, data)
                print(f"\n  << {ptype_name} from {sender} | room={header['room_id']} | {payload_str}")

                if len(data) >= HEADER_SIZE + SEQ_SIZE:
                    seq = struct.unpack_from("<H", data, HEADER_SIZE)[0]
                    if ptype == PTYPE_CONNECT_ACK:
                        # Complete the 3-way handshake
                        syn_ack = build_packet(PTYPE_CONNECT_SYN_ACK, header["room_id"], struct.pack("<H", seq))
                        send(sock, syn_ack, server)
                    elif ptype in SERVER_RELIABLE_PTYPES:
                        ack = build_packet(PTYPE_ACK, header["room_id"], struct.pack("<H", seq))
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
    room_id = int(input("room id: "))
    return build_packet(ptype=PTYPE_DISCONNECT, room_id=room_id)

def packet_player_input():
    room_id = int(input("room id: "))
    return build_packet(ptype=PTYPE_PLAYER_INPUT, room_id=room_id)

def packet_health_check():
    room_id = int(input("room id: "))
    payload = input("payload: ").encode("utf-8")
    return build_packet(ptype=PTYPE_HEALTH_CHECK, room_id=room_id, payload=payload)

def packet_ping():
    return build_packet(ptype=PTYPE_PING, room_id=0)

def spam_player_input(sock, server):
    room_id = int(input("room id: "))
    count = int(input("packet count: "))
    packet = build_packet(ptype=PTYPE_PLAYER_INPUT, room_id=room_id)
    print(f"  sending {count} PlayerInput packets at 60fps...")
    for i in range(count):
        send(sock, packet, server)
        time.sleep(1 / 60)
    print("  done.")

# --- menu ---

PACKETS = [
    ("connect",           packet_connect),
    ("disconnect",        packet_disconnect),
    ("player input",      packet_player_input),
    ("health check",      packet_health_check),
    ("ping",              packet_ping),
    ("spam player input", None),
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
        if builder is None:
            spam_player_input(sock, server)
        else:
            packet = builder()
            if packet is not None:
                send(sock, packet, server)

    sock.close()

if __name__ == "__main__":
    main()
