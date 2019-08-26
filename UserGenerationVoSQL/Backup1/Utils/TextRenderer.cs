using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Globalization;
using System.Drawing.Text;

namespace FastReport.Utils
{
  /// <summary>
  /// Advanced text renderer is used to perform the following tasks:
  /// - draw justified text, text with custom line height, text containing html tags;
  /// - calculate text height, get part of text that does not fit in the display rectangle;
  /// - get paragraphs, lines, words and char sequence to perform accurate export to such
  /// formats as PDF, TXT, RTF
  /// </summary>
  /// <example>Here is how one may operate the renderer items:
  /// <code>
  /// foreach (AdvancedTextRenderer.Paragraph paragraph in renderer.Paragraphs)
  /// {
  ///   foreach (AdvancedTextRenderer.Line line in paragraph.Lines)
  ///   {
  ///     foreach (AdvancedTextRenderer.Word word in line.Words)
  ///     {
  ///       if (renderer.HtmlTags)
  ///       {
  ///         foreach (AdvancedTextRenderer.Run run in word.Runs)
  ///         {
  ///           using (Font f = run.GetFont())
  ///           using (Brush b = run.GetBrush())
  ///           {
  ///             g.DrawString(run.Text, f, b, run.Left, run.Top, renderer.Format);
  ///           }  
  ///         }
  ///       }
  ///       else
  ///       {
  ///         g.DrawString(word.Text, renderer.Font, renderer.Brush, word.Left, word.Top, renderer.Format);
  ///       }
  ///     }
  ///   }
  /// }
  /// </code>
  /// </example>
  internal class AdvancedTextRenderer
  {
    #region Fields
    private List<Paragraph> FParagraphs;
    private string FText;
    private Graphics FGraphics;
    private Font FFont;
    private Brush FBrush;
    private RectangleF FDisplayRect;
    private StringFormat FFormat;
    private HorzAlign FHorzAlign;
    private VertAlign FVertAlign;
    private float FLineHeight;
    private float FFontLineHeight;
    private int FAngle;
    private float FWidthRatio;
    private bool FForceJustify;
    private bool FWysiwyg;
    private bool FHtmlTags;
    private bool FPDFMode;
    private float FSpaceWidth;
    #endregion

    #region Properties
    public List<Paragraph> Paragraphs
    {
      get { return FParagraphs; }
    }

    public Graphics Graphics
    {
      get { return FGraphics; }
    }

    public Font Font
    {
      get { return FFont; }
    }

    public Brush Brush
    {
      get { return FBrush; }
    }

    public Color BrushColor
    {
      get { return FBrush is SolidBrush ? (FBrush as SolidBrush).Color : Color.Black; }
    }

    public RectangleF DisplayRect
    {
      get { return FDisplayRect; }
    }

    public StringFormat Format
    {
      get { return FFormat; }
    }

    public HorzAlign HorzAlign
    {
      get { return FHorzAlign; }
    }

    public VertAlign VertAlign
    {
      get { return FVertAlign; }
    }

    public float LineHeight
    {
      get { return FLineHeight; }
    }

    public float FontLineHeight
    {
      get { return FFontLineHeight; }
    }

    public int Angle
    {
      get { return FAngle; }
    }

    public float WidthRatio
    {
      get { return FWidthRatio; }
    }

    public bool ForceJustify
    {
      get { return FForceJustify; }
    }

    public bool Wysiwyg
    {
      get { return FWysiwyg; }
    }

    public bool HtmlTags
    {
      get { return FHtmlTags; }
    }

    public float TabSize
    {
      get
      {
        float firstTab = 0;
        float[] tabSizes = Format.GetTabStops(out firstTab);
        if (tabSizes.Length > 0)
          return tabSizes[0];
        return 0;
      }
    }

    public float TabOffset
    {
      get
      {
        float firstTab = 0;
        float[] tabSizes = Format.GetTabStops(out firstTab);
        return firstTab;
      }
    }

    public bool WordWrap
    {
      get { return (Format.FormatFlags & StringFormatFlags.NoWrap) == 0; }
    }

    public bool RightToLeft
    {
      get { return (Format.FormatFlags & StringFormatFlags.DirectionRightToLeft) != 0; }
    }
    
    public bool PDFMode
    {
      get { return FPDFMode; }
    }
    
    internal float SpaceWidth
    {
      get { return FSpaceWidth; }
    }  
    #endregion

