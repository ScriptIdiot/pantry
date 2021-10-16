import argparse, os
from core import logger

def get_args():
	dir_path = os.path.dirname(os.path.realpath(__file__))
	default_directory = dir_path + '/' + '../' + 'server/'
	parser = argparse.ArgumentParser(description = "Giver of shells")
	parser.add_argument('-a', '--server-address', default="127.0.0.1",help="Server Address")
	parser.add_argument('-p', '--server-port', default=13004, type=int, help="Server port")
	parser.add_argument('-d', '--directory', default=default_directory, help="Directory to serve")
	args = parser.parse_args()
	return args