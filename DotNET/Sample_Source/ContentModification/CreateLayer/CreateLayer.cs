using System;
using Datalogics.PDFL;

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
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace CreateLayer
{
    class CreateLayer
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CreateLayer Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/ducky.pdf";
                String sOutput = "CreateLayer-out.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Input file: " + sInput + ", writing to " + sOutput);

                Document doc = new Document(sInput);

                Console.WriteLine("Opened a document.");

                Page pg = doc.GetPage(0);
                Image img = (pg.Content.GetElement(0) as Image);

                // Containers, Forms and Annotations can be attached to an
                // OptionalContentGroup; other content (like Image) can
                // be made optional by placing it inside a Container
                Container container = new Container();
                container.Content = new Content();
                container.Content.AddElement(img);

                // We replace the Image with the Container
                // (which now holds the image)
                pg.Content.RemoveElement(0);
                pg.UpdateContent();

                pg.Content.AddElement(container);
                pg.UpdateContent();

                // We create a new OptionalContentGroup and place it in the
                // OptionalContentConfig.Order array
                OptionalContentGroup ocg = CreateNewOptionalContentGroup(doc, "Rubber Ducky");

                // Now we associate the Container with the OptionalContentGroup
                // via an OptionalContentMembershipDict.  Note that we MUST
                // update the Page's content afterwards.
                AssociateOCGWithContainer(doc, ocg, container);
                pg.UpdateContent();

                doc.Save(SaveFlags.Full, sOutput);
            }
        }

        // Create an OptionalContentGroup with a given name, and add it to the
        // default OptionalContentConfig's Order array.
        public static OptionalContentGroup CreateNewOptionalContentGroup(Document doc, string name)
        {
            // Create an OptionalContentGroup
            OptionalContentGroup ocg = new OptionalContentGroup(doc, name);

            // Add it to the Order array -- this is required so that the OptionalContentGroup
            // will appear in the 'Layers' control panel in Acrobat.  It will appear in
            // the control panel with the name given in the OptionalContentGroup constructor.
            OptionalContentOrderArray order_list = doc.DefaultOptionalContentConfig.Order;
            order_list.Insert(order_list.Length, new OptionalContentOrderLeaf(ocg));

            return ocg;
        }

        // Associate a Container with an OptionalContentGroup via an OptionalContentMembershipDict.
        // This function associates a Containter with a single OptionalContentGroup and uses
        // a VisibilityPolicy of AnyOn.
        public static void AssociateOCGWithContainer(Document doc, OptionalContentGroup ocg, Container cont)
        {
            // Create an OptionalContentMembershipDict.  The options here are appropriate for a
            // 'typical' usage; other options can be used to create an 'inverting' layer
            // (i.e. 'Display this content when the layer is turned OFF'), or to make the
            // Container's visibility depend on several OptionalContentGroups
            OptionalContentMembershipDict ocmd =
                new OptionalContentMembershipDict(doc, new[] {ocg}, VisibilityPolicy.AnyOn);

            // Associate the Container with the OptionalContentMembershipDict
            cont.OptionalContentMembershipDict = ocmd;
        }
    }
}
