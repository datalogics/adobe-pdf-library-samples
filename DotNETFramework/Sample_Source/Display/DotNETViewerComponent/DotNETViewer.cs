/*
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
*/

/*
 * DotNETViewer.cs
 * 
 * DotNETViewer is a User Control that can be added to an existing application from
 * the toolbox in visual studio. It provides a graphical user interface for users
 * to view/edit PDFs.
 * 
 *  Featues :
 *      - View PDF
 *          - Single Page and Continuous Page Modes
 *      - Edit Annotations in a PDF
 *          - Supports the following Annotations
 *              - Ellipse
 *              - Rectangle
 *              - Line
 *              - Arrow
 *              - Polygon
 *              - PolyLine
 *              - Free Text
 *          - Supports the following Arrowheads for the Arrow and PolyLine Annotations
 *              - None
 *              - Circle
 *              - Open Arrow
 *              - Closed Arrow
 *      - Appending PDFs to one another
 *      - Save and SaveAs
 *      - Unlocking a PDF to a higher permission level
 *      - Print PDF
 *          - Single Page Mode ONLY
 *      - Navigation using Bookmarks
 *          - ONLY supports GoTo Bookmarks that have a valid destination
 *          - Does NOT support adding/editing Bookmarks
 *      - Control for which Layers are displayed and which Layers are printed
 *      - Zoom (Marquee and In/Out based on preset zoom levels)
 *      - Searching the PDF for a String of alphanumeric characters (using WordFinder)
 *
 *  Editing Supported Annotations consists of the following supported properties :
 *      - Line Width
 *      - Line Style
 *      - Color
 *          - Fill 
 *          - Stroke
 *          - Transparent Fill
 *      - Position
 *      - Size
 *      - Title
 *      - Contents
 *      - Arrowhead (for those Annotations that have Arrowheads)
 *      - Font - Free Text Annotations ONLY
 *      - Font Size - Free Text Annotations ONLY
 *      - Font Alignment/Justification - Free Text Annotations ONLY
 * 
 *  Shortcut Keys :
 *      - Ctrl + O - Open File Dialog
 *      - Ctrl + S - Save File Dialog
 *      - Ctrl + P - Print File Dialog
 *      - Ctrl + C - Close Current Document
 *      - Ctrl + Alt + P - Toggle Bookmark/Layer Control
 *      - Ctrl + Alt + S - Single Page Mode
 *      - Ctrl + Alt + C - Continuous Page Mode
 *      - Del - Delete the Annotation that is selected
 *      - Esc - Cancel editing the selected Annotation (Saves the changes made to the Annotation)
 *          - While creating a Free Text, Polygon, or PolyLine Annotation Esc will cancel the creation process
 *      - Page Up - Moves to the previous page
 *      - Page Down - Moves to the next page
 *      - Home - Moves to the first page in the PDF
 *      - End - Moves to the last page in the PDF
 *      - Arrow Keys - Scroll the document in the direction of the Arrow
 * 
 *  Known Bugs/Issues :
 *      - At zoom levels of 200% or more the results of searching for a String are not highlighted
 *      - PDFs with non standard coordinate systems may not perform as suspected
 *      - At small zoom levels there are issues with scrolling/using arrow keys to change pages
 * 
 */

using System;
using System.Data;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Datalogics.PDFL;
using System.Diagnostics;

namespace DotNETViewerComponent
{
    [ToolboxItem(true)]
    public partial class DotNETViewer : UserControl
    {
        #region Members
        private DotNETController userInterfaceControls;
        private DocumentView docView;

        private string annotationType = "Circle";
        private bool showArrow = false;
        private string ProgName = "DotNETViewer";
        #endregion

        #region Constructor
        public DotNETViewer()
        {
            InitializeComponent();
            disableControls();

            docView = new DocumentView();
            userInterfaceControls = new DotNETController(ViewingSurfaceSplitContainer.Panel2, BookmarkTabPage, LayerTabPage, docView);
            
            // add the sub items for the font menu
            createFontFaces();

            // add the sub items for the font size
            createFontSizes();

            // set default properties
            docView.ActiveProps.ForeColor = System.Drawing.Color.Black;
            docView.ActiveProps.InteriorColor = System.Drawing.Color.White;
            docView.ActiveProps.LineWidth = 1;
            docView.ActiveProps.Solid = true;

            // modify to meet needs
            ViewingSurfaceSplitContainer.Panel2.VerticalScroll.SmallChange = 10;
            ViewingSurfaceSplitContainer.Panel2.VerticalScroll.LargeChange = 60;

            docView.ActiveProps.Update += setMenuStates;
            docView.FunctionModeChanged += FunctionModeChanged;
            docView.CommandStackUpdateCallback += OnEditorStateChanged;
            userInterfaceControls.DocumentChanged += new EventHandler(OnDocumentChanged);
            setMenuStates(false);
        }

        #endregion
        #region Methods/Events

        private void OnDocumentChanged(object sender, EventArgs e)
        {
            if (docView.Document == null)
            {
                disableControls();
                if (docView.Document == null)
                {
                    pageNumberToolStripLabel.Text = "of 0";
                    selectedPageNumberToolStripTextBox.Text = "";
                    this.Parent.Text = ProgName;
                }
                return;
            }

            this.Parent.Text = this.ProgName + " " + docView.DocFileName();

            docView.IsSerchRectDraw = false;
            searchPhraseToolStripTextBox.Text = "";

            enableControls();

            selectedPageNumberToolStripTextBox.Text = userInterfaceControls.DisplayablePageNumber;
            pageNumberToolStripLabel.Text = "of " + userInterfaceControls.DisplayableNumberOfPages;

            ViewingSurfaceSplitContainer.Size = new Size(ViewingSurfaceSplitContainer.SplitterDistance + userInterfaceControls.docView.Width, ViewingSurfaceSplitContainer.Height);
            ViewingSurfaceSplitContainer.Panel2.Focus();

            ViewingSurfaceSplitContainer.Panel1Collapsed = !userInterfaceControls.showPanel;
            showBookmarkLayersToolStripMenuItem.Enabled = userInterfaceControls.showPanel;
            showBookmarkLayersToolStripMenuItem.Checked = userInterfaceControls.showPanel;

            if (userInterfaceControls.showPanel)
                userInterfaceControls.resizeLayerControls();

            selectModeToolStripButton_Click(sender, e);
        }
        private void FunctionModeChanged(object unused, EventArgs e)
        {
            setMenuStates(false);
        }
        /**
         * disableControls - 
         * 
         * This function disables the controls listed so that they can
         * NOT be used before a document has been opened.
         */
        private void disableControls()
        {
            saveAsToolStripMenuItem.Enabled = false;
            saveToolStripButton.Enabled = false;
            saveToolStripMenuItem.Enabled = false;
            appendToolStripMenuItem.Enabled = false;
            printToolStripButton.Enabled = false;
            printToolStripMenuItem.Enabled = false;
            unlockToolStripMenuItem.Enabled = false;
            deleteToolStripMenuItem.Enabled = false;
            firstPageToolStripButton.Enabled = false;
            previousPageToolStripButton.Enabled = false;
            selectedPageNumberToolStripTextBox.Enabled = false;
            nextPageToolStripButton.Enabled = false;
            lastPageToolStripButton.Enabled = false;
            decreaseZoomToolStripButton.Enabled = false;
            zoomLevelToolStripComboBox.Enabled = false;
            increaseZoomToolStripButton.Enabled = false;
            freeTextToolStripDropDownButton.Enabled = false;
            toolStripDropDownAnnotationButton.Enabled = false;
            toolStripDropDownLineTypeButton.Enabled = false;
            toolStripDropDownLineWidthButton.Enabled = false;
            selectModeToolStripButton.Enabled = false;
            zoomModeToolStripButton.Enabled = false;
            marqueeZoomToolStripButton.Enabled = false;
            searchPhraseToolStripTextBox.Enabled = false;
            searchToolStripButton.Enabled = false;
            viewPrintModeToolStripMenuItem.Enabled = false;
            monitorResolutionToolStripMenuItem.Enabled = false;
            colorPickerToolStripButton.Enabled = false;
            ViewingSurfaceSplitContainer.Panel1Collapsed = true;

            if (userInterfaceControls != null)
            {
                // remove the subscriptions to the events 
                userInterfaceControls.onPageChange -= delegate(String displayablePageNumber) { selectedPageNumberToolStripTextBox.Text = displayablePageNumber; };
                userInterfaceControls.zoomManager.onZoomLevelChange -= delegate(int selectedZoomLevel) { zoomLevelToolStripComboBox.SelectedIndex = selectedZoomLevel; };
                userInterfaceControls.zoomManager.onCustomZoomLevel -= delegate(int customZoomLevel) { zoomLevelToolStripComboBox.Text = customZoomLevel.ToString() + "%"; };
            }
        }

        /**
         * enableControls - 
         * 
         * This function will enable all the controls that disableControls()
         * disabled. It is called after a document has been opend succesfully.
         */
        private void enableControls()
        {
            // determine if some controls should be enabled
            // based on the permissions the user has for the
            // document that is currently open
            if (userInterfaceControls.EditPermission == true)
            {
                saveAsToolStripMenuItem.Enabled = true;
                appendToolStripMenuItem.Enabled = true;
                deleteToolStripMenuItem.Enabled = (docView.ActiveAnnotation != null);
                freeTextToolStripDropDownButton.Enabled = true;
                toolStripDropDownAnnotationButton.Enabled = true;
                toolStripDropDownLineTypeButton.Enabled = true;
                toolStripDropDownLineWidthButton.Enabled = true;
                monitorResolutionToolStripMenuItem.Enabled = true;
                colorPickerToolStripButton.Enabled = true;
            }

            unlockToolStripMenuItem.Enabled = !userInterfaceControls.AllPermission;
            saveToolStripButton.Enabled = true;
            saveToolStripMenuItem.Enabled = true;

            if (userInterfaceControls.PrintPermission == true)
            {
                printToolStripButton.Enabled = true;
                printToolStripMenuItem.Enabled = true;
            }

            firstPageToolStripButton.Enabled = true;
            previousPageToolStripButton.Enabled = true;
            selectedPageNumberToolStripTextBox.Enabled = true;
            nextPageToolStripButton.Enabled = true;
            lastPageToolStripButton.Enabled = true;
            decreaseZoomToolStripButton.Enabled = true;
            zoomLevelToolStripComboBox.Enabled = true;
            increaseZoomToolStripButton.Enabled = true;
            selectModeToolStripButton.Enabled = true;
            zoomModeToolStripButton.Enabled = true;
            marqueeZoomToolStripButton.Enabled = true;
            searchPhraseToolStripTextBox.Enabled = true;
            searchToolStripButton.Enabled = true;
            viewPrintModeToolStripMenuItem.Enabled = true;
            this.Parent.Text = ProgName + " " + docView.DocFileName();
            zoomLevelToolStripComboBox.Items.Clear();

            // add items to the zoom dropdown list
            for (int i = 0; i < userInterfaceControls.zoomManager.zoomArray.Length; i++)
                zoomLevelToolStripComboBox.Items.Add(userInterfaceControls.zoomManager.zoomArray[i].ToString());

            zoomLevelToolStripComboBox.SelectedIndex = userInterfaceControls.zoomManager.selectedZoomIndex;
            /* subscribe to the events that DotNETController sets up
             * 
             * onSelectAnnotation - when the user clicks on an Annotation in the document
             * 
             * onRightClick - when the user right clicks on an Annotation
             * 
             * onZoomLevelChange - anytime the zoom level is changed update the selected level in the zoom combo box
             * 
             * onCustomZoomLevel - when using marquee zoom we do not have a defined value so we pass back the actual zoom value
             * 
             * onPageChange - anytime the page is changed in DotNETViewer update the page number that is displayed
             */
            userInterfaceControls.zoomManager.onZoomLevelChange += delegate(int selectedZoomLevel) { zoomLevelToolStripComboBox.SelectedIndex = selectedZoomLevel; };
            userInterfaceControls.zoomManager.onCustomZoomLevel += delegate(int customZoomLevel) { zoomLevelToolStripComboBox.Text = customZoomLevel.ToString() + "%"; };
            userInterfaceControls.onPageChange += delegate(String displayablePageNumber) { selectedPageNumberToolStripTextBox.Text = displayablePageNumber; };
        }

