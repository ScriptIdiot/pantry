#!/usr/bin/python3
import re,sys
from impacket.smbconnection import SMBConnection, SessionError

class Hashcat:
	def __init__(self,nt,password):
		self.nt = nt
		self. password = password

class User:
	def __init__(self,domain,username,sid,ntlm,nt,lm,password):
		self.domain = domain
		self.username = username
		self.sid = sid
		self.ntlm = ntlm
		self.nt = nt
		self.lm = lm
		self.password = password

def show_help():
	print("Usage: python3 parse_hashcat.py <ntlm dump> <hashcat output> <(optional) domain name> <(optional) domain controller>")
	print("NTLM Dump Example: DOMAIN.LOCAL\\Administrator:1001:b4b9b02e6f09a9bd760f388b67351e2b:b4b9b02e6f09a9bd760f388b67351e2b:::")
	print("Hashcat Example: b4b9b02e6f09a9bd760f388b67351e2b:Password1!")

def parse_hashcat_output(hashcat_file):
	# Takes  in a hashcat '--show' output and splits on: hash:password
	hashcat_data = []
	with open(hashcat_file,'r') as cracked_file:
		for cracked_line in cracked_file:
			cracked_line = cracked_line.strip('\n')
			hashcat = Hashcat(cracked_line.split(':')[0],cracked_line.split(':')[1])
			if hashcat not in hashcat_data:
				hashcat_data.append(hashcat)
	return hashcat_data

def extract_hashcat_hashes(ntlm_hashes_file,hashcat_data):
	users = []
	ntlm_regex = re.compile(r'(^\w+.*)\\(.*)\:(\d{1,5})\:(\w+)\:(\w+)')
	with open(ntlm_hashes_file,'r') as hash_file:
		for hash_line in hash_file:
			hash_line = hash_line.strip('\n')
			for hashcat in hashcat_data:
				if hashcat.nt in hash_line:
					match = ntlm_regex.search(hash_line)
					if match != None:
						print(match.group())
						domain = match.group(1)
						username = match.group(2)
						sid = match.group(3)
						lm = match.group(4)
						nt = match.group(5)
						ntlm = '%s:%s' % (lm,nt)
						user = User(domain,username,sid,ntlm,hashcat.nt,lm,hashcat.password)
						if user not in users:
							users.append(user)
	return users

def make_it_pretty(users):
	pesky_duplicates = [] # idk why but Guest:<blank> wouldnt remove in the previous duplicate check.
	for user in users:
		user_pass = '%s:%s' % (user.username,user.password)
		if user_pass not in pesky_duplicates:
			pesky_duplicates.append(user_pass)
			# <sid> <nt> <username>:<password>
			print("[!] (%s)\t(%s)\t%s:%s" % (user.sid,user.nt,user.username,user.password))

def check_auth(users,dc,domain):
	authenticated = []
	print('[!] Connecting to Domain Controller: ' + dc)
	for user in users:
		try:
			smb = SMBConnection(dc, dc, None, 445)
		except Exception as e:
			print('[-] Failed to connect to Domain Controller:')
			print(e)
			quit()
		try:
			smb.login(user.username, user.password,domain)
			print('[+] %s\\%s:%s' % (domain,user.username,user.password))
			smb.close()
			if user not in authenticated:
				authenticated.append(user)
		except Exception as e:
			error = str(e).split('(')[0]
			print('[-] %s\\%s:%s (%s)' % (domain,user.username,user.password,error))
		smb.close()
	return authenticated

if __name__ == '__main__':
	if len(sys.argv) <= 3:
		try:
			ntlm_hashes_file = sys.argv[1]
			hashcat_file = sys.argv[2]
		except:
			show_help()
			quit()

	elif len(sys.argv) > 3:
		try:
			ntlm_hashes_file = sys.argv[1]
			hashcat_file = sys.argv[2]
			domain = sys.argv[3]
			dc = sys.argv[4]
		except:
			show_help()
			quit()
	elif len(sys.argv) == 1:
		if sys.argv[1] == '-h' or sys.argv[1] == '--help':
			show_help()
	else:
		show_help()
		quit()

	hashcat_data = parse_hashcat_output(hashcat_file) # take the hashcat --show output, and split it from hash:password into an object
	users = extract_hashcat_hashes(ntlm_hashes_file,hashcat_data) # parse the ntlm with regex, check if the cracked hash is in the raw hashes
	make_it_pretty(users) # make it look pretty
	if len(sys.argv) > 3: # if specified, take the cracked creds an attempt to authenticate as them to determine if the creds are valid, if not, retuern the NTError
		authenticated = check_auth(users,dc,domain)
		print()
		print("Valid Credentials:")
		if len(authenticated) == 0:
			print('None found!')
		else:
			for user in authenticated:
				print('[+] %s\\%s:%s' % (domain,user.username,user.password))