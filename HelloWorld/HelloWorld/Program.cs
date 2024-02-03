using nanoFramework.M5Stack;
using System;
using System.Diagnostics;
using System.Threading;

namespace HelloWorld
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            M5StickC.InitializeScreen();
            nanoFramework.M5Stack.Console.Clear();
            nanoFramework.M5Stack.Console.ForegroundColor = System.Drawing.Color.Green;
            nanoFramework.M5Stack.Console.WriteLine("HELLO WORLD");

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
