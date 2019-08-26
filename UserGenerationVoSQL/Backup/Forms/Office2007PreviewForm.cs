using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using FastReport.Utils;

namespace FastReport.Forms
{
  internal partial class Office2007PreviewForm : Office2007Form
  {
    private void Office2007PreviewForm_FormClosed(object sender, FormClosedEventArgs e)
    {
      Done();
    }

    private void Office2007PreviewForm_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyData == Keys.Escape)
        Close();
    }

    private void Init()
    {
      Config.RestoreFormState(this);
      Icon = Config.PreviewSettings.Icon;
    }

    private void Done()
    {
      Config.SaveFormState(this);
    }

    public Office2007PreviewForm()
    {
      InitializeComponent();
      Name = "PreviewForm";
      Init();
      EnableGlass = false;
    }
  }
}