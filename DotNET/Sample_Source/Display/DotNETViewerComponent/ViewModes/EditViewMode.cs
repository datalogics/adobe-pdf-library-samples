using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Datalogics.PDFL;

namespace DotNETViewerComponent
{
    /**
     * View mode: edit annotation. Used to edit for all types of annotations.
     * It checks annotation's guide rect hits and tracks a guide movings.
     * Also it provides context menu for setting annotation's properties.
     */ 
    public class EditViewMode : ViewMode
    {
        #region Contructors
        public EditViewMode(DocumentView view)
            : base(view)
        {
            // shapeAnnotationContextMenu items
            //*
            MenuItem commonAnnotationPropertiesMenuItem = new MenuItem("Properties", ShowAnnotationProperties);
            MenuItem commonDeleteAnnotationMenuItem = new MenuItem("Delete", RemoveAnnotation);
            MenuItem commonSeperatorMenuItem = new MenuItem("-");
            MenuItem commonAnnotationColorMenuItem = new MenuItem("Color", ChooseColor);

            // sub-menus for the shapeAnnotationContextMenu
            //*
            MenuItem lineStyleSubMenu = new MenuItem("Line Pattern");
            lineStyleSubMenu.Name = "LineStyle";

            MenuItem solidLineStyleSubMenuItem = new MenuItem("Solid", SolidLine);
            MenuItem dashed1LineStyleSubMenuItem = new MenuItem("Dashed 1", DashLine1);
            MenuItem dashed2LineStyleSubMenuItem = new MenuItem("Dashed 2", DashLine2);
            MenuItem dashed3LineStyleSubMenuItem = new MenuItem("Dashed 3", DashLine3);
            MenuItem dashed4LineStyleSubMenuItem = new MenuItem("Dashed 4", DashLine4);

            lineStyleSubMenu.MenuItems.AddRange(new MenuItem[] { solidLineStyleSubMenuItem, dashed1LineStyleSubMenuItem, dashed2LineStyleSubMenuItem, dashed3LineStyleSubMenuItem,
                    dashed4LineStyleSubMenuItem});

            MenuItem lineWidthSubMenu = new MenuItem("Line Width");
            lineWidthSubMenu.Name = "LineWidth";

            MenuItem onePointLineWidthSubMenuItem = new MenuItem("1 pt.", LineWidth1);
            MenuItem twoPointLineWidthSubMenuItem = new MenuItem("2 pt.", LineWidth2);
            MenuItem threePointLineWidthSubMenuItem = new MenuItem("3 pt.", LineWidth3);
            MenuItem fivePointLineWidthSubMenuItem = new MenuItem("5 pt.", LineWidth5);
            MenuItem otherLineWidthSubMenuItem = new MenuItem("Other...", LineWidthOther);

            lineWidthSubMenu.MenuItems.AddRange(new MenuItem[] { onePointLineWidthSubMenuItem, twoPointLineWidthSubMenuItem, threePointLineWidthSubMenuItem, fivePointLineWidthSubMenuItem,
                    otherLineWidthSubMenuItem});
            //*
            MenuItem shapeLineWidth = lineWidthSubMenu.CloneMenu();
            shapeLineWidth.Name = "LineWidth";
            MenuItem shapeLineStyle = lineStyleSubMenu.CloneMenu();
            shapeLineStyle.Name = "LineStyle";
            shapeAnnotationContextMenu = new ContextMenu(new MenuItem[] { commonAnnotationPropertiesMenuItem.CloneMenu(), commonDeleteAnnotationMenuItem.CloneMenu(), commonSeperatorMenuItem.CloneMenu(), commonAnnotationColorMenuItem.CloneMenu(),
                shapeLineStyle, shapeLineWidth});
            //*

            // textAnnotationContextMenu items
            MenuItem textAnnotationAutoSizeMenuItem = new MenuItem("Autosize Vertically", AutosizeTextAnnotation);

            textAnnotationContextMenu = new ContextMenu(new MenuItem[] { commonAnnotationPropertiesMenuItem.CloneMenu(), commonDeleteAnnotationMenuItem.CloneMenu(), commonSeperatorMenuItem.CloneMenu(),
                commonAnnotationColorMenuItem.CloneMenu(), commonSeperatorMenuItem.CloneMenu(), textAnnotationAutoSizeMenuItem});
            //*

            // linkAnnotationContextMenu items
            MenuItem linkAnnotationShowBorder = new MenuItem("Show Border", ShowBorder);
            linkAnnotationShowBorder.Name = "ShowBorder";

            MenuItem linkLineWidth = lineWidthSubMenu.CloneMenu();
            linkLineWidth.Name = "LineWidth";
            MenuItem linkLineStyle = lineStyleSubMenu.CloneMenu();
            linkLineStyle.Name = "LineStyle";
            linkAnnotationContextMenu = new ContextMenu(new MenuItem[] { commonAnnotationPropertiesMenuItem.CloneMenu(), commonDeleteAnnotationMenuItem.CloneMenu(), commonSeperatorMenuItem.CloneMenu(),
                commonAnnotationColorMenuItem.CloneMenu(), linkLineStyle, linkLineWidth, commonSeperatorMenuItem.CloneMenu(), linkAnnotationShowBorder});
            //*

            if (docView.ActiveAnnotation != null)
            {
                cachedIndex = docView.ActiveAnnotation.Index;
                if (docView.ActiveAnnotation.Properties.Subtype == "FreeText") CreateRichTextBox();
            }
        }
        #endregion
        #region Methods
        private void UpdateMenuItems(ContextMenu currentMenu)
        {
            int lineWidthIndex = 0;
            switch ((int)Math.Round(docView.ActiveProps.LineWidth))
            {
                case 1:
                    lineWidthIndex = 0;
                    break;
                case 2:
                    lineWidthIndex = 1;
                    break;
                case 3:
                    lineWidthIndex = 2;
                    break;
                case 5:
                    lineWidthIndex = 3;
                    break;
                default:
                    lineWidthIndex = 4;
                    break;
            }
            MenuItem lineWidthMenu = currentMenu.MenuItems["LineWidth"];
            for (int i = 0; i < (lineWidthMenu != null ? lineWidthMenu.MenuItems.Count : 0); i++)
                lineWidthMenu.MenuItems[i].Checked = lineWidthIndex == i;

            int lineStyleIndex = 0;
            if (docView.ActiveProps.Solid)
                lineStyleIndex = 0;
            else
            {
                switch (docView.GetNumPattern(docView.ActiveProps.DashPattern))
                {
                    case 0:
                        lineStyleIndex = 1;
                        break;
                    case 1:
                        lineStyleIndex = 2;
                        break;
                    case 2:
                        lineStyleIndex = 3;
                        break;
                    case 3:
                        lineStyleIndex = 4;
                        break;
                    default:
                        lineStyleIndex = 0;
                        break;
                }
            }
            MenuItem lineStyleMenu = currentMenu.MenuItems["LineStyle"];
            for (int i = 0; i < (lineStyleMenu != null ? lineStyleMenu.MenuItems.Count : 0); i++)
                lineStyleMenu.MenuItems[i].Checked = lineStyleIndex == i;

            MenuItem linkShowBorder = currentMenu.MenuItems["ShowBorder"];
            if (linkShowBorder != null)
                linkShowBorder.Checked = (docView.ActiveAnnotation != null && docView.ActiveProps.ShowBorder);
        }
        /**
         * Searches for annotation using given location.
         * Location - pixel coordinate in view, including scroll offsets.
         * Index - index of last annotation found, to increase performance,
         * and for prioritized annotation selection
         * (it means that even if point belongs to several annotations, the one with given index will be chosen)
         */
        private int FindAnnotation(Datalogics.PDFL.Point location, int index)
        {
            Dictionary<int, BaseAnnotationEditor> editors = docView.EditAnnotations;
            if (editors == null) return -1;
            if (editors.ContainsKey(index) && (editors[index].TestHit(location).flags != HitFlags.NoHit)) return index;

            bool firstHit = true;
            double distance = 0;
            int nearestAnnoIndex = -1;
            foreach(BaseAnnotationEditor editor in editors.Values)
            {
                if ((editor.Index == index) || (editor.TestHit(location).flags == HitFlags.NoHit)) continue;

                Datalogics.PDFL.Rect rect = editor.Properties.BoundingRect;
                Datalogics.PDFL.Point center = new Datalogics.PDFL.Point((rect.Left + rect.Right) / 2, (rect.Top + rect.Bottom) / 2);
                double d = (center.H - location.H) * (center.H - location.H) + (center.V - location.V) * (center.V - location.V);
                if (firstHit)
                {
                    distance = d;
                    firstHit = false;
                    nearestAnnoIndex = editor.Index;
                }
                else if (d < distance)
                {
                    distance = d;
                    nearestAnnoIndex = editor.Index;
                }
            }
            return nearestAnnoIndex;
        }
        /**
         * Mouse click handler, checks annotation's guide rect hit
         */
        public override void MouseDown(MouseEventArgs e, System.Drawing.Point location)
        {
            bool isControlPressed = Control.ModifierKeys == Keys.Control;
            bool isShiftPressed = Control.ModifierKeys == Keys.Shift;

            if (e.Button == MouseButtons.Left)
            {
                int pageNum = docView.GetPageByCoord(location);
                if (pageNum != -1) docView.EditPage = pageNum;
                captured = true;
                capturedPoint = location;
                capturedGuide = null;
            }

            if (!docView.EditPermission||
                docView.EditPage == -1 ||
                docView.EditAnnotations == null ||
                docView.EditAnnotations.Count == 0)
            {
                return;
            }

            Datalogics.PDFL.Point pdfPoint = docView.ViewToPdf(location, docView.EditPage);
            cachedIndex = FindAnnotation(pdfPoint, cachedIndex);
            if (cachedIndex == -1 || isControlPressed)
            {
                /*if (docView.ActiveAnnotation != null)
                {
                    SaveFinalState();
                    CreateEditCommand();
                }*/
                docView.ActiveAnnotation = null;
                return;
            }

            if (isShiftPressed)
            {
                docView.SelectAnnotation(docView.EditPage, cachedIndex, false);
            }
            else // no shift pressed
            {
                if ((docView.ActiveAnnotation is GroupAnnotationEditor) &&
                    (docView.ActiveAnnotation as GroupAnnotationEditor).Contains(docView.EditAnnotations[cachedIndex]))
                {
                    // if we clicked on annotation that is member of group - then do nothing (save selection unchanged)
                }
                else
                {
                    docView.SelectAnnotation(docView.EditPage, cachedIndex, true);
                }
            }

            Hit hit = docView.ActiveAnnotation.TestHit(pdfPoint);
            capturedGuide = hit.guide;
            if (capturedGuide != null)
            {
                Datalogics.PDFL.Point guideLocation = capturedGuide.Location;
                capturedOffset = new Datalogics.PDFL.Point(guideLocation.H - pdfPoint.H, guideLocation.V - pdfPoint.V);
            }
        }
        /**
         * Mouse move handler, tracks a guide rect movings.
         */
        public override void MouseMove(MouseEventArgs e, System.Drawing.Point location)
        {
            // if no annotation is currently selected then just drag view like in ScrollViewMode
            if (captured && docView.ActiveAnnotation == null)
            {
                // drag page like in ScrollViewMode
                System.Drawing.Point delta = location;
                delta.X -= capturedPoint.X;
                delta.Y -= capturedPoint.Y;
                System.Drawing.Point scroll = docView.Scroll;
                scroll.X -= delta.X;
                scroll.Y -= delta.Y;
                docView.Scroll = scroll;
                docView.Cursor = Cursors.Default;
                return;
            }

            if (!docView.EditPermission ||
                docView.EditPage == -1 ||
                docView.EditAnnotations == null ||
                docView.EditAnnotations.Count == 0)
            {
                docView.Cursor = Cursors.Default;
                return;
            }

            // calculate pdf coordinate on the page
            Datalogics.PDFL.Point pdfPoint = docView.ViewToPdf(location, docView.EditPage);

            // left mouse button dragging: move or resize annotation
            if (e.Button == MouseButtons.Left && docView.ActiveAnnotation != null)
            {
                if (capturedGuide != null) // if guide is selected - move it
                {
                    Datalogics.PDFL.Point newGuideLocation = new Datalogics.PDFL.Point(pdfPoint.H + capturedOffset.H, pdfPoint.V + capturedOffset.V);
                    capturedGuide = docView.MoveGuide(capturedGuide, newGuideLocation);
                    docView.Cursor = capturedGuide.CursorForRotation(docView.Document.GetPage(docView.EditPage).Rotation);
                }
                else // if no guides is selected - move the whole annotation
                {
                    if (capturedPoint.Equals(location)) return;
                    Datalogics.PDFL.Point oldPdfPoint = docView.ViewToPdf(capturedPoint, docView.EditPage);
                    Datalogics.PDFL.Matrix matrix = new Datalogics.PDFL.Matrix()
                        .Translate(pdfPoint.H - oldPdfPoint.H, pdfPoint.V - oldPdfPoint.V);
                    capturedPoint = location;
                    docView.TransformAnnotation(matrix);
                    docView.Cursor = Cursors.Hand;
                }
                docView.IsDocumentChanged = true;
                return;
            }

            // no left mouse button down, just process cursor
            int index = FindAnnotation(pdfPoint, cachedIndex);
            if (index == -1)
            {
                docView.HoverAnnotation = null;
                docView.Cursor = Cursors.Default;
            }
            else // when no mouse dragging, we track annotation under cursor just to draw it's bounding rectangle
            {
                BaseAnnotationEditor anno = docView.EditAnnotations[index];
                Hit hit = anno.TestHit(pdfPoint);
                if (anno == docView.ActiveAnnotation && (hit.flags & HitFlags.GuideHit) != 0)
                {
                    docView.Cursor = hit.guide.CursorForRotation(docView.Document.GetPage(docView.EditPage).Rotation);
                }
                else if ((hit.flags & (HitFlags.BorderHit | HitFlags.RectHit)) != 0)
                {
                    docView.Cursor = Cursors.Hand;
                }
                else
                {
                    docView.Cursor = Cursors.Default;
                }
                docView.HoverAnnotation = anno;
            }
        }
        /**
         * Mouse up handler.
         * All annotation transformation is performed in mouse move handler.
         * This handler is for context menu calls, and for finishing annotation tracking.
         */
        public override void MouseUp(MouseEventArgs e, System.Drawing.Point location)
        {
            captured = false;
            if (!docView.EditPermission) return;

            if (e.Button == MouseButtons.Right)
            {
                if (docView.ActiveAnnotation != null)
                {
                    ContextMenu currentContextMenu = null;

                    switch (docView.ActiveAnnotation.Properties.Subtype)
                    {
                        case "FreeText":
                            currentContextMenu = textAnnotationContextMenu;
                            break;
                        case "Link":
                            currentContextMenu = linkAnnotationContextMenu;
                            break;
                        default:
                            currentContextMenu = shapeAnnotationContextMenu;
                            break;
                    }
                    if (currentContextMenu != null)
                    {
                        UpdateMenuItems(currentContextMenu);
                        currentContextMenu.Show(docView.Parent, e.Location);
                    }
                }

            }
            else
            {
                docView.EditCheckPoint();
                if (e.Button == MouseButtons.Left && Control.ModifierKeys == Keys.Control && (docView.ActiveAnnotation != null || cachedIndex != -1))
                {
                    BaseAnnotationEditor editor = docView.EditAnnotations[cachedIndex];
                    if (editor is LinkAnnotationEditor)
                    {
                        LinkAnnotation annot = editor.Annotation as LinkAnnotation;
                        if (annot.Action is URIAction)
                        {
                            URIAction act = annot.Action as URIAction;
                            System.Diagnostics.Process.Start(act.URI);
                        }
                        else if (annot.Action is RemoteGoToAction)
                        {
                            RemoteGoToAction action = annot.Action as RemoteGoToAction;
                            FileSpecification spec = action.FileSpecification;

                            string fullPath = System.IO.Path.GetFullPath(spec.Path);
                            if (System.IO.File.Exists(fullPath))
                                System.Diagnostics.Process.Start(fullPath);
                        }
                        else if (annot.Action is GoToAction || annot.Destination != null)
                        {
                            GoToAction action = (annot.Action as GoToAction);
                            ViewDestination dest = action == null ? annot.Destination : action.Destination;

                            if (dest != null) docView.GoToDestionation(dest);
                        }
                    }
                }
            }
        }
        /**
         * Double click handler. Used only for FreeText annotation editing: opens annotation's rich text box.
         */
        public override void MouseDoubleClick(MouseEventArgs e, System.Drawing.Point location)
        {
            if (!docView.EditPermission) return;

            BaseAnnotationEditor editor = docView.ActiveAnnotation;
            if (editor == null) return;
            switch (editor.Properties.Subtype)
            {
                // we have clicked on free text annotation: create rich text box
                case "FreeText": CreateRichTextBox(); return;
                // we have clicked on link annotation: switch to target view mode to select link target
                case "Link":
                    docView.FunctionMode = ApplicationFunctionMode.TargetMode;
                    return;
            }
        }
        /**
         * Keyboard handler - deletes annotation on delete key.
         */
        public override void KeyDown(KeyEventArgs e)
        {
            if (!docView.EditPermission) return;

            if (e.KeyCode == Keys.Delete)
            {
                RemoveAnnotation(this, e);
            }
        }
        /**
         * Context menu handlers.
         */
        public void RemoveAnnotation(Object sender, EventArgs e)
        {
            docView.DeleteAnnotation();
        }
        private void ShowAnnotationProperties(object sender, EventArgs e)
        {
            docView.ShowAnnotationProperties();
        }
        private void ChooseColor(object sender, EventArgs e)
        {
            docView.ChooseColor(false);
        }
        private void SolidLine(object sender, EventArgs e)
        {
            docView.LinePattern(false, 0);
        }
        private void DashLine1(object sender, EventArgs e)
        {
            docView.LinePattern(false, 1);
        }
        private void DashLine2(object sender, EventArgs e)
        {
            docView.LinePattern(false, 2);
        }
        private void DashLine3(object sender, EventArgs e)
        {
            docView.LinePattern(false, 3);
        }
        private void DashLine4(object sender, EventArgs e)
        {
            docView.LinePattern(false, 4);
        }
        private void LineWidth1(object sender, EventArgs e)
        {
            docView.LineWidth(false, 1);
        }
        private void LineWidth2(object sender, EventArgs e)
        {
            docView.LineWidth(false, 2);
        }
        private void LineWidth3(object sender, EventArgs e)
        {
            docView.LineWidth(false, 3);
        }
        private void LineWidth5(object sender, EventArgs e)
        {
            docView.LineWidth(false, 5);
        }
        private void LineWidthOther(object sender, EventArgs e)
        {
            docView.LineWidth(false, -1);
        }
        private void AutosizeTextAnnotation(object sender, EventArgs e)
        {
            docView.ActiveProps.IsAutoSize = true;
        }
        private void ShowBorder(object sender, EventArgs e)
        {
            docView.ActiveProps.ShowBorder = !docView.ActiveProps.ShowBorder;
        }
        /**
         * Creates rich text box while editing FreeText annotation.
         */
        private void CreateRichTextBox()
        {
            if (!docView.EditPermission) return;

            AnnotationProperties props = docView.ActiveProps;
            Datalogics.PDFL.Rect rect = props.BoundingRect;
            Datalogics.PDFL.Rect newRect = Utils.Transform(rect, docView.GetPagePdfToWinCoordMatrix(docView.EditPage));

            textBox = new RichTextBox();
            docView.Controls.Add(textBox);
            textBox.Bounds = new Rectangle((int)newRect.LLx, (int)newRect.LLy, (int)newRect.Width, (int)newRect.Height);
            textBox.Text = props.Contents;
            textBox.BorderStyle = BorderStyle.None;
            textBox.TextChanged += new EventHandler(TextBoxTextChanged);
            textBox.Leave += new EventHandler(TextBoxLeaveHandler);
            textBox.KeyDown += new KeyEventHandler(TextBoxKeyDownHandler);
            props.Update += UpdateRichTextBox;
            textBox.Visible = true;
            textBox.Focus();

            UpdateRichTextBox(false);
        }
        void TextBoxKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                docView.Focus();
            }
        }
        private void TextBoxTextChanged(Object sender, EventArgs e)
        {
            docView.ActiveProps.Contents = textBox.Text;
        }
        private void TextBoxLeaveHandler(Object sender, EventArgs e)
        {
            docView.ActiveProps.Update -= UpdateRichTextBox;
            textBox.TextChanged -= new EventHandler(TextBoxTextChanged);
            textBox.Leave -= new EventHandler(TextBoxLeaveHandler);
            textBox.KeyDown -= new KeyEventHandler(TextBoxKeyDownHandler);
            docView.Controls.Remove(textBox);
            textBox = null;
        }
        private void UpdateRichTextBox(object unused)
        {
            AnnotationProperties props = docView.ActiveProps;
            textBox.BackColor = props.InteriorColor;
            textBox.ForeColor = props.TextColor;

            FontStyle[] possibleStyles = new FontStyle[] { FontStyle.Regular, FontStyle.Italic, FontStyle.Bold, FontStyle.Italic | FontStyle.Bold };
            System.Drawing.Font drawFont = null;
            foreach (FontStyle style in possibleStyles)
            {
                try
                {
                    drawFont = new System.Drawing.Font(props.FontFace, (float)(props.FontSize * docView.Zoom * docView.DPI), style, GraphicsUnit.Pixel);
                    break;
                }
                catch (ArgumentException)
                {
                    // possible exception if font can't be rendered with specified style
                }
            }
            textBox.Font = drawFont;
        }
        #endregion
        #region Members
        private int cachedIndex = -1;
        private bool captured = false;
        private System.Drawing.Point capturedPoint;
        private Datalogics.PDFL.Point capturedOffset;
        private GuideRect capturedGuide = null;

        private ContextMenu shapeAnnotationContextMenu;
        private ContextMenu textAnnotationContextMenu;
        private ContextMenu linkAnnotationContextMenu;
        private RichTextBox textBox;
        #endregion
    }
    /**
     * View mode: select link location.
     * Allows to select link annotation destination by double click
     */
    public class TargetViewMode : ViewMode
    {
        #region Contructors
        public TargetViewMode(DocumentView view)
            : base(view)
        {
            // save docView scroll value to return to it after targetting
            oldScroll = docView.Scroll;
            oldZoom = docView.Zoom;
            oldFit = docView.CurrentFit;

            linkAnnProp = new frmLinkAnnotationProperties();
            linkAnnProp.Show(docView);
            docView.Focus();
            linkAnnProp.FormClosed += new FormClosedEventHandler(linkAnnProp_FormClosed);
        }
        #endregion
        #region Methods
        public void linkAnnProp_FormClosed(object sender, EventArgs e)
        {
            if (linkAnnProp.DialogResult == DialogResult.OK)
            {
                string fitMode = "XYZ";
                double zoom = 0;
                location = new System.Drawing.Point(docView.Scroll.X, docView.Scroll.Y);
                Datalogics.PDFL.Point destPoint = docView.ViewToPdf(location, docView.CurrentPage);
                if (linkAnnProp.IsCustom)
                {
                    if (docView.CurrentFit == FitModes.FitPage || docView.CurrentFit == FitModes.FitWidth)
                    {
                        fitMode = docView.CurrentFit == FitModes.FitPage ? "Fit" : "FitH";
                        zoom = ViewDestination.NullValue;
                        destRect = new Rect(ViewDestination.NullValue, ViewDestination.NullValue, ViewDestination.NullValue, docView.CurrentFit == FitModes.FitWidth && linkAnnProp.IsPosition ? destPoint.V : ViewDestination.NullValue);
                    }
                    else
                    {
                        zoom = linkAnnProp.IsZoom ? docView.Zoom : ViewDestination.NullValue;
                        destRect = new Rect(linkAnnProp.IsPosition ? destPoint.H : ViewDestination.NullValue, ViewDestination.NullValue, ViewDestination.NullValue, linkAnnProp.IsPosition ? destPoint.V : ViewDestination.NullValue);
                    }
                }
                else
                {
                    fitMode = "FitR";
                    Rectangle bRect = docView.PageInfo[docView.CurrentPage].boundingRectangle;
                    Rectangle pRect = new Rectangle(docView.Scroll, docView.Size);
                    pRect.Intersect(bRect);
                    Datalogics.PDFL.Point LeftBottom = new Datalogics.PDFL.Point();
                    Datalogics.PDFL.Point RightTop = new Datalogics.PDFL.Point();
                    System.Drawing.Point leftBottom = new System.Drawing.Point(pRect.Left, pRect.Bottom);
                    System.Drawing.Point rightTop = new System.Drawing.Point(pRect.Right, pRect.Top);
                    LeftBottom = docView.ViewToPdf(leftBottom, docView.CurrentPage);
                    RightTop = docView.ViewToPdf(rightTop, docView.CurrentPage);
                    destRect = new Rect(LeftBottom.H, LeftBottom.V, RightTop.H, RightTop.V);
                }
                int pageNum = docView.EditPage;
                if (pageNum != -1)
                {
                    ViewDestination viewDestination = new ViewDestination(docView.Document, docView.CurrentPage, fitMode, destRect, zoom);
                    Rect rc = viewDestination.DestRect;
                    //docView.ActiveAnnotation.Properties.Action = new GoToAction(viewDestination);
                    docView.ActiveAnnotation.Properties.ActionData = new ActionData(viewDestination);

                    docView.Scroll = oldScroll;
                    docView.RequestZoom(new ZoomEventArgs(oldZoom, oldFit));

                    captured = false;
                }
            }
            docView.FunctionMode = ApplicationFunctionMode.AnnotationEditMode;
        }
        public override void MouseDown(MouseEventArgs e, System.Drawing.Point location)
        {
            if (e.Button == MouseButtons.Left)
            {
                captured = true;
                capturedPoint = location;
            }
        }
        public override void MouseMove(MouseEventArgs e, System.Drawing.Point location)
        {
            docView.Cursor = Cursors.Default;
            if (captured)
            {
                // drag page like in ScrollViewMode
                System.Drawing.Point delta = location;
                delta.X -= capturedPoint.X;
                delta.Y -= capturedPoint.Y;
                System.Drawing.Point scroll = docView.Scroll;
                scroll.X -= delta.X;
                scroll.Y -= delta.Y;

                docView.Scroll = scroll;
                return;
            }
        }
        public override void MouseUp(MouseEventArgs e, System.Drawing.Point location)
        {
            captured = false;
        }
        #endregion
        #region Members
        private bool captured = false;
        private System.Drawing.Point capturedPoint;
        private System.Drawing.Point oldScroll;
        private double oldZoom;
        private FitModes oldFit;
        
        private Datalogics.PDFL.Rect destRect;
        private System.Drawing.Point location;
        frmLinkAnnotationProperties linkAnnProp;
        #endregion
    }
}
