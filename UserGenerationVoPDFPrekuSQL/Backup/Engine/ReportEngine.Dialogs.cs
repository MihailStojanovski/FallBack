using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FastReport.Dialog;

namespace FastReport.Engine
{
  public partial class ReportEngine
  {
    private bool RunDialog(DialogPage page)
    {
      try
      {
        page.InitializeControls();
        return page.ShowDialog() == DialogResult.OK;
      }
      finally
      {
        page.FinalizeControls();
      }
    }

    private bool RunDialogs()
    {
      foreach (PageBase page in Report.Pages)
      {
        if (page is DialogPage)
        {
          DialogPage dialogPage = page as DialogPage;
          if (dialogPage.Visible && !RunDialog(dialogPage))
            return false;
        }    
      }
      return true;
    }

  }
}
