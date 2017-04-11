/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Cache;

import java.awt.Dimension;
import java.awt.Graphics2D;
import java.awt.Image;
import java.awt.Point;
import java.awt.Rectangle;
import java.awt.Transparency;
import java.awt.geom.Point2D;
import java.awt.image.BufferedImage;
import java.awt.image.ColorModel;
import java.awt.image.ComponentColorModel;
import java.awt.image.DataBufferByte;
import java.awt.image.Raster;
import java.awt.image.WritableRaster;
import java.nio.ByteOrder;
import java.util.EnumSet;

import com.datalogics.PDFL.ColorSpace;
import com.datalogics.PDFL.DrawFlags;
import com.datalogics.PDFL.DrawParams;
import com.datalogics.PDFL.LibraryException;
import com.datalogics.PDFL.Matrix;
import com.datalogics.PDFL.OptionalContentContext;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.SmoothFlags;

/**
 * PageCache - contains all information that is used to render the PDF page to
 * the screen.
 * 
 * This class holds the dimensions of each page, the scroll position and an
 * array of tiles which within each tile is a bitmap containing a piece of a
 * page (depending on the size/zoom level there may be more than one tile).
 */
public class PageCache implements PageModel {
    PageCache(Page page, double scale, OptionalContentContext layers) {
        calcPageParams(page, scale, layers);
    }

    public Rectangle getBounds() {
        return new Rectangle(this.offsetPageBounds);
    }

    public double getScale() {
        return this.scale;
    }

    public double getRotation() {
        return this.rotation;
    }

    public Rectangle transform(Rect rectPdf) {
        final Rect rectTemp = rectPdf.transform(calcPageMatrix(offsetPageBounds));
        Rectangle rectView = new Rectangle((int) Math.round(rectTemp.getLeft()), (int) Math.round(rectTemp.getBottom()), (int) Math.round(rectTemp.getWidth()), (int) Math.round(rectTemp.getHeight()));
        return rectView;
    }

    public Point transform(com.datalogics.PDFL.Point pointPdf) {
        final com.datalogics.PDFL.Point pointTemp = pointPdf.transform(calcPageMatrix(offsetPageBounds));
        return new Point((int) Math.round(pointTemp.getH()), (int) Math.round(pointTemp.getV()));
    }

    public Rect transform(Rectangle rectView) {
        Rectangle pageRect = new Rectangle(rectView);
        return new Rect(pageRect).transform(calcPageMatrix(offsetPageBounds).invert());
    }

    public com.datalogics.PDFL.Point transform(Point pointView) {
        final Rect rectPdf = transform(new Rectangle(pointView));
        return new com.datalogics.PDFL.Point(rectPdf.getLeft(), rectPdf.getBottom());
    }

    public Point2D.Float relative(Point pointView) {
        Point2D.Float relLocation = new Point2D.Float();

        relLocation.x = (float) (pointView.x - offsetPageBounds.x) / (float) offsetPageBounds.getWidth();
        relLocation.y = (float) (pointView.y - offsetPageBounds.y) / (float) offsetPageBounds.getHeight();
        return relLocation;
    }

    public Point2D.Float relative(com.datalogics.PDFL.Point pointPdf) {
        return relative(transform(pointPdf));
    }

    public java.awt.Point relative(Point2D.Float pointView) {
        java.awt.Point absLocation = new Point();

        absLocation.x = Math.round(pointView.x * (float) offsetPageBounds.getWidth()) + offsetPageBounds.x;
        absLocation.y = Math.round(pointView.y * (float) offsetPageBounds.getHeight()) + offsetPageBounds.y;
        return absLocation;
    }

    public com.datalogics.PDFL.Point relativePdf(Point2D.Float pointView) {
        return transform(relative(pointView));
    }

    void cache() {
        final PageTile fullPage = cacheTile(this.cropRect);
        if (fullPage != null) { // try caching full page at first
            for (int i = 0, rows = tiles.length; i < rows; ++i) {
                for (int j = 0, cols = tiles[i].length; j < cols; ++j) {
                    tiles[i][j] = cacheTile(tiles[i][j].getRect(), fullPage);
                }
            }
        } else {
            tiles[0][0] = cacheTile(tiles[0][0].getRect());
        }
    }

