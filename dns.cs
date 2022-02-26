//
// Shows network connection information
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

class DnsApp
{
    static ConcurrentDictionary<string,string> inmemoryEntries = new ConcurrentDictionary<string,string>();
    static ConcurrentDictionary<string,string> persistentEntries = new ConcurrentDictionary<string,string>();
    static ConcurrentDictionary<string,string> prefixEntries = new ConcurrentDictionary<string,string>();

    static string likelyOwnerFromLookIP( string ip )
    {
        // temporarily throttled, so skip
        return null;

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

                    return owner;
                }
            }
        }

        return null;
    } //likelyOwnerFromLookIP

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

    static void Usage()
    {
        Console.WriteLine( "enumerates currently open tcp connections" );
        Console.WriteLine( "usage: dns [-l] [-i]" );
        Console.WriteLine( "arguments:" );
        Console.WriteLine( "    -a         Show active listeners on this machine" );
        Console.WriteLine( "    -i:IPV4    Find a host or company name for the address, e.g. /i:64.74.236.127" );
        Console.WriteLine( "    -l         Loop forever" );
        Console.WriteLine( "    -l:X       Loop X times" );
        Console.WriteLine( "    -s         Show network connection statistics" );
        Console.WriteLine( "reads from and appends to dns_entries.txt to map IP addresses to dns names" );
        Environment.Exit( 0 );
    } //Usage

    static void Main( string[] args )
    {
        bool loop = false;
        int loopPasses = -1;

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
                string argUpper = args[i].ToUpper();
                string arg = args[i];
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
                        
                        string [] elems = ip.Split( '.' );
                        if ( 4 == elems.Length )
                        {
                            IPAddress address = new IPAddress( Convert.ToInt32( elems[ 0 ] ) << 24 |
                                                               Convert.ToInt32( elems[ 1 ] ) << 16 | 
                                                               Convert.ToInt32( elems[ 2 ] ) << 8  | 
                                                               Convert.ToInt32( elems[ 3 ] ) );
                            try
                            {
                                System.Net.IPHostEntry entry = System.Net.Dns.GetHostEntry( address );
                                Console.WriteLine( "address {0}, hostname {1}", ip, entry.HostName );
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
                else if ( 'S' == c )
                {
                    ShowCurrentConnections();
                    Environment.Exit( 0 );
                }
                else
                    Usage();
            }
            else
                Usage();
        }

        // When looping, short-lived connections will be missed. This app just polls.

        Console.WriteLine( "  Proto  State          Local address          Foreign address          Host/Company name");
        int passes = 0;

        do
        {
            Parallel.ForEach( props.GetActiveTcpConnections(), conn =>
            {
                IPEndPoint endPoint = conn.RemoteEndPoint;
                string hostName = null;

                // strip off the ":port"

                string ip = endPoint.ToString();
                int colon = ip.IndexOf( ':' );
                if ( -1 != colon )
                    ip = ip.Substring( 0, colon );
    
                try
                {
                    IPAddress a = endPoint.Address;
                    string address = a.ToString();

                    if ( address.Length >= 7 && '[' != address[ 0 ] ) // IPV4 only
                    {
                        string newValue = null;

                        // look at both caches since many domains will be persistent and (unknown) entries will be in inmemory
    
                        if ( persistentEntries.TryGetValue( ip, out newValue ) )
                            hostName = newValue;
                        else if ( inmemoryEntries.TryGetValue( ip, out newValue ) )
                            hostName = newValue;
                        else
                        {
                            // This call will raise an exception if the host name can't be determined

                            System.Net.IPHostEntry hostEntry = System.Net.Dns.GetHostEntry( a );
                            hostName = hostEntry.HostName;
                        }
                    }
                }
                catch ( Exception )
                {
                    // Azure domains aren't resolved above; don't flood LookIP with those. Just use the prefix.

                    hostName = FindPrefixEntry( ip );
                    if ( null == hostName )
                    {
                        try
                        {
                            // This may return either a domain or a company name
                            hostName = likelyOwnerFromLookIP( ip );
                        }
                        catch ( Exception ) { }
                    }
                }

                if ( null == hostName || 0 == hostName.Length )
                    hostName = "(unknown)";

                bool added = inmemoryEntries.TryAdd( ip, hostName );

                if ( added )
                    Console.WriteLine( "  TCP    {0,-15}{1,-23}{2,-23}  {3}", conn.State, conn.LocalEndPoint, endPoint, hostName );

                // Don't persist failed reverse dns lookups

                if ( 0 != String.Compare( hostName, "(unknown)" ) )
                {
                    added = persistentEntries.TryAdd( ip, hostName );
                    if ( added )
                    {
                        lock( args )
                        {
                            //Console.WriteLine( "new persistent entry {0} {1}", ip, hostName );
    
                            if ( !File.Exists( persistentFilename ) )
                                using ( StreamWriter sw = File.CreateText( persistentFilename ) )
                                    sw.WriteLine( "{0} {1}", ip, hostName );
                            else
                                using ( StreamWriter sw = File.AppendText( persistentFilename ) )
                                    sw.WriteLine( "{0} {1}", ip, hostName );
                        }
                    }
                }
            } );

            if ( -1 != loopPasses )
            {
                passes++;
                if ( passes >= loopPasses )
                    break;
            }

            if ( loop )
                Thread.Sleep( 500 );
        } while ( loop );
    } //Main
} //DnsApp
