using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using FastReport.Editor;
using FastReport.Editor.Common;
using FastReport.Editor.Syntax;
using FastReport.Utils;
using FastReport.Forms;
using FastReport.Design.ToolWindows;
using FastReport.Data;
using FastReport.Code;
using System.Reflection;

namespace FastReport.Design.PageDesigners.Code
{
  internal class CodePageDesigner : PageDesignerBase
  {
    #region Fields
    private SyntaxEdit FEdit;
    private TextSource FSource;
    private bool FEditInitialized;
    private string FScript;
    #endregion

    #region Properties
    public SyntaxEdit Edit
    {
      get
      {
        if (!FEditInitialized)
          CreateEdit();
        return FEdit;
      }
    }

    public TextSource Source
    {
      get
      {
        if (!FEditInitialized)
          CreateEdit();
        return FSource;
      }
    }
    
    public string Script
    {
      get 
      {
        return FEditInitialized ? Edit.Source.Text : FScript; 
      }  
      set 
      { 
        FScript = value; 
        if (FEditInitialized)
          SetScriptText();
      }
    }
    #endregion

    #region Private Methods
    private void CreateEdit()
    {
      FEditInitialized = true;
      FSource = new TextSource();
      FEdit = new SyntaxEdit();
      FEdit.Dock = DockStyle.Fill;
      FEdit.BorderStyle = EditBorderStyle.None;
      FEdit.Source = FSource;
      FEdit.AllowDrop = true;
      FEdit.DragOver += new DragEventHandler(Edit_DragOver);
      FEdit.DragDrop += new DragEventHandler(Edit_DragDrop);
      Controls.Add(FEdit);

      // do this after controls.add!
      FEdit.IndentOptions = IndentOptions.AutoIndent | IndentOptions.SmartIndent;
      FEdit.NavigateOptions = NavigateOptions.BeyondEol;
      FEdit.Braces.BracesOptions = BracesOptions.Highlight | BracesOptions.HighlightBounds;
      FEdit.Braces.BackColor = Color.LightGray;
      FEdit.Braces.Style = FontStyle.Regular;
      FEdit.Scroll.ScrollBars = RichTextBoxScrollBars.Both;
      FEdit.Scroll.Options = ScrollingOptions.AllowSplitHorz | ScrollingOptions.AllowSplitVert;
      FEdit.Outlining.AllowOutlining = true;
      FEdit.DisplayLines.UseSpaces = true;
      FEdit.SplitHorz += new EventHandler(Edit_SplitHorz);
      FEdit.SplitVert += new EventHandler(Edit_SplitVert);
      FEdit.TextChanged += new EventHandler(Edit_TextChanged);

      UpdateOptions();
      UpdateEditColors();
      SetScriptText();
    }

    private void SetScriptText()
    {
      Edit.Source.Text = FScript;
    }

    private void UpdateEditColors()
    {
      FEdit.Gutter.BrushColor = UIStyleUtils.GetControlColor(Designer.UIStyle);
      FEdit.Gutter.PenColor = FEdit.Gutter.BrushColor;
    }

    private void Edit_SplitHorz(object sender, EventArgs e)
    {
      Edit.HorzSplitEdit.BorderStyle = Edit.BorderStyle;
    }

    private void Edit_SplitVert(object sender, EventArgs e)
    {
      Edit.VertSplitEdit.BorderStyle = Edit.BorderStyle;
    }

    private void Edit_DragOver(object sender, DragEventArgs e)
    {
      Point pt = Edit.PointToClient(new Point(e.X, e.Y));
      HitTestInfo hit = new HitTestInfo();
      Edit.GetHitTest(pt, ref hit);
      if (hit.Pos != -1 && hit.Line != -1)
        Edit.Position = new Point(hit.Pos, hit.Line);
      e.Effect = e.AllowedEffect;
    }

    private void Edit_DragDrop(object sender, DragEventArgs e)
    {
      DictionaryWindow.DraggedItem item = (DictionaryWindow.DraggedItem)e.Data.GetData(
        typeof(DictionaryWindow.DraggedItem));
      
      CodeHelperBase codeHelper = Designer.ActiveReport.Report.CodeHelper;
      string text = "";
      if (item.Obj is Column)
        text = codeHelper.ReplaceColumnName(item.Text, (item.Obj as Column).DataType);
      else if (item.Obj is SystemVariable)
        text = codeHelper.ReplaceVariableName(item.Obj as Parameter);
      else if (item.Obj is Parameter)
        text = codeHelper.ReplaceParameterName(item.Obj as Parameter);
      else if (item.Obj is Total)
        text = codeHelper.ReplaceTotalName(item.Text);
      else if (item.Obj is MethodInfo)
        text = item.Text;
      else
        text = "Report.Calc(\"" + item.Text + "\")";
      
      Edit.Selection.InsertString(text);
      Edit.Focus();
    }

    private void Edit_TextChanged(object sender, EventArgs e)
    {
      IDesignerPlugin stdToolbar = Designer.Plugins.Find("StandardToolbar");
      if (stdToolbar != null)
        stdToolbar.UpdateContent();
      IDesignerPlugin menu = Designer.Plugins.Find("MainMenu");
      if (menu != null)
        menu.UpdateContent();
    }
    
    private void CommitChanges()
    {
      if (Edit.Modified)
      {
        Edit.Modified = false;
        Designer.SetModified(this, "Script");
      }
    }
    #endregion
    
    #region Public Methods
    public void UpdateLanguage()
    {
      SyntaxParser parser = Designer.ActiveReport.Report.CodeHelper.Parser;
      Edit.Lexer = parser;
      Source.Lexer = parser;
      Source.FormatText();
      Designer.ActiveReport.Report.CodeHelper.RegisterAssemblies();
      Edit.Refresh();
    }

    public void UpdateOptions()
    {
      Edit.NavigateOptions = CodePageSettings.EnableVirtualSpace ? NavigateOptions.BeyondEol : NavigateOptions.None;
      Edit.DisplayLines.UseSpaces = CodePageSettings.UseSpaces;
      Edit.Outlining.AllowOutlining = CodePageSettings.AllowOutlining;
      Edit.DisplayLines.TabStops = new int[] { CodePageSettings.TabSize };
    }

    public void Copy()
    {
      Edit.Selection.Copy();
    }
    
    public void Cut()
    {
      Edit.Selection.Cut();
    }
    
    public void Paste()
    {
      Edit.Selection.Paste();
    }

    public bool CanUndo()
    {
      return Source.CanUndo();
    }

    public bool CanRedo()
    {
      return Source.CanRedo();
    }

    public override void FillObjects(bool resetSelection)
    {
      // do nothing
    }

    public override void PageActivated()
    {
      base.PageActivated();
      UpdateOptions();
      UpdateLanguage();
    }

    public override void PageDeactivated()
    {
      base.PageDeactivated();
      if (FEditInitialized)
        CommitChanges();
    }
    #endregion

    #region IDesignerPlugin
    public override void SaveState()
    {
      CodePageSettings.SaveState();
    }

    public override void RestoreState()
    {
    }

    public override DesignerOptionsPage GetOptionsPage()
    {
      return new CodePageOptions(this);
    }

    public override void UpdateUIStyle()
    {
      base.UpdateUIStyle();
      if (FEditInitialized)
        UpdateEditColors();
    }
    #endregion

    public CodePageDesigner(Designer designer) : base(designer)
    {
      Name = "Code";
    }
  }

}
