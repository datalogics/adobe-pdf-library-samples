/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Annotations;

import com.datalogics.PDFL.Action;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.FileSpecification;
import com.datalogics.PDFL.GoToAction;
import com.datalogics.PDFL.LinkAnnotation;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.RemoteDestination;
import com.datalogics.PDFL.RemoteGoToAction;
import com.datalogics.PDFL.URIAction;
import com.datalogics.PDFL.ViewDestination;
import com.datalogics.PDFL.JavaViewer.Utils;

/**
 * ActionProperties - the class is a wrapper for the PDFL.GoToAction.
 * 
 * This class stores such parameters as zoom, fit type, page number and
 * destination rectangle. It allows serialization/deserialization of GoToAction
 * and primarily used in undo/redo commands.
 */
public class ActionProperties {
    public ActionProperties() {
        this(new Rect(), 0, "XYZ", 0);
    }

    public ActionProperties(Action action) {
        if (action instanceof GoToAction) {
            ViewDestination dest = ((GoToAction) action).getDestination();
            if (dest != null) {
                this.destRect = Utils.cloneNonSorted(dest.getDestRect());

                // Set Right and Bottom values for destination rect because
                // ViewDestination clean unused parameters by itself
                if (dest.getFitType().equals("XYZ")) {
                    destRect.setRight(destRect.getLeft());
                    destRect.setBottom(destRect.getTop());
                }
                this.pageNum = dest.getPageNumber();
                this.zoom = dest.getZoom();
                this.fitType = dest.getFitType();
            }
        } else if (action instanceof RemoteGoToAction) {
            final RemoteDestination remoteDest = ((RemoteGoToAction) action).getRemoteDestination();
            this.fileSpecPath = ((RemoteGoToAction) action).getFileSpecification().getPath();

            this.pageNum = remoteDest.getPageNumber();
            this.destRect = Utils.cloneNonSorted(remoteDest.getDestRect());
            this.fitType = remoteDest.getFitType();
            this.zoom = remoteDest.getZoom();
        } else if (action instanceof URIAction) {
            this.uri = ((URIAction) action).getURI();
            this.useMap = ((URIAction) action).getIsMap();
        }
    }

    public ActionProperties(Rect destRect, int pageNum, String fitType, double zoom) {
        this.destRect = Utils.cloneNonSorted(destRect);
        this.pageNum = pageNum;
        this.fitType = fitType;
        this.zoom = zoom;
    }

    public Action createAction(LinkAnnotationHolder holder) {
        if (holder.getAnnotation() instanceof LinkAnnotation) {
            final String subtype = holder.getProperties().getActionSubtype();
            final Document doc = holder.getAnnotation().getPDFDict().getDocument();

            if (subtype.equals(AnnotationConsts.ActionTypes.GO_TO)) {
                ViewDestination destination = new ViewDestination(doc, getPageNum(), getFitType(), getDestRect(), getZoom());
                return new GoToAction(destination);
            } else if (subtype.equals(AnnotationConsts.ActionTypes.GO_TO_REMOTE)) {
                final FileSpecification fileSpec = new FileSpecification(doc, getFileSpecPath());
                final RemoteDestination remoteDest = new RemoteDestination(doc, getPageNum(), getFitType(), getDestRect(), getZoom());
                return new RemoteGoToAction(fileSpec, remoteDest);
            } else if (subtype.equals(AnnotationConsts.ActionTypes.URI)) {
                return new URIAction(getURI(), getUseMap());
            }
        }
        return null;
    }

    private int getPageNum() {
        return pageNum;
    }

    private String getFitType() {
        return fitType;
    }

    private Rect getDestRect() {
        return destRect;
    }

    private double getZoom() {
        return zoom;
    }

    private String getFileSpecPath() {
        return fileSpecPath;
    }

    private String getURI() {
        return uri;
    }

    private boolean getUseMap() {
        return useMap;
    }

    private int pageNum;
    private String fitType;
    private Rect destRect;
    private double zoom;
    private String fileSpecPath;
    private String uri;
    private boolean useMap;
}
