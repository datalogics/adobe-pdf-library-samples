using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Datalogics.PDFL;

namespace DotNETViewerComponent
{
    /**
     * Base class for annotation creation, encapsulates common keyboard message handling:
     * Escape key - cancels annotation creation.
     * Enter key - finalizes annotation creation.
     */
    public class CreateAnnotationMode : ViewMode
    {
        #region Constuctors
        public CreateAnnotationMode(DocumentView view, string subType)
            : base(view)
        {
            properties = new AnnotationProperties(subType);
        }
        #endregion
        #region Methods
        /**
         * Transforming: move sticked guide.
         */
        public override void MouseMove(MouseEventArgs e, System.Drawing.Point location)
        {
            if (!docView.EditPermission) return;
            SetCursor(location, captured);
            if (!captured) return;

            // move guide that is sticked to cursor
            Datalogics.PDFL.Point pdfPoint = docView.ViewToPdf(location, capturedPage);
            capturedGuide = docView.ActiveAnnotation.MoveGuide(capturedGuide, pdfPoint);
            docView.Invalidate();
        }
        public override void KeyDown(KeyEventArgs e)
        {
            if (!docView.EditPermission) return;

            switch (e.KeyCode)
            {
                case Keys.Enter: FinalizeAnnotation(false); return;
                case Keys.Escape: CancelAnnotation(false); return;
            }
        }
        protected virtual void FinalizeAnnotation(bool calledFromContextMenu)
        {
        }
        protected virtual void CancelAnnotation(bool calledFromContextMenu)
        {
        }
        protected virtual void SetCursor(System.Drawing.Point location, bool captured)
        {
            if (!captured)
            {
                if (docView.GetPageByCoord(location) == -1) docView.Cursor = Cursors.Default;
                else docView.Cursor = Cursors.Cross;
            }
            else if (capturedGuide != null)
            {
                docView.Cursor = capturedGuide.CursorForRotation(docView.Document.GetPage(capturedPage).Rotation);
            }
            else docView.Cursor = Cursors.Cross;
        }
        #endregion
        #region Members
        protected bool captured = false;
        protected int capturedPage = -1;
        protected AnnotationProperties properties;
        protected GuideRect capturedGuide;
        #endregion
    }
    /**
     * Creates circle and square annotations - these annotations don't have additional guides.
     * It creates by click-drag-up.
     */
    public class CreateSimpleAnnotationMode : CreateAnnotationMode
    {
        #region Contructors
        public CreateSimpleAnnotationMode(DocumentView view, string annoSubType)
            : base(view, annoSubType)
        {
        }
        #endregion
        #region Methods
        public override void MouseDown(MouseEventArgs e, System.Drawing.Point location)
        {
            if (!docView.EditPermission) return;
            if (e.Button != MouseButtons.Left) return;

            int pageNum = docView.GetPageByCoord(location);
            if (!captured && pageNum == -1) return;

            if (!captured) // first click, put start point, stick endpoint to mouse cursor
            {
                captured = true;
                capturedPage = pageNum;

                // create initial rectangle
                Datalogics.PDFL.Point pdfPoint = docView.ViewToPdf(location, pageNum);
                Datalogics.PDFL.Rect rect = new Datalogics.PDFL.Rect(pdfPoint.H - 1, pdfPoint.V - 1, pdfPoint.H, pdfPoint.V);

                properties.BoundingRect = rect;
                BaseAnnotationEditor editor = docView.CreateAnnotation(pageNum, properties);

                capturedGuide = editor.Guides[2]; // guide: bottom-right corner of rectangle
                docView.Invalidate();
            }
        }
        /**
         * End of creation. All processing already done in mouse move handler, just switch to edit mode.
         */
        public override void MouseUp(MouseEventArgs e, System.Drawing.Point location)
        {
            if (!docView.EditPermission) return;

            if (e.Button == MouseButtons.Left && captured)
            {
                docView.FunctionMode = ApplicationFunctionMode.AnnotationEditMode;
            }
        }
        #endregion
    }
    /**
     * Creates line annotations.
     * Line annotation is created by click-drag-up.
     * There are no bounding guides for lines, just start and end-point guides.
     */
    public class CreateLineAnnotationMode : CreateAnnotationMode
    {
        #region Contructors
        public CreateLineAnnotationMode(DocumentView view)
            : base(view, "Line")
        {
        }
        #endregion
        #region Methods
        public override void MouseDown(MouseEventArgs e, System.Drawing.Point location)
        {
            if (!docView.EditPermission) return;
            if (e.Button != MouseButtons.Left) return;

            int pageNum = docView.GetPageByCoord(location);
            if (!captured && pageNum == -1) return;

            if (!captured) // first click, put start point, stick endpoint to cursor
            {
                captured = true;
                capturedPage = pageNum;

                Datalogics.PDFL.Point pdfStartPoint = docView.ViewToPdf(location, pageNum);
                Datalogics.PDFL.Point pdfEndPoint = new Point(pdfStartPoint.H + 1, pdfStartPoint.V + 1);
                Datalogics.PDFL.Rect rect = new Datalogics.PDFL.Rect(pdfStartPoint.H, pdfStartPoint.V, pdfEndPoint.H, pdfEndPoint.V);

                properties.BoundingRect = rect;
                properties.StartPoint = pdfStartPoint;
                properties.EndPoint = pdfEndPoint;
                BaseAnnotationEditor editor = docView.CreateAnnotation(pageNum, properties);

                capturedGuide = editor.Guides[1]; // endpoint guide
                docView.Invalidate();
            }
        }
        /**
         * End of creation. All processing already done in mouse move handler, just switch to edit mode.
         */
        public override void MouseUp(MouseEventArgs e, System.Drawing.Point location)
        {
            if (!docView.EditPermission) return;

            if (e.Button == MouseButtons.Left && captured)
            {
                docView.FunctionMode = ApplicationFunctionMode.AnnotationEditMode;
            }
        }
        #endregion
    }
    /**
     * Creates polyline and polygon annotations.
     * Annotation created by adding points with mouse click.
     * Use Enter key or context menu to finalize annotation.
     */
    public class CreatePolyAnnotationMode : CreateAnnotationMode
    {
        #region Contructors
        public CreatePolyAnnotationMode(DocumentView view, string annoSubType)
            : base(view, annoSubType)
        {
            // creatingAnnotationContextMenu items - displayed only for Polygon and PolyLine Annotations
            MenuItem completeCreatingAnnotationMenuItem = new MenuItem("Complete", Complete);
            MenuItem cancelCreatingAnnotationMenuItem = new MenuItem("Cancel", Cancel);

            creatingAnnotationContextMenu = new ContextMenu(new MenuItem[] { completeCreatingAnnotationMenuItem, cancelCreatingAnnotationMenuItem });
        }
        #endregion
        #region Methods
        public override void MouseDown(MouseEventArgs e, System.Drawing.Point location)
        {
            if (!docView.EditPermission) return;
            if (e.Button != MouseButtons.Left) return;

            int pageNum = docView.GetPageByCoord(location);
            if (!captured && pageNum == -1) return;

            if (!captured) // first click, add start & end points, stick end-point to cursor
            {
                captured = true;
                capturedPage = pageNum;

                // create 2 initial points
                pointCount = 2;

                IList<Datalogics.PDFL.Point> vertices = new List<Datalogics.PDFL.Point>();
                Datalogics.PDFL.Point pdfPoint = docView.ViewToPdf(location, capturedPage);
                vertices.Add(pdfPoint);
                vertices.Add(pdfPoint); // end point is equal to start point

                Datalogics.PDFL.Rect rect = new Datalogics.PDFL.Rect(pdfPoint.H, pdfPoint.V, pdfPoint.H, pdfPoint.V);

                properties.BoundingRect = rect;
                properties.Vertices = vertices;
                BaseAnnotationEditor editor = docView.CreateAnnotation(pageNum, properties);
                capturedGuide = editor.Guides[9]; // endpoint guide
                docView.Invalidate();
            }
            else // every next click - add new point
            {
                Datalogics.PDFL.Point newVertex = docView.ViewToPdf(location, capturedPage);
                ++pointCount;
                Page page = docView.Document.GetPage(capturedPage);
                BaseAnnotationEditor editor = docView.ActiveAnnotation;
                IList<Datalogics.PDFL.Point> vertices = editor.Properties.Vertices;
                vertices.Add(newVertex);
                editor.Properties.Vertices = vertices;
                capturedGuide = editor.Guides[8 + pointCount - 1];
                docView.Invalidate();
            }
        }
        public override void MouseUp(MouseEventArgs e, System.Drawing.Point location)
        {
            if (!docView.EditPermission) return;
            if (e.Button != MouseButtons.Right) return;

            if (docView.ActiveAnnotation == null) throw new Exception("Logic error");
            creatingAnnotationContextMenu.Show(docView.Parent, e.Location);
        }
        protected override void FinalizeAnnotation(bool calledFromContextMenu)
        {
            /*if (!calledFromContextMenu)
            {
                // remove last vertex from annotation before finalizing (vertex that is sticked to mouse cursor)
                BaseAnnotationEditor editor = docView.ActiveAnnotation;

                IList<Datalogics.PDFL.Point> vertices = editor.Properties.Vertices;
                if (vertices.Count > 2)
                {
                    vertices.RemoveAt(vertices.Count - 1);
                    editor.Properties.Vertices = vertices;
                }
            }*/
            docView.FunctionMode = ApplicationFunctionMode.AnnotationEditMode;
        }
        protected override void CancelAnnotation(bool calledFromContextMenu)
        {
            docView.DeleteAnnotation();
            docView.FunctionMode = ApplicationFunctionMode.AnnotationEditMode;
        }
        private void Complete(Object sender, EventArgs e)
        {
            FinalizeAnnotation(true);
        }
        private void Cancel(Object sender, EventArgs e)
        {
            CancelAnnotation(true);
        }
        #endregion
        #region Members
        private int pointCount = 0;
        private ContextMenu creatingAnnotationContextMenu;
        #endregion
    }
    /**
     * Creates free text annotations.
     * Annotation created by click-drag-up. There is no annotation text editing in this mode.
     * You can enter annotation text just after creation, in edit mode.
     */
    public class CreateFreeTextAnnotationMode : CreateAnnotationMode
    {
        #region Constructors
        public CreateFreeTextAnnotationMode(DocumentView view)
            : base(view, "FreeText")
        {
        }
        #endregion
        #region Methods
        public override void MouseDown(MouseEventArgs e, System.Drawing.Point location)
        {
            if (!docView.EditPermission) return;
            if (e.Button != MouseButtons.Left) return;

            int pageNum = docView.GetPageByCoord(location);
            if (!captured && pageNum == -1) return;

            if (!captured) // first click, start point
            {
                captured = true;
                capturedPage = pageNum;

                // create initial rectangle
                Datalogics.PDFL.Point pdfPoint = docView.ViewToPdf(location, capturedPage);
                Datalogics.PDFL.Rect rect = new Datalogics.PDFL.Rect(pdfPoint.H - 1, pdfPoint.V - 1, pdfPoint.H, pdfPoint.V);

                properties.BoundingRect = rect;
                BaseAnnotationEditor editor = docView.CreateAnnotation(pageNum, properties);

                capturedGuide = editor.Guides[2]; // guide: bottom-right corner of rectangle
                docView.Invalidate();
            }
        }
        public override void MouseUp(MouseEventArgs e, System.Drawing.Point location)
        {
            if (!docView.EditPermission) return;

            if (captured)
            {
                docView.FunctionMode = ApplicationFunctionMode.AnnotationEditMode;
            }
        }
        #endregion
    }

