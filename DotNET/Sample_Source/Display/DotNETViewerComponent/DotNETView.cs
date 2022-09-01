/*
Copyright (C) 2008-2009, Datalogics, Inc. All rights reserved.
 
The information and code in this sample is for the exclusive use
of Datalogics customers and evaluation users only.  Datalogics 
permits you to use, modify and distribute this file in accordance 
with the terms of your license agreement. Sample code is for 
demonstrative purposes only and is not intended for production use.
*/

/*
 * DotNETView :
 * 
 * DotNETView is responsible for the actual rendering of a PDF on the
 * screen. The constructor for DotNETView takes a System.Windows.Forms.Panel
 * which it uses as the location in an application that it should
 * render the PDF to.
 * 
 * 
 * Drawing a Page to the screen :
 *  This actually involves drawing the page out to a bitmap and then drawing
 *  the bitmap on the screen. To speed up this drawing, pages may be broken
 *  down into pieces (called Tiles here), these Tiles contain a portion of the
 *  page. The number of Tiles that are created is determined by the zoom level
 *  and the size of the page. The Tiles that are displayed are determined by
 *  the page number that the user is viewing and the scroll position.
 * 
 * 
 * DotNETView also contains a few helper objects that handle monitor resolution,
 * zooming in/out, and allowing the user to draw/edit annotations.
 */

using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using Datalogics.PDFL;

using System.Diagnostics;

namespace DotNETViewerComponent
{
    public class OpenDocumentData
    {
        public OpenDocumentData() : this(null, null, PermissionRequestOperation.AllOperations) { }
        public OpenDocumentData(string fileName) : this(fileName, null, PermissionRequestOperation.AllOperations) { }
        public OpenDocumentData(string fileName, string password) : this(fileName, password, PermissionRequestOperation.AllOperations) { }
        public OpenDocumentData(string fileName, string password, PermissionRequestOperation permission)
        {
            this.fileName = fileName;
            this.password = password;
            this.permission = permission;
            this.document = null;
        }

