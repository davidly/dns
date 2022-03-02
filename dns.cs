//
// Shows network connection information
// For the reverse DNS resolutions that .net 6 didn't find, I used (with example IPs):
//    manual:                 https://otx.alienvault.com/indicator/ip/40.114.105.100
//    automatic (use /x):     https://www.lookip.net/ip/38.133.127.31
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Runtime.InteropServices;

class DnsApp
{
    static SortedSet<string> unknownEntries = new SortedSet<string>();
    static Dictionary<string,string> persistentEntries = new Dictionary<string,string>();
    static Dictionary<string,string> prefixEntries = new Dictionary<string,string>();

    public enum ConName : byte { unresolvedCN = 0, persistentCN, unknownCN, revipCN, lookipCN, prefixCN }

    public class TCPConnection
    {
        public TCPConnection()
        {
            state = 0;
            pid = 0;
            local = null;
            remote = null;
            newConnection = false;
            conName = ConName.unresolvedCN;
            remoteName = null;
        }

        public int state;
        public int pid;
        public IPEndPoint local;
        public IPEndPoint remote;
        public bool newConnection;
        public ConName conName;
        public string remoteName;
    }

    static string likelyOwnerFromLookIP( string ip )
    {
        var client = new HttpClient();
        string request = "https://www.lookip.net/ip/" + ip;
        HttpResponseMessage responseMessage = client.GetAsync( request ).Result;
        HttpContent content = responseMessage.Content;

        using ( var reader = new StreamReader( content.ReadAsStream()) )
        {
            string response = reader.ReadToEnd();
            int startTitle = response.IndexOf( "<title>" );
            int endTitle = response.IndexOf( "</title>" );

            if ( -1 != startTitle && -1 != endTitle && endTitle > startTitle && ( endTitle - startTitle > 20 ) )
            {
                string title = response.Substring( startTitle + 7, endTitle - startTitle - 7 );

                // example:        "<title>52.178.17.3 - Microsoft Azure | IP Address Information Lookup</title>"
                // when throttled: "<title>38.133.127.31 -  | IP Address Information Lookup</title>"

                int dash = title.IndexOf( '-' );
                int bar = title.IndexOf( '|' );

                if ( -1 != dash && -1 != bar && bar > dash )
                {
                    string owner = title.Substring( dash + 2, bar - dash - 3 );
                    if ( 0 == owner.Length )
                        return null;

                    //Console.WriteLine( "lookip resolution succeeded for {0}", owner );
                    return owner;
                }
            }
        }

        return null;
    } //likelyOwnerFromLookIP

