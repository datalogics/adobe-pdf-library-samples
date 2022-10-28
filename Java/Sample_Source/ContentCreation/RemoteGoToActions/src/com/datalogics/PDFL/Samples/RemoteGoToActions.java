package com.datalogics.pdfl.samples.ContentCreation.RemoteGoToActions;


import java.util.EnumSet;

import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.LinkAnnotation;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.FileSpecification;
import com.datalogics.PDFL.RemoteDestination;
import com.datalogics.PDFL.RemoteGoToAction;

/*
 * RemoteGoToActions is similar to LaunchActions. The program generates a PDF file
 * with an annotation, in the form of a rectangle. Click on the rectangle shown in this
 * PDF file and a separate PDF file opens.
 * 
 * RemoteGoToActions differs from LaunchActions in that it includes a RemoteDestination object.
 * This object describes the rectangle used in the PDF file in a series of statements at the command prompt. 
 *
 * For more detail see the description of the RemoteGoToActions sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/working-with-actions-in-pdf-files#remotegotoactions
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class RemoteGoToActions {

    /**
    * @param args
    */
    public static void main(String[] args) throws Throwable {
        System.out.println("CreateRemoteGoToActions sample:");

        Library lib = new Library();

        try {
            Document doc = new Document();
            
            // Standard letter size page (8.5" x 11")
            Rect pageRect = new Rect(0, 0, 612, 792);
            Page docpage = doc.createPage(Document.BEFORE_FIRST_PAGE, pageRect);
            System.out.println("Created page.");
            
            // Create our link with a RemoteGoToAction
            LinkAnnotation newLink = new LinkAnnotation(docpage, new Rect(153, 198, 306, 396));
            System.out.println("Created new Link Annotation");       
 
            // RemoteGoToActions need a FileSpecification and a RemoteDestination.
            //
            // The FileSpecification specifies which file should be opened when the LinkAnnotation is "clicked"
            //
            // The RemoteDestination specifies a particular view (page number, Fit type, rectangle, and zoom level)
            // to display when the file in the FileSpecification is opened. Remember page numbers start at 0.
            //
            // FileSpecification objects, RemoteDestination objects, and RemoteGoToActions 
            // must be associated with a Document. The association happens at object creation time and cannot be changed.
            //
            // FileSpecifications and RemoteDestinations are associated with the Document that is passed in the constructor.
            // RemoteGoToActions are associated with the same Document as the RemoteDestination used to create it.
            //
            // When creating a RemoteGoToAction, make sure that the FileSpecification and RemoteDestination are both
            // associated with the same Document, or an exception will be thrown.
            
            // FileSpecifications can take either a relative or an absolute path. It is best to specify
            // a relative path if the document is intended to work a cross multiple platforms (Windows, Linux, Mac)
            FileSpecification fileSpec = new FileSpecification(doc, Library.getResourceDirectory() + "Sample_Input/ducky.pdf");
            System.out.println("Path to remote document : " + fileSpec.getPath());

            RemoteDestination remoteDest = new RemoteDestination(doc, 0, "XYZ", new Rect(0, 0, 4 * 72, 4 * 72), 1.5);
            System.out.println("When the Link is clicked the remote document will open to : ");
            System.out.println("Page Number : " + remoteDest.getPageNumber());
            System.out.println("zoom level : " + remoteDest.getZoom());
            System.out.println("fit type : " + remoteDest.getFitType());
            System.out.println("rectangle : " + remoteDest.getDestRect().toString());

            // Now create the RemoteGoToAction from the fileSpec and the RemoteDestination
            RemoteGoToAction remoteAction = new RemoteGoToAction(fileSpec, remoteDest);
            
            // assign the RemoteGoToAction to the LinkAnnotation
            newLink.setAction(remoteAction);
            
            doc.save(EnumSet.of(SaveFlags.FULL), "RemoteGoToActions-out.pdf");
   
            System.out.println("Output saved as RemoteGoToActions-out.pdf");
        }
        finally {
            lib.delete();
        }
    }

}