        public string fileName;
        public string password;
        public PermissionRequestOperation permission;
        public Document document;
    }
    [ToolboxItem(false)]
    public class DocumentView : Control
    {
        #region Constructors
        public DocumentView()
        {
            tileManager = new TileManager(this);
            editAnnotations = new Dictionary<int, BaseAnnotationEditor>();

            defaultAnnoProps = new AnnotationProperties();
            defaultAnnoProps.UpdateAlways = true;

            activeProps = new AnnotationProperties();
            activeProps.UpdateAlways = true;
            activeProps.Update += ActiveProperteisUpdateCallback;
            activeProps.SetFrom(defaultAnnoProps);

            patterns = new float[4][] { pattern1, pattern2, pattern3, pattern4 };
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Opaque, true);
        }
        #endregion
        #region Properties
        public bool CanUndo { get { return editCommandStack.CanUndo; } }
        public bool CanRedo { get { return editCommandStack.CanRedo; } }
        public FitModes CurrentFit { get { return tileManager.Fit; } }
        /**
         * Sets document to new value.
         * If you open new document you need first set Document to null.
         */
        public Datalogics.PDFL.Document Document
        {
            get { return document; }
            set
            {
                tileManager.Document = null;
                if (document == null && value != null) // opening new document
                {
                    FunctionMode = ApplicationFunctionMode.AnnotationEditMode;
                    document = value;
                    tileManager.Document = value;
                    currentPageNum = -1;
                    editPageNum = -1;
                    CurrentPage = 0;
                    EditPage = 0;
                    tileManager.Scroll = new System.Drawing.Point(0, 0);
                    if (DocumentChanged != null) DocumentChanged(this, null);
                }
                else if (document != null && value != null) // appending document to existing document
                {
                    ActiveAnnotation = null;
                    document = value;
                    tileManager.Document = value;
                    if ((document != null) && (CurrentPage >= document.NumPages)) CurrentPage = document.NumPages - 1;
                    if (DocumentChanged != null) DocumentChanged(this, null);
                }
                else if (value == null) // closing document or resetting document before opening new document
                {
                    ActiveAnnotation = null;
                    if (document != null) document.Dispose();
                    document = value;
                    tileManager.Document = value;
                    tileManager.Scroll = new System.Drawing.Point(0, 0);
                    editPageNum = -1;
                    currentPageNum = -1;
                    if (DocumentChanged != null) DocumentChanged(this, null);
                }
            }
        }
        public bool EditPermission
        {
            get { return Document != null? Document.PermRequest(PermissionRequestOperation.Modify): false; }
        }
        public double Zoom
        {
            get { return tileManager.Zoom; }
        }
        public double Rotation
        {
            get { return tileManager.RotateAngle; }
        }
        public void RequestZoom(ZoomEventArgs zoomMode)
        {
            if (ZoomChanged != null)
                ZoomChanged(this, zoomMode);
        }
        public void DrawSearchRects(Quad[] quades)
        {
            this.IsSerchRectDraw = true;
            this.Quades = quades;
            this.Invalidate();
        }
        public void DrawSearchRects(Graphics g, Matrix m)
        {

            Rectangle[] highlightRect = new Rectangle[Quades.Length];
            int i = 0;
            foreach (Quad q in Quades)
            {
                Datalogics.PDFL.Point p1 = Utils.Transform(q.TopLeft, m);
                Datalogics.PDFL.Point p2 = Utils.Transform(q.BottomRight, m);
                highlightRect[i] = new Rectangle((int)p1.H, (int)p1.V, (int)Math.Abs(p2.H - p1.H), (int)Math.Abs(p2.V - p1.V));
                i++;
            }
            g.DrawRectangles(Pens.Blue, highlightRect);

        }
        public bool IsSerchRectDraw
        {
            get { return isDrawSerchRect; }
            set { isDrawSerchRect = value; }
        }
        public Quad[] Quades
        {
            get { return quades; }
            set { quades = value; }
        }
        public AnnotationProperties ActiveProps
        {
            get { return activeProps; }
        }
        public AnnotationProperties DefaultProps
        {
            get { return defaultAnnoProps; }
        }
        public void setZoom(FitModes mode, double value) // returns recalculated scrolling value
        {
            Datalogics.PDFL.Point location = GetCurrentRelativeLocation();

            if (mode == FitModes.FitNone)
                tileManager.Zoom = value;
            else
            {
                tileManager.Fit = mode;
                tileManager.UpdateFitZooms(Size);
            }

            Scroll = CalcPagePosition(CurrentPage, location, true);
        }
        public System.Drawing.Size ViewSize
        {
            get { return tileManager.ViewSize;  }
        }
        public System.Drawing.Point Scroll
        {
            get { return tileManager.Scroll; }
            set
            {
                if (document == null)
                {
                    tileManager.Scroll = new System.Drawing.Point(0, 0);
                    return;
                }

                System.Drawing.Point scroll = value;

                scroll.X = Math.Min(scroll.X, ViewSize.Width - Size.Width);
                scroll.X = Math.Max(scroll.X, 0);
                scroll.Y = Math.Min(scroll.Y, ViewSize.Height - Size.Height);
                scroll.Y = Math.Max(scroll.Y, 0);

                tileManager.Scroll = scroll;
                int newCurrentPage = currentPageNum;

                if (SinglePageMode)
                {
                    int bottomY = Math.Min(scroll.Y + Size.Height, ViewSize.Height);
                    int topPage = GetPageByY(scroll.Y + PageIndent);
                    int bottomPage = GetPageByY(bottomY);

                    int prevPage = currentPageNum - 1;
                    int nextPage = currentPageNum + 1;
                    if (currentPageNum >= topPage && currentPageNum <= bottomPage)
                    {
                        if (topPage <= prevPage)
                        {
                            newCurrentPage = prevPage;
                            tileManager.Scroll = new System.Drawing.Point(tileManager.Scroll.X, Math.Max(PageInfo[newCurrentPage].boundingRectangle.Bottom - Size.Height, PageInfo[newCurrentPage].boundingRectangle.Top - PageIndent));
                        }
                        else if (bottomPage >= nextPage && (PageInfo[currentPageNum].boundingRectangle.Bottom > bottomY || PageInfo[currentPageNum].boundingRectangle.Top < scroll.Y))
                        {
                            newCurrentPage = nextPage;
                            tileManager.Scroll = new System.Drawing.Point(tileManager.Scroll.X, PageInfo[newCurrentPage].boundingRectangle.Top - PageIndent);
                        }
                    }
                    else
                    {
                        newCurrentPage = topPage;

                        int newX = Math.Min(tileManager.Scroll.X, PageInfo[newCurrentPage].boundingRectangle.Right - Size.Width);
                        newX = Math.Max(newX, PageInfo[newCurrentPage].boundingRectangle.Left - PageIndent);

                        int newY = Math.Min(tileManager.Scroll.Y, PageInfo[newCurrentPage].boundingRectangle.Bottom - Size.Height);
                        newY = Math.Max(newY, PageInfo[newCurrentPage].boundingRectangle.Top - PageIndent);
                        tileManager.Scroll = new System.Drawing.Point(newX, newY);
                    }
                }
                else
                {
                    newCurrentPage = GetPageByY(scroll.Y + Size.Height / 2);
                }

                Focus();
                CurrentPage = newCurrentPage;

                if (ScrollChanged != null) ScrollChanged(this, null);
            }
        }
        public PageInfo[] PageInfo
        {
            get { return tileManager.PageInfo; }
        }
        public OptionalContentContext OptionalContentContext
        {
            get { return tileManager.OptionalContentContext; }
            set { tileManager.OptionalContentContext = value; }
        }
        public bool SinglePageMode
        {
            get { return tileManager.SinglePageMode; }
            set
            {
                tileManager.SinglePageMode = value;
                Invalidate();
            }
        }
        public int PageIndent
        {
            get { return tileManager.PageIndent; }
            set { tileManager.PageIndent = value; }
        }
        public int CurrentPage
        {
            get { return currentPageNum; }
            set
            {
                if (document == null) value = -1;
                else if (value >= document.NumPages) value = document.NumPages - 1;

                if (currentPageNum != value)
                {
                    currentPageNum = value;
                    if (PageChanged != null) PageChanged(this, null);
                }
            }
        }
        public int EditPage
        {
            get { return editPageNum; }
            set
            {
                if ((document == null) || (value < 0) || (value >= document.NumPages)) value = -1;
                if (editPageNum != value)
                {
                    ActiveAnnotation = null;
                    HoverAnnotation = null;
                    if (editPage != null)
                    {
                        editPage.Dispose();
                        editPage = null;
                    }
                    editPageNum = value;
                    if (editPageNum != -1)
                    {
                        editPage = document.GetPage(editPageNum);
                    }
                    markupQuads = GetPageQuads(value);
                    LoadAnnotations();
                    Invalidate();
                }
            }
        }
        public Dictionary<int, BaseAnnotationEditor> EditAnnotations
        {
            get { return editAnnotations; }
        }
        public BaseAnnotationEditor ActiveAnnotation
        {
            get { return activeAnnotation; }
            set
            {
                if (activeAnnotation == value) return;
                if (activeAnnotation != null)
                {
                    activeAnnotation.Detach(AnnotationUpdateCallback);
                    SaveFinalEditState();
                }

                if (!EditPermission) // can't select annotation if no edit permissions
                    value = null;

                activeAnnotation = value;
                if (activeAnnotation != null)
                    activeAnnotation.Attach(AnnotationUpdateCallback);
                else
                    hoverAnnotation = null;

                ++lockEditCommandsFlag;
                activeProps.SetFrom((activeAnnotation != null) ? activeAnnotation.Properties : defaultAnnoProps);
                activeProps.RaiseUpdates(false);
                --lockEditCommandsFlag;

                if (ActiveAnnotation is MarkupAnnotationEditor)
                    (ActiveAnnotation as MarkupAnnotationEditor).PageQuads = MarkupQuads;

                if (FunctionMode == ApplicationFunctionMode.AnnotationEditMode && value != null)
                {
                    SaveInitialEditState();
                }

                Invalidate();
            }
        }
        public BaseAnnotationEditor HoverAnnotation
        {
            get { return hoverAnnotation; }
            set
            {
                if (hoverAnnotation == value) return;
                hoverAnnotation = value;

                toolTip.Hide(this.Parent); // hide previous tooltip
                if (hoverAnnotation != null)
                {
                    Rect annoRect = Utils.Transform(hoverAnnotation.Properties.BoundingRect, GetPagePdfToWinCoordMatrix(CurrentPage));
                    string title = (hoverAnnotation.Properties.Title.Length != 0) ? hoverAnnotation.Properties.Title + "\n\n" : "";

                    toolTip = new ToolTip();
                    toolTip.Show(title + hoverAnnotation.Properties.Contents, this.Parent, (int)Math.Round(annoRect.Right) + 2, (int)Math.Round(annoRect.Top) + 2, 2000);
                }

                Invalidate();
            }
        }
        public String DebugMessage
        {
            get { return debugMessage; }
            set { debugMessage = value; Invalidate(new Rectangle(0, 0, 500, 200)); }
        }
        public ApplicationFunctionMode FunctionMode
        {
            get { return functionMode; }
            set
            {
                if (underCreationFlag && (value != ApplicationFunctionMode.TargetMode))
                {
                    FinalizeAnnotationCreation();
                }
                if ((value != ApplicationFunctionMode.AnnotationEditMode) && (value != ApplicationFunctionMode.TargetMode))
                {
                    ActiveAnnotation = null;
                }
                functionMode = value;
                viewModeHandler = CreateViewModeHandler(functionMode);
                if (FunctionModeChanged != null) FunctionModeChanged(null, null);
                Invalidate();
            }
        }
        public Rectangle MarqueeZoomRect
        {
            get { return marqueeZoomRect; }
            set
            {
                marqueeZoomRect = value;
                Invalidate();
            }
        }
        public double DPI { get { return tileManager.DPIScaleX; } }
        public bool IsDocumentChanged
        {
            get { return isDocumentChanged; }
            set { isDocumentChanged = value; }
        }
        public bool ShowMarkup
        {
            get { return showMarkup; }
            set
            {
                if (showMarkup == value) return;
                showMarkup = value;
                Invalidate();
            }
        }
        public IList<Quad> MarkupQuads
        {
            get { return markupQuads; }
        }
        #endregion
        #region EditCommands
        private void Execute(EditCommand command)
        {
            int pageNum = EditPage;
            EditPage = -1;

            ++lockEditCommandsFlag;
            editCommandStack.Execute(command);
            --lockEditCommandsFlag;

            EditPage = pageNum;
            tileManager.InvalidateCache();
            Invalidate();

            IsDocumentChanged = true;
        }
        private void Push(EditCommand command)
        {
            editCommandStack.Push(command);
        }
        public void Redo()
        {
            int pageNum = EditPage;
            EditPage = -1;

            ++lockEditCommandsFlag;
            editCommandStack.Redo();
            --lockEditCommandsFlag;

            EditPage = pageNum;
            tileManager.InvalidateCache();
            Invalidate();

            IsDocumentChanged = true;
        }
        public void Undo()
        {
            int pageNum = EditPage;
            EditPage = -1;

            ++lockEditCommandsFlag;
            editCommandStack.Undo();
            --lockEditCommandsFlag;

            EditPage = pageNum;
            tileManager.InvalidateCache();
            Invalidate();

            IsDocumentChanged = true;
        }
        public BaseAnnotationEditor CreateAnnotation(int pageNum, AnnotationProperties properties)
        {
            EditPage = pageNum;
            int index = editPage.NumAnnotations;

            AnnotationFactory.CreateAnnotation(document, editPage, index, properties);
            BaseAnnotationEditor editor = LoadAnnotation(editPage, index);
            editAnnotations.Add(index, editor);
            editor.Annotation.Flags |= AnnotationFlags.Hidden;
            DefaultProps.Dirty = true;
            editor.Properties.UpdateFrom(DefaultProps);
            DefaultProps.Dirty = false;
            editor.Properties.RaiseUpdates(true);

            ActiveAnnotation = editor;
            underCreationFlag = true;

            return editor;
        }
        private void FinalizeAnnotationCreation()
        {
            if (!underCreationFlag) return;
            underCreationFlag = false;

            CreateAnnCommand.State state = new CreateAnnCommand.State(this, ActiveAnnotation);
            CreateAnnCommand command = new CreateAnnCommand(this, state);
            Push(command);
            SaveInitialEditState();
        }
        public void SelectAnnotation(int pageNum, int index, bool newSelection)
        {
            EditPage = pageNum;

            if (newSelection)
            {
                ActiveAnnotation = editAnnotations[index];
            }
            else
            {
                if (ActiveAnnotation is GroupAnnotationEditor)
                {
                    GroupAnnotationEditor group = (ActiveAnnotation as GroupAnnotationEditor);
                    ActiveAnnotation = null;
                    group.Invert(editAnnotations[index]);
                    if (group.Count == 1)
                    {
                        ActiveAnnotation = group.Get(0);
                    }
                    else
                    {
                        ActiveAnnotation = group;
                    }
                }
                else
                {
                    GroupAnnotationEditor group = new GroupAnnotationEditor(editPage);
                    group.Add(ActiveAnnotation);
                    group.Add(editAnnotations[index]);
                    ActiveAnnotation = group;
                }
            }

            Invalidate();
        }
        private void SaveEditState(ref EditAnnCommand.State outState, ref IList<EditAnnCommand.State> outGroupState)
        {
            if (ActiveAnnotation is GroupAnnotationEditor)
            {
                outState = null;
                outGroupState = new List<EditAnnCommand.State>();
                GroupAnnotationEditor group = ActiveAnnotation as GroupAnnotationEditor;
                for (int i = 0; i < group.Count; ++i)
                {
                    BaseAnnotationEditor editor = group.Get(i);
                    EditAnnCommand.State state = new EditAnnCommand.State(this, editor);
                    outGroupState.Add(state);
                }
            }
            else if (ActiveAnnotation != null)
            {
                outGroupState = null;
                outState = new EditAnnCommand.State(this, ActiveAnnotation);
            }
            else
            {
                outState = null;
                outGroupState = null;
            }
        }
        public void EditCheckPoint()
        {
            SaveFinalEditState();
        }
        private void SaveInitialEditState()
        {
            SaveEditState(ref editAnnotationInitState, ref editGroupAnnotationInitState);
            editAnnotationFinalState = null;
            editGroupAnnotationFinalState = null;
            changedFlag = false;
        }
        private void SaveFinalEditState()
        {
            if ((lockEditCommandsFlag > 0) ||
                (!changedFlag) ||
                ((editAnnotationInitState == null && editGroupAnnotationInitState == null)))
            {
                return;
            }
            SaveEditState(ref editAnnotationFinalState, ref editGroupAnnotationFinalState);
            CreateEditCommand();
            editAnnotationInitState = editAnnotationFinalState;
            editGroupAnnotationInitState = editGroupAnnotationFinalState;
            changedFlag = false;
        }
        private void CreateEditCommand()
        {
            if ((editAnnotationInitState != null) || (editGroupAnnotationInitState != null))
            {
                if ((editAnnotationInitState != null) && (editAnnotationFinalState != null))
                {
                    EditAnnCommand command = new EditAnnCommand(this, editAnnotationInitState, editAnnotationFinalState);
                    Push(command);
                }
                else if ((editGroupAnnotationInitState != null) && (editGroupAnnotationFinalState != null))
                {
                    GroupCommand groupCommand = new GroupCommand(this);
                    for (int i = 0; i < editGroupAnnotationInitState.Count; ++i)
                    {
                        EditAnnCommand command = new EditAnnCommand(this, editGroupAnnotationInitState[i], editGroupAnnotationFinalState[i]);
                        groupCommand.Add(command);
                    }
                    Push(groupCommand);
                }
            }
        }
        public GuideRect MoveGuide(GuideRect guide, Datalogics.PDFL.Point location)
        {
            ++lockEditCommandsFlag;
            GuideRect result = ActiveAnnotation.MoveGuide(guide, location);
            --lockEditCommandsFlag;
            changedFlag = true;
            return result;
        }
        public void TransformAnnotation(Matrix matrix)
        {
            ++lockEditCommandsFlag;
            ActiveAnnotation.Transform(matrix);
            --lockEditCommandsFlag;
            changedFlag = true;
        }
        #endregion
        #region Methods
        public bool DocChangeRequest()
        {
            if (document != null)
            {
                // pop up a dialog to ask if the user would like to save the changes
                // that have been made to the document that is currently open
                if (IsDocumentChanged)
                {
                    DialogResult saveResult = MessageBox.Show(
                        "The document has been altered since it was opened. Do you wish to save the changes?",
                        "Save document", MessageBoxButtons.YesNoCancel);

                    if (saveResult == DialogResult.Yes)
                    {
                        SaveDocument();
                        return true;
                    }
                    else if (saveResult == DialogResult.No)
                    {
                        IsDocumentChanged = false;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public string DocFileName()
        {
            return originalFileName;
        }
        public void SaveDocument()
        {
            if (document == null) return;
            try
            {
                EditPage = -1;
                if (originalFileName == document.FileName)
                {
                    document.Save(SaveFlags.Full);
                }
                else
                {
                    document.Save(SaveFlags.Full, originalFileName);
                }
                IsDocumentChanged = false;
            }
            catch (Exception exc)
            {
                MessageBox.Show("The File " + originalFileName + " cannot be saved. " + exc.ToString());
            }
        }
        public void SaveDocumentAs()
        {
            // if the document is null do not attempt to save
            if (document == null) return;
            String fileName = "";
            try
            {
                // open a save file dialog so the user can decide where to save and what name to use
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "PDF File (*.pdf)|*.pdf";
                saveDialog.Title = "Save file as";
                DialogResult saveResult = saveDialog.ShowDialog();

                if (saveResult == DialogResult.OK)
                {
                    originalFileName = saveDialog.FileName;
                    SaveDocument();
                }

                if (DocumentChanged != null) DocumentChanged(this, null);
            }
            catch (Exception)
            {
                MessageBox.Show("The File " + fileName + " could not be saved. The file may be read-only, or another user may have it open! Please save the document with a different name or in different folder!");
            }
        }
        public OpenDocumentData OpenDocument(OpenDocumentData open)
        {
            // Open the PDF, obtaining a password if required
            try
            {
                if (open.password == null)
                {
                    open.document = new Document(open.fileName);
                }
                else
                {
                    open.document = new Document(open.fileName, open.password, open.permission, true);
                }
                return open;
            }
            catch (Exception fileOpenException)
            {
                string exceptionMessage = fileOpenException.Message;

                // Trap password-required exception:
                if (exceptionMessage.Contains("1073938471"))
                {
                    // Loop until we get a valid password (or user cancels from dialog)
                    while (open.document == null)
                    {
                        // Request a password
                        frmPassword passwordForm = new frmPassword();
                        DialogResult passwordFormResult = passwordForm.ShowDialog();
                        if (passwordFormResult != DialogResult.OK) return null;

                        open.password = passwordForm.password;

                        try
                        {
                            open.document = new Document(open.fileName, open.password, open.permission, true);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show(
                                "The supplied password has insufficient permission.\n" +
                                "Please enter a different password.",
                                "Datalogics DotNETViewer");
                        }
                    }
                    return open;
                }
                else
                {
                    MessageBox.Show("Unable to open PDF document.", "Datalogics DotNETViewer");
                    return null;
                }
            }
        }
        public void OpenDocument()
        {
            if (!DocChangeRequest()) return;

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "PDF File (*.pdf)|*.pdf";
            fileDialog.Title = "Select a PDF file";
            DialogResult result = fileDialog.ShowDialog();
            if (result != DialogResult.OK) return;

            OpenDocumentData open = new OpenDocumentData(fileDialog.FileName, null, PermissionRequestOperation.Open);
            open = OpenDocument(open);
            if (open == null || open.document == null) return;

            Document = null;
            originalFileName = open.document.FileName;
            password = open.password;
            permission = open.permission;
            Document = open.document;

            editCommandStack.Reset();
            editCommandStack.OnUpdate(OnCommandStackChanged);
        }
        public void AppendDocument(OpenDocumentData append)
        {
            OpenDocumentData open = new OpenDocumentData(document.FileName, password, permission);
            NewAppendDocCommand.State state = new NewAppendDocCommand.State(this, open, append);
            NewAppendDocCommand command = new NewAppendDocCommand(this, state);
            Execute(command);
        }
        public void AppendDocument()
        {
            if (!document.PermRequest(PermissionRequestOperation.AllOperations))
            {
                MessageBox.Show("You must unlock this document before you can append.", "Datalogics DotNETViewer");
                return;
            }

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "PDF File (*.pdf)|*.pdf";
            fileDialog.Title = "Select a PDF file";
            DialogResult result = fileDialog.ShowDialog();
            if (result != DialogResult.OK) return;

            OpenDocumentData openDocumentData = new OpenDocumentData(fileDialog.FileName, null, PermissionRequestOperation.AllOperations);
            openDocumentData = OpenDocument(openDocumentData);
            if (openDocumentData == null || openDocumentData.document == null) return;

            AppendDocument(openDocumentData);
        }
        public void UnlockDocument()
        {
            if (document == null) return;

            // create a new password form
            frmPassword passwordForm = new frmPassword();

            // store the result that is recieved once the user closes the dialog
            DialogResult passwordResult = passwordForm.ShowDialog();

            // if the user did not cancel 
            if (passwordResult == DialogResult.OK)
            {
                String newPassword = passwordForm.password;

                // attempt to unlock the Document
                UnlockDocument(newPassword);
            }
        }
        private void UnlockDocument(String newPassword)
        {
            if (document == null) return;

            if (!this.Document.PermRequest(newPassword, PermissionRequestOperation.AllOperations))
            {
                MessageBox.Show(Parent, "Invalid Password for " + document.FileName + ".", "Password input");
            }
            else
            {
                password = newPassword;
                if (DocumentChanged != null) DocumentChanged(this, null);
            }
        }
        public void CloseDocument()
        {
            if (!DocChangeRequest()) return;
            Document = null;
            editCommandStack.Reset();
            editCommandStack.OnUpdate(OnCommandStackChanged);
        }
        public void ClearPageCache()
        {
            tileManager.Document = null;
        }
        public Page GetEditPage()
        {
            return editPage;
        }
        private Quad Concat(Quad a, Quad b)
        {
            return Utils.Clone(new Quad(a.TopLeft, b.TopRight, a.BottomLeft, b.BottomRight));
        }
        private IList<Quad> GetPageQuads(int pageNum)
        {
            if ((document == null) || (pageNum < 0) || (document.NumPages <= pageNum)) return null;

            List<Quad> quads = new List<Quad>();
            WordFinderConfig config = new WordFinderConfig();
            config.IgnoreCharGaps = true;
            config.NoAnnots = true;
            config.NoEncodingGuess = true;
            config.NoSkewedQuads = true;
            config.NoStyleInfo = true;
            config.PreserveSpaces = false;
            config.PreserveRedundantChars = false;
            config.DisableTaggedPDF = true;

            using (WordFinder wordFinder = new WordFinder(document, WordFinderVersion.Latest, config))
            {
                IList<Word> pageWords = null;
                pageWords = wordFinder.GetWordList(pageNum);
                if (pageWords == null) return null;
                foreach (Word word in pageWords)
                {
                    string text = word.Text;
                    bool letter = false;
                    Quad q = null;
                    IList<Quad> charQuads = word.CharQuads;
                    for (int i = 0; i < text.Length; ++i)
                    {
                        if (char.IsLetter(text, i) != letter)
                        {
                            if (q != null) quads.Add(q);
                            q = null;
                            letter = !letter;
                        }
                        if (q == null)
                        {
                            q = Utils.Clone(charQuads[i]);
                        }
                        else // check distance between quads - if we can concat them or not
                        {
                            const double interval = 0.5;
                            if (q.BottomRight.H + interval < charQuads[i].BottomLeft.H)
                            {
                                quads.Add(q);
                                q = Utils.Clone(charQuads[i]);
                            }
                            else
                            {
                                q = Concat(q, charQuads[i]);
                            }
                        }
                    }
                    if (q != null) quads.Add(q);
                }
            }
            if (quads.Count > 0) return quads;
            else return null;
        }
        public void DrawQuads(IList<Quad> quads, Graphics g, Matrix matrix)
        {
            if (quads == null) return;
            foreach (Quad quad in quads)
            {
                Datalogics.PDFL.Point bottomLeft = Utils.Transform(quad.BottomLeft, matrix);
                Datalogics.PDFL.Point bottomRight = Utils.Transform(quad.BottomRight, matrix);
                Datalogics.PDFL.Point topLeft = Utils.Transform(quad.TopLeft, matrix);
                Datalogics.PDFL.Point topRight = Utils.Transform(quad.TopRight, matrix);
                g.DrawLine(Pens.Red, (float)bottomLeft.H, (float)bottomLeft.V, (float)bottomRight.H, (float)bottomRight.V);
                g.DrawLine(Pens.Red, (float)bottomRight.H, (float)bottomRight.V, (float)topRight.H, (float)topRight.V);
                g.DrawLine(Pens.Red, (float)topRight.H, (float)topRight.V, (float)topLeft.H, (float)topLeft.V);
                g.DrawLine(Pens.Red, (float)topLeft.H, (float)topLeft.V, (float)bottomLeft.H, (float)bottomLeft.V);
            }
        }
        public System.Drawing.Point CalcPagePosition(int pageNumber, Datalogics.PDFL.Point location, bool withIndent)
        {
            Rect cropBox;
            using (Page page = Document.GetPage(pageNumber)) cropBox = page.CropBox;
            Rectangle pageRect = PageInfo[pageNumber].boundingRectangle;
            System.Drawing.Point pageLocation = pageRect.Location;
            pageLocation.X += (int)(pageRect.Width * (double.IsInfinity(location.H)? 0 : (location.H - cropBox.Left) / cropBox.Width));
            pageLocation.Y += (int)(pageRect.Height * (double.IsInfinity(location.V) ? 0 : (1 - (location.V - cropBox.Bottom) / cropBox.Height)));

            if (SinglePageMode)
            {
                if (pageRect.Width <= Size.Width) // no page move, as it's fully in the view
                    pageLocation.X = PageIndent;

                pageLocation.Y = (pageRect.Height <= Size.Height) ? pageRect.Top
                                                  : Math.Min(pageLocation.Y, pageRect.Bottom - Size.Height);
            }

            if (withIndent)
            {
                pageLocation.X -= PageIndent;
                pageLocation.Y -= PageIndent;
            }
            return pageLocation;
        }
        public int GetNumPattern(float[] pattern)
        {
            int pos = -1;
            for (int i = 0; i < patterns.Length && pattern != null && pos == -1; ++i)
            {
                float[] cur = patterns[i];
                bool samePattern = (cur.Length == pattern.Length);
                for (int j = 0; j < cur.Length && samePattern; ++j)
                    samePattern = samePattern && (cur[j] == pattern[j]);

                if (samePattern)
                    pos = i;
            }
            return pos;
        }
        public void SetCreateAnnotationMode(string subtype)
        {
            if (subtype == "") return;
            newAnnotationSubtype = subtype;
            ActiveAnnotation = null;
            FunctionMode = ApplicationFunctionMode.AnnotationShapeCreateMode;
        }
        /**
         * Translates page pdf coordinate to window coordinate.
         * To get doc view coordinate, add scroll to window coordinate 
         */
        public Matrix GetPagePdfToWinCoordMatrix(int pageNum)
        {
            if (pageNum == -1) return new Datalogics.PDFL.Matrix();

            using (Page page = document.GetPage(pageNum))
            {
                System.Drawing.Point location = tileManager.PageInfo[pageNum].boundingRectangle.Location;
                int w = tileManager.PageInfo[pageNum].boundingRectangle.Width;
                int h = tileManager.PageInfo[pageNum].boundingRectangle.Height;

                location.X -= tileManager.Scroll.X;
                location.Y -= tileManager.Scroll.Y;

                double scaleX = tileManager.GetPageZoom(pageNum) * tileManager.DPIScaleX;
                double scaleY = tileManager.GetPageZoom(pageNum) * tileManager.DPIScaleY;


                Datalogics.PDFL.Matrix matrix = new Datalogics.PDFL.Matrix();

                switch (page.Rotation)
                {
                    case PageRotation.Rotate0:
                        matrix = matrix
                            .Translate(location.X, location.Y)
                            .Scale(scaleX, -scaleY)
                            .Translate(-page.CropBox.LLx, -page.CropBox.URy);
                        break;
                    case PageRotation.Rotate90:
                        matrix = matrix
                            .Translate(location.X, location.Y)
                            .Scale(scaleX, -scaleY)
                            .Rotate(-90.0)
                            .Translate(-page.CropBox.LLx, -page.CropBox.LLy);

                        break;
                    case PageRotation.Rotate180:
                        matrix = matrix
                            .Translate(location.X, location.Y)
                            .Scale(scaleX, -scaleY)
                            .Rotate(-180.0)
                            .Translate(-page.CropBox.URx, -page.CropBox.LLy);
                        break;
                    case PageRotation.Rotate270:
                        matrix = matrix
                            .Translate(location.X, location.Y)
                            .Scale(scaleX, -scaleY)
                            .Rotate(-270.0)
                            .Translate(-page.CropBox.URx, -page.CropBox.URy);
                        break;

                }
                return matrix;
            }
        }
        /**
         * Translates page pixel coordinates to page pdf coordinates.
         */
        public Matrix GetPagePixelToPagePdfMatrix(int pageNum)
        {
            using (Page page = document.GetPage(pageNum))
            {
                double scaleX = tileManager.GetPageZoom(pageNum) * tileManager.DPIScaleX;
                double scaleY = tileManager.GetPageZoom(pageNum) * tileManager.DPIScaleY;

                Datalogics.PDFL.Matrix matrix = new Datalogics.PDFL.Matrix();

                switch (page.Rotation)
                {
                    case PageRotation.Rotate0:
                        matrix = matrix
                            .Translate(page.CropBox.LLx, page.CropBox.URy)
                            .Scale(1 / scaleX, -1 / scaleY);
                        break;
                    case PageRotation.Rotate90:
                        matrix = matrix
                            .Translate(page.CropBox.LLx, page.CropBox.LLy)
                            .Rotate(90.0)
                            .Scale(1 / scaleX, -1 / scaleY);
                        break;
                    case PageRotation.Rotate180:
                        matrix = matrix
                            .Translate(page.CropBox.URx, page.CropBox.LLy)
                            .Rotate(180.0)
                            .Scale(1 / scaleX, -1 / scaleY);
                        break;
                    case PageRotation.Rotate270:
                        matrix = matrix
                            .Translate(page.CropBox.URx, page.CropBox.URy)
                            .Rotate(270.0)
                            .Scale(1 / scaleX, -1 / scaleY);
                        break;

                }
                return matrix;
            }
        }
        public System.Drawing.Point ViewToPage(System.Drawing.Point location, int pageNum)
        {
            System.Drawing.Point pageLocation = PageInfo[pageNum].boundingRectangle.Location;
            System.Drawing.Point result = location;
            result.X -= pageLocation.X;
            result.Y -= pageLocation.Y;
            return result;
        }
        public Datalogics.PDFL.Point PageToPdf(System.Drawing.Point location, int pageNum)
        {
            return Utils.Transform(location, GetPagePixelToPagePdfMatrix(pageNum));
        }
        public Datalogics.PDFL.Point ViewToPdf(System.Drawing.Point location, int pageNum)
        {
            return PageToPdf(ViewToPage(location, pageNum), pageNum);
        }
        private ViewMode CreateViewModeHandler(ApplicationFunctionMode mode)
        {
            switch (mode)
            {
                case ApplicationFunctionMode.ScrollMode: return new ScrollViewMode(this);
                case ApplicationFunctionMode.TargetMode: return new TargetViewMode(this);
                case ApplicationFunctionMode.AnnotationEditMode: return new EditViewMode(this);
                case ApplicationFunctionMode.MarqueeZoomMode: return new MarqueeZoomMode(this);
                case ApplicationFunctionMode.ZoomMode: return new ZoomViewMode(this);
                case ApplicationFunctionMode.AnnotationShapeCreateMode:
                    // TODO: pass creation context instead of just subtype
                    switch (newAnnotationSubtype)
                    {
                        case "Circle":
                        case "Square":      return new CreateSimpleAnnotationMode(this, newAnnotationSubtype);
                        case "Line":        return new CreateLineAnnotationMode(this);
                        case "PolyLine":
                        case "Polygon":     return new CreatePolyAnnotationMode(this, newAnnotationSubtype);
                        case "FreeText":    return new CreateFreeTextAnnotationMode(this);
                        case "Link":        return new CreateLinkAnnotationMode(this);
                        case "Ink":         return new CreateInkAnnotationMode(this);
                        case "Underline":   return new CreateMarkupAnnotationMode(this, newAnnotationSubtype);
                        case "Highlight":   return new CreateMarkupAnnotationMode(this, newAnnotationSubtype);
                        default:            return new CreateSimpleAnnotationMode(this, newAnnotationSubtype);
                    }
                default: return new ScrollViewMode(this);
            }
        }
        public int GetPageByCoord(System.Drawing.Point location)
        {
            return tileManager.GetPageByCoord(location);
        }
        /**
         * DeleteAnnotation -
         * 
         * deletes the currently selected Annotation
         */
        public void DeleteAnnotation()
        {
            if (ActiveAnnotation == null) return;

            if (ActiveAnnotation is GroupAnnotationEditor)
            {
                GroupCommand groupCommand = new GroupCommand(this);
                GroupAnnotationEditor group = ActiveAnnotation as GroupAnnotationEditor;
                // editors in group is sorted by index, we must delete from max index to min
                for (int i = group.Count - 1; i >= 0; --i)
                {
                    BaseAnnotationEditor editor = group.Get(i);
                    DeleteAnnCommand.State state = new DeleteAnnCommand.State(this, editor);
                    DeleteAnnCommand command = new DeleteAnnCommand(this, state);
                    groupCommand.Add(command);
                }
                Execute(groupCommand);
            }
            else
            {
                DeleteAnnCommand.State state = new DeleteAnnCommand.State(this, ActiveAnnotation);
                DeleteAnnCommand command = new DeleteAnnCommand(this, state);
                if (underCreationFlag)
                {
                    underCreationFlag = false;
                    command.Do();
                }
                else Execute(command);
            }
            IsDocumentChanged = true;
            ActiveAnnotation = null;
            LoadAnnotations();
            tileManager.InvalidateCache();
            Invalidate();
        }
        /** 
         * ShowAnnotationProperties -
         * 
         * display a form containing the annotation title and contents
         * to the user. If the annotation is a FreeText Annotation allow
         * the user to modify the contents.
         */
        public void ShowAnnotationProperties()
        {
            if (ActiveAnnotation == null) return;
            AnnotationProperties props = ActiveAnnotation.Properties;

            frmAnnotationProperties annotationProperties = new frmAnnotationProperties(props.Title, props.Contents);

            // do not allow the user to modify the contents of a Free Text Annotation through the form
            // the user can double click on the Free Text Annotation to edit its contents in a TextBox

            // if the user does not have edit permissions on the PDF then do not allow them to edit
            // the contents or the title
            annotationProperties.annotationContentsTextBox.ReadOnly = !EditPermission || ActiveAnnotation.Properties.Subtype == "FreeText";
            annotationProperties.annotationTitleTextBox.ReadOnly = !EditPermission;

            DialogResult propertiesResult = annotationProperties.ShowDialog();

            if (propertiesResult == DialogResult.OK && EditPermission == true)
            {
                props.Contents = annotationProperties.AnnotationContents;
                props.Title = annotationProperties.AnnotationTitle;
            }
        }
        /**
         * ChooseColor -
         * 
         * display a ColorPicker form to the user so they can select
         * the stroke and fill colors to use for annotations.
         * 
         * If this is clicked on while an annotation is selected it
         * changes the selected annotations colors to those that the 
         * user selects.
         */
        public void ChooseColor(bool global)
        {
            if (!global && ActiveAnnotation == null) return;

            frmColorPicker colorPicker = new frmColorPicker();

            // pass the current colorPair to the form to display to the user
            colorPicker.savedColorPair = new ColorPairWithTransparencyFlags();
            colorPicker.savedColorPair.strokeColor = ActiveAnnotation is FreeTextAnnotationEditor ? ActiveProps.TextColor : ActiveProps.ForeColor;
            colorPicker.savedColorPair.fillColor = ActiveProps.InteriorColor;
            colorPicker.savedColorPair.fillColorTransparent = !ActiveProps.HasFill;

            if (colorPicker.ShowDialog() == DialogResult.OK)
            {
                ColorPairWithTransparencyFlags newAnnotationColor = colorPicker.colorPair.Clone();

                ++lockEditCommandsFlag;
                if (ActiveAnnotation is FreeTextAnnotationEditor)
                    ActiveProps.TextColor = newAnnotationColor.strokeColor;
                else
                    ActiveProps.ForeColor = newAnnotationColor.strokeColor;

                ActiveProps.InteriorColor = newAnnotationColor.fillColor;
                ActiveProps.HasFill = !newAnnotationColor.fillColorTransparent;
                --lockEditCommandsFlag;
                changedFlag = true;
                EditCheckPoint();
            }
        }
        /**
         * LinePattern - 
         * 
         * sets the Annotation LineStyle to needed value for the currently
         * selected Annotation or the Annotation about to be drawn
         */
        public void LinePattern(bool global, int value)
        {
            if (!global && ActiveAnnotation == null) return;

            if (value == 0)
                ActiveProps.Solid = true;
            else
                ActiveProps.DashPattern = patterns[value - 1];
        }
        /**
         * LineWidth - 
         * 
         * these set the Annotation LineWidth to a certain value for
         * either the currently selected Annotation or the Annotation
         * that is about to be drawn
         */
        public void LineWidth(bool global, int value)
        {
            if (!global && ActiveAnnotation == null) return;

            if (value == -1) // need dialog to choose line width
            {
                frmLineWidth lineWidthFrm = new frmLineWidth();

                // pass the current lineWidth to the form
                lineWidthFrm.lineWidth = (int)Math.Round(ActiveProps.LineWidth);

                DialogResult lineWidthResult = lineWidthFrm.ShowDialog();

                if (lineWidthResult == DialogResult.OK)
                    value = lineWidthFrm.lineWidth;
            }
            
            if (value > 0)
                ActiveProps.LineWidth = value;
        }
        private int GetPageByY(int y)
        {
            return tileManager.GetPageByY(y);
        }
        private BaseAnnotationEditor LoadAnnotation(Page page, int index)
        {
            BaseAnnotationEditor editor = AnnotationFactory.CreateAnnotationEditor(document, page, index);
            if (editor.Properties.IsAnnotationSupported &&
                (editor.Annotation.Flags & AnnotationFlags.Hidden) == 0 &&
                editor.GetType() != typeof(BaseAnnotationEditor))
            {
                if (editor is MarkupAnnotationEditor) (editor as MarkupAnnotationEditor).PageQuads = markupQuads;
                editor.UpdateBoundingRect();
                return editor;
            }
            else
            {
                return null;
            }
        }
        private void LoadAnnotations()
        {
            editAnnotations = new Dictionary<int, BaseAnnotationEditor>();
            if (editPage == null) return;
            int numAnnotations = editPage.NumAnnotations;
            for (int i = 0; i < numAnnotations; ++i)
            {
                BaseAnnotationEditor editor = LoadAnnotation(editPage, i);
                if (editor != null) editAnnotations.Add(i, editor);
            }
        }
        private System.Drawing.Point WindowToView(System.Drawing.Point point)
        {
            System.Drawing.Point result = point;
            result.X += Scroll.X;
            result.Y += Scroll.Y;
            return result;
        }
        public void queryUserForMonitorResolution()
        {
            frmMonitorResolution monitorResolutionForm = new frmMonitorResolution();
            monitorResolutionForm.monitorResolution = (int)tileManager.DpiX;
            DialogResult monitorResolutionResult = monitorResolutionForm.ShowDialog();

            if (monitorResolutionResult == DialogResult.OK)
            {
                Datalogics.PDFL.Point location = GetCurrentRelativeLocation();

                tileManager.DpiX = monitorResolutionForm.monitorResolution;
                tileManager.DpiY = monitorResolutionForm.monitorResolution;
                tileManager.UpdateFitZooms(Size);

                if (DpiChanged != null)
                    DpiChanged(this, null);
                
                Scroll = CalcPagePosition(CurrentPage, location, true);
                this.Invalidate();
            }
        }
        /*
         *Function for go to the bookmark or link annotation
         */
        public void GoToDestionation(ViewDestination destination)
        {
            FitModes fitMode = this.CurrentFit;
            double newZoom = this.Zoom;
            Datalogics.PDFL.Point location = new Datalogics.PDFL.Point(0, 0);
            switch (destination.FitType)
            {
                case "XYZ":
                    if (!double.IsInfinity(destination.Zoom) && Math.Abs(destination.Zoom) > 0.001)
                    {
                        newZoom = destination.Zoom;
                        fitMode = FitModes.FitNone;
                    }
                    location = new Datalogics.PDFL.Point(destination.DestRect.LLx, destination.DestRect.URy);
                    break;
                case "Fit":
                case "FitV":
                    fitMode = FitModes.FitPage;
                    location = new Datalogics.PDFL.Point();
                    break;
                case "FitH":
                    fitMode = FitModes.FitWidth;
                    location = new Datalogics.PDFL.Point(0, destination.DestRect.URy);
                    break;
                case "FitR":
                case "FitB":
                case "FitBH":
                case "FitBV":
                    fitMode = FitModes.FitNone;
                    location = new Datalogics.PDFL.Point(destination.FitType == "FitBH" ? 0 : destination.DestRect.LLx, 
                        destination.FitType == "FitBV" ? 0 : destination.DestRect.URy);

                    Rect fitR;
                    if (destination.FitType == "FitR")
                    {
                        fitR = destination.DestRect;
                    }
                    else
                    {
                        using (Page page = document.GetPage(destination.PageNumber)) fitR = page.BBox;
                    }
                    double wRatio = (double)Size.Width / (fitR.Width * DPI);
                    double hRatio = (double)Size.Height / (fitR.Height * DPI);
                    newZoom = Math.Min(wRatio, hRatio);
                    if (!double.IsInfinity(newZoom) && Math.Abs(newZoom) > 0.001)
                        newZoom = Math.Max(Math.Min(newZoom, GlobalConsts.MAX_PERCENT_ZOOM_LEVEL), GlobalConsts.MIN_PERCENT_ZOOM_LEVEL);
                    break;

                default:
                    MessageBox.Show("Selected bookmark has unsupported destination type.", "Datalogics DotNETViewer");
                    break;
            }
            if (newZoom != Zoom || fitMode != CurrentFit)
                RequestZoom(new ZoomEventArgs(newZoom, fitMode));
            Scroll = CalcPagePosition(destination.PageNumber, location, false);
        }
        private Datalogics.PDFL.Point GetCurrentRelativeLocation()
        {
            Rectangle currentPageRect = tileManager.PageInfo[currentPageNum].boundingRectangle;
            Datalogics.PDFL.Point location = new Datalogics.PDFL.Point((tileManager.Scroll.X - currentPageRect.X + PageIndent) / (tileManager.Zoom * tileManager.DPIScaleX),
                                                                   (tileManager.Scroll.Y - currentPageRect.Y + PageIndent) / (tileManager.Zoom * tileManager.DPIScaleY));

            Rect cropBox;
            using (Page page = Document.GetPage(CurrentPage)) cropBox = page.CropBox;
            return new Datalogics.PDFL.Point(location.H + cropBox.Left, cropBox.Top - location.V);
        }
        #endregion
        #region Events
        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();
            viewModeHandler.MouseDown(e, WindowToView(e.Location));
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            viewModeHandler.MouseUp(e, WindowToView(e.Location));
        }
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            viewModeHandler.MouseDoubleClick(e, WindowToView(e.Location));
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            viewModeHandler.MouseMove(e, WindowToView(e.Location));
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            //mouseInputHandler.MouseWheel(e, WindowToView(e.Location));
            base.OnMouseWheel(e);
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            viewModeHandler.KeyDown(e);
            base.OnKeyDown(e);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (e.ClipRectangle.IsEmpty) return;
            try
            {
                if (document == null) return;

                tileManager.CurrentPage = CurrentPage;
                tileManager.Draw(e.Graphics, e.ClipRectangle);

                Matrix currentPageMatrix = GetPagePdfToWinCoordMatrix(currentPageNum);
                Matrix editPageMatrix = GetPagePdfToWinCoordMatrix(editPageNum);
                if (this.IsSerchRectDraw && Quades.Length > 0) DrawSearchRects(e.Graphics, currentPageMatrix);

                //DrawQuads(markupQuads, e.Graphics, editPageMatrix);

                if (activeAnnotation != null)
                {
                    activeAnnotation.Zoom = (float)Zoom;
                    activeAnnotation.Rotation = (float)Rotation;
                    activeAnnotation.Draw(e.Graphics, editPageMatrix);
                }
                if (hoverAnnotation != null && hoverAnnotation != activeAnnotation)
                {
                    hoverAnnotation.DrawBoundingRect(e.Graphics, editPageMatrix);
                }

                if (functionMode == ApplicationFunctionMode.MarqueeZoomMode && !marqueeZoomRect.IsEmpty)
                {
                    Rectangle rect = marqueeZoomRect;
                    rect.X -= Scroll.X;
                    rect.Y -= Scroll.Y;
                    e.Graphics.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(64, 96, 96, 96)), rect);
                    e.Graphics.DrawRectangle(Pens.Black, rect);
                }

                if (debugMessage != null && debugMessage != "")
                {
                    System.Drawing.Font font = new System.Drawing.Font("Arial", 10);
                    SizeF textSize = e.Graphics.MeasureString(debugMessage, font);
                    int x = 5;
                    int y = 5;
                    Rectangle rect = new Rectangle(x, y, x + (int)textSize.Width, y + (int)textSize.Height);
                    e.Graphics.FillRectangle(Brushes.LightYellow, rect);
                    e.Graphics.DrawRectangle(Pens.Black, rect);
                    e.Graphics.DrawString(debugMessage, font, Brushes.Black, x + 2, y + 2);
                }
            }
            catch (ApplicationException ae)
            {
                if (ae.Message.Contains("Error number: 537001994"))
                    // use the existing exception as the inner exception
                    throw new ApplicationException("There was an error drawing the page. The Document may contain invalid objects or be corrupt.", ae);
                else
                    throw;
            }
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            tileManager.UpdateFitZooms(Size);
            Invalidate();
        }
        protected void AnnotationUpdateCallback(object appearanceUpdate)
        {
            if (AnnotationAppearance.AppearanceUpdate.ObjectHidden.Equals(appearanceUpdate) || AnnotationAppearance.AppearanceUpdate.ObjectShown.Equals(appearanceUpdate))
            {
                tileManager.InvalidateCache();
            }
            else
            {
                activeProps.SetFrom(ActiveAnnotation.Properties);
                IsDocumentChanged = true;
            }
            this.Invalidate();
        }
        protected void ActiveProperteisUpdateCallback(object unused)
        {
            if (ActiveAnnotation != null)
            {
                ActiveAnnotation.Properties.UpdateFrom(activeProps);
                ActiveAnnotation.Properties.RaiseUpdates(true);
                if (FunctionMode == ApplicationFunctionMode.AnnotationEditMode && lockEditCommandsFlag == 0)
                {
                    changedFlag = true;
                    SaveFinalEditState();
                }
            }
            else
                defaultAnnoProps.SetFrom(activeProps);
        }

