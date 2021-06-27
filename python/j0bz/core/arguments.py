import argparse

def get_args():
	parser = argparse.ArgumentParser(description='Automate all of the internal inf please.')
	parser.add_argument('-c', '--client-name', metavar='', required=True, help='Name of the client')
	parser.add_argument('-s', '--scope-file', metavar='', required=True, help='File containing the scope')
	parser.add_argument('-t', '--top-ports', metavar='', default=100, type=int, help='Top X ports to scan (Default: 100)')
	parser.add_argument('-o', '--output', metavar='', default=None, help='Directory to create the project in')
	parser.add_argument('--threads', metavar='', default=2, type=int, help='Amount of threads to use (Default: 2)')
	parser.add_argument('--verbose-level',metavar='', default=0, type=int, help='Set the verbosity level (Default: 0)')
	parser.add_argument('--exclude', metavar='', default=None, help='Comma seperated list of IPs to exclude')
	parser.add_argument('--use-chunks', default=False, action='store_true', help='Chunk scope up for better threading')
	parser.add_argument('--find-alive', default=False, action='store_true', help='Enable ICMP Scanning')
	args = parser.parse_args()
	return args	