    /**
     * Creates free text annotations.
     * Annotation created by click-drag-up.
     * After annotation creation user enters in link target mode and can choose link destination.
     */
    public class CreateLinkAnnotationMode : CreateAnnotationMode
    {
        #region Constructors
        public CreateLinkAnnotationMode(DocumentView view)
            : base(view, "Link")
        {
        }
        #endregion
        #region Methods
        public override void MouseDown(MouseEventArgs e, System.Drawing.Point location)
        {
            int pageNum = docView.GetPageByCoord(location);
            if (!captured && pageNum == -1) return;
            if (!captured)
            {
                captured = true;
                capturedPage = pageNum;
                Datalogics.PDFL.Point pdfPoint = docView.ViewToPdf(location, pageNum);
                Datalogics.PDFL.Rect rect = new Datalogics.PDFL.Rect(pdfPoint.H - 1, pdfPoint.V - 1, pdfPoint.H, pdfPoint.V);

                properties.BoundingRect = rect;
                BaseAnnotationEditor editor = docView.CreateAnnotation(pageNum, properties);

                capturedGuide = editor.Guides[2]; // guide: bottom-right corner of rectangle
                docView.Invalidate();
            }
            done = true;
        }
        public override void MouseUp(MouseEventArgs e, System.Drawing.Point location)
        {
            if (done && captured)
            {
                // switch to link targetting mode
                docView.FunctionMode = ApplicationFunctionMode.TargetMode;
                captured = false;
                done = false;
            }
        }
        #endregion
        #region Members
        bool done = false;
        #endregion
    }

