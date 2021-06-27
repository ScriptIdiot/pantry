<p align="center">
  <img alt="surfer" src="https://terrigen-cdn-dev.marvel.com/content/prod/1x/_silversurfer_card.jpg" height="140" />
  <p align="center">
    <a href="https://github.com/mez-0/surfer/releases/latest"><img alt="Release" src="https://img.shields.io/github/release/mez-0/surfer.svg?style=flat-square"></a>
    <a href="https://github.com/mez-0/surfer/blob/master/LICENSE"><img alt="Software License" src="https://img.shields.io/badge/license-MIT-brightgreen.svg?style=flat-square"></a>
    <a href="https://github.com/mez-0/surfer/issues"><img alt="GitHub issues" src="https://img.shields.io/github/issues/mez-0/surfer.svg?style=flat-square"></a>
    </p>
</p>

<h5 align="center"><i>Manageable HTTP Server</i></h5>

This is a project to replace `SimpleHTTPServer` for myself. Got fed up of constantly bringing it up and down and remember paths to things. This manages all that

***

## Help

Start `surfer.py` with these flags, something like:

```bash
./surfer.py -a 10.10.11.93 -p 13004 -d server/
```

Full help:

```
usage: surfer.py [-h] [-a SERVER_ADDRESS] [-p SERVER_PORT] [-d DIRECTORY]

Giver of shells

optional arguments:
  -h, --help            show this help message and exit
  -a SERVER_ADDRESS, --server-address SERVER_ADDRESS
                        Server Address
  -p SERVER_PORT, --server-port SERVER_PORT
                        Server port
  -d DIRECTORY, --directory DIRECTORY
                        Directory to serve
```

Then, the help page within `surfer.py`:
```
14/05/20, 12:50:50 ==> Listening @ http://10.10.11.93:13002
surfer> help                                                                                                                                                                                                       
list      List the contents of the server directory
config    Show the servers current configuration
generate  Generate download utility commands
read      Read the contents of a file
add       Add a file to the server directory
delete    Delete a file from the server directory
exit      Close Surfer.py
surfer>  
```

## Usage

My favourite feature is the `generate` function. When typing `generate`, `surfer.py` will suggest all its known executors. Natively, this will spit out download commands for all the files. However, one can be specified by doing something like:

```
generate Invoke-WebRequest PowerUp.ps1
```

It does this based on the following logic:

```python
completer = NestedCompleter.from_nested_dict({
    'list': None,
    'exit': None,
    'add': None,
    'delete': None,
    'config':None,
    'read':get_files(),
    'generate': {
    	'curl':get_files(),
    	'wget':get_files(),
    	'Invoke-WebRequest':get_files(),
    	'DownloadString':get_files()
    	}
})
```

**I've given this no opsec considerations.**