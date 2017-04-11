/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Annotations;

import java.util.EnumSet;

import com.datalogics.PDFL.Annotation;
import com.datalogics.PDFL.Color;
import com.datalogics.PDFL.Content;
import com.datalogics.PDFL.Element;
import com.datalogics.PDFL.Font;
import com.datalogics.PDFL.Form;
import com.datalogics.PDFL.GraphicState;
import com.datalogics.PDFL.HorizontalAlignment;
import com.datalogics.PDFL.Path;
import com.datalogics.PDFL.PathPaintOpFlags;
import com.datalogics.PDFL.Point;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.Text;
import com.datalogics.PDFL.TextRun;
import com.datalogics.PDFL.TextState;
import com.datalogics.PDFL.JavaViewer.Utils;

/**
 * FreeTextAnnotationHolder - the child of the BaseAnnotationHolder.
 * 
 * This class allows to manipulate PDFL.FreeTextAnnotation using specific
 * functionality. It helps generate proper appearance for
 * PDFL.FreeTextAnnotation with regard to system font, justification, border
 * size etc.
 */
public class FreeTextAnnotationHolder extends BaseAnnotationHolder {
    @Override
    protected void init(Annotation annotation, int index, int pageIndex) {
        super.init(annotation, index, pageIndex);
    }

    @Override
    protected void doReadProperties() {
        super.doReadProperties();
        readPropertyByName(AnnotationConsts.TEXT_COLOR, Color.class, false);
        readPropertyByName(AnnotationConsts.INTERIOR_COLOR, Color.class, false);
        readPropertyByName(AnnotationConsts.FONT_FACE, String.class, false);
        readPropertyByName(AnnotationConsts.FONT_SIZE, double.class, false);
        readPropertyByName(AnnotationConsts.QUADDING, HorizontalAlignment.class, false);
        setAutosize();
    }

    private void setAutosize() {
        if (getProperties().getAutoSize()) {
            Content content = getAnnotation().getNormalAppearance().getContent();
            double newBoundHeight = 0;
            for (int i = 0; i < content.getNumElements(); i++) {
                Element element = content.getElement(i);
                if (element instanceof Text) {
                    Text currentText = (Text) element;
                    for (int j = 0; j < currentText.getNumberOfRuns(); j++)
                        newBoundHeight += currentText.getRun(j).getBoundingBox().getHeight();
                }
            }
            getProperties().setBoundingRect(new Rect(getProperties().getBoundingRect().getLLx(), getProperties().getBoundingRect().getURy() - newBoundHeight, getProperties().getBoundingRect().getURx(), getProperties().getBoundingRect().getURy()));
            getProperties().setAutoSize(false);
        }
    }

    /**
     * Method is used to generate annotation appearance. It uses current font
     * face, size, alignment and background and foreground colors of annotation.
     * 
     * @return - new annotation form which constrains from input data
     */
    private Form getAnnotationForm() {
        Rect dLRect = getProperties().getBoundingRect();
        String annotationText = getProperties().getContents();
        Content content = (getProperties().getNormalAppereance() != null) ? getProperties().getNormalAppereance().getContent() : new Content();
        GraphicState gs = new GraphicState();
        Path path = new Path();
        EnumSet<PathPaintOpFlags> pathPaintOpFlag;

        if (gs.getStrokeColor() != null) {
            gs.setStrokeColor(Utils.transform(getProperties().getForeColor()));
            pathPaintOpFlag = path.getPaintOp();
            pathPaintOpFlag.add(PathPaintOpFlags.STROKE);
            path.setPaintOp(pathPaintOpFlag);
        }
        if (getProperties().hasFill()) {
            gs.setFillColor(Utils.transform(getProperties().getInteriorColor()));
            pathPaintOpFlag = path.getPaintOp();
            pathPaintOpFlag.add(PathPaintOpFlags.FILL);
            path.setPaintOp(pathPaintOpFlag);
        }

        // get the properties for the text
        GraphicState gsTextRun = new GraphicState();
        gsTextRun.setFillColor(Utils.transform(getProperties().getForeColor()));
        TextState textState = new TextState();
        textState.setFontSize(getProperties().getFontSize());

        com.datalogics.PDFL.Matrix matrix = new com.datalogics.PDFL.Matrix();
        matrix.setH(dLRect.getLeft());
        matrix.setV(dLRect.getTop() - (textState.getFontSize() * 0.85)); // not the right formula but "close enough"

        double textWidth = dLRect.getRight() - dLRect.getLeft();

        // get the font
        com.datalogics.PDFL.Font dLFont = initFont();
        TextRun textRun = null;
        Text text = new Text();

        int i = 0;

        double maxTextHeight = 0;
        // grab the text from the Rich Textbox and add it to the text that will
        // be used
        // to create the FreeText Annotation
        if (annotationText != null) {
            String[] chars = annotationText.split("\n");
            while (i < chars.length) {
                if (chars.length > 0) {
                    if (getProperties().getQuadding() == HorizontalAlignment.LEFT)
                        matrix.setH(matrix.getH() + 2.0 * (gs.getWidth()));
                    try {
                        textRun = new TextRun(chars[i], dLFont, gsTextRun, textState, matrix);
                    } catch (Exception MissingCMapException) {
                        if (MissingCMapException.getMessage().contains("537067605")) {
                            Font tempFont = new com.datalogics.PDFL.Font("Helvetica");
                            textRun = new TextRun(chars[i], tempFont, gsTextRun, textState, matrix);
                        }
                    }

                    double advance = textRun.getAdvance();
                    double align = textWidth - advance;

                    maxTextHeight += textRun.getBoundingBox().getHeight();
                    if (getProperties().getQuadding() == HorizontalAlignment.RIGHT) {
                        matrix.setH(matrix.getH() + align);
                        textRun = new TextRun(chars[i], dLFont, gsTextRun, textState, matrix);
                    } else if (getProperties().getQuadding() == HorizontalAlignment.CENTER) {
                        matrix.setH(matrix.getH() + (align / 2.0));
                        textRun = new TextRun(chars[i], dLFont, gsTextRun, textState, matrix);
                    }

                    matrix.setH(dLRect.getLeft());
                    text.addRun(textRun);
                }
                matrix.setV(matrix.getV() - 1.2 * textState.getFontSize());
                i++;
            }
        }

        // create content to save the new annotation to
        content = new Content();
        path.setGraphicState(gs);
        // adjust the rectangle so it does not clip the text
        double width = dLRect.getRight() - dLRect.getLeft();
        double height = dLRect.getTop() - dLRect.getBottom();

        path.addRect(new Point(dLRect.getLeft(), dLRect.getBottom()), width, height);
        content.addElement(path);
        content.addElement(text);

        return new Form(content); // create a new form and return it
    }

