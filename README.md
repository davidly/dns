# dns

Windows command-line app that shows network connection information. Useful for seeing what services your PC
is connecting to. 

Bits and pieces of ping, netstat, and nslookup.

    usage: dns [-l] [-i]
    arguments:
        -a         Show active listeners on this machine
        -i:IPV4    Find a host or company name for the address, e.g. /i:64.74.236.127
        -l         Loop forever
        -l:X       Loop X times
        -p:X       Ping the site or IP address, e.g. /p:cnn.com
        -s         Show network connection statistics
        -x         For unknown sites, use LookIP.net for resolution
    reads from and appends to dns_entries.txt to map IP addresses to dns names
    
Build using mr.bat, which invokes .net 6.0 to generate dns.exe

Sample usage:

C:\Users\david\OneDrive\dns>dns -i:104.98.114.24

    address 104.98.114.24, hostname a104-98-114-24.deploy.static.akamaitechnologies.com
    
C:\Users\david\OneDrive\dns>dns

    PID    State          Local address          Foreign address          Host/Company name                                     Process
    5400   Established    192.168.0.105:55571    23.215.176.115:80        a23-215-176-115.deploy.static.akamaitechnologies.com  svchost
    16388  Established    192.168.0.105:55538    140.82.112.25:443        lb-140-82-112-25-iad.github.com                       msedge
    5400   Established    192.168.0.105:55623    13.107.4.50:80           Microsoft Azure                                       svchost
    16388  Established    192.168.0.105:51366    151.101.130.166:443      Fastly                                                msedge
    15632  Established    192.168.0.105:51478    44.194.17.145:443        ec2-44-194-17-145.compute-1.amazonaws.com             CoreSync
    16388  Established    192.168.0.105:53225    104.244.42.66:443        Twitter                                               msedge
    16388  Established    192.168.0.105:61594    104.244.42.193:443       Twitter                                               msedge
    5148   Established    192.168.0.105:56850    52.96.166.50:443         Microsoft                                             SearchHost
    16388  Established    192.168.0.105:51240    151.101.22.114:443       Fastly                                                msedge
    16388  Established    192.168.0.105:61515    185.184.8.65:443         ip-185-184-8-65.rtbhouse.net                          msedge
    5488   Established    192.168.0.105:56814    13.64.180.106:443        wns.windows.com                                       svchost
    16388  Established    192.168.0.105:61479    151.101.22.49:443        Fastly                                                msedge
    16388  Established    192.168.0.105:51492    192.229.173.16:443       Verizon Business                                      msedge
    5400   Established    192.168.0.105:55572    23.195.235.45:80         a23-195-235-45.deploy.static.akamaitechnologies.com   svchost
    3384   CloseWait      192.168.0.105:62039    23.216.80.9:443          a23-216-80-9.deploy.static.akamaitechnologies.com     Video.UI
    16388  Established    192.168.0.105:55537    185.199.110.133:443      cdn-185-199-110-133.github.com                        msedge
    16388  Established    192.168.0.105:51464    65.8.164.100:443         server-65-8-164-100.sfo53.r.cloudfront.net            msedge
    33448  Established    192.168.0.105:55624    51.104.15.253:443        events.data.microsoft.com                             OneDrive
    16388  Established    192.168.0.105:53367    104.244.43.131:443       Fastly                                                msedge
    16388  Established    192.168.0.105:55565    192.168.0.174:2869       Xbox-SystemOS.local                                   msedge
    16388  Established    192.168.0.105:51218    151.101.21.164:443       Fastly                                                msedge
    16388  Established    192.168.0.105:61636    152.195.54.7:443         Verizon Internet Services                             msedge
    16388  Established    192.168.0.105:51344    151.101.21.44:443        Fastly                                                msedge
    16388  Established    192.168.0.105:51350    152.195.55.192:443       Verizon Business                                      msedge
    15632  Established    192.168.0.105:62031    54.160.197.138:443       ec2-54-160-197-138.compute-1.amazonaws.com            CoreSync
    16388  Established    192.168.0.105:51198    151.101.22.217:443       Fastly                                                msedge
    5480   Established    127.0.0.1:50322        127.0.0.1:65001          david-pc                                              nvcontainer

C:\Users\david\OneDrive\dns>dns -s

      Connection Data:
          Current:           28
          Cumulative:        66
          Initiated:         128348
          Accepted:          390
          Failed Attempts:   2103
          Reset:             23009
          Errors:            0
          
C:\Users\david\OneDrive\dns>dns /p:cnn.com

    Pinging cnn.com [151.101.65.67], [151.101.129.67], [151.101.193.67], [151.101.1.67]
    Reply from 151.101.65.67: bytes=32 time=20ms TTL=56
    Reply from 151.101.65.67: bytes=32 time=17ms TTL=56
    Reply from 151.101.65.67: bytes=32 time=21ms TTL=56
    Reply from 151.101.65.67: bytes=32 time=19ms TTL=56
