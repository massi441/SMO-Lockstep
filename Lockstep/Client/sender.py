import socket
import struct
import sys
import threading
import time

MAGIC = 0x534D4F4C  # "SMOL"
VERSION = 1
FLAGS   = 0
PORT = 5001

# PacketType enum order
PTYPE_JOIN_ROOM           = 0
PTYPE_JOIN_ROOM_SELF      = 1
PTYPE_JOIN_ROOM_BROADCAST = 2
PTYPE_LEAVE_ROOM          = 3
PTYPE_PLAYER_INPUT        = 4
PTYPE_HEALTH_CHECK        = 5
PTYPE_PING                = 6
PTYPE_ACK                 = 7

PTYPE_NAMES = {
    PTYPE_JOIN_ROOM:           "JoinRoom",
    PTYPE_JOIN_ROOM_SELF:      "JoinRoomSelf",
    PTYPE_JOIN_ROOM_BROADCAST: "JoinRoomBroadcast",
    PTYPE_LEAVE_ROOM:          "LeaveRoom",
    PTYPE_PLAYER_INPUT:        "PlayerInput",
    PTYPE_HEALTH_CHECK:        "HealthCheck",
    PTYPE_PING:                "Ping",
    PTYPE_ACK:                 "Ack",
}

HEADER_FORMAT = "<IBBBHH"   # Magic(4) Type(1) Flags(1) Version(1) RoomId(2) PayloadSize(2)
HEADER_SIZE   = struct.calcsize(HEADER_FORMAT)
SEQ_SIZE      = 2  # ushort sequence number in reliable packets

RELIABLE_PTYPES = {PTYPE_JOIN_ROOM, PTYPE_JOIN_ROOM_SELF, PTYPE_JOIN_ROOM_BROADCAST, PTYPE_LEAVE_ROOM, PTYPE_ACK}
ECHO_PTYPES     = {PTYPE_PING, PTYPE_HEALTH_CHECK}

# [Magic: 4 LE][Type: 1][Flags: 1][Version: 1][RoomId: 2 LE][PayloadSize: 2 LE][Payload]
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
    is_reliable = ptype in RELIABLE_PTYPES
    payload_start = HEADER_SIZE + (SEQ_SIZE if is_reliable else 0)
    payload = data[payload_start:]

    seq_str = ""
    if is_reliable and len(data) >= HEADER_SIZE + SEQ_SIZE:
        seq = struct.unpack_from("<H", data, HEADER_SIZE)[0]
        seq_str = f"seq={seq} "

    if ptype in (PTYPE_JOIN_ROOM, PTYPE_JOIN_ROOM_BROADCAST, PTYPE_LEAVE_ROOM) and len(payload) >= 1:
        port = struct.unpack_from("B", payload, 0)[0]
        return f"{seq_str}PlayerPort={port}"
    elif ptype == PTYPE_JOIN_ROOM_SELF and len(payload) >= 2:
        self_port, other_count = struct.unpack_from("BB", payload, 0)
        ports = [struct.unpack_from("B", payload, 2 + i)[0] for i in range(other_count) if 2 + i < len(payload)]
        return f"{seq_str}SelfPort={self_port}, OtherPlayers={other_count}, Ports={ports}"
    elif ptype == PTYPE_ACK and len(payload) >= 2:
        seq = struct.unpack_from("<H", payload, 0)[0]
        return f"{seq_str}AckedSeq={seq}"
    if ptype in ECHO_PTYPES and len(payload) > 0:
        return f"{seq_str}echo={payload.decode('utf-8', errors='replace')}"
    return f"{seq_str}({len(payload)} payload bytes)"

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
                print(f"\n  << {ptype_name} from {sender} | room={header['room_id']} flags={header['flags']:#04x} | {payload_str}")

                if ptype in RELIABLE_PTYPES and len(data) >= HEADER_SIZE + SEQ_SIZE:
                    seq = struct.unpack_from("<H", data, HEADER_SIZE)[0]
                    ack = build_packet(PTYPE_ACK, header["room_id"], struct.pack("<H", seq))
                    send(sock, ack, server)
            else:
                print(f"\n  << {len(data)} bytes from {sender} (unrecognised) | {data.hex(' ')}")
            print("> ", end="", flush=True)
        except OSError:
            break

# --- packet builders ---

def packet_join_room():
    room_id = int(input("room id: "))
    name = input("player name: ").encode("utf-8")
    payload = bytes([len(name)]) + name
    return build_packet(ptype=PTYPE_JOIN_ROOM, room_id=room_id, payload=payload)

def packet_leave_room():
    room_id = int(input("room id: "))
    return build_packet(ptype=PTYPE_LEAVE_ROOM, room_id=room_id)

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
    ("join room",           packet_join_room),
    ("leave room",          packet_leave_room),
    ("player input",        packet_player_input),
    ("health check",        packet_health_check),
    ("ping",                packet_ping),
    ("spam player input",   None),
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
        name, builder = PACKETS[int(choice) - 1]
        if builder is None:
            spam_player_input(sock, server)
        else:
            send(sock, builder(), server)

    sock.close()

if __name__ == "__main__":
    main()
