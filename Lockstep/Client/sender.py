import socket
import struct
import sys
import threading

MAGIC = 0x534D4F4C  # "SMOL"
VERSION = 1
PORT = 5001

# [Magic: 4 LE][Type: 1][RoomId: 2 LE][Version: 1][PayloadSize: 2 LE][Payload]
def build_packet(ptype, room_id, payload=b""):
    header = struct.pack("<IBHBH", MAGIC, ptype, room_id, VERSION, len(payload))
    return header + payload

def send(sock, packet, server):
    sock.sendto(packet, server)
    print(f"  >> sent {len(packet)} bytes")

def receive_loop(sock):
    while True:
        try:
            data, sender = sock.recvfrom(1024)
            print(f"\n  << received {len(data)} bytes from {sender}: {data}")
            print("\n> ", end="", flush=True)
        except OSError:
            break

# --- packet builders ---

def packet_connect():
    return build_packet(ptype=0, room_id=0)

def packet_join_room():
    room_id = int(input("room id: "))
    name = input("player name: ").encode("utf-8")
    payload = bytes([len(name)]) + name
    return build_packet(ptype=2, room_id=room_id, payload=payload)

def packet_player_input():
    room_id = int(input("room id: "))
    return build_packet(ptype=3, room_id=room_id)

# --- menu ---

PACKETS = [
    ("connect",      packet_connect),
    ("join room",    packet_join_room),
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