    #region Private Methods
    private void SplitToParagraphs(string text)
    {
      StyleDescriptor style = new StyleDescriptor(Font.Style, BrushColor, BaseLine.Normal);
      string[] lines = text.Split(new char[] { '\n' });
      int originalCharIndex = 0;

      foreach (string line in lines)
      {
        string s = line;
        if (s.Length > 0 && s[s.Length - 1] == '\r')
          s = s.Remove(s.Length - 1);

        Paragraph paragraph = new Paragraph(s, this, originalCharIndex);
        FParagraphs.Add(paragraph);
        if (HtmlTags)
          style = paragraph.WrapHtmlLines(style);
        else
          paragraph.WrapLines();

        originalCharIndex += line.Length + 1;
      }

      // skip empty paragraphs at the end
      for (int i = FParagraphs.Count - 1; i >= 0; i--)
      {
        if (FParagraphs[i].IsEmpty)
          FParagraphs.RemoveAt(i);
        else
          break;
      }
    }

    private void AdjustParagraphLines()
    {
      // calculate text height
      float height = 0;
      foreach (Paragraph paragraph in Paragraphs)
      {
        height += paragraph.Lines.Count * LineHeight;
      }

      // calculate Y offset
      float offsetY = DisplayRect.Top;
      if (VertAlign == VertAlign.Center)
        offsetY += (DisplayRect.Height - height) / 2;
      else if (VertAlign == VertAlign.Bottom)
        offsetY += (DisplayRect.Height - height) - 1;

      for (int i = 0; i < Paragraphs.Count; i++)
      {
        Paragraph paragraph = Paragraphs[i];
        paragraph.AlignLines(i == Paragraphs.Count - 1 && ForceJustify);

        // adjust line tops
        foreach (Line line in paragraph.Lines)
        {
          line.Top = offsetY;
          line.MakeUnderlines();
          offsetY += LineHeight;
        }
      }
    }
    #endregion

    #region Public Methods
    public void Draw()
    {
      // set clipping
      GraphicsState state = Graphics.Save();
      Graphics.SetClip(DisplayRect, CombineMode.Intersect);
      
      // reset alignment
      StringAlignment saveAlign = Format.Alignment;
      StringAlignment saveLineAlign = Format.LineAlignment;
      Format.Alignment = StringAlignment.Near;
      Format.LineAlignment = StringAlignment.Near;

      if (Angle != 0)
      {
        Graphics.TranslateTransform(DisplayRect.Left + DisplayRect.Width / 2,
          DisplayRect.Top + DisplayRect.Height / 2);
        Graphics.RotateTransform(Angle);
      }

      Graphics.ScaleTransform(WidthRatio, 1);

      foreach (Paragraph paragraph in Paragraphs)
      {
        paragraph.Draw();
      }

      // restore alignment and clipping
      Format.Alignment = saveAlign;
      Format.LineAlignment = saveLineAlign;
      Graphics.Restore(state);
    }

    public float CalcHeight()
    {
      int charsFit = 0;
      return CalcHeight(out charsFit);
    }

    public float CalcHeight(out int charsFit)
    {
      StyleDescriptor style = null;
      return CalcHeight(out charsFit, out style);
    }

    public float CalcHeight(out int charsFit, out StyleDescriptor style)
    {
      charsFit = 0;
      style = null;
      float height = 0;
      float displayHeight = DisplayRect.Height;

      foreach (Paragraph paragraph in Paragraphs)
      {
        foreach (Line line in paragraph.Lines)
        {
          height += LineHeight;
          if (charsFit == 0 && height > displayHeight)
          {
            charsFit = line.OriginalCharIndex;
            if (HtmlTags)
              style = line.Style;
          }
        }
      }

      if (charsFit == 0)
        charsFit = FText.Length;
      return height;
    }

    public float CalcWidth()
    {
      float width = 0;

      foreach (Paragraph paragraph in Paragraphs)
      {
        foreach (Line line in paragraph.Lines)
        {
          if (width < line.Width)
            width = line.Width;
        }
      }
      return width + FSpaceWidth;
    }

    internal float GetTabPosition(float pos)
    {
      int tabPosition = (int)((pos - TabOffset) / TabSize);
      return (tabPosition + 1) * TabSize + TabOffset;
    }
    #endregion

