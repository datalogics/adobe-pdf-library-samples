/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import java.awt.BasicStroke;
import java.awt.Color;
import java.awt.Graphics;
import java.awt.Graphics2D;
import java.awt.Rectangle;
import java.awt.RenderingHints;
import java.awt.Stroke;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

import com.datalogics.PDFL.Matrix;
import com.datalogics.PDFL.Point;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.JavaViewer.Utils;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationHolder;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationListener;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationProperties;
import com.datalogics.PDFL.JavaViewer.Presentation.Cache.PageModel;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.EditorListener;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Interactive;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.PDF;

/**
 * BaseAnotationEditor - responsible for the capturing, releasing, drawing,
 * moving and changing editors' state.
 * 
 * This class configures an editor and passes the control to its child.
 */
public abstract class BaseAnnotationEditor implements Interactive, AnnotationListener {
    public BaseAnnotationEditor() {
        this.guides = new LinkedHashMap<Integer, GuideRectangle>();
        this.inputMap = new HashMap<Enum<?>, BaseAnnotationInput>();
        this.outputMap = new HashMap<Enum<?>, BaseAnnotationOutput>();

        this.input = ensureValidInput(input);
        this.output = ensureValidOutput(output);

        this.lastHit = new Hit();
        this.listeners = new ArrayList<EditorListener>();
    }

    public void init(AnnotationHolder annotationHolder, Enum<?> state) {
        this.annotationHolder = annotationHolder;
        this.state = state;
    }

    public void capture() {
        annotationHolder.addAnnotationListener(this);
        annotationHolder.capture();
        onCapture(); // perform custom init
    }

    public void release() {
        onRelease(); // perform custom deinit
        annotationHolder.release();
        annotationHolder.removeAnnotationListener(this);
    }

    public AnnotationHolder getHolder() {
        return annotationHolder;
    }

    public AnnotationProperties getProperties() {
        return annotationHolder.getProperties();
    }

    public void configureState(Enum<?> state, BaseAnnotationInput input, BaseAnnotationOutput drawer) {
        inputMap.put(state, input);
        outputMap.put(state, drawer);
    }

    public Hit testHit(java.awt.Point point) {
        if (lastHit == null || !lastHit.getLocation().equals(point)) {
            Hit result = new Hit();

            final double borderDelta = GUIDE_SIZE;
            final Rect boundingRect = annotationHolder.getProperties().getBoundingRect();

            final Rect externalRect = new Rect(boundingRect.getLLx() - borderDelta, boundingRect.getLLy() - borderDelta, boundingRect.getURx() + borderDelta, boundingRect.getURy() + borderDelta);

            final Rect internalRect = new Rect(boundingRect.getLLx() + borderDelta, boundingRect.getLLy() + borderDelta, boundingRect.getURx() - borderDelta, boundingRect.getURy() - borderDelta);

            final Point pdfPoint = getPageModel().transform(point);
            result.setLocation(pdfPoint);

            if (Utils.pointInRect(pdfPoint, boundingRect)) {
                result.includeHit(Hit.Flags.SHAPE_HIT);
                result.includeHit(Hit.Flags.RECT_HIT);
            }

            if (Utils.pointInRect(pdfPoint, externalRect) && !Utils.pointInRect(pdfPoint, internalRect)) {
                result.includeHit(Hit.Flags.BORDER_HIT);
            }

            if (result.hasHit()) {
                for (GuideRectangle guide : guides.values()) {
                    final Point location = guide.getLocation();
                    final double dx = Math.abs(location.getH() - pdfPoint.getH());
                    final double dy = Math.abs(location.getV() - pdfPoint.getV());
                    final double distance = Math.max(dx, dy);
                    if (distance <= GUIDE_SIZE + GUIDE_DELTA) {
                        result.includeHit(Hit.Flags.GUIDE_HIT);
                        result.setGuide(guide);
                        break;
                    }
                }
            }

            lastHit = result;
            output.changeCursor(lastHit);
        }
        return lastHit;
    }