    /**
     * Creates Ink annotation.
     * Creation is like Pen drawing. You can finish creation by ENTER key, or using context menu.
     */
    public class CreateInkAnnotationMode : CreateAnnotationMode
    {
        #region Constructors
        public CreateInkAnnotationMode(DocumentView view)
            : base(view, "Ink")
        {
            // creatingAnnotationContextMenu items - displayed only for Polygon and PolyLine Annotations
            MenuItem completeCreatingAnnotationMenuItem = new MenuItem("Complete", Complete);
            MenuItem cancelCreatingAnnotationMenuItem = new MenuItem("Cancel", Cancel);

            creatingAnnotationContextMenu = new ContextMenu(new MenuItem[] { completeCreatingAnnotationMenuItem, cancelCreatingAnnotationMenuItem });
        }
        #endregion
        #region Methods
        public override void MouseDown(MouseEventArgs e, System.Drawing.Point location)
        {
            if (e.Button != MouseButtons.Left) return;

            int pageNum = docView.GetPageByCoord(location);
            if (pageNum == -1) return;

            Datalogics.PDFL.Point pdfPoint = docView.ViewToPdf(location, pageNum);

            captured = true;
            if (scribbles == null) // adding first scribble - need to create annotation first
            {
                capturedPage = pageNum;
                scribbles = new List<IList<Point>>();
                properties.Scribbles = scribbles;
                docView.CreateAnnotation(pageNum, properties);
            }
            else
            {
                if (pageNum != capturedPage) return;
            }

            scribbles.Add(new List<Datalogics.PDFL.Point>());
            scribbles[scribbles.Count - 1].Add(pdfPoint);
            docView.ActiveAnnotation.Properties.Scribbles = scribbles;
        }
        public override void MouseMove(MouseEventArgs e, System.Drawing.Point location)
        {
            int pageNum = docView.GetPageByCoord(location);

            SetCursor(location, captured);

            if (!captured) return;
            if ((pageNum == -1) || (pageNum != capturedPage)) return;

            Datalogics.PDFL.Point pdfPoint = docView.ViewToPdf(location, docView.EditPage);
            scribbles[scribbles.Count - 1].Add(pdfPoint);
            docView.ActiveAnnotation.Properties.Scribbles = scribbles;
        }
        public override void MouseUp(MouseEventArgs e, System.Drawing.Point location)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (docView.ActiveAnnotation == null) throw new Exception("Logic error");
                creatingAnnotationContextMenu.Show(docView.Parent, e.Location);
                return;
            }

