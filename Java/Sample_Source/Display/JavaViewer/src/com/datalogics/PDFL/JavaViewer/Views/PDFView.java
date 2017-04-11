/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views;

/*
 * PDFView - it is an implementation of PDF interface. This class is view model
 * of the document. It contains document size, getters and setters to scroll
 * position and for rendering document image on the panel. It is also
 * responsible for the tooltips showing.
 */

import java.awt.Adjustable;
import java.awt.BorderLayout;
import java.awt.Dimension;
import java.awt.Graphics;
import java.awt.KeyEventDispatcher;
import java.awt.KeyboardFocusManager;
import java.awt.Point;
import java.awt.Rectangle;
import java.awt.event.AdjustmentEvent;
import java.awt.event.AdjustmentListener;
import java.awt.event.ComponentAdapter;
import java.awt.event.ComponentEvent;
import java.awt.event.ComponentListener;
import java.awt.event.KeyEvent;
import java.awt.event.MouseEvent;
import java.awt.event.MouseWheelEvent;
import java.awt.event.MouseWheelListener;
import java.awt.image.BufferedImage;
import java.util.HashMap;

import javax.swing.JComponent;
import javax.swing.JPanel;
import javax.swing.JScrollBar;
import javax.swing.ToolTipManager;
import javax.swing.event.MouseInputAdapter;

import com.datalogics.PDFL.JavaViewer.Presentation.ApplicationController;
import com.datalogics.PDFL.JavaViewer.Presentation.PDFPresenter;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.PDF;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.TextEdit;

public class PDFView extends JComponent implements PDF {
    public PDFView(ApplicationController appCtrl) {
        this.appController = appCtrl;
        this.presenter = appController.getPDFPresenter();
        this.oldLocation = new Point();
        this.viewOrigin = new Point();
        this.viewSize = new Dimension();
        this.bufferImage = new BufferedImage(1, 1, BufferedImage.TYPE_INT_ARGB);

        setLayout(new BorderLayout());

        initPdfPanel();
        initInputHandlers();
        initScrolls();
        initCursors();

        this.doLayout();
    }

    private void initPdfPanel() {
        pdfPanel = new JPanel() {
            @Override
            public void paintComponent(Graphics g) {
                Graphics gr = null;
                Rectangle visibleRect = getVisibleRect();
                try {
                    Rectangle clip = g.getClipBounds();
                    g.setClip(0, 0, visibleRect.width, visibleRect.height);

                    if (bufferImage.getWidth() != visibleRect.width || bufferImage.getHeight() != visibleRect.height)
                        bufferImage = new BufferedImage(visibleRect.width, visibleRect.height, BufferedImage.TYPE_INT_ARGB);

                    gr = bufferImage.getGraphics();
                    gr.setColor(g.getColor());
                    gr.setFont(g.getFont());
                    gr.setClip(g.getClipBounds());

                    g.setClip(clip.x, clip.y, clip.width, clip.height);

                    if (partialRepaint) {
                        int dx = viewOrigin.x - oldLocation.x;
                        int dy = viewOrigin.y - oldLocation.y;

                        Point shiftDistance = new Point();
                        Rectangle paintRect = new Rectangle();
                        Dimension repaintAreaSize = new Dimension();

                        if (canBlit(dx, dy, shiftDistance, paintRect, visibleRect, repaintAreaSize)) {
                            gr.copyArea(dx, dy, repaintAreaSize.width, repaintAreaSize.height, shiftDistance.x, shiftDistance.y);
                            gr.setClip(visibleRect.intersection(paintRect));
                        } else
                            partialRepaint = false;
                    }
                    if (!partialRepaint)
                        gr.setClip(0, 0, visibleRect.width, visibleRect.height);

                    partialRepaint = false;
                    fullRepaint = false;
                    oldLocation = viewOrigin;

                    super.paintComponent(gr);
                    gr.translate(-viewOrigin.x, -viewOrigin.y);
                    appController.draw(gr, gr.getClipBounds());
                } finally {
                    gr.dispose();
                }
                g.drawImage(bufferImage, visibleRect.x, visibleRect.y, null);
            }

            @Override
            public Point getToolTipLocation(MouseEvent e) {
                return new Point(toolTipLocation.x - viewOrigin.x, toolTipLocation.y - viewOrigin.y);
            }
        };
        pdfPanel.setLayout(null);
        pdfPanel.setDoubleBuffered(false);
        this.add(pdfPanel, BorderLayout.CENTER);

        this.addComponentListener(new ComponentAdapter() {
            @Override
            public void componentResized(ComponentEvent e) {
                presenter.visibleViewSized();
                repaintView();
            }
        });
    }

