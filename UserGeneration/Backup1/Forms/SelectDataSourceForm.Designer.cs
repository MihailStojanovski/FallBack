namespace FastReport.Forms
{
  partial class SelectDataSourceForm
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectDataSourceForm));
      this.tvAvailableDs = new System.Windows.Forms.TreeView();
      this.lblHint = new System.Windows.Forms.Label();
      this.labelLine1 = new FastReport.Controls.LabelLine();
      this.btnAdd = new System.Windows.Forms.Button();
      this.btnRemove = new System.Windows.Forms.Button();
      this.btnRemoveAll = new System.Windows.Forms.Button();
      this.lblAvailableDs = new System.Windows.Forms.Label();
      this.lblSelectedDs = new System.Windows.Forms.Label();
      this.tvSelectedDs = new FastReport.Controls.DataTreeView();
      this.SuspendLayout();
      // 
      // btnOk
      // 
      this.btnOk.Location = new System.Drawing.Point(428, 292);
      // 
      // btnCancel
      // 
      this.btnCancel.Location = new System.Drawing.Point(8, 292);
      this.btnCancel.Visible = false;
      // 
      // tvAvailableDs
      // 
      this.tvAvailableDs.HideSelection = false;
      this.tvAvailableDs.Location = new System.Drawing.Point(8, 60);
      this.tvAvailableDs.Name = "tvAvailableDs";
      this.tvAvailableDs.Size = new System.Drawing.Size(220, 212);
      this.tvAvailableDs.TabIndex = 1;
      this.tvAvailableDs.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvAvailableDs_AfterSelect);
      // 
      // lblHint
      // 
      this.lblHint.AutoSize = true;
      this.lblHint.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.lblHint.Location = new System.Drawing.Point(8, 12);
      this.lblHint.Name = "lblHint";
      this.lblHint.Size = new System.Drawing.Size(317, 13);
      this.lblHint.TabIndex = 2;
      this.lblHint.Text = "Select one or several data sources to use in the report.";
      // 
      // labelLine1
      // 
      this.labelLine1.Location = new System.Drawing.Point(8, 272);
      this.labelLine1.Name = "labelLine1";
      this.labelLine1.Size = new System.Drawing.Size(496, 20);
      this.labelLine1.TabIndex = 4;
      // 
      // btnAdd
      // 
      this.btnAdd.Location = new System.Drawing.Point(240, 60);
      this.btnAdd.Name = "btnAdd";
      this.btnAdd.Size = new System.Drawing.Size(32, 23);
      this.btnAdd.TabIndex = 5;
      this.btnAdd.Text = ">";
      this.btnAdd.UseVisualStyleBackColor = true;
      this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
      // 
      // btnRemove
      // 
      this.btnRemove.Location = new System.Drawing.Point(240, 88);
      this.btnRemove.Name = "btnRemove";
      this.btnRemove.Size = new System.Drawing.Size(32, 23);
      this.btnRemove.TabIndex = 5;
      this.btnRemove.Text = "<";
      this.btnRemove.UseVisualStyleBackColor = true;
      this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
      // 
      // btnRemoveAll
      // 
      this.btnRemoveAll.Location = new System.Drawing.Point(240, 116);
      this.btnRemoveAll.Name = "btnRemoveAll";
      this.btnRemoveAll.Size = new System.Drawing.Size(32, 23);
      this.btnRemoveAll.TabIndex = 5;
      this.btnRemoveAll.Text = "<<";
      this.btnRemoveAll.UseVisualStyleBackColor = true;
      this.btnRemoveAll.Click += new System.EventHandler(this.btnRemoveAll_Click);
      // 
      // lblAvailableDs
      // 
      this.lblAvailableDs.AutoSize = true;
      this.lblAvailableDs.Location = new System.Drawing.Point(8, 40);
      this.lblAvailableDs.Name = "lblAvailableDs";
      this.lblAvailableDs.Size = new System.Drawing.Size(119, 13);
      this.lblAvailableDs.TabIndex = 6;
      this.lblAvailableDs.Text = "Available data sources:";
      // 
      // lblSelectedDs
      // 
      this.lblSelectedDs.AutoSize = true;
      this.lblSelectedDs.Location = new System.Drawing.Point(284, 40);
      this.lblSelectedDs.Name = "lblSelectedDs";
      this.lblSelectedDs.Size = new System.Drawing.Size(117, 13);
      this.lblSelectedDs.TabIndex = 6;
      this.lblSelectedDs.Text = "Selected data sources:";
      // 
      // tvSelectedDs
      // 
      this.tvSelectedDs.ExpandedNodes = ((System.Collections.Generic.List<string>)(resources.GetObject("tvSelectedDs.ExpandedNodes")));
      this.tvSelectedDs.ImageIndex = 0;
      this.tvSelectedDs.Location = new System.Drawing.Point(284, 60);
      this.tvSelectedDs.Name = "tvSelectedDs";
      this.tvSelectedDs.SelectedImageIndex = 0;
      this.tvSelectedDs.ShowColumns = false;
      this.tvSelectedDs.ShowDataSources = true;
      this.tvSelectedDs.ShowEnabledOnly = true;
      this.tvSelectedDs.ShowNone = false;
      this.tvSelectedDs.ShowRelations = false;
      this.tvSelectedDs.ShowTotals = false;
      this.tvSelectedDs.ShowVariables = false;
      this.tvSelectedDs.ShowParameters = false;
      this.tvSelectedDs.Size = new System.Drawing.Size(220, 212);
      this.tvSelectedDs.TabIndex = 7;
      this.tvSelectedDs.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvAvailableDs_AfterSelect);
      // 
      // SelectDataSourceForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.ClientSize = new System.Drawing.Size(512, 324);
      this.Controls.Add(this.tvSelectedDs);
      this.Controls.Add(this.lblSelectedDs);
      this.Controls.Add(this.lblAvailableDs);
      this.Controls.Add(this.btnRemoveAll);
      this.Controls.Add(this.btnRemove);
      this.Controls.Add(this.btnAdd);
      this.Controls.Add(this.labelLine1);
      this.Controls.Add(this.lblHint);
      this.Controls.Add(this.tvAvailableDs);
      this.Name = "SelectDataSourceForm";
      this.Text = "Select Data Source";
      this.Controls.SetChildIndex(this.btnOk, 0);
      this.Controls.SetChildIndex(this.tvAvailableDs, 0);
      this.Controls.SetChildIndex(this.btnCancel, 0);
      this.Controls.SetChildIndex(this.lblHint, 0);
      this.Controls.SetChildIndex(this.labelLine1, 0);
      this.Controls.SetChildIndex(this.btnAdd, 0);
      this.Controls.SetChildIndex(this.btnRemove, 0);
      this.Controls.SetChildIndex(this.btnRemoveAll, 0);
      this.Controls.SetChildIndex(this.lblAvailableDs, 0);
      this.Controls.SetChildIndex(this.lblSelectedDs, 0);
      this.Controls.SetChildIndex(this.tvSelectedDs, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TreeView tvAvailableDs;
    private System.Windows.Forms.Label lblHint;
    private FastReport.Controls.LabelLine labelLine1;
    private System.Windows.Forms.Button btnAdd;
    private System.Windows.Forms.Button btnRemove;
    private System.Windows.Forms.Button btnRemoveAll;
    private System.Windows.Forms.Label lblAvailableDs;
    private System.Windows.Forms.Label lblSelectedDs;
    private FastReport.Controls.DataTreeView tvSelectedDs;
  }
}