    public AdvancedTextRenderer(string text, Graphics g, Font font, Brush brush,
      RectangleF rect, StringFormat format, HorzAlign horzAlign, VertAlign vertAlign,
      float lineHeight, int angle, float widthRatio,
      bool forceJustify, bool wysiwyg, bool htmlTags, bool pdfMode)
    {
      FParagraphs = new List<Paragraph>();
      FText = text;
      FGraphics = g;
      FFont = font;
      FBrush = brush;
      FDisplayRect = rect;
      FFormat = format;
      FHorzAlign = horzAlign;
      FVertAlign = vertAlign;
      FLineHeight = lineHeight;
      FFontLineHeight = font.GetHeight(g);
      if (FLineHeight == 0)
        FLineHeight = FFontLineHeight;
      FAngle = angle % 360;
      FWidthRatio = widthRatio;
      FForceJustify = forceJustify;
      FWysiwyg = wysiwyg;
      FHtmlTags = htmlTags;
      FPDFMode = pdfMode;
      FSpaceWidth = g.MeasureString(" ", font).Width;

      StringFormatFlags saveFlags = Format.FormatFlags;
      StringTrimming saveTrimming = Format.Trimming;
      
      // match DrawString behavior: 
      // if height is less than 1.25 of font height, turn off word wrap
      if (rect.Height < FFontLineHeight * 1.25f)
        FFormat.FormatFlags |= StringFormatFlags.NoWrap;

      // if word wrap is set, ignore trimming
      if (WordWrap)
        Format.Trimming = StringTrimming.Word;

      if (Angle != 0)
      {
        // shift displayrect 
        FDisplayRect.X = -DisplayRect.Width / 2;
        FDisplayRect.Y = -DisplayRect.Height / 2;

        // rotate displayrect if angle is 90 or 270
        if ((Angle >= 90 && Angle < 180) || (Angle >= 270 && Angle < 360))
          FDisplayRect = new RectangleF(DisplayRect.Y, DisplayRect.X, DisplayRect.Height, DisplayRect.Width);
      }

      FDisplayRect.X /= WidthRatio;
      FDisplayRect.Width /= WidthRatio;

      SplitToParagraphs(text);
      AdjustParagraphLines();

      // restore original values
      FDisplayRect = rect;
      Format.FormatFlags = saveFlags;
      Format.Trimming = saveTrimming;
    }


    /// <summary>
    /// Paragraph represents single paragraph. It consists of one or several <see cref="Lines"/>.
    /// </summary>
    internal class Paragraph
    {
      #region Fields
      private List<Line> FLines;
      private AdvancedTextRenderer FRenderer;
      private string FText;
      private int FOriginalCharIndex;
      #endregion

      #region Properties
      public List<Line> Lines
      {
        get { return FLines; }
      }

      public AdvancedTextRenderer Renderer
      {
        get { return FRenderer; }
      }
      
      public bool Last
      {
        get { return FRenderer.Paragraphs[FRenderer.Paragraphs.Count - 1] == this; }
      }

      public bool IsEmpty
      {
        get { return String.IsNullOrEmpty(FText); }
      }
      #endregion

      #region Private Methods
      private int MeasureString(string text)
      {
        int charsFit = 0;
        int linesFit = 0;
        Renderer.Graphics.MeasureString(text, Renderer.Font,
          new SizeF(Renderer.DisplayRect.Width, Renderer.FontLineHeight + 1),
          Renderer.Format, out charsFit, out linesFit);
        return charsFit;
      }
      #endregion

      #region Public Methods
      public void WrapLines()
      {
        string text = FText;
        int charsFit = 0;

        if (String.IsNullOrEmpty(text))
        {
          FLines.Add(new Line("", this, FOriginalCharIndex));
          return;
        }

        if (Renderer.WordWrap)
        {
          int originalCharIndex = FOriginalCharIndex;
          while (text.Length > 0)
          {
            charsFit = MeasureString(text);
            string textFit = text.Substring(0, charsFit).TrimEnd(new char[] { ' ' });
            FLines.Add(new Line(textFit, this, originalCharIndex));
            text = text.Substring(charsFit);
            originalCharIndex += charsFit;
          }
        }
        else
        {
          string ellipsis = "…";
          StringTrimming trimming = Renderer.Format.Trimming;
          if (trimming == StringTrimming.EllipsisPath)
            Renderer.Format.Trimming = StringTrimming.Character;
          charsFit = MeasureString(text);

          switch (trimming)
          {
            case StringTrimming.Character:
            case StringTrimming.Word:
              text = text.Substring(0, charsFit);
              break;

            case StringTrimming.EllipsisCharacter:
            case StringTrimming.EllipsisWord:
              if (charsFit < text.Length)
              {
                text = text.Substring(0, charsFit);
                if (text.EndsWith(" "))
                  text = text.Substring(0, text.Length - 1);
                text += ellipsis;
              }
              break;

            case StringTrimming.EllipsisPath:
              if (charsFit < text.Length)
              {
                while (text.Length > 3)
                {
                  int mid = text.Length / 2;
                  string newText = text.Substring(0, mid) + ellipsis + text.Substring(mid + 1);
                  if (MeasureString(newText) == newText.Length)
                  {
                    text = newText;
                    break;
                  }
                  else
                  {
                    text = text.Remove(mid, 1);
                  }
                }
              }
              break;
          }

          FLines.Add(new Line(text, this, FOriginalCharIndex));
        }
      }

      public StyleDescriptor WrapHtmlLines(StyleDescriptor style)
      {
        Line line = new Line("", this, FOriginalCharIndex);
        FLines.Add(line);
        Word word = new Word("", line);
        line.Words.Add(word);

        string text = FText;
        string currentWord = "";
        float width = 0;
        bool skipSpace = true;
        int originalCharIndex = FOriginalCharIndex;

        for (int i = 0; i < text.Length; i++)
        {
          if (text[i] == '<')
          {
            // probably html tag
            StyleDescriptor newStyle = new StyleDescriptor(style.FontStyle, style.Color, style.BaseLine);
            string tag = "";
            bool match = true;

            // <b>, <i>, <u>
            if (i + 3 <= text.Length)
            {
              match = true;
              tag = text.Substring(i, 3).ToLower();
              if (tag == "<b>")
                newStyle.FontStyle |= FontStyle.Bold;
              else if (tag == "<i>")
                newStyle.FontStyle |= FontStyle.Italic;
              else if (tag == "<u>")
                newStyle.FontStyle |= FontStyle.Underline;
              else
                match = false;

              if (match)
                i += 3;
            }

            // </b>, </i>, </u>
            if (!match && i + 4 <= text.Length && text[i + 1] == '/')
            {
              match = true;
              tag = text.Substring(i, 4).ToLower();
              if (tag == "</b>")
                newStyle.FontStyle &= ~FontStyle.Bold;
              else if (tag == "</i>")
                newStyle.FontStyle &= ~FontStyle.Italic;
              else if (tag == "</u>")
                newStyle.FontStyle &= ~FontStyle.Underline;
              else
                match = false;

              if (match)
                i += 4;
            }

            // <sub>, <sup>
            if (!match && i + 5 <= text.Length)
            {
              match = true;
              tag = text.Substring(i, 5).ToLower();
              if (tag == "<sub>")
                newStyle.BaseLine = BaseLine.Subscript;
              else if (tag == "<sup>")
                newStyle.BaseLine = BaseLine.Superscript;
              else
                match = false;

              if (match)
                i += 5;
            }

            // </sub>, </sup>
            if (!match && i + 6 <= text.Length && text[i + 1] == '/')
            {
              match = true;
              tag = text.Substring(i, 6).ToLower();
              if (tag == "</sub>")
                newStyle.BaseLine = BaseLine.Normal;
              else if (tag == "</sup>")
                newStyle.BaseLine = BaseLine.Normal;
              else
                match = false;

              if (match)
                i += 6;
            }

            // <strike>
            if (!match && i + 8 <= text.Length && text.Substring(i, 8).ToLower() == "<strike>")
            {
              newStyle.FontStyle |= FontStyle.Strikeout;
              match = true;
              i += 8;
            }

            // </strike>
            if (!match && i + 9 <= text.Length && text.Substring(i, 9).ToLower() == "</strike>")
            {
              newStyle.FontStyle &= ~FontStyle.Strikeout;
              match = true;
              i += 9;
            }

            // <font color
            if (!match && i + 12 < text.Length && text.Substring(i, 12).ToLower() == "<font color=")
            {
              int start = i + 12;
              int end = start;
              for (; end < text.Length && text[end] != '>'; end++)
              {
              }

              if (end < text.Length)
              {
                string colorName = text.Substring(start, end - start);
                if (colorName.StartsWith("#"))
                {
                  newStyle.Color = Color.FromArgb((int)(0xFF000000 + uint.Parse(colorName.Substring(1), NumberStyles.HexNumber)));
                }
                else
                {
                  newStyle.Color = Color.FromName(colorName);
                }
                i = end + 1;
                match = true;
              }
            }

            // </font>
            if (!match && i + 7 <= text.Length && text.Substring(i, 7).ToLower() == "</font>")
            {
              newStyle.Color = Renderer.BrushColor;
              match = true;
              i += 7;
            }

            if (match)
            {
              if (currentWord != "")
              {
                // finish the word
                word.Runs.Add(new Run(currentWord, style, word));
              }

              currentWord = "";
              style = newStyle;
              i--;
              
              if (i >= text.Length - 1)
              {
                // check width
                width += word.Width + Renderer.SpaceWidth;
                if (width > Renderer.DisplayRect.Width)
                {
                  // line is too long, make a new line
                  if (line.Words.Count > 1)
                  {
                    // if line has several words, delete the last word from the current line
                    line.Words.RemoveAt(line.Words.Count - 1);
                    // make new line
                    line = new Line("", this, originalCharIndex);
                    // and add word to it
                    line.Words.Add(word);
                    word.SetLine(line);
                    FLines.Add(line);
                  }
                }
              }
              continue;
            }
          }
          if (text[i] == ' ' || text[i] == '\t' || i == text.Length - 1)
          {
            // finish the last word
            bool isLastWord = i == text.Length - 1;
            if (isLastWord)
            {
              currentWord += text[i];
              skipSpace = false;
            }

            if (text[i] == '\t')
              skipSpace = false;

            // space
            if (skipSpace)
            {
              currentWord += text[i];
            }
            else
            {
              // finish the word
              if (currentWord != "")
                word.Runs.Add(new Run(currentWord, style, word));

              // check width
              width += word.Width + Renderer.SpaceWidth;
              if (width > Renderer.DisplayRect.Width)
              {
                // line is too long, make a new line
                width = 0;
                if (line.Words.Count > 1)
                {
                  // if line has several words, delete the last word from the current line
                  line.Words.RemoveAt(line.Words.Count - 1);
                  // make new line
                  line = new Line("", this, originalCharIndex);
                  // and add word to it
                  line.Words.Add(word);
                  word.SetLine(line);
                  width += word.Width + Renderer.SpaceWidth;
                }
                else
                {
                  line = new Line("", this, i + 1);
                }
                FLines.Add(line);
              }

              // TAB symbol
              if (text[i] == '\t')
              {
                word = new Word("\t", line);
                line.Words.Add(word);
                // adjust width
                width = Renderer.GetTabPosition(width);
              }

              if (!isLastWord)
              {
                word = new Word("", line);
                line.Words.Add(word);
                currentWord = "";
                originalCharIndex = FOriginalCharIndex + i + 1;
                skipSpace = true;
              }
            }
          }
          else
          {
            // symbol
            currentWord += text[i];
            skipSpace = false;
          }
        }

        return style;
      }

