import socket
import struct

MAGIC = 0x534D4F4C  # "SMOL"
VERSION = 1

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind(("0.0.0.0", 0))

ptype = 0    # Connect
room_id = 0
payload = bytes([0xAB]) * 10
payload_size = len(payload)

# [Magic: 4 LE][Type: 1][RoomId: 2 LE][Version: 1][PayloadSize: 2 LE][Payload]
header = struct.pack("<IBHBH", MAGIC, ptype, room_id, VERSION, payload_size)
packet = header + payload

sock.sendto(packet, ("127.0.0.1", 5001))
print("sent", len(packet), "bytes")

sock.settimeout(2.0)
try:
    data, sender = sock.recvfrom(1024)
    print("received", len(data), "bytes from", sender)
    print("data:", data)
except socket.timeout:
    print("no reply within timeout")