using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms.PropertyGridInternal;
using System.Reflection;
using FastReport.Utils;
using FastReport.Controls;
using System.Runtime.InteropServices;
using DevComponents.DotNetBar.Controls;
using DevComponents.DotNetBar;

namespace FastReport.Design.ToolWindows
{
  /// <summary>
  /// Represents the "Properties" window.
  /// </summary>
  public class PropertiesWindow : ToolWindowBase
  {
    #region Fields
    private bool FUpdating;
    private FRPropertyGrid Grid;
    private ComboBoxEx cbxObjects;
    private Bar FToolbar;
    private ButtonItem btnCategory;
    private ButtonItem btnAlphabetical;
    private ButtonItem btnProperties;
    private ButtonItem btnEvents;
    #endregion

    #region Private Methods
    private void cbxObjects_DrawItem(object sender, DrawItemEventArgs e)
    {
      e.DrawBackground();
      Graphics g = e.Graphics;
      if (e.Index >= 0)
      {
        Base c = cbxObjects.Items[e.Index] as Base;
        using (Font f = new Font(e.Font, FontStyle.Bold))
        {
          SizeF sz = g.MeasureString(c.Name, f);
          if (c is Report)
          {
            TextRenderer.DrawText(g, c.ClassName, f, new Point(e.Bounds.X, e.Bounds.Y), e.ForeColor);
          }
          else
          {
            TextRenderer.DrawText(g, c.Name, f, new Point(e.Bounds.X, e.Bounds.Y), e.ForeColor);
            TextRenderer.DrawText(g, c.ClassName, e.Font, new Point(e.Bounds.X + (int)sz.Width + 3, e.Bounds.Y), e.ForeColor);
          }
        }
      }
      else
      {
        SelectedObjectCollection selection = Designer.SelectedObjects;
        string s = selection.Count == 0 ? "" : selection.Count > 1 ?
          String.Format(Res.Get("Designer,ToolWindow,Properties,NObjectsSelected"), selection.Count) :
          selection[0].Name;
        TextRenderer.DrawText(g, s, e.Font, new Point(e.Bounds.X, e.Bounds.Y), e.ForeColor);
      }
    }

    private void cbxObjects_SelectedValueChanged(object sender, EventArgs e)
    {
      if (FUpdating)
        return;
      Base c = cbxObjects.SelectedItem as Base;
      if (!(c is Report) && Designer.ActiveReportTab != null)
      {
        FUpdating = true;
        Designer.ActiveReportTab.ActivePage = c.Page;
        FUpdating = false;
      }
      if (Designer.SelectedObjects != null)
      {
        Designer.SelectedObjects.Clear();
        Designer.SelectedObjects.Add(c);
        Designer.SelectionChanged(null);
      }
    }

    private void grid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
    {
      Designer.SetModified(null, "Change");
    }

    private void ParentControl_VisibleChanged(object sender, EventArgs e)
    {
      // workaround property grid bug
      Grid.ShowEvents = true;
    }

    private void btnCategory_Click(object sender, EventArgs e)
    {
      Grid.PropertySort = btnCategory.Checked ? PropertySort.Categorized : PropertySort.Alphabetical;
    }

    private void btnProperties_Click(object sender, EventArgs e)
    {
      Grid.SelectedTabIndex = 0;
    }

    private void btnEvents_Click(object sender, EventArgs e)
    {
      Grid.SelectedTabIndex = 1;
    }
    #endregion

    #region Public Methods
    /// <inheritdoc/>
    public override void SaveState()
    {
      XmlItem xi = Config.Root.FindItem("Designer").FindItem(Name);
      xi.SetProp("Sort", Grid.PropertySort.ToString());
    }

    /// <inheritdoc/>
    public override void RestoreState()
    {
      XmlItem xi = Config.Root.FindItem("Designer").FindItem(Name);
      Grid.PropertySort = xi.GetProp("Sort") == "Alphabetical" ? PropertySort.Alphabetical : PropertySort.CategorizedAlphabetical;
      btnCategory.Checked = Grid.PropertySort == PropertySort.CategorizedAlphabetical;
      btnAlphabetical.Checked = !btnCategory.Checked;
    }

    /// <inheritdoc/>
    public override void SelectionChanged()
    {
      if (FUpdating)
        return;
      // prevent fire SelectedValueChanged
      FUpdating = true;
      int selectedTab = Grid.SelectedTabIndex;
      try
      {
        if (Designer.SelectedObjects != null && Designer.SelectedObjects.Count == 1)
        {
          cbxObjects.SelectedIndex = cbxObjects.Items.IndexOf(Designer.SelectedObjects[0]);
          cbxObjects.Refresh();
          Grid.SelectedObjects = Designer.SelectedObjects.ToArray();
        }  
        else
        {
          cbxObjects.SelectedItem = null;
          if (Designer.SelectedObjects != null)
            Grid.SelectedObjects = Designer.SelectedObjects.ToArray();
          else  
            Grid.SelectedObjects = null;
          cbxObjects.Refresh();  
        }  
      }
      finally
      {
        FUpdating = false;
        if (selectedTab == 1)
        {
          // hack, prevent focus changing
          ContainerControl ctrl = Designer;
          while (ctrl.ActiveControl is ContainerControl)
          {
            ctrl = ctrl.ActiveControl as ContainerControl;
          }
          
          Grid.SelectedTabIndex = selectedTab;
          
          if (ctrl != null)
            ctrl.Focus();
        }  
      }  
    }

