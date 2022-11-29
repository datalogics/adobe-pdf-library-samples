/*
Copyright (C) 2008-2009, Datalogics, Inc. All rights reserved.
 
The information and code in this sample is for the exclusive use
of Datalogics customers and evaluation users only.  Datalogics 
permits you to use, modify and distribute this file in accordance 
with the terms of your license agreement. Sample code is for 
demonstrative purposes only and is not intended for production use.
*/

/*
 * DotNETController : 
 * 
 * DotNETController is essentially a "wrapper" around the other classes in this
 * project. DotNETController is responsible for coordinating between the user
 * interface and the other classes. The functions contained in this class are
 * the ones that should be called from a user interface. 
 * 
 * DotNETControllers purpose is to decouple the user interface from the code that
 * actually displays and modifies the data. This design should allow for easy
 * integration of a user interface or integrating DotNETViewer as a .Net Component
 * into an existing application. This should also allow features to be added on
 * easily in the future.
 */

using System;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using Datalogics.PDFL;

using System.Diagnostics;

#if HAS_DATALOGICS_PRINTING
using Datalogics.Printing;
#endif

namespace DotNETViewerComponent
{
    public struct TreeToBookmarkMapping
    {
        // Used in BookMarkManager.cs map BookMarks with the required 
        // action to move to the bookmark in the PDF.
        public TreeNode treeNode;
        public Datalogics.PDFL.Action action;
    };

    struct GlobalConsts
    {
        // Contains Constants that are used throughout DotNETViewer
        public const float MIN_PERCENT_ZOOM_LEVEL = 0.05f;
        public const float MAX_PERCENT_ZOOM_LEVEL = 8.0f;
        public const uint pdfDPI = 72;
    };

    public class DotNETController
    {
        #region Members
        private Datalogics.PDFL.Document document;
        public ZoomManager zoomManager;

        // custom scrolling
        private ScrollBar verticalScrollBar;
        private ScrollBar horizontalScrollBar;
        public DocumentView docView;

        /* these are references to objects in the User Interface that objects should
         * be displayed on.
         * 
         * displayPanel - the panel that the view of the document is drawn on
         * bookmarkControl - the panel that the bookmarks are displayed in
         * layersControl - the panel that the layers are displayed in
         */
        public Panel displayPanel;
        // tab control where bookmark and layer tabs are located
        public Control tabControl;
        public Control bookmarkControl;
        public Control layersControl;

        // various helper objects to minimize code here
        public BookmarkManager bookmarkManager;
        public LayersManager layersManager;
        public TextSearchManager textSearchManager;

        public event EventHandler DocumentChanged;
        // event + delegate to register to update controls that display the current page number
        public event PageChange onPageChange;
        public delegate void PageChange(String displayablePageNumber);
        #endregion

