using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FastReport.Design;
using FastReport.Forms;

namespace FastReport.Wizards
{
  /// <summary>
  /// Represents the "New Data Source" wizard.
  /// </summary>
  public class NewDataSourceWizard : WizardBase
  {
    /// <inheritdoc/>
    public override bool Run(Designer designer)
    {
      using (DataWizardForm form = new DataWizardForm(designer.ActiveReport))
      {
        bool result = form.ShowDialog() == DialogResult.OK;
        if (result)
          designer.SetModified(this, "EditData");
        return result;
      }
    }
  }
}
