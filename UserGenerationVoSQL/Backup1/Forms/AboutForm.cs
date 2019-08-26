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
  internal partial class AboutForm : BaseDialogForm
  {
    private void label5_Click(object sender, EventArgs e)
    {
      System.Diagnostics.Process.Start(label5.Text);
    }

    private void AboutForm_Shown(object sender, EventArgs e)
    {
      int labelWidth = ClientSize.Width - label3.Left * 2;
      label3.Width = labelWidth;
      label4.Width = labelWidth;
      label5.Width = labelWidth;
    }

    private void AboutForm_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyData == Keys.Escape)
        DialogResult = DialogResult.Cancel;
    }

    public AboutForm()
    {
      InitializeComponent();
      label2.Text = "Version " + Config.Version;
    }
  }
}