    /// <inheritdoc/>
    public override void UpdateContent()
    {
      cbxObjects.BeginUpdate();
      try
      {
        cbxObjects.Items.Clear();
        if (Designer.ActiveReport != null)
        {
          Report report = Designer.ActiveReport.Report;
          SortedList<string, Base> sl = new SortedList<string, Base>();
          sl.Add("", report);
          foreach (Base c in report.AllObjects)
          {
            if (!sl.ContainsKey(c.Name))
              sl.Add(c.Name, c);
          }
          foreach (Base c in sl.Values)
          {
            cbxObjects.Items.Add(c);
          }
        }  
        SelectionChanged();
      }
      finally
      {
        cbxObjects.EndUpdate();
      }
    }

    /// <inheritdoc/>
    public override void Lock()
    {
      base.Lock();
      Grid.SelectedObjects = null;
    }

    /// <inheritdoc/>
    public override void Localize()
    {
      Text = Res.Get("Designer,ToolWindow,Properties");
    }

    /// <inheritdoc/>
    public override void UpdateUIStyle()
    {
      base.UpdateUIStyle();
      FToolbar.Style = UIStyleUtils.GetDotNetBarStyle(Designer.UIStyle);
      cbxObjects.Style = FToolbar.Style;
      Color color = UIStyleUtils.GetControlColor(Designer.UIStyle);
      Grid.BackColor = color;
      Grid.LineColor = color;
      Grid.HelpBackColor = color;
      ParentControl.BackColor = color;
      cbxObjects.IsStandalone = Designer.UIStyle != UIStyle.VisualStudio2005 && Designer.UIStyle != UIStyle.Office2003;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    internal static extern int SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

    internal void TypeChar(char c)
    {
      Grid.Focus();
      SendMessage(Grid.ActiveControl.Handle, 0x0102, (int)c, IntPtr.Zero); 
    }
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertiesWindow"/> class with default settings.
    /// </summary>
    /// <param name="designer">The report designer.</param>
    public PropertiesWindow(Designer designer) : base(designer)
    {
      Name = "PropertiesWindow";
      Image = Res.GetImage(68);

      cbxObjects = new ComboBoxEx();
      cbxObjects.Dock = DockStyle.Top;
      cbxObjects.DisableInternalDrawing = true;
      cbxObjects.DropDownStyle = ComboBoxStyle.DropDownList;
      cbxObjects.DrawMode = DrawMode.OwnerDrawFixed;
      cbxObjects.ItemHeight = DrawUtils.DefaultItemHeight + 2;
      cbxObjects.DrawItem += new DrawItemEventHandler(cbxObjects_DrawItem);
      cbxObjects.SelectedValueChanged += new EventHandler(cbxObjects_SelectedValueChanged);

      FToolbar = new Bar();
      FToolbar.Dock = DockStyle.Top;
      FToolbar.Font = DrawUtils.DefaultFont;
      FToolbar.RoundCorners = false;
      
      btnCategory = new ButtonItem();
      btnCategory.AutoCheckOnClick = true;
      btnCategory.OptionGroup = "1";
      btnCategory.Image = Res.GetImage(69);
      btnCategory.Click += new EventHandler(btnCategory_Click);
      btnAlphabetical = new ButtonItem();
      btnAlphabetical.AutoCheckOnClick = true;
      btnAlphabetical.OptionGroup = "1";
      btnAlphabetical.Image = Res.GetImage(67);
      btnAlphabetical.Click += new EventHandler(btnCategory_Click);
      btnProperties = new ButtonItem();
      btnProperties.BeginGroup = true;
      btnProperties.AutoCheckOnClick = true;
      btnProperties.OptionGroup = "2";
      btnProperties.Checked = true;
      btnProperties.Image = Res.GetImage(78);
      btnProperties.Click += new EventHandler(btnProperties_Click);
      btnEvents = new ButtonItem();
      btnEvents.AutoCheckOnClick = true;
      btnEvents.OptionGroup = "2";
      btnEvents.Image = Res.GetImage(79);
      btnEvents.Click += new EventHandler(btnEvents_Click);
      
      FToolbar.Items.AddRange(new ButtonItem[] { btnCategory, btnAlphabetical, btnProperties, btnEvents });
      
      Grid = new FRPropertyGrid();
      Grid.Dock = DockStyle.Fill;
      Grid.ShowEvents = true;
      Grid.ToolbarVisible = false;
      Grid.PropertyValueChanged += new PropertyValueChangedEventHandler(grid_PropertyValueChanged);
      Shortcut = eShortcut.F4;

      ParentControl.VisibleChanged += new EventHandler(ParentControl_VisibleChanged);

      ParentControl.Controls.AddRange(new Control[] { Grid, FToolbar, cbxObjects });
      Localize();
    }
  }

}