        public void OnCommandStackChanged(EditCommandStack e)
        {
            if (editCommandStack == null) return;
            if (CommandStackUpdateCallback != null) CommandStackUpdateCallback(editCommandStack);
        }
       
        #endregion
        #region Members
        public event EventHandler PageChanged;
        public event EventHandler ScrollChanged;
        public event EventHandler ZoomChanged;
        public event EventHandler DpiChanged;
        public event EventHandler FunctionModeChanged;
        public event EventHandler DocumentChanged;

        private Datalogics.PDFL.Document document;
        private string originalFileName;
        private string password;
        private PermissionRequestOperation permission;

        private TileManager tileManager;
        private int currentPageNum;
        private int editPageNum;
        private Page editPage;

        private BaseAnnotationEditor activeAnnotation;
        private BaseAnnotationEditor hoverAnnotation;
        private Dictionary<int, BaseAnnotationEditor> editAnnotations;
        private AnnotationProperties activeProps;
        private AnnotationProperties defaultAnnoProps;

        private bool showMarkup = true;
        private IList<Datalogics.PDFL.Quad> markupQuads;

        // create annotation context
        private string newAnnotationSubtype;

        private ApplicationFunctionMode functionMode;
        private ViewMode viewModeHandler;
        private Rectangle marqueeZoomRect;

