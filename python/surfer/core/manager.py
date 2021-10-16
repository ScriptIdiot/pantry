#!/usr/bin/python3
import threading
from queue import Queue
from core import cli, server

threads = 2
services = [1, 2]
queue = Queue()

def work():
    while True:
        x = queue.get()
        if x == 1: # set job 1 to be the serving connections
            server.start_server()
        if x == 2: # set job 2 to be the c2 cli
            cli.start_cli()
        queue.task_done()

def create_workers():
    for thread in range(threads):
        thread = threading.Thread(target=work)
        thread.daemon = True
        thread.start()

def create_jobs():
    for service in services:
        queue.put(service)
    queue.join()

def start():
    try:
        create_workers()
        create_jobs()
    except KeyboardInterrupt as e:
        quit()