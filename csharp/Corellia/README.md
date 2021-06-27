# Corellia

This is **<u>NOT</u>** a port-scanner. By default Corellia will only use SMB as a method of enumeration.

```
    __________________________________
   |:                           ``::%H|
   |%:.        Corellia            `:%|
   |H%::..___________________________:|


PS> Corellia.exe <arguments>

OPTIONS:
   -username      Username to authenticate with
   -domain        Domain to authenticate with
   -password      Password to authenticate with

EXAMPLES:
   PS> Corellia.exe -username="administrator" -password="Password1" -domain="lab.local"
```

By default, this will:

1. GPPPassword
2. Kerberoast
3. Descriptions

It has the capability to spray, but I disabled it. I have also had weird issues with it when not in a domain context, so use at your own peril. It was more of an experiment for other projects :upside_down_face:

