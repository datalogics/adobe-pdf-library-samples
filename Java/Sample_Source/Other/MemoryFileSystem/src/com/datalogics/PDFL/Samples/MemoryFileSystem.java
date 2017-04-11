package com.datalogics.PDFL.Samples;


import java.util.EnumSet;

import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.TempStoreType;

/*
  * 
 * This sample demonstrates how to initialize and use RAM memory instead of the local
 * hard disk to save temporary files, in order to save processing time.
 * 
 * The program sets the DefaultTempStore property of the Adobe PDF Library object to
 * TempStoreType.Memory. The program can also set a maximum amount of RAM to use by
 * applying a value to the DefaultTempStoreMemLimit property.
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class MemoryFileSystem {

    /**
     * @param args
     */
    public static void main(String[] args) throws Throwable {
        Library lib = new Library();
        System.out.println("Initialized the library.");

        try {
            String sOutput = "MemoryFileSystem-out.pdf";
            if ( args.length > 0 )
                sOutput = args[0];
            System.out.println("MemoryFileSystem sample: will write " + sOutput );

            Rect bounds = new Rect(0, 0, 612, 792);

            // Set in-memory file system as temporary storage
            lib.setDefaultTempStore(TempStoreType.MEMORY);

            // Set memory limit up to 100 kB. When occupied memory
            // exceeds the limit value, disk temporary storage will be used.
            lib.setDefaultTempStoreMemLimit(100);

            Document doc = null;
            Page pg = null;
            try {
                doc = new Document();
                try {
                    pg = doc.createPage(Document.BEFORE_FIRST_PAGE, bounds);
                }
                finally {
                    if (pg != null)
                        pg.delete();
                }

                doc.save(EnumSet.of(SaveFlags.FULL), sOutput);
            }
            finally {
                if (doc != null)
                    doc.delete();
            }
        }
        finally {
            lib.delete();
        }
    }
}