    private void initInputHandlers() {
        MouseInputAdapter mouseAdapter = new MouseInputAdapter() {
            @Override
            public void mouseClicked(MouseEvent e) {
                if (e.getClickCount() == 2)
                    appController.mouseDoubleClicked(new InputDataView(e, viewOrigin, false));
            }

            @Override
            public void mouseMoved(MouseEvent e) {
                appController.mouseMoved(new InputDataView(e, viewOrigin, false));
            }

            @Override
            public void mouseDragged(MouseEvent e) {
                appController.mouseMoved(new InputDataView(e, viewOrigin, true));
            }

            @Override
            public void mousePressed(MouseEvent e) {
                appController.mousePressed(new InputDataView(e, viewOrigin, true));
            }

            @Override
            public void mouseReleased(MouseEvent e) {
                appController.mouseReleased(new InputDataView(e, viewOrigin, false));
            }
        };

        pdfPanel.addMouseListener(mouseAdapter);
        pdfPanel.addMouseMotionListener(mouseAdapter);

        KeyboardFocusManager.getCurrentKeyboardFocusManager().addKeyEventDispatcher(new KeyEventDispatcher() {
            public boolean dispatchKeyEvent(KeyEvent e) {
                switch (e.getID()) {
                case KeyEvent.KEY_PRESSED:
                    appController.keyPressed(new InputDataView(e));
                    break;
                case KeyEvent.KEY_RELEASED:
                    appController.keyReleased(new InputDataView(e));
                    break;
                case KeyEvent.KEY_TYPED:
                    appController.keyTyped(new InputDataView(e));
                    break;
                }
                return false;
            }
        });

        this.addMouseWheelListener(new MouseWheelListener() {
            public void mouseWheelMoved(MouseWheelEvent e) {
                final JScrollBar scroll = (e.isControlDown()) ? horzScroll : vertScroll;
                final int wheelRotation = e.getWheelRotation();
                final int incrementMultiplier = (wheelRotation != 0) ? wheelRotation / Math.abs(wheelRotation) : 0; // has values of 1 or -1 to indicate scroll direction
                final int increment = ((e.isShiftDown()) ? scroll.getBlockIncrement() : scroll.getUnitIncrement()) * incrementMultiplier;
                scroll.setValue(scroll.getValue() + increment);
            }
        });
    }

    private void initScrolls() {
        final AdjustmentListener pdfAdjustment = new AdjustmentListener() {
            public void adjustmentValueChanged(AdjustmentEvent e) {
                presenter.scrollChanged(e.getAdjustable().getOrientation() == Adjustable.VERTICAL);
            }
        };

        final ComponentListener scrollVisibility = new ComponentAdapter() {
            @Override
            public void componentHidden(ComponentEvent e) {
                presenter.visibleViewSized();
            }

            @Override
            public void componentShown(ComponentEvent e) {
                presenter.visibleViewSized();
            }
        };

        horzScroll = new ScrollingView();
        horzScroll.setOrientation(JScrollBar.HORIZONTAL);
        horzScroll.addAdjustmentListener(pdfAdjustment);
        horzScroll.setUnitIncrement(5);
        horzScroll.setBlockIncrement(blockIncremet);
        horzScroll.addComponentListener(scrollVisibility);
        this.add(horzScroll, BorderLayout.SOUTH);

        vertScroll = new ScrollingView();
        vertScroll.setOrientation(JScrollBar.VERTICAL);
        vertScroll.addAdjustmentListener(pdfAdjustment);
        vertScroll.setUnitIncrement(5);
        vertScroll.setBlockIncrement(blockIncremet);
        vertScroll.addComponentListener(scrollVisibility);
        this.add(vertScroll, BorderLayout.EAST);

        appController.setBlockIncrement(blockIncremet);
    }

    private void initCursors() {
        cursorMap = new HashMap<PDF.Cursor, Integer>();
        cursorMap.put(PDF.Cursor.DEFAULT, java.awt.Cursor.DEFAULT_CURSOR);
        cursorMap.put(PDF.Cursor.CROSS, java.awt.Cursor.CROSSHAIR_CURSOR);
        cursorMap.put(PDF.Cursor.HAND, java.awt.Cursor.HAND_CURSOR);
        cursorMap.put(PDF.Cursor.TEXT, java.awt.Cursor.TEXT_CURSOR);
        cursorMap.put(PDF.Cursor.MOVE, java.awt.Cursor.MOVE_CURSOR);
        cursorMap.put(PDF.Cursor.SIZE_N, java.awt.Cursor.N_RESIZE_CURSOR);
        cursorMap.put(PDF.Cursor.SIZE_NE, java.awt.Cursor.NE_RESIZE_CURSOR);
        cursorMap.put(PDF.Cursor.SIZE_E, java.awt.Cursor.E_RESIZE_CURSOR);
        cursorMap.put(PDF.Cursor.SIZE_SE, java.awt.Cursor.SE_RESIZE_CURSOR);
        cursorMap.put(PDF.Cursor.SIZE_S, java.awt.Cursor.S_RESIZE_CURSOR);
        cursorMap.put(PDF.Cursor.SIZE_SW, java.awt.Cursor.SW_RESIZE_CURSOR);
        cursorMap.put(PDF.Cursor.SIZE_W, java.awt.Cursor.W_RESIZE_CURSOR);
        cursorMap.put(PDF.Cursor.SIZE_NW, java.awt.Cursor.NW_RESIZE_CURSOR);

        for (PDF.Cursor cursor : PDF.Cursor.values()) {
            if (!cursorMap.containsKey(cursor))
                System.out.println("No cursor binding for key " + cursor);
        }

        setViewCursor(PDF.Cursor.DEFAULT);
    }

