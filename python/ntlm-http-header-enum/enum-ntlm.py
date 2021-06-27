#!/usr/bin/python3
import requests,sys
from requests_ntlm import HttpNtlmAuth
from ntlmdecoder import ntlmdecode
from requests.packages.urllib3.exceptions import InsecureRequestWarning
requests.packages.urllib3.disable_warnings(InsecureRequestWarning)

# Main reference:
# https://github.com/byt3bl33d3r/SprayingToolkit/blob/0838df444ab237d7c55ee2397d60ec6118f4dd77/core/sprayers/owa.py

class Domain:
	def __init__(self,name):
		self.name = name
		self.discovery_url = self.get_autodiscover_url(self.name)

	def get_autodiscover_url(self,domain):
	    urls = [
	        	f"https://autodiscover.{domain}/autodiscover/autodiscover.xml",
	        	f"http://autodiscover.{domain}/autodiscover/autodiscover.xml",
	        	f"https://{domain}/autodiscover/autodiscover.xml"
	        ]

	    headers = {"Content-Type": "text/xml"}
	    for url in urls:
	        try:
	            r = requests.get(url, headers=headers, verify=False)
	            if r.status_code == 401 or r.status_code == 403:
	                return url
	        except:
	            continue
	    return None

	def get_owa_domain(self):
		# Stolen from https://github.com/dafthack/MailSniper
		if self.discovery_url == None:
			print('Unable to get autodiscover URL')
			return False

		print('Using %s as autodiscover url' % self.discovery_url)
		print()

		auth_header = {"Authorization": "NTLM TlRMTVNTUAABAAAAB4IIogAAAAAAAAAAAAAAAAAAAAAGAbEdAAAADw=="}
		r = requests.post(self.discovery_url, headers=auth_header, verify=False)
		if r.status_code == 401 or r.status_code == 403:
			try:
				ntlm_info = ntlmdecode(r.headers["WWW-Authenticate"])
				return ntlm_info
			except Exception as e:
				print(e)
				return False
		else:
			return False

	def get_data(self):
		ntlm_data = self.get_owa_domain()
		if not ntlm_data: return False
		try:
			for key,value in ntlm_data.items():
				print('%s: %s' % (key,value))
		except Exception as e:
			print(e)
			return

if len(sys.argv) == 2:
	d = Domain(sys.argv[1])
	d.get_data()
else:
	print('python3 enum-ntlm.py <ip>')