        #region Constructor
        public DotNETController(Panel displayPanel, Control bookmarkControl, Control layersControl, DocumentView docView)
        {
            this.displayPanel = displayPanel;
            this.tabControl = bookmarkControl.Parent;
            this.bookmarkControl = bookmarkControl;
            this.layersControl = layersControl;

            // initialize the manager objects
            zoomManager = new ZoomManager();
            bookmarkManager = new BookmarkManager(this);
            layersManager = new LayersManager(this);
            textSearchManager = new TextSearchManager(this);

            this.bookmarkControl.Controls.Add(bookmarkManager);
            this.layersControl.Controls.Add(layersManager);

            this.docView = docView;
            this.horizontalScrollBar = new HScrollBar();
            this.verticalScrollBar = new VScrollBar();
            this.displayPanel.Controls.Add(this.docView);
            this.displayPanel.Controls.Add(this.horizontalScrollBar);
            this.displayPanel.Controls.Add(this.verticalScrollBar);
            this.displayPanel.Resize += new EventHandler(displayPanel_Resize);

            this.horizontalScrollBar.ValueChanged += new EventHandler(horizontalScrollBar_ValueChanged);
            this.verticalScrollBar.ValueChanged += new EventHandler(verticalScrollBar_ValueChanged);
            this.docView.MouseWheel += new MouseEventHandler(docView_MouseWheel);
            this.docView.PageChanged += new EventHandler(docView_PageChanged);
            this.docView.ScrollChanged += new EventHandler(docView_ScrollChanged);
            this.docView.ZoomChanged += new EventHandler(docView_ZoomChanged);
            this.docView.DpiChanged +=new EventHandler(docView_DpiChanged);
            this.docView.DocumentChanged +=new EventHandler(docView_DocumentChanged);
        }
        #endregion
        #region Properties
        // refactored
        public Document Document
        {
            get { return document; }
        }
        public ApplicationFunctionMode ApplicationMode
        {
            get { return docView.FunctionMode; }
        }
        public String DisplayablePageNumber
        {
            get { return (docView.CurrentPage + 1).ToString(); }
        }
        public int NumberOfPages
        {
            get { return document.NumPages; }
        }
        public String DisplayableNumberOfPages
        {
            get { return NumberOfPages.ToString(); }
        }
        public bool PrintPermission
        {
            get { return document.PermRequest(PermissionRequestOperation.PrintHigh); }
        }
        public bool EditPermission
        {
            get { return document.PermRequest(PermissionRequestOperation.Modify); }
        }
        public bool AllPermission
        {
            get { return document.PermRequest(PermissionRequestOperation.AllOperations); }
        }
        public PageViewMode PageDisplayMode
        {
            get
            {
                if (docView.SinglePageMode) return PageViewMode.singlePage;
                else return PageViewMode.continuousPage;
            }

            set
            {
                bool singlePageMode = (value == PageViewMode.singlePage);
                if (docView.SinglePageMode != singlePageMode)
                {
                    docView.SinglePageMode = singlePageMode;

                    if (!docView.SinglePageMode)
                    {
                        if (docView.ViewSize.Height > docView.Size.Height &&
                            docView.ViewSize.Height < docView.Size.Height + docView.Scroll.Y)
                        {
                            docView.Scroll = new System.Drawing.Point(docView.Scroll.X, docView.ViewSize.Height - docView.Size.Height);
                        }
                    }

                    docView.Scroll = docView.Scroll; // recalculate page position once more
                    docView.Invalidate();
                }
            }
        }
        #endregion