    /**
     * Method draws all tiles which user can see.
     * 
     * @param g - graphics for drawing
     * @param updRect - the part of document view port which has to update.
     * @return - repainted area
     * @throws OutOfMemoryError
     */
    Rectangle draw(Graphics2D g, Rectangle updRect) throws OutOfMemoryError {
        Rectangle occupiedRect = new Rectangle();

        // read invalid tiles from the document, then perform draw
        int left = (int) cropRect.getMaxX(), top = (int) cropRect.getMaxY(), right = 0, bottom = 0;
        for (int i = 0, rows = tiles.length; i < rows; ++i) {
            for (int j = 0, cols = tiles[i].length; j < cols; ++j) {
                PageTile tile = tiles[i][j];
                Rectangle tileOffsetRect = new Rectangle(tile.getRect());
                tileOffsetRect.translate(pageOffset.x - cropRect.x, pageOffset.y - cropRect.y);

                if (tileOffsetRect.intersects(updRect)) {
                    if (!tile.isValid()) {
                        tile = tiles[i][j] = cacheTile(tile.getRect());
                    }
                    g.drawImage(tile.get(), tileOffsetRect.x, tileOffsetRect.y, null);

                    left = Math.min(left, tileOffsetRect.x);
                    top = Math.min(top, tileOffsetRect.y);
                    right = Math.max(right, (int) Math.round(tileOffsetRect.getMaxX()));
                    bottom = Math.max(bottom, (int) Math.round(tileOffsetRect.getMaxY()));
                }
            }
        }
        occupiedRect = new Rectangle(left, top, Math.max(right - left, 0), Math.max(bottom - top, 0));

        /*
         * g.setColor(java.awt.Color.BLUE); g.draw(this.cropRect);
         * g.setColor(java.awt.Color.RED); for (PageTile[] tileCols : tiles) {
         * for (PageTile tile : tileCols) { Rectangle tileOffsetRect = new
         * Rectangle(tile.getRect()); tileOffsetRect.translate(pageOffset.x,
         * pageOffset.y); g.draw(tileOffsetRect); } }
         */
        return occupiedRect;
    }

    void invalidate(boolean collect) {
        createPageTiles();
        if (collect)
            System.gc();
    }

    void invalidate(Rectangle changedArea) {
        Rectangle relativeArea = new Rectangle(changedArea);
        relativeArea.translate(-pageOffset.x, -pageOffset.y);

        // find tiles which overlap changed area and invalidate them
        isPageValid = false;
        for (int i = 0, rows = tiles.length; i < rows; ++i) {
            for (int j = 0, cols = tiles[i].length; j < cols; ++j) {
                if (tiles[i][j].getRect().intersects(relativeArea)) {
                    tiles[i][j] = createTile(tiles[i][j].getRect(), null);
                }
                isPageValid = isPageValid || (tiles[i][j].get() != null);
            }
        }
    }

    boolean isValid() {
        return this.isPageValid;
    }

    void setOffset(java.awt.Point offset) {
        this.pageOffset = new java.awt.Point(offset);

        this.offsetPageBounds = new Rectangle(cropRect);
        offsetPageBounds.setLocation(pageOffset);
    }

    java.awt.Point getOffset() {
        return new java.awt.Point(this.pageOffset);
    }

    Dimension getPageSize() {
        return this.cropRect.getSize();
    }

    @Override
    protected void finalize() throws Throwable {
        page.delete();
        super.finalize();
    }

    private Rect transformRectByPageParams(Rect rect) {
        Matrix transformMatrix = new Matrix();
        final double xShift = rect.getLLx() + rect.getWidth() / 2.0;
        final double yShift = rect.getLLy() + rect.getHeight() / 2.0;

        final double xShiftModified = xShift * scale;
        final double yShiftModified = yShift * scale;
        transformMatrix = (isPageSideways()) ? transformMatrix.translate(yShiftModified, xShiftModified) : transformMatrix.translate(xShiftModified, yShiftModified);
        transformMatrix = transformMatrix.rotate(rotation).scale(scale, -scale);

        transformMatrix = transformMatrix.translate(-xShift, -yShift);
        return rect.transform(transformMatrix);
    }