      public void AlignLines(bool forceJustify)
      {
        for (int i = 0; i < Lines.Count; i++)
        {
          HorzAlign align = Renderer.HorzAlign;
          if (align == HorzAlign.Justify && i == Lines.Count - 1 && !forceJustify)
            align = HorzAlign.Left;
          Lines[i].AlignWords(align);
        }
      }

      public void Draw()
      {
        foreach (Line line in Lines)
        {
          line.Draw();
        }
      }
      #endregion

      public Paragraph(string text, AdvancedTextRenderer renderer, int originalCharIndex)
      {
        FLines = new List<Line>();
        FText = text;
        FRenderer = renderer;
        FOriginalCharIndex = originalCharIndex;
      }
    }


    /// <summary>
    /// Line represents single text line. It consists of one or several <see cref="Words"/>.
    /// Simple line (that does not contain tabs, html tags, and is not justified) has
    /// single <see cref="Word"/> which contains all the text.
    /// </summary>
    internal class Line
    {
      #region Fields
      private List<Word> FWords;
      private string FText;
      private bool FHasTabs;
      private Paragraph FParagraph;
      private float FTop;
      private float FWidth;
      private int FOriginalCharIndex;
      private List<RectangleF> FUnderlines;
      private List<RectangleF> FStrikeouts;
      #endregion