    static void PingServer( string remoteServer )
    {
        try
        {
            string server = null;
            IPAddress serverip = null;
            bool parseOK = IPAddress.TryParse( remoteServer, out serverip );
            if ( parseOK )
            {
                // try to find the host name in the persistent cache or via a call, but it's not required

                if ( !persistentEntries.TryGetValue( remoteServer, out server ) )
                {
                    try
                    {
                        Console.WriteLine( "finding dns name..." );
                        System.Net.IPHostEntry entry = System.Net.Dns.GetHostEntry( serverip );
                        server = entry.HostName;
                    }
                    catch( Exception )
                    {
                        server = "(unknown)";
                    }
                }

                Console.WriteLine( "Pinging {0} [{1}]", server, serverip );
            }
            else
            {
                // Find the IP address of the hostname

                server = remoteServer;
                System.Net.IPHostEntry entry = System.Net.Dns.GetHostEntry( server );
                serverip = entry.AddressList[ 0 ];
                Console.Write( "Pinging {0} ", server );

                for ( int a = 0; a < entry.AddressList.Length; a++ )
                    Console.Write( "{0}" + "[" + entry.AddressList[ a ] + "]", ( a > 0 ) ? ", " : "" );
    
                Console.WriteLine();
            }

            for ( int p = 0; p < 4; p++ )
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send( serverip );

                if ( IPStatus.Success == reply.Status )
                    Console.WriteLine( "Reply from {0}: bytes={1} time={2}ms TTL={3}", reply.Address.ToString(), reply.Buffer.Length, reply.RoundtripTime, reply.Options.Ttl );
                else
                    Console.WriteLine( "Ping failed with status {0}", reply.Status.ToString() );
            }
        }
        catch( System.Net.NetworkInformation.PingException e )
        {
            Exception baseex = e.GetBaseException();
            if ( null != baseex )
                Console.WriteLine( "ping exception: {0}", baseex.Message );
            else
                Console.WriteLine( "ping exception: {0}", e.Message );
        }
        catch( Exception e )
        {
            Exception baseex = e.GetBaseException();
            if ( null != baseex )
                Console.WriteLine( "exception: {0}", baseex.Message );
            else
                Console.WriteLine( "exception: {0}", e.Message );
        }
    } //PingServer

    static void ShowCurrentConnections()
    {
        IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
        TcpStatistics tcpstat = properties.GetTcpIPv4Statistics();
    
        Console.WriteLine("  Connection Data:");
        Console.WriteLine("      Current:           {0}", tcpstat.CurrentConnections);
        Console.WriteLine("      Cumulative:        {0}", tcpstat.CumulativeConnections);
        Console.WriteLine("      Initiated:         {0}", tcpstat.ConnectionsInitiated);
        Console.WriteLine("      Accepted:          {0}", tcpstat.ConnectionsAccepted);
        Console.WriteLine("      Failed Attempts:   {0}", tcpstat.FailedConnectionAttempts);
        Console.WriteLine("      Reset:             {0}", tcpstat.ResetConnections);
        Console.WriteLine("      Errors:            {0}", tcpstat.ErrorsReceived);
    } //ShowCurentConnections

    static void InitializePrefixEntries()
    {
        // most of these are Microsoft services that run on Azure -- Office, Defender, etc.

        prefixEntries.TryAdd( "192.168",  "PrivateNetwork" );
        prefixEntries.TryAdd( "13.69",    "Microsoft Azure" );
        prefixEntries.TryAdd( "13.78",    "Microsoft Azure" );
        prefixEntries.TryAdd( "13.89",    "Microsoft Azure" );
        prefixEntries.TryAdd( "13.91",    "Microsoft Azure" );
        prefixEntries.TryAdd( "13.105",   "Microsoft Azure" );
        prefixEntries.TryAdd( "13.107",   "Microsoft Azure" );
        prefixEntries.TryAdd( "20.40",    "Microsoft Azure" );
        prefixEntries.TryAdd( "20.42",    "Microsoft Azure" );
        prefixEntries.TryAdd( "20.44",    "Microsoft Azure" );
        prefixEntries.TryAdd( "20.49",    "Microsoft Azure" );
        prefixEntries.TryAdd( "20.50",    "Microsoft Azure" );
        prefixEntries.TryAdd( "20.54",    "Microsoft Azure" );
        prefixEntries.TryAdd( "20.60",    "Microsoft Azure" );
        prefixEntries.TryAdd( "20.69",    "Microsoft Azure" );
        prefixEntries.TryAdd( "20.72",    "Microsoft Azure" );
        prefixEntries.TryAdd( "20.189",   "Microsoft Azure" );
        prefixEntries.TryAdd( "20.190",   "Microsoft Azure" );
        prefixEntries.TryAdd( "40.70",    "Microsoft Azure" );
        prefixEntries.TryAdd( "40.79",    "Microsoft Azure" );
        prefixEntries.TryAdd( "40.90",    "Microsoft Azure" );
        prefixEntries.TryAdd( "40.91",    "Microsoft Azure" );
        prefixEntries.TryAdd( "40.97",    "Microsoft Azure" );
        prefixEntries.TryAdd( "40.125",   "Microsoft Azure" );
        prefixEntries.TryAdd( "40.126",   "Microsoft Azure" );
        prefixEntries.TryAdd( "51.132",   "Microsoft Azure" );
        prefixEntries.TryAdd( "52.96",    "Microsoft Azure" );
        prefixEntries.TryAdd( "52.108",   "Microsoft Azure" );
        prefixEntries.TryAdd( "52.109",   "Microsoft Azure" );
        prefixEntries.TryAdd( "52.111",   "Microsoft Azure" );
        prefixEntries.TryAdd( "52.113",   "Microsoft Azure" );
        prefixEntries.TryAdd( "52.152",   "Microsoft Azure" );
        prefixEntries.TryAdd( "52.160",   "Microsoft Azure" );
        prefixEntries.TryAdd( "52.168",   "Microsoft Azure" );
        prefixEntries.TryAdd( "52.174",   "Microsoft Azure" );
        prefixEntries.TryAdd( "52.182",   "Microsoft Azure" );
        prefixEntries.TryAdd( "52.239",   "Microsoft Azure" );
        prefixEntries.TryAdd( "52.249",   "Microsoft Azure" );
        prefixEntries.TryAdd( "52.137",   "Microsoft Azure" );
        prefixEntries.TryAdd( "104.208",  "Microsoft Azure" );
        prefixEntries.TryAdd( "104.46",   "Microsoft Azure" );
    } //InitializePrefixEntries

    static string FindPrefixEntry( string ip )
    {
        int dot = ip.IndexOf( '.' );
        if ( -1 != dot )
        {
            int dot2 = ip.IndexOf( '.', dot + 1 );
            if ( -1 != dot2 )
            {
                string prefix = ip.Substring( 0, dot2 );
                string hostName = null;
                if ( prefixEntries.TryGetValue( prefix, out hostName ) )
                    return hostName;
            }
        }

        return null;
    } //FindPrefixEntry

    static string GetProcessName( int pid )
    {
        string name = "n/a";

        try
        {
            Process proc = Process.GetProcessById( pid );
            name = proc.ProcessName;
        }
        catch( Exception ) {} // The process may have ended, resulting in ArgumentException

        return name;
    } //GetProcessName

    static void Usage()
    {
        Console.WriteLine( "usage: dns [-a] [-i] [-l] [-p] [-s] [-x]" );
        Console.WriteLine( "enumerates currently open tcp connections" );
        Console.WriteLine( "arguments:" );
        Console.WriteLine( "    -a         Show active listeners on this machine" );
        Console.WriteLine( "    -i:IPV4    Find a host or company name for the address, e.g. /i:64.74.236.127" );
        Console.WriteLine( "    -l         Loop forever" );
        Console.WriteLine( "    -l:X       Loop X times, e.g. /l:40" );
        Console.WriteLine( "    -p:X       Ping the site or IP address, e.g. /p:cnn.com" );
        Console.WriteLine( "    -s         Show network connection statistics" );
        Console.WriteLine( "    -x         For unknown sites, use LookIP.net for resolution" );
        Console.WriteLine( "notes:" );
        Console.WriteLine( "    reads from and appends to dns_entries.txt to map IP addresses to dns names" );
        Console.WriteLine( "    -a, -i, -p, and -s are modes that run once and ignore -l looping" );
        Console.WriteLine( "    The default mode with no arguments is to enumerate all connections" );
        Environment.Exit( 0 );
    } //Usage

    static string IPWithoutPort( IPEndPoint endPoint )
    {
        string ip = endPoint.ToString();
        int colon = ip.IndexOf( ':' );
        if ( -1 != colon )
            ip = ip.Substring( 0, colon );

        return ip;
    } //IPWithoutPort

    static void PrintConnection( TCPConnection conn )
    {
        Console.WriteLine( "  {0,-13}{1,-22}{2,-22}{3,-55} {4,-6} {5}",
                           (TcpState) conn.state, conn.local, conn.remote, conn.remoteName, conn.pid, GetProcessName( conn.pid ) );
    } //PrintConnection

    static void Main( string[] args )
    {
        bool loop = false;
        int loopPasses = -1;
        bool useLookIP = false;

        InitializePrefixEntries();
        const string persistentFilename = "dns_entries.txt";

        if ( File.Exists( persistentFilename ) )
        {
            using ( StreamReader sr = File.OpenText( persistentFilename ) )
            {
                string s;
                while ( ( s = sr.ReadLine() ) != null )
                {
                    int space = s.IndexOf( ' ' );
                    if ( s.Length > 10 && space != -1 )
                    {
                        string ip = s.Substring( 0, space );
                        string hostname = s.Substring( space + 1 );
                        persistentEntries.TryAdd( ip, hostname );
                    }
                }
            }
        }

        var props = IPGlobalProperties.GetIPGlobalProperties();

        for ( int i = 0; i < args.Length; i++ )
        {
            if ( '-' == args[i][0] || '/' == args[i][0] )
            {
                string arg = args[i];
                if ( arg.Length < 2 )
                    Usage();

                string argUpper = arg.ToUpper();
                char c = argUpper[1];

                if ( 'A' == c )
                {
                    Console.WriteLine( "  Proto  Local listening address" );
                    foreach ( var listener in props.GetActiveTcpListeners() )
                        Console.WriteLine( "  TCP    {0}", listener );
            
                    foreach ( var listener in props.GetActiveUdpListeners() )
                        Console.WriteLine( "  UDP    {0}", listener );

                    Environment.Exit( 0 );
                }
                else if ( 'L' == c )
                {
                    loop = true;

                    if ( arg.Length >= 4 && ':' == arg[2] )
                        loopPasses = Convert.ToInt32( arg.Substring( 3 ) );
                }
                else if ( 'I' == c )
                {
                    if ( arg.Length >= 10 && ':' == arg[2] )
                    {
                        string ip = arg.Substring( 3 );
                        int colon = ip.IndexOf( ':' );
                        if ( -1 != colon )
                            ip = ip.Substring( 0, colon );

                        string hostName = null;
                        if ( persistentEntries.TryGetValue( ip, out hostName ) )
                        {
                            Console.WriteLine( "address {0}, hostname {1}", ip, hostName );
                            Environment.Exit( 0 );
                        }

                        IPAddress serverip;
                        bool parseOK = IPAddress.TryParse( ip, out serverip );
                        if ( parseOK )
                        {
                            try
                            {
                                System.Net.IPHostEntry entry = System.Net.Dns.GetHostEntry( serverip );
                                Console.WriteLine( "address {0}, hostname {1}", serverip, entry.HostName );
                            }
                            catch( Exception )
                            {
                                Console.WriteLine( "unable to find hostname for address {0}", ip );
                            }

                            Environment.Exit( 0 );
                        }
                        else
                        {
                            Console.WriteLine( "ipv4 address is malformed" );
                            Usage();
                        }
                    }
                    else
                        Usage();
                }
                else if ( 'P' == c )
                {
                    if ( arg.Length < 6 || ':' != arg[ 2 ] )
                    {
                        Console.WriteLine( "ping argument is malformed" );
                        Usage();
                    }

                    PingServer( arg.Substring( 3 ) );
                    Environment.Exit( 0 );
                }
                else if ( 'S' == c )
                {
                    ShowCurrentConnections();
                    Environment.Exit( 0 );
                }
                else if ( 'X' == c )
                {
                    useLookIP = true;
                }
                else
                    Usage();
            }
            else
                Usage();
        }

        // When looping, short-lived connections will be missed. This app just polls.

        Console.WriteLine( "  State        Local address         Foreign address       Host/Company name                                       PID    Process");
        int passes = 0;
        TCPConnection [] prev = new TCPConnection[ 0 ];

        do
        {
            TCPConnection [] conns = GetTcpConnections();
            List<int> unresolvedIndexes = new List<int>();

            for ( int i = 0; i < conns.Length; i++ )
            {
                TCPConnection conn = conns[ i ];

                // see if it's in the previous snapshot. If so, ignore it

                bool duplicate = false;
                for ( int p = 0; p < prev.Length; p++ )
                {
                    if ( conn.remote.Address.Equals( prev[ p ].remote.Address )  &&
                         conn.remote.Port == prev[ p ].remote.Port &&
                         conn.local.Address.Equals( prev[ p ].local.Address ) &&
                         conn.local.Port == prev[ p ].local.Port &&
                         conn.pid  == prev[ p ].pid &&
                         conn.state  == prev[ p ].state )
                    {
                        duplicate = true;
                        break;
                    }
                }

                if ( duplicate )
                    continue;

                conn.newConnection = true;

                // resolve the remote name in the persistent cache or unknown list

                string ip = IPWithoutPort( conn.remote );
                if ( persistentEntries.TryGetValue( ip, out conn.remoteName ) )
                {
                    conn.conName = ConName.persistentCN;
                    PrintConnection( conn );
                }
                else if ( unknownEntries.Contains( ip ) )
                {
                    conn.remoteName = "(unknown)";
                    conn.conName = ConName.unknownCN;
                }

                if ( ConName.unresolvedCN == conn.conName )
                    unresolvedIndexes.Add( i );
            }

            // Perform a reverse DNS lookup for unresolved entries

            //for ( int u = 0; u < unresolvedIndexes.Count; u++ )
            Parallel.For( 0, unresolvedIndexes.Count, u =>
            {
                TCPConnection conn = conns[ unresolvedIndexes[ u ] ];

                try
                {
                    IPAddress a = conn.remote.Address;
                    string address = a.ToString();

                    if ( address.Length >= 7 && '[' != address[ 0 ] ) // IPV4 only
                    {
                        // This call will raise an exception if the host name can't be determined

                        System.Net.IPHostEntry hostEntry = System.Net.Dns.GetHostEntry( a );
                        conn.remoteName = hostEntry.HostName;
                        conn.conName = ConName.revipCN;
                    }
                }
                catch ( Exception )
                {
                    // Azure domains aren't resolved above; don't flood LookIP with those. Just use the prefix.

                    string ip = IPWithoutPort( conn.remote );
                    conn.remoteName = FindPrefixEntry( ip );
                    if ( null != conn.remoteName )
                    {
                        conn.conName = ConName.prefixCN;
                    }
                    else if ( useLookIP )
                    {
                        try
                        {
                            // This may return either a domain or a company name

                            conn.remoteName = likelyOwnerFromLookIP( ip );
                            if ( null != conn.remoteName )
                                conn.conName = ConName.lookipCN;
                        }
                        catch ( Exception ) { }
                    }
                }

                if ( null == conn.remoteName || 0 == conn.remoteName.Length )
                {
                    conn.remoteName = "(unknown)";
                    conn.conName = ConName.unknownCN;
                }
            } );

            // Update caches and print out any new entries not found in the persistent cache and already printed above 

            for ( int i = 0; i < conns.Length; i++ )
            {
                TCPConnection conn = conns[ i ];

                if ( conn.newConnection )
                {
                    string ip = IPWithoutPort( conn.remote );
                    Debug.Assert( ConName.unresolvedCN != conn.conName );

                    if ( conn.conName == ConName.lookipCN ||
                         conn.conName == ConName.prefixCN ||
                         conn.conName == ConName.revipCN )
                    {
                        if ( persistentEntries.TryAdd( ip, conn.remoteName ) )
                        {
                            try
                            {
                                if ( !File.Exists( persistentFilename ) )
                                    using ( StreamWriter sw = File.CreateText( persistentFilename ) )
                                        sw.WriteLine( "{0} {1}", ip, conn.remoteName );
                                else
                                    using ( StreamWriter sw = File.AppendText( persistentFilename ) )
                                        sw.WriteLine( "{0} {1}", ip, conn.remoteName );
                            }
                            catch( Exception ) {}
                        }
                    }
                    else if ( ConName.unknownCN == conn.conName )
                         unknownEntries.Add( ip );    // may already exist, and that's OK

                    if ( ConName.persistentCN != conn.conName )
                        PrintConnection( conn );
                }
            }

            prev = conns;

            if ( -1 != loopPasses )
            {
                passes++;
                if ( passes >= loopPasses )
                    break;
            }

            if ( loop )
                Thread.Sleep( 50 );
        } while ( loop );
    } //Main

    // Call the DLL directly to get PID information, since .net doesn't provide this

    [DllImport( "iphlpapi.dll", SetLastError = true )]
    public static extern int GetExtendedTcpTable( byte[] pTcpTable, out int dwOutBufLen, bool sort, int ipVersion, int tblClass, int reserved );

    public static IPEndPoint BufferToIPEndPoint( byte[] buffer, ref int offset )
    {
        UInt32 address= BitConverter.ToUInt32( buffer, offset );
        offset += 4;

        ushort port = (ushort) IPAddress.NetworkToHostOrder( BitConverter.ToInt16( buffer, offset ) );
        offset += 4;

        return new IPEndPoint( address, port );
    } //BufferToIPEndPoint

    public static TCPConnection[] GetTcpConnections()
    {
        try
        {
            int AF_INET = 2; // IP_v4
            int TCP_TABLE_OWNER_PID_CONNECTIONS = 4;  // don't enumerate listeners
            int buffSize = 32 * 1024; // tested with 0, but non-0 to avoid a loop
            byte [] buffer;
    
            do
            {
                buffer = new byte[ buffSize ];
                int res = GetExtendedTcpTable( buffer, out buffSize, false, AF_INET, TCP_TABLE_OWNER_PID_CONNECTIONS, 0 );
                if ( 0 == res )
                    break;
    
                if ( 122 == res ) // ERROR_INSUFFICIENT_BUFFER
                    continue;
    
                Console.WriteLine( "failed to read tcp entries, error {0}", res );
                return null;
            } while ( true );
      
            int offset = 0;
            int numEntries = BitConverter.ToInt32( buffer, offset );
            offset += 4;
    
            TCPConnection[] cons = new TCPConnection[ numEntries ];
    
            for ( int i = 0; i < numEntries; i++ )
            {
                cons[ i ] = new TCPConnection();
    
                cons[ i ].state = BitConverter.ToInt32( buffer, offset );
                offset += 4;
              
                cons[ i ].local = BufferToIPEndPoint( buffer, ref offset );
                cons[ i ].remote = BufferToIPEndPoint( buffer, ref offset );
    
                cons[ i ].pid = BitConverter.ToInt32( buffer, offset );
                offset += 4;
            }

            return cons;
        }
        catch( Exception e )
        {
            Console.WriteLine( "caught exception getting tcp connections {0}", e.ToString() );
        }

        return new TCPConnection[ 0 ];
    } //GetTcpConnections
} //DnsApp

