/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Annotations;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;

import com.datalogics.PDFL.Action;
import com.datalogics.PDFL.Color;
import com.datalogics.PDFL.Content;
import com.datalogics.PDFL.Form;
import com.datalogics.PDFL.HorizontalAlignment;
import com.datalogics.PDFL.LineEndingStyle;
import com.datalogics.PDFL.Point;
import com.datalogics.PDFL.Quad;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.JavaViewer.Utils;

/**
 * AnnotationProperties - the class which stores serialized form of annotation's
 * properties.
 * 
 * It contains getters and setters to change annotation's properties
 * programmatically. It has couple of service methods (such as updateFrom() and
 * setFrom()) for updating annotation's properties from another
 * AnnotationProperties class. This class sends notifications when annotation's
 * properties has changed.
 */
public class AnnotationProperties {
    @Override
    public boolean equals(Object obj) {
        if (this == obj)
            return true;
        if (obj == null)
            return false;
        if (this.getClass() != obj.getClass())
            return false;
        final AnnotationProperties other = (AnnotationProperties) obj;
        boolean sameProps = true;
        for (String property : this.getNativeProps()) {
            sameProps = other.getNativeProps().contains(property) && this.getNativePropsMap().get(property).equals(other.getNativePropsMap().get(property));

            if (!sameProps)
                break;
        }
        return sameProps;
    }

    @Override
    public int hashCode() {
        return getNativePropsMap().hashCode();
    }

    public AnnotationProperties() {
        this.listeners = new ArrayList<AnnotationListener>();
        this.properties = new HashMap<String, Property>();
    }

    public void addAnnotationListener(AnnotationListener listener) {
        listeners.add(listener);
    }

    public void removeAnnotationListener(AnnotationListener listener) {
        listeners.remove(listener);
    }

    public String getSubtype() {
        return getProperty(AnnotationConsts.SUBTYPE, "", String.class);
    }

    public boolean getIsSuported() {
        return getProperty(AnnotationConsts.IS_ANNOT_SUPPORT, true, boolean.class);
    }

    public void setSuported(boolean isSupported) {
        if (!updateAlways && (getIsSuported() == isSupported))
            return;
        updateProperty(AnnotationConsts.IS_ANNOT_SUPPORT, isSupported, boolean.class);
    }

    public Rect getBoundingRect() {
        // Because we create defValue every time we need to delete it if it's
        // not used we check if curValue not equals defValue - delete defValue;

        Rect retValue = null;
        Rect defValue = null;

        try {
            defValue = new Rect();
            final Rect curValue = getProperty(AnnotationConsts.BOUNDING_RECT, defValue, Rect.class);
            if (curValue == defValue) {
                defValue = null;
            }
            retValue = Utils.clone(curValue);
        } finally {
            if (defValue != null)
                defValue.delete();
        }
        return retValue;
    }

    public void setBoundingRect(Rect rect) {
        if (rect == null || (!updateAlways && getBoundingRect().equals(rect)))
            return;
        updateProperty(AnnotationConsts.BOUNDING_RECT, Utils.clone(rect), Rect.class);
    }

    public String getTitle() {
        return getProperty(AnnotationConsts.TITLE, "", String.class);
    }

    public void setTitle(String title) {
        if (title == null || (!updateAlways && title.equals(getTitle())))
            return;
        updateProperty(AnnotationConsts.TITLE, title, String.class);
    }

    public String getContents() {
        return getProperty(AnnotationConsts.CONTENTS, "", String.class);
    }

    public void setContents(String contents) {
        if (contents == null || (!updateAlways && contents.equals(getContents())))
            return;
        updateProperty(AnnotationConsts.CONTENTS, contents, String.class);
    }

    public float getLineWidth() {
        return getProperty(AnnotationConsts.WIDTH, 1.0, double.class).floatValue();
    }