        // text search context
        private Quad[] quades;
        private bool isDrawSerchRect = false;
        private bool isDocumentChanged = false;
        private ToolTip toolTip = new ToolTip();

        // line dash patterns
        readonly float[] pattern1 = new float[] { 2.0f, 2.0f };
        readonly float[] pattern2 = new float[] { 3.0f, 3.0f };
        readonly float[] pattern3 = new float[] { 4.0f, 4.0f };
        readonly float[] pattern4 = new float[] { 4.0f, 3.0f, 2.0f, 3.0f };
        float[][] patterns;

        // debug
        private String debugMessage;

        // undo/redo
        EditCommandStack editCommandStack = new EditCommandStack();
        public event EditCommandStack.UpdateCallback CommandStackUpdateCallback;
        bool underCreationFlag = false;
        int lockEditCommandsFlag = 0;
        bool changedFlag = false;
        EditAnnCommand.State editAnnotationInitState;
        EditAnnCommand.State editAnnotationFinalState;
        IList<EditAnnCommand.State> editGroupAnnotationInitState;
        IList<EditAnnCommand.State> editGroupAnnotationFinalState;
        #endregion
        #region Tests
        public void CreateTestDocuments()
        {
            double pageWidth = 600;
            double pageHeight = 400;
            String[] strings = new String[]
            {
                "This is simple text,",
                "use it to test markup annotations.",
                "bla bla bla",
                "bla bla bla",
                "bla bla bla",
            };

            Document doc = new Document();
            Rect pageRect = new Rect(0, 0, pageWidth, pageHeight);
            Page page = doc.CreatePage(Document.BeforeFirstPage, pageRect);

            Path boldLines = new Path();
            GraphicState boldGS = boldLines.GraphicState;
            boldLines.PaintOp = PathPaintOpFlags.Stroke;
            boldGS.StrokeColor = new Datalogics.PDFL.Color(0, 0, 0);
            boldGS.Width = 0.5;
            boldLines.GraphicState = boldGS;

            Path thinLines = new Path();
            GraphicState thinGS = thinLines.GraphicState;
            thinLines.PaintOp = PathPaintOpFlags.Stroke;
            thinGS.StrokeColor = new Datalogics.PDFL.Color(0.7, 0.7, 0.7);
            thinGS.Width = 0.25;
            thinLines.GraphicState = thinGS;

            // horizontal lines
            for (int i = 0; i < 4; ++i)
            {
                boldLines.MoveTo(new Datalogics.PDFL.Point(0, 100 * i));
                boldLines.AddLine(new Datalogics.PDFL.Point(600, 100 * i));
                for (int j = 1; j < 5; ++j)
                {
                    thinLines.MoveTo(new Datalogics.PDFL.Point(0, 100 * i + 20 * j));
                    thinLines.AddLine(new Datalogics.PDFL.Point(600, 100 * i + 20 * j));
                }
            }
            // vertical lines
            for (int i = 0; i < 6; ++i)
            {
                boldLines.MoveTo(new Datalogics.PDFL.Point(100 * i, 0));
                boldLines.AddLine(new Datalogics.PDFL.Point(100 * i, 400));
                for (int j = 1; j < 5; ++j)
                {
                    thinLines.MoveTo(new Datalogics.PDFL.Point(100 * i + 20 * j, 0));
                    thinLines.AddLine(new Datalogics.PDFL.Point(100 * i + 20 * j, 400));
                }
            }
            boldLines.ClosePath();
            thinLines.ClosePath();

            Datalogics.PDFL.Font font = new Datalogics.PDFL.Font("Arial", FontCreateFlags.Embedded | FontCreateFlags.Subset);
            TextState textState = new TextState();
            textState.RenderMode = TextRenderMode.FillThenStroke;

            GraphicState gs = new GraphicState();
            gs.StrokeColor = new Datalogics.PDFL.Color(0, 0, 0);
            gs.Width = 0;

            gs.FillColor = new Datalogics.PDFL.Color(1, 0.5, 0.5);
            Text page1text = new Text();
            for (int i = 0; i < strings.GetLength(0); ++i)
            {
                String s = strings[i];
                Matrix m = new Matrix().Translate(20, pageHeight - 20 - 25 * (i + 1)).Scale(25, 25);
                page1text.AddRun(new TextRun(s, font, gs, textState, m));
            }
            page.Content.AddElement(thinLines);
            page.Content.AddElement(boldLines);
            page.Content.AddElement(page1text);
            page.UpdateContent();

            gs.FillColor = new Datalogics.PDFL.Color(0.5, 1, 0.5);
            Text page2text = new Text();
            for (int i = 0; i < strings.GetLength(0); ++i)
            {
                String s = strings[i];
                Matrix m = new Matrix().Translate(20, pageHeight - 20 - 25 * (i + 1)).Scale(25, 25);
                page2text.AddRun(new TextRun(s, font, gs, textState, m));
            }
            page = doc.CreatePage(0, pageRect);
            page.Content.AddElement(thinLines);
            page.Content.AddElement(boldLines);
            page.Content.AddElement(page2text);
            page.UpdateContent();

            doc.EmbedFonts(EmbedFlags.None);
            doc.Save(SaveFlags.Full, "test-document.pdf");
            doc.Secure(PermissionFlags.Open, "owner", "user");
            doc.Save(SaveFlags.Full, "test-document-pwd.pdf");
        }
        private void OpenTestPocument(String fileName)
        {
            if (document != null && document.FileName == fileName) return;

            OpenDocumentData open = new OpenDocumentData(fileName);
            open = OpenDocument(open);

            Document = null;
            originalFileName = open.document.FileName;
            password = open.password;
            permission = open.permission;
            Document = open.document;

            editCommandStack.Reset();
            editCommandStack.OnUpdate(OnCommandStackChanged);
        }
        private bool ApproxEqual(double a, double b, double delta)
        {
            return Math.Abs(a - b) < delta;
        }
        private bool ApproxEqual(Datalogics.PDFL.Point a, Datalogics.PDFL.Point b, double delta)
        {
            return ApproxEqual(a.H, b.H, delta) && ApproxEqual(a.V, b.V, delta);
        }
        private bool ApproxEqual(Datalogics.PDFL.Rect a, Datalogics.PDFL.Rect b, double delta)
        {
            return
                ApproxEqual(a.LLx, b.LLx, delta) && ApproxEqual(a.LLy, b.LLy, delta) &&
                ApproxEqual(a.Width, b.Width, delta) && ApproxEqual(a.Height, b.Height, delta);
        }
        private void TestCircle()
        {
            using (Page page = document.GetPage(0))
            {
                int expectedIndex = page.NumAnnotations;

                // create circle annotation
                defaultAnnoProps.LineWidth = 3;
                defaultAnnoProps.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
                defaultAnnoProps.InteriorColor = System.Drawing.Color.FromArgb(255, 0, 0);

                AnnotationProperties circle = new AnnotationProperties("Circle");
                Rect boundingRect = new Rect(400, 250, 500, 350);
                circle.BoundingRect = boundingRect;

                CreateAnnotation(page.PageNumber, circle);
                FinalizeAnnotationCreation();

                Debug.Assert(ActiveAnnotation.Index == expectedIndex);
                double delta = 0.001;
                Debug.Assert(ApproxEqual(ActiveAnnotation.Properties.BoundingRect, boundingRect, delta));
                // checking annotation transformation
                Matrix m = new Matrix().Translate(-100, -100);
                boundingRect = boundingRect.Transform(m);
                ActiveAnnotation.Transform(m);
                Debug.Assert(ApproxEqual(ActiveAnnotation.Properties.BoundingRect, boundingRect, delta));
                m = new Matrix().Scale(1.2, 0.8);
                boundingRect = boundingRect.Transform(m);
                ActiveAnnotation.Transform(m);
                Debug.Assert(ApproxEqual(ActiveAnnotation.Properties.BoundingRect, boundingRect, delta));
                // try to move annotation outside page bounds - this transformation must be rejected
                Rect oldRect = ActiveAnnotation.Properties.BoundingRect;
                ActiveAnnotation.Transform(new Matrix().Translate(-oldRect.LLx - 100, 0));
                Debug.Assert(ApproxEqual(ActiveAnnotation.Properties.BoundingRect, oldRect, delta));

                // save changes on page
                ActiveAnnotation = null;

                // checking annotation
                Debug.Assert(page.GetAnnotation(expectedIndex).Subtype == "Circle");
                CircleAnnotation ann = page.GetAnnotation(expectedIndex) as CircleAnnotation;

                // checking properties
                Debug.Assert(ann.Width == 3);
                Debug.Assert(ann.Color.Equals(new Datalogics.PDFL.Color(0, 0, 0)));
                Debug.Assert(ann.InteriorColor.Equals(new Datalogics.PDFL.Color(1, 0, 0)));
                ann = null;

                // checking command stack
                Debug.Assert(editCommandStack.CanUndo);
                Undo();
                Debug.Assert(editCommandStack.CanRedo);
                Redo();

                // changing properties
                SelectAnnotation(page.PageNumber, expectedIndex, true);
                Debug.Assert(ActiveAnnotation.Annotation.Subtype == "Circle");
                ActiveProps.LineWidth = 5;
                ActiveProps.ForeColor = System.Drawing.Color.FromArgb(255, 0, 255);
                ActiveProps.InteriorColor = System.Drawing.Color.FromArgb(0, 255, 255);

                // save changes on page
                ActiveAnnotation = null;
                ann = page.GetAnnotation(expectedIndex) as CircleAnnotation;
                // checking properties
                Debug.Assert(ann.Width == 5);
                Debug.Assert(ann.Color.Equals(new Datalogics.PDFL.Color(1, 0, 1)));
                Debug.Assert(ann.InteriorColor.Equals(new Datalogics.PDFL.Color(0, 1, 1)));
                ann = null;
            }
        }
        private void TestPolygon()
        {
            using (Page page = document.GetPage(0))
            {
                int expectedIndex = page.NumAnnotations;

                // create circle annotation
                defaultAnnoProps.LineWidth = 3;
                defaultAnnoProps.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
                defaultAnnoProps.InteriorColor = System.Drawing.Color.FromArgb(255, 0, 0);

                AnnotationProperties polygon = new AnnotationProperties("Polygon");
                IList<Datalogics.PDFL.Point> vertices = new List<Datalogics.PDFL.Point>();
                vertices.Add(new Datalogics.PDFL.Point(400, 50));
                vertices.Add(new Datalogics.PDFL.Point(500, 50));
                vertices.Add(new Datalogics.PDFL.Point(450, 150));
                polygon.Vertices = vertices;

                CreateAnnotation(page.PageNumber, polygon);
                FinalizeAnnotationCreation();

                Debug.Assert(ActiveAnnotation.Index == expectedIndex);

                // save changes on page
                ActiveAnnotation = null;

                // checking annotation
                Debug.Assert(page.GetAnnotation(expectedIndex).Subtype == "Polygon");
                PolygonAnnotation ann = page.GetAnnotation(expectedIndex) as PolygonAnnotation;

                // checking properties
                Debug.Assert(ann.Width == 3);
                Debug.Assert(ann.Color.Equals(new Datalogics.PDFL.Color(0, 0, 0)));
                Debug.Assert(ann.InteriorColor.Equals(new Datalogics.PDFL.Color(1, 0, 0)));
                Debug.Assert(ann.Vertices.Count == 3);
                ann = null;

                // checking command stack
                Debug.Assert(editCommandStack.CanUndo);
                Undo();
                Debug.Assert(editCommandStack.CanRedo);
                Redo();

                // changing properties
                SelectAnnotation(page.PageNumber, expectedIndex, true);
                Debug.Assert(ActiveAnnotation.Annotation.Subtype == "Polygon");
                ActiveProps.LineWidth = 5;
                ActiveProps.ForeColor = System.Drawing.Color.FromArgb(255, 0, 255);
                ActiveProps.InteriorColor = System.Drawing.Color.FromArgb(0, 255, 255);

                // save changes on page
                ActiveAnnotation = null;
                ann = page.GetAnnotation(expectedIndex) as PolygonAnnotation;
                // checking properties
                Debug.Assert(ann.Width == 5);
                Debug.Assert(ann.Color.Equals(new Datalogics.PDFL.Color(1, 0, 1)));
                Debug.Assert(ann.InteriorColor.Equals(new Datalogics.PDFL.Color(0, 1, 1)));
                Debug.Assert(ann.Vertices.Count == 3);
                ann = null;
            }
        }
        private void TestFreeText()
        {
            using (Page page = document.GetPage(0))
            {
                int expectedIndex = page.NumAnnotations;

                // create circle annotation
                defaultAnnoProps.FontFace = "Arial";
                defaultAnnoProps.FontSize = 12;
                defaultAnnoProps.ForeColor = System.Drawing.Color.FromArgb(0, 255, 0);
                defaultAnnoProps.InteriorColor = System.Drawing.Color.FromArgb(255, 255, 0);

                AnnotationProperties freeText = new AnnotationProperties("FreeText");
                freeText.BoundingRect = new Rect(100, 200, 200, 250);
                freeText.Contents = "Free text annotation";

                CreateAnnotation(page.PageNumber, freeText);
                FinalizeAnnotationCreation();

                Debug.Assert(ActiveAnnotation.Index == expectedIndex);

                // save changes on page
                ActiveAnnotation = null;

                // checking annotation
                Debug.Assert(page.GetAnnotation(expectedIndex).Subtype == "FreeText");
                FreeTextAnnotation ann = page.GetAnnotation(expectedIndex) as FreeTextAnnotation;

                // checking properties
                Debug.Assert(ann.Color.Equals(new Datalogics.PDFL.Color(0, 1, 0)));
                Debug.Assert(ann.Contents.Equals("Free text annotation"));
                ann = null;

                // checking command stack
                Debug.Assert(editCommandStack.CanUndo);
                Undo();
                Debug.Assert(editCommandStack.CanRedo);
                Redo();

                // changing properties
                SelectAnnotation(page.PageNumber, expectedIndex, true);
                Debug.Assert(ActiveAnnotation.Annotation.Subtype == "FreeText");
                ActiveProps.ForeColor = System.Drawing.Color.FromArgb(255, 255, 0);
                ActiveProps.InteriorColor = System.Drawing.Color.FromArgb(0, 255, 255);
                ActiveProps.Contents = "Revolution!";

                // save changes on page
                ActiveAnnotation = null;
                ann = page.GetAnnotation(expectedIndex) as FreeTextAnnotation;
                // checking properties
                Debug.Assert(ann.Color.Equals(new Datalogics.PDFL.Color(1, 1, 0)));
                Debug.Assert(ann.Contents.Equals("Revolution!"));
                ann = null;
            }
        }
        private void TestMarkup()
        {
            using (Page page = document.GetPage(0))
            {
                List<Quad> quads = new List<Quad>();
                WordFinderConfig config = new WordFinderConfig();
                config.IgnoreCharGaps = true;
                config.NoAnnots = true;
                config.NoEncodingGuess = true;
                config.NoSkewedQuads = true;
                config.NoStyleInfo = true;
                config.PreserveSpaces = false;
                config.PreserveRedundantChars = false;
                config.DisableTaggedPDF = true;

                using (WordFinder wordFinder = new WordFinder(document, WordFinderVersion.Latest, config))
                {
                    IList<Word> pageWords = null;
                    pageWords = wordFinder.GetWordList(page.PageNumber);
                    if (pageWords == null) return;
                    foreach (Word word in pageWords)
                    {
                        string text = word.Text;
                        bool letter = false;
                        Quad q = null;
                        IList<Quad> charQuads = word.CharQuads;
                        for (int i = 0; i < text.Length; ++i)
                        {
                            if (char.IsLetter(text, i) != letter)
                            {
                                if (q != null) quads.Add(q);
                                q = null;
                                letter = !letter;
                            }
                            if (q == null)
                            {
                                q = Utils.Clone(charQuads[i]);
                            }
                            else // check distance between quads - if we can concat them or not
                            {
                                const double interval = 0.5;
                                if (q.BottomRight.H + interval < charQuads[i].BottomLeft.H)
                                {
                                    quads.Add(q);
                                    q = Utils.Clone(charQuads[i]);
                                }
                                else
                                {
                                    q = Concat(q, charQuads[i]);
                                }
                            }
                        }
                        if (q != null) quads.Add(q);
                    }
                }

                // here we have quad list, now create annotation
                int expectedIndex = page.NumAnnotations;
                defaultAnnoProps.ForeColor = System.Drawing.Color.FromArgb(0, 255, 0);
                defaultAnnoProps.InteriorColor = System.Drawing.Color.FromArgb(255, 0, 0);
                AnnotationProperties markup = new AnnotationProperties("Highlight");
                // put every second quad that was found on page into markup annotation
                List<Quad> evenQuads = new List<Quad>();
                for (int i = 0; i < quads.Count; i += 2)
                {
                    evenQuads.Add(quads[i]);
                }
                markup.Quads = evenQuads;

                CreateAnnotation(page.PageNumber, markup);
                FinalizeAnnotationCreation();

                ActiveAnnotation = null;

                // checking annotation
                Debug.Assert(page.GetAnnotation(expectedIndex).Subtype == "Highlight");
                HighlightAnnotation ann = page.GetAnnotation(expectedIndex) as HighlightAnnotation;
                Debug.Assert(ann.Quads.Count == evenQuads.Count);
                ann = null;
            }
        }
        private void TestInk()
        {
            using (Page page = document.GetPage(0))
            {
                int expectedIndex = page.NumAnnotations;

                // create circle annotation
                defaultAnnoProps.ForeColor = System.Drawing.Color.FromArgb(0, 0, 255);
                defaultAnnoProps.LineWidth = 2;
                defaultAnnoProps.DashPattern = patterns[0];

                AnnotationProperties ink = new AnnotationProperties("Ink");
                // create 2 sine scribbles
                IList<IList<Datalogics.PDFL.Point>> scribbles = new List<IList<Datalogics.PDFL.Point>>();
                IList<Datalogics.PDFL.Point> sine1 = new List<Datalogics.PDFL.Point>();
                IList<Datalogics.PDFL.Point> sine2 = new List<Datalogics.PDFL.Point>();
                for (int i = 0; i < 300; ++i)
                {
                    double x = 0.02 * i;
                    double y1 = Math.Sin(x);
                    double y2 = Math.Cos(x);
                    sine1.Add(new Datalogics.PDFL.Point(100 + 50 * x, 200 + 100 * y1));
                    sine2.Add(new Datalogics.PDFL.Point(100 + 50 * x, 200 + 100 * y2));
                }
                scribbles.Add(sine1);
                scribbles.Add(sine2);
                ink.Scribbles = scribbles;

                CreateAnnotation(page.PageNumber, ink);
                FinalizeAnnotationCreation();

                Debug.Assert(ActiveAnnotation.Index == expectedIndex);

                // save changes on page
                ActiveAnnotation = null;

                // checking annotation
                Debug.Assert(page.GetAnnotation(expectedIndex).Subtype == "Ink");
                InkAnnotation ann = page.GetAnnotation(expectedIndex) as InkAnnotation;
                Debug.Assert(ann.Color.Equals(new Datalogics.PDFL.Color(0, 0, 1)));
                Debug.Assert(ann.Width == 2);
                Debug.Assert(ann.NumScribbles == 2);
                Debug.Assert(ann.GetScribble(0).Count == 300);
                Debug.Assert(ann.GetScribble(1).Count == 300);
                ann = null;

                // checking command stack
                Debug.Assert(editCommandStack.CanUndo);
                Undo();
                Debug.Assert(editCommandStack.CanRedo);
                Redo();

                // changing properties
                SelectAnnotation(page.PageNumber, expectedIndex, true);
                Debug.Assert(ActiveAnnotation.Annotation.Subtype == "Ink");
                ActiveProps.ForeColor = System.Drawing.Color.FromArgb(255, 0, 0);
                IList<IList<Datalogics.PDFL.Point>> newScribbles = new List<IList<Datalogics.PDFL.Point>>();
                IList<Datalogics.PDFL.Point> parabola1 = new List<Datalogics.PDFL.Point>();
                IList<Datalogics.PDFL.Point> parabola2 = new List<Datalogics.PDFL.Point>();
                IList<Datalogics.PDFL.Point> parabola3 = new List<Datalogics.PDFL.Point>();
                for (int i = 0; i < 100; ++i)
                {
                    double x = 0.02 * i - 1;
                    double y1 = x * x;
                    double y2 = 1 - y1;
                    double y3 = x * x * x - x * x;
                    parabola1.Add(new Datalogics.PDFL.Point(100 + 50 * x, 200 + 50 * y1));
                    parabola2.Add(new Datalogics.PDFL.Point(100 + 50 * x, 200 + 50 * y2));
                    parabola3.Add(new Datalogics.PDFL.Point(100 + 50 * x, 200 + 50 * y3));
                }
                newScribbles.Add(parabola1);
                newScribbles.Add(parabola2);
                newScribbles.Add(parabola3);
                ActiveProps.Scribbles = newScribbles;

                // save changes on page
                ActiveAnnotation = null;
                ann = page.GetAnnotation(expectedIndex) as InkAnnotation;
                // checking properties
                Debug.Assert(ann.Color.Equals(new Datalogics.PDFL.Color(1, 0, 0)));
                Debug.Assert(ann.NumScribbles == 3);
                Debug.Assert(ann.GetScribble(0).Count == 100);
                Debug.Assert(ann.GetScribble(1).Count == 100);
                Debug.Assert(ann.GetScribble(2).Count == 100);
                ann = null;
            }
        }
        private void TestLink()
        {
            using (Page page = document.GetPage(0))
            {
                int expectedIndex = page.NumAnnotations;

                // create circle annotation
                defaultAnnoProps.LineWidth = 3;
                defaultAnnoProps.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
                defaultAnnoProps.InteriorColor = System.Drawing.Color.FromArgb(255, 0, 0);

                AnnotationProperties link = new AnnotationProperties("Link");
                link.BoundingRect = new Rect(200, 150, 300, 200);
                // link destination: center of the next page, zoom = 300%
                link.ActionData = new ActionData(new ViewDestination(document, page.PageNumber + 1, "XYZ", new Rect(250, 50, 350, 150), 3));

                CreateAnnotation(page.PageNumber, link);
                FinalizeAnnotationCreation();

                Debug.Assert(ActiveAnnotation.Index == expectedIndex);

                // save changes on page
                ActiveAnnotation = null;

                // checking annotation
                Debug.Assert(page.GetAnnotation(expectedIndex).Subtype == "Link");
                LinkAnnotation ann = page.GetAnnotation(expectedIndex) as LinkAnnotation;

                // checking properties
                Debug.Assert(ann.Width == 3);
                Debug.Assert(ann.Color.Equals(new Datalogics.PDFL.Color(0, 0, 0)));
                GoToAction action = ann.Action as GoToAction;
                Debug.Assert(action.Destination.FitType == "XYZ");
                Debug.Assert(action.Destination.Zoom == 3);
                ann = null;

                // checking command stack
                Debug.Assert(editCommandStack.CanUndo);
                Undo();
                Debug.Assert(editCommandStack.CanRedo);
                Redo();

                // changing properties
                SelectAnnotation(page.PageNumber, expectedIndex, true);
                Debug.Assert(ActiveAnnotation.Annotation.Subtype == "Link");
                ActiveProps.LineWidth = 5;
                ActiveProps.ForeColor = System.Drawing.Color.FromArgb(255, 0, 255);
                // new link destination: upper left corner of the next page, zoom = 500%
                ActiveProps.ActionData = new ActionData(new ViewDestination(document, page.PageNumber + 1, "XYZ", new Rect(0, 300, 100, 400), 5));

                // save changes on page
                ActiveAnnotation = null;
                ann = page.GetAnnotation(expectedIndex) as LinkAnnotation;
                // checking properties
                Debug.Assert(ann.Width == 5);
                Debug.Assert(ann.Color.Equals(new Datalogics.PDFL.Color(1, 0, 1)));
                action = ann.Action as GoToAction;
                Debug.Assert(action.Destination.FitType == "XYZ");
                Debug.Assert(action.Destination.Zoom == 5);
                ann = null;
            }
        }
        private void TestGroup()
        {
            using (Page page = document.GetPage(0))
            {
                if (page.NumAnnotations < 3) return;

                ActiveAnnotation = null;
                // select single annotation
                SelectAnnotation(page.PageNumber, 0, true);
                Debug.Assert(ActiveAnnotation.Properties.Subtype == page.GetAnnotation(0).Subtype);
                // add annotation to selection
                SelectAnnotation(page.PageNumber, 1, false);
                Debug.Assert(ActiveAnnotation is GroupAnnotationEditor);
                Debug.Assert((ActiveAnnotation as GroupAnnotationEditor).Count == 2);
                // add another annotation
                SelectAnnotation(page.PageNumber, 2, false);
                Debug.Assert(ActiveAnnotation is GroupAnnotationEditor);
                Debug.Assert((ActiveAnnotation as GroupAnnotationEditor).Count == 3);
                // changing property of group annotation
                ActiveProps.ForeColor = System.Drawing.Color.FromArgb(255, 0, 0);
                // save changes on page
                ActiveAnnotation = null;
                // recreating group
                SelectAnnotation(page.PageNumber, 0, true);
                SelectAnnotation(page.PageNumber, 1, false);
                SelectAnnotation(page.PageNumber, 2, false);
                // checking all group members
                Debug.Assert(page.GetAnnotation(0).Color.Equals(new Datalogics.PDFL.Color(1, 0, 0)));
                Debug.Assert(page.GetAnnotation(1).Color.Equals(new Datalogics.PDFL.Color(1, 0, 0)));
                Debug.Assert(page.GetAnnotation(2).Color.Equals(new Datalogics.PDFL.Color(1, 0, 0)));
                // remove annotation 1 from group
                SelectAnnotation(page.PageNumber, 1, false);
                Debug.Assert(ActiveAnnotation is GroupAnnotationEditor);
                Debug.Assert((ActiveAnnotation as GroupAnnotationEditor).Count == 2);
                // remove annotation 0 from group, after that single annotation is selected
                SelectAnnotation(page.PageNumber, 0, false);
                Debug.Assert(!(ActiveAnnotation is GroupAnnotationEditor));
                Debug.Assert(ActiveAnnotation.Properties.Subtype == page.GetAnnotation(2).Subtype);
                // add annotations 0 & 1 again
                SelectAnnotation(page.PageNumber, 0, false);
                SelectAnnotation(page.PageNumber, 1, false);
                Debug.Assert(ActiveAnnotation is GroupAnnotationEditor);
                Debug.Assert((ActiveAnnotation as GroupAnnotationEditor).Count == 3);

                // transform annotation
                // (technically it is also possible to scale annotations, but user allowed only to move it)
                Matrix m = new Matrix().Translate(-10, 5);
                GroupAnnotationEditor group = ActiveAnnotation as GroupAnnotationEditor;
                List<Rect> boundingRects = new List<Rect>();
                boundingRects.Add(group.Get(0).Properties.BoundingRect);
                boundingRects.Add(group.Get(1).Properties.BoundingRect);
                boundingRects.Add(group.Get(2).Properties.BoundingRect);
                ActiveAnnotation.Transform(m);
                double delta = 0.001;
                double minX = 100000;
                for (int i = 0; i < 3; ++i)
                {
                    boundingRects[i] = boundingRects[i].Transform(m);
                    minX = Math.Min(minX, boundingRects[i].LLx);
                    Debug.Assert(ApproxEqual(group.Get(i).Properties.BoundingRect, boundingRects[i], delta));
                }
                // try to move group outside page bouds - operation must be rejected
                ActiveAnnotation.Transform(new Matrix().Translate(-minX - 100, 0));
                for (int i = 0; i < 3; ++i)
                {
                    Debug.Assert(ApproxEqual(group.Get(i).Properties.BoundingRect, boundingRects[i], delta));
                }

                // select single annotation
                SelectAnnotation(page.PageNumber, 0, true);
                Debug.Assert(!(ActiveAnnotation is GroupAnnotationEditor));
                Debug.Assert(ActiveAnnotation.Properties.Subtype == page.GetAnnotation(0).Subtype);

                ActiveAnnotation = null;
            }
        }
        public void TestAnnotations()
        {
            CloseDocument();
            OpenTestPocument("test-document.pdf");

            TestCircle();
            TestPolygon();
            TestFreeText();
            TestMarkup();
            TestInk();
            TestLink();
            TestGroup();

            // avoid Save document prompt
            isDocumentChanged = false;
            CloseDocument();
        }
        public delegate void TestFunction();
        public void RunTest(TestFunction Test)
        {
            try
            {
                Test();
                //MessageBox.Show("Tests passed", "Test results");
            }
            catch (Exception e)
            {
                MessageBox.Show("Tests failed:\n" + e.Message, "Test results");
            }
        }
        #endregion
    }
}
