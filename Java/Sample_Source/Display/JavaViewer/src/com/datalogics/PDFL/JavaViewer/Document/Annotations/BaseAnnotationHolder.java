/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Annotations;

import java.lang.reflect.Method;
import java.util.Arrays;
import java.util.EnumSet;
import java.util.List;

import com.datalogics.PDFL.Annotation;
import com.datalogics.PDFL.AnnotationFlags;
import com.datalogics.PDFL.Color;
import com.datalogics.PDFL.Form;
import com.datalogics.PDFL.LineEndingStyle;
import com.datalogics.PDFL.Rect;

/**
 * BaseAnnotationHolder - it is a base class for all holder types. It implements
 * AnnotationHolder interface.
 * 
 * This class contains service methods for serializing annotation's properties
 * from and to PDFL.Annotation class. It fills AnnotationProperties object with
 * actual properties of the annotation.
 * 
 * It allows polymorphically update annotation's appearance by request. This
 * class helps show/hide the annotation through capture()/release() calls.
 */
public class BaseAnnotationHolder implements AnnotationHolder {
    public int getIndex() {
        return this.index;
    }

    public void setIndex(int index) {
        this.index = index;
    }

    public int getPageIndex() {
        return this.pageIndex;
    }

    public AnnotationProperties getProperties() {
        return this.properties;
    }

    public void addAnnotationListener(AnnotationListener listener) {
        listenerBroadcast.addAnnotationListener(listener);
    }

    public void removeAnnotationListener(AnnotationListener listener) {
        listenerBroadcast.removeAnnotationListener(listener);
    }

    public void updateAppearance() {
        if (properties.getDirty()) {
            writeProperties();

            final Form normalAppearance = generateAppearance();
            writePropertyByName(AnnotationConsts.NORM_APPEARANCE, Form.class, normalAppearance);

            readProperties();
        }
    }

    public void capture() {
        properties.addAnnotationListener(listenerBroadcast);

        EnumSet<AnnotationFlags> flags = annotation.getFlags();
        if ((!flags.contains(AnnotationFlags.HIDDEN))) {
            flags.add(AnnotationFlags.HIDDEN);
            annotation.setFlags(flags);
            listenerBroadcast.onUpdate(AppearanceUpdate.OBJECT_HIDDEN);
        }
    }

    public void release() {
        updateAppearance();

        EnumSet<AnnotationFlags> flags = annotation.getFlags();
        flags.remove(AnnotationFlags.HIDDEN);

        annotation.setFlags(flags);
        listenerBroadcast.onUpdate(AppearanceUpdate.OBJECT_SHOWN);

        properties.removeAnnotationListener(listenerBroadcast);
    }

    Annotation getAnnotation() {
        return annotation;
    }

    protected void init(Annotation annotation, int index, int pageIndex) {
        this.annotation = annotation;
        this.index = index;
        this.pageIndex = pageIndex;

        this.properties = new AnnotationProperties();
        this.listenerBroadcast = new AnnotationListenerBroadcaster();

        readProperties();
    }

    protected void doReadProperties() {
    }

    protected void doWriteProperties() {
    }

    protected void readPropertyByName(String property, Class<?> propClass, boolean shapeChanged) {
        try {
            Method prop = annotation.getClass().getMethod("get" + property, new Class[] {});
            properties.setProperty(property, prop.invoke(annotation, new Object[] {}), propClass, shapeChanged);
        } catch (Exception e) {
            System.out.println("wrong annotation property was read:" + property);
        }
    }

    protected Form generateAppearance() {
        return annotation.generateAppearance();
    }

    private void readProperties() {
        readPropertyByName(AnnotationConsts.SUBTYPE, String.class, false);
        readPropertyByName(AnnotationConsts.TITLE, String.class, false);
        readPropertyByName(AnnotationConsts.CONTENTS, String.class, false);
        readPropertyByName(AnnotationConsts.BOUNDING_RECT, Rect.class, true);
        readPropertyByName(AnnotationConsts.COLOR, Color.class, false);
        readPropertyByName(AnnotationConsts.NORM_APPEARANCE, Form.class, false);
        doReadProperties();

        setSupportedProperty(annotation);
        properties.setDirty(false);
    }

    private void setSupportedProperty(Annotation annotation) {
        boolean isSupp = (annotation.getAnnotationFeatureLevel() <= 1.5);

        // check the ending styles of the Line and PolyLine (in case the line is
        // an Arrow) for the endings that JavaViewer knows how to draw

        List<LineEndingStyle> lineStyles = Arrays.asList(properties.getStartEndingStyle(), properties.getEndEndingStyle());
        List<LineEndingStyle> supportedStyles = Arrays.asList(LineEndingStyle.NONE, LineEndingStyle.CIRCLE, LineEndingStyle.OPEN_ARROW, LineEndingStyle.CLOSED_ARROW);

        for (LineEndingStyle style : lineStyles) {
            isSupp = isSupp && (supportedStyles.indexOf(style) != -1);
            if (!isSupp)
                break;
        }

        properties.setSuported(isSupp);
    }

    private void writeProperties() {
        for (String property : properties.getNativeProps()) {
            writePropertyByName(property, properties.getPropClass(property), properties.getValue(property));
        }
        doWriteProperties();
    }

    private void writePropertyByName(String property, Class<?> propClass, Object value) {
        try {
            Method prop = annotation.getClass().getMethod("set" + property, new Class[] { propClass });
            prop.invoke(annotation, new Object[] { value });
        } catch (Exception e) {
            if (!property.equals(AnnotationConsts.SUBTYPE)) {
                System.out.println("wrong annotation property was wrote:" + property);
            }
        }
    }

    private Annotation annotation;
    private int index;
    private int pageIndex;

    private AnnotationProperties properties;
    private AnnotationListenerBroadcaster listenerBroadcast;
}
