using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Codebound.Core;

namespace Codebound.Game
{
    /// <summary>
    /// Internet multiplayer: asks the home router (UPnP IGD) to forward the game port to this PC, only when the host
    /// presses the button, and removes the forward when hosting stops. Runs on a worker thread; the UI reads Status.
    /// </summary>
    public static class RouterPort
    {
        public static volatile string Status;
        public static volatile string ExternalAddress;
        static string serviceType, controlUrl;
        static volatile bool mapped;
        static int mappedPort;

        public static bool Busy { get; private set; }

        public static void OpenAsync(int port)
        {
            if (Busy) return;
            Busy = true;
            ThreadPool.QueueUserWorkItem(_ => { try { Open(port); } finally { Busy = false; } });
        }

        static void Open(int port)
        {
            string manual = Loc.T("net.manual", "forward TCP port {0} to this PC in your router's settings", port);
            try
            {
                Status = Loc.T("net.upnp.search", "Looking for the router (UPnP)…");
                var (location, router) = Discover();
                if (location == null) { Status = Loc.T("net.upnp.none", "No UPnP router answered: {0}.", manual); return; }
                var svc = Upnp.WanService(Get(location), location);
                if (svc == null) { Status = Loc.T("net.upnp.noservice", "The router does not offer port forwarding by UPnP: {0}.", manual); return; }
                (serviceType, controlUrl) = svc.Value;
                string local = LocalAddressTowards(router);
                Post("AddPortMapping", Upnp.AddPortMapping(serviceType, port, local));
                mapped = true; mappedPort = port;
                string ext = Upnp.Tag(Post("GetExternalIPAddress", Upnp.Soap(serviceType, "GetExternalIPAddress")), "NewExternalIPAddress");
                ExternalAddress = ext;
                Status = ext != null ? Loc.T("net.upnp.ok", "Port open. Friends join with the address {0}", ext)
                                     : Loc.T("net.upnp.okNoIp", "Port open. Your internet address is shown by your router.");
            }
            catch (Exception e) { Status = Loc.T("net.upnp.fail", "UPnP failed ({0}): {1}.", e.Message, manual); }
        }

        /// <summary>Removes the forward (called when hosting stops or the game quits). Blocks for at most ~2 s.</summary>
        public static void Close()
        {
            if (!mapped) return;
            mapped = false; ExternalAddress = null; Status = null;
            try { Post("DeletePortMapping", Upnp.DeletePortMapping(serviceType, mappedPort), 2000); } catch (Exception) { }
        }

        static (string location, IPAddress router) Discover()
        {
            using (var udp = new UdpClient(new IPEndPoint(IPAddress.Any, 0)))
            {
                udp.Client.ReceiveTimeout = 1000;
                var req = Encoding.ASCII.GetBytes(Upnp.SearchRequest());
                var to = new IPEndPoint(IPAddress.Parse(Upnp.SsdpAddress), Upnp.SsdpPort);
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    udp.Send(req, req.Length, to);
                    var until = DateTime.UtcNow.AddSeconds(1.5);
                    while (DateTime.UtcNow < until)
                    {
                        IPEndPoint from = null;
                        byte[] data;
                        try { data = udp.Receive(ref from); } catch (SocketException) { break; }
                        string loc = Upnp.Location(Encoding.ASCII.GetString(data));
                        // only trust a description served by the device that answered (no redirect to other hosts)
                        if (loc != null && IPAddress.TryParse(new Uri(loc).Host, out var host) && host.Equals(from.Address)) return (loc, from.Address);
                    }
                }
            }
            return (null, null);
        }

        static string LocalAddressTowards(IPAddress router)
        {
            using (var s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                s.Connect(router, 1900);   // UDP connect sends nothing; it only picks the local interface
                return ((IPEndPoint)s.LocalEndPoint).Address.ToString();
            }
        }

        static string Get(string url)
        {
            var r = (HttpWebRequest)WebRequest.Create(url);
            r.Timeout = r.ReadWriteTimeout = 4000; r.AllowAutoRedirect = false;
            using (var resp = r.GetResponse()) return ReadLimited(resp.GetResponseStream());
        }

        static string Post(string action, string body, int timeout = 4000)
        {
            var r = (HttpWebRequest)WebRequest.Create(controlUrl);
            r.Method = "POST"; r.Timeout = r.ReadWriteTimeout = timeout; r.AllowAutoRedirect = false;
            r.ContentType = "text/xml; charset=\"utf-8\"";
            r.Headers.Add("SOAPACTION", $"\"{serviceType}#{action}\"");
            var b = Encoding.UTF8.GetBytes(body);
            r.ContentLength = b.Length;
            using (var s = r.GetRequestStream()) s.Write(b, 0, b.Length);
            try { using (var resp = r.GetResponse()) return ReadLimited(resp.GetResponseStream()); }
            catch (WebException e) when (e.Response != null)
            {
                string fault;
                using (var resp = e.Response) fault = ReadLimited(resp.GetResponseStream());
                throw new IOException(Upnp.Tag(fault, "errorDescription") ?? e.Message);
            }
        }

        static string ReadLimited(Stream s)
        {
            var ms = new MemoryStream(); var buf = new byte[8192]; int n;
            while ((n = s.Read(buf, 0, buf.Length)) > 0) { ms.Write(buf, 0, n); if (ms.Length > 256 * 1024) throw new IOException("router answer too large"); }
            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}
