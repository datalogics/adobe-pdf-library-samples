package com.datalogics.pdfl.samples.Images.ImageEmbedICCProfile;

import com.datalogics.PDFL.ColorSpace;
import com.datalogics.PDFL.CompressionCode;
import com.datalogics.PDFL.Content;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.ICCBasedColorSpace;
import com.datalogics.PDFL.ImageSaveParams;
import com.datalogics.PDFL.ImageType;
import com.datalogics.PDFL.PDFStream;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.PageImageParams;
import com.datalogics.PDFL.RenderIntent;

/*
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
public class ExportDocImage {

	ImageType exporttype;

	public void export_doc_images_type(Document doc, PDFStream profileStream,
			ImageType imtype) {
		exporttype = imtype;
		ColorSpace cs = new ICCBasedColorSpace(profileStream);

		for (int pgno = 0; pgno < doc.getNumPages(); pgno++) {
			Page pg = doc.getPage(pgno);

			Export_Element_Images(pg.getContent(), cs, pg, pgno);

		}
	}

	public void export_doc_images(Document doc, PDFStream profileStream) {

		export_doc_images_type(doc, profileStream, ImageType.TIFF);

	}

	public void Export_Element_Images(Content content, ColorSpace csp, Page pg,
			int pNum) {

		ImageSaveParams isp;

		try {
			isp = new ImageSaveParams();
			isp.setCompression(CompressionCode.LZW);

			PageImageParams pip = new PageImageParams();
			pip.setImageColorSpace(csp);

			com.datalogics.PDFL.Image outImage = pg.getImage(pg.getCropBox(),
					pip);

			String filenamevar = "";

			pip.setRenderIntent(RenderIntent.SATURATION);
			outImage = pg.getImage(pg.getCropBox(), pip);
			filenamevar = "ImageEmbedICCProfile-out_sat" + pNum + ".tif";
			outImage.save(filenamevar, exporttype, isp);

			pip.setRenderIntent(RenderIntent.ABS_COLORIMETRIC);
			outImage = pg.getImage(pg.getCropBox(), pip);
			filenamevar = "ImageEmbedICCProfile-out_abs" + pNum + ".tif";
			outImage.save(filenamevar, exporttype, isp);

			pip.setRenderIntent(RenderIntent.PERCEPTUAL);
			outImage = pg.getImage(pg.getCropBox(), pip);
			filenamevar = "ImageEmbedICCProfile-out_pert" + pNum + ".tif";
			outImage.save(filenamevar, exporttype, isp);

			pip.setRenderIntent(RenderIntent.REL_COLORIMETRIC);
			outImage = pg.getImage(pg.getCropBox(), pip);
			filenamevar = "ImageEmbedICCProfile-out_rel" + pNum + ".tif";
			outImage.save(filenamevar, exporttype, isp);

		} catch (Exception ex) {
			System.out.println("Cannot write file: " + ex.getMessage());
		}

	}

}
