using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Datalogics.PDFL;

namespace DotNETViewerComponent
{
    /**
     * Flags describing guide layout, used for guides on bounding rect
     */
    [Flags]
    public enum GuideLayout
    {
        Nothing = 0,
        N = 1,
        E = 2,
        S = 4,
        W = 8,
        WE = E | W,
        NE = N | E,
        NW = N | W,
        NS = N | S,
        SE = S | E,
        SW = S | W,

        All = N | E | S | W
    }

    /**
     * Guide rectangle: helper class for annotation moving and reshaping
     */
    public class GuideRect
    {
        #region Constructors
        public GuideRect(int i, BaseAnnotationEditor ann, Datalogics.PDFL.Point loc, GuideLayout lay, Cursor c)
        {
            index = i;
            annotation = ann;
            location = new Datalogics.PDFL.Point(loc.H, loc.V);
            layout = lay;
            cursor = c;
        }
        #endregion

        #region Properties
        public Datalogics.PDFL.Point Location
        {
            get { return location; }
            set
            {
                if (location == value) return;
                location = value;
            }
        }
        public GuideLayout Layout
        {
            get { return layout; }
            set { layout = value; }
        }
        public int Index
        {
            get { return index; }
            set { index = value; }
        }
        public Cursor Cursor
        {
            get { return cursor; }
            set { cursor = value; }
        }
        #endregion

        #region Methods
        /**
         * Returns cursor for guide for given page rotation
         */
        public Cursor CursorForRotation(PageRotation rotation)
        {
            GuideLayout newLayout = GuideLayout.Nothing;
            // transform guide layout using page rotation
            switch (rotation)
            {
                case PageRotation.Rotate0:
                    newLayout = layout; break;
                case PageRotation.Rotate90:
                    if ((layout & GuideLayout.N) != 0) newLayout |= GuideLayout.E;
                    if ((layout & GuideLayout.E) != 0) newLayout |= GuideLayout.S;
                    if ((layout & GuideLayout.S) != 0) newLayout |= GuideLayout.W;
                    if ((layout & GuideLayout.W) != 0) newLayout |= GuideLayout.N;
                    break;
                case PageRotation.Rotate180:
                    if ((layout & GuideLayout.N) != 0) newLayout |= GuideLayout.S;
                    if ((layout & GuideLayout.E) != 0) newLayout |= GuideLayout.W;
                    if ((layout & GuideLayout.S) != 0) newLayout |= GuideLayout.N;
                    if ((layout & GuideLayout.W) != 0) newLayout |= GuideLayout.E;
                    break;
                case PageRotation.Rotate270:
                    if ((layout & GuideLayout.N) != 0) newLayout |= GuideLayout.W;
                    if ((layout & GuideLayout.E) != 0) newLayout |= GuideLayout.N;
                    if ((layout & GuideLayout.S) != 0) newLayout |= GuideLayout.E;
                    if ((layout & GuideLayout.W) != 0) newLayout |= GuideLayout.S;
                    break;
            }
            switch (newLayout)
            {
                case GuideLayout.N:
                case GuideLayout.S:
                case GuideLayout.NS: return Cursors.SizeNS;

                case GuideLayout.W:
                case GuideLayout.E:
                case GuideLayout.WE: return Cursors.SizeWE;

                case GuideLayout.NE:
                case GuideLayout.SW: return Cursors.SizeNESW;

                case GuideLayout.NW:
                case GuideLayout.SE: return Cursors.SizeNWSE;

                case GuideLayout.All: return Cursors.SizeAll;

                default: return Cursors.Default;
            }
        }
        #endregion

        #region Members
        private BaseAnnotationEditor annotation;
        private Datalogics.PDFL.Point location;
        private GuideLayout layout;
        private int index;
        private Cursor cursor;
        #endregion
    }

    /**
     * Flags used for annotation hit detection
     */
    [Flags]
    public enum HitFlags
    {
        NoHit = 0,
        ShapeHit = 1,
        RectHit = 2,
        BorderHit = 4,
        GuideHit = 8
    }

    /**
     * Class storing information about annotation hit
     */
    public class Hit
    {
        public HitFlags flags;
        public GuideRect guide; // if there was guide hit then guide != null and GuideHit flag is set

        public Hit()
        {
            flags = HitFlags.NoHit;
            guide = null;
        }
    }

