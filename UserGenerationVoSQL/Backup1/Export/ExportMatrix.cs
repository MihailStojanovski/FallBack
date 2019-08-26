using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using FastReport;
using FastReport.Utils;
using FastReport.Format;
using FastReport.Table;
using System.Threading;

namespace FastReport.Export
{

    internal class ExportMatrix
    {
        #region Private fields

        private Font DefaultOneSizeFont;

        private List<ExportIEMObjectList> FIEMObjectList;
        private List<ExportIEMStyle> FIEMStyleList;

        private BinaryTree FXPos;
        private BinaryTree FYPos;

        private List<ExportIEMPage> FPages;
        private int FWidth;
        private int FHeight;
        private float FMaxWidth;
        private float FMaxHeight;
        private float FMinLeft;
        private float FMinTop;
        private List<int> FMatrix;
        private float FDeltaY;
        private bool FShowProgress;
        private float FInaccuracy;
        private bool FRotatedImage;
        private bool FPlainRich;
        private bool FCropFillArea;
        private bool FFillArea;
        private bool FOptFrames;
        private float FLeft;
        private float FTop;
        private bool FPrintable;
        private bool FImages;
        private bool FWrap;
        private bool FBrushAsBitmap;
        private List<string> FFontList;
        private bool FDotMatrix;
        private Report FReport;
        private MyRes Res;
        private ExportIEMPage FCurrentPage;
        private bool FThreaded;
        private float FZoom;
        private bool FFullTrust;
        private bool FDataOnly;
        private ImageFormat FPictureFormat;
        private int FJpegQuality;
        private bool FFillAsBitmap;
        private bool FHTMLMode;
        private bool FWatermarks;
        private IDictionary FPicsCache;

        #endregion

        #region Properties

        public List<ExportIEMPage> Pages
        {
            get { return FPages; }
        }

        public bool Watermarks
        {
            get { return FWatermarks; }
            set { FWatermarks = value; }
        }

        public bool HTMLMode
        {
            get { return FHTMLMode; }
            set { FHTMLMode = value; }
        }

        public bool FillAsBitmap
        {
            get { return FFillAsBitmap; }
            set { FFillAsBitmap = value; }
        }

        public int JpegQuality
        {
            get { return FJpegQuality; }
            set { FJpegQuality = value; }
        }

        public ImageFormat ImageFormat
        {
            get { return FPictureFormat; }
            set { FPictureFormat = value; }
        }

        public bool FullTrust
        {
            get { return FFullTrust; }
            set { FFullTrust = value; }
        }

        public float Zoom
        {
            get { return FZoom; }
            set { FZoom = value; }
        }

        public bool Threaded
        {
            get { return FThreaded; }
            set { FThreaded = value; }
        }
        public int Width
        {
            get { return FWidth; }
        }
        public int Height
        {
            get { return FHeight; }
        }
        public float MaxWidth
        {
            get { return FMaxWidth; }
        }
        public float MaxHeight
        {
            get { return FMaxHeight; }
        }
        public float MinLeft
        {
            get { return FMinLeft; }
        }
        public float MinTop
        {
            get { return FMinTop; }
        }
        public bool ShowProgress
        {
            get { return FShowProgress; }
            set { FShowProgress = value; }
        }
        public float MaxCellHeight
        {
            get { return FYPos.MaxDistance; }
            set { FYPos.MaxDistance = value; }
        }
        public float MaxCellWidth
        {
            get { return FXPos.MaxDistance; }
            set { FXPos.MaxDistance = value;
            }
        }
        public int PagesCount
        {
            get { return FPages.Count; }
        }
        public int StylesCount
        {
            get { return FIEMStyleList.Count; }
        }
        public int ObjectsCount
        {
            get { return FIEMObjectList.Count; }
        }
        public float Inaccuracy
        {
            get { return FXPos.Inaccuracy; }
            set 
            { 
                FXPos.Inaccuracy = value;
                FYPos.Inaccuracy = value;
            }
        }
        public bool RotatedAsImage
        {
            get { return FRotatedImage; }
            set { FRotatedImage = value; }
        }
        public bool PlainRich
        {
            get { return FPlainRich; }
            set { FPlainRich = value; }
        }
        public bool AreaFill
        {
            get { return FFillArea; }
            set { FFillArea = value; }
        }
        public bool CropAreaFill
        {
            get { return FCropFillArea; }
            set { FCropFillArea = value; }
        }
        public bool FramesOptimization
        {
            get { return FOptFrames; }
            set { FOptFrames = value; }
        }
        public float Left
        {
            get { return FLeft; }
        }
        public float Top
        {
            get { return FTop; }
        }
        public bool Printable
        {
            get { return FPrintable; }
            set { FPrintable = value; }
        }
        public bool Images
        {
            get { return FImages; }
            set { FImages = value; }
        }
        public bool WrapText
        {
            get { return FWrap; }
            set { FWrap = value; }
        }
        public bool BrushAsBitmap
        {
            get { return FBrushAsBitmap; }
            set { FBrushAsBitmap = value; }
        }
             
        public bool DotMatrix
        {
            get { return FDotMatrix; }
            set { FDotMatrix = value; }
        }

        public bool DataOnly
        {
            get { return FDataOnly; }
            set { FDataOnly = value; }
        }

        public Report Report
        {
            get { return FReport; }
            set { FReport = value; }
        }

        #endregion

        #region Private Methods
       
        private int AddStyleInternal(ExportIEMStyle Style)
        {
            for (int i = FIEMStyleList.Count - 1; i >= 0 ; i--)
                if (Style.Equals(FIEMStyleList[i]))
                    return i;
            FIEMStyleList.Add(Style);
            return FIEMStyleList.Count - 1;
        }

