#!/usr/bin/env python3
import http.server, socketserver, os
from core import logger, arguments

args = arguments.get_args()

class MyHTTPHandler(http.server.SimpleHTTPRequestHandler):
    def log_message(self, format, *args):
        logger.request(self)

def start_server():
    web_dir = args.directory
    os.chdir(web_dir)
    host = args.server_address
    port = args.server_port
    Handler = MyHTTPHandler
    httpd = socketserver.TCPServer((host, port), Handler)
    try:
        logger.msg('Listening @ ',f'http://{host}:{port}','green')
        httpd.serve_forever()
        quit()
    except KeyboardInterrupt:
        httpd.server_close()
        quit()
    finally:
        httpd.server_close()
        quit()