        #region Methods
        public void SaveDocument()
        {
            if (document == null) return;
            Debug.WriteLine("saving document: " + document.FileName);
            try
            {
                document.Save(SaveFlags.Full);
                docView.IsDocumentChanged = false;
            }
            catch (Exception exc)
            {
                MessageBox.Show("The File " + document.FileName + " cannot be saved. " + exc.ToString());
            }
        }
        private void ResizeControls()
        {
            if (docView.Document == null)
            {
                docView.Visible = false;
                horizontalScrollBar.Visible = false;
                verticalScrollBar.Visible = false;
                return;
            }

            Size viewSize = docView.ViewSize;
            System.Drawing.Point viewScroll = docView.Scroll;
            Size clientSize = displayPanel.ClientRectangle.Size;
            Size pageSize = docView.PageInfo[docView.CurrentPage].boundingRectangle.Size;

            bool needsHScroll = horizontalScrollBar.Visible;
            bool needsVScroll = verticalScrollBar.Visible;
            bool hScrollVisible = false, vScrollVisible = false;
            int vertScrollExtent = 0, vertScrollHeight = 0;

            do {
                // recalculate client size if scroll visibility has changed
                if (needsHScroll != hScrollVisible)
                {
                    int mult = needsHScroll ? -1 : 1;
                    clientSize.Height += (horizontalScrollBar.Height * mult);
                }
                if (needsVScroll != vScrollVisible)
                {
                    int mult = needsVScroll ? -1 : 1;
                    clientSize.Width += (verticalScrollBar.Width * mult);
                }
                hScrollVisible = needsHScroll;
                vScrollVisible = needsVScroll;

                // vertical scrolling
                bool docHeightInView = viewSize.Height <= clientSize.Height;
                if (docView.SinglePageMode)
                {
                    bool pageHeightInView = pageSize.Height <= clientSize.Height;
                    vertScrollHeight = docView.ViewSize.Height - 2 * docView.PageIndent;
                    vertScrollExtent = pageHeightInView ? pageSize.Height : clientSize.Height;
                    needsVScroll = docView.Document.NumPages != 1 || !docHeightInView;
                }
                else
                {
                    vertScrollHeight = docView.ViewSize.Height;
                    vertScrollExtent = clientSize.Height;
                    needsVScroll = !docHeightInView;
                }

                // horizontal scrolling
                bool pageWidthInView = docView.SinglePageMode ? pageSize.Width <= clientSize.Width : docView.ViewSize.Width <= clientSize.Width;
                needsHScroll = !pageWidthInView;
            } while (needsHScroll != hScrollVisible || needsVScroll != vScrollVisible);

            docView.Size = clientSize;
            docView.Visible = true;

            if (pageSize != docView.PageInfo[docView.CurrentPage].boundingRectangle.Size) // page sizes may change in the fit mode when changing DocumentView size
            {
                docView.Scroll = new System.Drawing.Point(docView.PageInfo[docView.CurrentPage].boundingRectangle.X, docView.PageInfo[docView.CurrentPage].boundingRectangle.Y);
                ResizeControls();
                return;
            }
            
            horizontalScrollBar.Visible = needsHScroll;
            horizontalScrollBar.Location = new System.Drawing.Point(0, clientSize.Height);
            horizontalScrollBar.Width = clientSize.Width;
            horizontalScrollBar.Minimum = 0;
            horizontalScrollBar.Maximum = Math.Max(docView.ViewSize.Width, 0);
            horizontalScrollBar.SmallChange = 20;
            horizontalScrollBar.LargeChange = clientSize.Width;
            if (horizontalScrollBar.Visible) horizontalScrollBar.Value = Math.Min(viewScroll.X, horizontalScrollBar.Maximum);
            else horizontalScrollBar.Value = 0;

            verticalScrollBar.Visible = needsVScroll;
            verticalScrollBar.Location = new System.Drawing.Point(clientSize.Width, 0);
            verticalScrollBar.Height = clientSize.Height;
            verticalScrollBar.Minimum = 0;
            verticalScrollBar.Maximum = Math.Max(vertScrollHeight, 0);
            verticalScrollBar.SmallChange = 20;
            verticalScrollBar.LargeChange = vertScrollExtent;
            if (verticalScrollBar.Visible) verticalScrollBar.Value = Math.Min(viewScroll.Y, verticalScrollBar.Maximum);
            else verticalScrollBar.Value = 0;
        }
        /**
         * changeDotNETViewerApplicationMode - 
         * 
         * resets the state of DotNETViewerComponent to having no annotations highlighted/selected
         * and no textboxes for free text annotations created. Then changes the state to the
         * passed in value.
         */
        public void changeDotNETViewerApplicationMode(ApplicationFunctionMode nextApplicationMode)
        {
            // assign the new Application Mode
            docView.FunctionMode = nextApplicationMode;
        }
        /**
         * openDocument - 
         * 
         * Should be called to open a document. This will display a 
         * dialog box to the user and ask for the documents name. In
         * the case that the document is password protected, this 
         * function will prompt the user for the password and attempt
         * to open it using openPasswordProtectedDocument().
         */
        public bool showPanel = false;

        /**
         * unlockDocument - 
         * 
         * elevates the permission level to all operations when the
         * correct password is supplied.
         */
        public void unlockDocument(String password)
        {
            // if the document is null do not attempt to unlock
            if (this.Document != null)
            {
                if (!this.Document.PermRequest(password, PermissionRequestOperation.AllOperations))
                    MessageBox.Show(displayPanel.Parent, "Invalid Password for " + this.Document.FileName + ".", "Password input");
            }
        }