    /**
     * Method is used to update FreeTextAnnotation's appearance. It sets font
     * face, font size, font alignment and foreground and background colors of
     * annotation.
     * 
     * @param content - the Datalogics Content is from annotation
     * NormalAppearance.
     */
    private void setProperties(Content content) {
        boolean useDefaultFont = false;
        Path path = null;
        TextRun currentTextRun = null;
        GraphicState gState = null;
        Font dlFont = null;
        for (int i = 0; i < content.getNumElements(); i++) {
            Element element = content.getElement(i);
            if (element instanceof Text) {
                Text currentText = (Text) element;
                for (int j = 0; j < currentText.getNumberOfRuns(); j++) {
                    currentTextRun = currentText.getRun(j);
                    dlFont = currentTextRun.getFont();
                    if (dlFont != null)
                        break;
                }
            } else if (element instanceof Path) {
                path = (Path) element;
                gState = path.getGraphicState();
            }
        }

        if (dlFont == null) {
            dlFont = initFont();
            useDefaultFont = true;
        }

        if (gState == null)
            gState = new GraphicState();

        if (currentTextRun == null)
            currentTextRun = new TextRun("", dlFont, gState, new TextState(), new com.datalogics.PDFL.Matrix());

        if (path == null)
            path = new Path(gState);

        String fontName = "";
        for (String name : AnnotationFactory.getAvailableFonts()) {
            String trimmedName = name.replace(" ", "");
            if (name.indexOf(dlFont.getName()) != -1 || trimmedName.indexOf(dlFont.getName()) != -1 || dlFont.getName().indexOf(name) != -1 || dlFont.getName().indexOf(trimmedName) != -1) {
                fontName = name;
                break;
            }
        }

        if (!useDefaultFont) {
            // if we had no content, we'll probably lose text
            // run with custom font parameters, so, use ones
            // that left in the object from previous operations
            getProperties().setFontFace(fontName.length() != 0 ? fontName : "Arial");
            getProperties().setFontSize((int) currentTextRun.getTextState().getFontSize());
        }

        if (gState.getStrokeColor() != null)
            getProperties().setForeColor(Utils.transform(gState.getStrokeColor()));
        if (gState.getFillColor() != null && path.getPaintOp().contains(PathPaintOpFlags.FILL)) /*& PathPaintOpFlags.FILL)*/
            getProperties().setInteriorColor(Utils.transform(gState.getFillColor()));
        else
            getProperties().resetFill();
    }

    private Font initFont() {
        Font dlFont = null;
        try {
            dlFont = new Font(getProperties().getFontFace());
        } catch (RuntimeException e) {
            getProperties().setFontFace(Font.getFontList().get(0).getName());
            dlFont = new Font(getProperties().getFontFace());
        }
        return dlFont;
    }
}
