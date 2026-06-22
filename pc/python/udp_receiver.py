import signal
import socket
import sys

UDP_IP = "0.0.0.0"
UDP_PORT = 5005

running = True

def handle_sigint():
    global running
    running = False

def main():
    global running

    signal.signal(signal.SIGINT, handle_sigint)

    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.bind((UDP_IP, UDP_PORT))
    sock.settimeout(0.25)

    print(f"Listening on {UDP_IP}:{UDP_PORT}")

    try:
        while running:
            try:
                data, addr = sock.recvfrom(1024)
                message = data.decode("utf-8", errors="replace")
                print(f"From {addr}: {message}")
            except socket.timeout:
                continue
            except OSError as e:
                if running:
                    print(f"Socket error: {e}")
                break
    finally:
        sock.close()
        print("Receiver stopped.")
        sys.exit(0)


if __name__ == "__main__":
    main()