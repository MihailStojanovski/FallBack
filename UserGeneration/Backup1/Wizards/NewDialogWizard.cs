using System;
using System.Collections.Generic;
using System.Text;
using FastReport.Design;

namespace FastReport.Wizards
{
  /// <summary>
  /// Represents the "New Dialog" wizard.
  /// </summary>
  public class NewDialogWizard : WizardBase
  {
    /// <inheritdoc/>
    public override bool Run(Designer designer)
    {
      designer.cmdNewDialog.Invoke();
      return true;
    }
  }
}