        private int AddStyle(ReportComponentBase Obj)
        {             
            ExportIEMStyle Style = new ExportIEMStyle();
            if (Obj is TextObject)
            {
                TextObject MObj = Obj as TextObject;
                Style.Font = new Font(MObj.Font.FontFamily, MObj.Font.Size * FZoom, MObj.Font.Style, MObj.Font.Unit, MObj.Font.GdiCharSet, MObj.Font.GdiVerticalFont);
                Style.TextFill = MObj.TextFill;
                Style.RTL = MObj.RightToLeft;
                Style.Fill = MObj.Fill; 
                Style.Format = MObj.Format;
                Style.VAlign = MObj.VertAlign;
                if (Style.RTL)
                    Style.HAlign = MObj.HorzAlign == HorzAlign.Left ? HorzAlign.Right : (MObj.HorzAlign == HorzAlign.Right ? HorzAlign.Left : MObj.HorzAlign);
                else
                    Style.HAlign = MObj.HorzAlign;
                Style.Padding = new Padding((int)Math.Round(MObj.Padding.Left * FZoom),
                    (int)Math.Round(MObj.Padding.Top * FZoom),
                    (int)Math.Round(MObj.Padding.Right * FZoom),
                    (int)Math.Round(MObj.Padding.Bottom * FZoom));
                Style.FirstTabOffset = MObj.FirstTabOffset;                
                Style.Border = MObj.Border;
                Style.Border.LeftLine.Width = Style.Border.LeftLine.Width > 1 ? Style.Border.LeftLine.Width * FZoom : Style.Border.LeftLine.Width;
                Style.Border.RightLine.Width = Style.Border.RightLine.Width > 1 ? Style.Border.RightLine.Width * FZoom : Style.Border.RightLine.Width;
                Style.Border.TopLine.Width = Style.Border.TopLine.Width > 1 ? Style.Border.TopLine.Width * FZoom : Style.Border.TopLine.Width;
                Style.Border.BottomLine.Width = Style.Border.BottomLine.Width > 1 ? Style.Border.BottomLine.Width * FZoom : Style.Border.BottomLine.Width;
                Style.Angle = MObj.Angle;
            }
            else if (Obj is BandBase)
            {
                BandBase Band = Obj as BandBase;
                Style.Fill = Band.Fill;
                Style.Border = Band.Border;
            }
            else if (IsLine(Obj))
            {
                Style.Border = Obj.Border;
                if (Obj.Width == 0)
                    Style.Border.Lines = BorderLines.Left;
                else if (Obj.Height == 0)
                    Style.Border.Lines = BorderLines.Top;
                Style.Font = DefaultOneSizeFont;
            }
            else if (IsRect(Obj))
            {
                Style.Border = (Obj as ShapeObject).Border;
                Style.Border.Lines = BorderLines.All;
                Style.Fill = (Obj as ShapeObject).Fill;
            }
            else
            {                
                Style.Border = Obj.Border;
                Style.Fill = Obj.Fill;
                Style.Font = DefaultOneSizeFont;
                Style.Border.LeftLine.Width = Style.Border.LeftLine.Width > 1 ? Style.Border.LeftLine.Width * FZoom : Style.Border.LeftLine.Width;
                Style.Border.RightLine.Width = Style.Border.RightLine.Width > 1 ? Style.Border.RightLine.Width * FZoom : Style.Border.RightLine.Width;
                Style.Border.TopLine.Width = Style.Border.TopLine.Width > 1 ? Style.Border.TopLine.Width * FZoom : Style.Border.TopLine.Width;
                Style.Border.BottomLine.Width = Style.Border.BottomLine.Width > 1 ? Style.Border.BottomLine.Width * FZoom : Style.Border.BottomLine.Width;
            }
            return AddStyleInternal(Style);
        }

        private int AddInternalObject(ExportIEMObject Obj, int x, int y, int dx, int dy)
        {
            ExportIEMObjectList FObjItem = new ExportIEMObjectList();
            FObjItem.x = x;
            FObjItem.y = y;
            FObjItem.dx = dx;
            FObjItem.dy = dy;
            FObjItem.Object = Obj;
            FIEMObjectList.Add(FObjItem);
            return FIEMObjectList.Count - 1;
        }

        private bool IsMemo(ReportComponentBase Obj)
        {
            return (Obj is TextObject && ((Obj as TextObject).Angle == 0 || !FRotatedImage) && (Obj as TextObject).FontWidthRatio == 1);
        }
    
        private bool IsLine(ReportComponentBase Obj)
        {
            return (Obj is LineObject) && ((Obj.Width == 0) || (Obj.Height == 0));
        }

        private bool IsRect(ReportComponentBase Obj)
        {
            return (Obj is ShapeObject) && ((Obj as ShapeObject).Shape == ShapeKind.Rectangle);
        }
        
        private void FillArea(int x, int y, int dx, int dy, int Value)
        {
            int k;
            int ddx = x + dx;
            int ddy = y + dy;
            for (int i = y; i < ddy; i++)
            {
                k = FWidth * i;
                for (int j = x; j < ddx; j++)
                    FMatrix[k + j] = Value;
            }
        }
    
        private void ReplaceRect(int ObjIndex, int x, int y, int dx, int dy, int Value)
        {
            int k;
            int ddx = x + dx;
            int ddy = y + dy;           
            for (int i = y; i < ddy; i++)
            {
                k = FWidth * i;
                for (int j = x; j < ddx; j++)
                    if (FMatrix[k + j] == ObjIndex)
                        FMatrix[k + j] = Value;
            }
        }
    