    public void setLineWidth(float width) {
        if (!updateAlways && width == getLineWidth())
            return;
        updateProperty(AnnotationConsts.WIDTH, (double) width, double.class);
    }

    public java.awt.Color getForeColor() {
        java.awt.Color retValue = null;
        Color defValue = null;

        try {
            defValue = new Color(0, 0, 0);
            final Color curValue = getProperty(AnnotationConsts.COLOR, defValue, Color.class);
            if (curValue == defValue) {
                defValue = null;
            }
            retValue = Utils.transform(curValue);
        } finally {
            if (defValue != null)
                defValue.delete();
        }
        return retValue;
    }

    public void setForeColor(java.awt.Color color) {
        final java.awt.Color oldColor = getForeColor();
        if ((color == null && oldColor == null) || (!updateAlways && color != null && color.equals(oldColor)))
            return;
        updateProperty(AnnotationConsts.COLOR, Utils.transform(color), Color.class);
    }

    public java.awt.Color getInteriorColor() {
        java.awt.Color retValue = null;
        Color defValue = null;

        try {
            defValue = new Color(0, 0, 0);
            final Color curValue = getProperty(AnnotationConsts.INTERIOR_COLOR, defValue, Color.class);
            if (curValue == defValue) {
                defValue = null;
            }
            retValue = Utils.transform(curValue);
        } finally {
            if (defValue != null)
                defValue.delete();
        }
        return retValue;
    }

    public void setInteriorColor(java.awt.Color color) {
        final java.awt.Color oldColor = getInteriorColor();
        if ((color == null && oldColor == null) || (!updateAlways && color != null && color.equals(oldColor)))
            return;
        updateProperty(AnnotationConsts.INTERIOR_COLOR, Utils.transform(color), Color.class);
    }

    public java.awt.Color getTextColor() {
        java.awt.Color retValue = null;
        Color defValue = null;

        try {
            defValue = new Color(0, 0, 0);
            final Color curValue = getProperty(AnnotationConsts.TEXT_COLOR, defValue, Color.class);
            if (curValue == defValue) {
                defValue = null;
            }
            retValue = Utils.transform(curValue);
        } finally {
            if (defValue != null)
                defValue.delete();
        }
        return retValue;
    }

    public void setTextColor(java.awt.Color color) {
        final java.awt.Color oldColor = getTextColor();
        if ((color == null && oldColor == null) || (!updateAlways && color != null && color.equals(oldColor)))
            return;
        updateProperty(AnnotationConsts.TEXT_COLOR, Utils.transform(color), Color.class);
    }

    @SuppressWarnings("unchecked")
    public List<Point> getVertices() {
        final List<Point> vertices = getProperty(AnnotationConsts.VERTICES, new ArrayList<Point>(), List.class /*List<Point>*/);

        List<Point> copy = new ArrayList<Point>(vertices.size());
        for (Point vertex : vertices) {
            copy.add(Utils.clone(vertex));
        }
        return copy;
    }

    public void setVertices(List<Point> vertices) {
        final List<Point> oldVertices = getVertices();
        if (!updateAlways && oldVertices.equals(vertices))
            return;

        List<Point> newVertices = new ArrayList<Point>(vertices.size());
        for (int i = 0; i < vertices.size(); ++i) {
            newVertices.add(Utils.clone(vertices.get(i)));
        }
        updateProperty(AnnotationConsts.VERTICES, newVertices, List.class /*List<Point>*/);
    }

    @SuppressWarnings("unchecked")
    public List<List<Point>> getScribbles() {
        final List<List<Point>> scribble = getProperty(AnnotationConsts.SCRIBBLES, new ArrayList<Point>(), List.class /*List<List<Point>>*/);

        List<List<Point>> copy = new ArrayList<List<Point>>(scribble.size());
        for (List<Point> subList : scribble) {
            List<Point> tempList = new ArrayList<Point>();
            for (Point vertex : subList) {
                tempList.add(Utils.clone(vertex));
            }
            copy.add(tempList);
        }

        return copy;
    }