        /**
         * printDocument - 
         * 
         * displays a print dialog in order to print the current document
         */
        public void printDocument()
        {
            // check for a null document
            if (document == null)
            {
                MessageBox.Show("A PDF must be open in order to print.", "Datalogics DotNETViewer");
                return;
            }

            // continuous page printing not supported
            if (PageDisplayMode == PageViewMode.continuousPage)
            {
                MessageBox.Show("Unable to Print in Continuous Page Mode", "Print Error", MessageBoxButtons.OK);
                return;
            }

            try
            {
                double docGreatestWidthInPrinterRezUnits = 0.0;
                double docGreatestWidthInInches = 0.0;
                double docGreatestWidthInPDFUserUnits = 0.0;

                double docHeightInInches = 0.0;
                double docHeightInPrinterRezUnits = 0.0;
                double docHeightInPDFUserUnits = 0.0;

                Graphics graphics;
                uint resolutionX;
                uint resolutionY;

                // DLADD wrl 6/24/08 -- Use DotNET shrinkToFit flag to handle scaling
                bool shrinkToFit = false;

                PrintDocument printDocument = new PrintDocument();

                // create the settings to use while printing
                PrinterSettings printerSettings = printDocument.PrinterSettings;
                printerSettings.MinimumPage = 1;
                printerSettings.MaximumPage = document.NumPages;
                printerSettings.PrintRange = PrintRange.AllPages;
                printerSettings.FromPage = 1;
                printerSettings.ToPage = document.NumPages;

                // create a print dialog
                DotNETPrintDialog dpd = new DotNETPrintDialog();
                dpd.Document = printDocument;
                dpd.AllowSelection = false;
                dpd.AllowSomePages = true;

                if (PageDisplayMode == PageViewMode.singlePage)
                {
                    dpd.AllowShrinkToFit = true;
                    dpd.ShrinkToFit = true;
                }

                // display the print dialog if the user does not
                // click "OK" throw a new exception to cancel printing
                if (dpd.ShowDialog() != DialogResult.OK)
                    throw new CancelPrinting();

                shrinkToFit = dpd.ShrinkToFit;

                graphics = printerSettings.CreateMeasurementGraphics();
                resolutionX = (uint)graphics.DpiX;
                resolutionY = (uint)graphics.DpiY;

                // grab some additional parameters if printing to a file
                if (printerSettings.PrintToFile)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.DefaultExt = ".plt";
                    saveFileDialog.FileName = "plot.plt";

                    if (saveFileDialog.ShowDialog() != DialogResult.OK)
                        throw new CancelPrinting();
                    else
                        printerSettings.PrintFileName = saveFileDialog.FileName;
                }

                // find the dimensions we need for printing
                docGreatestWidthInPDFUserUnits = 0;
                for (int i = 0; i < document.NumPages; ++i)
                {
                    using (Page page = document.GetPage(i)) docGreatestWidthInPDFUserUnits = Math.Max(docGreatestWidthInPDFUserUnits, page.CropBox.Width);
                }
                docHeightInPDFUserUnits = 0;
                for (int i = printerSettings.FromPage - 1; i <= printerSettings.ToPage - 1; ++i)
                {
                    using (Page page = document.GetPage(i)) docHeightInPDFUserUnits += page.CropBox.Height;
                }

                docGreatestWidthInInches = docGreatestWidthInPDFUserUnits / (double)GlobalConsts.pdfDPI;
                docHeightInInches = docHeightInPDFUserUnits / (double)GlobalConsts.pdfDPI;

                docGreatestWidthInPrinterRezUnits = docGreatestWidthInInches * (double)resolutionX;
                docHeightInPrinterRezUnits = docHeightInInches * (double)resolutionY;

                if (PageDisplayMode == PageViewMode.singlePage)
                {
#if HAS_DATALOGICS_PRINTING
                    Printer S_printer = new Printer(printerSettings.PrinterName);

                    if(S_printer.HardwareID == "isysij18")
                    {
                        // special workaround for iSys iTerra IJ1800
                        // use our special print controller to print via unmanaged code
                        PrintController pc = new DLPrinterPrintController(S_printer, printDocument.PrinterSettings, document.fileName);
                        printDocument.PrintController = new PrintControllerWithStatusDialog(pc);
                    }
                    else
                        S_printer.Dispose();
#endif

                    // User can control printing of OCGs
                    DrawParams drawParams = new DrawParams();

                    for (int j = 0; j < layersManager.ocgStates.Count; j++)
                        layersManager.ocgStates[j] = layersManager.layersInDocument[j].printLayer.Checked;

                    layersManager.docLayerContext.SetOCGStates(layersManager.docLayers, layersManager.ocgStates);
                    drawParams.OptionalContentContext = layersManager.docLayerContext;

                    // create the print controller
                    DotNETPrintController ppc = new DotNETPrintController(document, printDocument, drawParams, true, shrinkToFit);
                    
                    // start the printing
                    ppc.Print();
                }
            }
            catch (CancelPrinting)
            {
                // Ignore, user cancelled printing
            }
        }

        public void GoToPosition(System.Drawing.Point location)
        {
            if (horizontalScrollBar.Visible)
            {
                horizontalScrollBar.Value =
                    Math.Min(horizontalScrollBar.Maximum - horizontalScrollBar.LargeChange, location.X);
            }

            if (verticalScrollBar.Visible)
            {
                verticalScrollBar.Value =
                    Math.Min(verticalScrollBar.Maximum - verticalScrollBar.LargeChange, location.Y);
            }
        }
        public void goToPage(int pageNumber)
        {
            System.Drawing.Point pageLocation = docView.PageInfo[pageNumber].boundingRectangle.Location;
            pageLocation.X -= docView.PageIndent;
            pageLocation.Y -= docView.PageIndent;
            GoToPosition(pageLocation);
            docView.Focus();
        }
        public void firstPage()
        {
            goToPage(0);
        }
        public void lastPage()
        {
            goToPage(document.NumPages - 1);
        }
        public void previousPage()
        {
            int pageNumber = docView.CurrentPage;
            if (pageNumber > 0) goToPage(pageNumber - 1);
        }
        public void nextPage()
        {
            int pageNumber = docView.CurrentPage;
            if (pageNumber < document.NumPages - 1) goToPage(pageNumber + 1);
        }

