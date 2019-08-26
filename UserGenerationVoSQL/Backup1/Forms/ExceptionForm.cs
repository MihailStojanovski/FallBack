using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FastReport.Utils;

namespace FastReport.Forms
{
  internal partial class ExceptionForm : BaseDialogForm
  {
    private void ExceptionForm_Shown(object sender, EventArgs e)
    {
      lblException.Width = ClientSize.Width - lblException.Left * 2;
    }

    private void btnCopyToClipboard_Click(object sender, EventArgs e)
    {
      string text = "FastReport.Net v" + Config.Version + "\r\n";
      text += lblException.Text + "\r\n";
      text += tbStack.Text;
      Clipboard.SetText(text);
    }

    public override void Localize()
    {
      base.Localize();
      MyRes res = new MyRes("Forms,Exception");
      Text = res.Get("");
      lblHint.Text = res.Get("Hint");
      lblStack.Text = res.Get("Stack");
      btnCopyToClipboard.Text = res.Get("Copy");
    }
    
    public ExceptionForm(Exception ex)
    {
      InitializeComponent();
      Localize();
      lblException.Text = ex.Message;
      tbStack.Text = ex.StackTrace;
      if (ex.InnerException != null)
      {
        lblException.Text += "\r\nInner exception:\r\n" + ex.InnerException.Message;
        tbStack.Text = ex.InnerException.StackTrace + "\r\n" + tbStack.Text;
      }  
    }
  }
}

