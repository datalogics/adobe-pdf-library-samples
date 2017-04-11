package com.datalogics.PDFL.Samples;

import com.datalogics.PDFL.*;

/*
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class ExportDocumentImages {
       ImageType exporttype;
        int next;
        ImageCollection ic;
        
        public void export_doc_images_type( Document doc, ImageType imtype)
        {
            exporttype = imtype;
            int pgno;
            for (pgno = 0; pgno < doc.getNumPages(); pgno++)
            {
                Page pg = doc.getPage(pgno);
                Content content = pg.getContent();
                Export_Element_Images(content);
            }
			
            if ( ic.getCount() != 0 )
            {
                try
                {
                    ic.save("ImageExport-page" + pgno + "-out.tif", ImageType.TIFF);
                }
                catch( Exception ex)
                {
                    System.out.println("Cannot write file: " + ex.getMessage());
                }
            }
        }
        
        public void export_doc_images( Document doc)
        {
            export_doc_images_type(doc, ImageType.TIFF);
            export_doc_images_type(doc, ImageType.JPEG);
            export_doc_images_type(doc, ImageType.PNG);
            export_doc_images_type(doc, ImageType.GIF);
            export_doc_images_type(doc, ImageType.BMP);
        }
        
        public void Export_Element_Images(Content content )
        {
            int i = 0;
            ImageSaveParams isp;

            while (i < content.getNumElements() )
            {
                Element e = content.getElement(i);
                if (e instanceof Image)
                {
                    Image img = (Image)e;
                    // Weed out impossible on nonsensical combinations.
                    if (img.getColorSpace() == ColorSpace.DEVICE_CMYK && exporttype != ImageType.JPEG)
                    {
                        i++;
                        continue;
                    }
                    
                    try
                    {

	                    if ( exporttype == ImageType.TIFF)
	                    {
	                        ic.append(img);
	                        isp = new ImageSaveParams();
	                        isp.setCompression(CompressionCode.LZW);
	                        img.save("ImageExport-out" + next + ".tif", exporttype, isp);
	                    }
	
	                    if ( exporttype == ImageType.JPEG)
	                    {
	                        isp = new ImageSaveParams();
	                        isp.setJPEGQuality(80);
							img.save("ImageExport-out" + next + ".jpg", exporttype, isp);
	                    }
	
	                    if ( exporttype == ImageType.PNG )
	                    {
							img.save("ImageExport-out" + next + ".png", exporttype);
	                    }
	
	                    if ( exporttype == ImageType.GIF)
	                    {
							img.save("ImageExport-out" + next + ".gif", exporttype);
	                    }
	
	                    if ( exporttype == ImageType.BMP)
	                    {
							img.save("ImageExport-out" + next + ".bmp", exporttype);
	                    }
                    }
                    catch (Exception ex)
                    {
                    	System.out.println("Cannot write file: " + ex.getMessage());
                    }
                    next++;


                }
                else if (e instanceof Container)
                {
                    System.out.println("Recursing through a Container");
                    Export_Element_Images(((Container)e).getContent());
                }
                else if (e instanceof Group)
                {
                    System.out.println("Recursing through a Group");
                    Export_Element_Images(((Group)e).getContent());
                }
                else if (e instanceof Form)
                {
                    System.out.println("Recursing through a Form");
                    Export_Element_Images(((Form) e).getContent());
                }
                i++;
            }
        }

    public ExportDocumentImages() {
        next = 0;
        ic = new ImageCollection();
        
    }

}
