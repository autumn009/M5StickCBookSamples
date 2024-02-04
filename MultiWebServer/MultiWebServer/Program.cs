using Iot.Device.Axp192;
using MultiWebServer;
using nanoFramework.Hardware.Esp32;
using nanoFramework.M5Stack;
using nanoFramework.Networking;
using nanoFramework.UI;
using nanoFramework.WebServer;
using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using UnitsNet;

Axp192 power = null;

Debug.WriteLine("Initializing...");
InitiM5Stick();
M5StickC.InitializeScreen();

power.LDO2OutputVoltage = ElectricPotential.FromVolts(8);

int backLightPin = -1; // Not managed thru ESP32 but thru AXP192
int chipSelect = 5;
int dataCommand = 23;
int reset = 18;
Configuration.SetPinFunction(4, DeviceFunction.SPI1_MISO); // 4 is unused but necessary
Configuration.SetPinFunction(15, DeviceFunction.SPI1_MOSI);
Configuration.SetPinFunction(13, DeviceFunction.SPI1_CLOCK);
DisplayControl.Initialize(new SpiConfiguration(1, chipSelect, dataCommand, reset, backLightPin), new ScreenConfiguration(26, 1, 80, 160), 10 * 1024);
Debug.WriteLine($"DisplayControl.MaximumBufferSize:{DisplayControl.MaximumBufferSize}");

var blue = Color.Blue.ToBgr565(); // aka
var red = Color.Red.ToBgr565(); // ao
var green = Color.Green.ToBgr565();
var white = Color.White.ToBgr565();
ushort[] toSend = new ushort[8 * 8];
for (int i = 0; i < toSend.Length; i++) toSend[i] = white;

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

const string html1 = @"
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

<form action=""/"" method=""get"">
  <input type =""text"" name=""msg"">
  <input type=""submit"" value=""SEND (0-9,A-Z only)"">
</form>

<h2>Accelerometer</h2>
<ul>
";
const string html2 = @"
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
        var index = url.ToLower().IndexOf("?msg=");
        if( index > 0)
        {
            var text = url.Substring(index + 5);
            drawText(text);
            Debug.WriteLine($"Text: {text}");
        }
        var sb = new StringBuilder();
        sb.Append(html1);
        var r = M5StickC.AccelerometerGyroscope.GetAccelerometer();
        sb.Append($"<li>x={r.X}</li>");
        sb.Append($"<li>y={r.Y}</li>");
        sb.Append($"<li>z={r.Z}</li>");
        sb.Append(html2);
        WebServer.OutPutStream(e.Context.Response, sb.ToString());
    }
    catch (Exception ex)
    {
        var sb = new StringBuilder();
        sb.Append(html1);
        var r = M5StickC.AccelerometerGyroscope.GetAccelerometer();
        sb.Append(ex.ToString());   // –{“–‚ÍŠÔˆá‚¢
        sb.Append(html2);
        WebServer.OutPutStream(e.Context.Response, sb.ToString());
    }
}

const string font09 =
@"00000
0   0
0   0
0   0
0   0
0   0
00000
 111
  11
  11
  11
  11
  11
 1111
22222
   22
   22
22222
22
22
22222
33333
   33
   33
33333
   33
   33
33333
44 44
44 44
44 44
44444
   44
   44
   44
55555
55
55
55555
   55
   55
55555
66666
66
66
66666
66 66
66 66
66666
77777
   77
   77
  77
  77
  77
  77
88888
88 88
88 88
88888
88 88
88 88
88888
99999
99 99
99 99
99999
   99
   99
99999
";
const string fontAZ = @"
";

bool[][] getFontSub(string font, int index)
{
    var reader = new StringReader(font);
    for (int i = 0; i < index * 7; i++)
    {
        var s = reader.ReadLine();
        if (s == null) return getFontSub(font09,0);
    }
    bool[][] r = new bool[7][];
    for (int i = 0; i < 7; i++)
    {
        r[i] = new bool[5];
        string line = reader.ReadLine();
        if (line == null) return getFontSub(font09, 0);
        for (int j = 0; j < 5; j++)
        {
            if(line.Length > j ) r[i][j] = line[j] != ' ';
        }
    }
    return r;
}

bool[][] getFont(char ch)
{
    if( ch >= '0' && ch <= '9')
    {
        return getFontSub(font09, ch - '0');
    }
    else if( ch >= 'A' && ch <= 'Z')
    {
        return getFontSub(fontAZ, ch - 'A');
    }
    return getFontSub(font09, 0);
}

void pset(int x, int y)
{
    DisplayControl.Write((ushort)(x * 8), (ushort)(y * 8), 8, 8, toSend);
}