    public void setScribbles(List<List<Point>> scribble) {
        final List<List<Point>> oldScribbles = getScribbles();
        if (!updateAlways && oldScribbles.equals(scribble))
            return;

        List<List<Point>> newScribbles = new ArrayList<List<Point>>(scribble.size());
        for (int i = 0; i < scribble.size(); ++i) {
            List<Point> tempList = new ArrayList<Point>();
            for (int j = 0; j < scribble.get(i).size(); j++) {
                tempList.add(Utils.clone(scribble.get(i).get(j)));
            }
            newScribbles.add(tempList);
        }
        updateProperty(AnnotationConsts.SCRIBBLES, newScribbles, List.class /*List<List<Point>>*/);
    }

    public Point getStartPoint() {
        Point retValue = null;
        Point defValue = null;

        try {
            defValue = new Point();
            final Point curValue = getProperty(AnnotationConsts.START_POINT, defValue, Point.class);
            if (curValue == defValue) {
                defValue = null;
            }
            retValue = Utils.clone(curValue);
        } finally {
            if (defValue != null)
                defValue.delete();
        }
        return retValue;
    }

    public void setStartPoint(Point startPoint) {
        if (startPoint == null || (!updateAlways && startPoint.equals(getStartPoint())))
            return;
        updateProperty(AnnotationConsts.START_POINT, Utils.clone(startPoint), Point.class);
    }

    public Point getEndPoint() {
        Point retValue = null;
        Point defValue = null;

        try {
            defValue = new Point();
            final Point curValue = getProperty(AnnotationConsts.END_POINT, defValue, Point.class);
            if (curValue == defValue) {
                defValue = null;
            }
            retValue = Utils.clone(curValue);
        } finally {
            if (defValue != null)
                defValue.delete();
        }
        return retValue;
    }

    public void setEndPoint(Point endPoint) {
        if (endPoint == null || (!updateAlways && endPoint.equals(getEndPoint())))
            return;
        updateProperty(AnnotationConsts.END_POINT, Utils.clone(endPoint), Point.class);
    }

    public Form getNormalAppereance() {
        return getProperty(AnnotationConsts.NORM_APPEARANCE, new Form(new Content()), Form.class);
    }

    public String getFontFace() {
        return getProperty(AnnotationConsts.FONT_FACE, "Arial", String.class);
    }

    public void setFontFace(String fontFace) {
        if (fontFace == null || (!updateAlways && fontFace.equals(getFontFace())))
            return;
        updateProperty(AnnotationConsts.FONT_FACE, fontFace, String.class);
    }

    public float getFontSize() {
        return getProperty(AnnotationConsts.FONT_SIZE, 12.0, double.class).floatValue();
    }

    public void setFontSize(float fontSize) {
        if (!updateAlways && fontSize == getFontSize())
            return;
        updateProperty(AnnotationConsts.FONT_SIZE, (double) fontSize, double.class);
    }

    public HorizontalAlignment getQuadding() {
        return getProperty(AnnotationConsts.QUADDING, HorizontalAlignment.LEFT, HorizontalAlignment.class);
    }

    public void setQuadding(HorizontalAlignment quadding) {
        if (quadding == null || (!updateAlways && quadding.equals(getQuadding())))
            return;
        updateProperty(AnnotationConsts.QUADDING, quadding, HorizontalAlignment.class);
    }

    @SuppressWarnings("unchecked")
    public List<Quad> getQuads() {
        final List<Quad> quads = getProperty(AnnotationConsts.QUADS, new ArrayList<Quad>(), List.class /*List<Quad>*/);

        List<Quad> copy = new ArrayList<Quad>(quads.size());
        for (Quad quad : quads) {
            copy.add(Utils.clone(quad));
        }
        return copy;
    }

