/*
Copyright (C) 2008-2009, Datalogics, Inc. All rights reserved.
 
The information and code in this sample is for the exclusive use
of Datalogics customers and evaluation users only.  Datalogics 
permits you to use, modify and distribute this file in accordance 
with the terms of your license agreement. Sample code is for 
demonstrative purposes only and is not intended for production use.
*/

/*
 * ImageArray2DManager :
 * 
 * ImageArray2DManager contains a structure that holds all of the
 * information that is used to render the PDF to the screen. This
 * structure holds the dimensions of each page, the scroll position
 * in pixels and percent, and an array of tiles which within each
 * tile is a bitmap containing a piece of a page (depending on the 
 * size/zoom level there may be 1 to many tiles).
 */

using System;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using Datalogics.PDFL;

namespace DotNETViewerComponent
{
    public struct PageInfo
    {
        public System.Drawing.Rectangle boundingRectangle;
    }

    public struct Tile
    {
        public Rectangle TileRect;
        public Bitmap TileBitmap;

        public bool IsValid { get { return TileBitmap != null; } }
    }

    public class PageCache
    {
        #region Methods
        public void Load(Document doc, Page page, Datalogics.PDFL.Point scale, OptionalContentContext occ)
        {
            Clear();
            SetPageParam(doc, page, scale, occ);

            Tile fullPage = Create(CropRect);
            if (fullPage.IsValid)
                fullBitmap = fullPage.TileBitmap;
            else
                tiles[0, 0] = Create(tiles[0, 0].TileRect);
        }
        public Tile Create(Rectangle tileRect)
        {
            if (tileRect.Width > TileSize.Width && tileRect.Height > TileSize.Height)
            {
                return new Tile();
            }
            DrawParams drawParams = new DrawParams();
            drawParams.Flags = DrawFlags.UseAnnotFaces | DrawFlags.DoLazyErase | DrawFlags.OptimizeForInteractive;
            drawParams.SmoothFlags = SmoothFlags.LineArt | SmoothFlags.Text | SmoothFlags.Image;
            drawParams.BypassCopyPerm = true;
            drawParams.OptionalContentContext = occ;

            Matrix pageMatrix = new Matrix();
            double xShiftModified = CropRect.X + CropRect.Width / 2.0;
            double yShiftModified = CropRect.Y + CropRect.Height / 2.0;
            pageMatrix = pageMatrix.Translate(xShiftModified, yShiftModified);
            pageMatrix = pageMatrix.Rotate(RotateAngle).Scale(scale.H, -scale.V);

            using (Page page = doc.GetPage(pageNum))
            {
                Rect originalRect = page.CropBox;
                pageMatrix = pageMatrix.Translate(-originalRect.LLx - originalRect.Width / 2.0, -originalRect.LLy - originalRect.Height / 2.0);

                drawParams.Matrix = new Matrix().Translate(-tileRect.X, -tileRect.Y).Concat(pageMatrix);
                drawParams.UpdateRect = new Rect(tileRect).Transform(pageMatrix.Invert());
                drawParams.DestRect = new Rect(0, 0, tileRect.Width, tileRect.Height);

                Bitmap bitmap = new Bitmap(tileRect.Width, tileRect.Height);
                page.DrawContents(bitmap, drawParams);
                //using (Graphics g = Graphics.FromImage(bitmap)) { g.DrawRectangle(Pens.Red, new Rectangle(0, 0, bitmap.Width-1, bitmap.Height-1)); }
                return CreateTile(tileRect, bitmap);
            }
        }
        public void Clear()
        {
            if (tiles != null)
            {
                for (int y = 0; y < tiles.GetLength(1); ++y)
                {
                    for (int x = 0; x < tiles.GetLength(0); ++x)
                    {
                        if (tiles[x, y].TileBitmap != null)
                        {
                            tiles[x, y].TileBitmap.Dispose();
                            tiles[x, y].TileBitmap = null;
                        }
                    }
                }
                tiles = null;
            }

            if (fullBitmap != null)
            {
                fullBitmap.Dispose();
                fullBitmap = null;
            }

            //if (page != null) page.Dispose();
            //page = null;
            occ = null;
        }
        public void SetPageParam(Document doc, Page page, Datalogics.PDFL.Point scale, OptionalContentContext occ)
        {
            this.scale = scale;
            //this.page = page;
            this.doc = doc;
            this.pageNum = page.PageNumber;
            this.occ = occ;
            SetPageRotate(page.Rotation);
            Rect cropBoxScaled = TransformRectByPageParam(page.CropBox ,scale);

            cropRect = new Rectangle();
            cropRect.Width = (int)cropBoxScaled.Width;
            cropRect.Height = (int)cropBoxScaled.Height;

            TileSize = new Size(1600, 1600);
            CreatePageTiles();
        }
        private bool IsPageSideWays()
        {
            return RotateAngle % 180 != 0;
        }
        private Rect TransformRectByPageParam(Rect rect, Datalogics.PDFL.Point scale)
        {
            Matrix tMatrix = new Matrix();
            double xShift = rect.LLx + rect.Width / 2.0;
	        double yShift = rect.LLy + rect.Height / 2.0;

            double xShiftModified = xShift * scale.H;
            double yShiftModified = yShift * scale.V;
            tMatrix = (IsPageSideWays()) ? tMatrix.Translate(yShiftModified, xShiftModified) : tMatrix.Translate(xShiftModified, yShiftModified);
            tMatrix = tMatrix.Rotate(RotateAngle).Scale(scale.H, -scale.V);

            tMatrix = tMatrix.Translate(-xShift, -yShift);
            return rect.Transform(tMatrix);
        }        

