import socket
import struct

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

ptype = 1
version = 1
payload_size = 0

header = struct.pack("<BBh", ptype, version, payload_size)

sock.sendto(header, ("127.0.0.1", 5001))
print("sent", len(header), "bytes")
