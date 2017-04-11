package com.datalogics.PDFL.Samples;

/*
 * This sample shows how to open a PDF document, search for text in the pages and highlight
 * the text. This utility is a simpler version of the JavaViewer sample.
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 * 
 */

import java.awt.Dimension;
import java.awt.Graphics;
import java.awt.Graphics2D;
import java.awt.Image;
import java.awt.image.BufferedImage;
import java.awt.image.DirectColorModel;
import java.awt.image.WritableRaster;
import java.nio.ByteOrder;

import javax.swing.JComponent;

import com.datalogics.PDFL.*;
import java.util.ArrayList;
import java.util.List;

@SuppressWarnings("serial")
public class  DisplayPDF extends JComponent {

	private Document currentDoc = null;
	private Image currentPageRaster = null;
	private double zoom = 1.0; // percent to scale/zoom the page
	private List<Quad> highlightedQuads = new ArrayList<Quad>();
    private Matrix matrix = new Matrix();
    
	public DisplayPDF() {
		
		final Dimension d = new Dimension(400, 500);
		setDoubleBuffered(true);
		setIgnoreRepaint(true);
		setPreferredSize(d);
		setMinimumSize(d);
		setSize(d);
	}
	
	public void setDocument(Document _document) {
		this.currentDoc = _document;
	}

    public void clearHighlights() {
        highlightedQuads.clear();
    }
    
    public void addHighlight(Quad highlightQuad) {
        highlightedQuads.add(highlightQuad);
    }
    
	public void displayPage(int pageNumber) {
		Page pg = new Page(currentDoc, pageNumber);
		
		// Use the CropBox to determine region of the page to display.
		// Some documents may have a CropBox that is not located at the
		// origin of the page.  
		Rect cropBox = pg.getCropBox();
		int width = (int)cropBox.getRight() - (int)cropBox.getLeft();
		int height = (int)cropBox.getTop() - (int)cropBox.getBottom();

		// Shift the page display based on the location of the CropBox
		int shiftx = (int)cropBox.getLeft();
		int shifty = (int)cropBox.getBottom();

		width = (int)((double)width * zoom);
		height = (int)((double)height * zoom);
		shiftx = (int)((double)shiftx * zoom);
		shifty = (int)((double)shifty * zoom);		

		// Create an image in the appropriate organization for the library to draw into
		// Channel masks are R, G, B, A, and our implementation of DrawContents does use DeviceRGBA
		DirectColorModel colorModel;
		// MUCH faster to ignore the alpha channel. 
		if (ByteOrder.nativeOrder() == ByteOrder.LITTLE_ENDIAN)
			colorModel = new DirectColorModel(32, 0x000000ff, 0x0000ff00, 0x00ff0000);
		else
			colorModel = new DirectColorModel(32, 0xff000000, 0x00ff0000, 0x0000ff00);
			
		WritableRaster raster = colorModel.createCompatibleWritableRaster(width, height);
		BufferedImage image = new BufferedImage(colorModel, raster, true, null);
		final Graphics2D g = image.createGraphics();
		g.setColor(java.awt.Color.white);
		g.fillRect(0, 0, image.getWidth(), image.getHeight());
		g.dispose();

        // Must invert the page to get from PDF with origin at lower left,
        // to a bitmap with the origin at upper right.
	// Also, we use the shiftx and shifty to take the location of the CropBox
	// into account.
        matrix = new Matrix().scale(1, -1).translate(-shiftx, -height - shifty).scale(zoom, zoom);
	
	Rect updateRect = new Rect((int)cropBox.getLeft(), (int)cropBox.getBottom(),
				   (int)cropBox.getLeft() + width, (int)cropBox.getBottom() + height);
		
        pg.drawContents(image, 
                        matrix, // matrix
                        updateRect);  
        // TODO Could speed this up more by making a copy? Sources seem to indicate that once the DataBuffer for an
        // image has been accessed, the JVM can no longer optimize by keeping the image in VRAM.  Since
        // the call to drawContents does just that, we may want to create a new buffered image from the result
        // in order to give the JVM back the ability to keep the image cached in VRAM.
        currentPageRaster = image;
        
        setPreferredSize(new Dimension(width, height));
        setSize(width, height);
        repaint();
	}

	@Override
	public void paintComponent(Graphics g) {
		super.paintComponent(g);
	
		g.setColor(java.awt.Color.gray);
		g.fillRect(0, 0, this.getWidth(), this.getHeight());
		g.drawImage(currentPageRaster, 0, 0, null);
        
        for (Quad q : highlightedQuads) {
            Quad tq = q.transform(matrix);
            g.setColor(java.awt.Color.blue);
            
            int[] xpoints = new int[] {
                (int)tq.getTopLeft().getH(),
                (int)tq.getBottomLeft().getH(),
                (int)tq.getBottomRight().getH(),
                (int)tq.getTopRight().getH(),
            };
            int[] ypoints = new int[] {
                (int)tq.getTopLeft().getV(),
                (int)tq.getBottomLeft().getV(),
                (int)tq.getBottomRight().getV(),
                (int)tq.getTopRight().getV(),
            };
            g.drawPolygon(xpoints, ypoints, xpoints.length);
        }
	}

	public void setZoom(double zoom) {
		this.zoom = zoom;
	}

	public double getZoom() {
		return zoom;
	}
	
	
}
