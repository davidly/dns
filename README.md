# dns

Windows command-line app that shows network connection information. Useful for seeing what services your PC
is connecting to. 

    usage: dns [-l] [-i]
    arguments:
        -a         Show active listeners on this machine
        -i:IPV4    Find a host or company name for the address, e.g. /i:64.74.236.127
        -l         Loop forever
        -l:X       Loop X times
        -s         Show network connection statistics
    reads from and appends to dns_entries.txt to map IP addresses to dns names
    
Build using mr.bat, which invokes .net 6.0 to generate dns.exe

Sample usage:

C:\Users\david\OneDrive\dns>dns -i:104.98.114.24

    address 104.98.114.24, hostname a104-98-114-24.deploy.static.akamaitechnologies.com
    
C:\Users\david\OneDrive\dns>dns

      Proto  State          Local address          Foreign address          Host/Company name
      TCP    Established    127.0.0.1:50322        127.0.0.1:65001          david-pc
      TCP    Established    192.168.0.105:56814    13.64.180.106:443        Microsoft Azure
      TCP    Established    192.168.0.105:56850    52.96.166.50:443         Microsoft
      TCP    TimeWait       192.168.0.105:57356    40.97.132.226:443        Microsoft Azure
      TCP    Established    192.168.0.105:57411    104.244.43.131:443       Fastly
      TCP    Established    192.168.0.105:57705    104.244.42.130:443       Twitter
      TCP    Established    192.168.0.105:57848    54.167.222.166:443       ec2-54-167-222-166.compute-1.amazonaws.com
      TCP    Established    192.168.0.105:57858    65.8.164.100:443         server-65-8-164-100.sfo53.r.cloudfront.net
      TCP    Established    192.168.0.105:57864    40.97.162.82:443         Microsoft Azure
      TCP    Established    192.168.0.105:57952    151.101.20.159:443       Fastly
      TCP    Established    192.168.0.105:57954    104.98.114.24:443        a104-98-114-24.deploy.static.akamaitechnologies.com
      TCP    Established    192.168.0.105:58002    40.97.161.34:443         Microsoft Azure
      TCP    TimeWait       192.168.0.105:58006    65.8.164.113:443         server-65-8-164-113.sfo53.r.cloudfront.net
      TCP    Established    192.168.0.105:58024    185.199.109.154:443      cdn-185-199-109-154.github.com
      TCP    Established    192.168.0.105:58032    185.199.110.133:443      cdn-185-199-110-133.github.com
      TCP    TimeWait       192.168.0.105:58050    52.137.102.105:443       Microsoft Azure
      TCP    TimeWait       192.168.0.105:58055    52.239.174.4:443         Microsoft Azure
      TCP    TimeWait       192.168.0.105:58051    52.168.112.66:443        Microsoft Azure
      TCP    TimeWait       192.168.0.105:58072    192.30.255.116:443       lb-192-30-255-116-sea.github.com
      TCP    TimeWait       192.168.0.105:58071    140.82.112.21:443        lb-140-82-112-21-iad.github.com
      TCP    TimeWait       192.168.0.105:58076    52.109.6.42:443          Microsoft Azure
      TCP    Established    192.168.0.105:58081    140.82.113.26:443        lb-140-82-113-26-iad.github.com
      TCP    TimeWait       192.168.0.105:58084    23.45.229.15:443         a23-45-229-15.deploy.static.akamaitechnologies.com
      TCP    Established    192.168.0.105:61594    104.244.42.193:443       Twitter
      TCP    Established    192.168.0.105:64688    54.160.197.138:443       ec2-54-160-197-138.compute-1.amazonaws.com
      TCP    CloseWait      192.168.0.105:64691    23.216.80.9:443          a23-216-80-9.deploy.static.akamaitechnologies.com

C:\Users\david\OneDrive\dns>dns -s

      Connection Data:
          Current:           28
          Cumulative:        66
          Initiated:         128348
          Accepted:          390
          Failed Attempts:   2103
          Reset:             23009
          Errors:            0