        private void CreatePageTiles()
        {
            int cols = (CropRect.Width + TileSize.Width - 1) / TileSize.Width;
            int rows = (CropRect.Height + TileSize.Height - 1) / TileSize.Height;
		    tiles = new Tile[rows,cols];

            for (int r = 0, yOff = CropRect.Y; r < rows; ++r, yOff += TileSize.Height)
            {
                int tileW = (IsPageSideWays()) ? TileSize.Height : TileSize.Width;
                int tileH = (IsPageSideWays()) ? TileSize.Width : TileSize.Height;
    		
			    if (r == rows - 1)
                    tileH = Math.Min(tileH, (int)CropRect.Height - yOff);

                for (int c = 0, xOff = CropRect.X; c < cols; ++c, xOff += TileSize.Width)
                {
				    if (c == cols - 1)
                        tileW = Math.Min(tileW, (int)CropRect.Width - xOff);
				    tiles[r,c] = CreateTile(new Rectangle(xOff, yOff, tileW, tileH), null);
			    }
		    }
        }
        private Tile CreateTile(Rectangle rect, Bitmap bmp)
        {
            Tile tile = new Tile();
            tile.TileRect = rect;
            tile.TileBitmap = bmp;
            return tile;
        }
        #endregion
        #region Properties
        public Size TileSize
        {
            get { return tileSize; }
            set { tileSize = value; }
        }
        public Rectangle CropRect
        {
            get { return cropRect; }
            set { cropRect = value; }
        }
        public float RotateAngle{get { return rotateAngle; }}
        private void SetPageRotate(PageRotation pr)
        {
            switch (pr)
            {
                case PageRotation.Rotate0: rotateAngle = 0.0F; break;
                case PageRotation.Rotate90: rotateAngle = 90.0F; break;
                case PageRotation.Rotate180: rotateAngle = 180.0F; break;
                case PageRotation.Rotate270: rotateAngle = 270.0F; break;
                default: rotateAngle = 0; break;
            }
        }
        #endregion
        #region Members
        public Tile[,] tiles;
        public Bitmap fullBitmap;
        private float rotateAngle;
        private Rectangle cropRect;
        private Size tileSize;
        //private Page page;
        private Document doc;
        private int pageNum;
        private OptionalContentContext occ;
        private Datalogics.PDFL.Point scale;
        #endregion
    }

