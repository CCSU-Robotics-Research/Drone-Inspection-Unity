import signal
import socket
import sys

running = True

def handle_sigint(_signum, _frame):
    global running
    running = False

def main():
    UDP_IP, UDP_PORT

    args = sys.argv[1:]

    if(len(args) == 0): # no input -> use default
        UDP_IP = "0.0.0.0"
        UDP_PORT = 5005
    elif(len(args) == 2): # input for each
        UDP_IP = args[1]
        UDP_PORT = args[2]

        try: # is port a valid integer ?
            UDP_PORT = int(UDP_PORT)
        except ValueError: 
            print("Port must be a number. Please provide both an IP and a port using: python udp_receiver.py [ip] [port]")
            sys.exit(1)
    elif(len(args) == 1): # missing input
        print("Please provide both an IP and a port using: python udp_receiver.py [ip] [port]")
        sys.exit(1)
    else: # >2 inputs
        print("Too many arguments.")
        sys.exit(1)

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