    private boolean canBlit(int dx, int dy, Point shiftDistance, Rectangle paintRect, Rectangle visibleRect, Dimension repaintAreaSize) {
        int absDX = Math.abs(dx);
        int absDY = Math.abs(dy);
        boolean retValue = false;
        if ((dx == 0) && (dy != 0) && absDY < visibleRect.height) {
            if (dy > 0)
                paintRect.y = visibleRect.height - dy;
            else
                paintRect.y = 0;
            shiftDistance.y = -dy;
            repaintAreaSize.width = visibleRect.width;
            repaintAreaSize.height = visibleRect.height + absDY;
            paintRect.width = visibleRect.width;
            paintRect.height = absDY;
            retValue = true;
        }
        if ((dx != 0) && (dy == 0) && absDX < visibleRect.width) {
            if (dx > 0)
                paintRect.x = visibleRect.width - dx;
            else
                paintRect.x = 0;
            shiftDistance.x = -dx;
            repaintAreaSize.width = visibleRect.width + absDX;
            repaintAreaSize.height = visibleRect.height;
            paintRect.height = visibleRect.height;
            paintRect.width = absDX;
            retValue = true;
        }
        return retValue;
    }

    private void correctBoundsOnPanel() {
        if (textEdit != null)
            textEdit.setBoundsOffset(new Point(-viewOrigin.x, -viewOrigin.y));
    }

    public PDF.Scrolling getScroll(boolean vertical) {
        return (vertical) ? vertScroll : horzScroll;
    }

    public Dimension getVisibleViewSize() {
        return pdfPanel.getVisibleRect().getSize();
    }

    public void setViewSize(Dimension preferredSize) {
        this.viewSize = preferredSize;
        oldLocation = new Point();
        viewOrigin = new Point();
        repaintView();
    }

    public Point getViewOrigin() {
        return new Point(this.viewOrigin);
    }

    public void setViewOrigin(Point origin) {
        this.viewOrigin = new Point(origin);
        correctBoundsOnPanel();

        if (!fullRepaint) {
            partialRepaint = true;
            this.repaint();
        }
    }

    public void repaintView() {
        partialRepaint = false;
        fullRepaint = true;
        this.repaint();
    }

    public PDF.Cursor getViewCursor() {
        return currentCursor;
    }

    public void setViewCursor(PDF.Cursor cursor) {
        if (cursor != null && cursor != currentCursor) {
            currentCursor = cursor;
            pdfPanel.setCursor(new java.awt.Cursor(cursorMap.get(cursor)));
        }
    }

    public TextEdit createTextEdit(Rectangle textRect) {
        this.textEdit = new TextEditView(textRect, pdfPanel);
        correctBoundsOnPanel();
        return textEdit;
    }

    public ToolTip showToolTip(final String message, final Point location) {
        this.toolTipLocation = location;

        pdfPanel.setToolTipText(convertToolTipMessage(message));

        ToolTipManager.sharedInstance().registerComponent(pdfPanel);
        ToolTipManager.sharedInstance().setEnabled(true);

        ToolTipManager.sharedInstance().setInitialDelay(2000);
        ToolTipManager.sharedInstance().setDismissDelay(3000);

        return new ToolTip() {
            public void cancel() {
                ToolTipManager.sharedInstance().setEnabled(false);
                ToolTipManager.sharedInstance().unregisterComponent(pdfPanel);
            }
        };
    }

    // use this method for create MultiLine tooltip
    private String convertToolTipMessage(String message) {
        String mess = "<Html>";
        for (char ch : message.toCharArray()) {
            mess += ch == '\n' ? "<br>" : ch;
        }
        mess += "</html>";
        return mess;
    }

    private Point viewOrigin;
    private Dimension viewSize;

    private ApplicationController appController;
    private PDFPresenter presenter;

    private JPanel pdfPanel;
    private ScrollingView horzScroll;
    private ScrollingView vertScroll;
    private BufferedImage bufferImage;
    private boolean partialRepaint;
    private boolean fullRepaint;
    private Point oldLocation;

    private HashMap<PDF.Cursor, Integer> cursorMap;
    private PDF.Cursor currentCursor;

    private TextEditView textEdit;
    private Point toolTipLocation;

    private int blockIncremet = 50;
}
