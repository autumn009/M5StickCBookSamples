using nanoFramework.Networking;
using System;
using System.Diagnostics;
using System.Threading;

Debug.WriteLine("Starting...");

bool success;
CancellationTokenSource cs = new(60000);
                success = WifiNetworkHelper.ConnectDhcp(MySsid, MyPassword, requiresDateTime: true, token: cs.Token);
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
