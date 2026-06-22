import socket
import sys
import time

DEFAULT_IP = "127.0.0.1"
DEFAULT_PORT = 5005

def main():
    ip = sys.argv[1] if len(sys.argv) > 1 else DEFAULT_IP
    port = int(sys.argv[2]) if len(sys.argv) > 2 else DEFAULT_PORT

    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    try:
        for i in range(5):
            message = f"test message {i + 1}"
            sock.sendto(message.encode("utf-8"), (ip, port))
            print(f"Sent to {ip}:{port}: {message}")
            time.sleep(1)
    finally:
        sock.close()

if __name__ == "__main__":
    main()