        private void FindRect(int x, int y, out int dx, out int dy)
        {
            int Obj = FMatrix[FWidth * y + x];
            int px = x;
            int py = y;
            int ky;
            dx = 0;
            while (FMatrix[FWidth * py + px] == Obj)
            {
                ky = FWidth * py;
                while (FMatrix[ky + px] == Obj)
                    px++;                    
                if (dx == 0)
                    dx = px - x;
                else if ((px - x) < dx) 
                    break;
                py++;
                px = x;                                
            }
            dy = py - y;
        }

        private void Cut(int ObjIndex, int x, int y, int dx, int dy)
        {                        
            ExportIEMObject Obj = FIEMObjectList[ObjIndex].Object;
            ExportIEMObject NewObject = new ExportIEMObject();
            NewObject.StyleIndex = Obj.StyleIndex;
            NewObject.Style = Obj.Style;
            NewObject.Left = FXPos.Nodes[x].value;
            NewObject.Top = FYPos.Nodes[y].value;
            NewObject.Width = FXPos.Nodes[x + dx].value - FXPos.Nodes[x].value;
            NewObject.Height = FYPos.Nodes[y + dy].value - FYPos.Nodes[y].value;
            NewObject.Parent = Obj;
            NewObject.IsText = Obj.IsText;
            NewObject.IsRichText = Obj.IsRichText;
            NewObject.Metafile = Obj.Metafile;
            NewObject.PictureStream = Obj.PictureStream;
            NewObject.Base = Obj.Base;
            NewObject.Hash = Obj.Hash;
            float fdy = Obj.Top + Obj.Height - NewObject.Top;
            float fdx = Obj.Left + Obj.Width - NewObject.Left;
            if ((fdy > Obj.Height / 3) && (fdx > Obj.Width / 3))
            {
                NewObject.IsText = Obj.IsText;
                NewObject.Text = Obj.Text;
                NewObject.HtmlTags = Obj.HtmlTags;
                Obj.Text = String.Empty;
                Obj.IsText = true;                
            }
            int NewIndex = AddInternalObject(NewObject, x, y, dx, dy);
            ReplaceRect(ObjIndex, x, y, dx, dy, NewIndex);
            CloneFrames(ObjIndex, NewIndex);
            FIEMObjectList[NewIndex].Exist = true;
        }
    
        private void CloneFrames(int Obj1, int Obj2)
        {
            ExportIEMObject FOld, FNew;
            BorderLines FrameTyp;
            FOld = FIEMObjectList[Obj1].Object;
            FNew = FIEMObjectList[Obj2].Object;
            if ((FOld.Style != null) && (FNew.Style != null))
            {
                FrameTyp = BorderLines.None;
                if (((BorderLines.Top & FOld.Style.Border.Lines) != 0) && 
                    (Math.Abs(FOld.Top - FNew.Top) <= FInaccuracy))
                    FrameTyp |= BorderLines.Top;
                if (((BorderLines.Left & FOld.Style.Border.Lines) != 0) && 
                    (Math.Abs(FOld.Left - FNew.Left) <= FInaccuracy))
                    FrameTyp |= BorderLines.Left;
                if (((BorderLines.Bottom & FOld.Style.Border.Lines) != 0) && 
                    (Math.Abs((FOld.Top + FOld.Height) - (FNew.Top + FNew.Height)) <= FInaccuracy))
                    FrameTyp |= BorderLines.Bottom;
                if (((BorderLines.Right & FOld.Style.Border.Lines) != 0) && 
                    (Math.Abs((FOld.Left + FOld.Width) - (FNew.Left + FNew.Width)) <= FInaccuracy))
                    FrameTyp |= BorderLines.Right;

                if (FrameTyp != FNew.Style.Border.Lines)
                { 
                    ExportIEMStyle NewStyle = new ExportIEMStyle();
                    NewStyle.Assign(FOld.Style);
                    NewStyle.Border.Lines = FrameTyp;               
                    FNew.StyleIndex = AddStyleInternal(NewStyle);
                    FNew.Style = FIEMStyleList[FNew.StyleIndex];     
                }
            }
        }

        private void Render()
        {
            int i, j, k, Old;
            ExportIEMObjectList Obj;
            ExportIEMStyle Style;
            FillBase OldColor;

            FXPos.Close();
            FYPos.Close();

            FWidth = FXPos.Count;
            FHeight = FYPos.Count;
            
            FMatrix.Clear();

            k = FWidth * FHeight;
            for (i = 0; i < k; i++)
                FMatrix.Add(-1);
            
            for (i = 0; i < FIEMObjectList.Count; i++)
            {
                Obj = FIEMObjectList[i];
                
                j = FXPos.IndexOf(Obj.Object.Left);
                if ((j != -1) && (j < FXPos.Count))
                {
                    FIEMObjectList[i].x = j;
                    Obj.Object.Left = FXPos.Nodes[j].value;
                    k = FXPos.IndexOf(Obj.Object.Left + Obj.Object.Width);
                    FIEMObjectList[i].dx = k - j;
                }
                j = FYPos.IndexOf(Obj.Object.Top);
                if ((j != -1) && (j < FYPos.Count))
                {
                    FIEMObjectList[i].y = j;
                    Obj.Object.Top = FYPos.Nodes[j].value;
                    k = FYPos.IndexOf(Obj.Object.Top + Obj.Object.Height);
                    FIEMObjectList[i].dy = k - j;
                }

                if ((Obj.Object.Style != null) && Obj.Object.Style.FillColor == Color.Transparent) 
                {
                    Old = FMatrix[FWidth * Obj.y + Obj.x];
                    if (Old != -1 && FIEMObjectList[Old].Object.Style != null)
                    {
                        OldColor = FIEMObjectList[Old].Object.Style.Fill;
                        if ((ExportUtils.GetColorFromFill(OldColor) != Obj.Object.Style.FillColor) && 
                            (ExportUtils.GetColorFromFill(OldColor) != Obj.Object.Style.TextColor))
                        {
                            Style = new ExportIEMStyle();
                            Style.Assign(Obj.Object.Style);
                            Style.Fill = OldColor;
                            Obj.Object.StyleIndex = AddStyleInternal(Style);
                            Obj.Object.Style = FIEMStyleList[Obj.Object.StyleIndex];
                        }
                    }
                }

                if (!(Obj.Object.IsBand && Obj.Object.Style != null &&                    
                    ((Obj.Object.Style.FillColor == Color.White) || (Obj.Object.Style.FillColor == Color.Transparent)) &&
                    (Obj.Object.Style.Border.Lines == BorderLines.None))) 
                  FillArea(Obj.x, Obj.y, Obj.dx, Obj.dy, i);
          }
        }

