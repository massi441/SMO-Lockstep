import socket
import struct
import sys
import threading

MAGIC = 0x534D4F4C  # "SMOL"
VERSION = 1
FLAGS   = 0
PORT = 5001

# PacketType enum order
PTYPE_JOIN_ROOM    = 0
PTYPE_LEAVE_ROOM   = 1
PTYPE_PLAYER_INPUT = 2
PTYPE_HEALTH_CHECK = 3
PTYPE_ACK          = 4

PTYPE_NAMES = {
    PTYPE_JOIN_ROOM:    "JoinRoom",
    PTYPE_LEAVE_ROOM:   "LeaveRoom",
    PTYPE_PLAYER_INPUT: "PlayerInput",
    PTYPE_HEALTH_CHECK: "HealthCheck",
    PTYPE_ACK:          "Ack",
}

HEADER_FORMAT = "<IBBBHH"   # Magic(4) Type(1) Flags(1) Version(1) RoomId(2) PayloadSize(2)
HEADER_SIZE   = struct.calcsize(HEADER_FORMAT)

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

def send(sock, packet, server):
    sock.sendto(packet, server)
    print(f"  >> sent {len(packet)} bytes")

def receive_loop(sock):
    while True:
        try:
            data, sender = sock.recvfrom(1024)
            header = parse_header(data)
            if header:
                ptype_name = PTYPE_NAMES.get(header["type"], f"Unknown({header['type']})")
                print(f"\n  << {ptype_name} from {sender} (room={header['room_id']}, v={header['version']}, flags={header['flags']:#04x})")
            else:
                print(f"\n  << {len(data)} bytes from {sender} (unrecognised)")
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

# --- menu ---

PACKETS = [
    ("join room",    packet_join_room),
    ("leave room",   packet_leave_room),
    ("player input", packet_player_input),
]

def main():
    host = sys.argv[1] if len(sys.argv) > 1 else "127.0.0.1"
    server = (host, PORT)
    print(f"connecting to {host}:{PORT}")

    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.bind(("0.0.0.0", 0))

    receiver = threading.Thread(target=receive_loop, args=(sock,), daemon=True)
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
        _, builder = PACKETS[int(choice) - 1]
        send(sock, builder(), server)

    sock.close()

if __name__ == "__main__":
    main()