    /**
     * Moves annotation or its guide. This methods corrects moving location if
     * it is out of page bound. Also this method lets to reflect guides if it is
     * necessary
     * 
     * @param hit - contains moving information.
     * @param location - location where annotation has been moved to
     */
    public void move(Hit hit, java.awt.Point location) {
        Point pdfPoint = getPageModel().transform(location);
        GuideRectangle guide = hit.getGuide();

        final Rect boundingRect = getProperties().getBoundingRect();
        Rect newRect = null;
        double scaleX = 1, scaleY = 1;

        if (guide != null) { // guide-based move
            pdfPoint = ensurePointInPage(pdfPoint);
            final int guideLayout = hit.getGuide().getLayout();
            final Point guideLocation = hit.getGuide().getLocation();

            // determine move directions
            final boolean moveX0 = (guideLayout & GuideRectangle.Layout.W) != 0;
            final boolean moveY0 = (guideLayout & GuideRectangle.Layout.S) != 0;
            final boolean moveX1 = (guideLayout & GuideRectangle.Layout.E) != 0;
            final boolean moveY1 = (guideLayout & GuideRectangle.Layout.N) != 0;

            // calculate shift distance
            final double dx0 = moveX0 ? (pdfPoint.getH() - guideLocation.getH()) : 0;
            final double dy0 = moveY0 ? (pdfPoint.getV() - guideLocation.getV()) : 0;
            final double dx1 = moveX1 ? (pdfPoint.getH() - guideLocation.getH()) : 0;
            final double dy1 = moveY1 ? (pdfPoint.getV() - guideLocation.getV()) : 0;

            // determine new bounding points
            double x0 = boundingRect.getLLx() + dx0;
            double y0 = boundingRect.getLLy() + dy0;
            double x1 = boundingRect.getURx() + dx1;
            double y1 = boundingRect.getURy() + dy1;

            // avoid zero-sizes
            final double delta = 0.01;
            if (moveX0 && (x0 > x1 - delta))
                x0 = Math.max(x0, x1 + delta);
            if (moveY0 && (y0 > y1 - delta))
                y0 = Math.max(y0, y1 + delta);
            if (moveX1 && (x0 > x1 - delta))
                x1 = Math.min(x1, x0 - delta);
            if (moveY1 && (y0 > y1 - delta))
                y1 = Math.min(y1, y0 - delta);

            guide = reflectGuide(guides, guide, x0, y0, x1, y1);
            hit.setGuide(guide);

            newRect = new Rect(x0, y0, x1, y1);
            scaleX = newRect.getWidth() / boundingRect.getWidth();
            scaleY = newRect.getHeight() / boundingRect.getHeight();
        } else {
            // obtain valid rectangle with regard to produced move
            newRect = transformBoundingRect(new Matrix().translate(pdfPoint.getH() - hit.getLocation().getH(), pdfPoint.getV() - hit.getLocation().getV()));
        }

        hit.setLocation(pdfPoint);
        lastHit = hit;
        output.changeCursor(lastHit);

        Matrix matrix = new Matrix().translate(newRect.getLLx(), newRect.getLLy()).scale(scaleX, scaleY).translate(-boundingRect.getLLx(), -boundingRect.getLLy());

        if (matrix != null && !matrix.equals(new Matrix())) {
            transform(guide, matrix);
        }
    }

    final public void mousePressed(InputData input) {
        final BaseAnnotationInput lastInput = this.input;
        lastInput.mousePressed(input);

        // handler may have been changed during this input operation
        if (lastInput != this.input)
            mousePressed(input);
    }

    final public void mouseReleased(InputData input) {
        final BaseAnnotationInput lastInput = this.input;
        lastInput.mouseReleased(input);

        // handler may have been changed during this input operation
        if (lastInput != this.input)
            mouseReleased(input);
    }

    final public void mouseMoved(InputData input) {
        final BaseAnnotationInput lastInput = this.input;
        lastInput.mouseMoved(input);

        // handler may have been changed during this input operation
        if (lastInput != this.input)
            mouseMoved(input);
    }

    final public void mouseDoubleClicked(InputData input) {
        final BaseAnnotationInput lastInput = this.input;
        lastInput.mouseDoubleClicked(input);

        // handler may have been changed during this input operation
        if (lastInput != this.input)
            mouseDoubleClicked(input);
    }

    final public void keyPressed(InputData input) {
        final BaseAnnotationInput lastInput = this.input;
        lastInput.keyPressed(input);

        // handler may have been changed during this input operation
        if (lastInput != this.input)
            keyPressed(input);
    }

    final public void keyReleased(InputData input) {
        final BaseAnnotationInput lastInput = this.input;
        lastInput.keyReleased(input);

        // handler may have been changed during this input operation
        if (lastInput != this.input)
            keyReleased(input);
    }

