/*
Copyright (C) 2008-2009, Datalogics, Inc. All rights reserved.
 
The information and code in this sample is for the exclusive use
of Datalogics customers and evaluation users only.  Datalogics 
permits you to use, modify and distribute this file in accordance 
with the terms of your license agreement. Sample code is for 
demonstrative purposes only and is not intended for production use.
*/

/*
 * TextSearchManager
 * 
 * This class handles the search function of DotNETViewer. The main feature
 * is searching a Document for a String of text, which if found in the 
 * Document is highlighted at every occurence (if there is more than one).
 */

using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using Datalogics.PDFL;

namespace DotNETViewerComponent
{
    public class TextSearchManager
    {
        #region Members
        DotNETController dleController;
        public List<Quad> highlightQuads = new List<Quad>(0);
        String searchString = null;
        public ApplicationHighlight highlightSelected = ApplicationHighlight.NoHighlight;
        #endregion

        #region Properties
        public String SearchString
        {
            get { return searchString; }
            set { searchString = value; }
        }
        #endregion

        #region Constructor
        public TextSearchManager(DotNETController dleController)
        {
            this.dleController = dleController;
        }
        #endregion
        
        #region Methods
        /**
         * performSearch() - 
         * 
         * This function wraps the search() and highlight() functions
         * into one so that they are performed after each other as long
         * as we have a non-null/empty string in our property.
         */
        public void performSearch()
        {
            // if the string is null or empty just clear the light
            // of coordinates that we have so nothing is highlighted.
            if (String.IsNullOrEmpty(searchString))
            {
                clearHighlightList();
                searchString = null;
                return;
            }
            else
            {
                clearHighlightList();
                highlightSelected = ApplicationHighlight.Highlight;

                if (dleController.Document!= null)
                    search(dleController.docView.CurrentPage);
                
            }
        }

        /**
         * search -
         * 
         * Does the actual searching of the wordlist in the document.
         * Here we search one page at a time but it would be possible
         * to search the entire document at once.
         */
        private void search(int PageNum)
        {
            // create the config for the wordfinder
            WordFinderConfig config = new WordFinderConfig();
            config.IgnoreCharGaps = true;

            // create a wordfinder for the current page 
            using (WordFinder wordFinder = new WordFinder(dleController.Document, WordFinderVersion.Latest, config))
            {
                IList<Word> wordsOnCurrentPage = wordsOnCurrentPage = wordFinder.GetWordList(PageNum);
                
                // compare the search string to each word in the wordlist
                foreach (Word w in wordsOnCurrentPage)
                {
                    int searchIndex = 0;

                    // check if the word contains the search string
                    if (w.Text.ToLower().Contains(searchString.ToLower()))
                    {
                        //once we know it contains the search string we need to
                        // determine where in the word the string is
                        while (searchIndex < w.Text.Length)
                        {
                            int firstOccurence = w.Text.ToLower().IndexOf(searchString.ToLower(), searchIndex);

                            if (firstOccurence == -1 || firstOccurence >= w.Text.Length)
                                break;

                            // get the quad that should be highlighted by taking the left edge of the match
                            // and the right edge (determined by taking the top right and bottom right from
                            // taking the firstOccurence and adding the search string length)
                            Quad highlightQuad = new Quad(w.CharQuads[firstOccurence].TopLeft, w.CharQuads[firstOccurence + searchString.Length - 1].TopRight, w.CharQuads[firstOccurence].BottomLeft, w.CharQuads[firstOccurence + searchString.Length - 1].BottomRight);
                            highlightQuads.Insert(0, highlightQuad);

                            searchIndex = firstOccurence + searchString.Length;
                        }
                    }
                }
            }
            if (highlightSelected == ApplicationHighlight.Highlight)
                dleController.docView.DrawSearchRects(highlightQuads.ToArray());
            
        }
        
        /**
         * clearHighlightList - 
         * 
         * clear the list of quads that we have in the highlight list
         * so that they are not redrawn.
         */
        private void clearHighlightList()
        {
            highlightSelected = ApplicationHighlight.NoHighlight;
            highlightQuads.Clear();
            dleController.docView.Refresh();
        }
        #endregion
    }
}