        private void Analyze()
        {
            int k;
            int dx, dy, ki;
            ExportIEMObjectList obj;
            for (int i = 0; i < FHeight - 1; i++)
            {
                ki = FWidth * i;
                for (int j = 0; j < FWidth - 1; j++)
                {
                    k = FMatrix[ki + j];
                    if (k != -1)
                    {
                        obj = FIEMObjectList[k];
                        if (!obj.Exist)
                        {
                            FindRect(j, i, out dx, out dy);
                            if (j + dx >= FXPos.Count)
                                dx = FXPos.Count - j - 1;
                            if (i + dy >= FYPos.Count)
                                dy = FYPos.Count - i - 1;
                            if ((obj.x != j) || (obj.y != i) || (obj.dx != dx) || (obj.dy != dy))
                            {
                                if (!obj.Exist)
                                    Cut(k, j, i, dx, dy);
                                else
                                    obj.Exist = true;
                            }
                            else
                                obj.Exist = true;
                        }
                    }
                }
            }
        }

        private void OptimizeFrames()
        {
            int x, y;
            ExportIEMObject Obj, PrevObj;
            BorderLines CurrentLines;
            ExportIEMStyle Style;
            for (y = 0; y < Height; y++)
                for (x = 0; x < Width; x++)
                {
                    Obj = GetObject(x, y);
                    if (Obj == null)
                        continue;
                    if (Obj.Style != null && Obj.Style.Border.Lines != BorderLines.None)
                    {
                        CurrentLines = Obj.Style.Border.Lines;
                        if (((BorderLines.Top & CurrentLines) > 0) && (y > 0))
                        {
                            PrevObj = GetObject(x, y - 1);
                            if ((PrevObj != null) && (PrevObj != Obj) && (PrevObj.Style != null)
                                    && ((BorderLines.Bottom & PrevObj.Style.Border.Lines) > 0)
                                && (PrevObj.Style.Border.BottomLine.Width == Obj.Style.Border.TopLine.Width)
                                && (PrevObj.Style.Border.BottomLine.Color == Obj.Style.Border.TopLine.Color)
                                )
                            {
                                CurrentLines = CurrentLines & ~BorderLines.Top;
                                Style = new ExportIEMStyle();
                                Style.Assign(Obj.Style);
                                Style.Border.Lines = CurrentLines;
                                Obj.StyleIndex = AddStyleInternal(Style);
                                Obj.Style = FIEMStyleList[Obj.StyleIndex];
                            }
                        }
                        if (((BorderLines.Left & CurrentLines) > 0) && (x > 0))
                        {
                            PrevObj = GetObject(x - 1, y);
                            if ((PrevObj != null) && (PrevObj != Obj) && (PrevObj.Style != null)
                                    && ((BorderLines.Right & PrevObj.Style.Border.Lines) > 0)
                                && (PrevObj.Style.Border.RightLine.Width == Obj.Style.Border.LeftLine.Width)
                                && (PrevObj.Style.Border.RightLine.Color == Obj.Style.Border.LeftLine.Color)
                                )
                            {
                                CurrentLines = CurrentLines & ~BorderLines.Left;
                                Style = new ExportIEMStyle();
                                Style.Assign(Obj.Style);
                                Style.Border.Lines = CurrentLines;
                                Obj.StyleIndex = AddStyleInternal(Style);
                                Obj.Style = FIEMStyleList[Obj.StyleIndex];
                            }
                        }
                    }
                }
        }

        private void AddSetObjectPos(ReportComponentBase Obj, ref ExportIEMObject FObj)
        {
            float Left = Obj.AbsLeft;
            float Top = Obj.AbsTop;
            float Width = Obj.Width;
            float Height = Obj.Height;

            if (Left >= 0)
                FObj.Left = Width > 0 ? Left * FZoom : (Left + Width) * FZoom;
            else
                FObj.Left = 0;

            if (Top >= 0)
                FObj.Top = Height > 0 ? FDeltaY + Top * FZoom : FDeltaY + (Top + Height) * FZoom;
            else
                FObj.Top = FDeltaY;

            if (IsLine(Obj))
            {
                FObj.Width = Math.Abs(Width) > 0 ? Math.Abs(Width) * FZoom : Obj.Border.Width * FZoom;
                FObj.Height = Math.Abs(Height) > 0 ? Math.Abs(Height) * FZoom : Obj.Border.Width * FZoom;
            }
            else
            {
                FObj.Width = Math.Abs(Width) * FZoom;
                FObj.Height = Math.Abs(Height) * FZoom;
            }

            if ((FObj.Left + FObj.Width) > (FCurrentPage.Width - FCurrentPage.LeftMargin - FCurrentPage.RightMargin) * Units.Millimeters)
                FObj.Width = (FCurrentPage.Width - FCurrentPage.LeftMargin - FCurrentPage.RightMargin) * Units.Millimeters - FObj.Left;

            if ((FObj.Top + FObj.Height) > FMaxHeight)
                FMaxHeight = FObj.Top + FObj.Height;
            if ((FObj.Left + FObj.Width) > FMaxWidth)
                FMaxWidth = FObj.Left + FObj.Width;
            if (FObj.Left < FMinLeft)
                FMinLeft = FObj.Left;
            if (FObj.Top < FMinTop)
                FMinTop = FObj.Top;
            if ((FObj.Left < FLeft) || (FLeft == 0))
                FLeft = FObj.Left;
            if ((FObj.Top < FTop) || (FTop == 0))
                FTop = FObj.Top;

            FXPos.Add(FObj.Left);
            FXPos.Add(FObj.Left + FObj.Width);
            FYPos.Add(FObj.Top);
            FYPos.Add(FObj.Top + FObj.Height);

            AddInternalObject(FObj, 0, 0, 1, 1);
        }