    final public void keyTyped(InputData input) {
        final BaseAnnotationInput lastInput = this.input;
        lastInput.keyTyped(input);

        // handler may have been changed during this input operation
        if (lastInput != this.input)
            keyTyped(input);
    }

    final public void draw(Graphics g, Rectangle updateRect) {
        output.drawShape(g);
        output.drawSelection(g);
    }

    public void setInteractiveContext(InteractiveContext context) {
        this.context = context;
        configureGuides(annotationHolder.getProperties().getBoundingRect()); // re-create guides with consideration of page rotation
    }

    public void update(Object updateData) {
        if (AnnotationHolder.AppearanceUpdate.SHAPE_CHANGED.equals(updateData)) {
            getHolder().updateAppearance();
            configureGuides(getProperties().getBoundingRect());
        }
        getPdfView().repaintView();
    }

    public Enum<?> getState() {
        return state;
    }

    public void setState(Enum<?> state) {
        if (!this.state.equals(state)) {
            final Enum<?> oldState = this.state;
            this.state = state;

            // configure annotation input
            input.activate(false);
            input = ensureValidInput(inputMap.get(state));
            input.activate(true);

            // configure annotation drawer
            output.activate(false);
            output = ensureValidOutput(outputMap.get(state));
            output.activate(true);

            // make cursor change for a new output handler
            output.changeCursor(lastHit);

            // make repaint since output handler has changed
            getPdfView().repaintView();

            for (EditorListener l : getListeners())
                l.stateChanged(this, oldState, this.state);
        }
    }

    public void addEditorListener(EditorListener listener) {
        listeners.add(listener);
    }

    public void removeEditorListener(EditorListener listener) {
        listeners.remove(listener);
    }

    final public InteractiveContext getInteractiveContext() {
        return context;
    }

    final PDF getPdfView() {
        return context != null ? context.getPdfView() : null;
    }

    final public PageModel getPageModel() {
        return context != null ? context.getPageModel() : null;
    }

    final void drawBoundingRect(Graphics g) {
        try {
            Graphics2D gr = (Graphics2D) g;
            final Stroke savedStroke = gr.getStroke();

            final Rectangle rect = getPageModel().transform(getProperties().getBoundingRect());
            gr.setStroke(new BasicStroke(1, BasicStroke.CAP_BUTT, BasicStroke.JOIN_MITER, 1f, new float[] { 1, 1 }, 0.0f));
            gr.setColor(Color.RED);

            gr.draw(rect);
            gr.setStroke(savedStroke);
        } catch (ClassCastException e) {
        }
    }

    final void drawGuides(Graphics g) {
        if (guides != null && !getProperties().getBoundingRect().isEmpty()) {
            for (GuideRectangle guide : guides.values()) {
                drawGuide(g, guide);
            }
        }
    }

    final void drawShape(Graphics g) {
        Graphics savedGraphics = g.create(); // create a new Graphics for annotation drawing
        try {
            configureGraphics((Graphics2D) savedGraphics);
            doDrawShape((Graphics2D) savedGraphics);
        } finally {
            savedGraphics.dispose();
        }
    }

    Hit getGuideHit(int layout) {
        Hit result = new Hit();
        final GuideRectangle guide = guides.get(layout);

        if (guide != null) {
            result.includeHit(Hit.Flags.BORDER_HIT);
            result.includeHit(Hit.Flags.GUIDE_HIT);

            result.setGuide(guide);
            result.setLocation(guide.getLocation());
        }
        return result;
    }

    protected void onCapture() {
    }

    protected void onRelease() {
    }

    protected GuideRectangle reflectGuide(Map<Integer, GuideRectangle> guides, GuideRectangle guide, double x0, double y0, double x1, double y1) {
        // horizontal reflection: switch to other guide
        if (x0 > x1) {
            guide = guides.get(GuideRectangle.Layout.reflect(guide.getLayout(), false));
        }

        // vertical reflection: switch to other guide
        if (y0 > y1) {
            guide = guides.get(GuideRectangle.Layout.reflect(guide.getLayout(), true));
        }
        return guide;
    }

    protected void doTransform(GuideRectangle guide, boolean moveAll, Matrix matrix) {
        getProperties().setBoundingRect(getProperties().getBoundingRect().transform(matrix));
    }