        /**
         * setMenuStates - 
         * 
         * when an annotation is clicked on this method
         * is called to set the toolbar items for shape,
         * lineWidth, line style, and color to match those
         * of the selected annotation
         */
        private void setMenuStates(object unused)
        {
            switch ((docView.ActiveAnnotation != null) ? docView.ActiveAnnotation.Properties.Subtype : annotationType)
            {
                case "Line":
                    bool arrow = (docView.ActiveAnnotation != null) ? docView.ActiveProps.StartEndingStyle != LineEndingStyle.None || docView.ActiveProps.EndEndingStyle != LineEndingStyle.None : showArrow;
                    toolStripDropDownAnnotationButton.Image = arrow ? global::DotNETViewerComponent.Properties.Resources.ArrowMenuChoice1 : global::DotNETViewerComponent.Properties.Resources.LineMenuChoice1;
                    break;
                case "Circle":
                    toolStripDropDownAnnotationButton.Image = global::DotNETViewerComponent.Properties.Resources.EllipseMenuChoice1;
                    break;
                case "Polygon":
                    toolStripDropDownAnnotationButton.Image = global::DotNETViewerComponent.Properties.Resources.PolygonMenu;
                    break;
                case "PolyLine":
                    toolStripDropDownAnnotationButton.Image = global::DotNETViewerComponent.Properties.Resources.PolyLineMenu1;
                    break;
                case "Square":
                    toolStripDropDownAnnotationButton.Image = global::DotNETViewerComponent.Properties.Resources.RectangleMenuChoice1;
                    break;
                case "Link":
                    toolStripDropDownAnnotationButton.Image = global::DotNETViewerComponent.Properties.Resources.LinkChoice;
                    break;
                case "Ink":
                    toolStripDropDownAnnotationButton.Image = global::DotNETViewerComponent.Properties.Resources.InkChoice;
                    break;
                case "Underline":
                    toolStripDropDownAnnotationButton.Image = global::DotNETViewerComponent.Properties.Resources.UnderlineChoice;
                    break;
                case "Highlight":
                    toolStripDropDownAnnotationButton.Image = global::DotNETViewerComponent.Properties.Resources.HighlightChoice;
                    break;
            }

            if (docView.ActiveProps.Solid)
                toolStripDropDownLineTypeButton.Image = global::DotNETViewerComponent.Properties.Resources.SolidLineMenuChoice1;
            else
            {
                switch (docView.GetNumPattern(docView.ActiveProps.DashPattern))
                {
                    case 0:
                        toolStripDropDownLineTypeButton.Image = global::DotNETViewerComponent.Properties.Resources.Dashed1LineMenuChoice1;
                        break;
                    case 1:
                        toolStripDropDownLineTypeButton.Image = global::DotNETViewerComponent.Properties.Resources.Dashed2LineMenuChoice1;
                        break;
                    case 2:
                        toolStripDropDownLineTypeButton.Image = global::DotNETViewerComponent.Properties.Resources.Dashed3LineMenuChoice1;
                        break;
                    case 3:
                        toolStripDropDownLineTypeButton.Image = global::DotNETViewerComponent.Properties.Resources.Dashed4LineMenuChoice1;
                        break;
                }
            }

            switch ((int)Math.Round(docView.ActiveProps.LineWidth))
            {
                case 1:
                    toolStripDropDownLineWidthButton.Image = global::DotNETViewerComponent.Properties.Resources.LineWeight1MenuChoice1;
                    break;
                case 2:
                    toolStripDropDownLineWidthButton.Image = global::DotNETViewerComponent.Properties.Resources.LineWeight2MenuChoice1;
                    break;
                case 3:
                    toolStripDropDownLineWidthButton.Image = global::DotNETViewerComponent.Properties.Resources.LineWeight3MenuChoice1;
                    break;
                case 5:
                    toolStripDropDownLineWidthButton.Image = global::DotNETViewerComponent.Properties.Resources.LineWeight4MenuChoice1;
                    break;
                default:
                    toolStripDropDownLineWidthButton.Image = global::DotNETViewerComponent.Properties.Resources.LineWeight4MenuChoice1;
                    break;
            }

            arrowStartNoneToolStripMenuItem.Checked = false;
            arrowStartCircleToolStripMenuItem.Checked = false;
            arrowStartOpenArrowToolStripMenuItem.Checked = false;
            arrowStartClosedArrowToolStripMenuItem.Checked = false;

            polyLineStartNoneToolStripMenuItem.Checked = false;
            polyLineStartCircleToolStripMenuItem.Checked = false;
            polyLineStartOpenArrowToolStripMenuItem.Checked = false;
            polyLineStartClosedArrowToolStripMenuItem.Checked = false;
            switch (docView.ActiveProps.StartEndingStyle)
            {
                case LineEndingStyle.None:
                    arrowStartNoneToolStripMenuItem.Checked = true;
                    polyLineStartNoneToolStripMenuItem.Checked = true;
                    break;
                case LineEndingStyle.Circle:
                    arrowStartCircleToolStripMenuItem.Checked = true;
                    polyLineStartCircleToolStripMenuItem.Checked = true;
                    break;
                case LineEndingStyle.OpenArrow:
                    arrowStartOpenArrowToolStripMenuItem.Checked = true;
                    polyLineStartOpenArrowToolStripMenuItem.Checked = true;
                    break;
                case LineEndingStyle.ClosedArrow:
                    arrowStartClosedArrowToolStripMenuItem.Checked = true;
                    polyLineStartClosedArrowToolStripMenuItem.Checked = true;
                    break;
            }

            arrowEndNoneToolStripMenuItem.Checked = false;
            arrowEndCircleToolStripMenuItem.Checked = false;
            arrowEndOpenArrowToolStripMenuItem.Checked = false;
            arrowEndClosedArrowToolStripMenuItem.Checked = false;

            polyLineEndNoneToolStripMenuItem.Checked = false;
            polyLineEndCircleToolStripMenuItem.Checked = false;
            polyLineEndOpenArrowToolStripMenuItem.Checked = false;
            polyLineEndClosedArrowToolStripMenuItem.Checked = false;
            switch (docView.ActiveProps.EndEndingStyle)
            {
                case LineEndingStyle.None:
                    arrowEndNoneToolStripMenuItem.Checked = true;
                    polyLineEndNoneToolStripMenuItem.Checked = true;
                    break;
                case LineEndingStyle.Circle:
                    arrowEndCircleToolStripMenuItem.Checked = true;
                    polyLineEndCircleToolStripMenuItem.Checked = true;
                    break;
                case LineEndingStyle.OpenArrow:
                    arrowEndOpenArrowToolStripMenuItem.Checked = true;
                    polyLineEndOpenArrowToolStripMenuItem.Checked = true;
                    break;
                case LineEndingStyle.ClosedArrow:
                    arrowEndClosedArrowToolStripMenuItem.Checked = true;
                    polyLineEndClosedArrowToolStripMenuItem.Checked = true;
                    break;
            }

            leftToolStripMenuItem.Checked = false;
            centerToolStripMenuItem.Checked = false;
            rightToolStripMenuItem.Checked = false;
            if ((docView.ActiveProps.Quadding & TextFormatFlags.HorizontalCenter) != 0)
                centerToolStripMenuItem.Checked = true;
            else if ((docView.ActiveProps.Quadding & TextFormatFlags.Right) != 0)
                rightToolStripMenuItem.Checked = true;
            else // left by default
                leftToolStripMenuItem.Checked = true;

            // step through the installed fonts
            int fontFace = docView.ActiveProps.FontFace.GetHashCode();
            for (int i = 0, len = fontToolStripMenuItem.DropDownItems.Count; i < len; ++i)
            {
                ToolStripMenuItem current = (ToolStripMenuItem)fontToolStripMenuItem.DropDownItems[i];
                current.Checked = ((int)current.Tag == fontFace);
            }

            // find the fontSize menu item that matches the current font size
            int fontSize = (int)Math.Round(docView.ActiveProps.FontSize);
            for (int i = 0, len = sizeToolStripMenuItem.DropDownItems.Count; i < len; ++i)
            {
                ToolStripMenuItem current = (ToolStripMenuItem)sizeToolStripMenuItem.DropDownItems[i];
                current.Checked = ((int)current.Tag == fontSize);
            }

            deleteToolStripMenuItem.Enabled = (docView.ActiveAnnotation != null);
            colorPickerToolStripButton.Invalidate();
            
            switch (docView.FunctionMode)
            {
                case ApplicationFunctionMode.MarqueeZoomMode:
                    marqueeZoomToolStripButton.Image = global::DotNETViewerComponent.Properties.Resources.MarqueeZoomModeMenuSelected1;
                    zoomModeToolStripButton.Image = global::DotNETViewerComponent.Properties.Resources.ZoomModeMenu1;
                    selectModeToolStripButton.Image = global::DotNETViewerComponent.Properties.Resources.SelectModeMenu1;
                    break;
                case ApplicationFunctionMode.ZoomMode:
                    zoomModeToolStripButton.Image = global::DotNETViewerComponent.Properties.Resources.ZoomModeMenuSelected1;
                    marqueeZoomToolStripButton.Image = global::DotNETViewerComponent.Properties.Resources.MarqueeZoomModeMenu1;
                    selectModeToolStripButton.Image = global::DotNETViewerComponent.Properties.Resources.SelectModeMenu1;
                    break;
                default:
                    selectModeToolStripButton.Image = global::DotNETViewerComponent.Properties.Resources.SelectModeSelected;
                    zoomModeToolStripButton.Image = global::DotNETViewerComponent.Properties.Resources.ZoomModeMenu1;
                    marqueeZoomToolStripButton.Image = global::DotNETViewerComponent.Properties.Resources.MarqueeZoomModeMenu1;
                    break;
            }

            undoToolStripMenuItem.Enabled = docView.CanUndo;
            redoToolStripMenuItem.Enabled = docView.CanRedo;
        }

