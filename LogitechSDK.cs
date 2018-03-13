using System;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;

namespace etr.Common
{

  public static class LogitechSDK
  {
    public const int LOGI_LCD_COLOR_BUTTON_LEFT = (0x00000100);
    public const int LOGI_LCD_COLOR_BUTTON_RIGHT = (0x00000200);
    public const int LOGI_LCD_COLOR_BUTTON_OK = (0x00000400);
    public const int LOGI_LCD_COLOR_BUTTON_CANCEL = (0x00000800);
    public const int LOGI_LCD_COLOR_BUTTON_UP = (0x00001000);
    public const int LOGI_LCD_COLOR_BUTTON_DOWN = (0x00002000);
    public const int LOGI_LCD_COLOR_BUTTON_MENU = (0x00004000);

    public const int LOGI_LCD_MONO_BUTTON_0 = (0x00000001);
    public const int LOGI_LCD_MONO_BUTTON_1 = (0x00000002);
    public const int LOGI_LCD_MONO_BUTTON_2 = (0x00000004);
    public const int LOGI_LCD_MONO_BUTTON_3 = (0x00000008);

    public const int LOGI_LCD_MONO_WIDTH = 160;
    public const int LOGI_LCD_MONO_HEIGHT = 43;

    public const int LOGI_LCD_COLOR_WIDTH = 320;
    public const int LOGI_LCD_COLOR_HEIGHT = 240;

    public const int LOGI_LCD_TYPE_MONO = (0x00000001);
    public const int LOGI_LCD_TYPE_COLOR = (0x00000002);

    [DllImport("LogitechLcdEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool LogiLcdInit(String friendlyName, int lcdType);
    [DllImport("LogitechLcdEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool LogiLcdIsConnected(int lcdType);
    [DllImport("LogitechLcdEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool LogiLcdIsButtonPressed(int button);
    [DllImport("LogitechLcdEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool LogiLcdUpdate(int lcdType);
    [DllImport("LogitechLcdEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool LogiLcdShutdown();
    [DllImport("LogitechLcdEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool LogiLcdMonoSetBackground(byte [] monoBitmap);
    [DllImport("LogitechLcdEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool LogiLcdMonoSetText(int lineNumber, String text);
    [DllImport("LogitechLcdEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool LogiLcdColorSetBackground(byte [] colorBitmap);
    [DllImport("LogitechLcdEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool LogiLcdColorSetTitle(String text, int red, int green, int blue);
    [DllImport("LogitechLcdEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool LogiLcdColorSetText(int lineNumber, String text, int red, int green, int blue);

  }

  //-- класс управляющий отображением дополнительной информации на мониторе клавиатуры
  public static class LogitechMonitor
  {
    public static bool InitLogitechMonitor()
    {
      Boolean res = false;
      //-- если монитор вообще подсоединен, то ..
      if (TryInitLogitechMonitor())
      {
        //-- пытаемся его инициализировать
        if (LogitechSDK.LogiLcdIsConnected(LogitechSDK.LOGI_LCD_TYPE_MONO))
        {
          //LogitechSDK.LogiLcdMonoSetBackground(BitmapToByteRgbNaive(Properties.Resources.russ));
          LogitechSDK.LogiLcdMonoSetBackground(BitmapToByteRgbNaive(Properties.Resources.ETR));
//          LogitechSDK.LogiLcdMonoSetText(0, "Elite Dangerous");
//          LogitechSDK.LogiLcdMonoSetText(3, "Приветствую Вас, пилот!");
          LogitechSDK.LogiLcdUpdate(LogitechSDK.LOGI_LCD_TYPE_MONO);
        }
        res = true;
      }
      return res;
    }

    public static void DeInitLogitechMonitor()
    {
      if (LogitechSDK.LogiLcdIsConnected(LogitechSDK.LOGI_LCD_TYPE_MONO))
      {
        LogitechSDK.LogiLcdShutdown();
      }
    }

    public static bool TryInitLogitechMonitor()
    {
      return LogitechSDK.LogiLcdInit("ELITE", LogitechSDK.LOGI_LCD_TYPE_MONO | LogitechSDK.LOGI_LCD_TYPE_COLOR);
    }

    internal static byte[] ConvertToMonochrome(byte[] bitmap)
    {
      byte[] monochromePixels = new byte[bitmap.Length / 4];

      for (int ii = 0; ii < (int)(LogitechSDK.LOGI_LCD_MONO_HEIGHT) * (int)LogitechSDK.LOGI_LCD_MONO_WIDTH; ii++)
      {
        monochromePixels[ii] = bitmap[ii * 4];
      }

      return monochromePixels;
    }

    public static byte[] BitmapToByteRgbNaive(Bitmap bmp)
    {
      int width = bmp.Width,
          height = bmp.Height;
      byte[] res = new byte[height*width];
      for (int y = 0; y < height; y++)
      {
        for (int x = 0; x < width; x++)
        {
          Color color = bmp.GetPixel(x, y);
          if ((color.R > 0))
            SetPixel(res,x,y);
        }
      }
      return res;
    }

    private static void SetPixel(byte [] bmp, int x , int y, bool on=true)
    {
      if (on)
      {
        bmp[(x+y) + (y*(LogitechSDK.LOGI_LCD_MONO_WIDTH-1))] = 255;
      }
      else
      {
        bmp[(x + y) + (y * (LogitechSDK.LOGI_LCD_MONO_WIDTH - 1))] = 0;
      }
    }

  }

}
