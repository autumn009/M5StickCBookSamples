using MultiWebServer;
using nanoFramework.M5Stack;
using nanoFramework.Networking;
using nanoFramework.WebServer;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

Debug.WriteLine("Initializing...");
M5StickC.InitializeScreen();

string ssid = "";
string password = "";
GetSsidAndPassword();

bool success;
CancellationTokenSource cs = new(60000);

Debug.WriteLine($"ssid={ssid} password={password}");

success = WifiNetworkHelper.ConnectDhcp(ssid, password, requiresDateTime: true, token: cs.Token);
if (!success)
{
    Debug.WriteLine($"Can't get a proper IP address and DateTime, error: {WifiNetworkHelper.Status}.");
    if (WifiNetworkHelper.HelperException != null)
    {
        Debug.WriteLine($"Exception: {WifiNetworkHelper.HelperException}");
    }
    return;
}
Debug.WriteLine("Starting...");
using (WebServer server = new WebServer(80, HttpProtocol.Http, new Type[0]))
{
    // Add a handler for commands that are received by the server.
    server.CommandReceived += ServerCommandReceived;
    // Start the server.
    server.Start();

    Thread.Sleep(Timeout.Infinite);
}

const string html = @"
<html>
<head>
<title>Multi Web Server on M5StickC</title>
</head>
<body>
<h1>Multi Web Server on M5StickC</h1>

<ul>
<li><a href=""/ledon"">LED ON</a></li>
<li><a href=""/ledoff"">LED OFF</a></li>
</ul>

</body>
</html>
";

void ServerCommandReceived(object source, WebServerEventArgs e)
{
    try
    {
        var url = e.Context.Request.RawUrl;
        Debug.WriteLine($"Command received: {url}, Method: {e.Context.Request.HttpMethod}");

        if (url.ToLower() == "/ledon")
        {
            M5StickC.Led.Write(false);
        }
        else if (url.ToLower() == "/ledoff")
        {
            M5StickC.Led.Write(true);
        }
        WebServer.OutPutStream(e.Context.Response, html);
    }
    catch (Exception)
    {
        WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.InternalServerError);
    }
}

void GetSsidAndPassword()
{
    var r = Resources.GetBytes(Resources.BinaryResources.netinfo);
    using (var sr = new StreamReader(new MemoryStream(r)))
    {
        ssid = sr.ReadLine().Trim();
        password = sr.ReadLine().Trim();
    }
}