    private void calcPageParams(Page page, double scale, OptionalContentContext layers) {
        this.page = page;
        this.scale = scale;
        this.layers = layers;
        this.rotation = page.getRotation().ordinal() * 90;

        Rect cropBoxTransformed = transformRectByPageParams(page.getCropBox());
        Rect mediaBoxTransformed = transformRectByPageParams(page.getMediaBox());

        this.cropRect = new Rectangle();
        this.cropRect.x = (int) (mediaBoxTransformed.getLeft() + cropBoxTransformed.getLeft());
        this.cropRect.y = (int) (mediaBoxTransformed.getTop() - cropBoxTransformed.getTop());
        this.cropRect.width = (int) cropBoxTransformed.getWidth();
        this.cropRect.height = (int) cropBoxTransformed.getHeight();

        setOffset(new java.awt.Point());
        createPageTiles();
    }

    private void createPageTiles() {
        final int cols = (int) Math.ceil(this.cropRect.getWidth() / PageCache.TILE_SIZE.getWidth());
        final int rows = (int) Math.ceil(this.cropRect.getHeight() / PageCache.TILE_SIZE.getHeight());
        this.tiles = new PageTile[rows][cols];

        for (int r = 0, yOff = this.cropRect.y; r < rows; ++r, yOff += PageCache.TILE_SIZE.height) {
            int tileW = (isPageSideways()) ? PageCache.TILE_SIZE.height : PageCache.TILE_SIZE.width;
            int tileH = (isPageSideways()) ? PageCache.TILE_SIZE.width : PageCache.TILE_SIZE.height;

            if (r == rows - 1)
                tileH = Math.min(tileH, (int) this.cropRect.getMaxY() - yOff);

            for (int c = 0, xOff = this.cropRect.x; c < cols; ++c, xOff += PageCache.TILE_SIZE.width) {
                if (c == cols - 1)
                    tileW = Math.min(tileW, (int) this.cropRect.getMaxX() - xOff);
                this.tiles[r][c] = createTile(new Rectangle(xOff, yOff, tileW, tileH), null);
            }
        }
        isPageValid = false;
    }

    private PageTile cacheTile(Rectangle tileRect) {
        class PageSoBigException extends Exception {
            public PageSoBigException() {
            }

            public PageSoBigException(String message) {
            }
        }
        PageTile cahedTile = null;
        try {
            final ColorSpace colorSpace = ColorSpace.DEVICE_RGBA;
            final int pixelStride = colorSpace.getNumComponents();

            if (tileRect.isEmpty() || MAX_PAGE_SIZE < (tileRect.width * tileRect.height * pixelStride))
                throw new PageSoBigException("bitmap is so big. Just clip it!");

            final Matrix pageMatrix = calcPageMatrix(cropRect);
            DrawParams params = new DrawParams();
            params.setOptionalContentContext(layers);
            params.setColorSpace(colorSpace);
            params.setMatrix(new Matrix().translate(-tileRect.x, -tileRect.y).concat(pageMatrix));
            params.setDestRect(new Rect(0, 0, tileRect.getWidth(), tileRect.getHeight()));
            params.setUpdateRect(new Rect(tileRect).transform(pageMatrix.invert()));
            params.setFlags(EnumSet.of(DrawFlags.DO_LAZY_ERASE, DrawFlags.USE_ANNOT_FACES));
            params.setSmoothFlags(EnumSet.of(SmoothFlags.IMAGE, SmoothFlags.LINE_ART, SmoothFlags.TEXT));
            params.setEnableBlackPointCompensation(true);
            params.setBypassCopyPerm(true);
            // Draw. We get a byte array that has the samples in left-right top-bottom order. may throw OutOfMemoryError
            byte[] values = null;
            try {
                values = page.drawContents(params);
            } catch (LibraryException e) {
                values = new byte[pixelStride]; // create false pixel to have valid byte array
            } catch (RuntimeException e) {
                throw new PageSoBigException();// new OutOfMemoryError();
            }

            // wrap our byte array in a data buffer for the Raster
            final DataBufferByte buf = new DataBufferByte(values, values.length);

            // Make a raster out of the interleaved values
            final boolean littleEndian = (ByteOrder.nativeOrder() == ByteOrder.LITTLE_ENDIAN);
            int bands[] = new int[pixelStride];
            for (int i = 0; i < pixelStride; ++i)
                bands[littleEndian ? i : (pixelStride - 1 - i)] = i;

            final int stride = tileRect.width * pixelStride;
            WritableRaster raster = Raster.createInterleavedRaster(buf, tileRect.width, tileRect.height, stride, pixelStride, bands, null);

            // A color model for component raster, where the samples are spread
            // between data bytes
            ColorModel colorModel = new ComponentColorModel(java.awt.color.ColorSpace.getInstance(java.awt.color.ColorSpace.CS_sRGB), true, false, Transparency.OPAQUE, buf.getDataType());

            // mix the color model and the raster itself into a new image to
            // draw
            BufferedImage img = new BufferedImage(colorModel, raster, true, null);
            cahedTile = createTile(tileRect, img);
        } catch (PageSoBigException e) {
            cahedTile = null;
        }
        return cahedTile;
    }

