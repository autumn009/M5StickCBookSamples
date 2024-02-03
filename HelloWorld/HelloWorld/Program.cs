using nanoFramework.M5Stack;
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
            Console.Clear();
            Console.ForegroundColor = System.Drawing.Color.Green;
            Console.WriteLine("HELLO WORLD to screen");

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
