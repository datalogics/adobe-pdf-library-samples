package com.datalogics.pdfl.samples.ContentCreation.CreateBookmarks;


import java.util.EnumSet;

import com.datalogics.PDFL.Bookmark;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.*;
/*
 * 
 * This sample program shows how to automatically add bookmarks to a PDF file.
 * The program opens a source file called sample.PDF, adds bookmarks to it, and
 * saves an output file called CreateBookmarks-out.PDF that shows the new bookmark structure.
 * 
 * Bookmarks can help a user move through a PDF document. A hyperlink can be added elsewhere
 * in the same document that can be associated with a bookmark. For example, bookmarks can be to
 * assigned to each section heading. The user could click on a heading link in the Table of Contents 
 * and move directly to the appropriate section of the document.
 * 
 * Open the CreateBookmarks-out.PDF file in Acrobat and click the Bookmark icon to display the bookmarks that
 * the CreateBookmarks program added to the output PDF document. Several of these bookmarks will zoom to
 * parts of the page. The last three, Child1, 2, and 3, are dummy bookmarks that do not respond when you
 * click on them.  They demonstrate how to rearrange existing bookmarks.
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
public class CreateBookmarks 
{
	static Action CreateGoToAction(Document doc, Rect rect, double zoom)
    {
        ViewDestination viewDest = new ViewDestination(doc, 0, "XYZ", rect, zoom);
        return new GoToAction(viewDest);
    }

    static Bookmark PrepareDescendentBookmarkToRebase(Document doc, String bookmarkName)
    {
        Bookmark bm = doc.getBookmarkRoot().findDescendentBookmark(bookmarkName);
        bm.unlink();
        return bm;
    }

    static void AddBookmarkAsChild(Document doc, String parentBookmarkName, String childBookmarkName)
    {
        doc.getBookmarkRoot().findDescendentBookmark(parentBookmarkName).addChild(PrepareDescendentBookmarkToRebase(doc, childBookmarkName));
    }

    static void AddBookmarkAsSubtree(Document doc, int maxDepth, String parentBookmarkName, String childBookmarkName, String subTreeTitle)
    {
        doc.findBookmark(parentBookmarkName, maxDepth).addSubtree(PrepareDescendentBookmarkToRebase(doc, childBookmarkName), subTreeTitle);
    }

	/**
	 * @param args
	 */
	public static void main(String[] args) throws Throwable 
	{
                System.out.println("CreateBookmarks sample:");

                Library lib = new Library();
		System.out.println("Initialized the library.");
                String sInput = Library.getResourceDirectory() + "Sample_Input/sample.pdf";
                String sOutput = "CreateBookmarks-out.pdf";
                if (args.length > 0)
                    sInput = args[0];
                if (args.length > 1)
                    sOutput = args[1];
                System.out.println("Input file: " + sInput + ", will write to " + sOutput);

		try 
		{
	        
	        Document doc = new Document(sInput);
	        try
	        {
	            Bookmark rootBookmark = doc.getBookmarkRoot();

                // Create a few bookmarks that point to page "0" (developer page 0, user page 1)
                // with different ViewDestinations that have different Rects and zoom levels
                Page page = doc.getPage(0);
                try
                {
                    // Use CreateNewChild() to hang a new bookmark in tree off root bookmark
                    Bookmark bm0 = rootBookmark.createNewChild("(A) Root child, points to page 1, upper left corner, 300% zoom");
                    Rect rect = page.getMediaBox();
                    bm0.setAction(CreateGoToAction(doc, rect, 3.0));

                    // Use CreateNewChild() to hang a new bookmark in tree off newly created bookmark
                    Bookmark bm1 = bm0.createNewChild("(B) Root child's child, points to page 1, halfway down page, 75% zoom");
                    rect = new Rect(rect.getLeft(), rect.getBottom(), rect.getRight(), rect.getTop() / 2.0);
                    bm1.setAction(CreateGoToAction(doc, rect, 0.75));

                    // Use CreateNewSibling() to hang a new bookmark in tree next to existing bookmark
                    bm1 = bm0.createNewSibling("(C) Root child's sibling, points to page 1, 1/4 from top of page, 133% zoom");
                    rect = new Rect(rect.getLeft(), rect.getBottom(), rect.getRight(), rect.getTop() * 0.75);
                    bm1.setAction(CreateGoToAction(doc, rect, 1.33));

                    AddBookmarkAsChild(
                        doc,
                        "(C) Root child's sibling, points to page 1, 1/4 from top of page, 133% zoom",
                        "(B) Root child's child, points to page 1, halfway down page, 75% zoom"
                        );

                    AddBookmarkAsSubtree(
                        doc,
                        12,
                        "(A) Root child, points to page 1, upper left corner, 300% zoom",
                        "(C) Root child's sibling, points to page 1, 1/4 from top of page, 133% zoom",
                        "Bookmark formerly known as '(C) ... '"
                        );

                    // Create three bookmarks as new children to the root
                    bm0 = rootBookmark.createNewChild("Child 2");

                    bm0.addNextSibling(rootBookmark.createNewChild("Child 1"));
                    bm0.addPreviousSibling(rootBookmark.createNewChild("Child 3"));

                    doc.save(EnumSet.of(SaveFlags.FULL), sOutput);
                }
                finally
                {
                    page.delete();
                }
            }
            finally
            {
                doc.delete();
            }
        }
		finally 
		{
			lib.delete();
		}
	}
}
