using System;
using System.Security;
using System.Text.RegularExpressions;

namespace Codebound.Core
{
    /// <summary>
    /// Text side of UPnP IGD port mapping (internet multiplayer): the SSDP search, finding the WAN connection
    /// service in the router's description, and the SOAP requests/answers. The sockets are in Game/RouterPort.cs.
    /// </summary>
    public static class Upnp
    {
        public const string SsdpAddress = "239.255.255.250";
        public const int SsdpPort = 1900;

        public static string SearchRequest(string target = "urn:schemas-upnp-org:device:InternetGatewayDevice:1") =>
            "M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 2\r\nST: " + target + "\r\n\r\n";

        /// <summary>The LOCATION header of an SSDP answer (the router's description URL), or null.</summary>
        public static string Location(string answer)
        {
            var m = Regex.Match(answer ?? "", @"^location:\s*(\S+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return m.Success && Uri.TryCreate(m.Groups[1].Value, UriKind.Absolute, out var u) && u.Scheme == "http" ? u.ToString() : null;
        }

        /// <summary>Finds the WANIPConnection (or WANPPPConnection) service: its type and absolute control URL.</summary>
        public static (string serviceType, string controlUrl)? WanService(string descriptionXml, string descriptionUrl)
        {
            foreach (Match m in Regex.Matches(descriptionXml ?? "", @"<service>(.*?)</service>", RegexOptions.Singleline | RegexOptions.IgnoreCase))
            {
                string type = Tag(m.Groups[1].Value, "serviceType"), ctl = Tag(m.Groups[1].Value, "controlURL");
                if (type == null || ctl == null || !(type.Contains(":WANIPConnection:") || type.Contains(":WANPPPConnection:"))) continue;
                string baseUrl = Tag(descriptionXml, "URLBase") ?? descriptionUrl;
                if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var bu) || !Uri.TryCreate(bu, ctl, out var abs) || abs.Scheme != "http") continue;
                return (type, abs.ToString());
            }
            return null;
        }

        /// <summary>A SOAP request body for an action of the WAN service.</summary>
        public static string Soap(string serviceType, string action, params (string name, string value)[] args)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("<?xml version=\"1.0\"?><s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\"><s:Body>");
            sb.Append("<u:").Append(action).Append(" xmlns:u=\"").Append(SecurityElement.Escape(serviceType)).Append("\">");
            foreach (var (n, v) in args) sb.Append('<').Append(n).Append('>').Append(SecurityElement.Escape(v)).Append("</").Append(n).Append('>');
            sb.Append("</u:").Append(action).Append("></s:Body></s:Envelope>");
            return sb.ToString();
        }

        public static string AddPortMapping(string serviceType, int port, string localIp) => Soap(serviceType, "AddPortMapping",
            ("NewRemoteHost", ""), ("NewExternalPort", port.ToString()), ("NewProtocol", "TCP"), ("NewInternalPort", port.ToString()),
            ("NewInternalClient", localIp), ("NewEnabled", "1"), ("NewPortMappingDescription", "Codebound"), ("NewLeaseDuration", "0"));

        public static string DeletePortMapping(string serviceType, int port) => Soap(serviceType, "DeletePortMapping",
            ("NewRemoteHost", ""), ("NewExternalPort", port.ToString()), ("NewProtocol", "TCP"));

        /// <summary>Text inside the first &lt;name&gt; (any namespace prefix), or null.</summary>
        public static string Tag(string xml, string name)
        {
            var m = Regex.Match(xml ?? "", @"<(?:\w+:)?" + Regex.Escape(name) + @"(?:\s[^>]*)?>\s*([^<]*?)\s*</(?:\w+:)?" + Regex.Escape(name) + ">", RegexOptions.IgnoreCase);
            return m.Success && m.Groups[1].Value.Length > 0 ? System.Net.WebUtility.HtmlDecode(m.Groups[1].Value) : null;
        }
    }
}
