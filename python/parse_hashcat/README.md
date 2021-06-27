# Hashcat Parser

This script was a rushed hotel script where I had a huge potfile and needed to pair the output up to valid accounts. This script will take in the potfile and the secretsdump output and cross-reference them and show the accounts which are valid.

It can also support credential validation by adding the optional flags.

## Usage

```
python3 parse_hashcat.py <ntlm dump> <hashcat output> <(optional) domain name> <(optional) domain controller>
```

Example:

**<u>Extract:</u>**

```
python3 parse_hashcat.py secretsdump.log pot.file
```

**<u>Extract and validate</u>**:

```
python3 parse_hashcat.py secretsdump.log pot.file MEGACORP.LOCAL 10.10.11.111
```

If validation is being used, it will use SMB:

```python
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
```