    public void setQuads(List<Quad> quads) {
        final List<Quad> oldQuads = getQuads();
        if (!updateAlways && oldQuads.equals(quads))
            return;

        List<Quad> newVertices = new ArrayList<Quad>(quads.size());
        for (int i = 0; i < quads.size(); ++i) {
            newVertices.add(Utils.clone(quads.get(i)));
        }
        updateProperty(AnnotationConsts.QUADS, newVertices, List.class /*List<Quad>*/);
    }

    public void setAction(ActionProperties actionProperties) {
        final ActionProperties oldActionProperties = getAction();
        if (actionProperties.equals(oldActionProperties))
            return;

        updateProperty(AnnotationConsts.ACTION, actionProperties, ActionProperties.class);
    }

    public ActionProperties getAction() {
        return getProperty(AnnotationConsts.ACTION, new ActionProperties(), ActionProperties.class);
    }

    public void setActionSubtype(String actionSubType) {
        final String oldActionSubtype = getActionSubtype();
        if (actionSubType.equals(oldActionSubtype))
            return;

        updateProperty(AnnotationConsts.ACTION_SUBTYPE, actionSubType, String.class);
    }

    public String getActionSubtype() {
        return getProperty(AnnotationConsts.ACTION_SUBTYPE, new String(), String.class);
    }

    public boolean hasFill() {
        return getInteriorColor() != null;
    }

    public void resetFill() {
        setInteriorColor(null);
    }

    public String getStyle() {
        return getProperty(AnnotationConsts.STYLE, "S", String.class);
    }

    public void setStyle(String style) {
        if ((!updateAlways && style.equals(getStyle())) || (!style.equals("S") && !style.equals("D") && !style.equals("B") && !style.equals("I") && !style.equals("U")))
            return;
        updateProperty(AnnotationConsts.STYLE, style, String.class);
    }

    public boolean isSolid() {
        return getStyle().equals("S");
    }

    public void makeSolid() {
        setStyle("S");
    }

    public LineEndingStyle getStartEndingStyle() {
        return getProperty(AnnotationConsts.START_ENDING_STYLE, LineEndingStyle.NONE, LineEndingStyle.class);
    }

    public void setStartEndingStyle(LineEndingStyle lineEndingStyle) {
        if (!updateAlways && lineEndingStyle.equals(getStartEndingStyle()))
            return;
        updateProperty(AnnotationConsts.START_ENDING_STYLE, lineEndingStyle, LineEndingStyle.class);
    }

    public LineEndingStyle getEndEndingStyle() {
        return getProperty(AnnotationConsts.END_ENDING_STYLE, LineEndingStyle.NONE, LineEndingStyle.class);
    }

    public void setEndEndingStyle(LineEndingStyle lineEndingStyle) {
        if (!updateAlways && lineEndingStyle.equals(getEndEndingStyle()))
            return;
        updateProperty(AnnotationConsts.END_ENDING_STYLE, lineEndingStyle, LineEndingStyle.class);
    }

    public float[] getDashPattern() {
        @SuppressWarnings("unchecked")
        final List<Double> dashPattern = getProperty(AnnotationConsts.DASH_PATTERN, new ArrayList<Double>(), List.class /*List<Double>*/);
        float[] pattern = null;

        if (dashPattern != null && !dashPattern.isEmpty()) {
            pattern = new float[dashPattern.size()];
            for (int i = 0, len = dashPattern.size(); i < len; ++i) {
                pattern[i] = dashPattern.get(i).floatValue();
            }
        }
        return pattern;
    }

    public void setDashPattern(float[] pattern) {
        if (!updateAlways && Arrays.equals(pattern, getDashPattern()))
            return;
        List<Double> dashPattern = new ArrayList<Double>();
        if (pattern != null) {
            for (float dash : pattern) {
                dashPattern.add((double) dash);
            }
        }
        setProperty(AnnotationConsts.STYLE, dashPattern.isEmpty() ? "S" : "D", String.class, properties.get(AnnotationConsts.STYLE).getChangeData());
        updateProperty(AnnotationConsts.DASH_PATTERN, dashPattern, List.class /*List<Double>*/);
    }

