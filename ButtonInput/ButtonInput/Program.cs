using nanoFramework.M5Stack;
using System.Drawing;
using System.Threading;

Color[] colors = new Color[] { Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Cyan, Color.Magenta, Color.White };
int colorIndex = 0;

M5StickC.ButtonM5.Press += (s, e) =>
{
    Console.ForegroundColor = colors[colorIndex];
    colorIndex = (colorIndex + 1) % colors.Length;
};

M5StickC.ButtonRight.Press += (s, e) =>
{
    M5StickC.Power.PowerOff();
};

M5StickC.InitializeScreen();
Console.Clear();
Console.ForegroundColor = System.Drawing.Color.White;

for(; ; )
{
    Thread.Sleep(500);
    Console.Write("A");
}
