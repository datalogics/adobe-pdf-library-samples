/*
Copyright (C) 2008-2009, Datalogics, Inc. All rights reserved.
 
The information and code in this sample is for the exclusive use
of Datalogics customers and evaluation users only.  Datalogics 
permits you to use, modify and distribute this file in accordance 
with the terms of your license agreement. Sample code is for 
demonstrative purposes only and is not intended for production use.
*/

using System;
using System.Text;
using System.Collections.Generic;
using Datalogics.PDFL;

namespace DotNETViewerComponent
{
    /**
     * AnnotationFactory - contains functions for creating different types of annotations
     */
    public static class AnnotationFactory
    {
        /**
         * Creates annotation, type of annotation is taken from given properties
         */
        public static Annotation CreateAnnotation(Document doc, Page page, int index, AnnotationProperties props)
        {
            Annotation ann = null;
            int addAfter = index - 1;
            switch (props.Subtype)
            {
                case "Line": ann = new LineAnnotation(page, props.BoundingRect, props.StartPoint, props.EndPoint, addAfter); break;
                case "Circle": ann = new CircleAnnotation(page, props.BoundingRect, addAfter); break;
                case "Square": ann = new SquareAnnotation(page, props.BoundingRect, addAfter); break;
                case "FreeText": ann = new FreeTextAnnotation(page, props.BoundingRect, "", addAfter); break;
                case "PolyLine": ann = new PolyLineAnnotation(page, props.BoundingRect, props.Vertices, addAfter); break;
                case "Polygon": ann = new PolygonAnnotation(page, props.BoundingRect, props.Vertices, addAfter); break;
                case "Link": ann = new LinkAnnotation(page, props.BoundingRect, addAfter); break;
                case "Ink": ann = new InkAnnotation(page, props.BoundingRect, addAfter); break;
                case "Underline": ann = new UnderlineAnnotation(page, addAfter, props.Quads); break;
                case "Highlight": ann = new HighlightAnnotation(page, addAfter, props.Quads); break;
                default: throw new Exception("Logic error");
            }
            AnnotationAppearance app = new AnnotationAppearance(doc, ann);
            app.CaptureAnnotation();
            app.Properties.SetFrom(props);
            app.Properties.Dirty = true;
            app.UpdateAppearance();
            app.ReleaseAnnotation();
            return ann;
        }
        /**
         * Creates annotation editor class by given annotation info
         */
        public static BaseAnnotationEditor CreateAnnotationEditor(Document doc, Page page, int index)
        {
            Annotation ann = page.GetAnnotation(index);
            System.Drawing.Color underlineSelectionColor = System.Drawing.Color.FromArgb(128, 0, 0, 128);
            System.Drawing.Color highlightSelectionColor = System.Drawing.Color.FromArgb(128, 0, 128, 0);
            BaseAnnotationEditor editor = null;
            switch (ann.Subtype)
            {
                case "Circle": editor = new CircleAnnotationEditor(); break;
                case "Square": editor = new SquareAnnotationEditor(); break;
                case "Line": editor = new LineAnnotationEditor(); break;
                case "PolyLine": editor = new PolyLineAnnotationEditor(); break;
                case "Polygon": editor = new PolygonAnnotationEditor(); break;
                case "Link": editor = new LinkAnnotationEditor(); break;
                case "Ink": editor = new InkAnnotationEditor(); break;
                case "Underline": editor = new MarkupAnnotationEditor(underlineSelectionColor); break;
                case "Highlight": editor = new MarkupAnnotationEditor(highlightSelectionColor); break;
                case "FreeText": editor = new FreeTextAnnotationEditor(); break;
                default: editor = new BaseAnnotationEditor(); break;
            }

            if (editor != null) editor.Init(doc, page, ann, index);
            return editor;
        }
        public static BaseAnnotationEditor CreateAnnotationAndEditor(Document doc, Page page, int index, AnnotationProperties props)
        {
            CreateAnnotation(doc, page, index, props);
            return CreateAnnotationEditor(doc, page, index);
        }
    }
}