    public boolean getShowBorder() {
        return getProperty(AnnotationConsts.SHOW_BORDER, false, boolean.class);
    }

    public void setShowBorder(boolean border) {
        if (!updateAlways && (border == getShowBorder()))
            return;
        if (border && getLineWidth() == 0) {
            setLineWidth(getSavedWidth());
        } else if (!border && getLineWidth() != 0) {
            setSavedLineWidth(getLineWidth());
            setLineWidth(0);
        }
        updateProperty(AnnotationConsts.SHOW_BORDER, border, boolean.class);
    }

    public void setSavedLineWidth(float width) {
        updateProperty(AnnotationConsts.SAVED_WIDTH, width, float.class);
    }

    public float getSavedWidth() {
        return getProperty(AnnotationConsts.SAVED_WIDTH, 1.0f, float.class);
    }

    public boolean getAutoSize() {
        return getProperty(AnnotationConsts.IS_AUTO_SIZE, false, boolean.class);
    }

    public void setAutoSize(boolean autosize) {
        if (!updateAlways && (autosize == getAutoSize()))
            return;
        updateProperty(AnnotationConsts.IS_AUTO_SIZE, autosize, boolean.class);
    }

    public boolean getDirty() {
        boolean dirty = false;

        for (Property prop : properties.values()) {
            dirty = dirty || prop.getDirty();
            if (dirty)
                break;
        }
        return dirty;
    }

    public void setDirty(boolean dirty) {
        String[] keys = new String[properties.keySet().size()];
        properties.keySet().toArray(keys);

        for (int i = 0; i < keys.length; ++i) {
            final String property = keys[i];
            properties.put(property, new Property(properties.get(property).getValue(), properties.get(property).getPropClass(), properties.get(property).getNative(), dirty, properties.get(property).getChangeData()));
        }
    }

    public void setUpdateAlways(boolean updateAlways) {
        this.updateAlways = updateAlways;
    }

    public void raiseUpdates(Object changeData) {
        onUpdate(changeData);
    }

    public void setFrom(AnnotationProperties other) {
        copyFrom(other, true);
        setDirty(false);
    }

    public void updateFrom(AnnotationProperties other) {
        copyFrom(other, false);
    }

    void setSubtype(String subType) {
        if (subType.equals(getSubtype()))
            return;
        setProperty(AnnotationConsts.SUBTYPE, subType, String.class, false);
    }

    Set<String> getNativeProps() {
        return new HashSet<String>(getNativePropsMap().keySet());
    }

    Object getValue(String property) {
        return properties.containsKey(property) ? properties.get(property).getValue() : null;
    }

    Class<?> getPropClass(String property) {
        return properties.containsKey(property) ? properties.get(property).getPropClass() : null;
    }

    void setProperty(String property, Object value, Class<?> propClass, Object change) {
        updateProperty(property, value, propClass, change, true, false);
    }

    void setNotNativeProperty(String property, Object value, Class<?> propClass, Object change) {
        updateProperty(property, value, propClass, change, false, false);
    }

    private Map<String, Property> getNativePropsMap() {
        Map<String, Property> nativeProps = new HashMap<String, Property>();
        for (String property : properties.keySet()) {
            if (properties.get(property).getNative())
                nativeProps.put(property, properties.get(property));
        }
        return nativeProps;
    }

    private Set<String> getAvailable() {
        return new HashSet<String>(properties.keySet());
    }

