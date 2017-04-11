/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Annotations;

import java.util.List;

import com.datalogics.PDFL.Action;
import com.datalogics.PDFL.Annotation;
import com.datalogics.PDFL.Form;
import com.datalogics.PDFL.GoToAction;
import com.datalogics.PDFL.LinkAnnotation;
import com.datalogics.PDFL.PDFArray;
import com.datalogics.PDFL.PDFDict;
import com.datalogics.PDFL.PDFInteger;
import com.datalogics.PDFL.PDFObject;
import com.datalogics.PDFL.PDFReal;
import com.datalogics.PDFL.RemoteGoToAction;
import com.datalogics.PDFL.URIAction;
import com.datalogics.PDFL.ViewDestination;

/**
 * LinkAnnotationHolder - the child of the BaseAnnotationHolder.
 * 
 * This class allows to manipulate PDFL.LinkAnnotation using specific
 * functionality. It is also responsible for showing/hiding of the
 * PDFL.LinkAnnotation border (optionally).
 */
public class LinkAnnotationHolder extends BaseAnnotationHolder {
    @Override
    protected void init(Annotation annotation, int index, int pageIndex) {
        this.annotation = (LinkAnnotation) annotation;
        super.init(annotation, index, pageIndex);
    }

    @Override
    protected void doReadProperties() {
        super.doReadProperties();
        readPropertyByName(AnnotationConsts.DASH_PATTERN, List.class /*List<Double>*/, false);
        readPropertyByName(AnnotationConsts.STYLE, String.class, false);
        readPropertyByName(AnnotationConsts.WIDTH, double.class, true);

        if (annotation.getAction() != null) {
            Action action = annotation.getAction();
            ActionProperties actionProperties = new ActionProperties(annotation.getAction());
            getProperties().setAction(actionProperties);
            getProperties().setActionSubtype(action.getSubtype());
        } else if (annotation.getDestination() != null) {
            final ViewDestination dest = annotation.getDestination();
            ActionProperties actionProperties = new ActionProperties(dest.getDestRect(), dest.getPageNumber(), dest.getFitType(), dest.getZoom());
            getProperties().setAction(actionProperties);
            getProperties().setActionSubtype(AnnotationConsts.ActionTypes.GO_TO);
        }
        SetShowBorderProperties();

    }

    @Override
    protected Form generateAppearance() {
        return getProperties().getShowBorder() ? annotation.generateAppearance() : null;
    }

    @Override
    protected void doWriteProperties() {
        super.doWriteProperties();
        annotation.setAction(getProperties().getAction().createAction(this));
    }

    private void SetShowBorderProperties() {
        PDFObject width = null;
        // if annotation dictionary includes BS entry, then the Border entry is ignored
        if (annotation.getPDFDict().contains("BS"))
            width = (PDFObject) ((PDFDict) annotation.getPDFDict().get("BS")).get("W");
        else {
            try {
                width = (PDFInteger) ((PDFArray) annotation.getPDFDict().get("Border")).get(2);
            } catch (Exception e) {
            }
        }
        double lineWidth = getProperties().getLineWidth(); // default value
        if (width != null)
            lineWidth = width instanceof PDFReal ? ((PDFReal) width).getValue() : ((PDFInteger) width).getValue();
        getProperties().setLineWidth((float) lineWidth); // set proper border value from library properties

        PDFArray color = null;
        if (annotation.getPDFDict().contains("C")) {
            try {
                color = (PDFArray) annotation.getPDFDict().get("C");
            } catch (Exception e) {
            }
        }
        int colorLen = (color != null) ? color.getLength() : 3;
        getProperties().setNotNativeProperty(AnnotationConsts.SHOW_BORDER, colorLen != 0 && lineWidth != 0, boolean.class, false);
    }

    private LinkAnnotation annotation;
}
