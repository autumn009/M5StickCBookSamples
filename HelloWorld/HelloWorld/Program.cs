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
            Debug.WriteLine("HELLO WORLD to debug window");

            M5StickC.InitializeScreen();
            nanoFramework.M5Stack.Console.Clear();
            nanoFramework.M5Stack.Console.ForegroundColor = System.Drawing.Color.Green;
            nanoFramework.M5Stack.Console.WriteLine("HELLO WORLD to screen");

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