void drawText(string text)
{
    nanoFramework.M5Stack.Console.Clear();
    for (int i = 0; i < text.Length; i++)
    {
        var img = getFont(text[i]);
        for (int y = 0; y < 7; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                if (img[y][x])
                {
                    pset(x, y);
                }
            }
        }
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

void InitiM5Stick()
{
    Debug.WriteLine("This is the sequence to power on the Axp192 for M5 Stick");

    Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
    Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);

    I2cDevice i2cAxp192 = new(new I2cConnectionSettings(1, Axp192.I2cDefaultAddress));
    power = new Axp192(i2cAxp192);

    // NOTE: the comments include code which was originally used
    // to setup the AXP192 and can be found in the M5Stick repository
    // This allows to understand the selection dome.
    // Set LDO2 & LDO3(TFT_LED & TFT) 3.0V
    // I2cWrite(Register.VoltageSettingLdo2_3, 0xcc);
    power.LDO2OutputVoltage = ElectricPotential.FromVolts(3);
    power.LDO3OutputVoltage = ElectricPotential.FromVolts(3);
    // Set ADC sample rate to 200hz
    // I2cWrite(Register.AdcFrequency, 0xF2);
    power.AdcFrequency = AdcFrequency.Frequency200Hz;
    power.AdcPinCurrent = AdcPinCurrent.MicroAmperes80;
    power.BatteryTemperatureMonitoring = true;
    power.AdcPinCurrentSetting = AdcPinCurrentSetting.AlwaysOn;
    // Set ADC to All Enable
    // I2cWrite(Register.AdcPin1, 0xff);
    power.AdcPinEnabled = AdcPinEnabled.All;
    // Bat charge voltage to 4.2, Current 100MA
    // I2cWrite(Register.ChargeControl1, 0xc0);
    power.SetChargingFunctions(true, ChargingVoltage.V4_2, ChargingCurrent.Current100mA, ChargingStopThreshold.Percent10);
    // Depending on configuration enable LDO2, LDO3, DCDC1, DCDC3.
    // byte data = I2cRead(Register.SwitchControleDcDC1_3LDO2_3);
    // data = (byte)((data & 0xEF) | 0x4D);
    // I2cWrite(Register.SwitchControleDcDC1_3LDO2_3, data);
    power.LdoDcPinsEnabled = LdoDcPinsEnabled.All;
    // 128ms power on, 4s power off
    // I2cWrite(Register.ParameterSetting, 0x0C);
    power.SetButtonBehavior(LongPressTiming.S1, ShortPressTiming.Ms128, true, SignalDelayAfterPowerUp.Ms64, ShutdownTiming.S10);
    // Set RTC voltage to 3.3V
    // I2cWrite(Register.VoltageOutputSettingGpio0Ldo, 0xF0);
    power.PinOutputVoltage = PinOutputVoltage.V3_3;
    // Set GPIO0 to LDO
    // I2cWrite(Register.ControlGpio0, 0x02);
    power.Gpio0Behavior = Gpio0Behavior.LowNoiseLDO;
    // Disable vbus hold limit
    // I2cWrite(Register.PathSettingVbus, 0x80);
    power.SetVbusSettings(true, false, VholdVoltage.V4_0, false, VbusCurrentLimit.MilliAmper500);
    // Set temperature protection
    // I2cWrite(Register.HigTemperatureAlarm, 0xfc);
    power.SetBatteryHighTemperatureThreshold(ElectricPotential.FromVolts(3.2256));
    // Enable RTC BAT charge 
    // I2cWrite(Register.BackupBatteryChargingControl, 0xa2);
    power.SetBackupBatteryChargingControl(true, BackupBatteryCharingVoltage.V3_0, BackupBatteryChargingCurrent.MicroAmperes200);
    // Enable bat detection
    // I2cWrite(Register.ShutdownBatteryDetectionControl, 0x46);
    // Note 0x46 is not a possible value, most likely 0x4A
    power.SetShutdownBatteryDetectionControl(false, true, ShutdownBatteryPinFunction.HighResistance, true, ShutdownBatteryTiming.S2);
    // Set Power off voltage 3.0v            
    // data = I2cRead(Register.VoltageSettingOff);
    // data = (byte)((data & 0xF8) | (1 << 2));
    // I2cWrite(Register.VoltageSettingOff, data);
    power.VoffVoltage = VoffVoltage.V3_0;

}

class StringReader:TextReader
{
    private string[] lines;
    private int counter = 0;
    public override string ReadLine()
    {
        if (counter >= lines.Length) return null;
        return lines[counter++];
    }
    public StringReader(string src)
    {
        lines = src.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].Split('\r')[0];
        }
    }
}
