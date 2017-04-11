/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationHolder;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationProperties;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.EditorStates;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.GroupAnnotationEditor;

/**
 * DeleteGroupCommand - allows to delete group annotation from the page (all
 * annotations in the group will be removed).
 */
public class DeleteGroupCommand extends BaseGroupCommand {
    @Override
    protected void prepare() throws FinalState {
        saveAnotationParams();
    }

    @Override
    protected void doInner() throws FinalState {
        deleteGroup();
    }

    @Override
    protected void undoInner() throws FinalState {
        super.undoInner();
        getApplication().getActiveDocument().select(null);
        repairHolders();
        getAnnotationInteractive().getEditorByHolder(getHolder()).setState(EditorStates.SELECT);
        changeProperties();
    }

    @Override
    protected void redoInner() throws FinalState {
        super.redoInner();
        setCommandState(null);
        if (getHolder() == null)
            reCreateGroup();
        doInner();
    }

    private void deleteGroup() {
        ((GroupAnnotationEditor) getAnnotationInteractive().getEditorByHolder(getHolder())).clearEditors();
        getApplication().getActiveDocument().deleteAnnotation(getHolder());

        Integer[] indexesArray = getProps().keySet().toArray(new Integer[0]);
        List<AnnotationHolder> holders = new ArrayList<AnnotationHolder>(getApplication().getActiveDocument().getPageAnnotations(getPageNum()));
        for (int i = indexesArray.length - 1; i >= 0; --i) {
            getApplication().getActiveDocument().deleteAnnotation(holders.get(indexesArray[i]));
        }
    }

    private void repairHolders() {
        final Iterator<Integer> holderNum = getProps().keySet().iterator();
        while (holderNum.hasNext()) {
            final int annotNum = holderNum.next();
            final AnnotationProperties props = getProps().get(annotNum);
            getApplication().getActiveDocument().createAnnotation(props.getSubtype(), getPageNum(), props, annotNum - 1);
        }

        reCreateGroup();
    }

    private final static CommandType type = CommandType.EDIT; // field used through reflection
}