        /**
         * changeResolutionDPI -
         * 
         * Used to change the DPI that is used to calculate dimensions
         * of the view to display.
         */
        public void changeResolutionDPI()
        {
            // ask the view to query the user for the new
            // monitor resolution
            docView.queryUserForMonitorResolution();
        }

        /**
         * changeZoomLevel(int) - 
         * 
         * When the Zoom Level changes this is the function to call
         * passing it the index of the zoom list. This function will
         * destroy and then recreate the pageSizeAndLocationArray with
         * the new zoom level and then redraw the page on the screen
         */
        public void changeZoomLevel(int selectedZoomIndex)
        {
            if (document == null) return;
            zoomManager.zoomLevelChanged(selectedZoomIndex);
            docView.setZoom(zoomManager.fit, zoomManager.currentZoomFactor);
            docView.Invalidate();
        }

        /**
         * search(String) - 
         * 
         * Searches the current document for the passed in string and
         * highlights the results by drawing a blue rectangle around
         * them.
         */
        public void search(String searchString)
        {
            // pass the searchString to the Text Search Manager
            textSearchManager.SearchString = searchString;

            // perform the search
            textSearchManager.performSearch();
        }       
        #endregion

        #region Events
        void docView_DocumentChanged(object sender, EventArgs e)
        {
            document = docView.Document;
            if (bookmarkManager != null) bookmarkManager.DestroyBookmarkTree();
            if (layersManager != null) layersManager.DestroyLayers();

            // set the search word for the TextSearchManager to null to force
            // the user to click the search button to search the new document
            textSearchManager.SearchString = "";

            bool show1 = true;
            bool show2 = true;

            if (tabControl.Controls.Contains(bookmarkControl)) tabControl.Controls.Remove(bookmarkControl);
            if (tabControl.Controls.Contains(layersControl)) tabControl.Controls.Remove(layersControl);

            bookmarkManager.CreateBookmarkTree(docView.Document);
            layersManager.CreateLayerItems(docView.Document);

            if (bookmarkManager.numberOfBookmarks > 0) tabControl.Controls.Add(bookmarkControl);
            else show1 = false;
            if (layersManager.layersInDocument.Count > 0) tabControl.Controls.Add(layersControl);
            else show2 = false;
            showPanel = show1 || show2;

            if (document != null) changeZoomLevel(zoomManager.zoomArray.Length - 1);
            ResizeControls();
            horizontalScrollBar.Value = 0;
            verticalScrollBar.Value = 0;
            if (DocumentChanged != null) DocumentChanged(this, null);
        }
        void docView_DpiChanged(object sender, EventArgs e)
        {
            ResizeControls();
        }
        void docView_ZoomChanged(object sender, EventArgs e)
        {
            ZoomEventArgs zoomArgs = e as ZoomEventArgs;
            zoomManager.setCustomZoom(zoomArgs.Mode == ZoomEventArgs.ZoomMode.CUSTOM ? zoomArgs.Zoom : docView.Zoom);

            switch (zoomArgs.Mode)
            {
                case ZoomEventArgs.ZoomMode.INCREASE:
                    zoomManager.increaseZoom();
                    break;
                case ZoomEventArgs.ZoomMode.DECREASE:
                    zoomManager.decreaseZoom();
                    break;
                case ZoomEventArgs.ZoomMode.CUSTOM:
                    docView.setZoom(zoomArgs.Fit, zoomArgs.Zoom);

                    if (zoomArgs.Fit == FitModes.FitNone)
                        zoomManager.onCustomZoomLevelSelected();
                    else
                        zoomManager.onZoomLevelChanged(zoomArgs.Fit == FitModes.FitPage ? 12 : 11);
                    break;
            }
        }
        void docView_PageChanged(object sender, EventArgs e)
        {
            if (docView.IsSerchRectDraw)
                if (textSearchManager.SearchString != null)
                    textSearchManager.performSearch();
            
            if (onPageChange != null)
                onPageChange((docView.CurrentPage + 1).ToString());
        }
        void docView_ScrollChanged(object sender, EventArgs e)
        {
            ResizeControls();
            horizontalScrollBar.Value = docView.Scroll.X;
            verticalScrollBar.Value = docView.Scroll.Y;
        }
        void docView_MouseWheel(object sender, MouseEventArgs e)
        {
            int newValue = verticalScrollBar.Value - e.Delta;
            newValue = Math.Min(verticalScrollBar.Maximum - verticalScrollBar.LargeChange, newValue);
            newValue = Math.Max(verticalScrollBar.Minimum, newValue);
            verticalScrollBar.Value = newValue;
        }
        void verticalScrollBar_ValueChanged(object sender, EventArgs e)
        {
            System.Drawing.Point scroll = docView.Scroll;
            scroll.Y = verticalScrollBar.Value;
            docView.Scroll = scroll;
            docView.Invalidate();
        }
        void horizontalScrollBar_ValueChanged(object sender, EventArgs e)
        {
            System.Drawing.Point scroll = docView.Scroll;
            scroll.X = horizontalScrollBar.Value;
            docView.Scroll = scroll;
            docView.Invalidate();
        }
        void displayPanel_Resize(object sender, EventArgs e)
        {
            ResizeControls();
        }
        /**
         * bookmarkTree_AfterSelect - 
         * 
         * Occurs when a user clicks on a node in the bookmark treeview,
         * but only if they are not expanding a node.
         */
        public void bookmarkTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.ByMouse || e.Action == TreeViewAction.ByKeyboard)
            {
                int i = 0;

                while (i < bookmarkManager.numberOfBookmarks && e.Node.Handle != bookmarkManager.bookmarkMappingArray[i].treeNode.Handle)
                    i++;

                if (i >= bookmarkManager.numberOfBookmarks)
                    return;

                // get the bookmark information
                if (bookmarkManager.bookmarkMappingArray[i].action != null &&
                    bookmarkManager.bookmarkMappingArray[i].action is GoToAction &&
                    ((GoToAction)bookmarkManager.bookmarkMappingArray[i].action).Destination != null)
                {
                    ViewDestination destination = ((GoToAction)bookmarkManager.bookmarkMappingArray[i].action).Destination;
                    docView.GoToDestionation(destination);
                   /* FitModes fitMode = FitModes.FitNone;
                    double newZoom = docView.Zoom;
                    int zoomIndex = 12;
                    Datalogics.PDFL.Point location = new Datalogics.PDFL.Point(0, 0);
                    switch (destination.FitType)
                    {
                        case "XYZ":
                            if (!double.IsInfinity(destination.Zoom) && Math.Abs(destination.Zoom) > 0.001)
                                newZoom = destination.Zoom;
                            fitMode = FitModes.FitNone;
                            location = new Datalogics.PDFL.Point(destination.DestRect.LLx, destination.DestRect.URy);
                            break;
                        case "Fit":
                        case "FitV":
                            fitMode = FitModes.FitPage;
                            location = new Datalogics.PDFL.Point();
                            break;
                        case "FitH":
                            fitMode = FitModes.FitWidth;
                            zoomIndex = 11;
                            location = new Datalogics.PDFL.Point(0, destination.DestRect.URy);
                            break;
                        case "FitR":
                        case "FitB":
                        case "FitBH":
                        case "FitBV":
                            fitMode = FitModes.FitNone;
                            location = new Datalogics.PDFL.Point(destination.FitType == "FitBH" ? 0 : destination.DestRect.URx, destination.FitType == "FitBV" ? 0 : destination.DestRect.URy);

                            Rect fitR = destination.FitType == "FitR" ? destination.DestRect : document.GetPage(destination.PageNumber).BBox;
                            double wRatio = (double)docView.Size.Width / (fitR.Width * docView.DPI);
                            double hRatio = (double)docView.Size.Height / (fitR.Height * docView.DPI);
                            newZoom = Math.Min(wRatio, hRatio);
                            if (!double.IsInfinity(newZoom) && Math.Abs(newZoom) > 0.001)
                                newZoom = Math.Max(Math.Min(newZoom, GlobalConsts.MAX_PERCENT_ZOOM_LEVEL), GlobalConsts.MIN_PERCENT_ZOOM_LEVEL);
                            break;

                        default:
                            MessageBox.Show("Selected bookmark has unsupported destination type.", "Datalogics DotNETViewer");
                            break;
                    }
                    docView.setZoom(fitMode, newZoom);
                    docView.Invalidate();

                    if (fitMode == FitModes.FitNone)
                    {
                        zoomManager.setCustomZoom(docView.Zoom);
                        zoomManager.onCustomZoomLevelSelected();
                    }
                    else
                        zoomManager.onZoomLevelChanged(zoomIndex);
                    GoToPosition(docView.CalcPagePosition(destination.PageNumber, location));*/
                }
                else
                {
                    MessageBox.Show("Selected bookmark does not have a valid destination in the document.",
                        "Datalogics DotNETViewer");
                }
            }
        }

        /**
         * displayLayer_Click - 
         * 
         * Occurs when a user clicks the display checkbox for any layer
         * in the layers tab control page.
         */
        public void displayLayer_Click(object sender, EventArgs e)
        {
            CheckBox tempCheckBox = (CheckBox)sender;

            // if it is the "master" control that is clicked
            if (tempCheckBox.Name == "Master")
            {
                if (layersManager.masterLayer.displayLayer.CheckState == CheckState.Indeterminate)
                    layersManager.masterLayer.displayLayer.CheckState = CheckState.Unchecked;

                for (int i = 0; i < layersManager.layersInDocument.Count; i++)
                    layersManager.layersInDocument[i].displayLayer.CheckState = layersManager.masterLayer.displayLayer.CheckState;
            }
            // else it is one of the other layers
            else
            {
                CheckState currentState = layersManager.layersInDocument[0].displayLayer.CheckState;

                for (int i = 1; i < layersManager.layersInDocument.Count; i++)
                {
                    if (layersManager.layersInDocument[i].displayLayer.CheckState != layersManager.layersInDocument[0].displayLayer.CheckState)
                    {
                        currentState = CheckState.Indeterminate;
                        break;
                    }
                }

                if (layersManager.masterLayer != null) layersManager.masterLayer.displayLayer.CheckState = currentState;
            }

            for (int i = 0; i < layersManager.ocgStates.Count; i++)
            {
                layersManager.ocgStates[i] = layersManager.layersInDocument[i].displayLayer.Checked;
            }
            layersManager.docLayerContext.SetOCGStates(layersManager.docLayers, layersManager.ocgStates);
            docView.OptionalContentContext = layersManager.docLayerContext;
            docView.Invalidate();
        }

        /**
         * printLayer_Click - 
         * 
         * Occurs when a user clicks a print checkbox for any print
         * layer in the document.
         */
        public void printLayer_Click(object sender, EventArgs e)
        {
            CheckBox tempCheckBox = (CheckBox)sender;

            // if it is the "master" control that is clicked
            if (tempCheckBox.Name == "Master")
            {
                if (layersManager.masterLayer.printLayer.CheckState == CheckState.Indeterminate)
                    layersManager.masterLayer.printLayer.CheckState = CheckState.Unchecked;

                for (int i = 0; i < layersManager.layersInDocument.Count; i++)
                    layersManager.layersInDocument[i].printLayer.CheckState = layersManager.masterLayer.printLayer.CheckState;
            }
            // else it is one of the other layers
            else
            {
                CheckState currentState = layersManager.layersInDocument[0].printLayer.CheckState;

                for (int i = 1; i < layersManager.layersInDocument.Count; i++)
                {
                    if (layersManager.layersInDocument[i].printLayer.CheckState != layersManager.layersInDocument[0].printLayer.CheckState)
                    {
                        currentState = CheckState.Indeterminate;
                        break;
                    }
                }
                
                if (layersManager.masterLayer != null)
                    layersManager.masterLayer.printLayer.CheckState = currentState;
            }
            docView.Invalidate();
        }

        /**
         * resizeLayerControls - 
         * 
         */
        public void resizeLayerControls()
        {
            if (layersManager != null)
            {
                layersManager.ResizeLayerControls();
            }

            layersControl.Invalidate();
        }
        #endregion

        #region Dispose
        public void Dipose()
        {
            if (docView != null)
            {
                if (displayPanel.Controls.Contains(docView)) displayPanel.Controls.Remove(docView);
                docView.Dispose();
                docView = null;
            }
            bookmarkManager.Dispose();
            layersManager.Dispose();
        }
        #endregion
    }
}
