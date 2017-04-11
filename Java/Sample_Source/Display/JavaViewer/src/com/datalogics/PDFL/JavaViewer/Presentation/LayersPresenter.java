/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation;

import java.util.ArrayList;
import java.util.List;

import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.OptionalContentGroup;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.JavaViewer.Document.JavaDocument;
import com.datalogics.PDFL.JavaViewer.Document.DocumentListener;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationHolder;
import com.datalogics.PDFL.JavaViewer.Document.Command.LayersCommand;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.Layers;

/**
 * LayersPresenter - keeps all information about document layers and their
 * state.
 * 
 * Also this class sends updates when layers are updated.
 */
public class LayersPresenter implements DocumentListener {
    public class LayerInfo {
        public LayerInfo(OptionalContentGroup ocg) {
            this.ocg = ocg;
            this.name = ocg != null ? ocg.getName() : "";
        }

        protected LayerInfo(String name) {
            this((OptionalContentGroup) null);
            this.name = name;
        }

        public void setShowState(boolean state) {
            updateLayers(state, true, getLayerUpdateList());
        }

        public void setPrintState(boolean state) {
            updateLayers(state, false, getLayerUpdateList());
        }

        public boolean getShowState() {
            return document.getShowLayerState(ocg);
        }

        public boolean getPrintState() {
            return document.getPrintLayerState(ocg);
        }

        public String getName() {
            return this.name;
        }

        public List<OptionalContentGroup> getLayerUpdateList() {
            List<OptionalContentGroup> ocgList = new ArrayList<OptionalContentGroup>();
            ocgList.add(this.ocg);
            return ocgList;
        }

        @Override
        public int hashCode() {
            return ocg != null ? ocg.hashCode() : 0;
        }

        @Override
        public boolean equals(Object obj) {
            boolean isEqual = false;
            if (obj == null || obj.getClass() != this.getClass())
                isEqual = false;
            else if (this == obj)
                isEqual = true;
            else {
                LayerInfo li = (LayerInfo) obj;
                isEqual = (this.ocg == null && li.ocg == null) || (this.ocg != null && li.ocg != null && this.ocg.equals(li.ocg));
            }
            return isEqual;
        }

        @Override
        public String toString() {
            return this.name;
        }

        protected void updateLayers(boolean state, boolean isDisplayableLayers, List<OptionalContentGroup> ocgList) {
            application.executeCommand(new LayersCommand(state, isDisplayableLayers, ocgList));
        }

        private OptionalContentGroup ocg;
        private String name;
    }

    public class AllLayersInfo extends LayerInfo {
        public AllLayersInfo(String name) {
            super(name);
        }

        @Override
        public boolean getPrintState() {
            boolean allVisible = true;
            for (OptionalContentGroup ocg : document.getOCGList()) {
                allVisible = document.getPrintLayerState(ocg);
                if (!allVisible)
                    break;
            }
            return allVisible;
        }

        @Override
        public boolean getShowState() {
            boolean allVisible = true;
            for (OptionalContentGroup ocg : document.getOCGList()) {
                allVisible = document.getShowLayerState(ocg);
                if (!allVisible)
                    break;
            }
            return allVisible;
        }

        @Override
        public List<OptionalContentGroup> getLayerUpdateList() {
            return document.getOCGList();
        }
    }

    public LayersPresenter(Application application) {
        this.application = application;
    }

    void setView(Layers view) {
        this.layersView = view;
    }

    void activeDocumentChanged() {
        document = application.getActiveDocument();
    }

    public void pdfChanged(Document pdfDoc) {
        if (document.hasLayers()) {
            layersView.clearLayers();

            final List<OptionalContentGroup> ocgList = document.getOCGList();
            if (ocgList.size() > 1)
                layersView.updateLayer(new AllLayersInfo(ALL_LAYERS_NAME));

            for (OptionalContentGroup ocg : ocgList)
                layersView.updateLayer(new LayerInfo(ocg));
        }

        layersView.setLayersVisible(document.hasLayers());
    }

    public void permissionChanged() {
    }

    public void pdfLayerUpdated(List<OptionalContentGroup> ocgList) {
        for (OptionalContentGroup ocg : ocgList)
            layersView.updateLayer(new LayerInfo(ocg));

        if (document.getOCGList().size() > 1)
            layersView.updateLayer(new AllLayersInfo(ALL_LAYERS_NAME));
    }

    public void pageChanged(int index, Rect changedArea) {
    }

    public void selectionChanged(Object oldSelection) {
    }

    public void annotationUpdated(AnnotationHolder holder, boolean created) {
    }

    public void annotationDeleted() {
    }

    private final static String ALL_LAYERS_NAME = "All layers";

    private Application application;
    private Layers layersView;
    private JavaDocument document;
}
