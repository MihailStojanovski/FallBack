namespace FastReport.Forms
{
  partial class AspSelectDataSourceForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.tvDataSources = new System.Windows.Forms.TreeView();
      this.SuspendLayout();
      // 
      // btnOk
      // 
      this.btnOk.Location = new System.Drawing.Point(120, 232);
      // 
      // btnCancel
      // 
      this.btnCancel.Location = new System.Drawing.Point(200, 232);
      // 
      // tvDataSources
      // 
      this.tvDataSources.CheckBoxes = true;
      this.tvDataSources.Location = new System.Drawing.Point(12, 12);
      this.tvDataSources.Name = "tvDataSources";
      this.tvDataSources.ShowLines = false;
      this.tvDataSources.ShowPlusMinus = false;
      this.tvDataSources.ShowRootLines = false;
      this.tvDataSources.Size = new System.Drawing.Size(264, 208);
      this.tvDataSources.TabIndex = 1;
      // 
      // AspSelectDataSourceForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.ClientSize = new System.Drawing.Size(287, 266);
      this.Controls.Add(this.tvDataSources);
      this.Name = "AspSelectDataSourceForm";
      this.Text = "Select Data Source";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AspSelectDataSourceForm_FormClosed);
      this.Controls.SetChildIndex(this.btnOk, 0);
      this.Controls.SetChildIndex(this.btnCancel, 0);
      this.Controls.SetChildIndex(this.tvDataSources, 0);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TreeView tvDataSources;
  }
}
