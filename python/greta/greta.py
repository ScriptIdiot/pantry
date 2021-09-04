#!/usr/bin/env python3

import json, sys, argparse
from core.fnv import Fnv
from core.aes import AES256

class Key:
	def __init__(self, config, before, after, fnv, password):
		self.config = config
		self.before = before
		self.after = after
		self.fnv = fnv
		self.password = password

class Greta:
	def __init__(self, config_file):
		self.config_file = config_file
		self.registry_keys = []
		self.variables_keys = []

	def parse_config(self):
		try:
			with open(self.config_file,'r') as json_file:
				config = json.load(json_file)

			if 'registry' in config.keys():
				self.registry_keys = config['registry']
				for i in range(0,len(self.registry_keys)): self.registry_keys[i]['Type'] = 'Registry'

			if 'variable' in config.keys():
				self.variables_keys = config['variable']
				for i in range(0,len(self.variables_keys)): self.variables_keys[i]['Type'] = 'Variable'

			if len(self.registry_keys) == 0 and len(self.variables_keys) == 0:
				print('[!] No keys found!')
				return False

			return True
		except Exception as e:
			print(str(e))
			return False

	def combine_lists(self):
		return self.registry_keys + self.variables_keys

	def build_crypto_keys(self, plaintext):
		combined_list = self.combine_lists()
		ciphertext = plaintext
		keys = []

		for count, environmental_key in enumerate(combined_list):
			password = environmental_key['Value']
			aes256 = AES256(password)

			if count == 0:
				before = plaintext
			else:
				before = ciphertext

			cc = aes256.encrypt(ciphertext)
			after = cc
			ciphertext = cc
			keys.append(Key(environmental_key, before, after, Fnv().calculate(password), password))

		return keys

def banner():
	print('  _,-._')
	print(' / \\_/ \\')
	print(' >-(_)-<    ~ Greta ~')
	print(' \\_/ \\_/')
	print('   `-\'	')

def pretty_hex(h):
	return '{ 0x' + ', 0x'.join(hex(x)[2:] for x in h) + ' };'

def parse_sleep(src, sleep):
	counter = 0
	new_src = ''
	if sleep > 0:
		src = src.replace('// Sleep: ','')
		import random
		for line in src.split('\n'):
			if 'zzSleep' in line and 'void' not in line:
				r = random.randint(0,sleep)
				counter += r
				line = line.replace('SLEEP', str(r))
			new_src += line + '\n'
		print(f'|> Total Execution Time: {counter / 1000}s')
		return new_src
	else:
		return src

def build_cpp(keys, output, plaintext, sleep):
	try:
		hives = { "HKEY_LOCAL_MACHINE": "HKLM", "HKEY_CURRENT_USER": "HKCU" }

		code = ''

		if len(keys) == 1:
			pos = 0
		else:
			pos = len(keys) - 1

		starting_cipher = 'std::vector<unsigned char> startingEnc = ' + pretty_hex(keys[pos].after) + '\n' # where the crypto will start from

		code += 'GRETA Greta()\n{\n'
		code += 'printf("  ~ Greta ~ \\n");\n'
		code += 'int origSz = ' + str(len(plaintext)) + ';'
		code += starting_cipher
		code += '\n'
		code += '//Debug: print_data((char*)"Starting Data", startingEnc);'
		init = '''
	GRETA greta{};
	greta.bStatus = FALSE;
	greta.payload = startingEnc;
		'''	
		code += init

		for count,key in enumerate(reversed(keys)):
			var_type = key.config['Type']
			var_value = key.config['Value']
			var_fnv = key.fnv

			if var_type == 'Variable':
				lpsubkey = '""'
				lpvaluename = '""'
				hive = '0'
				lpenv =  '"' + key.config['Variable'] + '"'
			elif var_type == 'Registry':
				lpsubkey = '"' + key.config['Path'] + '"'
				lpvaluename = '"'+ key.config['Key'] + '"'

				hive = key.config['Hive']
				if hive in hives.keys(): hive = hives[hive]

				lpenv =  '""'
			else:
				return False

			segment = '''
	printf("|> [{0}]");
	// {1} == {2}
	greta.envvar = {3};
	greta.subkey = {4};
	greta.valuename = {5};
	greta.hive = {6};
	greta.keySz = {7};
	greta.type = {8};
	greta.fnv = {9};

	greta = do_keying(&greta);

	if (greta.bStatus == FALSE)
	{{
		printf("[!] Greta Error: %s\\n", greta.errormsg.c_str());
		return{{}};
	}}
	else
	{{
		greta.envvar = "";
	}}
		'''.format(count + 1, var_type, var_value, lpenv, lpsubkey, lpvaluename, hive, len(key.password), var_type.upper(), var_fnv)

			code += segment
		code += 'printf("|> Done!\\n");\nreturn greta;\n}'

		with open('src/greta.cpp','r') as f:
			src = f.read()

			src = parse_sleep(src, sleep)

		with open(output,'w') as f:
			src += '\n\n'
			src += code
			f.seek(0)
			f.write(src)
			f.truncate()
		return True
	except Exception as e:
		print('[!] Error writing output: ' + str(e))
		return False

