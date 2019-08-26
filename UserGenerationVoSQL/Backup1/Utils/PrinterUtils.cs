using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing.Printing;

namespace FastReport.Utils
{
  internal static class PrinterUtils
  {
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
    private static extern IntPtr GlobalLock(IntPtr mem);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
    private static extern uint GlobalUnlock(IntPtr mem);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
    private static extern uint GlobalFree(IntPtr mem);

    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int DocumentProperties(IntPtr hwnd, IntPtr printer,
      string devicename, IntPtr devModeOut, IntPtr devModeInput, uint mode);

    public static void ShowPropertiesDialog(PrinterSettings printerSettings)
    {
      IntPtr hDevMode = printerSettings.GetHdevmode(printerSettings.DefaultPageSettings);
      IntPtr pDevMode = GlobalLock(hDevMode);
      int sizeNeeded = DocumentProperties(IntPtr.Zero, IntPtr.Zero, printerSettings.PrinterName, pDevMode, pDevMode, 0);
      IntPtr devModeData = Marshal.AllocHGlobal(sizeNeeded);
      int result = DocumentProperties(IntPtr.Zero, IntPtr.Zero, printerSettings.PrinterName, devModeData, pDevMode, 14);
      GlobalUnlock(hDevMode);

      if (result == 1)
      {
        printerSettings.SetHdevmode(devModeData);
        printerSettings.DefaultPageSettings.SetHdevmode(devModeData);
      }
      GlobalFree(hDevMode);
      Marshal.FreeHGlobal(devModeData);
    }
  }  
}