            captured = false;
        }
        public override void KeyDown(KeyEventArgs e)
        {
            if (!docView.EditPermission) return;

            if (e.KeyCode == Keys.Back)
            {
                if (scribbles != null)
                {
                    if (scribbles.Count > 1)
                    {
                        captured = false;
                        scribbles.Remove(scribbles[scribbles.Count - 1]);
                        docView.ActiveAnnotation.Properties.Scribbles = scribbles;
                    }
                    else
                    {
                        CancelAnnotation(false);
                    }
                }
            }
            else base.KeyDown(e);
        }
        protected override void FinalizeAnnotation(bool calledFromContextMenu)
        {
            docView.FunctionMode = ApplicationFunctionMode.AnnotationEditMode;
        }
        protected override void CancelAnnotation(bool calledFromContextMenu)
        {
            docView.DeleteAnnotation();
            docView.FunctionMode = ApplicationFunctionMode.AnnotationEditMode;
        }
        private void Complete(Object sender, EventArgs e)
        {
            FinalizeAnnotation(true);
        }
        private void Cancel(Object sender, EventArgs e)
        {
            CancelAnnotation(true);
        }
        #endregion
        #region Members
        private List<IList<Datalogics.PDFL.Point>> scribbles;
        private ContextMenu creatingAnnotationContextMenu;
        #endregion
    }

    /**
     * Creates underline or highlight annotation.
     * After creation you can edit selection borders and other properties in edit mode.
     */
    public class CreateMarkupAnnotationMode : CreateAnnotationMode
    {
        #region Constructors
        public CreateMarkupAnnotationMode(DocumentView view, string annoSubType)
            : base(view, annoSubType)
        {
        }
        #endregion
        #region Methods
        public override void MouseDown(MouseEventArgs e, System.Drawing.Point location)
        {
            int pageNum = docView.GetPageByCoord(location);
            if (pageNum == -1) return;

            captured = true;
            capturedPage = pageNum;
            Datalogics.PDFL.Point startPoint = docView.ViewToPdf(location, pageNum);
            Datalogics.PDFL.Rect rect = new Rect(startPoint.H, startPoint.V, startPoint.H + 1, startPoint.V + 1);
            Quad[] quads = new Quad[] { Utils.Quad(rect) }; // quad list cannot be empty, TODO: don't create annotation in this case

            properties.BoundingRect = rect;
            properties.Quads = quads;
            BaseAnnotationEditor editor = docView.CreateAnnotation(pageNum, properties);

            if (editor.Annotation is HighlightAnnotation)
                editor.Properties.ForeColor = System.Drawing.Color.Yellow;

            capturedGuide = editor.Guides[1]; // endpoint guide
        }
        public override void MouseUp(MouseEventArgs e, System.Drawing.Point location)
        {
            if (captured)
            {
                docView.FunctionMode = ApplicationFunctionMode.AnnotationEditMode;
            }
        }
        #endregion
    }
}
