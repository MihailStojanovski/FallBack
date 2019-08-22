using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using FastReport.Editor;
using FastReport.Editor.Syntax;
using FastReport.Utils;
using FastReport.Code;
using FastReport.Design.PageDesigners.Code;


namespace FastReport.Forms
{
  internal class CodePageOptions : DesignerOptionsPage
  {
    private CodePageDesigner FEditor;
    private CheckBox cbEnableVirtualSpace;
    private CheckBox cbAllowOutlining;
    private NumericUpDown udTabSize;
    private Label lblTabSize;
    private CheckBox cbUseSpaces;

    private void InitializeComponent()
    {
      this.cbEnableVirtualSpace = new System.Windows.Forms.CheckBox();
      this.cbUseSpaces = new System.Windows.Forms.CheckBox();
      this.cbAllowOutlining = new System.Windows.Forms.CheckBox();
      this.udTabSize = new System.Windows.Forms.NumericUpDown();
      this.lblTabSize = new System.Windows.Forms.Label();
      this.tc1.SuspendLayout();
      this.tab1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.udTabSize)).BeginInit();
      this.SuspendLayout();
      // 
      // tab1
      // 
      this.tab1.Controls.Add(this.udTabSize);
      this.tab1.Controls.Add(this.lblTabSize);
      this.tab1.Controls.Add(this.cbEnableVirtualSpace);
      this.tab1.Controls.Add(this.cbUseSpaces);
      this.tab1.Controls.Add(this.cbAllowOutlining);
      // 
      // cbEnableVirtualSpace
      // 
      this.cbEnableVirtualSpace.AutoSize = true;
      this.cbEnableVirtualSpace.Location = new System.Drawing.Point(16, 16);
      this.cbEnableVirtualSpace.Name = "cbEnableVirtualSpace";
      this.cbEnableVirtualSpace.Size = new System.Drawing.Size(122, 17);
      this.cbEnableVirtualSpace.TabIndex = 0;
      this.cbEnableVirtualSpace.Text = "Enable virtual space";
      this.cbEnableVirtualSpace.UseVisualStyleBackColor = true;
      // 
      // cbUseSpaces
      // 
      this.cbUseSpaces.AutoSize = true;
      this.cbUseSpaces.Location = new System.Drawing.Point(16, 40);
      this.cbUseSpaces.Name = "cbUseSpaces";
      this.cbUseSpaces.Size = new System.Drawing.Size(130, 17);
      this.cbUseSpaces.TabIndex = 1;
      this.cbUseSpaces.Text = "Use spaces to fill tabs";
      this.cbUseSpaces.UseVisualStyleBackColor = true;
      // 
      // cbAllowOutlining
      // 
      this.cbAllowOutlining.AutoSize = true;
      this.cbAllowOutlining.Location = new System.Drawing.Point(16, 64);
      this.cbAllowOutlining.Name = "cbAllowOutlining";
      this.cbAllowOutlining.Size = new System.Drawing.Size(94, 17);
      this.cbAllowOutlining.TabIndex = 2;
      this.cbAllowOutlining.Text = "Allow outlining";
      this.cbAllowOutlining.UseVisualStyleBackColor = true;
      // 
      // udTabSize
      // 
      this.udTabSize.Location = new System.Drawing.Point(152, 92);
      this.udTabSize.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
      this.udTabSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.udTabSize.Name = "udTabSize";
      this.udTabSize.Size = new System.Drawing.Size(76, 21);
      this.udTabSize.TabIndex = 5;
      this.udTabSize.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // lblTabSize
      // 
      this.lblTabSize.AutoSize = true;
      this.lblTabSize.Location = new System.Drawing.Point(16, 96);
      this.lblTabSize.Name = "lblTabSize";
      this.lblTabSize.Size = new System.Drawing.Size(50, 13);
      this.lblTabSize.TabIndex = 4;
      this.lblTabSize.Text = "Tab size:";
      // 
      // CodePageOptions
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.ClientSize = new System.Drawing.Size(389, 313);
      this.Name = "CodePageOptions";
      this.tc1.ResumeLayout(false);
      this.tab1.ResumeLayout(false);
      this.tab1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.udTabSize)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    public void Localize()
    {
      MyRes res = new MyRes("Forms,CodePageOptions");
      tab1.Text = res.Get("");
      cbEnableVirtualSpace.Text = res.Get("EnableVirtualSpace");
      cbUseSpaces.Text = res.Get("UseSpaces");
      cbAllowOutlining.Text = res.Get("AllowOutlining");
      lblTabSize.Text = res.Get("TabSize");
    }

    public override void Init()
    {
      cbEnableVirtualSpace.Checked = CodePageSettings.EnableVirtualSpace;
      cbUseSpaces.Checked = CodePageSettings.UseSpaces;
      cbAllowOutlining.Checked = CodePageSettings.AllowOutlining;
      udTabSize.Value = CodePageSettings.TabSize;
    }

    public override void Done(DialogResult result)
    {
      if (result == DialogResult.OK)
      {
        CodePageSettings.EnableVirtualSpace = cbEnableVirtualSpace.Checked;
        CodePageSettings.UseSpaces = cbUseSpaces.Checked;
        CodePageSettings.AllowOutlining = cbAllowOutlining.Checked;
        CodePageSettings.TabSize = (int)udTabSize.Value;
        FEditor.UpdateOptions();
      }  
    }

    public CodePageOptions(CodePageDesigner editor) : base()
    {
      FEditor = editor;
      InitializeComponent();
      Localize();
    }

  }

}
