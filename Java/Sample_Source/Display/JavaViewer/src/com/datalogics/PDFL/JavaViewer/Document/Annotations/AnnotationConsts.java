/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Annotations;

/**
 * AnnotationConst - class contains constants of annotation properties' names
 * and annotations' subtypes.
 * 
 * These constants are used for purposes of annotation properties'
 * serialization/deserialization. This class also contains a static method to
 * check whether some annotation is supported or not.
 */
public class AnnotationConsts {
    final static String SUBTYPE = "Subtype";
    final static String TITLE = "Title";
    final static String CONTENTS = "Contents";
    final static String WIDTH = "Width";
    final static String COLOR = "Color";
    final static String INTERIOR_COLOR = "InteriorColor";
    final static String TEXT_COLOR = "TextColor";
    final static String DASH_PATTERN = "DashPattern";
    final static String BOUNDING_RECT = "Rect";
    final static String STYLE = "Style";

    final static String START_POINT = "StartPoint";
    final static String END_POINT = "EndPoint";
    final static String VERTICES = "Vertices";
    final static String QUADS = "Quads";
    final static String NORM_APPEARANCE = "NormalAppearance";

    final static String START_ENDING_STYLE = "StartPointEndingStyle";
    final static String END_ENDING_STYLE = "EndPointEndingStyle";

    final static String FONT_FACE = "FontFace";
    final static String FONT_SIZE = "FontSize";
    final static String QUADDING = "Quadding";

    final static String SCRIBBLES = "Scribbles";

    final static String IS_ANNOT_SUPPORT = "IsAnnotationSupport";

    final static String IS_AUTO_SIZE = "IsAutoSize";
    final static String SAVED_WIDTH = "SavedWidth";
    final static String SHOW_BORDER = "ShowBorder";

    final static String ACTION = "Action";
    final static String ACTION_SUBTYPE = "ActionSubtype";

    public static class Subtypes {
        public final static String CIRCLE = "Circle";
        public final static String SQUARE = "Square";
        public final static String LINE = "Line";
        public final static String ARROW = "Arrow";
        public final static String POLYLINE = "PolyLine";
        public final static String POLYGON = "Polygon";
        public final static String FREETEXT = "FreeText";
        public final static String LINK = "Link";
        public final static String INK = "Ink";
        public final static String UNDERLINE = "Underline";
        public final static String HIGHLIGHT = "Highlight";
        public final static String GROUP = "Group";

        public static boolean isSupportedType(String str) {
            return str.equals(CIRCLE) || str.equals(SQUARE) ||
                   str.equals(LINE) || str.equals(ARROW) ||
                   str.equals(POLYLINE) || str.equals(POLYGON) ||
                   str.equals(FREETEXT) || str.equals(LINK) ||
                   str.equals(INK) || str.equals(UNDERLINE) ||
                   str.equals(HIGHLIGHT) || str.equals(GROUP);
        }
    }

    public static class ActionTypes {
        public final static String GO_TO_REMOTE = "GoToR";
        public final static String URI = "URI";
        public final static String GO_TO = "GoTo";
    }
}