        #endregion

        #region Public Methods

        public ExportIEMStyle StyleById(int Index)
        {
            return FIEMStyleList[Index];
        }
                 
        public int Cell(int x, int y)
        {
            if ((x < FWidth) && (y < FHeight) && (x >= 0) && (y >= 0))
                return FMatrix[FWidth * y + x];
            return -1;
        }

        public ExportIEMObject ObjectById(int ObjIndex)
        {
            if (ObjIndex < FIEMObjectList.Count)
                return FIEMObjectList[ObjIndex].Object;
            return null;
        }

        public float XPosById(int PosIndex)
        {
            return FXPos.Nodes[PosIndex].value;
        }

        public float YPosById(int PosIndex)
        {
            return FYPos.Nodes[PosIndex].value;
        }

        public ExportIEMObject GetObject(int x, int y)
        {
            int i = FMatrix[FWidth * y + x];
            if (i == -1)
                return null;
            return FIEMObjectList[i].Object;
        }
                       
        public void Clear()
        {
            FPicsCache.Clear();
            FIEMObjectList.Clear();
            FIEMStyleList.Clear();
            FXPos.Clear();
            FYPos.Clear();
            FPages.Clear();
            FFontList.Clear();
            FMatrix.Clear();
            FDeltaY = 0;
        }

        public void AddBandObject(BandBase Obj)
        {            
            ExportIEMObject FObj = new ExportIEMObject();
            FObj.StyleIndex = AddStyle(Obj);
            if (FObj.StyleIndex != -1)
                FObj.Style = FIEMStyleList[FObj.StyleIndex];
            FObj.IsText = true;
            FObj.IsBand = true;            
            AddSetObjectPos(Obj as ReportComponentBase, ref FObj);
        }

        private void AddLineObject(LineObject Obj)
        {
            ExportIEMObject FObj = new ExportIEMObject();
            FObj.StyleIndex = AddStyle(Obj);
            if (FObj.StyleIndex != -1)
                FObj.Style = FIEMStyleList[FObj.StyleIndex];
            FObj.IsText = true;
            AddSetObjectPos(Obj as ReportComponentBase, ref FObj);
        }

        private void AddRectangle(ShapeObject Obj)
        {
            ExportIEMObject FObj = new ExportIEMObject();
            FObj.StyleIndex = AddStyle(Obj);
            if (FObj.StyleIndex != -1)
                FObj.Style = FIEMStyleList[FObj.StyleIndex];
            FObj.IsText = true;
            AddSetObjectPos(Obj as ReportComponentBase, ref FObj);
        }

        private Size GetImageSize(ExportIEMObject FObj, ReportComponentBase Obj)
        {
            float dx = FObj.Width;
            float dy = FObj.Height;
            if (FHTMLMode)
            {
                if ((Obj.Border.Lines & BorderLines.Left) != 0)
                    dx -= Obj.Border.LeftLine.Width * FZoom;
                if ((Obj.Border.Lines & BorderLines.Right) != 0)
                    dx -= Obj.Border.RightLine.Width * FZoom;
                if ((Obj.Border.Lines & BorderLines.Top) != 0)
                    dy -= Obj.Border.TopLine.Width * FZoom;
                if ((Obj.Border.Lines & BorderLines.Bottom) != 0)
                    dy -= Obj.Border.BottomLine.Width * FZoom;
            }
            dx = (dx >= 1 ? dx : 1);
            dy = (dy >= 1 ? dy : 1);

            return new Size((int)Math.Round(dx), (int)Math.Round(dy));
        }

        private void DrawImage(Graphics g, ReportComponentBase Obj)
        {
            using (GraphicCache cache = new GraphicCache())
            {
                // g.Clear is needed to draw anti-aliased text correctly
                if (Obj is TextObjectBase)
                    g.Clear(Color.White);

                float Left = Obj.Width >= 0 ? Obj.AbsLeft : Obj.AbsLeft + Obj.Width;
                float Top = Obj.Height >= 0 ? Obj.AbsTop : Obj.AbsTop + Obj.Height;

                if (FHTMLMode)
                {
                    float dx = (Obj.Border.Lines & BorderLines.Left) != 0 ? Obj.Border.LeftLine.Width : 0;
                    float dy = (Obj.Border.Lines & BorderLines.Top) != 0 ? Obj.Border.TopLine.Width : 0;
                    g.TranslateTransform((-Left - dx) * FZoom, (-Top - dy) * FZoom);
                }
                else
                    g.TranslateTransform(-Left * FZoom, -Top * FZoom);
                BorderLines oldLines = Obj.Border.Lines;
                Obj.Border.Lines = BorderLines.None;
                Obj.Draw(new FRPaintEventArgs(g, FZoom, FZoom, cache));
                Obj.Border.Lines = oldLines;
            }
        }
      