      #region Properties
      public List<Word> Words
      {
        get { return FWords; }
      }

      public string Text
      {
        get { return FText; }
      }

      public bool HasTabs
      {
        get { return FHasTabs; }
      }

      public float Left
      {
        get { return Words.Count > 0 ? Words[0].Left : 0; }
      }
      
      public float Top
      {
        get { return FTop; }
        set { FTop = value; }
      }

      public float Width
      {
        get { return FWidth; }
      }

      public int OriginalCharIndex
      {
        get { return FOriginalCharIndex; }
      }

      public AdvancedTextRenderer Renderer
      {
        get { return FParagraph.Renderer; }
      }

      public StyleDescriptor Style
      {
        get
        {
          if (Words.Count > 0)
            if (Words[0].Runs.Count > 0)
              return Words[0].Runs[0].Style;
          return null;
        }
      }
      
      public bool Last
      {
        get { return FParagraph.Lines[FParagraph.Lines.Count - 1] == this; }
      }
      
      public List<RectangleF> Underlines
      {
        get { return FUnderlines; }
      }

      public List<RectangleF> Strikeouts
      {
        get { return FStrikeouts; }
      }
      #endregion
      
      #region Private Methods
      private void PrepareUnderlines(List<RectangleF> list, FontStyle style)
      {
        list.Clear();
        if (Words.Count == 0)
          return;

        if (Renderer.HtmlTags)
        {
          float left = 0;
          float right = 0;
          bool styleOn = false;

          foreach (Word word in Words)
          {
            foreach (Run run in word.Runs)
            {
              using (Font fnt = run.GetFont())
              {
                if ((fnt.Style & style) > 0)
                {
                  if (!styleOn)
                  {
                    styleOn = true;
                    left = run.Left;
                  }
                  right = run.Left + run.Width;
                }
                if ((fnt.Style & style) == 0 && styleOn)
                {
                  styleOn = false;
                  list.Add(new RectangleF(left, Top, right - left, 1));
                }
              }
            }
          }
          // close the style
          if (styleOn)
            list.Add(new RectangleF(left, Top, right - left, 1));
        }
        else if ((Renderer.Font.Style & style) > 0)
        {
          float lineWidth = Width;
          if (Renderer.HorzAlign == HorzAlign.Justify && (!Last || (FParagraph.Last && Renderer.ForceJustify)))
            lineWidth = Renderer.DisplayRect.Width - Renderer.SpaceWidth;

          list.Add(new RectangleF(Left, Top, lineWidth, 1));
        }
      }
      #endregion

      #region Public Methods
      public void AlignWords(HorzAlign align)
      {
        FWidth = 0;

        // handle each word
        if (align == HorzAlign.Justify || HasTabs || Renderer.Wysiwyg || Renderer.HtmlTags)
        {
          float left = 0;
          for (int i = 0; i < Words.Count; i++)
          {
            Word word = Words[i];
            word.Left = left;

            if (word.Text == "\t")
            {
              left = Renderer.GetTabPosition(left);
              // remove tab
              Words.RemoveAt(i);
              i--;
            }
            else
              left += word.Width + Renderer.SpaceWidth;
          }

          FWidth = left - Renderer.SpaceWidth;
        }
        else
        {
          // join all words into one
          Words.Clear();
          Words.Add(new Word(FText, this));
          FWidth = Words[0].Width;
        }

        float rectWidth = Renderer.DisplayRect.Width;
        if (align == HorzAlign.Justify)
        {
          float delta = (rectWidth - FWidth - Renderer.SpaceWidth) / (Words.Count - 1);
          float curDelta = delta;
          for (int i = 1; i < Words.Count; i++)
          {
            FWords[i].Left += curDelta;
            curDelta += delta;
          }
        }
        else
        {
          float delta = 0;
          if (align == HorzAlign.Center)
            delta = (rectWidth - FWidth) / 2;
          else if (align == HorzAlign.Right)
            delta = rectWidth - FWidth - Renderer.SpaceWidth;
          for (int i = 0; i < Words.Count; i++)
          {
            FWords[i].Left += delta;
          }
        }

        // adjust X offset
        foreach (Word word in Words)
        {
          if (Renderer.RightToLeft)
            word.Left = Renderer.DisplayRect.Right - word.Left;
          else
            word.Left += Renderer.DisplayRect.Left;
          word.AdjustRuns();
          if (Renderer.RightToLeft && Renderer.PDFMode)
            word.Left -= word.Width;
        }
      }

