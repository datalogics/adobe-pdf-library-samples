package com.datalogics.PDFL.Samples;

/*
Copyright (c) 2017, Datalogics, Inc. All rights reserved. 

For complete copyright information, refer to:
http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
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


@SuppressWarnings("serial")
public class ImageDisplayer extends JComponent {

    private Document currentDoc = null;
    private Image currentPageRaster = null;
    private Matrix matrix = new Matrix();

    public ImageDisplayer() {
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
    
    public void displayPage() {
        Page pg = new Page(currentDoc, 0);
        
        Rect mediaBox = pg.getMediaBox();
        int width = (int)mediaBox.getRight() - (int)mediaBox.getLeft();
        int height = (int)mediaBox.getTop() - (int)mediaBox.getBottom();

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
        matrix = new Matrix().scale(1, -1).translate(0, -height);
        
        pg.drawContents(image, 
                        matrix, // matrix
                        new Rect(0, 0, width, height));  // updateRect
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
	}
        
}