    protected void doConfigureGuides(Map<Integer, GuideRectangle> guides, Rect r) {
        createBoundingGuides(guides, r);
    }

    protected final void createBoundingGuides(Map<Integer, GuideRectangle> guides, Rect r) {
        final int rotation = getPageModel() != null ? (int) getPageModel().getRotation() : 0;
        {
            final int layout = GuideRectangle.Layout.SW;
            guides.put(layout, new GuideRectangle(new Point(r.getLeft(), r.getBottom()), layout, cursorMap.get(GuideRectangle.Layout.rotate(layout, rotation))));
        }
        {
            final int layout = GuideRectangle.Layout.S;
            guides.put(layout, new GuideRectangle(new Point(r.getLeft() + r.getWidth() / 2, r.getBottom()), layout, cursorMap.get(GuideRectangle.Layout.rotate(layout, rotation))));
        }
        {
            final int layout = GuideRectangle.Layout.SE;
            guides.put(layout, new GuideRectangle(new Point(r.getRight(), r.getBottom()), layout, cursorMap.get(GuideRectangle.Layout.rotate(layout, rotation))));
        }
        {
            final int layout = GuideRectangle.Layout.W;
            guides.put(layout, new GuideRectangle(new Point(r.getLeft(), r.getBottom() + r.getHeight() / 2), layout, cursorMap.get(GuideRectangle.Layout.rotate(layout, rotation))));
        }
        {
            final int layout = GuideRectangle.Layout.E;
            guides.put(layout, new GuideRectangle(new Point(r.getRight(), r.getBottom() + r.getHeight() / 2), layout, cursorMap.get(GuideRectangle.Layout.rotate(layout, rotation))));
        }
        {
            final int layout = GuideRectangle.Layout.NW;
            guides.put(layout, new GuideRectangle(new Point(r.getLeft(), r.getTop()), layout, cursorMap.get(GuideRectangle.Layout.rotate(layout, rotation))));
        }
        {
            final int layout = GuideRectangle.Layout.N;
            guides.put(layout, new GuideRectangle(new Point(r.getLeft() + r.getWidth() / 2, r.getTop()), layout, cursorMap.get(GuideRectangle.Layout.rotate(layout, rotation))));
        }
        {
            final int layout = GuideRectangle.Layout.NE;
            guides.put(layout, new GuideRectangle(new Point(r.getRight(), r.getTop()), layout, cursorMap.get(GuideRectangle.Layout.rotate(layout, rotation))));
        }
    }

    protected abstract void doDrawShape(Graphics2D g);

    protected void configureGraphics(Graphics2D gr) {
        final AnnotationProperties annotationProperties = getProperties();
        final float lineWidth = (float) (annotationProperties.getLineWidth() * getPageModel().getScale());
        float[] dashPattern = annotationProperties.getDashPattern();

        gr.setRenderingHint(RenderingHints.KEY_TEXT_ANTIALIASING, RenderingHints.VALUE_TEXT_ANTIALIAS_ON);
        gr.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON);