      public void MakeUnderlines()
      {
        PrepareUnderlines(FUnderlines, FontStyle.Underline);
        PrepareUnderlines(FStrikeouts, FontStyle.Strikeout);
      }

      public void Draw()
      {
        foreach (Word word in Words)
        {
          word.Draw();
        }
      }
      #endregion

      public Line(string text, Paragraph paragraph, int originalCharIndex)
      {
        FWords = new List<Word>();
        FText = text;
        FParagraph = paragraph;
        FOriginalCharIndex = originalCharIndex;
        FUnderlines = new List<RectangleF>();
        FStrikeouts = new List<RectangleF>();
        FHasTabs = text.Contains("\t");

        // split text by spaces
        string[] words = text.Split(new char[] { ' ' });
        string textWithSpaces = "";

        foreach (string word in words)
        {
          if (word == "")
            textWithSpaces += " ";
          else
          {
            // split text by tabs
            textWithSpaces += word;
            string[] tabWords = textWithSpaces.Split(new char[] { '\t' });

            foreach (string word1 in tabWords)
            {
              if (word1 == "")
                FWords.Add(new Word("\t", this));
              else
              {
                FWords.Add(new Word(word1, this));
                FWords.Add(new Word("\t", this));
              }
            }

            // remove last tab
            FWords.RemoveAt(FWords.Count - 1);

            textWithSpaces = "";
          }
        }
      }
    }


    /// <summary>
    /// Word represents single word. It may consist of one or several <see cref="Runs"/>, in case
    /// when HtmlTags are enabled in the main <see cref="AdvancedTextRenderer"/> class.
    /// </summary>
    internal class Word
    {
      #region Fields
      private List<Run> FRuns;
      private string FText;
      private float FLeft;
      private float FWidth;
      private Line FLine;
      #endregion

      #region Properties
      public string Text
      {
        get { return FText; }
      }

      public float Left
      {
        get { return FLeft; }
        set { FLeft = value; }
      }

      public float Width
      {
        get
        {
          if (FWidth == -1)
          {
            if (Renderer.HtmlTags)
            {
              FWidth = 0;
              foreach (Run run in Runs)
              {
                FWidth += run.Width;
              }
            }
            else
            {
              FWidth = Renderer.Graphics.MeasureString(FText, Renderer.Font, 10000, StringFormat.GenericTypographic).Width;
            }
          }
          return FWidth;
        }
      }

      public float Top
      {
        get { return FLine.Top; }
      }

      public AdvancedTextRenderer Renderer
      {
        get { return FLine.Renderer; }
      }

      public List<Run> Runs
      {
        get { return FRuns; }
      }
      #endregion

      #region Public Methods
      public void AdjustRuns()
      {
        float left = Left;
        foreach (Run run in Runs)
        {
          run.Left = left;

          if (Renderer.RightToLeft)
          {
            left -= run.Width;
            if (Renderer.PDFMode)
              run.Left -= run.Width;
          }  
          else
            left += run.Width;
        }
      }

      public void SetLine(Line line)
      {
        FLine = line;
      }

      public void Draw()
      {
        if (Renderer.HtmlTags)
        {
          foreach (Run run in Runs)
          {
            run.Draw();
          }
        }
        else
        {
          Renderer.Graphics.DrawString(Text, Renderer.Font, Renderer.Brush, Left, Top, Renderer.Format);
        }
      }
      #endregion

      public Word(string text, Line line)
      {
        FText = text;
        FRuns = new List<Run>();
        FLine = line;
        FWidth = -1;
      }
    }


    /// <summary>
    /// Represents character placement.
    /// </summary>
    internal enum BaseLine
    {
      Normal,
      Subscript,
      Superscript
    }


    /// <summary>
    /// Represents a style used in HtmlTags mode.
    /// </summary>
    internal class StyleDescriptor
    {
      #region Fields
      private FontStyle FFontStyle;
      private Color FColor;
      private BaseLine FBaseLine;
      #endregion

      #region Properties
      public FontStyle FontStyle
      {
        get { return FFontStyle; }
        set { FFontStyle = value; }
      }

      public Color Color
      {
        get { return FColor; }
        set { FColor = value; }
      }