    private Matrix calcPageMatrix(Rectangle viewPageBounds) {
        Matrix pageMatrix = new Matrix();
        final double xShiftModified = viewPageBounds.x + viewPageBounds.width / 2.0;
        final double yShiftModified = viewPageBounds.y + viewPageBounds.height / 2.0;

        pageMatrix = pageMatrix.translate(xShiftModified, yShiftModified);
        pageMatrix = pageMatrix.rotate(rotation).scale(scale, -scale);

        final Rect pdfPageBounds = page.getCropBox();
        pageMatrix = pageMatrix.translate(-pdfPageBounds.getLLx() - pdfPageBounds.getWidth() / 2.0, -pdfPageBounds.getLLy() - pdfPageBounds.getHeight() / 2.0);

        return pageMatrix;
    }

    private PageTile cacheTile(Rectangle tileRect, PageTile fullTile) {
        Rectangle relToTileRect = new Rectangle(tileRect);
        relToTileRect.translate(-fullTile.getRect().x, -fullTile.getRect().y);

        final BufferedImage fullImage = (BufferedImage) fullTile.get();
        BufferedImage img = fullImage.getSubimage(relToTileRect.x, relToTileRect.y, relToTileRect.width, relToTileRect.height);
        return createTile(tileRect, img);
    }

    private PageTile createTile(Rectangle tileRect, Image img) {
        if (img != null)
            isPageValid = true;

        final PageTile tile = new PageTile(tileRect, img);
        return tile;
    }

    private boolean isPageSideways() {
        return (rotation % 180) != 0;
    }

    private static class PageTile {
        public PageTile(Rectangle tileRect, Image tile) {
            this.tileRect = tileRect;
            this.tile = tile;
        }

        public boolean isValid() {
            return tile != null;
        }

        public Rectangle getRect() {
            return this.tileRect;
        }

        public Image get() {
            return this.tile;
        }

        private Rectangle tileRect;
        private Image tile;
    }

    private final static int MAX_PAGE_SIZE = 16 * 1024 * 1024; // 16 megabytes is maximum size for one-time caching
    private static Dimension TILE_SIZE = new Dimension(600, 600);

    private Page page;
    private double scale;
    private double rotation;
    private java.awt.Point pageOffset; // view page offset relative to the viewed document
    private Rectangle cropRect; // scaled crop rectangle to calculate proper tiles positions
    private Rectangle offsetPageBounds; // view page bounds that used in a page model representation
    private OptionalContentContext layers; // layers which are to be rendered

    private PageTile[][] tiles; // tile array
    private boolean isPageValid; // page valid if at least one tile has been created
}