        if (!annotationProperties.isSolid()) {
            for (int i = 0; i < dashPattern.length; i++)
                dashPattern[i] *= getPageModel().getScale();
            gr.setStroke(new BasicStroke(lineWidth, BasicStroke.CAP_BUTT, BasicStroke.JOIN_MITER, 1f, dashPattern, 0.0f));
        } else {
            gr.setStroke(new BasicStroke(lineWidth));
        }
    }

    private EditorListener[] getListeners() {
        return listeners.toArray(new EditorListener[0]);
    }

    protected final Point ensurePointInPage(Point point) {
        final Rect pageBounds = getPageModel().transform(getPageModel().getBounds());
        Point corrected = new Point();

        corrected.setH(Math.max(Math.min(point.getH(), pageBounds.getRight()), pageBounds.getLeft()));
        corrected.setV(Math.max(Math.min(point.getV(), pageBounds.getTop()), pageBounds.getBottom()));
        return corrected;
    }

    protected void transform(GuideRectangle guide, Matrix matrix) {
        doTransform(guide, guide == null || isStandardGuide(guide.getLayout()), matrix);
        getPdfView().repaintView();
    }

    private void drawGuide(Graphics g, GuideRectangle guide) {
        java.awt.Point point = getPageModel().transform(guide.getLocation());
        Rectangle rect = new Rectangle(point.x - GUIDE_SIZE, point.y - GUIDE_SIZE, 2 * GUIDE_SIZE, 2 * GUIDE_SIZE);

        g.setColor(Color.WHITE);
        g.fillRect(rect.x, rect.y, rect.width, rect.height);
        g.setColor(Color.RED);
        g.drawRect(rect.x, rect.y, rect.width, rect.height);
    }

    private void configureGuides(Rect r) {
        final int savedLayout = lastHit.getGuide() != null ? lastHit.getGuide().getLayout() : GuideRectangle.Layout.NOTHING;

        // re-initialize guides
        guides.clear();
        doConfigureGuides(guides, r);

        lastHit.setGuide(guides.get(savedLayout));
    }

    private boolean isStandardGuide(int layout) {
        return !GuideRectangle.Layout.isCustom(layout);
    }

    private Rect transformBoundingRect(Matrix matrix) {
        final Rect newRect = getProperties().getBoundingRect().transform(matrix);
        Point newLL = new Point(newRect.getLLx(), newRect.getLLy());
        Point newUR = new Point(newRect.getURx(), newRect.getURy());

        Point validLL = ensurePointInPage(newLL);
        Point validUR = ensurePointInPage(newUR);
        Rect validRect = new Rect(validLL.getH(), validLL.getV(), validUR.getH(), validUR.getV());

        if (validRect.getWidth() != newRect.getWidth() || validRect.getHeight() != newRect.getHeight()) {
            double minH = 0;
            if (validLL.getH() > newLL.getH())
                minH = Math.max(validLL.getH() - newLL.getH(), validUR.getH() - newUR.getH());
            else
                minH = Math.min(validLL.getH() - newLL.getH(), validUR.getH() - newUR.getH());

            double minV = 0;
            if (validLL.getV() > newLL.getV())
                minV = Math.max(validLL.getV() - newLL.getV(), validUR.getV() - newUR.getV());
            else
                minV = Math.min(validLL.getV() - newLL.getV(), validUR.getV() - newUR.getV());

            validRect = newRect.transform(new Matrix().translate(minH, minV));
        }
        return validRect;
    }

    private BaseAnnotationInput ensureValidInput(BaseAnnotationInput input) {
        BaseAnnotationInput validInput = (input != null) ? input : new BaseAnnotationInput() {
            @Override
            public void activate(boolean active) {
                // leave empty
            }
        };
        validInput.setAnnotationEditor(this);
        return validInput;
    }

    private BaseAnnotationOutput ensureValidOutput(BaseAnnotationOutput output) {
        BaseAnnotationOutput validOutput = (output != null) ? output : new BaseAnnotationOutput() {
        };
        validOutput.setAnnotationEditor(this);
        return validOutput;
    }

    protected static final int START_LINE_GUIDE = 0; // used in guide retrieval method for line annotation
    protected static final int END_LINE_GUIDE = 1;

    protected static final int GUIDE_SIZE = 2; // this is not width or height, this is half size, "radius" of the guide
    protected static final int GUIDE_DELTA = 2; // use delta > 0 if you want to hit if mouse is "close enough" to guide

    private static final Map<Integer, PDF.Cursor> cursorMap = Collections.unmodifiableMap(new HashMap<Integer, PDF.Cursor>() {
        {
            put(GuideRectangle.Layout.N, PDF.Cursor.SIZE_N);
            put(GuideRectangle.Layout.E, PDF.Cursor.SIZE_E);
            put(GuideRectangle.Layout.W, PDF.Cursor.SIZE_W);
            put(GuideRectangle.Layout.S, PDF.Cursor.SIZE_S);
            put(GuideRectangle.Layout.NE, PDF.Cursor.SIZE_NE);
            put(GuideRectangle.Layout.NW, PDF.Cursor.SIZE_NW);
            put(GuideRectangle.Layout.SE, PDF.Cursor.SIZE_SE);
            put(GuideRectangle.Layout.SW, PDF.Cursor.SIZE_SW);
        }
    });

    private Enum<?> state;
    private final Map<Enum<?>, BaseAnnotationInput> inputMap;
    private final Map<Enum<?>, BaseAnnotationOutput> outputMap;

    private AnnotationHolder annotationHolder;

    private final Map<Integer, GuideRectangle> guides;
    private InteractiveContext context;

    private BaseAnnotationInput input;
    private BaseAnnotationOutput output;

    private Hit lastHit;

    private List<EditorListener> listeners;
}