    /**
     * BaseAnnotationEditor - base class for classes used to edit different types of annotations.
     * Contains functionality for moving and tranfrorming annotations
     */
    public class BaseAnnotationEditor
    {
        #region Constructors
        public void Init(Datalogics.PDFL.Document doc, Datalogics.PDFL.Page p, Datalogics.PDFL.Annotation ann, int ind)
        {
            document = doc;
            page = p;
            annotation = ann;
            index = ind;
            appearance = new AnnotationAppearance(document, annotation);
        }
        #endregion
        #region Properties
        public GuideRect[] Guides { get { return guideRects; } }
        public Page Page { get { return page; } }
        public int Index { get { return index; } }
        public Annotation Annotation { get { return annotation; } }
        public virtual AnnotationProperties Properties { get { return appearance.Properties; } }
        public virtual float Zoom
        {
            get { return zoom; }
            set { zoom = value; }
        }
        public virtual float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }
        #endregion
        #region Methods
        // temporarily removes annotation from document for editing (to render page without this annotation)
        public virtual void Detach(AnnotationProperties.UpdateCallback update)
        {
            appearance.ReleaseAnnotation();
            appearance.Update -= update;
            appearance.Update -= OnAppearanceUpdate;
        }
        // puts annotation back to document after detaching and editing
        public virtual void Attach(AnnotationProperties.UpdateCallback update)
        {
            appearance.Update += OnAppearanceUpdate;
            appearance.Update += update;
            appearance.CaptureAnnotation();
        }
        public virtual void UpdateBoundingRect()
        {
            appearance.UpdateAppearance();
        }
        public virtual void Transform(Datalogics.PDFL.Matrix matrix)
        {
            Properties.BoundingRect = TransformBoundingRect(matrix);
        }
        public Datalogics.PDFL.Rect TransformBoundingRect(Datalogics.PDFL.Matrix matrix)
        {
            Datalogics.PDFL.Rect newRect = Utils.Transform(Properties.BoundingRect, matrix);

            Datalogics.PDFL.Rect validatedRect = new Datalogics.PDFL.Rect();
            Datalogics.PDFL.Point LL = EnsurePointInPage(new Datalogics.PDFL.Point(newRect.LLx, newRect.LLy));
            validatedRect.LLx = LL.H;
            validatedRect.LLy = LL.V;
            Datalogics.PDFL.Point UR = EnsurePointInPage(new Datalogics.PDFL.Point(newRect.URx, newRect.URy));
            validatedRect.URx = UR.H;
            validatedRect.URy = UR.V;

            if (!validatedRect.Equals(newRect)) newRect = Properties.BoundingRect;

            return newRect;
        }
        public GuideRect MoveGuide(GuideRect guide, Datalogics.PDFL.Point location)
        {
            return doMoveGuide(guide, EnsurePointInPage(location));
        }
        // returns true if point belongs to annotation or guide rectangle (pdf coordinate system)
        public virtual Hit TestHit(Datalogics.PDFL.Point point)
        {
            Hit result = new Hit();
            result.flags = HitFlags.NoHit;
            Rect boundingRect = Properties.BoundingRect;

            // external & internal rects help to detect if point is near the border
            Datalogics.PDFL.Rect externalRect = new Datalogics.PDFL.Rect(
                boundingRect.LLx - borderDelta,
                boundingRect.LLy - borderDelta,
                boundingRect.URx + borderDelta,
                boundingRect.URy + borderDelta);

            Datalogics.PDFL.Rect internalRect = new Datalogics.PDFL.Rect(
                boundingRect.LLx + borderDelta,
                boundingRect.LLy + borderDelta,
                boundingRect.URx - borderDelta,
                boundingRect.URy - borderDelta);

            if (PointInRect(point, boundingRect))
            {
                result.flags |= HitFlags.ShapeHit; // default shape is equal to bounding rect
                result.flags |= HitFlags.RectHit;
            }

            if (PointInRect(point, externalRect) && !PointInRect(point, internalRect))
            {
                result.flags |= HitFlags.BorderHit;
            }

            if (guideRects != null)
            {
                foreach (GuideRect guide in guideRects)
                {
                    Datalogics.PDFL.Point loc = guide.Location;
                    double dx = Math.Abs(loc.H - point.H);
                    double dy = Math.Abs(loc.V - point.V);
                    double distance = Math.Max(dx, dy);
                    if (distance <= guideSize + guideDelta)
                    {
                        result.flags |= HitFlags.GuideHit;
                        result.guide = guide;
                        break;
                    }
                }
            }

            return result;
        }
        // base draw: bounding rectangle, guide rectangles
        // g      - graphics context
        // matrix - transformation matrix from pdf to graphics coords
        public virtual void Draw(Graphics g, Datalogics.PDFL.Matrix matrix)
        {
            DrawShape(g, matrix);
            DrawBoundingRect(g, matrix);
            DrawGuides(g, matrix);
        }
        public void DrawShape(Graphics g, Datalogics.PDFL.Matrix matrix)
        {
            GraphicsState state = g.Save();
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            doDraw(g, matrix);
            g.Restore(state);
        }
        // helper: draws annotation's bounding rectangle
        // g      - graphics context
        // matrix - transformation matrix from pdf to graphics coords
        public void DrawBoundingRect(Graphics g, Datalogics.PDFL.Matrix matrix)
        {
            Rect newRect = Utils.Transform(Properties.BoundingRect, matrix);
            Rectangle rect = new Rectangle((int)newRect.LLx, (int)newRect.LLy, (int)newRect.Width, (int)newRect.Height);
            Pen pen = new Pen(rectColor);
            pen.DashPattern = new float[] { 1, 1 };
            g.DrawRectangle(pen, rect);
        }
        // helper: performs custom draw
        // g      - graphics context
        // matrix - transformation matrix from pdf to graphics coords
        protected virtual GuideRect doMoveGuide(GuideRect guide, Datalogics.PDFL.Point location)
        {
            return MoveBoundingGuide(guide, location);
        }
        protected GuideRect MoveBoundingGuide(GuideRect guide, Datalogics.PDFL.Point location)
        {
            int index = guide.Index;

            bool moveX0 = (guide.Layout & GuideLayout.W) != 0;
            bool moveX1 = (guide.Layout & GuideLayout.E) != 0;
            bool moveY0 = (guide.Layout & GuideLayout.S) != 0;
            bool moveY1 = (guide.Layout & GuideLayout.N) != 0;
            double dx0 = 0;
            double dy0 = 0;
            double dx1 = 0;
            double dy1 = 0;
            if (moveX0) dx0 = location.H - guide.Location.H;
            if (moveY0) dy0 = location.V - guide.Location.V;
            if (moveX1) dx1 = location.H - guide.Location.H;
            if (moveY1) dy1 = location.V - guide.Location.V;

            Rect boundingRect = Properties.BoundingRect;
            double x0 = boundingRect.LLx + dx0;
            double x1 = boundingRect.URx + dx1;
            double y0 = boundingRect.LLy + dy0;
            double y1 = boundingRect.URy + dy1;

            // avoid zero-sizes
            double delta = 0.01;
            if (moveX0 && (x0 > x1 - delta))
            {
                x0 = Math.Max(x0, x1 + delta);
            }
            if (moveX1 && (x0 > x1 - delta))
            {
                x1 = Math.Min(x1, x0 - delta);
            }
            if (moveY0 && (y0 > y1 - delta))
            {
                y0 = Math.Max(y0, y1 + delta);
            }
            if (moveY1 && (y0 > y1 - delta))
            {
                y1 = Math.Min(y1, y0 - delta);
            }

            // horizontal reflection: switch to other guide
            if (x0 > x1)
            {
                switch (index)
                {
                    case 0: index = 2; break;
                    case 2: index = 0; break;
                    case 3: index = 4; break;
                    case 4: index = 3; break;
                    case 5: index = 7; break;
                    case 7: index = 5; break;
                }
            }
            // vertical reflection: switch to other guide
            if (y0 > y1)
            {
                switch (index)
                {
                    case 0: index = 5; break;
                    case 1: index = 6; break;
                    case 2: index = 7; break;
                    case 5: index = 0; break;
                    case 6: index = 1; break;
                    case 7: index = 2; break;
                }
            }

            Rect newRect = new Rect(x0, y0, x1, y1);
            double scaleX = (x1 - x0) / boundingRect.Width;
            double scaleY = (y1 - y0) / boundingRect.Height;
            //if (Math.Abs(scaleX) < 0.001 || Math.Abs(scaleY) < 0.001) return guideRects[index];

            Datalogics.PDFL.Matrix matrix = new Datalogics.PDFL.Matrix()
                .Translate(x0, y0)
                .Scale(scaleX, scaleY)
                .Translate(-boundingRect.LLx, -boundingRect.LLy)
                ;

            Transform(matrix);
            return guideRects[index];
        }
        protected virtual void doDraw(Graphics g, Datalogics.PDFL.Matrix matrix)
        {
        }
        // helper: draws annotation's guide rectangles
        // g      - graphics context
        // matrix - transformation matrix from pdf to graphics coords
        public void DrawGuides(Graphics g, Datalogics.PDFL.Matrix matrix)
        {
            if (guideRects == null) return;

            foreach (GuideRect guide in guideRects)
            {
                Datalogics.PDFL.Point center = guide.Location;
                center = Utils.Transform(center, matrix);
                System.Drawing.Point location = new System.Drawing.Point((int)(center.H) - guideSize / 2, (int)(center.V) - guideSize / 2);
                Rectangle rect = new Rectangle(location, new Size(guideSize, guideSize));
                g.FillRectangle(Brushes.White, rect);
                g.DrawRectangle(new Pen(guideColor), rect);
            }
        }
        protected virtual GuideRect[] CreateGuides()
        {
            return CreateBoundingGuides();
        }
        protected GuideRect[] CreateBoundingGuides()
        {
            Datalogics.PDFL.Rect rect = Properties.BoundingRect;
            double x = rect.LLx;
            double y = rect.LLy;
            double w = rect.Width;
            double h = rect.Height;
            GuideRect[] result = new GuideRect[8];
            result[0] = new GuideRect(0, this, new Datalogics.PDFL.Point(x, y), GuideLayout.SW, Cursors.SizeNESW);
            result[1] = new GuideRect(1, this, new Datalogics.PDFL.Point(x + w / 2, y), GuideLayout.S, Cursors.SizeNS);
            result[2] = new GuideRect(2, this, new Datalogics.PDFL.Point(x + w, y), GuideLayout.SE, Cursors.SizeNWSE);
            result[3] = new GuideRect(3, this, new Datalogics.PDFL.Point(x, y + h / 2), GuideLayout.W, Cursors.SizeWE);
            result[4] = new GuideRect(4, this, new Datalogics.PDFL.Point(x + w, y + h / 2), GuideLayout.E, Cursors.SizeWE);
            result[5] = new GuideRect(5, this, new Datalogics.PDFL.Point(x, y + h), GuideLayout.NW, Cursors.SizeNWSE);
            result[6] = new GuideRect(6, this, new Datalogics.PDFL.Point(x + w / 2, y + h), GuideLayout.N, Cursors.SizeNS);
            result[7] = new GuideRect(7, this, new Datalogics.PDFL.Point(x + w, y + h), GuideLayout.NE, Cursors.SizeNESW);

            return result;
        }
        protected Pen GetPen(Datalogics.PDFL.Matrix matrix)
        {
            Pen pen = new Pen(Properties.ForeColor, (float)(Properties.LineWidth * Zoom));
            if (!Properties.Solid)
            {
                float[] dash = Properties.DashPattern;
                for (int i = 0; i < dash.Length; i++)
                    dash[i] /= Properties.LineWidth;
                pen.DashPattern = dash;
            }
            return pen;
        }
        // catches annotation update events and re-creates move guides depending on new bounding rect
        protected void OnAppearanceUpdate(object obj)
        {
            if (obj == null || !(obj is AnnotationAppearance.AppearanceUpdate)) return;
            AnnotationAppearance.AppearanceUpdate appearanceUpdate = (AnnotationAppearance.AppearanceUpdate)obj;
            if (appearanceUpdate == AnnotationAppearance.AppearanceUpdate.ShapeChanged ||
                appearanceUpdate == AnnotationAppearance.AppearanceUpdate.ObjectHidden)
            {
                if (appearanceUpdate == AnnotationAppearance.AppearanceUpdate.ShapeChanged)
                {
                    UpdateBoundingRect();
                }
                guideRects = CreateGuides();
            }
        }
        // checks that current point is inside of page bounds
        protected Datalogics.PDFL.Point EnsurePointInPage(Datalogics.PDFL.Point point)
        {
            Datalogics.PDFL.Point corrected = new Datalogics.PDFL.Point();

            corrected.H = Math.Max(Math.Min(point.H, page.CropBox.Right), page.CropBox.Left);
            corrected.V = Math.Max(Math.Min(point.V, page.CropBox.Top), page.CropBox.Bottom);
            return corrected;
        }
        // helper: tests if a point belongs to rectangle
        private static bool PointInRect(Datalogics.PDFL.Point point, Datalogics.PDFL.Rect rect)
        {
            return point.H >= rect.LLx && point.H <= rect.URx && point.V >= rect.LLy && point.V <= rect.URy;
        }
        #endregion
        #region Members
        protected AnnotationAppearance appearance;
        protected Datalogics.PDFL.Document document;
        protected Datalogics.PDFL.Page page;
        protected Datalogics.PDFL.Annotation annotation;
        protected GuideRect[] guideRects;
        protected int index;
        private float zoom;
        private float rotation;
        private System.Drawing.Color rectColor = System.Drawing.Color.Red;
        private System.Drawing.Color guideColor = System.Drawing.Color.Red;