        /**
         * createFontFaces - 
         * 
         * creates a list of font faces that the user can create 
         * Free Text Annotations in
         */
        private void createFontFaces()
        {
            System.Drawing.Text.InstalledFontCollection installedFonts = new System.Drawing.Text.InstalledFontCollection();
            int availableFontCount = installedFonts.Families.Length;

            FontFamily[] fontFamilies = new FontFamily[availableFontCount];
            ToolStripMenuItem[] fontFaces = new ToolStripMenuItem[availableFontCount];

            for (int i = 0; i < availableFontCount; i++)
            {
                // create the tool strip menu item
                fontFaces[i] = new ToolStripMenuItem();

                // add it to the list of font families
                fontFamilies[i] = installedFonts.Families[i];

                // set proper check box form
                fontFaces[i].ImageScaling = ToolStripItemImageScaling.None;

                // set the name of the item
                fontFaces[i].Text = fontFamilies[i].Name;

                // get and add the hash code
                fontFaces[i].Tag = fontFamilies[i].Name.GetHashCode();

                // add event for user changed font face
                fontFaces[i].Click += new EventHandler(userChangedFontFace);
            }

            // add the items to the menu
            fontToolStripMenuItem.DropDownItems.AddRange(fontFaces);

            // Here is where code can be added to set a certain fault as the default font
            // we just set the userChosenFontIndex equal to zero (the first font found)
            int current = 0;
            fontFaces[current].Checked = true;
            docView.ActiveProps.FontFace = fontFamilies[current].Name;
        }
        /**
         * createFontSizes - 
         * 
         * creates a list of font sizes that the user can create 
         * Free Text Annotations in
         */
        private void createFontSizes()
        {
            int[] availableFontSizes = new int[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
            ToolStripMenuItem[] fontSizes = new ToolStripMenuItem[availableFontSizes.Length];

            for (int i = 0; i < availableFontSizes.Length; ++i)
            {
                int fontSize = availableFontSizes[i];

                // create the tool strip menu item
                fontSizes[i] = new ToolStripMenuItem(fontSize.ToString());

                // set proper check box form
                fontSizes[i].ImageScaling = ToolStripItemImageScaling.None;

                // save the value of current font size
                fontSizes[i].Tag = fontSize;

                // add event for user changed font size
                fontSizes[i].Click += new EventHandler(userChangedFontSize);
            }

            // add the items to the menu
            sizeToolStripMenuItem.DropDownItems.AddRange(fontSizes);

            // set the font size to 10pt
            int current = 2;
            fontSizes[current].Checked = true;
            docView.ActiveProps.FontSize = (int)fontSizes[current].Tag;
        }

        /**
         * Opening a Document - 
         * 
         * There are three methods here, two of them are the same while one is different.
         * 
         * openToolStripButton_Click just wraps the openToolStripMenuItem_Click. These
         * will display a open file dialog to the user so they can select the file to open.
         * 
         * openDocument can be called to open a specific file by passing it the file name.
         * This bypasses the open file dialog. A message box is displayed if the file could
         * not be opened.
         */
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.OpenDocument();
        }
        /**
         * Saving a Document -
         * 
         * In DotNETController there are 4 ways to save a document.
         * 
         * saveDocument - saves the document to its current location with the current name
         * using the SaveFlags.Full
         * 
         * saveDocument (SaveFlags) - saves the document using the save flags passed into
         * the function, still saves to the current location with the current name
         * 
         * saveDocumentAs (SaveFlags) - saves the document using the save flags passed.
         * By default this function will display a save dialog to the user to get the new path.
         * 
         * saveDocumentAs (SaveFlags, String) - saves the document using the save flags passed
         * and expects the String to be a new path to save the document to.
         */
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.SaveDocument();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.SaveDocumentAs();
        }

