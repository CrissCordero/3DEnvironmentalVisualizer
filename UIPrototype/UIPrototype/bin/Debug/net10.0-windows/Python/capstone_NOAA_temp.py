#just to make sure IPC works

import sys

url = sys.stdin.readline().strip();

print(f"Python received URL: {url}", flush=True)