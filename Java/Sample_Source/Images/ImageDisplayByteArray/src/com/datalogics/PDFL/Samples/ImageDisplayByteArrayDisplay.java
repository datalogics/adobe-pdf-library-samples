package com.datalogics.pdfl.samples.Images.ImageDisplayByteArray;

/*
Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved. 

For complete copyright information, refer to:
http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 */
import java.awt.*;
import java.awt.Point;
import java.awt.image.*;
import java.nio.ByteOrder;
import java.util.EnumSet;

import javax.swing.*;

import com.datalogics.PDFL.*;

@SuppressWarnings("serial")
public class ImageDisplayByteArrayDisplay extends JComponent {

    private Document currentDoc = null;
    private BufferedImage currentPageRaster = null;
    
    public ImageDisplayByteArrayDisplay() {
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

        // Must invert the page to get from PDF with origin at lower left,
        // to a bitmap with the origin at upper right.
        Matrix matrix = new Matrix().scale(1, -1).translate(0, -height);
        
        // Make some drawing parameters
        DrawParams params = new DrawParams();
        params.setColorSpace(ColorSpace.DEVICE_RGBA);
        params.setMatrix(matrix);
        params.setDestRect(new Rect(0, 0, width, height));
        params.setFlags(EnumSet.of(DrawFlags.DO_LAZY_ERASE, DrawFlags.USE_ANNOT_FACES));
        params.setSmoothFlags(EnumSet.of(SmoothFlags.IMAGE, SmoothFlags.LINE_ART, SmoothFlags.TEXT));
        // demonstrate setting Black Point Compensation
        params.setEnableBlackPointCompensation(true);
        // demonstrate using a cancel procedure for long draws
        params.setCancelProc(new PGDCancelProc(false));
        // Setting a progress procedure will print drawing stage information.
        params.setProgressProc(new PGDRenderProgressProc());
        // Draw. We get a byte array that has the samples in left-right
        // top-bottom order.
        byte[] values = pg.drawContents(params);

        // wrap our byte array in a data buffer for the Raster
        DataBufferByte buf = new DataBufferByte(values, values.length);

        // Make a raster out of the interleaved values
        int bands[];
        if (ByteOrder.nativeOrder() == ByteOrder.LITTLE_ENDIAN)
            bands = new int[]{0, 1, 2, 3};
        else
            bands = new int[]{3, 2, 1, 0};

        int stride = width * 4;
        WritableRaster raster = Raster.createInterleavedRaster(buf, width, height, 
                stride,  /* pixel stride */4, 
                bands, new Point(0,0));

        // A color model for component raster, where the samples are spread between data bytes
        ColorModel colorModel = new ComponentColorModel(
                java.awt.color.ColorSpace.getInstance(java.awt.color.ColorSpace.CS_sRGB), 
                true, false, Transparency.OPAQUE, DataBuffer.TYPE_BYTE);

        // mix the color model and the raster itself into a new image to draw
        BufferedImage image = new BufferedImage(colorModel, raster, true, null);


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