def debug_print(keys):
	for count,key in enumerate(keys):
		config = key.config
		before = key.before
		after = key.after
		fnv = key.fnv
		password = key.password

		if count == 0:
			print('|> Starting ')

		print(f'|> Config [{count+1}\\{len(keys)}]:')
		for k,v in config.items():
			print(" - {:<10}: {:<30}".format(k,v))

		print(" - {:<10}: {:<30}".format('Password', pretty_hex(password.encode())))
		print(" - {:<10}: {:<30}".format('Length', len(password)))
		print(" - {:<10}: {:<30}".format('Before AES', pretty_hex(before)))
		print(" - {:<10}: {:<30}".format('After AES', pretty_hex(after)))
		print(" - {:<10}: {:<30}".format('Fnv',fnv))

def handle_prints(file_name, debug):
	try:
		with open(file_name,'r+') as f:
			src = f.read()
			if debug == True: # uncomment some debug printfs
				src = src.replace('// Debug: ', '')
			elif debug == False: # remove all printfs
				import re
				r = re.compile(r'.*printf.*')
				matches = re.findall(r, src)
				for match in matches:
					src = src.replace(match, '')
			else: # dont do anything, keep some pretties
				src = src
			f.seek(0)
			f.write(src)
			f.truncate()
		return True			
	except Exception as e:
		print('[!] Error removing prints: ' + str(e))
		return False

def main():
	parser = argparse.ArgumentParser(description='Convert environmental data to multiple layers of AES!')
	parser.add_argument('input', help='File to encrypt')
	parser.add_argument('config', help='Config file to parse')
	parser.add_argument('--debug', action='store_true', default=False, help='Enable debug prints in output file')
	parser.add_argument('--no-prints', action='store_true', default=False, help='Remove all printfs')
	parser.add_argument('--output', default='output.cpp', help='Output file')
	parser.add_argument('--sleep', type=int, default=0, help='Sleep for random time between 0 and arg (between "substantial ops")')

	args = parser.parse_args()

	if args.debug and args.no_prints:
		print('[!] Cant have all the prints and no prints at the same time!')
		return

	try:
		# plaintext = 'hello world'.encode() + b'\x00'
		plaintext = open(args.input, "rb").read() + b'\x00'
	except Exception as e:
		print('[!] Error getting bytes: ' + str(e))
		return

	banner()

	config_file = args.config

	greta = Greta(config_file)

	if not greta.parse_config():
		return
	
	keys = greta.build_crypto_keys(plaintext)

	print('|> Plaintext Length:', len(plaintext))
	print('|> Rounds of crypto:', len(keys))

	if build_cpp(keys, args.output, plaintext, args.sleep):
		print(f'|> Wrote "{args.output}"!')
	else:
		print('[!] Failed to write to disk!')
		return

	if args.debug:
		# debug_print(keys) # print more to the console (python)
		handle_prints(args.output, True) # print more with cpp

	if args.no_prints:
		handle_prints(args.output, False) # dont print anything with cpp
	else:
		handle_prints(args.output, None) # default: print a few pretty things

	print('\n- [Cya!]')

if __name__ == '__main__':
	main()

