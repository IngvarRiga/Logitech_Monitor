using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Drawing;
using System;
using System.Drawing.Imaging;
using System.Threading;

namespace plgLogitechSDK
{
  public enum GKey
  {
    G1,G2,G3,G4,G5,G6,G7,G8,G9,G10,G11,G12,G13,G14,G15,G16,G17,G18,G19,G20,G21,G22,G23
  }
  
  public delegate void LogitechKeyProcessor(LogitechGSDK.GkeyCode KeyCode, string KeyString, IntPtr context);
  public delegate void LogitechLCDKeyProcessor(int KeyPressed);


  /// <summary>
  /// Класс-обертка для обслуживания дополнительных клавиш клавиатур (и не только) Logitech устройств
  /// </summary>
  public static class LogitechGSDK
  {
//G-KEY SDK
    public const int LOGITECH_MAX_MOUSE_BUTTONS = 20;
    public const int LOGITECH_MAX_GKEYS = 29;
    public const int LOGITECH_MAX_M_STATES = 3;

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct GkeyCode
    {
      public ushort complete;
      /// <summary>
      /// Индекс G-клавиши, которая была нажата, например 6 -> G6 или Button 6
      /// </summary>
      public int keyIdx
      {
        get { return complete & 255; }
      }
      /// <summary>
      /// Статус клавиши, 1 - нажата, 0 - отпущена 
      /// </summary>
      public int keyDown
      {
        get { return (complete >> 8) & 1; }
      }
      /// <summary>
      /// Режим клавиатуры (1, 2 или 3 для M1, M2 и M3)
      /// </summary>
      public int mState
      {
        get { return (complete >> 9) & 3; }
      }
      /// <summary>
      /// Индикатор, что событие пришло от клавиш мыши если = 1 и нет, если = 0.
      /// </summary>
      public int mouse
      {
        get { return (complete >> 11) & 15; }
      }
      /// <summary>
      /// Зарезервировано
      /// </summary>
      public int reserved1
      {
        get { return (complete >> 15) & 1; }
      }
      /// <summary>
      /// Зарезервировано
      /// </summary>
      public int reserved2
      {
        get { return (complete >> 16) & 131071; }
      }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void logiGkeyCB(GkeyCode gkeyCode, [MarshalAs(UnmanagedType.LPWStr)] String gkeyOrButtonString, IntPtr context); // ??

    [DllImport("LogitechGkeyEnginesWrapper ", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern int LogiGkeyInitWithoutCallback();

    [DllImport("LogitechGkeyEnginesWrapper ", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern int LogiGkeyInitWithoutContext(logiGkeyCB gkeyCB);

    [DllImport("LogitechGkeyEnginesWrapper ", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern int LogiGkeyIsMouseButtonPressed(int buttonNumber);

    [DllImport("LogitechGkeyEnginesWrapper ", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr LogiGkeyGetMouseButtonString(int buttonNumber);

    public static String LogiGkeyGetMouseButtonStr(int buttonNumber)
    {
      var str = Marshal.PtrToStringUni(LogiGkeyGetMouseButtonString(buttonNumber));
      return str;
    }

    [DllImport("LogitechGkeyEnginesWrapper ", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern int LogiGkeyIsKeyboardGkeyPressed(int gkeyNumber, int modeNumber);

    [DllImport("LogitechGkeyEnginesWrapper ")]
    private static extern IntPtr LogiGkeyGetKeyboardGkeyString(int gkeyNumber, int modeNumber);

    public static String LogiGkeyGetKeyboardGkeyStr(int gkeyNumber, int modeNumber)
    {
      var str = Marshal.PtrToStringUni(LogiGkeyGetKeyboardGkeyString(gkeyNumber, modeNumber));
      return str;
    }

    [DllImport("LogitechGkeyEnginesWrapper ", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern void LogiGkeyShutdown();

  }
  /// <summary>
  /// Класс обертка для обслуживания LCD-экранов на клавиатурах (и не только) Logitech устройств
  /// </summary>
  public static class LogitechSDK
  {
    public const int LOGI_LCD_COLOR_BUTTON_LEFT = (0x00000100);
    public const int LOGI_LCD_COLOR_BUTTON_RIGHT = (0x00000200);
    public const int LOGI_LCD_COLOR_BUTTON_OK = (0x00000400);
    public const int LOGI_LCD_COLOR_BUTTON_CANCEL = (0x00000800);
    public const int LOGI_LCD_COLOR_BUTTON_UP = (0x00001000);
    public const int LOGI_LCD_COLOR_BUTTON_DOWN = (0x00002000);
    public const int LOGI_LCD_COLOR_BUTTON_MENU = (0x00004000);

    //-- кнопки монохромного монитора
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

    [DllImport("LogitechLcdEnginesWrapper", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
    public static extern bool LogiLcdInit(String friendlyName, int lcdType);
    [DllImport("LogitechLcdEnginesWrapper", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
    public static extern bool LogiLcdIsConnected(int lcdType);
    [DllImport("LogitechLcdEnginesWrapper", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
    public static extern bool LogiLcdIsButtonPressed(int button);
    [DllImport("LogitechLcdEnginesWrapper", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
    public static extern bool LogiLcdUpdate(int lcdType);
    [DllImport("LogitechLcdEnginesWrapper", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
    public static extern bool LogiLcdShutdown();
    [DllImport("LogitechLcdEnginesWrapper", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
    public static extern bool LogiLcdMonoSetBackground(byte[] monoBitmap);
    [DllImport("LogitechLcdEnginesWrapper", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
    public static extern bool LogiLcdMonoSetText(int lineNumber, String text);
    [DllImport("LogitechLcdEnginesWrapper", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
    public static extern bool LogiLcdColorSetBackground(byte[] colorBitmap);
    [DllImport("LogitechLcdEnginesWrapper", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
    public static extern bool LogiLcdColorSetTitle(String text, int red, int green, int blue);
    [DllImport("LogitechLcdEnginesWrapper", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
    public static extern bool LogiLcdColorSetText(int lineNumber, String text, int red, int green, int blue);

  }
  /// <summary>
  /// Структура, описывающая 1 знак
  /// </summary>
  public class CharsSign
  {
    public string Sign;
    public int pixCnt; //-- количество пикселей в знаке
    public int CharWidth;
    public int[,] rstr;

    public CharsSign(Bitmap Src, int CharWd=6, int Pos = 0, int CharH = 8, int CharW = 6)
    {
      // 6*8 = 48 x,y координаты
      //-- инициализация массива пикселей
      rstr = new int[48, 2];
      pixCnt = 0;
      //-- ширина символа с пробелом справа в 1 пиксель
      CharWidth = CharWd+1;
      for (var y = 0; y < CharH; y++)
      {
        for (var x = (0 + Pos); x < (CharW + Pos); x++)
        {
          var color = Src.GetPixel(x, y);
          if (color.R > 0)
          {
            rstr[pixCnt, 0] = x - Pos;
            rstr[pixCnt, 1] = y;
            pixCnt += 1;
          }
        }
      }
    }
  }

  /// <summary>
  /// Класс, монохромного знакогенератора
  /// </summary>
  public static class MonoCharsGen6x8
  { 
    //-- набор "букафф"
    public static Dictionary<string, CharsSign> Alphabet = new Dictionary<string, CharsSign>();
    public static Dictionary<string, CharsSign> AlphabetPost = new Dictionary<string, CharsSign>();
    private const int SW = 6;
    private const int SH = 8;
    public static string[] AlphRU = { "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я" };
    public static string[] AlphRD = { "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я" };
    public static string[] AlphEU = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
    public static string[] AlphED = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
    public static string[] AlphDig = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
    public static string[] AlphDigPost = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", ":" };
    public static string[] AlphSign = { "~", "`", "!", "@", "#", "№", ";", "%", ":", "?", "*", "(", ")", "[", "]", "_", "+", "=", "-", "\\", "/", "|", "$", "^", "<", ">", ",", ".", "\"", "'", " ", "«", "»", "&" };
    //---                        А  Б  В  Г  Д  Е  Ё  Ж  З  И  Й  К  Л  М  Н  О  П  Р  С  Т  У  Ф  Х  Ц  Ч  Ш  Щ  Ъ  Ы  Ь  Э  Ю  Я
    private static int[] wrU = { 4, 4, 4, 4, 5, 4, 4, 5, 4, 5, 4, 4, 4, 5, 4, 4, 4, 4, 4, 3, 4, 5, 4, 5, 4, 5, 5, 5, 5, 4, 4, 5, 4 };
    private static int[] wrD = { 4, 4, 4, 3, 5, 4, 4, 5, 4, 4, 4, 4, 4, 5, 4, 4, 4, 4, 4, 3, 4, 5, 4, 5, 4, 5, 5, 5, 5, 4, 4, 5, 4 };
    //---                        A  B  C  D  E  F  G  H  I  J  K  L  M  N  O  P  Q  R  S  T  U  V  W  X  Y  Z
    private static int[] weU = { 4, 4, 4, 4, 4, 4, 4, 4, 3, 4, 4, 4, 5, 4, 4, 4, 4, 4, 4, 3, 4, 5, 5, 4, 5, 4 };
    private static int[] weD = { 4, 4, 4, 4, 4, 4, 4, 4, 1, 3, 4, 1, 5, 4, 4, 4, 4, 3, 4, 3, 4, 5, 5, 4, 4, 3 };
    //---                       0  1  2  3  4  5  6  7  8  9
    private static int[] wD = { 4, 3, 4, 4, 4, 4, 4, 4, 4, 4 };
    //---                           0  1  2  3  4  5  6  7  8  9  :
    private static int[] wDPost = { 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 1 };
    //---                       ~  `  !  @  #  №  ;  %  :  ?  *  (  )  [  ]  _  +  =  -  \  /  |  $  ^  <  >  ,  .  "  ' Sp  «  »  &
    private static int[] wS = { 5, 2, 1, 5, 5, 5, 2, 5, 1, 5, 5, 2, 2, 2, 2, 5, 5, 4, 4, 5, 5, 1, 5, 5, 3, 3, 2, 1, 3, 1, 1, 5, 5, 5 };

    static MonoCharsGen6x8()
    {
      //-- большие русские буквы
      for (var i = 0; i < AlphRU.Length; i++) { Alphabet.Add(AlphRU[i], new CharsSign(Properties.Resources.AlpRU, wrU[i] ,i*SW, SH, SW)); }
      //-- маленькие русские буквы
      for (var i = 0; i < AlphRD.Length; i++) { Alphabet.Add(AlphRD[i], new CharsSign(Properties.Resources.AlpRD, wrD[i], i * SW, SH, SW)); }
      //-- большие английские
      for (var i = 0; i < AlphEU.Length; i++) { Alphabet.Add(AlphEU[i], new CharsSign(Properties.Resources.AlpEU, weU[i], i * SW, SH, SW)); }
      //-- маленькие английские
      for (var i = 0; i < AlphED.Length; i++) { Alphabet.Add(AlphED[i], new CharsSign(Properties.Resources.AlpED, weD[i], i * SW, SH, SW)); }
      //-- цифры
      for (var i = 0; i < AlphDig.Length; i++) { Alphabet.Add(AlphDig[i], new CharsSign(Properties.Resources.AlpNUM, wD[i], i * SW, SH, SW)); }
      //-- знаки
      for (var i = 0; i < AlphSign.Length; i++) { Alphabet.Add(AlphSign[i], new CharsSign(Properties.Resources.AlpSign, wS[i], i * SW, SH, SW)); }
      //-- цифры POST
      for (var i = 0; i < AlphDigPost.Length; i++)
      {
        AlphabetPost.Add(AlphDigPost[i], new CharsSign(Properties.Resources.AlpNUMPost, wDPost[i], i * SW, SH, SW));
      }
    }

    public static int GetCharLen(string str = "")
    {
      if (str.Length == 1)
        return Alphabet[str].CharWidth;
      else
      { return -1; }
    }

    public static int GetCharLenPost(string str = "")
    {
      if (str.Length == 1)return AlphabetPost[str].CharWidth;
      else
      { return -1; }
    }


  }

  /// <summary>
  /// Класс описания одного логического экземпляра монохромного монитора. Таких мониторов может быть столько, сколько необходимо программе.
  /// Все данные, которые нужно отображать на мониторе клавиатуры хранятся во внутренней структуре и выводятся как только пользователь (программа)
  /// даст команду переключится на отображение данных конкретного монитора. Виртуальные "мониторы" идентифицируются по Int32 ID
  /// </summary>
  public class MonitorMONO
  {
    /// <summary>
    /// Внутренне поле, определяющее, что данный экран активен
    /// </summary>
    private bool fActive;
    /// <summary>
    /// Признак того, что данные на этот монитор выводятся в текстовом режиме 
    /// </summary>
    private bool fTextMode;
    /// <summary>
    /// ID логического монитора
    /// </summary>
    private int fID;
    /// <summary>
    /// Отображаемые строки для логического монохромного монитора (5 строк - 0-4)
    /// </summary>
    private string[] MonoStr=new string[5];
    /// <summary>
    /// Фон экрана монитора
    /// </summary>
    private Bitmap fFon;
    /// <summary>
    /// Реальное содержимое монитора, которое должно выводится на физический монитор
    /// </summary>
    public byte[] fMon;

    /// <summary>
    /// Конструктор класса
    /// </summary>
    public MonitorMONO()
    {
      fActive = false;
      fID = -1;
      for (var i = 0; i < MonoStr.Length; i++)
        MonoStr[i]=string.Empty;
      //-- по умолчанию - фон монитора - черный
      fFon = Properties.Resources.cls;
    }
    /// <summary>
    /// Свойство, указывающее, активен ли в данный момент текущий экран
    /// </summary>
    public bool Active
    {
      get { return fActive; }
      set
      {
        if (value != fActive)
        {
          fActive = value;
          if (fActive)
          {
            Repaint();
          }
        }
      }
    }

    /// <summary>
    /// Очистка содержимого монитора (всего сразу)
    /// </summary>
    public void ClearMonitor()
    {
      //-- очитка, заливка черным цветом
      fMon = LogitechMonitor.BitmapToByteRgb(fFon);
    }

    /// <summary>
    /// Возвращает битовую карту, представляющую собой фон, на котором рисуется содержимое монитора
    /// </summary>
    /// <returns></returns>
    public byte[] GetMonitorFon()
    {
      return LogitechMonitor.BitmapToByteRgb(fFon);
    }

    public int GetStringLength(string str = "")
    {
      var len = 0;
      if (!string.IsNullOrEmpty(str))
      {
        for (var i = 0; i < str.Length; i++)
        {
          len += MonoCharsGen6x8.GetCharLen(str.Substring(i, 1));
        }
      }
      return len;
    }

    public int GetStringLengthPost(string str = "")
    {
      var len = 0;
      if (!string.IsNullOrEmpty(str))
      {
        for (var i = 0; i < str.Length; i++)
        {
          len += MonoCharsGen6x8.GetCharLenPost(str.Substring(i, 1));
        }
      }
      return len;
    }

    /// <summary>
    /// Вывод текстовой информации в текстовом режиме
    /// </summary>
    /// <param name="strId">Номер строки 0-3</param>
    /// <param name="Text">Выводимый текст</param>
    public void TextOutTM(int strId, string Text="")
    {
      if (fActive)
      {
        if (LogitechMonitor.IsMonitorInit)
        {
          MonoStr[strId] = Text;
          Repaint();
        }
      }
    }

    /// <summary>
    /// Вовод текста на фон, задаваемый в первом параметре
    /// </summary>
    /// <param name="ekr">Фон, на который выводится указанная надпись</param>
    /// <param name="strId">Номер строки</param>
    /// <param name="Text">Выводимый текст</param>
    public void TextOut(ref byte[] ekr, int strId, string Text = "")
    {
      CharsSign sgn = null;
      var delta = 0;
      if ((strId > -1) && (strId < 5))
      {
        for (var i = 0; i < Text.Length; i++)
        {
          sgn = MonoCharsGen6x8.Alphabet[Text[i].ToString()];
          for (var j = 0; j < sgn.pixCnt; j++)
          {
            if (sgn.rstr[j, 0] + delta <= 159)
            {
              SetPixel(ekr, sgn.rstr[j, 0] + delta, sgn.rstr[j, 1] + strId * 8);
            }
          }
          delta += sgn.CharWidth;
        }
      }
    }

    /// <summary>
    /// Вовод текста на фон, задаваемый в первом параметре
    /// </summary>
    /// <param name="ekr">Фон, на который необходимо вывести надпись</param>
    /// <param name="PosY">Позиция X начала надписи. (0,0) - верхний правый угол экрана</param>
    /// <param name="PosX">Позиция Y начала надписи. (0,0) - верхний правый угол экрана</param>
    /// <param name="Text">Выводимый текст</param>
    public void TextOut(ref byte[] ekr, int PosX=0, int PosY=0, string Text = "")
    {
      if (string.IsNullOrEmpty(Text)) Text = "";
      var delta = PosX;
      if ((PosY<159) && (PosX > -1))
      {
        for (var i = 0; i < Text.Length; i++)
        {
          var sgn = MonoCharsGen6x8.Alphabet[Text[i].ToString()];
          for (var j = 0; j < sgn.pixCnt; j++)
          {
            if (sgn.rstr[j, 0] + delta <= 159)
            {
              SetPixel(ekr, sgn.rstr[j, 0] + delta, sgn.rstr[j, 1]+PosY);
            }
          }
          delta += sgn.CharWidth;
        }
      }
    }

    public void TextOutPost(ref byte[] ekr, int PosX = 0, int PosY = 0, string Text = "")
    {
      if (string.IsNullOrEmpty(Text)) Text = "";
      var delta = PosX;
      if ((PosY < 159) && (PosX > -1))
      {
        for (var i = 0; i < Text.Length; i++)
        {
          var sgn = MonoCharsGen6x8.AlphabetPost[Text[i].ToString()];
          for (var j = 0; j < sgn.pixCnt; j++)
          {
            if (sgn.rstr[j, 0] + delta <= 159)
            {
              SetPixel(ekr, sgn.rstr[j, 0] + delta, sgn.rstr[j, 1] + PosY);
            }
          }
          delta += sgn.CharWidth;
        }
      }
    }

    /// <summary>
    /// Установка пикселя на растре монитора
    /// </summary>
    /// <param name="bmp">Массив растра</param>
    /// <param name="x">Координата X</param>
    /// <param name="y">Координата Y</param>
    /// <param name="on">включить / выключить</param>
    private static void SetPixel(byte[] bmp, int x, int y, bool on = true)
    {
      if (on)
      {
        bmp[(x + y) + (y * (LogitechSDK.LOGI_LCD_MONO_WIDTH - 1))] = 255;
      }
      else
      {
        bmp[(x + y) + (y * (LogitechSDK.LOGI_LCD_MONO_WIDTH - 1))] = 0;
      }
    }

    public void Repaint()
    {
      if (fActive)
      {
        ClearMonitor();
      }
    }
  }

  /// <summary>
  /// Класс управляющий отображением дополнительной информации на мониторе клавиатуры. Не доработан до конца, в частности не реализован
  /// механизм управления логическими мониторами. 
  /// </summary>
  public static class LogitechMonitor
  {
    //-- набор логических мониторов
    private static readonly Dictionary<string, MonitorMONO> MMS = new Dictionary<string, MonitorMONO>();
    static Bitmap SplashBmp;
    //-- создаем стоящий на месте таймер опроса состояния нажатия клавиш на мониторе клавиатуры
    private static Timer lcdButtonsTimer = new Timer(TimerCallBack);

    private static LogitechLCDKeyProcessor KeyProcessorMetod;
    /// <summary>
    /// Признак того, что физический экран уже активизирован. По умолчанию, до вызова функции <seealso cref="InitLogitechMonitor"/> равен False
    /// </summary>
    public static bool IsMonitorInit = false;

    /// <summary>
    /// Очистка физического монитора
    /// </summary>
    public static void ClearMonitor()
    {
      SplashBmp =Properties.Resources.cls;
      //-- если монитор вообще подсоединен, то ..
      if (IsMonitorInit)
      {
        {
          try
          {
            LogitechSDK.LogiLcdMonoSetBackground(BitmapToByteRgb(SplashBmp));
            LogitechSDK.LogiLcdUpdate(LogitechSDK.LOGI_LCD_TYPE_MONO);
          }
          catch (Exception e)
          {
          }
        }
      }
    }

    /// <summary>
    /// Инициализация физического монитора. Автоматически создается логический монитор с именем MAIN (последнее не реализовано)
    /// </summary>
    /// <param name="Spl">Splash-изображение, выводимое при инициализации монитора. Если не задано, то монитор просто очищается.</param>
    /// <param name="KeyProcessor">Ссылка на метод - процессор, обрабатывающий нажатия клавиш на мониторе клавиатуры</param>
    /// <returns>"Истина" в случае успешной инициализации.</returns>
    public static bool InitLogitechMonitor(Bitmap Spl=null, LogitechLCDKeyProcessor KeyProcessor=null)
    {
      var res = false;
      //-- если растр не задан - используем очистку
      if (Spl == null) { SplashBmp = Properties.Resources.cls; }
      else { SplashBmp = Spl; }
      //-- установка процессора, если он не задан, (по умолчанию), то и эффекта не будет
      KeyProcessorMetod = KeyProcessor;
      IsMonitorInit = TryInitLogitechMonitor();
      //-- если монитор вообще подсоединен, то ..
      if (IsMonitorInit)
      {
        //-- пытаемся его инициализировать
        if (LogitechSDK.LogiLcdIsConnected(LogitechSDK.LOGI_LCD_TYPE_MONO))
        {
          //-- запуск таймера опроса состояния нажатий кнопок на экране клавиатуры
          lcdButtonsTimer.Change(1000, 250);
          //          LogitechSDK.LogiLcdMonoSetBackground(BitmapToByteRgbNaive(SplashBmp));
          LogitechSDK.LogiLcdMonoSetBackground(BitmapToByteRgb(SplashBmp));
          LogitechSDK.LogiLcdUpdate(LogitechSDK.LOGI_LCD_TYPE_MONO);
          //--создание основного логического монитора.
          //MMS.Add("MAIN", new MonitorMONO(Spl) { Active=true });
        }
        res=true;
      }
      return res;
    }

    public static void SetMonitorActive(string MonitorName = "MAIN")
    {
      var monitors = from rec in MMS select rec;
      foreach (var rec in monitors ) { rec.Value.Active = false;}
      MMS[MonitorName].Active = true;
    }
    /// <summary>
    /// Получить логический монитор по имени
    /// </summary>
    /// <param name="Name">Имя логического монитора</param>
    /// <returns>Ссылка на логический монитор</returns>
    public static MonitorMONO GetMonitorByName(string Name)
    {
      if (MMS.ContainsKey(Name)) { return MMS[Name]; }
      else
      { return null; }
    }

    private static void TimerCallBack(object inf)
    {
      if (IsMonitorInit)
      {
        //-- только если задан обработчик событий нажатия клавиш
        if (KeyProcessorMetod != null)
        {
          if (LogitechSDK.LogiLcdIsButtonPressed(LogitechSDK.LOGI_LCD_MONO_BUTTON_0)) KeyProcessorMetod(0);
          if (LogitechSDK.LogiLcdIsButtonPressed(LogitechSDK.LOGI_LCD_MONO_BUTTON_1)) KeyProcessorMetod(1);
          if (LogitechSDK.LogiLcdIsButtonPressed(LogitechSDK.LOGI_LCD_MONO_BUTTON_2)) KeyProcessorMetod(2);
          if (LogitechSDK.LogiLcdIsButtonPressed(LogitechSDK.LOGI_LCD_MONO_BUTTON_3)) KeyProcessorMetod(3);
        }
      }
    }

    public static void DeInitLogitechMonitor()
    {
      //-- удаление таймера
      lcdButtonsTimer.Dispose();
      if (LogitechSDK.LogiLcdIsConnected(LogitechSDK.LOGI_LCD_TYPE_MONO))
      {
        IsMonitorInit = false;
        LogitechSDK.LogiLcdShutdown();
      }
    }

    /// <summary>
    /// Отображение битовой матрицы на экране монитора
    /// </summary>
    /// <param name="View"></param>
    public static void Paint(byte[] View)
    {
      if (IsMonitorInit)
      {
        try
        {
          LogitechSDK.LogiLcdMonoSetBackground(View);
          LogitechSDK.LogiLcdUpdate(LogitechSDK.LOGI_LCD_TYPE_MONO);
        }
        catch (Exception e)
        {
          //throw;
        }
      }
    }

    public static bool TryInitLogitechMonitor()
    {
      // Поскольку у меня нет никакого другого варианта клавиатуры - я отлаживаю только для монохромного, уж извините )))
      return LogitechSDK.LogiLcdInit("KBD_LCD", LogitechSDK.LOGI_LCD_TYPE_MONO);//|LogitechSDK.LOGI_LCD_TYPE_COLOR);
    }

    /// <summary>
    /// Ускоренная версия копирования растра
    /// </summary>
    /// <param name="bmp">Объект Bitmap , который требуется скопировать в растр</param>
    /// <returns>Байтовый массив данных, подготовленный к выводу на экран клавиатуры</returns>
    public static unsafe byte[] BitmapToByteRgb(Bitmap bmp)
    {
      int width = bmp.Width,
          height = bmp.Height;
      var res = new byte[height*width];
      var bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bmp.PixelFormat);
      try
      {
        byte* curpos;
        for (var h = 0; h<height; h++)
        {
          curpos=((byte*)bd.Scan0)+h*bd.Stride;
          for (var w = 0; w<width; w++)
          {
            if (*(curpos++) > 0) { res[(w + h) + (h*(LogitechSDK.LOGI_LCD_MONO_WIDTH - 1))] = 255; }
            else{ res[(w + h) + (h*(LogitechSDK.LOGI_LCD_MONO_WIDTH - 1))] = 0; }
          }
        }
      }
      finally
      {
        bmp.UnlockBits(bd);
      }
      return res;
    }
  }

  /// <summary>
  /// Класс, управляющий дополнительными клавишами Logitech устройств
  /// </summary>
  public static class LogitechKeyboard{
    static LogitechGSDK.logiGkeyCB cblnstance;
    static LogitechKeyProcessor keyProcess;
    /// <summary>
    /// Инициализация хитрожопых клавиш Logitech устройств
    /// </summary>
    public static void InitLogitechKeyboard(LogitechKeyProcessor KeyProc)
    {
      //-- Назначаем внутренней переменной фунекцию обработчик нажатий клавиш
      keyProcess = KeyProc ?? throw new ArgumentNullException("При инициализации класса LogitechKeyboard необходимо задавать функцию-обработчик.");
      cblnstance = new LogitechGSDK.logiGkeyCB(GkeySDKCallback);
      LogitechGSDK.LogiGkeyInitWithoutContext(cblnstance);
    }
    /// <summary>
    /// Корректная ДЕинициализация клавиш Logitech устройств
    /// </summary>
    public static void DeInitLogitechKeyboard()
    {
      LogitechGSDK.LogiGkeyShutdown();
    }
    /// <summary>
    /// Функция, вызываемая библиотекой поддержки SDK
    /// </summary>
    /// <param name="gKeyCode">Структура, содержащая информацию о нажатой клавише</param>
    /// <param name="gKeyOrButtonString"></param>
    /// <param name="context"></param>
    static void GkeySDKCallback(LogitechGSDK.GkeyCode gKeyCode, String gKeyOrButtonString, IntPtr context)
    {
      keyProcess(gKeyCode, gKeyOrButtonString, context);

      /*if (gKeyCode.keyDown == 0)
      {
        if (gKeyCode.mouse == 1)
        {
// Code to handle what happens on gkey released on mouse
        }
        else
        {
// Code to handle what happens on gkey released on keyboard/headset
        }
      }
      else
      {
        if (gKeyCode.mouse == 1)
        {
// Code to handle what happens on gkey pressed on mouse
        }
        else
        {
// Code to handle what happens on gkey pressed on keyboard
        }
      }*/
    }
  }
}
