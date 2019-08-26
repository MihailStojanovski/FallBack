namespace FastReport.Preview
{
  partial class PreviewControl
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;


    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.tabControl = new DevComponents.DotNetBar.TabControl();
      this.splitter = new DevComponents.DotNetBar.ExpandableSplitter();
      this.toolBar = new DevComponents.DotNetBar.Bar();
      this.tbPageNo = new DevComponents.DotNetBar.Controls.TextBoxX();
      this.btnPrint = new DevComponents.DotNetBar.ButtonItem();
      this.btnOpen = new DevComponents.DotNetBar.ButtonItem();
      this.btnSave = new DevComponents.DotNetBar.ButtonItem();
      this.btnEmail = new DevComponents.DotNetBar.ButtonItem();
      this.btnEmailMapi = new DevComponents.DotNetBar.ButtonItem();
      this.btnFind = new DevComponents.DotNetBar.ButtonItem();
      this.btnOutline = new DevComponents.DotNetBar.ButtonItem();
      this.btnPageSetup = new DevComponents.DotNetBar.ButtonItem();
      this.btnEdit = new DevComponents.DotNetBar.ButtonItem();
      this.btnWatermark = new DevComponents.DotNetBar.ButtonItem();
      this.btnFirst = new DevComponents.DotNetBar.ButtonItem();
      this.btnPrior = new DevComponents.DotNetBar.ButtonItem();
      this.controlContainerItem1 = new DevComponents.DotNetBar.ControlContainerItem();
      this.lblTotalPages = new DevComponents.DotNetBar.LabelItem();
      this.btnNext = new DevComponents.DotNetBar.ButtonItem();
      this.btnLast = new DevComponents.DotNetBar.ButtonItem();
      this.btnClose = new DevComponents.DotNetBar.ButtonItem();
      this.statusBar = new DevComponents.DotNetBar.Bar();
      this.lblStatus = new DevComponents.DotNetBar.LabelItem();
      this.lblUrl = new DevComponents.DotNetBar.LabelItem();
      this.lblPerformance = new DevComponents.DotNetBar.LabelItem();
      this.itemContainer1 = new DevComponents.DotNetBar.ItemContainer();
      this.itemContainer2 = new DevComponents.DotNetBar.ItemContainer();
      this.btnZoomPageWidth = new DevComponents.DotNetBar.ButtonItem();
      this.btnZoomWholePage = new DevComponents.DotNetBar.ButtonItem();
      this.btnZoom100 = new DevComponents.DotNetBar.ButtonItem();
      this.slZoom = new DevComponents.DotNetBar.SliderItem();
      this.outlineControl = new FastReport.Preview.OutlineControl();
      ((System.ComponentModel.ISupportInitialize)(this.tabControl)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.toolBar)).BeginInit();
      this.toolBar.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.statusBar)).BeginInit();
      this.SuspendLayout();
      // 
      // tabControl
      // 
      this.tabControl.AutoCloseTabs = true;
      this.tabControl.BackColor = System.Drawing.SystemColors.AppWorkspace;
      this.tabControl.CanReorderTabs = true;
      this.tabControl.CloseButtonOnTabsVisible = true;
      this.tabControl.CloseButtonVisible = true;
      this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.tabControl.Location = new System.Drawing.Point(153, 26);
      this.tabControl.Name = "tabControl";
      this.tabControl.SelectedTabFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.tabControl.SelectedTabIndex = -1;
      this.tabControl.Size = new System.Drawing.Size(490, 229);
      this.tabControl.Style = DevComponents.DotNetBar.eTabStripStyle.Office2007Document;
      this.tabControl.TabIndex = 2;
      this.tabControl.TabLayoutType = DevComponents.DotNetBar.eTabLayoutType.FixedWithNavigationBox;
      this.tabControl.TabsVisible = false;
      this.tabControl.Text = "tabControl1";
      this.tabControl.TabItemClose += new DevComponents.DotNetBar.TabStrip.UserActionEventHandler(this.tabControl1_TabItemClose);
      this.tabControl.Resize += new System.EventHandler(this.tabControl_Resize);
      this.tabControl.SelectedTabChanged += new DevComponents.DotNetBar.TabStrip.SelectedTabChangedEventHandler(this.tabControl1_SelectedTabChanged);
      // 
      // splitter
      // 
      this.splitter.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(101)))), ((int)(((byte)(147)))), ((int)(((byte)(207)))));
      this.splitter.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
      this.splitter.BackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
      this.splitter.Expandable = false;
      this.splitter.ExpandFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(101)))), ((int)(((byte)(147)))), ((int)(((byte)(207)))));
      this.splitter.ExpandFillColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
      this.splitter.ExpandLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
      this.splitter.ExpandLineColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemText;
      this.splitter.GripDarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
      this.splitter.GripDarkColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemText;
      this.splitter.GripLightColor = System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(239)))), ((int)(((byte)(255)))));
      this.splitter.GripLightColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarBackground;
      this.splitter.HotBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(151)))), ((int)(((byte)(61)))));
      this.splitter.HotBackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(184)))), ((int)(((byte)(94)))));
      this.splitter.HotBackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemPressedBackground2;
      this.splitter.HotBackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemPressedBackground;
      this.splitter.HotExpandFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(101)))), ((int)(((byte)(147)))), ((int)(((byte)(207)))));
      this.splitter.HotExpandFillColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
      this.splitter.HotExpandLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
      this.splitter.HotExpandLineColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemText;
      this.splitter.HotGripDarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(101)))), ((int)(((byte)(147)))), ((int)(((byte)(207)))));
      this.splitter.HotGripDarkColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
      this.splitter.HotGripLightColor = System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(239)))), ((int)(((byte)(255)))));
      this.splitter.HotGripLightColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarBackground;
      this.splitter.Location = new System.Drawing.Point(150, 26);
      this.splitter.Name = "splitter";
      this.splitter.Size = new System.Drawing.Size(3, 229);
      this.splitter.Style = DevComponents.DotNetBar.eSplitterStyle.Office2007;
      this.splitter.TabIndex = 4;
      this.splitter.TabStop = false;
      // 
      // toolBar
      // 
      this.toolBar.AccessibleDescription = "bar1 (toolBar)";
      this.toolBar.AccessibleName = "bar1";
      this.toolBar.AccessibleRole = System.Windows.Forms.AccessibleRole.ToolBar;
      this.toolBar.Controls.Add(this.tbPageNo);
      this.toolBar.Dock = System.Windows.Forms.DockStyle.Top;
      this.toolBar.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.btnPrint,
            this.btnOpen,
            this.btnSave,
            this.btnEmail,
            this.btnEmailMapi,
            this.btnFind,
            this.btnOutline,
            this.btnPageSetup,
            this.btnEdit,
            this.btnWatermark,
            this.btnFirst,
            this.btnPrior,
            this.controlContainerItem1,
            this.lblTotalPages,
            this.btnNext,
            this.btnLast,
            this.btnClose});
      this.toolBar.Location = new System.Drawing.Point(0, 0);
      this.toolBar.Name = "toolBar";
      this.toolBar.RoundCorners = false;
      this.toolBar.Size = new System.Drawing.Size(643, 26);
      this.toolBar.Stretch = true;
      this.toolBar.Style = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
      this.toolBar.TabIndex = 6;
      this.toolBar.TabStop = false;
      this.toolBar.Text = "bar1";
      // 
      // tbPageNo
      // 
      // 
      // 
      // 
      this.tbPageNo.Border.Class = "TextBoxBorder";
      this.tbPageNo.Location = new System.Drawing.Point(436, 2);
      this.tbPageNo.Name = "tbPageNo";
      this.tbPageNo.Size = new System.Drawing.Size(40, 21);
      this.tbPageNo.TabIndex = 1;
      this.tbPageNo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.tbPageNo.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbPageNo_KeyDown);
      this.tbPageNo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbPageNo_KeyPress);
      this.tbPageNo.Click += new System.EventHandler(this.tbPageNo_Click);
      // 
      // btnPrint
      // 
      this.btnPrint.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
      this.btnPrint.Name = "btnPrint";
      this.btnPrint.Text = "Print";
      this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
      // 
      // btnOpen
      // 
      this.btnOpen.Name = "btnOpen";
      this.btnOpen.Text = "Open";
      this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
      // 
      // btnSave
      // 
      this.btnSave.AutoExpandOnClick = true;
      this.btnSave.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
      this.btnSave.Name = "btnSave";
      this.btnSave.Text = "Save";
      // 
      // btnEmail
      // 
      this.btnEmail.Name = "btnEmail";
      this.btnEmail.Text = "Email";
      this.btnEmail.Click += new System.EventHandler(this.btnEmail_Click);
      // 
      // btnEmailMapi
      // 
      this.btnEmailMapi.AutoExpandOnClick = true;
      this.btnEmailMapi.Name = "btnEmailMapi";
      this.btnEmailMapi.Text = "Email";
      // 
      // btnFind
      // 
      this.btnFind.Name = "btnFind";
      this.btnFind.Text = "Find";
      this.btnFind.Click += new System.EventHandler(this.btnFind_Click);
      // 
      // btnOutline
      // 
      this.btnOutline.AutoCheckOnClick = true;
      this.btnOutline.BeginGroup = true;
      this.btnOutline.Name = "btnOutline";
      this.btnOutline.Text = "Outline";
      this.btnOutline.Click += new System.EventHandler(this.btnOutline_Click);
      // 
      // btnPageSetup
      // 
      this.btnPageSetup.Name = "btnPageSetup";
      this.btnPageSetup.Text = "Page Setup";
      this.btnPageSetup.Click += new System.EventHandler(this.btnPageSetup_Click);
      // 
      // btnEdit
      // 
      this.btnEdit.Name = "btnEdit";
      this.btnEdit.Text = "Edit";
      this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
      // 
      // btnWatermark
      // 
      this.btnWatermark.Name = "btnWatermark";
      this.btnWatermark.Text = "Watermark";
      this.btnWatermark.Click += new System.EventHandler(this.btnWatermark_Click);
      // 
      // btnFirst
      // 
      this.btnFirst.BeginGroup = true;
      this.btnFirst.Name = "btnFirst";
      this.btnFirst.Text = "First";
      this.btnFirst.Click += new System.EventHandler(this.btnFirst_Click);
      // 
      // btnPrior
      // 
      this.btnPrior.Name = "btnPrior";
      this.btnPrior.Text = "Prior";
      this.btnPrior.Click += new System.EventHandler(this.btnPrior_Click);
      // 
      // controlContainerItem1
      // 
      this.controlContainerItem1.AllowItemResize = false;
      this.controlContainerItem1.Control = this.tbPageNo;
      this.controlContainerItem1.MenuVisibility = DevComponents.DotNetBar.eMenuVisibility.VisibleAlways;
      this.controlContainerItem1.Name = "controlContainerItem1";
      // 
      // lblTotalPages
      // 
      this.lblTotalPages.ForeColor = System.Drawing.SystemColors.WindowText;
      this.lblTotalPages.Name = "lblTotalPages";
      this.lblTotalPages.Text = "ofM";
      // 
      // btnNext
      // 
      this.btnNext.Name = "btnNext";
      this.btnNext.Text = "Next";
      this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
      // 
      // btnLast
      // 
      this.btnLast.Name = "btnLast";
      this.btnLast.Text = "Last";
      this.btnLast.Click += new System.EventHandler(this.btnLast_Click);
      // 
      // btnClose
      // 
      this.btnClose.BeginGroup = true;
      this.btnClose.Name = "btnClose";
      this.btnClose.Text = "Close";
      this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
      // 
      // statusBar
      // 
      this.statusBar.AccessibleDescription = "DotNetBar Bar (statusBar)";
      this.statusBar.AccessibleName = "DotNetBar Bar";
      this.statusBar.AccessibleRole = System.Windows.Forms.AccessibleRole.StatusBar;
      this.statusBar.BarType = DevComponents.DotNetBar.eBarType.StatusBar;
      this.statusBar.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.statusBar.GrabHandleStyle = DevComponents.DotNetBar.eGrabHandleStyle.ResizeHandle;
      this.statusBar.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.lblStatus,
            this.lblUrl,
            this.lblPerformance,
            this.itemContainer1});
      this.statusBar.Location = new System.Drawing.Point(0, 255);
      this.statusBar.Name = "statusBar";
      this.statusBar.PaddingBottom = 2;
      this.statusBar.PaddingTop = 3;
      this.statusBar.Size = new System.Drawing.Size(643, 28);
      this.statusBar.Stretch = true;
      this.statusBar.Style = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
      this.statusBar.TabIndex = 5;
      this.statusBar.TabStop = false;
      // 
      // lblStatus
      // 
      this.lblStatus.Height = 19;
      this.lblStatus.Name = "lblStatus";
      this.lblStatus.Text = "   ";
      this.lblStatus.Width = 200;
      // 
      // lblUrl
      // 
      this.lblUrl.Height = 19;
      this.lblUrl.Name = "lblUrl";
      this.lblUrl.Stretch = true;
      this.lblUrl.Text = "   ";
      // 
      // lblPerformance
      // 
      this.lblPerformance.Height = 19;
      this.lblPerformance.Name = "lblPerformance";
      this.lblPerformance.Text = "   ";
      // 
      // itemContainer1
      // 
      // 
      // 
      // 
      this.itemContainer1.BackgroundStyle.Class = "Office2007StatusBarBackground2";
      this.itemContainer1.Name = "itemContainer1";
      this.itemContainer1.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.itemContainer2,
            this.slZoom});
      // 
      // itemContainer2
      // 
      this.itemContainer2.BeginGroup = true;
      this.itemContainer2.Name = "itemContainer2";
      this.itemContainer2.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.btnZoomPageWidth,
            this.btnZoomWholePage,
            this.btnZoom100});
      this.itemContainer2.VerticalItemAlignment = DevComponents.DotNetBar.eVerticalItemsAlignment.Middle;
      // 
      // btnZoomPageWidth
      // 
      this.btnZoomPageWidth.Name = "btnZoomPageWidth";
      this.btnZoomPageWidth.Text = "PageWidth";
      this.btnZoomPageWidth.Click += new System.EventHandler(this.btnZoomPageWidth_Click);
      // 
      // btnZoomWholePage
      // 
      this.btnZoomWholePage.Name = "btnZoomWholePage";
      this.btnZoomWholePage.Text = "WholePage";
      this.btnZoomWholePage.Click += new System.EventHandler(this.btnZoomWholePage_Click);
      // 
      // btnZoom100
      // 
      this.btnZoom100.Name = "btnZoom100";
      this.btnZoom100.Text = "Zoom100";
      this.btnZoom100.Click += new System.EventHandler(this.btnZoom100_Click);
      // 
      // slZoom
      // 
      this.slZoom.Maximum = 200;
      this.slZoom.Name = "slZoom";
      this.slZoom.Step = 5;
      this.slZoom.Text = "100%";
      this.slZoom.Value = 100;
      this.slZoom.Width = 120;
      this.slZoom.ValueChanged += new System.EventHandler(this.slZoom_ValueChanged);
      // 
      // outlineControl
      // 
      this.outlineControl.Dock = System.Windows.Forms.DockStyle.Left;
      this.outlineControl.Location = new System.Drawing.Point(0, 26);
      this.outlineControl.Name = "outlineControl";
      this.outlineControl.Size = new System.Drawing.Size(150, 229);
      this.outlineControl.TabIndex = 3;
      // 
      // PreviewControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.AppWorkspace;
      this.Controls.Add(this.tabControl);
      this.Controls.Add(this.splitter);
      this.Controls.Add(this.outlineControl);
      this.Controls.Add(this.toolBar);
      this.Controls.Add(this.statusBar);
      this.Name = "PreviewControl";
      this.Size = new System.Drawing.Size(643, 283);
      ((System.ComponentModel.ISupportInitialize)(this.tabControl)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.toolBar)).EndInit();
      this.toolBar.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.statusBar)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private DevComponents.DotNetBar.TabControl tabControl;
    private OutlineControl outlineControl;
    private DevComponents.DotNetBar.ExpandableSplitter splitter;
    private DevComponents.DotNetBar.LabelItem lblStatus;
    private DevComponents.DotNetBar.LabelItem lblUrl;
    private DevComponents.DotNetBar.LabelItem lblPerformance;
    private DevComponents.DotNetBar.ItemContainer itemContainer1;
    private DevComponents.DotNetBar.ItemContainer itemContainer2;
    private DevComponents.DotNetBar.SliderItem slZoom;
    private DevComponents.DotNetBar.ButtonItem btnPrint;
    private DevComponents.DotNetBar.ButtonItem btnOpen;
    private DevComponents.DotNetBar.ButtonItem btnSave;
    private DevComponents.DotNetBar.ButtonItem btnEmail;
    private DevComponents.DotNetBar.ButtonItem btnEmailMapi;
    private DevComponents.DotNetBar.ButtonItem btnFind;
    private DevComponents.DotNetBar.ButtonItem btnOutline;
    private DevComponents.DotNetBar.ButtonItem btnPageSetup;
    private DevComponents.DotNetBar.ButtonItem btnEdit;
    private DevComponents.DotNetBar.ButtonItem btnWatermark;
    private DevComponents.DotNetBar.ButtonItem btnFirst;
    private DevComponents.DotNetBar.ButtonItem btnPrior;
    private DevComponents.DotNetBar.ButtonItem btnNext;
    private DevComponents.DotNetBar.ButtonItem btnLast;
    private DevComponents.DotNetBar.ButtonItem btnClose;
    private DevComponents.DotNetBar.ButtonItem btnZoomPageWidth;
    private DevComponents.DotNetBar.ButtonItem btnZoomWholePage;
    private DevComponents.DotNetBar.ButtonItem btnZoom100;
    private DevComponents.DotNetBar.Controls.TextBoxX tbPageNo;
    private DevComponents.DotNetBar.ControlContainerItem controlContainerItem1;
    private DevComponents.DotNetBar.LabelItem lblTotalPages;
    private DevComponents.DotNetBar.Bar statusBar;
    private DevComponents.DotNetBar.Bar toolBar;
  }
}