    private void copyFrom(AnnotationProperties other, boolean copyAll) {
        for (String property : other.getAvailable()) {
            if (copyAll || other.properties.get(property).getDirty()) {
                // if our properties have native subtype, then skip copying it
                // from other properties
                if (property.equals(AnnotationConsts.SUBTYPE) && properties.containsKey(property) && properties.get(property).getNative())
                    continue;

                // save 'nativity' trait of our properties if available,
                // otherwise take from other properties if making full copy
                final boolean nativeProp = this.properties.containsKey(property) ? this.properties.get(property).getNative() : (copyAll ? other.properties.get(property).getNative() : false);
                final Object changeData = this.properties.containsKey(property) ? this.properties.get(property).getChangeData() : (copyAll ? other.properties.get(property).getChangeData() : null);
                final Class<?> propClass = other.properties.get(property).getPropClass();
                updateProperty(property, other.getProperty(property, null, propClass), propClass, changeData, nativeProp, false);
            }
        }
        if (copyAll) { // remove all local properties that are not in the source
                       // properties
            for (String property : this.getAvailable()) {
                if (!other.properties.containsKey(property))
                    this.properties.remove(property);
            }
        }
    }

    private void updateProperty(String property, Object value, Class<?> propClass) {
        final boolean hasProp = properties.containsKey(property);
        updateProperty(property, value, propClass, hasProp ? properties.get(property).getChangeData() : null, hasProp ? properties.get(property).getNative() : false, true);
    }

    private void updateProperty(String property, Object value, Class<?> propClass, Object change, boolean isNative, boolean sendUpdate) {
        properties.remove(property);
        properties.put(property, new Property(value, propClass, isNative, true, change));

        if (sendUpdate)
            onUpdate(change);
    }

    @SuppressWarnings("unchecked")
    private <T> T getProperty(String property, T defVal, Class<T> propClass) {
        if (!properties.containsKey(property))
            properties.put(property, new Property(defVal, propClass, false, true, null));

        return (T) properties.get(property).getValue();
    }

    private void onUpdate(Object changeData) {
        for (AnnotationListener listener : getAnnotationListeners())
            listener.update(changeData);
    }

    private AnnotationListener[] getAnnotationListeners() {
        return listeners.toArray(new AnnotationListener[0]);
    }

    private static class Property {
        Property(Object value, Class<?> propClass, boolean isNative, boolean dirty, Object change) {
            this.value = value;
            this.propClass = propClass;
            this.isNative = isNative;
            this.dirty = dirty;
            this.changeData = change;
        }

        Object getValue() {
            return value;
        }

        Class<?> getPropClass() {
            return propClass;
        }

        boolean getDirty() {
            return dirty;
        }

        boolean getNative() {
            return isNative;
        }

        Object getChangeData() {
            return changeData;
        }

        @Override
        public int hashCode() {
            // auto-generated hashCode() method
            final int prime = 31;
            int result = 1;
            result = prime * result + ((changeData == null) ? 0 : changeData.hashCode());
            result = prime * result + (dirty ? 1231 : 1237);
            result = prime * result + ((propClass == null) ? 0 : propClass.hashCode());
            result = prime * result + ((value == null) ? 0 : value.hashCode());
            return result;
        }

        @Override
        public boolean equals(Object obj) {
            // auto-generated equals() method
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (getClass() != obj.getClass())
                return false;

            Property other = (Property) obj;
            if (isNative == other.isNative && isNative == false) // if current properties isn't native than it's same properties
                return true;
            if (changeData == null) {
                if (other.changeData != null)
                    return false;
            } else if (!changeData.equals(other.changeData))
                return false;
            if (dirty != other.dirty)
                return false;
            if (propClass == null) {
                if (other.propClass != null)
                    return false;
            } else if (!propClass.equals(other.propClass))
                return false;
            if (value == null) {
                if (other.value != null)
                    return false;
            } else if (!value.equals(other.value))
                return false;
            return true;
        }

        private Object value;
        private Class<?> propClass;
        private boolean dirty;
        private boolean isNative;
        private Object changeData;
    }

    private final List<AnnotationListener> listeners;
    private final Map<String, Property> properties;
    private boolean updateAlways;
}
