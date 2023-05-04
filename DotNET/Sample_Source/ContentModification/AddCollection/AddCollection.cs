using System;
using Datalogics.PDFL;

/*
 * 
 * This sample shows how to add a Collection to a PDF document to turn that document into a PDF Portfolio.
 * 
 * A PDF Portfolio can hold and display multiple additional files as attachments.
 * 
 * For more detail see the description of the AddCollection sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/working-with-a-pdf-collection-or-portfolio
 * 
 *
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace AddCollection
{
    class AddCollection
    {
        static void Main(string[] args)
        {
            Console.WriteLine("AddCollection Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/Attachments.pdf";
                String sOutput = "Portfolio.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                Document doc = new Document(sInput);

                Console.WriteLine("Input file: " + sInput + ". Writing to " + sOutput);

                // Check if document already has collection
                Collection collection = doc.Collection;

                // if document doesn't have collection create it.
                if (collection == null)
                {
                    doc.CreateCollection();
                    collection = doc.Collection;
                }

                // Create a couple of schema fields
                CollectionSchemaField field = new CollectionSchemaField("Description", SchemaFieldSubtype.Description);
                field.Name = "DescriptionField";
                field.Index = 0;
                field.Visible = true;
                field.Editable = false;

                CollectionSchemaField field1 = new CollectionSchemaField("Number", SchemaFieldSubtype.Number);
                field1.Name = "NumberField";
                field1.Index = 1;
                field1.Visible = true;
                field1.Editable = true;

                // Retrieve schema from collection.
                CollectionSchema schema = collection.Schema;

                // Add fields to the obtained schema.
                schema.AddField(field);
                schema.AddField(field1);

                // Create sort collection.
                // Each element of the array is a name that identifies a field
                // described in the parent collection dictionary.
                // The array form is used to allow additional fields to contribute
                // to the sort, where each additional field is used to break ties.
                System.Collections.Generic.IList<CollectionSortItem> colSort =
                    new System.Collections.Generic.List<CollectionSortItem>();
                colSort.Add(new CollectionSortItem("Description", false));
                colSort.Add(new CollectionSortItem("Number", true));

                // Set sort array to the collection
                collection.Sort = colSort;

                // Set view mode
                collection.ChangeCollectionViewMode(CollectionViewType.Detail, CollectionSplitType.SplitPreview);

                int fieldsCount = schema.FieldsNumber;
                for (int i = 0; i < fieldsCount; ++i)
                {
                    CollectionSchemaField fld = schema.GetField(i);
                    Console.WriteLine("Name: " + fld.Name + " Index:" + fld.Index);
                }

                foreach (CollectionSortItem item in collection.Sort)
                {
                    Console.WriteLine("Sort item name: " + item.Name + " Order:" + item.Ascending);
                }

                doc.Save(SaveFlags.Full, sOutput);
            }
        }
    }
}
