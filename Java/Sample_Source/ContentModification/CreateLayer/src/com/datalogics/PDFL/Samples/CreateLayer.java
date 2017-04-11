/*
 * This sample adds an Optional Content Group (a layer) to a PDF document and
 * then adds an image to that layer. 
 * 
 * The related ChangeLayerConfiguration program makes layers visible or invisible.
 * 
 * You can toggle back and forth to make the layer (the duck image) visible or invisible
 * in the PDF file. Open the output PDF document in Adobe Acrobat, click View and select
 * Show/Hide. Select Navigation Panes and Layers to display the layers in the PDF file. 
 * Click on the box next to the name of the layer.
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.Samples;

import com.datalogics.PDFL.*;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.EnumSet;

/**
 *
 */
public class CreateLayer {

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) {
        System.out.println("CreateLayer Sample:");

        Library lib = new Library();

        try {
            System.out.println("Initialized the library.");
            String sInput = "../../Resources/Sample_Input/ducky.pdf";
            String sOutput = "CreateLayer-out.pdf";
            if (args.length > 0)
                sInput = args[0];
            if (args.length > 1)
                sOutput = args[1];
            System.out.println("Input file: " + sInput + ", will write to " + sOutput);

            Document doc = new Document(sInput);

            Page pg = doc.getPage(0);
            Image img = (Image)pg.getContent().getElement(0);

            // Containers, Forms and Annotations can be attached to an
            // OptionalContentGroup; other content (like Image) can
            // be made optional by placing it inside a Container
            Container container = new Container();
            container.setContent(new Content());
            container.getContent().addElement(img);

            // We replace the Image with the Container
            // (which now holds the image)
            pg.getContent().removeElement(0);
            pg.updateContent();

            pg.getContent().addElement(container);
            pg.updateContent();

            // We create a new OptionalContentGroup and place it in the
            // OptionalContentConfig.Order array
            OptionalContentGroup ocg = createNewOptionalContentGroup(doc, "Rubber Ducky");

            // Now we associate the Container with the OptionalContentGroup
            // via an OptionalContentMembershipDict.  Note that we MUST
            // update the Page's content afterwards.
            associateOCGWithContainer(doc, ocg, container);
            pg.updateContent();

            doc.save(EnumSet.of(SaveFlags.FULL), sOutput);
        }
        finally {
            lib.delete();
        }
    }

    // Create an OptionalContentGroup with a given name, and add it to the
    // default OptionalContentConfig's Order array.
    public static OptionalContentGroup createNewOptionalContentGroup(Document doc, String name)
    {
        // Create an OptionalContentGroup
        OptionalContentGroup ocg = new OptionalContentGroup(doc, name);

        // Add it to the Order array -- this is required so that the OptionalContentGroup
        // will appear in the 'Layers' control panel in Acrobat.  It will appear in
        // the control panel with the name given in the OptionalContentGroup constructor.
        OptionalContentOrderArray order_list = doc.getDefaultOptionalContentConfig().getOrder();
        order_list.insert(order_list.getLength(), new OptionalContentOrderLeaf(ocg));

        return ocg;
    }

    // Associate a Container with an OptionalContentGroup via an OptionalContentMembershipDict.
    // This function associates a Containter with a single OptionalContentGroup and uses
    // a VisibilityPolicy of AnyOn.
    public static void associateOCGWithContainer(Document doc, OptionalContentGroup ocg, Container cont)
    {
        // Create an OptionalContentMembershipDict.  The options here are appropriate for a
        // 'typical' usage; other options can be used to create an 'inverting' layer
        // (i.e. 'Display this content when the layer is turned OFF'), or to make the
        // Container's visibility depend on several OptionalContentGroups
        OptionalContentMembershipDict ocmd = new OptionalContentMembershipDict(doc,
                new ArrayList<OptionalContentGroup>(Arrays.asList(ocg)), VisibilityPolicy.ANY_ON);

        // Associate the Container with the OptionalContentMembershipDict
        cont.setOptionalContentMembershipDict(ocmd);
    }

}