      public BaseLine BaseLine
      {
        get { return FBaseLine; }
        set { FBaseLine = value; }
      }
      #endregion

      #region Public Methods
      public override string ToString()
      {
        string result = "";

        if ((FontStyle & FontStyle.Bold) != 0)
          result += "<b>";
        if ((FontStyle & FontStyle.Italic) != 0)
          result += "<i>";
        if ((FontStyle & FontStyle.Underline) != 0)
          result += "<u>";
        if ((FontStyle & FontStyle.Strikeout) != 0)
          result += "<strike>";
        if (BaseLine == BaseLine.Subscript)
          result += "<sub>";
        if (BaseLine == BaseLine.Superscript)
          result += "<sup>";

        result += "<font color=\"";
        if (Color.IsKnownColor)
          result += Color.Name;
        else
          result += "#" + Color.ToArgb().ToString("x");
        result += "\">";

        return result;
      }
      #endregion

      public StyleDescriptor(FontStyle fontStyle, Color color, BaseLine baseLine)
      {
        FFontStyle = fontStyle;
        FColor = color;
        FBaseLine = baseLine;
      }
    }


    /// <summary>
    /// Represents sequence of characters that have the same <see cref="Style"/>.
    /// </summary>
    internal class Run
    {
      #region Fields
      private string FText;
      private StyleDescriptor FStyle;
      private Word FWord;
      private float FLeft;
      private float FWidth;
      #endregion

      #region Properties
      public string Text
      {
        get { return FText; }
      }

      public StyleDescriptor Style
      {
        get { return FStyle; }
      }

      public AdvancedTextRenderer Renderer
      {
        get { return FWord.Renderer; }
      }

      public float Left
      {
        get { return FLeft; }
        set { FLeft = value; }
      }

      public float Top
      {
        get
        {
          float baseLine = 0;
          if (Style.BaseLine == BaseLine.Subscript)
            baseLine += Renderer.FontLineHeight * 0.45f;
          else if (Style.BaseLine == BaseLine.Superscript)
            baseLine -= Renderer.FontLineHeight * 0.15f;
          return FWord.Top + baseLine;
        }
      }

      public float Width
      {
        get { return FWidth; }
      }
      #endregion

      #region Public Methods
      public Font GetFont()
      {
        float fontSize = Renderer.Font.Size;
        if (Style.BaseLine != BaseLine.Normal)
          fontSize *= 0.6f;
        return new Font(Renderer.Font.Name, fontSize, Style.FontStyle);
      }
      
      public Brush GetBrush()
      {
        return new SolidBrush(Style.Color);
      }

      public void Draw()
      {
        using (Font font = GetFont())
        using (Brush brush = GetBrush())
        {
          Renderer.Graphics.DrawString(FText, font, brush, Left, Top, Renderer.Format);
        }
      }
      #endregion

      public Run(string text, StyleDescriptor style, Word word)
      {
        FText = text;
        FStyle = new StyleDescriptor(style.FontStyle, style.Color, style.BaseLine);
        FWord = word;

        using (Font font = GetFont())
        {
          FWidth = Renderer.Graphics.MeasureString(text, font, 10000, StringFormat.GenericTypographic).Width;
        }  
      }
    }
  }


  /// <summary>
  /// Standard text renderer uses standard DrawString method to draw text. It also supports:
  /// - text rotation;
  /// - fonts with non-standard width ratio.
  /// In case your text is justified, or contains html tags, use the <see cref="AdvancedTextRenderer"/>
  /// class instead.
  /// </summary>
  internal class StandardTextRenderer
  {
    public static void Draw(string text, Graphics g, Font font, Brush brush,
      RectangleF rect, StringFormat format, int angle, float widthRatio)
    {
      GraphicsState state = g.Save();
      g.SetClip(rect, CombineMode.Intersect);
      g.TranslateTransform(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
      g.RotateTransform(angle);
      rect.X = -rect.Width / 2;
      rect.Y = -rect.Height / 2;

      if ((angle >= 90 && angle < 180) || (angle >= 270 && angle < 360))
        rect = new RectangleF(rect.Y, rect.X, rect.Height, rect.Width);

      g.ScaleTransform(widthRatio, 1);
      rect.X /= widthRatio;
      rect.Width /= widthRatio;

      g.DrawString(text, font, brush, rect, format);

      g.Restore(state);
    }
  }
}