        public void AddPictureObject(ReportComponentBase Obj)
        {
            ExportIEMObject FObj = new ExportIEMObject();
            Padding oldPadding = new Padding();
            if (Obj is TextObject)
            {
                oldPadding = (Obj as TextObject).Padding;
                (Obj as TextObject).Padding = new Padding();
            }
            FObj.StyleIndex = AddStyle(Obj);
            if (Obj is TextObject)
                (Obj as TextObject).Padding = oldPadding;
            if (FObj.StyleIndex != -1)
                FObj.Style = FIEMStyleList[FObj.StyleIndex];
            FObj.IsText = false;
            AddSetObjectPos(Obj, ref FObj);
            FObj.PictureStream = new MemoryStream();
            using (Bitmap bmp = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    IntPtr hdc = g.GetHdc();
                    try
                    {
                        FObj.Metafile = new Metafile(FObj.PictureStream, hdc,
                            new Rectangle(new Point(0, 0), GetImageSize(FObj, Obj)), MetafileFrameUnit.Pixel);
                    }
                    finally
                    {
                        g.ReleaseHdc(hdc);
                    }
                }
            }
            using (Graphics g = Graphics.FromImage(FObj.Metafile))
            {
                DrawImage(g, Obj);
            }
            FObj.PictureStream.Position = 0;
            CheckPicsCache(FObj);
            AddShadow(Obj);
        }

