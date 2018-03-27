# Logitech_Monitor
Библиотека управления ЧБ монитором клавиатуры Logitech.

Сборка под платформу 4.6

Библиотека предназначена для управления простым выводом текстовой информации на черно-белый монитор клавиатуры Logitech из собственных программ на C#. Является самодостаточной, однако не до конца отлаженной. Также при некоторой доработке может использоваться и для вывода на цветной монитор клавиатур того же производителя.

Никаких ограничений по использованию - берите и дорабатывайте кому надо.


Вариант использования:

```C#
using etisLogitechDisplay;
...
private static void Main(string[] args)
{
    ...
    Display.Init(Properties.Resources.ETR, KeyProcessor);
    ... 
    AppContext = new winMainWin();
    Application.Run(AppContext);
    ...
    Display.DeInit();
    ...
}

private static void KeyProcessor(int KeyPressed)
{
      switch (KeyPressed)
      {
        case 0:
          ...
          break;
        case 1:
          ...
          break;
        case 2:
          ...
          break;
        case 3:
          ...
          break;
        default: 
          throw new ArgumentException();
      }
}

```

Класс Display. Подстраивается по конкретную программу.

```C#
using System;
using System.Drawing;
using plgLogitechSDK;
using System.Threading;

namespace LogitechDisplay
{
  /// <summary>
  /// Класс управления монитором клавиатуры Logitech. 
  /// </summary>
  public static class Display
  {
    #region -- Внутренние поля класса
    /// <summary>
    /// Название отображаемой системы
    /// </summary>
    private static string fSystemName;
    /// <summary>
    /// Принадлежность системы
    /// </summary>
    private static string fSystemOwner;
    /// <summary>
    /// Наличие в системе продаваемых редких товаров на одной из станций
    /// </summary>
    private static bool fRarPresent;
    /// <summary>
    /// Наличие в системе станций с инженером
    /// </summary>
    private static bool fIngPresent;
    /// <summary>
    /// Класс управления монитором клавиатуры
    /// </summary>
    private static MonitorMONO mon;
    /// <summary>
    /// Полотно, на котором отображается информация и потом выводится на экран монитора
    /// </summary>
    private static byte[] View;
    /// <summary>
    /// Поток обновления экрана монитора
    /// </summary>
    private static Thread ViewRefreshThread;
    /// <summary>
    /// Флаг окончания 
    /// </summary>
    private static bool Exit;
    #endregion
    
    #region Внешние свойства класса
    public static string SystemName
    {
      get { return fSystemName; }
      set
      {
        if (value != null && value != fSystemName)
        {
          fSystemName = value;
          RefreshDisplay();
        }
      }
    }

    public static bool RarPresent
    {
      get { return fRarPresent; }
      set
      {
        if (value != fRarPresent)
        {
          fRarPresent = value;
          RefreshDisplay();
        }
      }
    }
    public static bool IngPresent
    {
      get { return fIngPresent; }
      set
      {
        if (value != fIngPresent)
        {
          fIngPresent = value;
          RefreshDisplay();
        }
      }
    }
    public static int DampPeriod
    {
      get { return fDampPeriod; }
      set
      {
        if (value != fDampPeriod)
        {
          fDampPeriod = value;
          RefreshDisplay();
        }
      }
    } 
    #endregion

    /// <summary>
    /// Инициализация монитора
    /// </summary>
    /// <param name="KeyProcessor"></param>
    public static void Init(Bitmap Spl = null, LogitechLCDKeyProcessor KeyProcessor = null)
    {
      //-- Инициализация монитора
      var initRes = LogitechMonitor.InitLogitechMonitor(Spl, KeyProcessor);
      //-- создание (пока что единственного) логического монитора
      mon = new MonitorMONO();
      fSystemName = "«Не указана»";
      fRarPresent = false;
      fIngPresent = false;
      if (initRes)
        StartMonitor();
    }

    public static void DeInit()
    {
      LogitechMonitor.DeInitLogitechMonitor();
      Exit = true;
    }
    /// <summary>
    /// Установить дисплей
    /// </summary>
    /// <param name="SysName">Название текущей системы</param>
    /// <param name="rp">Наличие в системе продаваемых редких товаров на одной из станций</param>
    /// <param name="ip">Наличие в системе станций с инженером</param>
    public static void SetupDisplay(string SysName, string Owner, bool rp, bool ip)
    {
      fSystemName = SysName;
      fSystemOwner = Owner;
      fRarPresent = rp;
      fIngPresent = ip;
      RefreshDisplay();
    }

    /// <summary>
    /// Обновление дисплея клавиатуры
    /// </summary>
    private static void RefreshDisplay()
    {
      //-- Получаем чистое полотно, на котором будем рисовать текущее содержимое дисплея
      View = LogitechMonitor.BitmapToByteRgbNaive(Properties.Resources.cls);
      //-- предварительная подготовка информации о требовании дампа.
      //-- надпись и центрирование по первой кнопке дисплея последняя строка с 0 по 39 пиксель
      //-- и запроса информации по EDAPI
      //-- с 80 по 119 пиксель
      var txtEDAPI = "EDAPI";
      var posEDAPI = 88;
      // на первой строке просто надпись
      mon.TextOut(ref View, 0, 0, "Текущая система:");
      //-- вторая строка - название текущей системы (даже при маленьком шрифте может и не уместиться)
      mon.TextOut(ref View, 0, 9, fSystemName);
      //-- третья строка - принадлежность системы
      mon.TextOut(ref View, 0, 18, fSystemOwner);
      //-- вывод надписей (иконок) над кнопками
      mon.TextOut(ref View, posEDAPI, 35, txtEDAPI);
      //-- текущее системное время: часы, минуты, секунды... этого достаточно
      var posTime = 161 - mon.GetStringLengthPost("00:00:00");
      mon.TextOutPost(ref View, posTime, 0, DateTime.Now.TimeOfDay.ToString("hh\\:mm\\:ss"));
      //-- Перенос сформированного изображения на экран монитора клавиатуры
      try
      {
        LogitechSDK.LogiLcdMonoSetBackground(View);
        LogitechSDK.LogiLcdUpdate(LogitechSDK.LOGI_LCD_TYPE_MONO);
      }
      catch (Exception e)
      {
      }

    }

    private static void StartMonitor()
    {
        Exit = false;
        ViewRefreshThread = new Thread(new ThreadStart(LoopRefresh)) {Name = "ETR_RefreshLogitechMonitor", IsBackground = true};
        ViewRefreshThread.Start();
    }

    private static void LoopRefresh()
    {
      while (!Exit)
      {
        try
        {
          Thread.Sleep(1000);
          RefreshDisplay();
        }
        catch (Exception ex)
        {
          //ServiceWins.ShowError(ex);
        }
      }
    }
  }
}
```
