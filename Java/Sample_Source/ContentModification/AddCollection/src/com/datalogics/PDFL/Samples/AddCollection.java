package com.datalogics.pdfl.samples.ContentModification.AddCollection;


import java.util.ArrayList;
import java.util.EnumSet;
import java.util.List;

import com.datalogics.PDFL.CollectionSchema;
import com.datalogics.PDFL.CollectionSchemaField;
import com.datalogics.PDFL.CollectionSortItem;
import com.datalogics.PDFL.CollectionSplitType;
import com.datalogics.PDFL.CollectionViewType;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.GoToAction;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.LinkAnnotation;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.SchemaFieldSubtype;
import com.datalogics.PDFL.URIAction;
import com.datalogics.PDFL.ViewDestination;
import com.datalogics.PDFL.Collection;

/*
* 
 * This sample shows how to add a Collection to a PDF document to turn that document into a PDF Portfolio.
 * 
 * A PDF Portfolio can hold and display multiple additional files as attachments.
 * 
 * For more detail see the description of the AddCollection sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/working-with-a-pdf-collection-or-portfolio
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class AddCollection {

	/**
	 * @param args
	 */
	public static void main(String[] args) throws Throwable {
        System.out.println("AddCollection sample:");

        String sInput = Library.getResourceDirectory() + "Sample_Input/Attachments.pdf";
        String sOutput = "AddCollections-out.pdf";

        Library lib = new Library();

        try {
            if (args.length != 0)
                sInput = args[0];
            System.out.println("Input file: " + sInput);
            Document doc = new Document(sInput);

            // Check if document already has collection
            Collection collection = doc.getCollection();

            // if document doesn't have collection create it.
            if (collection == null)
            {
                doc.createCollection();
                collection = doc.getCollection();
            }

            // Create a couple of schema fields
            CollectionSchemaField field = new CollectionSchemaField("Description", SchemaFieldSubtype.DESCRIPTION);
            field.setName("DescriptionField");
            field.setIndex(0);
            field.setVisible(true);
            field.setEditable(false);

            CollectionSchemaField field1 = new CollectionSchemaField("Number", SchemaFieldSubtype.NUMBER);
            field1.setName("NumberField");
            field1.setIndex(1);
            field1.setVisible(true);
            field1.setEditable(true);

            // Retrieve schema from collection.
            CollectionSchema schema = collection.getSchema();

            // Add fields to the obtained schema.
            schema.addField(field);
            schema.addField(field1);

            // Create sort collection.
            // Each element of the array is a name that identifies a field
            // described in the parent collection dictionary.
            // The array form is used to allow additional fields to contribute
            // to the sort, where each additional field is used to break ties.
            List<CollectionSortItem> colSort = new ArrayList<CollectionSortItem>();

            colSort.add(new CollectionSortItem("Description", false));
            colSort.add(new CollectionSortItem("Number", true));

            // Set sort array to the collection
            collection.setSort(colSort);

            // Set view mode
            collection.changeCollectionViewMode(CollectionViewType.DETAIL, CollectionSplitType.SPLIT_PREVIEW);

            int fieldsCount = schema.getFieldsNumber();
            for (int i = 0; i < fieldsCount; ++i)
            {
                CollectionSchemaField fld = schema.getField(i);
                System.out.println("Name: " + fld.getName() + " Index:" + fld.getIndex());
            }

            for (CollectionSortItem item : collection.getSort())
            {
                System.out.println("Sort item name: " + item.getName() + " Order:" + item.getAscending());
            }

            doc.save(EnumSet.of(SaveFlags.FULL), sOutput);
        }
        finally {
            lib.delete();
        }
    }
}