        public void AddPictureObject_Safe(ReportComponentBase Obj)
        {
            ExportIEMObject FObj = new ExportIEMObject();
            Padding oldPadding = new Padding();
            if (Obj is TextObject)
            {
                oldPadding = (Obj as TextObject).Padding;
                (Obj as TextObject).Padding = new Padding();
            }
            FObj.StyleIndex = AddStyle(Obj);
            if (Obj is TextObject)
                (Obj as TextObject).Padding = oldPadding;
            if (FObj.StyleIndex != -1)
                FObj.Style = FIEMStyleList[FObj.StyleIndex];
            FObj.IsText = false;
            AddSetObjectPos(Obj, ref FObj);



            FObj.PictureStream = new MemoryStream();
            Size imageSize = GetImageSize(FObj, Obj);
            using (System.Drawing.Image image = new Bitmap(imageSize.Width, imageSize.Height))
            {
                using (Graphics g = Graphics.FromImage(image))
                {
                    DrawImage(g, Obj);
                }

                if (FPictureFormat == ImageFormat.Jpeg)
                {
                    // handle jpeg separately to set the JpegQuality
                    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
                    ImageCodecInfo ici = null;
                    foreach (ImageCodecInfo codec in codecs)
                    {
                        if (codec.MimeType == "image/jpeg")
                        {
                            ici = codec;
                            break;
                        }
                    }
                    EncoderParameters ep = new EncoderParameters();
                    ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, FJpegQuality);
                    image.Save(FObj.PictureStream, ici, ep);
                }
                else
                    image.Save(FObj.PictureStream, FPictureFormat);
            }
            FObj.PictureStream.Position = 0;
            CheckPicsCache(FObj);                       
            AddShadow(Obj);
        }
                
        public void AddTextObject(TextObject Obj)
        {             
            ExportIEMObject FObj = new ExportIEMObject();
            FObj.StyleIndex = AddStyle(Obj);
            if (FObj.StyleIndex != -1)
                FObj.Style = FIEMStyleList[FObj.StyleIndex];           
            FObj.Text = Obj.Text;
            FObj.HtmlTags = Obj.HtmlTags;
            FObj.IsText = true;
            FObj.IsRichText = false;
            if (FWrap)
                FObj.WrappedText = WrapTextObject(Obj);
            AddSetObjectPos(Obj as ReportComponentBase, ref FObj);
            AddShadow(Obj);
            if (FFillAsBitmap && !(FObj.Style.Fill is SolidFill))
            {
                float dx = FObj.Width;
                float dy = FObj.Height;
                dx = (dx >= 1 ? dx : 1);
                dy = (dy >= 1 ? dy : 1);
                System.Drawing.Image image = new Bitmap((int)dx, (int)dy);
                
                using (Graphics g = Graphics.FromImage(image))
                {
                    g.Clear(Color.Transparent);
                    g.TranslateTransform(-Obj.AbsLeft * FZoom, -Obj.AbsTop * FZoom);
                    BorderLines oldLines = Obj.Border.Lines;
                    Obj.Border.Lines = BorderLines.None;
                    string oldText = Obj.Text;
                    Obj.Text = String.Empty;
                    Obj.Draw(new FRPaintEventArgs(g, FZoom, FZoom, Report.GraphicCache));
                    Obj.Text = oldText;
                    Obj.Border.Lines = oldLines;
                    FObj.PictureStream = new MemoryStream();
                    image.Save(FObj.PictureStream, FPictureFormat);
                    CheckPicsCache(FObj);
                }                
            }
        }


        private void CheckPicsCache(ExportIEMObject FObj)
        {
            FObj.Hash = ExportUtils.MD5Stream(FObj.PictureStream);
            FObj.Base = !((Dictionary<string, MemoryStream>)FPicsCache).ContainsKey(FObj.Hash);
            if (FObj.Base)
                FPicsCache.Add(FObj.Hash, FObj.PictureStream);
            else
                FObj.PictureStream = ((Dictionary<string, MemoryStream>)FPicsCache)[FObj.Hash];
        }

        private void AddShadow(ReportComponentBase Obj)
        {
            if (Obj.Border.Shadow)
            {
                using (TextObject shadow = new TextObject())
                {
                    shadow.Left = Obj.Left + Obj.Width;
                    shadow.Width = Obj.Border.ShadowWidth;
                    shadow.Top = Obj.Top + Obj.Border.ShadowWidth;
                    shadow.Height = Obj.Height;
                    shadow.Fill = new SolidFill(Obj.Border.ShadowColor);
                    AddTextObject(shadow);
                }
                using (TextObject shadow = new TextObject())
                {
                    shadow.Left = Obj.Left + Obj.Border.ShadowWidth;
                    shadow.Width = Obj.Width - Obj.Border.ShadowWidth;
                    shadow.Top = Obj.Top + Obj.Height;
                    shadow.Height = Obj.Border.ShadowWidth;
                    shadow.Fill = new SolidFill(Obj.Border.ShadowColor);
                    AddTextObject(shadow);
                }
            }
        }

        private List<string> WrapTextObject(TextObject obj)
        {
            float FDpiFX = 96f / DrawUtils.ScreenDpi;
            List<string> result = new List<string>();
            DrawText drawer = new DrawText();
            using (Bitmap b = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(b))
            using (Font f = new Font(obj.Font.Name, obj.Font.Size * FDpiFX, obj.Font.Style))
            {
                float h = f.Height - f.Height / 4;
                float memoWidth = obj.Width - obj.Padding.Horizontal;

                float memoHeight = drawer.CalcHeight(obj.Text, g, f,
                    memoWidth, obj.Height - obj.Padding.Vertical,
                    obj.HorzAlign, obj.LineHeight, obj.ForceJustify, obj.RightToLeft, obj.WordWrap, obj.Trimming);

                float y, prevy = 0;
                StringBuilder line = new StringBuilder(256);
                foreach (Paragraph par in drawer.Paragraphs)
                {
                    foreach (Word word in par.Words)
                    {
                        if (!word.Visible)
                            break;
                        y = word.Top + 1;
                        if (prevy == 0)
                            prevy = y;
                        if (y != prevy)
                        {
                            result.Add(line.ToString());
                            line.Length = 0;
                            prevy = y;
                        }
                        line.Append(word.Text).Append(' ');
                    }
                }
                result.Add(line.ToString());
            }
            return result;
        }
        
        public void AddPage(ReportPage page)
        {
            FCurrentPage = new ExportIEMPage();
            FCurrentPage.Landscape = page.Landscape;
            FCurrentPage.Width = page.PaperWidth * FZoom;
            FCurrentPage.Height = page.PaperHeight * FZoom;
            FCurrentPage.LeftMargin = page.LeftMargin * FZoom;
            FCurrentPage.TopMargin = page.TopMargin * FZoom;
            FCurrentPage.RightMargin = page.RightMargin * FZoom;
            FCurrentPage.BottomMargin = page.BottomMargin * FZoom;
            bool RepeatDataband = false;
            foreach (Base c in page.AllObjects)
            {
                if (c is ReportComponentBase)
                {
                    ReportComponentBase obj = c as ReportComponentBase;

                    if (obj is CellularTextObject)
                        obj = (obj as CellularTextObject).GetTable();
                    if (FDataOnly && (obj.Parent == null || !(obj.Parent is DataBand)))
                        continue;
                    if (obj is TableCell)
                        continue;
                    else
                        if (obj is TableBase)
                        {
                            TableBase table = obj as TableBase;
                            table.EmulateOuterBorder();
                            using (TextObject tableback = new TextObject())
                            {
                                tableback.Border = table.Border;
                                tableback.Fill = table.Fill;
                                tableback.Left = table.AbsLeft;
                                tableback.Top = table.AbsTop;
                                float tableWidth = 0;
                                for (int i = 0; i < table.ColumnCount; i++)
                                    tableWidth += table[i, 0].Width;
                                tableback.Width = (tableWidth < table.Width) ? tableWidth : table.Width;
                                tableback.Height = table.Height;
                                AddTextObject(tableback);
                            }
                            float y = 0;
                            for (int i = 0; i < table.RowCount; i++)
                            {
                                float x = 0;
                                for (int j = 0; j < table.ColumnCount; j++)
                                {
                                    if (!table.IsInsideSpan(table[j, i]))
                                    {
                                        TableCell textcell = table[j, i];
                                        textcell.Left = x;
                                        textcell.Top = y;
                                        if (IsMemo(textcell))
                                            AddTextObject(textcell as TextObject);
                                        else if (FImages)
                                            if (FFullTrust)
                                                AddPictureObject(textcell as ReportComponentBase);
                                            else
                                                AddPictureObject_Safe(textcell as ReportComponentBase);
                                    }
                                    x += (table.Columns[j]).Width;
                                }
                                y += (table.Rows[i]).Height;
                            }
                            RepeatDataband = true;
                        }
                        else if (IsMemo(obj))
                            AddTextObject(obj as TextObject);
                        else if (obj is BandBase)
                        {
                            if (!RepeatDataband)
                                AddBandObject(obj as BandBase);
                            RepeatDataband = false;
                        }
                        else if (IsLine(obj))
                            AddLineObject(obj as LineObject);
                        else if (IsRect(obj))
                            AddRectangle(obj as ShapeObject);
                        else if (FImages)
                            if (FFullTrust)
                                AddPictureObject(obj as ReportComponentBase);
                            else
                                AddPictureObject_Safe(obj as ReportComponentBase);
                }
            }
            if (FWatermarks)
                AddWatermark(page, FCurrentPage);
            FDeltaY = FMaxHeight;
            FCurrentPage.Value = FMaxHeight;
            FPages.Add(FCurrentPage);
        }

        private void AddWatermark(ReportPage page, ExportIEMPage matrixPage)
        {
            if (page.Watermark.Enabled)
            {
                int dx = (int)(page.PaperWidth * Units.Millimeters * FZoom);
                int dy = (int)(page.PaperHeight * Units.Millimeters * FZoom);

                using (System.Drawing.Image image = new Bitmap(dx, dy))
                {
                    matrixPage.WatermarkPictureStream = new MemoryStream();
                    using (Graphics g = Graphics.FromImage(image))
                    {
                        g.Clear(Color.White);
                        page.Watermark.DrawImage(new FRPaintEventArgs(g, 1, 1, page.Report.GraphicCache),
                            new Rectangle(0, 0, dx, dy), Report, true);
                        page.Watermark.DrawText(new FRPaintEventArgs(g, 1, 1, page.Report.GraphicCache),
                            new Rectangle(0, 0, dx, dy), Report, true);
                    }

                    if (FPictureFormat == ImageFormat.Jpeg)
                    {
                        // handle jpeg separately to set the JpegQuality
                        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
                        ImageCodecInfo ici = null;
                        foreach (ImageCodecInfo codec in codecs)
                        {
                            if (codec.MimeType == "image/jpeg")
                            {
                                ici = codec;
                                break;
                            }
                        }
                        EncoderParameters ep = new EncoderParameters();
                        ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, FJpegQuality);
                        image.Save(matrixPage.WatermarkPictureStream, ici, ep);
                    }
                    else
                        image.Save(matrixPage.WatermarkPictureStream, FPictureFormat);
                    matrixPage.WatermarkPictureStream.Position = 0;
                }
            }
        }

        public void Prepare()
        {
            ExportIEMStyle Style;
            ExportIEMObject FObj;
            ExportIEMObjectList FObjItem;
            if (FFillArea)
            {
                Style = new ExportIEMStyle();
                Style.Fill = new SolidFill(Color.Transparent);
                FObj = new ExportIEMObject();
                FObj.StyleIndex = AddStyleInternal(Style);
                FObj.Style = Style;
                if (FCropFillArea)
                {
                    FObj.Left = FMinLeft;
                    FObj.Top = FMinTop;
                }
                else
                    FObj.Left = FObj.Top = 0;                                     
                FObj.Width = MaxWidth;
                FObj.Height = MaxHeight;
                FObj.IsText = true;
                FXPos.Add(0);
                FYPos.Add(0);
                FObjItem = new ExportIEMObjectList();
                FObjItem.x = FObjItem.y = 0;                 
                FObjItem.dx = FObjItem.dy = 1;
                FObjItem.Object = FObj;
                FIEMObjectList.Insert(0, FObjItem);
            }
            if (FShowProgress)
              Config.ReportSettings.OnProgress(FReport, Res.Get("OrderByCells"));
            if (FThreaded)
                Thread.Sleep(0);
            Render();
            if (FShowProgress)
              Config.ReportSettings.OnProgress(FReport, Res.Get("AnalyzeObjects"));
            if (FThreaded)
              Thread.Sleep(0);
            Analyze();
            if (FOptFrames)
                OptimizeFrames();
        }

        public void ObjectPos(int ObjIndex, out int x, out int y, out int dx, out int dy)
        {
            x = FIEMObjectList[ObjIndex].x;
            y = FIEMObjectList[ObjIndex].y;
            dx = FIEMObjectList[ObjIndex].dx;
            dy = FIEMObjectList[ObjIndex].dy;
        }

        public float PageBreak(int Page)
        {
            if (Page < FPages.Count)
                return FPages[Page].Value;
            return 0f;
        }

        public float PageWidth(int Page)
        {
            if (Page < FPages.Count)
                return FPages[Page].Width;
            return 0f;
        }

        public float PageHeight(int Page)
        {
            if (Page < FPages.Count)
                return FPages[Page].Height;
            return 0f;
        }

        public float PageLMargin(int Page)
        {
            if (Page < FPages.Count)
                return FPages[Page].LeftMargin;
            return 0f;
        }

        public float PageTMargin(int Page)
        {
            if (Page < FPages.Count)
                return FPages[Page].TopMargin;
            return 0f;
        }

        public float PageRMargin(int Page)
        {
            if (Page < FPages.Count)
                return FPages[Page].RightMargin;
            return 0f;
        }

        public float PageBMargin(int Page)
        {
            if (Page < FPages.Count)
                return FPages[Page].BottomMargin;
            return 0f;
        }

        public bool Landscape(int Page)
        {
            if (Page < FPages.Count)
                return FPages[Page].Landscape;
            return false;
        }

        #endregion

        public ExportMatrix()
        {
            DefaultOneSizeFont = new Font("Arial", 1);

            FFontList = new List<string>();
            FIEMObjectList = new List<ExportIEMObjectList>();
            FIEMStyleList = new List<ExportIEMStyle>();
            
            FXPos = new BinaryTree();
            FYPos = new BinaryTree();

            FPages = new List<ExportIEMPage>();
            FMatrix = new List<int>();
            FMaxWidth = 0;
            FMaxHeight = 0;
            FMinLeft = 99999; 
            FMinTop = 99999; 
            FDeltaY = 0;
            FInaccuracy = 0.5F;
            FRotatedImage = false;
            FPlainRich = true;
            FFillArea = false;
            FCropFillArea = false;
            FOptFrames = false;
            FTop = 0;
            FLeft = 0;
            FPrintable = true;
            FImages = true;
            FWrap = false;
            FBrushAsBitmap = true;
            FThreaded = false;
            FZoom = 1f;
            FDataOnly = false;
            FFullTrust = Config.FullTrust;
            FPictureFormat = ImageFormat.Png;
            FJpegQuality = 100;
            Res = new MyRes("Export,Misc");
            FFillAsBitmap = false;
            FHTMLMode = false;
            FWatermarks = false;
            FPicsCache = new Dictionary<string, MemoryStream>();
        }
    }
}
