/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Annotations;

import java.lang.reflect.InvocationTargetException;
import java.util.List;

import com.datalogics.PDFL.Annotation;
import com.datalogics.PDFL.CircleAnnotation;
import com.datalogics.PDFL.FreeTextAnnotation;
import com.datalogics.PDFL.HighlightAnnotation;
import com.datalogics.PDFL.InkAnnotation;
import com.datalogics.PDFL.LineAnnotation;
import com.datalogics.PDFL.LineEndingStyle;
import com.datalogics.PDFL.LinkAnnotation;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.Point;
import com.datalogics.PDFL.PolyLineAnnotation;
import com.datalogics.PDFL.PolygonAnnotation;
import com.datalogics.PDFL.Quad;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.SquareAnnotation;
import com.datalogics.PDFL.UnderlineAnnotation;

/**
 * AnnotationFactory - the class which allows to create an annotation and a
 * wrapper (AnnontationHolder) for it.
 */
public class AnnotationFactory {
    public static AnnotationHolder createAnnotation(String subType, AnnotationProperties properties, Page page, int addAfter) {
        AnnotationHolder holder = null;
        Annotation annotation = null;
        final boolean isLastAnnotation = addAfter == -2;
        final int pageNum = page.getPageNumber();

        final Rect initRect = properties.getBoundingRect();
        final List<Quad> initQuads = properties.getQuads();
        final Point startPoint = properties.getStartPoint();
        final Point endPoint = properties.getEndPoint();
        final List<Point> initVertices = properties.getVertices();

        if (initVertices.isEmpty()) {
            initVertices.add(startPoint);
            initVertices.add(startPoint);
        }
        if (initQuads.isEmpty())
            initQuads.add(new Quad());

        if (subType.equals(AnnotationConsts.Subtypes.CIRCLE)) {
            annotation = new CircleAnnotation(page, initRect, addAfter);
        } else if (subType.equals(AnnotationConsts.Subtypes.SQUARE)) {
            annotation = new SquareAnnotation(page, initRect, addAfter);
        } else if (subType.equals(AnnotationConsts.Subtypes.LINE)) {
            properties.setEndEndingStyle(LineEndingStyle.NONE);
            properties.setStartEndingStyle(LineEndingStyle.NONE);
            annotation = new LineAnnotation(page, initRect, startPoint, endPoint, addAfter);
        } else if (subType.equals(AnnotationConsts.Subtypes.ARROW)) {
            annotation = new LineAnnotation(page, initRect, startPoint, endPoint, addAfter);
        } else if (subType.equals(AnnotationConsts.Subtypes.POLYLINE)) {
            annotation = new PolyLineAnnotation(page, initRect, initVertices, addAfter);
            properties.setVertices(initVertices);
        } else if (subType.equals(AnnotationConsts.Subtypes.POLYGON)) {
            annotation = new PolygonAnnotation(page, initRect, initVertices, addAfter);
            properties.setVertices(initVertices);
        } else if (subType.equals(AnnotationConsts.Subtypes.FREETEXT)) {
            annotation = new FreeTextAnnotation(page, initRect, "", addAfter);
        } else if (subType.equals(AnnotationConsts.Subtypes.LINK)) {
            annotation = new LinkAnnotation(page, initRect, addAfter);
        } else if (subType.equals(AnnotationConsts.Subtypes.HIGHLIGHT)) {
            annotation = new HighlightAnnotation(page, addAfter, initQuads);
            properties.setQuads(initQuads);
        } else if (subType.equals(AnnotationConsts.Subtypes.UNDERLINE)) {
            annotation = new UnderlineAnnotation(page, addAfter, initQuads);
            properties.setQuads(initQuads);
        } else if (subType.equals(AnnotationConsts.Subtypes.INK)) {
            annotation = new InkAnnotation(page, initRect, addAfter);
        }

        holder = createAnnotationHolder(annotation, isLastAnnotation ? page.getNumAnnotations() - 1 : addAfter + 1, pageNum);
        if (holder instanceof GroupAnnotationHolder) {
            ((GroupAnnotationHolder) holder).getProperties().setFrom(properties);

        } else {
            properties.setDirty(true);
            holder.getProperties().updateFrom(properties);
            properties.setDirty(false);
        }
        return holder;
    }

    public static AnnotationHolder createAnnotationHolder(Annotation annotation, int index, int pageNum) {
        final String subType = annotation != null ? annotation.getSubtype() : AnnotationConsts.Subtypes.GROUP;
        Class<? extends AnnotationHolder> holderClass = AnnotationHolder.class;
        AnnotationHolder holder = null;
        if (subType.equals(AnnotationConsts.Subtypes.CIRCLE) || subType.equals(AnnotationConsts.Subtypes.SQUARE)) {
            holderClass = RectangularAnnotationHolder.class;
        } else if (subType.equals(AnnotationConsts.Subtypes.LINE)) {
            holderClass = LineAnnotationHolder.class;
        } else if (subType.equals(AnnotationConsts.Subtypes.POLYLINE)) {
            holderClass = PolylineAnnotationHolder.class;
        } else if (subType.equals(AnnotationConsts.Subtypes.POLYGON)) {
            holderClass = PolygonAnnotationHolder.class;
        } else if (subType.equals(AnnotationConsts.Subtypes.FREETEXT)) {
            holderClass = FreeTextAnnotationHolder.class;
        } else if (subType.equals(AnnotationConsts.Subtypes.LINK)) {
            holderClass = LinkAnnotationHolder.class;
        } else if (subType.equals(AnnotationConsts.Subtypes.HIGHLIGHT) || subType.equals(AnnotationConsts.Subtypes.UNDERLINE)) {
            holderClass = TextMarkupAnnotationHolder.class;
        } else if (subType.equals(AnnotationConsts.Subtypes.INK)) {
            holderClass = InkAnnotationHolder.class;
        } else if (subType.equals(AnnotationConsts.Subtypes.GROUP)) {
            holderClass = GroupAnnotationHolder.class;
        } else {
            holderClass = BaseAnnotationHolder.class;
        }

        try {
            holder = holderClass.getConstructor(new Class[] {}).newInstance();
            if (holder instanceof GroupAnnotationHolder) {
                ((GroupAnnotationHolder) holder).init(pageNum);
            } else
                ((BaseAnnotationHolder) holder).init(annotation, index, pageNum);
        } catch (IllegalArgumentException e) {
            e.printStackTrace();
        } catch (SecurityException e) {
            e.printStackTrace();
        } catch (InstantiationException e) {
            e.printStackTrace();
        } catch (IllegalAccessException e) {
            e.printStackTrace();
        } catch (InvocationTargetException e) {
            e.printStackTrace();
        } catch (NoSuchMethodException e) {
            e.printStackTrace();
        }

        return holder;
    }

    public static void deleteAnnotation(Page page, int annotationIndex) {
        if (page.getAnnotation(annotationIndex) != null)
            page.removeAnnotation(annotationIndex);
    }

    public static void setAvailableFonts(List<String> availableFonts) {
        AnnotationFactory.availableFonts = availableFonts;
    }

    public static List<String> getAvailableFonts() {
        return availableFonts;
    }

    private static List<String> availableFonts;
}
