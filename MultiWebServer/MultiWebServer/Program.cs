using MultiWebServer;
using nanoFramework.M5Stack;
using nanoFramework.Networking;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

Debug.WriteLine("Starting...");

string ssid = "";
string password = "";
GetSsidAndPassword();

bool success;
CancellationTokenSource cs = new(60000);
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



Thread.Sleep(Timeout.Infinite);

void GetSsidAndPassword()
{
    var r = Resources.GetBytes(Resources.BinaryResources.netinfo);
    using (var sr = new StreamReader(new MemoryStream(r)))
    {
        ssid = sr.ReadLine();
        password = sr.ReadLine();
    }
}
