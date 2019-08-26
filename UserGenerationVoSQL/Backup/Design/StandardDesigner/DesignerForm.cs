using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using FastReport.Utils;

namespace FastReport.Design.StandardDesigner
{
  /// <summary>
  /// Represents standard designer's form.
  /// </summary>
  /// <remarks>
  /// This form contains the <see cref="DesignerControl"/>. Use the <see cref="Designer"/> 
  /// property to get access to this control.
  /// <para/>Usually you don't need to create an instance of this class. The designer can be called
  /// using the <see cref="FastReport.Report.Design()"/> method of 
  /// the <see cref="FastReport.Report"/> instance.
  /// <para/>If you decided to use this class, you need:
  /// <list type="bullet">
  ///   <item>
  ///     <description>create an instance of this class;</description>
  ///   </item>
  ///   <item>
  ///     <description>set the <b>Designer.Report</b> property to report that you need to design;</description>
  ///   </item>
  ///   <item>
  ///     <description>call either <b>ShowModal</b> or <b>Show</b> methods to display a form.</description>
  ///   </item>
  /// </list>
  /// </remarks>
  public partial class DesignerForm : Form
  {
    private DesignerControl FDesigner;
    
    /// <summary>
    /// Gets a reference to the <see cref="DesignerControl"/> control which is actually a designer.
    /// </summary>
    public DesignerControl Designer
    {
      get { return FDesigner; }
    }

    private void DesignerForm_Load(object sender, EventArgs e)
    {
      // bug/inconsistent behavior in .Net: if we set WindowState to Maximized, the
      // Load event will be fired *after* the form is shown.
      bool maximized = Config.RestoreFormState(this, true);
      Designer.RestoreConfig();
      if (maximized)
        WindowState = FormWindowState.Maximized;

      Config.DesignerSettings.OnDesignerLoaded(Designer, EventArgs.Empty);
    }

    private void DesignerForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      if (!Designer.CloseAll())
      {
        e.Cancel = true;
      }
      else
      {
        Config.SaveFormState(this);
        Designer.SaveConfig();
      }
    }
    
    /// <summary>
    /// Creates a new instance of the <see cref="DesignerForm"/> class with default settings.
    /// </summary>
    public DesignerForm()
    {
      InitializeComponent();

      FDesigner = new DesignerControl();
      FDesigner.Dock = DockStyle.Fill;
      FDesigner.UIStyle = Config.UIStyle;
      Controls.Add(FDesigner);
      
      Font = DrawUtils.DefaultFont;
      Icon = Config.DesignerSettings.Icon;
    }
  }
}