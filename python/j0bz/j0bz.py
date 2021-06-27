#!/usr/bin/env python3
import os, datetime, math, json, argparse, uuid
from multiprocessing import Pool
from libnmap.parser import NmapParser
from core import logger, banner, arguments

class Scan:
	def __init__(self, name, uid, command, path, scope):
		self.name = name
		self.uid = uid
		self.command = command
		self.path = path
		self.scope = scope

class j0bz:
	def __init__(self, client_name, scope_file, output, use_chunks=False, find_alive=True, top_ports=100, verbose_level=0, exclude=None, threads=2):
		self.client_name = client_name
		self.scope_file = scope_file
		self.use_chunks = use_chunks
		self.find_alive = find_alive
		self.top_ports = top_ports
		self.verbose_level = verbose_level
		self.output = output
		self.threads = threads
		self.exclude = exclude

		if self.exclude != None:
			if ',' in self.exclude:
				self.exclude = self.exclude.split(',')
			else:
				self.exclude = [self.exclude]

			self.exclude = [e for e in self.exclude if e.strip()]
			self.nmap_base = f'sudo nmap --exclude {",".join(self.exclude)}'
		else:
			self.nmap_base = 'sudo nmap'

		if self.verbose_level > 0:
			self.hide_output = ''
			self.verbosity_level = '-' + ('v' * self.verbose_level)
		else:
			self.hide_output = '2>/dev/null 1>&2'
			self.verbosity_level = ''

		self.cmd_nmap_icmp_base = f'{self.nmap_base} -sn {self.verbosity_level}'
		self.cmd_nmap_tcp_port_base = f'{self.nmap_base} {self.verbosity_level} --open --reason -sV'
		self.cmd_nmap_tcp_top_ports_base = f'{self.nmap_base} {self.verbosity_level} --open --reason -sSCV --top-ports {str(self.top_ports)}'

		if self.output == None:
			self.output = os.getcwd() + '/'
		else:
			if not self.output.endswith('/'):
				self.output = self.output + '/'
			else:
				self.output = self.output

		self.json_file = os.getcwd() + '/' + 'services.json'

		self.project_directory = self.get_project_directory()
		self.nmap_directory, self.host_directory = self.get_subdirectories()
		self.scope = self.get_scope()
		self.nmap_icmp_commands = self.nmap_get_alive_commands()
		self.nmap_tcp_portscan_commands = self.nmap_tcp_port_scan()
		self.nmap_service_scans = self.parse_json_scans()

	def make_directory(self, path):
		try:
			if not os.path.exists(path):
				os.makedirs(path)
				logger.good('Created Directory: ' + path)
				return path
			else:
				logger.bad(path + ' exists!')
				return path
		except Exception as e:
			logger.bad('Error making directory' + str(e))
			return None		

	def get_project_directory(self):
		dt = datetime.date.today().strftime("%b_%d_%Y")
		cn = self.client_name.replace(' ', '_')
		directory_name = f'{cn}-{dt}'
		path = self.output + directory_name + '/'
		return self.make_directory(path)

	def get_subdirectories(self):
		nmap_directory = self.project_directory + 'nmap/'
		hosts_directory = self.project_directory + 'hosts/'

		if not self.make_directory(nmap_directory):
			return None,None
		if not self.make_directory(hosts_directory):
			return None, None

		return (nmap_directory, hosts_directory)
		

	def get_scope(self):
		s = []
		if not os.path.exists(self.scope_file):
			logger.bad(self.scope_file + ' doesnt exist!')
			return None
		try:
			with open(self.scope_file, 'r') as f:
				lines = f.readlines()
				for line in lines:
					line = line.strip('\n')
					if line not in s:
						s.append(line)
			return s
		except Exception as e:
			logger.bad('Failed to read scope: ' + str(e))
			return None

	def chunk(self, data, override=0):
		if override == 0:
			amount = math.ceil(len(data) * 100.0 / 200)
		else:
			amount = math.ceil(override * 100.0 / 200)

		n = math.ceil(len(data) / amount)
		
		chunks = []
		for i in range(0, len(data), n):
			chunks.append(data[i:i + n])
		return chunks

	def nmap_get_alive_commands(self):
		scans = []
		if self.use_chunks:
			for count, i in enumerate(self.chunk(self.scope)):
				s = ' '.join(i)
				outname = f'{self.nmap_directory}icmp_sweep_chunk{str(count)}'
				cmd = f'{self.cmd_nmap_icmp_base} {s} -oA {outname} {self.hide_output}'
				scan = Scan('ICMP Sweep' ,str(uuid.uuid4()), cmd, outname, self.scope)
				if scan not in scans:
					scans.append(scan)
		else:
			s = ' '.join(self.scope)
			outname = self.nmap_directory + 'icmp_sweep_all'
			cmd = f'{self.cmd_nmap_icmp_base} {s} -oA {outname} {self.hide_output}'
			scan = Scan('ICMP Sweep' ,str(uuid.uuid4()), cmd, outname, self.scope)
			scans.append(scan)
		return scans

	def nmap_tcp_port_scan(self):
		scans = []
		if self.use_chunks:
			for count, i in enumerate(self.chunk(self.scope)):
				s = ' '.join(i)
				outname = self.nmap_directory + f'top_{str(self.top_ports)}_ports_chunk{str(count)}'
				cmd = f'{self. cmd_nmap_tcp_top_ports_base} -oA {outname} {s} {self.hide_output}'
				scan = Scan(f'Top {str(self.top_ports)} Ports' ,str(uuid.uuid4()), cmd, outname, self.scope)
				if scan not in scans:
					scans.append(scan)
		else:
			s = ' '.join(self.scope)
			outname = self.nmap_directory + f'top_{str(self.top_ports)}_ports'
			cmd = f'{self. cmd_nmap_tcp_top_ports_base} -oA {outname} {s} {self.hide_output}'
			scan = Scan(f'Top {str(self.top_ports)} Ports' ,str(uuid.uuid4()), cmd, outname, self.scope)
			scans.append(scan)
		return scans

	def run_threaded_cmd(self, scans):
		logger.verbose('Independant Thread Commands: ' + str(len(scans)))
		for scan in scans:
			logger.verbose('Executing: ' + scan.command)
			if os.system(scan.command) == 0:
				logger.good(f'Scan Finished: {scan.name}')
				return (scan, True)
			else:
				logger.bad(f'Scan Failed: {scan.command}')
				return (scan, False)

	def execute_nmap_commands(self, scans):
		executed = []

		if not self.use_chunks or len(scans) == 1:
			for scan in scans:
				logger.verbose('Executing: ' + scan.command)
				if os.system(scan.command) == 0:
					logger.good(f'Scan Finished: {scan.name}')
					if scan not in executed:
						executed.append(scan)
				else:
					logger.bad(f'Scan Failed: {scan.command}')
					return None
			return executed

		chunks = []

		if len(scans) == 2:
			chunks = self.chunk(scans, 2)

		elif len(scans) > 2:
			chunks = self.chunk(scans)
		logger.verbose('Chunks: ' + str(len(chunks)))

		with Pool(self.threads) as p:
			results = p.map(self.run_threaded_cmd, chunks)
			p.close()
			p.join()

		if len(results) == 0:
			logger.bad('Something went wrong, no idea!')
			return None

		for result in results:
			scan = result[0]
			status = result[1]
			if not status:
				return None
			else:
				if scan not in executed:
					executed.append(scan)
		return scans

	def parse_json_scans(self):
		scans = []
		with open(self.json_file) as f:
			j = json.load(f)
		
		for service, data in j["services"].items():
			svc_short_name = service
			svc_pretty_name = data["name"]
			svc_protocol = data["protocol"]
			svc_scripts = ','.join([str(script) for script in data["scripts"]])
			svc_ports = ','.join([str(port) for port in data["ports"]])

			if svc_protocol == 'tcp':
				protocol = '-sS'
			elif svc_protocol == 'udp':
				protocol = '-sU'
			else:
				logger.red('Failed to parse protocol!')
				return None

			if len(svc_scripts) == 0:
				svc_scripts = ''
			else:
				svc_scripts = '--script ' + svc_scripts

			if self.use_chunks:
				for count, i in enumerate(self.chunk(self.scope)):
					s = ' '.join(i)
					outname = self.nmap_directory + f'{service}_chunk{str(count)}'
					cmd = f'{self.cmd_nmap_tcp_port_base} -p {svc_ports} {protocol} {s} {svc_scripts} -oA {outname} {self.hide_output}'
					scan = Scan(svc_pretty_name ,str(uuid.uuid4()), cmd, outname, self.scope)
					if scan not in scans:
						scans.append(scan)
			else:
				s = ' '.join(self.scope)
				outname = self.nmap_directory + f'{service}_all'
				cmd = f'{self.cmd_nmap_tcp_port_base} -p {svc_ports} {protocol} {s} {svc_scripts} -oA {outname} {self.hide_output}'
				scan = Scan(svc_pretty_name ,str(uuid.uuid4()), cmd, outname, self.scope)
				scans.append(scan)
		return scans

	def parse_alive_hosts(self, scans):
		logger.info('Alive Hosts: ' + self.host_directory)
		all_hosts = []
		try:

			for scan in scans:
				path = scan.path + '.xml'
				nmap = NmapParser.parse_fromfile(path)
				hosts = nmap._hosts
				for host in hosts:
					addr = host._ipv4_addr
					if addr not in all_hosts:
						all_hosts.append(addr)
		except Exception as e:
			logger.bad('Error trying to read nmap xml: ' + str(e))
			return None

		logger.bullet(all_hosts)

		try:
			with open(self.host_directory + 'alive.txt', mode='wt') as f:
				f.write('\n'.join(all_hosts))
			self.scope = all_hosts
			return all_hosts
		except Exception as e:
			logger.bad('Error writing hosts: ' + str(e))
		return None

	def parse_port_scans(self, scans):
		msgs = []
		data = {}
		for scan in scans:
			path  = scan.path + '.xml'
			try:
				nmap = NmapParser.parse_fromfile(path)
			except Exception as e:
				logger.bad('Error reading XML: ' + str(e))
				continue
			hosts = nmap._hosts
			if len(hosts) == 0:
				logger.verbose('No hosts in:' + path)
				continue
			for host in hosts:
				addr = host._ipv4_addr
				ports = []
				services = host._services
				for service in services:
					protocol = service.protocol
					port = service._portid
					state = service._state['state']
					name = service._service['name']
					if 'product' in service._service.keys():
						product = service._service['product']
						msg = f'{addr} -> {protocol}/{port} -> {state} -> {name} -> {product}'
					else:
						msg = f'{addr} -> {protocol}/{port} -> {state} -> {name}'

					if msg not in msgs:
						msgs.append(msg)

					name_port = f'{name}_{port}'

					if name_port not in data.keys():
						data[name_port] = []

					if addr not in data[name_port]:
						data[name_port].append(addr)

		logger.bullet(msgs, 1)

		for key, value in data.items():
			service = key
			hosts = value
			outname = self.host_directory + service + '.txt'

			with open(outname, 'w+') as f:
				for host in hosts:
					f.write(host + '\n')

			logger.good(f'{service} hosts written to: '  + outname)
		return data

def main():
	args = arguments.get_args()


	if os.geteuid() != 0:
		logger.bad('Re-run as root!')
		return

	banner.show()
	j = j0bz(args.client_name, args.scope_file, args.output, args.use_chunks, args.find_alive, args.top_ports, args.verbose_level, args.exclude, args.threads)

	if j.project_directory == None:
		return

	if args.find_alive:
		logger.info('Running ICMP Scan')
		scans = j.execute_nmap_commands(j.nmap_icmp_commands)
		if scans == None: return
		if j.parse_alive_hosts(scans) == None: return

	logger.info(f'Running port scan of top {str(args.top_ports)} ports')
	scans = j.execute_nmap_commands(j.nmap_tcp_portscan_commands)
	if scans  == None: return
	if j.parse_port_scans(scans) == None:  return

	logger.info(f'Running service scans')
	scans = j.execute_nmap_commands(j.nmap_service_scans)
	if scans == None: return
	if j.parse_port_scans(scans) == None: return

if __name__ == '__main__':
	main()