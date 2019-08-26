using System;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using DevComponents.DotNetBar;

namespace FastReport.Utils
{
  internal static class FRMessageBox
  {
    public static void Error(string msg)
    {
      MessageBoxEx.UseSystemLocalizedString = true;
      MessageBoxEx.Show(msg, Res.Get("Messages,Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public static DialogResult Confirm(string msg, MessageBoxButtons buttons)
    {
      MessageBoxEx.UseSystemLocalizedString = true;
      return MessageBoxEx.Show(msg, Res.Get("Messages,Confirmation"), buttons, MessageBoxIcon.Question);
    }

    public static void Information(string msg)
    {
      MessageBoxEx.UseSystemLocalizedString = true;
      MessageBoxEx.Show(msg, Res.Get("Messages,Information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

  }
}