        // to detect guide rect hit
        private double guideDelta = 2;
        // to detect border hit
        private double borderDelta = 2;
        private const int guideSize = 4;

        #endregion
    }

    /**
     * CircleAnnotationEditor - edits circle annotations.
     * Overrides doDraw function - draws ellipse.
     */
    public class CircleAnnotationEditor : BaseAnnotationEditor
    {
        #region Methods
        public override void UpdateBoundingRect()
        {
        }
        protected override void doDraw(Graphics g, Datalogics.PDFL.Matrix matrix)
        {
            Rect rc = Utils.Transform(Properties.BoundingRect, matrix);
            Pen pen = GetPen(matrix);
            float width = rc.Width >= pen.Width ? (float)(rc.Width - pen.Width) : 1.0F;
            float height = rc.Height >= pen.Width ? (float)(rc.Height - pen.Width) : 1.0F;
            RectangleF drawRect = new RectangleF((float)(rc.LLx + pen.Width / 2.0F), (float)(rc.LLy + pen.Width / 2.0F), width, height);
            if (Properties.HasFill)
                g.FillEllipse(Properties.FillBrush, drawRect);
            g.DrawEllipse(pen, drawRect);
        }
        #endregion
    }

    /**
     * SquareAnnotationEditor - edits square annotations.
     * Overrides doDraw function - draws rectangle.
     */
    public class SquareAnnotationEditor : BaseAnnotationEditor
    {
        #region Methods
        public override void UpdateBoundingRect()
        {
        }
        protected override void doDraw(Graphics g, Datalogics.PDFL.Matrix matrix)
        {
            Rect rc = Utils.Transform(Properties.BoundingRect, matrix);
            double newWidth = Properties.LineWidth * Zoom;
            RectangleF drawRect = new RectangleF((float)(rc.LLx), (float)(rc.LLy), (float)(rc.Width), (float)(rc.Height));
            if (Properties.HasFill)
                g.FillRectangle(Properties.FillBrush, drawRect);
            g.DrawRectangle(GetPen(matrix), (int)Math.Round(drawRect.X), (int)Math.Round(drawRect.Y), (int)Math.Round(drawRect.Width), (int)Math.Round(drawRect.Height));
        }
        #endregion
    }

    /**
     * LineAnnotationEditor - edits line annotations.
     * Overrides doDraw function - draws line.
     * Overrides transformation and guide functions, because line annotations don't use bounding rectangle
     */
    public class LineAnnotationEditor : BaseAnnotationEditor
    {
        #region Methods
        public override void UpdateBoundingRect()
        {
            Datalogics.PDFL.Point startPoint = Properties.StartPoint;
            Datalogics.PDFL.Point endPoint = Properties.EndPoint;
            Datalogics.PDFL.Rect rect = new Rect(startPoint.H, startPoint.V, endPoint.H, endPoint.V);
            Properties.BoundingRect = rect;
        }
        public override void Transform(Datalogics.PDFL.Matrix matrix)
        {
            if (!Properties.BoundingRect.Equals(TransformBoundingRect(matrix)))
            {
                Properties.StartPoint = EnsurePointInPage(Utils.Transform(Properties.StartPoint, matrix));
                Properties.EndPoint = EnsurePointInPage(Utils.Transform(Properties.EndPoint, matrix));
                guideRects[0].Location = Properties.StartPoint;
                guideRects[1].Location = Properties.EndPoint;
            }
        }
        protected override GuideRect[] CreateGuides()
        {
            GuideRect[] guides = new GuideRect[] {
                new GuideRect(0, this, Properties.StartPoint, GuideLayout.All, Cursors.SizeAll),
                new GuideRect(1, this, Properties.EndPoint, GuideLayout.All, Cursors.SizeAll)};
            return guides;
        }
        protected override GuideRect doMoveGuide(GuideRect guide, Datalogics.PDFL.Point location)
        {
            // only 2 guides are available, which specify two line end points
            // guide is one of vertex guides
            location = EnsurePointInPage(location);
            guideRects[guide.Index].Location = location;
            if (guide.Index == 0) Properties.StartPoint = location;
            else if (guide.Index == 1) Properties.EndPoint = location;
            return guideRects[guide.Index];
        }
        protected override void doDraw(Graphics g, Datalogics.PDFL.Matrix matrix)
        {
            Datalogics.PDFL.Point start = Utils.Transform(Properties.StartPoint, matrix);
            Datalogics.PDFL.Point end = Utils.Transform(Properties.EndPoint, matrix);
            g.DrawLine(GetPen(matrix), new System.Drawing.PointF((float)start.H, (float)start.V), new System.Drawing.PointF((float)end.H, (float)end.V));
        }
        #endregion
    }

    /**
     * PolyannotationEditor - base for PolyLine and Polygon editors.
     */
    public class PolyAnnotationEditor : BaseAnnotationEditor
    {
        #region Methods
        public override void UpdateBoundingRect()
        {
            IList<Datalogics.PDFL.Point> vertices = Properties.Vertices;
            Datalogics.PDFL.Point min = new Datalogics.PDFL.Point(vertices[0].H, vertices[0].V);
            Datalogics.PDFL.Point max = new Datalogics.PDFL.Point(vertices[0].H, vertices[0].V);
            foreach (Datalogics.PDFL.Point vertex in vertices)
            {
                min.H = Math.Min(min.H, vertex.H);
                min.V = Math.Min(min.V, vertex.V);
                max.H = Math.Max(max.H, vertex.H);
                max.V = Math.Max(max.V, vertex.V);
            }
            /*double halfWidth = Properties.LineWidth / 2;
            min.H -= halfWidth;
            min.V -= halfWidth;
            max.H += halfWidth;
            max.V += halfWidth;*/
            Properties.BoundingRect = new Rect(min.H, min.V, max.H, max.V);
        }
        public override void Transform(Datalogics.PDFL.Matrix matrix)
        {
            if (!Properties.BoundingRect.Equals(TransformBoundingRect(matrix)))
            {
                IList<Datalogics.PDFL.Point> vertices = Properties.Vertices;
                for (int i = 0; i < vertices.Count; ++i)
                {
                    vertices[i] = Utils.Transform(vertices[i], matrix);
                    guideRects[8 + i].Location = vertices[i];
                }
                Properties.Vertices = vertices;
            }
        }
        public PointF[] GetVerticesInSystemCoordinates(Datalogics.PDFL.Matrix matrix)
        {
            PointF[] polyPoints = new PointF[Properties.Vertices.Count];
            for (int i = 0; i < Properties.Vertices.Count; i++)
            {
                Datalogics.PDFL.Point point = Utils.Transform(Properties.Vertices[i], matrix);
                polyPoints[i].X = (float)point.H;
                polyPoints[i].Y = (float)point.V;
            }
            return polyPoints;
        }
        protected override GuideRect[] CreateGuides()
        {
            GuideRect[] guides = base.CreateGuides();
            int len = guides.Length;
            IList<Datalogics.PDFL.Point> vertices = Properties.Vertices;
            int vCount = vertices.Count;
            Array.Resize(ref guides, len + vCount);
            for (int i = len; i < len + vCount; ++i)
            {
                guides[i] = new GuideRect(i, this, vertices[i - len], GuideLayout.All, Cursors.SizeAll);
            }
            return guides;
        }
        protected override GuideRect doMoveGuide(GuideRect guide, Datalogics.PDFL.Point location)
        {
            // first 8 guides is bounding guides, after it there are vertex guides
            // if it is bounding guide then just call base method
            if (guide.Index < 8) return base.doMoveGuide(guide, location);

            // guide is one of vertex guides
            location = EnsurePointInPage(location);
            IList<Datalogics.PDFL.Point> vertices = Properties.Vertices;
            int vIndex = guide.Index - 8;
            guideRects[guide.Index].Location = location;
            vertices[vIndex] = location;
            Properties.Vertices = vertices;
            return guideRects[guide.Index];
        }
        #endregion
    }

    /**
     * PolyLineAnnotationEditor - edits polyline annotations.
     * Overrides doDraw function - draws polyline.
     */
    public class PolyLineAnnotationEditor : PolyAnnotationEditor
    {
        #region Methods
        protected override void doDraw(Graphics g, Datalogics.PDFL.Matrix matrix)
        {
            g.DrawLines(GetPen(matrix), GetVerticesInSystemCoordinates(matrix));
        }
        #endregion
    }

    /**
     * PlygonAnnotationEditor - edits polygon annotations.
     * Overrides doDraw function - draws polygon.
     */
    public class PolygonAnnotationEditor : PolyAnnotationEditor
    {
        #region Methods
        protected override void doDraw(Graphics g, Datalogics.PDFL.Matrix matrix)
        {
            if (Properties.HasFill)
            {
                // Default is FillMode.Alternate.  In order to match the Fill mode during the GenerateAppearance
                // call in the Interface code, this needs to be set to Winding.  Otherwise, in edit mode, the fill
                // will be different for polygons with crossing lines versus the non-edit mode.
                g.FillPolygon(Properties.FillBrush, GetVerticesInSystemCoordinates(matrix), FillMode.Winding);
            }
            g.DrawPolygon(GetPen(matrix), GetVerticesInSystemCoordinates(matrix));
        }
        #endregion
    }

    /**
     * FreeTextAnnotationEditor - edits free text annotations.
     * Overrides doDraw function - draws annotation's text.
     */
    public class FreeTextAnnotationEditor : BaseAnnotationEditor
    {
        #region Methods
        private String[] splitString(String[] splitString, System.Drawing.Font font, int boundWidth, Graphics g)
        {
            List<String> stringList = new List<String>();
            foreach (string s in splitString)
            {
                bool newSplitPart = true;
                int currSymbol = 0;
                double accumulatedLen = 0;
                string substr = s;
                while (currSymbol < substr.Length)
                {
                    SizeF sz = g.MeasureString(substr.Substring(currSymbol, 1), font);
                    accumulatedLen += sz.Width * 72.0 / g.DpiX; // MeasureString() returns result in pixels, so translate it into the points.
                    if (accumulatedLen > boundWidth)
                    {
                        if (currSymbol != 0)
                            stringList.Add(substr.Substring(0, currSymbol));
                        else // if even one symbol can't be drawn within current bounds, just skip it
                            currSymbol = 1;

                        // prepare conditions for the next splitting iteration
                        substr = substr.Substring(currSymbol);
                        currSymbol = 0;
                        accumulatedLen = 0.0;
                        newSplitPart = false;
                    }
                    else
                    {
                        ++currSymbol;
                    }
                }
                if (newSplitPart || substr.Length != 0) // add the last piece which left or the whole new split part (even if it's empty)
                    stringList.Add(substr);
            }
            return stringList.ToArray();
        }

        protected override void doDraw(Graphics g, Datalogics.PDFL.Matrix matrix)
        {
            FontStyle[] possibleStyles = new FontStyle[] { FontStyle.Regular, FontStyle.Italic, FontStyle.Bold, FontStyle.Italic | FontStyle.Bold };
            System.Drawing.Font drawFont = null;
            try
            {
                foreach (FontStyle style in possibleStyles)
                {
                    try
                    {
                        drawFont = new System.Drawing.Font(Properties.FontFace, (float)/*Math.Ceiling*/(Zoom * Properties.FontSize), style, GraphicsUnit.Point);
                        

                        Rect trRect = Utils.Transform(Properties.BoundingRect, matrix);
                        Rectangle rect = new Rectangle((int)Math.Round(trRect.LLx), (int)Math.Round(trRect.LLy), (int)Math.Round(trRect.Width), (int)Math.Round(trRect.Height));
                        if (Properties.HasFill)
                            g.FillRectangle(Properties.FillBrush, rect);
                        System.Drawing.Point location = new System.Drawing.Point();
                        if (Rotation % 180 != 0)
                        {
                            location.X = Rotation == 90 ? rect.X + rect.Width : rect.X;
                            location.Y = Rotation == 90 ? rect.Y : rect.Y + rect.Height;

                            int h = rect.Height;
                            rect.Height = rect.Width;
                            rect.Width = h;
                        }
                        else
                        {
                            location.X = Rotation == 180 ? rect.X + rect.Width : rect.X;
                            location.Y = Rotation == 180 ? rect.Y + rect.Height : rect.Y;
                        }
                        g.TranslateTransform(location.X, location.Y);
                        g.RotateTransform(Rotation);

                        String[] lines = Properties.Contents.Split(new char[] { '\n', '\r' });
                        String[] strs = splitString(lines, drawFont, Properties.BoundingRect.RoundedWidth, g);

                        int y = 0;
                        int x = 0;

                        if (Properties.Quadding == TextFormatFlags.Right)
                            x = rect.Width;
                        if (Properties.Quadding == TextFormatFlags.HorizontalCenter)
                            x = rect.Width / 2;
                        foreach (string str in strs)
                        {
                            if (y < rect.Height)
                            {
                                g.DrawString(str, drawFont, new SolidBrush(Properties.TextColor), x, y, Properties.Alignment);
                                y += (int)g.MeasureString(str, drawFont).Height;
                            }
                        }

                        g.ResetTransform();
                        break;
                    }
                    catch (ArgumentException)
                    {
                        // possible exception if font can't be rendered with specified style
                    }
                }
            }
            finally
            {
                if (drawFont != null)
                    ((IDisposable)drawFont).Dispose();
            }
        }
        #endregion
    }

    /**
     * LinkAnnotationEditor - edits linkannotations.
     * Overrides doDraw function - draws link.
     */
    public class LinkAnnotationEditor : BaseAnnotationEditor
    {
        #region Methods
        protected override void doDraw(Graphics g, Datalogics.PDFL.Matrix matrix)
        {
            Rect rc = Utils.Transform(Properties.BoundingRect, matrix);
            Rectangle drawRect = new Rectangle((int)(rc.LLx), (int)(rc.LLy), (int)(rc.Width), (int)(rc.Height));
            if (Properties.ShowBorder)
                g.DrawRectangle(GetPen(matrix), drawRect);
        }
        #endregion
    }

    /**
     * InkAnnotationEditor - edits ink annotations.
     * Overrides doDraw function - draws scribbles.
     */
    public class InkAnnotationEditor : BaseAnnotationEditor
    {
        #region Methods
        public override void UpdateBoundingRect()
        {
            IList<IList<Datalogics.PDFL.Point>> scribbles = Properties.Scribbles;
            Datalogics.PDFL.Point min = new Datalogics.PDFL.Point(Page.CropBox.URx, Page.CropBox.URy);
            Datalogics.PDFL.Point max = new Datalogics.PDFL.Point(Page.CropBox.LLx, Page.CropBox.LLy);
            foreach (IList<Datalogics.PDFL.Point> scribble in scribbles)
            {
                foreach (Datalogics.PDFL.Point vertex in scribble)
                {
                    min.H = Math.Min(min.H, vertex.H);
                    min.V = Math.Min(min.V, vertex.V);
                    max.H = Math.Max(max.H, vertex.H);
                    max.V = Math.Max(max.V, vertex.V);
                }
            }
            Properties.BoundingRect = new Rect(min.H, min.V, max.H, max.V);
        }
        public override void Transform(Datalogics.PDFL.Matrix matrix)
        {
            if (!Properties.BoundingRect.Equals(TransformBoundingRect(matrix)))
            {
                IList<IList<Datalogics.PDFL.Point>> scribbles = Properties.Scribbles;
                if (scribbles == null) return;
                foreach(IList<Datalogics.PDFL.Point> scribble in scribbles)
                {
                    for (int i = 0; i < scribble.Count; ++i)
                    {
                        scribble[i] = Utils.Transform(scribble[i], matrix);
                    }
                }
                Properties.Scribbles = scribbles;
            }
        }
        protected override void doDraw(Graphics g, Datalogics.PDFL.Matrix matrix)
        {
            IList<IList<Datalogics.PDFL.Point>> scribbles = Properties.Scribbles;

            foreach (IList<Datalogics.PDFL.Point> scribble in scribbles)
            {
                PointF[] pointSet = new PointF[scribble.Count];
                for (int j = 0; j < scribble.Count; ++j)
                {
                    Datalogics.PDFL.Point p = Utils.Transform(scribble[j], matrix);
                    pointSet[j] = new PointF((float)p.H, (float)p.V);
                }
                if (pointSet.Length > 1) g.DrawLines(GetPen(matrix), pointSet);
            }
        }
        #endregion
    }

    /**
     * MarkupAnnotationEditor - base for Underline and Highlight editors.
     * Overrides UpdateBoundingRect function - calculates bounding rect using quad information.
     * Overrides doDraw function - draws selected quads
     */
    public class MarkupAnnotationEditor : BaseAnnotationEditor
    {
        #region Constructors
        public MarkupAnnotationEditor()
        {
            quadColor = System.Drawing.Color.FromArgb(128, 0, 0, 128);
        }
        public MarkupAnnotationEditor(System.Drawing.Color color)
        {
            quadColor = color;
        }
        #endregion
        #region Properties
        public IList<Quad> PageQuads
        {
            get { return pageQuads; }
            set { pageQuads = value; }
        }
        #endregion
        #region Methods
        public override void Transform(Datalogics.PDFL.Matrix matrix)
        {
        }
        protected override GuideRect[] CreateGuides()
        {
            IList<Quad> quads = Properties.Quads;
            int count = quads.Count;
            if (count > 0)
            {
                GuideRect[] guides = new GuideRect[] {
                    new GuideRect(0, this, quads[0].TopLeft, GuideLayout.All, Cursors.SizeAll),
                    new GuideRect(1, this, quads[count-1].BottomRight, GuideLayout.All, Cursors.SizeAll)};
                return guides;
            }
            else
            {
                Datalogics.PDFL.Rect rect = Properties.BoundingRect;
                Datalogics.PDFL.Point startPoint = new Datalogics.PDFL.Point(rect.Left, rect.Top);
                Datalogics.PDFL.Point endPoint = new Datalogics.PDFL.Point(rect.Right, rect.Bottom);
                GuideRect[] guides = new GuideRect[] {
                    new GuideRect(0, this, startPoint, GuideLayout.All, Cursors.SizeAll),
                    new GuideRect(1, this, endPoint, GuideLayout.All, Cursors.SizeAll)};
                return guides;
            }
        }
        protected override GuideRect doMoveGuide(GuideRect guide, Datalogics.PDFL.Point location)
        {
            // only 2 guides are available, which specify selection end points
            int index = guide.Index;
            location = EnsurePointInPage(location);
            guideRects[guide.Index].Location = location;
            Datalogics.PDFL.Point startPoint = guideRects[0].Location;
            Datalogics.PDFL.Point endPoint = guideRects[1].Location;
            Datalogics.PDFL.Point start = startPoint;
            Datalogics.PDFL.Point end = endPoint;
            if (startPoint.V < endPoint.V || (startPoint.V == endPoint.V && startPoint.H > endPoint.H))
            {
                start = endPoint;
                end = startPoint;
                index = 1 - index;
            }
            if (pageQuads != null)
            {
                IList<Quad> quads = new List<Quad>();
                const double delta = 1;
                foreach (Quad quad in pageQuads)
                {
                    Datalogics.PDFL.Rect quadRect = Utils.Rect(quad);
                    double middle = (quadRect.Top + quadRect.Bottom) / 2;
                    if ((middle > end.V) && (middle < start.V))
                    {
                        bool add = true;
                        if (quadRect.Top > start.V - delta) add &= quadRect.Right > start.H;
                        if (quadRect.Bottom < end.V + delta) add &= quadRect.Left < end.H;

                        if (add) quads.Add(quad);
                    }
                }
                if (quads.Count > 0)
                {
                    Properties.Quads = quads;
                    guideRects[0].Location = new Datalogics.PDFL.Point(
                        quads[0].TopLeft.H, quads[0].TopLeft.V);
                    guideRects[1].Location = new Datalogics.PDFL.Point(
                        quads[quads.Count - 1].BottomRight.H, quads[quads.Count - 1].BottomRight.V);
                }
            }

            return guideRects[index];
        }
        public override void UpdateBoundingRect()
        {
            IList<Quad> quads = Properties.Quads;
            Datalogics.PDFL.Point min = new Datalogics.PDFL.Point(quads[0].BottomLeft.H, quads[0].BottomLeft.V);
            Datalogics.PDFL.Point max = new Datalogics.PDFL.Point(quads[0].TopRight.H, quads[0].TopRight.V);
            foreach (Quad quad in quads)
            {
                min.H = Math.Min(min.H, quad.BottomLeft.H);
                min.V = Math.Min(min.V, quad.BottomLeft.V);
                max.H = Math.Max(max.H, quad.TopRight.H);
                max.V = Math.Max(max.V, quad.TopRight.V);
            }
            Properties.BoundingRect = new Rect(min.H, min.V, max.H, max.V);
        }
        protected override void doDraw(Graphics g, Datalogics.PDFL.Matrix matrix)
        {
            IList<Quad> quads = Properties.Quads;
            PointF[] points = new PointF[4];
            foreach (Quad quad in quads)
            {
                Datalogics.PDFL.Point p;
                p = Utils.Transform(quad.BottomLeft, matrix);
                points[0] = new PointF((float)p.H, (float)p.V);
                p = Utils.Transform(quad.BottomRight, matrix);
                points[1] = new PointF((float)p.H, (float)p.V);
                p = Utils.Transform(quad.TopRight, matrix);
                points[2] = new PointF((float)p.H, (float)p.V);
                p = Utils.Transform(quad.TopLeft, matrix);
                points[3] = new PointF((float)p.H, (float)p.V);
                g.FillPolygon(new SolidBrush(quadColor), points);
            }
        }
        #endregion
        #region Members
        private IList<Quad> pageQuads;
        private System.Drawing.Color quadColor;
        #endregion
    }

    public class GroupAnnotationEditor : BaseAnnotationEditor
    {
        #region Constructors
        public GroupAnnotationEditor(Page p): this(p, null)
        {
        }
        public GroupAnnotationEditor(Page p, IList<BaseAnnotationEditor> group)
        {
            page = p;
            index = -1;
            properties = new AnnotationProperties();
            AddPropertiesCallbacks();
            editors = group;
            if (editors == null) editors = new List<BaseAnnotationEditor>();
            SyncProperties();
        }
        #endregion
        #region Properties
        public override AnnotationProperties Properties { get { return properties; } }
        public int Count { get { return editors.Count; } }
        public override float Zoom
        {
            get
            {
                return base.Zoom;
            }
            set
            {
                base.Zoom = value;
                foreach (BaseAnnotationEditor editor in editors)
                {
                    editor.Zoom = value;
                }
            }
        }
        public override float Rotation
        {
            get
            {
                return base.Rotation;
            }
            set
            {
                base.Rotation = value;
                foreach (BaseAnnotationEditor editor in editors)
                {
                    editor.Rotation = value;
                }
            }
        }
        #endregion
        #region Methods
        private void SyncProperties()
        {
            if (editors.Count == 1)
            {
                properties.SetFrom(editors[0].Properties);
                if (properiesUpdateCallback != null) properiesUpdateCallback(AnnotationAppearance.AppearanceUpdate.ShapeChanged);
            }
            CalcBoundingRect();
        }
        public override void Attach(AnnotationProperties.UpdateCallback update)
        {
            properiesUpdateCallback = update;
            foreach (BaseAnnotationEditor editor in editors)
            {
                editor.Attach(update);
            }
        }
        public override void Detach(AnnotationProperties.UpdateCallback update)
        {
            properiesUpdateCallback = null;
            foreach (BaseAnnotationEditor editor in editors)
            {
                editor.Detach(update);
            }
        }
        public void Set(BaseAnnotationEditor editor)
        {
            editors.Clear();
            Add(editor);
        }
        public BaseAnnotationEditor Get(int index)
        {
            return editors[index];
        }
        public void Add(BaseAnnotationEditor editor) // list of editors must be sorted by anotation index
        {
            if ((editor == null) || editors.Contains(editor)) return;
            int index = 0;
            while ((index < editors.Count) && (editors[index].Index < editor.Index)) ++index;
            editors.Insert(index, editor);
            SyncProperties();
        }
        public void Remove(BaseAnnotationEditor editor)
        {
            if (!editors.Contains(editor)) return;
            editors.Remove(editor);
            SyncProperties();
        }
        public void Invert(BaseAnnotationEditor editor)
        {
            if (editors.Contains(editor)) Remove(editor);
            else Add(editor);
        }
        public void Clear()
        {
            editors.Clear();
            SyncProperties();
        }
        public bool Contains(BaseAnnotationEditor editor)
        {
            return editors.Contains(editor);
        }
        public void Remove()
        {
            for (int i = editors.Count - 1; i >= 0; --i)
            {
                editors[i].Page.RemoveAnnotation(editors[i].Index);
            }
        }
        private void AddPropertiesCallbacks()
        {
            properties.Update += OnAppearanceUpdate;
            properties.Update += OnPropertisUpdate;
        }
        private void RemovePropertiesCallbacks()
        {
            properties.Update -= OnPropertisUpdate;
            properties.Update -= OnAppearanceUpdate;
        }
        private void CalcBoundingRect()
        {
            RemovePropertiesCallbacks();
            properties.BoundingRect = CalcBoundingRect(editors);
            properties.Dirty = false;
            AddPropertiesCallbacks();
        }
        private Rect CalcBoundingRect(IList<BaseAnnotationEditor> editors)
        {
            Rect r = new Rect();
            if (editors.Count > 0)
            {
                r = editors[0].Properties.BoundingRect;
                double x0 = r.LLx;
                double y0 = r.LLy;
                double x1 = r.URx;
                double y1 = r.URy;
                foreach (BaseAnnotationEditor editor in editors)
                {
                    r = editor.Properties.BoundingRect;
                    x0 = Math.Min(x0, r.LLx);
                    y0 = Math.Min(y0, r.LLy);
                    x1 = Math.Max(x1, r.URx);
                    y1 = Math.Max(y1, r.URy);
                }
                r = new Rect(x0, y0, x1, y1);
            }
            return r;
        }
        public override void Transform(Datalogics.PDFL.Matrix matrix)
        {
            Datalogics.PDFL.Rect newRect = Utils.Transform(Properties.BoundingRect, matrix);

            Datalogics.PDFL.Rect validatedRect = new Datalogics.PDFL.Rect();
            Datalogics.PDFL.Point LL = EnsurePointInPage(new Datalogics.PDFL.Point(newRect.LLx, newRect.LLy));
            validatedRect.LLx = LL.H;
            validatedRect.LLy = LL.V;
            Datalogics.PDFL.Point UR = EnsurePointInPage(new Datalogics.PDFL.Point(newRect.URx, newRect.URy));
            validatedRect.URx = UR.H;
            validatedRect.URy = UR.V;

            if (validatedRect.Equals(newRect))
            {
                foreach (BaseAnnotationEditor editor in editors)
                {
                    editor.Transform(matrix);
                }
                CalcBoundingRect();
            }
        }
        protected override GuideRect[] CreateGuides()
        {
            // there is no guides, only moving is allowed
            return null;
        }
        protected override GuideRect doMoveGuide(GuideRect guide, Datalogics.PDFL.Point location)
        {
            // there is no bounding guides
            return null;
        }
        public override void UpdateBoundingRect()
        {
            // there is no bounding rect to update
        }
        public override Hit TestHit(Datalogics.PDFL.Point point)
        {
            foreach (BaseAnnotationEditor editor in editors)
            {
                Hit hit = editor.TestHit(point);
                if ((hit.flags & HitFlags.ShapeHit) != 0)
                {
                    hit.flags = HitFlags.ShapeHit;
                    hit.guide = null;
                    return hit;
                }
            }
            return new Hit(); // no hit
        }
        public override void Draw(Graphics g, Datalogics.PDFL.Matrix matrix)
        {
            if (editors.Count == 1)
            {
                editors[0].Draw(g, matrix);
            }
            else
            {
                foreach (BaseAnnotationEditor editor in editors)
                {
                    editor.DrawShape(g, matrix);
                    Rect newRect = Utils.Transform(editor.Properties.BoundingRect, matrix);
                    Rectangle rect = new Rectangle((int)newRect.LLx, (int)newRect.LLy, (int)newRect.Width, (int)newRect.Height);
                    Pen pen = new Pen(memberRectColor);
                    pen.DashPattern = new float[] { 1, 1 };
                    g.DrawRectangle(pen, rect);
                }
                DrawGroupRect(g, matrix);
            }
        }
        private void DrawGroupRect(Graphics g, Datalogics.PDFL.Matrix matrix)
        {
            Rect newRect = Utils.Transform(Properties.BoundingRect, matrix);
            Rectangle rect = new Rectangle((int)newRect.LLx, (int)newRect.LLy, (int)newRect.Width, (int)newRect.Height);
            Pen pen = new Pen(System.Drawing.Color.Black);
            pen.DashPattern = new float[] { 2, 2 };
            g.DrawRectangle(pen, rect);
        }
        private void OnPropertisUpdate(object data)
        {
            foreach(BaseAnnotationEditor editor in editors)
            {
                editor.Properties.UpdateFrom(properties);
                editor.Properties.RaiseUpdates(true);
            }
            properties.Dirty = false;
        }
        #endregion
        #region Members
        IList<BaseAnnotationEditor> editors; // this list is sorted by annotation index
        AnnotationProperties properties;
        AnnotationProperties.UpdateCallback properiesUpdateCallback;
        private System.Drawing.Color memberRectColor = System.Drawing.Color.Navy;
        #endregion
    }
}
