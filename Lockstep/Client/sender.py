import socket
import struct

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind(("0.0.0.0", 0))

ptype = 0
version = 1

payload_size = 10
payload = bytes([0xAB]) * payload_size

header = struct.pack("<BBh", ptype, version, payload_size)

sock.sendto(header + payload, ("127.0.0.1", 5001))
print("sent", len(header + payload), "bytes")

sock.settimeout(2.0)
try:
    data, sender = sock.recvfrom(1024)
    print("received", len(data), "bytes from", sender)
    print("data:", data)
except socket.timeout:
    print("no reply within timeout")