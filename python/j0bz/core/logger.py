from termcolor import colored
from core import arguments

args = arguments.get_args()

def good(s):
	prompt = colored('\\__ ','green')
	print(prompt + s)

def bad(s):
	prompt = colored('\\__ ','red')
	print(prompt + s)

def info(s):
	prompt = colored('\\__ ','blue')
	print(prompt + s)

def verbose(s):
	if args.verbose_level > 0:
		print('[v] ' + s)

def bullet(l, limit=4):
	prompt = colored('-> ','yellow')
	count = 0
	for s in l:
		end = '\t'
		count += 1
		if count == limit:
			end = '\n'
			count = 0
		print(f'   {prompt} {s}', end=end)
	print()

