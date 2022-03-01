# dns

Windows command-line app that shows network connection information. Useful for seeing what services your PC
is connecting to. 

Bits and pieces of ping, netstat, and nslookup.

    usage: dns [-a] [-i] [-l] [-p] [-s] [-x]
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

    State        Local address         Foreign address       Host/Company name                                       PID    Process
    Established  192.168.0.105:49224   72.21.81.200:443      Verizon Business                                        3724   Code
    Established  192.168.0.105:49228   20.189.173.5:443      Microsoft Azure                                         17764  OneDrive
    TimeWait     192.168.0.105:49238   140.82.113.4:443      lb-140-82-113-4-iad.github.com                          0      Idle
    TimeWait     192.168.0.105:49241   140.82.113.4:443      lb-140-82-113-4-iad.github.com                          0      Idle
    TimeWait     192.168.0.105:49242   20.42.65.90:443       Microsoft Azure                                         0      Idle
    Established  192.168.0.105:49246   54.80.149.42:443      ec2-54-80-149-42.compute-1.amazonaws.com                18328  msedge
    Established  192.168.0.105:49249   140.82.112.25:443     lb-140-82-112-25-iad.github.com                         18328  msedge
    Established  192.168.0.105:49540   54.161.91.12:443      ec2-54-161-91-12.compute-1.amazonaws.com                23008  CoreSync
    Established  192.168.0.105:49698   40.83.240.146:443     Microsoft Azure                                         5588   svchost
    TimeWait     192.168.0.105:50260   13.107.5.93:443       Microsoft Azure                                         0      Idle
    Established  127.0.0.1:50275       127.0.0.1:65001       david-pc                                                5608   nvcontainer
    Established  127.0.0.1:50286       127.0.0.1:50309       david-pc                                                14844  NVIDIA Web Helper
    Established  127.0.0.1:50309       127.0.0.1:50286       david-pc                                                16956  NVIDIA Share
    Established  192.168.0.105:50344   13.64.180.106:443     wns.windows.com                                         17764  OneDrive
    Established  192.168.0.105:50356   40.64.128.224:443     Microsoft Azure                                         3724   Code
    Established  127.0.0.1:50424       127.0.0.1:50559       david-pc                                                23084  node
    Established  127.0.0.1:50559       127.0.0.1:50424       david-pc                                                21748  Adobe CEF Helper
    CloseWait    192.168.0.105:50818   72.21.91.29:80        Edgecast/Verizon/Yahoo                                  25364  GameBar
    CloseWait    192.168.0.105:50820   23.45.228.133:443     a23-45-228-133.deploy.static.akamaitechnologies.com     25364  GameBar
    TimeWait     192.168.0.105:50824   152.199.4.33:443      Edgecast vo.msecnd.net azureedge.net                    0      Idle
    TimeWait     192.168.0.105:50825   20.189.173.5:443      Microsoft Azure                                         0      Idle
    Established  192.168.0.105:51279   104.244.42.1:443      Twitter                                                 18328  msedge
    Established  192.168.0.105:51876   13.107.5.93:443       Microsoft Azure                                         3724   Code
    TimeWait     192.168.0.105:51887   140.82.112.3:443      lb-140-82-112-3-iad.github.com                          0      Idle
    TimeWait     192.168.0.105:53074   40.64.128.224:443     Microsoft Azure                                         0      Idle
    TimeWait     192.168.0.105:53710   72.21.81.200:443      Verizon Business                                        0      Idle
    Established  192.168.0.105:53737   20.189.173.4:443      Microsoft Azure                                         18328  msedge
    TimeWait     192.168.0.105:53738   52.96.119.98:443      Microsoft Azure                                         0      Idle
    Established  192.168.0.105:53739   140.82.113.4:443      lb-140-82-113-4-iad.github.com                          18328  msedge
    Established  192.168.0.105:53744   185.199.108.133:443   cdn-185-199-108-133.github.com                          18328  msedge
    Established  192.168.0.105:53746   140.82.113.22:443     lb-140-82-113-22-iad.github.com                         18328  msedge
    Established  192.168.0.105:53747   192.30.255.116:443    lb-192-30-255-116-sea.github.com                        18328  msedge
    TimeWait     192.168.0.105:53755   23.32.46.59:80        a23-32-46-59.deploy.static.akamaitechnologies.com       0      Idle
    TimeWait     192.168.0.105:53756   23.215.176.146:80     a23-215-176-146.deploy.static.akamaitechnologies.com    0      Idle
    TimeWait     192.168.0.105:53757   104.99.72.226:80      a104-99-72-226.deploy.static.akamaitechnologies.com     0      Idle
    Established  192.168.0.105:55583   104.244.42.66:443     api.twitter.com                                         18328  msedge
    Established  192.168.0.105:55619   52.96.119.98:443      Microsoft Azure                                         28792  OUTLOOK
    Established  192.168.0.105:55655   40.97.142.18:443      Microsoft Azure                                         28792  OUTLOOK
    CloseWait    192.168.0.105:55685   23.45.228.133:443     a23-45-228-133.deploy.static.akamaitechnologies.com     16992  Video.UI
    CloseWait    192.168.0.105:55686   72.21.91.29:80        Edgecast/Verizon/Yahoo                                  16992  Video.UI
    Established  192.168.0.105:55716   40.97.132.18:443      Microsoft Azure                                         28792  OUTLOOK
    Established  192.168.0.105:55723   40.97.132.18:443      Microsoft Azure                                         28792  OUTLOOK
    Established  192.168.0.105:55728   44.194.17.145:443     ec2-44-194-17-145.compute-1.amazonaws.com               23008  CoreSync
    Established  192.168.0.105:55729   72.21.91.70:443       Edgecast/Verizon/Yahoo                                  18328  msedge
    Established  192.168.0.105:55732   52.84.159.6:443       server-52-84-159-6.sea19.r.cloudfront.net               18328  msedge
    Established  192.168.0.105:55733   99.86.38.61:443       server-99-86-38-61.sea19.r.cloudfront.net               18328  msedge
    TimeWait     192.168.0.105:57580   13.66.138.105:443     azurewebsites.com                                       0      Idle
    TimeWait     192.168.0.105:57731   93.184.215.201:443    Verizon Business                                        0      Idle
    TimeWait     192.168.0.105:59012   13.107.42.18:443      Microsoft Azure                                         0      Idle
    Established  192.168.0.105:60263   13.66.138.105:443     azurewebsites.com                                       3724   Code
    Established  192.168.0.105:60266   185.199.111.154:443   cdn-185-199-111-154.github.com                          18328  msedge
    TimeWait     192.168.0.105:61382   93.184.215.201:443    Verizon Business                                        0      Idle
    Established  192.168.0.105:61867   40.87.19.190:443      Microsoft Azure                                         23616  msteams
    Established  192.168.0.105:63981   13.107.42.18:443      Microsoft Azure                                         3724   Code
    Established  192.168.0.105:63983   185.199.111.154:443   cdn-185-199-111-154.github.com                          18328  msedge
    Established  127.0.0.1:65001       127.0.0.1:50275       david-pc                                                5608   nvcontainer

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
