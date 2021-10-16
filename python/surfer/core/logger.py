def version():
	v = '0.1'
	print("(¯`·._.··¸.-~*´¨¯¨`*·~-.,-(%s: %s)-,.-~*´¨¯¨`*·~-.¸··._.·´¯)" % (red('Version'),red(v)))

from time import gmtime, strftime

# Colours
colour_green = "\033[0;32m"
colour_red = "\033[0;31m"
colour_yellow = "\033[0;33m"
colour_blue = "\033[0;34m"
colour_remove= "\033[0m"

# Switches
colour_status = True

def green(string):
	string=str(string)
	if colour_status:
		return (colour_green + string + colour_remove)
	else:
		return string

def blue(string):
	string=str(string)
	if colour_status:
		return (colour_blue + string + colour_remove)
	else:
		return string

def red(string):
	string=str(string)
	if colour_status:
		return (colour_red + string + colour_remove)
	else:
		return string

def yellow(string):
	string=str(string)
	if colour_status:
		return (colour_yellow + string + colour_remove)
	else:
		return string

def msg(string,highlight,colour):
	log_time=strftime("%d/%m/%y, %H:%M:%S", gmtime())

	if highlight is not None:
		if colour == 'green':
			print(blue(log_time)+' ==> '+string+green(highlight))

		if colour == 'red':
			print(blue(log_time)+' ==> '+string+red(highlight))

		if colour == 'yellow':
			print(blue(log_time)+' ==> '+string+yellow(highlight))

		if colour == 'blue':
			print(blue(log_time)+' ==> '+string+blue(highlight))	

	else:
		if colour == 'green':
			print(blue(log_time)+' ==> '+green(string))

		if colour == 'red':
			print(blue(log_time)+' ==> '+red(string))

		if colour == 'yellow':
			print(blue(log_time)+' ==> '+yellow(string))

		if colour == 'blue':
			print(blue(log_time)+' ==> '+blue(string))

def request(request):
	log_time=strftime("%d/%m/%y, %H:%M:%S", gmtime())
	verb = request.command
	client_ip = request.client_address[0]
	client_port = request.client_address[1]
	version = request.request_version
	path = request.path

	print(
		blue(log_time) 
		+ ': ' 
		+ green(client_ip)
		+ ':'
		+ green(client_port)
		+' ==> ' 
		+ blue(verb)
		+ ' '
		+ green(path)
		+ ' '		
		+ version
		)

def list_dir(url,items):
	for item in items:
		print('• ' + url + green(item))

def list_commands(commands):
	for command in commands:
		print('• ' + green(command))