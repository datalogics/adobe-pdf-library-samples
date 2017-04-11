/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive;

import java.awt.Rectangle;

import com.datalogics.PDFL.Point;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.ViewDestination;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.ActionProperties;
import com.datalogics.PDFL.JavaViewer.Presentation.Application;
import com.datalogics.PDFL.JavaViewer.Presentation.ApplicationController;
import com.datalogics.PDFL.JavaViewer.Presentation.PDFPresenter;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.ZoomMode;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.BaseAnnotationEditor;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.LinkTargetInput;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.LinkTargetInput.TargettingState;
import com.datalogics.PDFL.JavaViewer.Presentation.PDFPresenter.ViewShot;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.LinkTargetDialog;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.Dialogs.NonModalDialogComplete;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.Dialogs.Result;

/**
 * LinkTargetInteractive - helper for setting link annotation parameters.
 * 
 * This class fills PDFL.Action for PDFL.LinkAnnotation, shows dialog with
 * parameters for link annotation. Also it processes user activity to decide
 * when LinkAnnotation creation was finished.
 * 
 * Before processing current view state is stored, and after user finishes work
 * with link annotation creation view state returns to its original point.
 */
public class LinkTargetInteractive extends ScrollModeInteractive {
    public LinkTargetInteractive(BaseAnnotationEditor editor, LinkTargetInput input) {
        this.editor = editor;
        this.input = input;
    }

    @Override
    protected void onPageChanged() {
        super.onPageChanged();
        if (input.getTargettingState().isTargetting()) {
            getApplication().registerInteractive(editor, originPage == getActivePage());
        }
    }

    @Override
    public void activate(boolean active) {
        super.activate(active);
        if (isActive()) {
            if (input.getTargettingState().isTargetting()) {
                startTargetting();
            }
        } else {
            if (input.getTargettingState().isTargetting()) {
                targetDialog.cancel();

                input.setTargettingState(TargettingState.DEACTIVATE_MODE);
                editorActivate(false);
            }
            targetDialog = null;
        }
    }

    private void editorActivate(boolean activate) {
        getApplication().getActiveDocument().select(activate ? editor.getHolder() : null);
        getApplication().registerInteractive(editor, activate);
        editor.setState(activate ? EditorStates.CUSTOM_EDIT : EditorStates.IDLE);
    }

    private void startTargetting() {
        originPage = getActivePage();
        editorActivate(true);

        targetDialog = getApplication().getView().getDialogs().linkTargetDialog(new NonModalDialogComplete() {
            public void complete(Result result) {
                if (Result.Yes.equals(result)) {
                    final PDFPresenter presenter = ((ApplicationController) getApplication()).getPDFPresenter();
                    ActionProperties actionProperties = createActionProperties(targetDialog, presenter.getViewShot());
                    editor.getProperties().setAction(actionProperties);
                }

                input.setTargettingState(TargettingState.DIALOG_FINISHED);
                editorActivate(false);
            }
        });
    }

    private ActionProperties createActionProperties(LinkTargetDialog targetDialog, ViewShot viewShot) {
        String fitMode = "XYZ";
        double zoom = 0;

        Point location = getPageModel().relativePdf(viewShot.getLocation());
        Rect destRect = new Rect(ViewDestination.NULL_VALUE, ViewDestination.NULL_VALUE, ViewDestination.NULL_VALUE, ViewDestination.NULL_VALUE);
        if (targetDialog.isCustom()) {
            if (viewShot.getZoomMode().equals(ZoomMode.ZOOM_FIT_PAGE) || viewShot.getZoomMode().equals(ZoomMode.ZOOM_FIT_WIDTH) || viewShot.getZoomMode().equals(ZoomMode.ZOOM_FIT_HEIGHT)) {
                boolean hasHorz = viewShot.getZoomMode().equals(ZoomMode.ZOOM_FIT_WIDTH);
                boolean hasVert = viewShot.getZoomMode().equals(ZoomMode.ZOOM_FIT_HEIGHT);

                if (hasHorz)
                    fitMode = "FitH";
                else if (hasVert)
                    fitMode = "FitV";
                else
                    fitMode = "Fit";

                zoom = ViewDestination.NULL_VALUE;
                destRect.setLeft(hasVert && targetDialog.isPosition() ? location.getH() : ViewDestination.NULL_VALUE);
                destRect.setTop(hasHorz && targetDialog.isPosition() ? location.getV() : ViewDestination.NULL_VALUE);

            } else {
                zoom = targetDialog.isZoom() ? viewShot.getZoom() : ViewDestination.NULL_VALUE;
                if (targetDialog.isPosition()) {
                    destRect.setLeft(location.getH());
                    destRect.setRight(location.getH());
                    destRect.setTop(location.getV());
                    destRect.setBottom(location.getV());
                }
            }
        } else {
            fitMode = "FitR";
            final Application application = getApplication();
            final Rectangle visibleViewRect = new Rectangle(application.getView().getPDF().getViewOrigin(), application.getView().getPDF().getVisibleViewSize());
            destRect = getPageModel().transform(visibleViewRect);
        }

        return new ActionProperties(destRect, viewShot.getPageNum(), fitMode, zoom);
    }

    private LinkTargetInput input;
    private BaseAnnotationEditor editor;

    private LinkTargetDialog targetDialog;
    private int originPage;
}