    public class TileManager
    {
        #region Constructor
        public TileManager(System.Windows.Forms.Control control)
        {
            parent = control;
            using (Graphics g = parent.CreateGraphics())
            {
                dpiX = g.DpiX;
                dpiY = g.DpiY;
            }
        }
        #endregion
        #region Properties
        public Document Document
        {
            get { return document; }
            set
            {
                document = value;
                occ = null;
                if (cachedPages != null) foreach (PageCache cache in cachedPages) cache.Clear();
                UpdateFitZooms(new Size(1, 1));
                UpdatePageInfo();
                invalidateCache = true;
                if (document == null) return;
                cachedPages = new PageCache[document.NumPages];
                for (int i = 0; i < document.NumPages; ++i)
                {
                    cachedPages[i] = new PageCache();
                }
            }
        }
        public OptionalContentContext OptionalContentContext
        {
            get { return occ; }
            set
            {
                occ = value;
                invalidateCache = true;
            }
        }
        public PageInfo[] PageInfo
        {
            get { return pageInfo; }
        }
        public Size ViewSize
        {
            get { return viewSize; }
        }
        // set zoom (1 = 100%)
        public double Zoom
        {
            get { return GetPageZoom(CurrentPage); }
            set
            {
                if (value == zoom && fit == FitModes.FitNone) return;

                fit = FitModes.FitNone;
                zoom = value;
                fitZooms = null;
                UpdatePageInfo();
                invalidateCache = true;
            }
        }
        public FitModes Fit
        {
            get { return fit; }
            set
            {
                if (fit == value) return;

                fit = value;
                zoom = 1.0;
                UpdateFitZooms(new Size(1, 1));
            }
        }
        // set indent between pages, in pixels
        public int PageIndent
        {
            get { return pageIndent; }
            set
            {
                if (value == pageIndent) return;
                pageIndent = value;
                UpdatePageInfo();
                invalidateCache = true;
            }
        }
        public System.Drawing.Color BackgroundColor
        {
            get { return backgroundColor; }
            set
            {
                if (value == backgroundColor) return;
                backgroundColor = value;
                invalidateCache = true;
            }
        }
        // set draw mode to single page display (true) or continuous page display (false)
        public bool SinglePageMode
        {
            get { return singlePageMode; }
            set
            {
                if (value == singlePageMode) return;
                singlePageMode = value;
                invalidateCache = true;
            }
        }
        public System.Drawing.Point Scroll
        {
            get { return scroll; }
            set
            {
                if (scroll == value) return;
                scroll = value;
            }
        }
        public int CurrentPage
        {
            get { return currentPage; }
            set
            {
                if (currentPage == value) return;
                currentPage = value;

                if (SinglePageMode)
                    invalidateCache = true;
            }
        }
        public double DPIScaleX { get { return dpiX / GlobalConsts.pdfDPI; } }
        public double DPIScaleY { get { return dpiY / GlobalConsts.pdfDPI; } }
        public double DpiX
        {
            get { return dpiX; }
            set
            {
                dpiX = value;
                UpdatePageInfo();
                invalidateCache = true;
            }
        }
        public double DpiY
        {
            get { return dpiY; }
            set
            {
                dpiY = value;
                UpdatePageInfo();
                invalidateCache = true;
            }
        }
        public double RotateAngle { get { return cachedPages[CurrentPage].RotateAngle; } }
        #endregion
        #region Methods
        public void Draw(Graphics g, Rectangle clipRect)
        {
            g.FillRectangle(new SolidBrush(backgroundColor), clipRect);
            if (invalidateCache)
            {
                for (int i = 0; i < document.NumPages; ++i)
                {
                    cachedPages[i].Clear();
                }
                invalidateCache = false;
            }
            int start = 0;
            int end = document.NumPages;
            if (singlePageMode)
            {
                start = currentPage;
                end = start + 1;
            }

            for (int i = 0; i < document.NumPages; ++i)
            {
                Rectangle pageRect = pageInfo[i].boundingRectangle;
                pageRect.X -= scroll.X;
                pageRect.Y -= scroll.Y;
                if (!pageRect.IntersectsWith(clipRect))
                {
                    int farEnough = 4096;
                    if ((clipRect.Top - pageRect.Bottom > farEnough) || (pageRect.Top - clipRect.Bottom > farEnough))
                    {
                        cachedPages[i].Clear();
                    }
                }
            }
            for (int i = start; i < end; ++i)
            {
                Rectangle pageRect = pageInfo[i].boundingRectangle;
                pageRect.X -= scroll.X;
                pageRect.Y -= scroll.Y;
                if (pageRect.IntersectsWith(clipRect))
                {
                    double zoom = GetPageZoom(i);
                    if (cachedPages[i].fullBitmap == null && cachedPages[i].tiles == null)
                    {
                        using (Page page = document.GetPage(i))
                        {
                            cachedPages[i].Load(document, page, new Datalogics.PDFL.Point(zoom * DPIScaleX, zoom * DPIScaleY), occ);
                        }
                    }
                    if (cachedPages[i].fullBitmap != null && cachedPages[i].tiles[0, 0].TileBitmap == null) // if page is small, then contents was rendered into single bitmap
                    {
                        g.DrawImage(cachedPages[i].fullBitmap, pageRect.Location);
                    }
                    else // if page is large, then contents was rendered into tile array
                    {

                        for (int y = 0; y < cachedPages[i].tiles.GetLength(1); y++)
                        {
                            for (int x = 0; x < cachedPages[i].tiles.GetLength(0); x++)
                            {
                                Tile tile = cachedPages[i].tiles[x, y];
                                Rectangle pRect = new Rectangle(0, 0, pageInfo[i].boundingRectangle.Width, pageInfo[i].boundingRectangle.Height);
                                pRect.Intersect(new Rectangle(scroll.X - pageInfo[i].boundingRectangle.X, scroll.Y - pageInfo[i].boundingRectangle.Y,
                                    clipRect.Width, clipRect.Height));

                                Rectangle tRect = tile.TileRect;
                                tRect.X -= cachedPages[i].CropRect.X;
                                tRect.Y -= cachedPages[i].CropRect.Y;
                                
                                if (pRect.IntersectsWith(tRect))
                                {
                                    if (!tile.IsValid)
                                        tile = cachedPages[i].tiles[x, y] = cachedPages[i].Create(tile.TileRect);

                                    System.Drawing.Point tileLocation = new System.Drawing.Point(tRect.X, tRect.Y);

                                    tileLocation.X += pageRect.Location.X;
                                    tileLocation.Y += pageRect.Location.Y;

                                    g.DrawImage(tile.TileBitmap, tileLocation);
                                }
                            }
                        }
                    }
                }
            }
        }
        public void InvalidateCache()
        {   
            invalidateCache = true;
        }
        private void UpdatePageInfo()
        {
            viewSize = new Size(0, 0);
            int pageCount = 0;
            if (document != null)
            {
                pageCount = document.NumPages;
            }
            if (pageCount == 0)
            {
                pageInfo = null;
                return;
            }

            if (pageInfo == null || pageInfo.GetLength(0) != pageCount)
            {
                pageInfo = new PageInfo[pageCount];
            }
            int totalHeight = pageIndent;
            int maxWidth = 0;
            for (int i = 0; i < pageCount; ++i)
            {
                using (Page page = document.GetPage(i))
                {
                    Rectangle pageRect = PageViewRect(page);
                    pageRect.Y = totalHeight;
                    pageInfo[i].boundingRectangle = pageRect;
                    totalHeight += pageRect.Height + pageIndent;
                    maxWidth = Math.Max(maxWidth, pageRect.Width);
                }
            }

            viewSize.Width = maxWidth + 2 * pageIndent;
            viewSize.Height = totalHeight;

            for (int i = 0; i < pageCount; ++i)
            {
                pageInfo[i].boundingRectangle.X += pageIndent;
            }
        }
        public double GetPageZoom(int index)
        {
            return (Fit != FitModes.FitNone && index < document.NumPages) ? fitZooms[index] : zoom;
        }
        public void UpdateFitZooms(Size viewSize)
        {
            if (fit != FitModes.FitNone && document != null && document.NumPages != 0)
            {
                viewSize.Width = Math.Max(viewSize.Width - 2 * PageIndent, 1);
                viewSize.Height = Math.Max(viewSize.Height - 2 * PageIndent, 1);

                fitZooms = new double[document.NumPages];
                for (int i = 0; i < document.NumPages; ++i) // assume that we have pages with zoom 1.0
                    fitZooms[i] = 1.0;

                for (int i = 0; i < document.NumPages; ++i)
                {
                    Size pageSize;
                    using (Page page = document.GetPage(i)) pageSize = PageViewSize(page);

                    double xRatio = viewSize.Width / (double)pageSize.Width;
                    double yRatio = viewSize.Height / (double)pageSize.Height;
				    double pageRatio = Math.Min(xRatio, yRatio); // fit page by default

                    if (fit == FitModes.FitWidth)
					    pageRatio = xRatio;

                    fitZooms[i] = pageRatio;
                }
                UpdatePageInfo();
                invalidateCache = true;
            }
        }
        public Size PageViewSize(Page page)
        {
            double scaleX;
            double scaleY;
           
            scaleX = GetPageZoom(page.PageNumber) * DPIScaleX;
            scaleY = GetPageZoom(page.PageNumber) * DPIScaleY;
           
            double x = page.CropBox.Width;
            double y = page.CropBox.Height;
            if (page.Rotation == PageRotation.Rotate90 || page.Rotation == PageRotation.Rotate270)
            {
                // swap x, y
                double t = x;
                x = y;
                y = t;
            }
            return new Size((int)(x * scaleX), (int)(y * scaleY));
        }
        public Rectangle PageViewRect(Page page)
        {
            return new Rectangle(new System.Drawing.Point(0, 0), PageViewSize(page));
        }
        public int GetPageByCoord(System.Drawing.Point coord)
        {
            if (lastUsedPageIndex >= 0 &&
                lastUsedPageIndex < document.NumPages &&
                pageInfo[lastUsedPageIndex].boundingRectangle.Contains(coord))
            {
                return lastUsedPageIndex;
            }

            lastUsedPageIndex = -1;
            for (int i =0; i < document.NumPages; ++i)
            {
                if (pageInfo[i].boundingRectangle.Contains(coord))
                {
                    lastUsedPageIndex = i;
                    return lastUsedPageIndex;
                }
            }

            return lastUsedPageIndex;
        }
        public int GetPageByY(int y)
        {
            int nearestDistance = ViewSize.Height; // distance from page center Y to view center Y
            for (int i = 0, length = pageInfo != null ? pageInfo.GetLength(0): 0; i < length; ++i)
            {
                Rectangle rect = pageInfo[i].boundingRectangle;
                int distance = Math.Min(Math.Abs(rect.Top - y), Math.Abs(rect.Bottom - y));
                if (distance > nearestDistance) return i - 1;
                else nearestDistance = distance;
            }
            return pageInfo != null ? (pageInfo.GetLength(0) - 1): 0;
        }
        #endregion
        #region Members
        Control parent;
        // display options
        private double zoom = 1;     // 1 == 100%
        private double[] fitZooms;   // array for zooms in fit modes
        private FitModes fit = FitModes.FitNone;
        private int pageIndent = 10; // pixels between pages
        private bool singlePageMode = true;
        private System.Drawing.Color backgroundColor = System.Drawing.Color.FromArgb(96, 96, 96);

        // document information
        private Document document;
        private OptionalContentContext occ;
        private PageInfo[] pageInfo;
        private Size viewSize;

        // navigation
        private System.Drawing.Point scroll;
        // cached values to improve performance
        int lastUsedPageIndex = 0;

        // drawing details
        private PageCache[] cachedPages;
        private bool invalidateCache = true;
        private int currentPage;

        private double dpiX;
        private double dpiY;
        #endregion
    }

}