        /**
         * appendToolStripMenuItem_Click - 
         * 
         * Appends a document to the end of the currently
         * open document. A document can NOT be appended to 
         * itself.
         * 
         * After the Append we update the number of pages on
         * the toolbar and resubscribe to the events that
         * DotNETController takes care of
         */
        private void appendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.AppendDocument();
        }

        /**
         * Printing a Document - 
         * 
         * DotNETController uses a custom print control to add extra
         * features to the Print Dialog. The printDocument function
         * takes care of display the dialog to the user and starting
         * the print process.
         * 
         * To change the Print Dialog see DotNETPrintController.cs and DotNETPrintDialog.cs
         */
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            userInterfaceControls.printDocument();
        }

        /**
         * Unlocking a Document -
         * 
         * In order to unlock a document, here a form is displayed to the user
         * so that they may enter the password. That password is then passed
         * along to the unlockDocument function so that it can increase the 
         * permission level to All Operations. If it fails it will display
         * a MessageBox containing a message saying so.
         */
        private void unlockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.UnlockDocument();
        }

        /**
         * closeToolStripMenuItem_Click - 
         * 
         * closes the current document in DotNETViewer
         */
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.CloseDocument();
        }

        /**
         * showBookmarkLayersToolStripMenuItem_Click - 
         * 
         * hide/show the panel that contains the Bookmark and Layers
         * controls
         */
        private void showBookmarkLayersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (showBookmarkLayersToolStripMenuItem.Checked == true)
            {
                showBookmarkLayersToolStripMenuItem.Checked = false;
                ViewingSurfaceSplitContainer.Panel1Collapsed = true;
            }
            else
            {
                showBookmarkLayersToolStripMenuItem.Checked = true;
                ViewingSurfaceSplitContainer.Panel1Collapsed = false;
            }
        }

        /**
         * BookmarkAndLayersTabControl_Selected - 
         * 
         * determine which panel to hide and which to show
         * when the user clicks on a tab in the panel
         */
        private void BookmarkAndLayersTabControl_Selected(object sender, TabControlEventArgs e)
        {
            if (BookmarkAndLayersTabControl.Controls.Count < 2) return;
            if (BookmarkAndLayersTabControl.SelectedTab.Text == "Bookmarks")
            {
                LayerTabPage.Hide();
                BookmarkTabPage.Show();
            }
            else
            {
                BookmarkTabPage.Hide();
                LayerTabPage.Show();
            }
        }

        /**
         * BookmarkAndLayersTabControl_SelectedIndexChanged - 
         * 
         * resize the layer control items so they span the length of the tab page control
         */
        private void BookmarkAndLayersTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (userInterfaceControls != null)
                userInterfaceControls.resizeLayerControls();
        }

        /**
         * ViewingSurfaceSplitContainer_Panel1_Resize - 
         * 
         * resize the layer control items so they span the width of the tab page control
         */
        private void ViewingSurfaceSplitContainer_Panel1_Resize(object sender, EventArgs e)
        {
            if (userInterfaceControls != null)
                userInterfaceControls.resizeLayerControls();
        }

        /**
         * firstPageToolStripButton_Click - 
         * 
         * move the view to the first page in the document
         */
        private void firstPageToolStripButton_Click(object sender, EventArgs e)
        {
            userInterfaceControls.firstPage();
        }

        /**
         * previousPageToolStripButton_Click - 
         * 
         * move the view to the page that is before the current page
         */
        private void previousPageToolStripButton_Click(object sender, EventArgs e)
        {
            userInterfaceControls.previousPage();
        }

        /**
         * nextPageToolStripButton_Click - 
         * 
         * move the view to the page that is after the current page
         */
        private void nextPageToolStripButton_Click(object sender, EventArgs e)
        {
            userInterfaceControls.nextPage();
        }

        /**
         * lastPageToolStripButton_Click - 
         * 
         * move the view to the last page of the document
         */
        private void lastPageToolStripButton_Click(object sender, EventArgs e)
        {
            userInterfaceControls.lastPage();
        }

        /**
         * selectedPageNumberToolStripTextBox_Leave - 
         * 
         * when the textbox loses focus revert page number in the textbox to the last valid value
         */
        private void selectedPageNumberToolStripTextBox_Leave(object sender, EventArgs e)
        {
            selectedPageNumberToolStripTextBox.Text = userInterfaceControls.DisplayablePageNumber;
        }

        /**
         * selectedPageNumberToolStripTextBox_KeyDown - 
         * 
         * if the user did not press "Enter" return right away.
         * If the user did press "Enter" validate that what they 
         * have entered in the PageNumber TextBox is numeric and
         * is greater than "0" and less than the number of Pages
         * in the document
         */
        private void selectedPageNumberToolStripTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // if not "Enter" return right away
            if (e.KeyCode != Keys.Enter)
                return;

            // validate that the text entered is numeric
            if (Regex.IsMatch(selectedPageNumberToolStripTextBox.Text, "^[0-9]+$"))
            {
                // turn the text into an int
                int newPageNumber = int.Parse(selectedPageNumberToolStripTextBox.Text) - 1;

                // check to see if the value is greater than "0" and less than the total
                // number of pages in the document
                if (newPageNumber >= 0 && newPageNumber < userInterfaceControls.NumberOfPages && newPageNumber != docView.CurrentPage)
                {
                    userInterfaceControls.goToPage(newPageNumber);
                }
            }
            selectedPageNumberToolStripTextBox.Text = userInterfaceControls.DisplayablePageNumber;
        }

        /**
         * monitorResolutionToolStripMenuItem_Click - 
         * 
         * displays a dialog to the user so they can change
         * the resolution DPI that is used in the calculations
         * to draw the document
         * 
         * This is managed internally beacuse it deals with some
         * of the low level objects that are used in displaying
         * the document
         */
        private void monitorResolutionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            userInterfaceControls.changeResolutionDPI();
        }

        /**
         * decreaseZoomToolStripButton_Click - 
         * 
         * decrease the zoom level by one step in the combo box
         */
        private void decreaseZoomToolStripButton_Click(object sender, EventArgs e)
        {
            userInterfaceControls.zoomManager.setCustomZoom(docView.Zoom);
            userInterfaceControls.zoomManager.decreaseZoom();
        }

        /**
         * increaseZoomToolStripButton_Click - 
         * 
         * increases the zoom level by one step in the combo box
         */
        private void increaseZoomToolStripButton_Click(object sender, EventArgs e)
        {
            userInterfaceControls.zoomManager.setCustomZoom(docView.Zoom);
            userInterfaceControls.zoomManager.increaseZoom();
        }

        /**
         * zoomLevelToopStripComboBox_SelectedIndexChanged - 
         * 
         * when the selected zoom level in the combo box changes
         * pass the new selected index to DotNETController. DotNETController
         * only needs the index because the combo box is filled with
         * values from an Array that is within DotNETController.
         */
        private void zoomLevelToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (zoomLevelToolStripComboBox.SelectedIndex != -1)
                userInterfaceControls.changeZoomLevel(zoomLevelToolStripComboBox.SelectedIndex);
            ViewingSurfaceSplitContainer.Panel2.Focus();
        }

        /**
         * deleteToolStripMenuItem_Click -
         * 
         * deletes the currently selected Annotation
         */
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.DeleteAnnotation();
        }

        /**
         * continuousToolStripMenuItem_Click - 
         * 
         * change the PageDisplayMode to the continuous display mode
         * 
         * This displays pages end to end with no space in between them
         */
        private void continuousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            continuousToolStripMenuItem.Checked = true;
            singlePageToolStripMenuItem.Checked = false;

            userInterfaceControls.PageDisplayMode = PageViewMode.continuousPage;
        }

        /**
         * singlePageToolStripMenuItem_Click -
         * 
         * change the PageDisplayMode to the single display mode
         * 
         * This displays a single page at a time.
         */
        private void singlePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            continuousToolStripMenuItem.Checked = false;
            singlePageToolStripMenuItem.Checked = true;

            userInterfaceControls.PageDisplayMode = PageViewMode.singlePage;
        }

        /**
         * freeTextToolStripDropDownButton_ButtonClick - 
         * 
         * allow the user to draw a new FreeText Annotation with current preferences
         */
        private void freeTextToolStripDropDownButton_ButtonClick(object sender, EventArgs e)
        {
            CreateAnnotation("FreeText");
        }

        /**
         * userChangedFontFace -
         * 
         * when the user selects a new font in the user interface this
         * method finds the newly selected fonts index and marks it as
         * checked while marking all others as unchecked.
         */
        public void userChangedFontFace(object sender, EventArgs e)
        {
            docView.ActiveProps.FontFace = sender.ToString();
        }

        /**
         * userChangedFontSize - 
         * 
         * when the user changes the font size pass the new font size to 
         * the user interface controls
         */
        private void userChangedFontSize(object sender, EventArgs e)
        {
            docView.ActiveProps.FontSize = Int32.Parse(((ToolStripMenuItem)sender).Text.ToString());
        }

        /**
         * leftToolStripMenuItem_Click - 
         * 
         * For FreeText Annotations set the Alignment/Justification
         * to the "Left"
         */
        private void leftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.ActiveProps.Quadding = TextFormatFlags.Left;
        }

        /**
         * centerToolStripMenuItem_Click - 
         * 
         * For FreeText Annotations set the Alignment/Justification
         * to the "Center"
         */
        private void centerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.ActiveProps.Quadding = TextFormatFlags.HorizontalCenter;
        }

        /**
         * rightToolStripMenuItem_Click - 
         * 
         * For FreeText Annotations set the Alignment/Justification
         * to the "Right"
         */
        private void rightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.ActiveProps.Quadding = TextFormatFlags.Right;
        }

        private void CreateAnnotation(string type)
        {
            if (type != "FreeText")
                annotationType = type;
            docView.SetCreateAnnotationMode(type);
            setMenuStates(false);
        }

        /**
         * toolStripDropDownAnnotationButton_ButtonClick -
         * 
         * move the Application Mode to the Shape Create Mode so
         * the user can create an Annotation with current preferences
         */
        private void toolStripDropDownAnnotationButton_ButtonClick(object sender, EventArgs e)
        {
            if (docView.ActiveAnnotation != null)
                annotationType = docView.ActiveAnnotation.Properties.Subtype;
            CreateAnnotation(annotationType);
        }

        /**
         * rectangleToolStripMenuItem_Click - 
         * 
         * sets the Application Mode to the Shape Create Mode, the
         * Annotation Shape to Rectangle, and changes the image for 
         * the DropDown Button to the Rectangle image.
         */
        private void rectangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateAnnotation("Square");
        }

        /**
         * ellipseToolStripMenuItem_Click - 
         * 
         * sets the Application Mode to the Circle Create Mode,
         * and changes the image for the DropDown Button to the Ellipse image.
         */
        private void ellipseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateAnnotation("Circle");
        }

        /**
         * lineToolStripMenuItem_Click - 
         * 
         * sets the Application Mode to the Shape Create Mode, the
         * Annotation Shape to Line, and changes the image for 
         * the DropDown Button to the Line image.
         */
        private void lineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showArrow = false;
            CreateAnnotation("Line");
        }

        /**
         * arrowToolStripMenuItem_Click - 
         * 
         * sets the Application Mode to the Shape Create Mode, the
         * Annotation Shape to Arrow, and changes the image for 
         * the DropDown Button to the Arrow image.
         */
        private void arrowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showArrow = true;
            CreateAnnotation("Line");
        }

        /**
         * linkToolStripMenuItem_Click - 
         * 
         * sets the Application Mode to the Target Create Mode, the
         */
        private void linkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateAnnotation("Link");
        }

        /**
         * Sets the Application mode to the Ink Create mode
         */
        private void inkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateAnnotation("Ink");
        }

        /**
         * Sets the Application mode to the Underline Create mode
         */
        private void underlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateAnnotation("Underline");
        }

        /**
         * Sets the Application mode to the Highlight Create mode
         */
        private void highlightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateAnnotation("Highlight");
        }

        /**
         * ArrowStart Arrowhead Type ToolStripMenuItem_Click -
         * 
         * sets the Start Arrowhead style to the Arrowhead Type in the
         * name of the method.
         * 
         * This sets the same object that the PolyLine Annotation uses
         * for its Start Arrowhead but in the menu they appear seperate
         * from one another
         */
        private void StartNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.ActiveProps.StartEndingStyle = LineEndingStyle.None;
        }

        private void StartOpenArrowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.ActiveProps.StartEndingStyle = LineEndingStyle.OpenArrow;
        }

        private void StartClosedArrowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.ActiveProps.StartEndingStyle = LineEndingStyle.ClosedArrow;
        }

        private void StartCircleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.ActiveProps.StartEndingStyle = LineEndingStyle.Circle;
        }

        /**
         * ArrowEnd Arrowhead Type ToolStripMenuItem_Click -
         * 
         * sets the End Arrowhead style to the Arrowhead Type in the
         * name of the method.
         * 
         * This sets the same object that the PolyLine Annotation uses
         * for its End Arrowhead but in the menu they appear seperate
         * from one another
         */
        private void EndNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.ActiveProps.EndEndingStyle = LineEndingStyle.None;
        }

        private void EndOpenArrowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.ActiveProps.EndEndingStyle = LineEndingStyle.OpenArrow;
        }

        private void EndClosedArrowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.ActiveProps.EndEndingStyle = LineEndingStyle.ClosedArrow;
        }

        private void EndCircleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.ActiveProps.EndEndingStyle = LineEndingStyle.Circle;
        }

        /**
         * polygonToolStripMenuItem_Click - 
         * 
         * sets the Application Mode to the Shape Create Mode, the
         * Annotation Shape to Polygon, and changes the image for 
         * the DropDown Button to the Polygon image.
         */
        private void polygonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateAnnotation("Polygon");
        }

        /**
         * polyLineToolStripMenuItem_Click - 
         * 
         * sets the Application Mode to the Shape Create Mode, the
         * Annotation Shape to PolyLine, and changes the image for 
         * the DropDown Button to the PolyLine image.
         */
        private void polylineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateAnnotation("PolyLine");
        }

        /**
         * solidToolStripMenuItem_Click - 
         * 
         * sets the Annotation LineStyle to solid for the currently
         * selected Annotation or the Annotation about to be drawn
         */
        private void solidToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripDropDownLineTypeButton.Image = global::DotNETViewerComponent.Properties.Resources.SolidLineMenuChoice1;
            docView.LinePattern(true, 0);
        }

        /**
         * dashed#ToolStripMenuItem_Click - 
         * 
         * sets the Annotation LineStyle to the dash pattern that
         * corresponds to the # in the method name. It is set for 
         * either the currently selected Annotation or the Annotation
         * that is about to be drawn.
         */
        private void dashed1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.LinePattern(true, 1);
        }

        private void dashed2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.LinePattern(true, 2);
        }

        private void dashed3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.LinePattern(true, 3);
        }

        private void dashed4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.LinePattern(true, 4);
        }

        /**
         * lineWidth#ToolStripMenuItem_Click - 
         * 
         * these set the Annotation LineWidth to a certain value for
         * either the currently selected Annotation or the Annotation
         * that is about to be drawn
         */
        private void lineWidth1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.LineWidth(true, 1);
        }

        private void lineWidth2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.LineWidth(true, 2);
        }

        private void lineWidth3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.LineWidth(true, 3);
        }

        private void lineWidth5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.LineWidth(true, 5);
        }

        /**
         * otherToolStripMenuItem_Click -
         * 
         * display a form for the user to enter a custom line width
         * for the annotation.
         */
        private void otherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.LineWidth(true, -1);
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
        private void colorPickerToolStripButton_Click(object sender, EventArgs e)
        {
            docView.ChooseColor(true);

            // force the buton to redraw itself
            colorPickerToolStripButton.Invalidate();
        }

        /**
         * searchToolStripButton_Click -
         * 
         * if the searchPhrase Textbox is not empty/null perform the search
         */
        private void searchToolStripButton_Click(object sender, EventArgs e)
        {
            if (searchPhraseToolStripTextBox.Text.ToString() != null)
                userInterfaceControls.search(searchPhraseToolStripTextBox.Text.ToString());
        }

        /**
         * searchPhraseToolStripTextBox_KeyDown -
         * 
         * if the "Enter" key is pressed while the cursor is in the 
         * searchPhrase Textbox start searching the document for the
         * text in the Textbox
         */
        private void searchPhraseToolStripTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            searchPhraseToolStripTextBox.Focus();

            if (e.KeyCode == Keys.Enter)
                userInterfaceControls.search(searchPhraseToolStripTextBox.Text.ToString());
        }

        /**
         * selectModeToolStripButton_Click -
         * 
         * take the application into select mode so the user can
         * click on an annotation to edit its attributes.
         */
        private void selectModeToolStripButton_Click(object sender, EventArgs e)
        {
            userInterfaceControls.changeDotNETViewerApplicationMode(ApplicationFunctionMode.AnnotationEditMode);
        }

        /**
         * zoomModeToolStripButton_Click -
         * 
         * take the application into the zoom mode so that when the user
         * clicks on the document it increases the zoom level by 1 step.
         * If the user clicks while holding down the "Shift" key it will
         * decrease the zoom level by 1 step.
         */
        private void zoomModeToolStripButton_Click(object sender, EventArgs e)
        {
            userInterfaceControls.changeDotNETViewerApplicationMode(ApplicationFunctionMode.ZoomMode);
        }

        /**
         * marqueeZoomToolStripButton_Click -
         * 
         * take the application into the marquee zoom mode so the user
         * can draw a rectangle over the area they want to zoom in on
         */
        private void marqueeZoomToolStripButton_Click(object sender, EventArgs e)
        {
            userInterfaceControls.changeDotNETViewerApplicationMode(ApplicationFunctionMode.MarqueeZoomMode);
        }

        /**
         * colorPickerToolStripButton_Paint -
         * 
         * paints the image for the colorPickerToolStripButton
         * so that the colors in the image match the colors that
         * were selected by the user or the ones used in the 
         * selected annotation
         * 
         * Design : two rectangles that overlap, filled with the
         * fill color then stroke color (back to front)
         */
        private void colorPickerToolStripButton_Paint(object sender, PaintEventArgs e)
        {
            if (colorPickerToolStripButton.Enabled)
            {
                Graphics g = e.Graphics;

                SolidBrush fillBrush = new SolidBrush(System.Drawing.Color.White);
                SolidBrush strokeBrush = new SolidBrush(System.Drawing.Color.LightGray);

                // if the fillColor of the annotation is transparent, use white to simulate it
                if (docView.ActiveProps.HasFill)
                    fillBrush.Color = docView.ActiveProps.InteriorColor;
                else
                    fillBrush.Color = System.Drawing.Color.White;

                Rectangle fillRectangle = new Rectangle(4, 4, 16, 16);
                g.FillRectangle(fillBrush, fillRectangle);

                // now draw a red line over the white background to show that the fill is transparent
                if (!docView.ActiveProps.HasFill)
                {
                    System.Drawing.Point start = new System.Drawing.Point(20, 4);
                    System.Drawing.Point end = new System.Drawing.Point(4, 20);

                    Pen transFlagPen = new Pen(System.Drawing.Color.Red);
                    transFlagPen.Width = 2;

                    g.DrawLine(transFlagPen, start, end);
                }

                Pen borderPen = new Pen(System.Drawing.Color.Black);
                g.DrawRectangle(borderPen, fillRectangle);

                // stroke color is never transparent so no need to worry about it
                strokeBrush.Color = docView.ActiveAnnotation is FreeTextAnnotationEditor ? docView.ActiveProps.TextColor : docView.ActiveProps.ForeColor;

                Rectangle strokeRectangle = new Rectangle(12, 12, 16, 16);
                g.FillRectangle(strokeBrush, strokeRectangle);

                g.DrawRectangle(borderPen, strokeRectangle);
            }
        }

        /**
         * searchPhraseToolStripTextBox_TextChanged -
         * 
         * when text serch box is empty
         *  search rectangles are cleared
         */
        private void searchPhraseToolStripTextBox_TextChanged(object sender, EventArgs e)
        {
            if (searchPhraseToolStripTextBox.Text.Length == 0)
            {
                if (docView.IsSerchRectDraw)
                {
                    docView.IsSerchRectDraw = false;
                    docView.Invalidate();
                }
            }
        }
        /**
         * zoomLevelToolStripComboBox_TextChanged - 
         * when user input zoom in combo box 
         * page zoom is changing
         */
        private void zoomLevelToolStripComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                char[] splitChars = new char[] { '%', ' ', '\t' };
                string strZoom = zoomLevelToolStripComboBox.Text.TrimEnd(splitChars);
                if (strZoom.Length != 0)
                {
                    double newZoom = 0;
                    try
                    {
                        newZoom = Convert.ToDouble(strZoom) / 100.0;
                        if (newZoom > 8)
                            newZoom = 8;
                        docView.setZoom(FitModes.FitNone, newZoom);
                    }
                    catch
                    {
                        docView.setZoom(FitModes.FitNone, docView.Zoom);
                    }
                }
                else
                    docView.setZoom(FitModes.FitNone, docView.Zoom);

                zoomLevelToolStripComboBox.Text = docView.Zoom * 100.0 + "%";
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.Redo();
        }

        private void OnEditorStateChanged(EditCommandStack commandStack)
        {
            undoToolStripMenuItem.Enabled = commandStack.CanUndo;
            redoToolStripMenuItem.Enabled = commandStack.CanRedo;
        }
        #endregion
        public DotNETController Controller { get { return userInterfaceControls; } }

        #region Tests
        public void RunTests()
        {
            test1ToolStripMenuItem_Click(null, null);
        }
        private void test1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.RunTest(docView.TestAnnotations);
        }
        private void createDocToolStripMenuItem_Click(object sender, EventArgs e)
        {
            docView.CreateTestDocuments();
        }
        #endregion
    }
}