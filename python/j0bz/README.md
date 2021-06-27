# J0bz

The purpose of `j0bz` is to solely do a bunch of nmap's with service-specific scripts, its mainly for reporting/evidencing.

## Help

```
usage: j0bz.py [-h] -c  -s  [-t] [-o] [--threads] [--verbose-level] [--exclude] [--use-chunks] [--find-alive]

Automate all of the internal inf please.

optional arguments:
  -h, --help           show this help message and exit
  -c , --client-name   Name of the client
  -s , --scope-file    File containing the scope
  -t , --top-ports     Top X ports to scan (Default: 100)
  -o , --output        Directory to create the project in
  --threads            Amount of threads to use (Default: 2)
  --verbose-level      Set the verbosity level (Default: 0)
  --exclude            Comma seperated list of IPs to exclude
  --use-chunks         Chunk scope up for better threading
  --find-alive         Enable ICMP Scanning
```

## Usage

Typically I'd use this at the start of an engagement so the `-c` flag is required to build the directory structure that I'd normally go for. Example:

```
python3 j0bz.py -c BigTimeClientPLC -s scope.txt -t 1000 --threads 4 --find-alive --use-chunks
```

