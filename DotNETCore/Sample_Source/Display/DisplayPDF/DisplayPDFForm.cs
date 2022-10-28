using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WinForm = System.Windows.Forms.Form;
using Datalogics.PDFL;

/*
 * Sample to demonstrate how to open a PDF document, search for text in
 * the pages and highlight the text using the C# drawing library.
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * The information and code in this sample is for the exclusive use of Datalogics
 * customers and evaluation users only.  Datalogics permits you to use, modify and
 * distribute this file in accordance with the terms of your license agreement.
 * Sample code is for demonstrative purposes only and is not intended for production use.
 *
 */

namespace DisplayPDF
{
    public partial class DisplayPDFForm : WinForm
    {
        Document PDFDoc = null;
        int maxpages;
        int currentPDPagenum = 0;
        double scalefactor = 1.0;
        Bitmap bitmap = null;
        String SearchWord = null;
        public string documentpassword = null;
        bool updatingScalefactorBox = false;

        public DisplayPDFForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PDFPagepicturebox.ClearList();
            TextSearch.Text = null;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "PDF File (*.pdf)|*.pdf";
            dialog.Title = "Select a PDF file";
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                String FileName = dialog.FileName;
                try
                {
                    if (PDFDoc != null)
                    {
                        PDFDoc.Dispose();
                    }
                    PDFDoc = new Document(FileName);
                    DisplayPDFForm.ActiveForm.Text = "PDFL Viewer - " + FileName;
                    maxpages = PDFDoc.NumPages;
                    currentPDPagenum = 0;
                    scalefactor = 1.0;
                    documentpassword = null;
                    drawcurrentPDFpage();
                }
                catch (Exception exc)
                {
                    MessageBox.Show("The File " + FileName + " cannot be opened. " + exc.ToString());
                }
            }
        }

        private bool passwordenter()
        {
            DialogResult result = DialogResult.None;
            while (result == DialogResult.None)
            {   PasswordForm mypasswordform = new PasswordForm(this);
                result = mypasswordform.ShowDialog();
                if (result == DialogResult.OK)
                {
                    if (PDFDoc.PermRequest(documentpassword, PermissionRequestOperation.Export) == true)
                    {
                        return true;
                    }
                    else
                    { 
                        MessageBox.Show("Invalid Document Password");
                        // Force this to loop until either the correct password is enter
                        // or Cancel is clicked.
                        result = DialogResult.None;
                    }
                }
                else
                {
                    break;
                }
            }
            return false;
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PDFDoc == null)
            {
                MessageBox.Show("Please open a PDF document first before printing");
                return;
            }

            // Get some parameters 
            PrintUserParams userParams = new PrintUserParams();
            // These are the "other" print parameters that hang off the user parameters.
            PrintParams printParams = userParams.PrintParams;

            // Print to a printer
            // For a list of the current print drivers available under WinNT, look at:
            // HKEY_CURRENT_USER\Software\Microsoft\WindowsNT\CurrentVersion\Devices
            // Or use the default printer

            if (userParams.PosePrintDialog(PDFDoc) == true)
            {
                PDFDoc.Print(userParams);
            }
        }     

        private void search_and_highlight(WordFinder wordFinder) {
            IList<Word> pageWords = wordFinder.GetWordList(currentPDPagenum);
            foreach (Word w in pageWords)
            {
                int searchindex = 0;
                // If this word matches, or is contains the search word, character quads of the
                // matching part onto the list, and then the matching part will be highlighted.

                if (w.Text.ToLower().Contains(SearchWord.ToLower()))
                {
                    List<Quad> WordHighLightQuads = new List<Quad>(0);
                    while (searchindex < w.Text.Length)
                    {
                        int matchindex = w.Text.ToLower().IndexOf(SearchWord.ToLower(), searchindex);
                        if (matchindex == -1 || matchindex >= w.Text.Length)
                        {
                            break;
                        }
                        for (int i = matchindex; i < matchindex + SearchWord.Length; i++)
                        {
                            WordHighLightQuads.Insert(WordHighLightQuads.Count, w.CharQuads[i]);
                        }
                        /*
                         * Create a quad with the points being the points of the entire
                         * matched region so all characters are highlighted together.
                         */
                        Quad BigQuad = new Quad(
                            WordHighLightQuads[0].TopLeft,
                            WordHighLightQuads[WordHighLightQuads.Count - 1].TopRight,
                            WordHighLightQuads[0].BottomLeft,
                            WordHighLightQuads[WordHighLightQuads.Count - 1].BottomRight
                        );
                        PDFPagepicturebox.HighLightQuads.Insert(0, BigQuad);
                        // Clear this out after each matching word subset
                        // so the individual matches are highlighted
                        WordHighLightQuads.Clear();
                        searchindex = matchindex + SearchWord.Length;
                    }
                }
            }
        }

         private void searchwords()
        {
            WordFinderConfig wordConfig = new WordFinderConfig();
            wordConfig.IgnoreCharGaps = true;

            using (WordFinder wordFinder = new WordFinder(PDFDoc, WordFinderVersion.Latest, wordConfig))
            {
                try // If we run into the security exception we will ask for a password and try again.
                {
                    search_and_highlight(wordFinder);                    
                }
                catch(Exception e)
                {
                    int errornumidx = e.ToString().IndexOf("Error number:");
                    if (errornumidx == -1)
                    {
                        throw e;
                    }
                    int colidx = e.ToString().IndexOf(": ", errornumidx);
                    if (colidx == -1)
                    {
                        throw e;
                    }
                    // We know that the error number we are wanting to find is formatted such as
                    // "Error number: 1073938472".  All we need to do is parse out the next 10
                    // digits and if they are this number, then this is the exception we want
                    // to intercept and then ask for the document password.
                    errornumidx = colidx + 2;
                    string errornum = e.ToString().Substring(errornumidx, 10);
                    if ( errornum != "1073938472") {
                        throw e;
                    } else {
                        if (passwordenter() == true)
                        {
                            search_and_highlight(wordFinder);
                        }
                        else
                        {
                            /*
                             * If we null this then the next click of the search button
                             * will re-prompt us for a password and do the searching
                             * we are hoping for.
                             */
                            SearchWord = null;
                        }
                    }
                }
            }
        }

        private void drawcurrentPDFpage()
        {
            if (PDFDoc != null)
            {
                Page pg = PDFDoc.GetPage(currentPDPagenum);

		// Use the CropBox to determine region of the page to display.
		// Some documents may have a CropBox that is not located at the
		// origin of the page.  
                Rect cropBox = pg.CropBox;
                int width = (int)cropBox.Right - (int)cropBox.Left;
                int height = (int)cropBox.Top - (int)cropBox.Bottom;

		// Shift the page display based on the location of the CropBox
                int shiftx = (int)cropBox.Left;
                int shifty = (int)cropBox.Bottom;

                width = (int)((double)width * scalefactor);
                height = (int)((double)height * scalefactor);
                shiftx = (int)((double)shiftx * scalefactor);
                shifty = (int)((double)shifty * scalefactor);


                if (bitmap != null)
                {
                    bitmap.Dispose();
                }

                // If they try to allocate too large a bitmap, 
                // print an error and abandon the draw
                try
                {
                    bitmap = new Bitmap(width, height);
                }
                catch (ArgumentException)
                {
                    String message = "File " + PDFDoc.FileName + " is " 
                        + width.ToString() + " x " + height.ToString()
                        + " pixels -- too large for DisplayPDF to handle.";

                    MessageBox.Show(message, "Document Too Large");
                    return;
                }
                // The bitmap starts out black, so clear it to white

                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.Clear(System.Drawing.Color.White);

	        // Must invert the page to get from PDF with origin at lower left,
	        // to a bitmap with the origin at upper right.
		// Also, we use the shiftx and shifty to take the location of the CropBox
		// into account.
                Matrix matrix = new Matrix().Scale(1, -1).Translate(-shiftx, -height - shifty).Scale(scalefactor, scalefactor);

                PDFPagepicturebox.Image = bitmap;
                PDFPagepicturebox.matrix = matrix;
                PDFPagepicturebox.Size = bitmap.Size;
		
		// Set the updateRect 
		Rect updateRect = new Rect((int)cropBox.LLx,
					   (int)cropBox.LLy,
					   (int)cropBox.LLx + width,
					   (int)cropBox.LLy + height);

                // Draw directly to the bitmap
                DrawParams parms = new DrawParams();
                parms.Matrix = matrix;
                parms.UpdateRect = updateRect;
                parms.Flags = DrawFlags.DoLazyErase | DrawFlags.UseAnnotFaces;

                pg.DrawContents(graphics, parms);
                PageNumberTextBox.Text = (currentPDPagenum + 1).ToString();
                PageLabel.Text = "of " + maxpages.ToString();
            }
        }

        private void gobackonepage_Click(object sender, EventArgs e)
        {
            if (PDFDoc != null & currentPDPagenum > 0)
            {
                currentPDPagenum -= 1;
                PDFPagepicturebox.ClearList();
                drawcurrentPDFpage();
                if (SearchWord != null)
                {
                    searchwords();
                }                
            }
        }

        private void forwardtodocend_Click(object sender, EventArgs e)
        {
            if (PDFDoc != null)
            {
                currentPDPagenum = maxpages - 1;
                PDFPagepicturebox.ClearList();
                drawcurrentPDFpage();
                if (SearchWord != null)
                {
                    searchwords();
                }
            }
        }

        private void rewindtodocstart_Click(object sender, EventArgs e)
        {
            if (PDFDoc != null)
            {
                currentPDPagenum = 0;
                PDFPagepicturebox.ClearList();
                drawcurrentPDFpage();
                if (SearchWord != null)
                {
                    searchwords();
                }
            }
        }

        private void forwardonedocpage_Click(object sender, EventArgs e)
        {
            if (PDFDoc != null & (currentPDPagenum + 1) < maxpages)
            {
                currentPDPagenum++;
                PDFPagepicturebox.ClearList();
                drawcurrentPDFpage();
                if (SearchWord != null)
                {
                    searchwords();
                }

            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PDFDoc != null)
            {
                PDFDoc.Close();
            }
            this.Close();
        }

        private void scaledown_Click(object sender, EventArgs e)
        {
            double newscale = 0.0;
            foreach (string item in scalepercent.Items)
            {
                double tmpscale = double.Parse(item.Replace("%", ""));
                if (tmpscale < (scalefactor*100.0))
                    newscale = tmpscale;
                else
                    break;
            }
            if (newscale != 0.0)
                setScale(newscale / 100.0);
        }

        private void scaleup_Click(object sender, EventArgs e)
        {
            double newscale = 0.0;
            foreach (string item in scalepercent.Items)
            {
                double tmpscale = double.Parse(item.Replace("%", ""));
                if (tmpscale > (scalefactor * 100.0))
                {
                    newscale = tmpscale;
                    break;
                }
            }
            if (newscale != 0.0)
                setScale(newscale / 100.0);
        }

        // Hitting enter in the text box or losing mouse
        // focus from the textbox both invoke this method.
        private void scalepercent_changed()
        {
            string scaleString = scalepercent.Text.Replace("%", "");
            
            // Only set scale factor if we are NOT programmatically
            // updating the scale text box (prevents a recursive call)
            if (!updatingScalefactorBox)
                setScale(double.Parse(scaleString) / 100);
        }

        private void setScale(double newscale)
        {
            if (newscale < 0.10)
                newscale = 0.10;
            else if (newscale > 32.0)
                newscale = 32.0;

            double oldscale = scalefactor;
            try
            {
                scalefactor = newscale;
                drawcurrentPDFpage();
            }
            catch
            {
                scalefactor = oldscale;
                drawcurrentPDFpage();
                throw;
            }

            // We bracket the change to scalepercent.Text with
            // a flag to prevent a recursive call to ourself
            updatingScalefactorBox = true;
            scalepercent.Text = scalefactor.ToString("p00");
            updatingScalefactorBox = false;
        }

        private void scalepercent_KeyDown(object sender, KeyEventArgs e) 
        {
            if (e.KeyCode == Keys.Enter)
            {
                scalepercent_changed();
            }
        }

        private void scalepercent_LostFocus(object sender, System.EventArgs e)
        {
            scalepercent_changed();
        }

        // Hitting enter in the text box or losing mouse
        // focusfrom the textbox both invoke this method.
        private void PageNumberTextBox_changed()
        {
            int newPDPagenum = int.Parse(PageNumberTextBox.Text) - 1;
            int savePDPagenum = currentPDPagenum;
            try
            {
                if (currentPDPagenum != newPDPagenum)
                {
                    currentPDPagenum = newPDPagenum;
                    PDFPagepicturebox.ClearList();
                    if (SearchWord != null)
                    {
                        searchwords();
                    }
                    drawcurrentPDFpage();
                }
            }
            catch
            {
                currentPDPagenum = savePDPagenum;
                PDFPagepicturebox.ClearList();
                if (SearchWord != null)
                {
                    searchwords();
                }
                drawcurrentPDFpage();
                throw;
            }
        }

        private void PageNumberTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                PageNumberTextBox_changed();
            }
        }

        private void PageNumberTextBox_LostFocus(object sender, System.EventArgs e)
        {
            PageNumberTextBox_changed();
        }

        private void perform_searchform()
        {
            String textboxword = TextSearch.Text.ToString();
            if (textboxword.Length == 0)
            {
                PDFPagepicturebox.ClearList();
                SearchWord = null;
                drawcurrentPDFpage();
                return;
            }
            
            if (SearchWord != null && SearchWord != textboxword )
            {
                PDFPagepicturebox.ClearList();
                drawcurrentPDFpage();
            }

            if (SearchWord == null || SearchWord != textboxword )
            {
                SearchWord = textboxword;
                if (PDFDoc != null)
                {
                    searchwords();
                    drawcurrentPDFpage();
                }
            }
        }

        private void textsearchbutton_Click(object sender, EventArgs e)
        {
            perform_searchform();
        }

        private void textsearchbutton_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                perform_searchform();
            }
        }
    }
}
