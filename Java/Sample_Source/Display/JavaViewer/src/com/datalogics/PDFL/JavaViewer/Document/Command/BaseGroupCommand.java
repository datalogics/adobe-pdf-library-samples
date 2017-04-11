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
import java.util.Map;
import java.util.TreeMap;

import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationConsts;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationHolder;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationProperties;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.GroupAnnotationEditor;

/**
 * BaseGroupCommand - the child of the BaseAnnotationCommand class.
 * 
 * Saves the properties of all annotations in a group and uses them during
 * undo/redo phases.
 */
public class BaseGroupCommand extends BaseAnnotationCommand {
    public BaseGroupCommand() {
        props = new TreeMap<Integer, AnnotationProperties>();
        newProps = new TreeMap<Integer, AnnotationProperties>();
    }

    @Override
    protected void doInner() throws FinalState {
        setCommandState(CommandState.DO);
        saveNewProperties(); // save properties for group holder
        saveGroupProperties();
    }

    @Override
    protected void saveAnotationParams() {
        super.saveAnotationParams();
        saveGroupProperties();
    }

    @Override
    protected void changeProperties() {
        reCreateGroup(); // because group holder always has same index we are
                         // should recreate it
        super.changeProperties();

        final GroupAnnotationEditor group = ((GroupAnnotationEditor) getAnnotationInteractive().getEditorByHolder(getHolder()));
        for (int i = 0, len = group.getEditorsCount(); i < len; ++i) {
            final AnnotationHolder holder = group.getEditor(i).getHolder();
            final AnnotationProperties property = getProps().get(holder.getIndex());
            holder.getProperties().setFrom(property);
            holder.getProperties().setDirty(true);
            holder.getProperties().raiseUpdates(true);
        }
    }

    protected void reCreateGroup() {
        getApplication().getActiveDocument().createAnnotation(AnnotationConsts.Subtypes.GROUP, getPageNum(), getProperties(), -2);

        GroupAnnotationEditor groupEditor = (GroupAnnotationEditor) getAnnotationInteractive().getEditorByHolder(getHolder());

        Iterator<Integer> holderNum = getProps().keySet().iterator();
        final List<AnnotationHolder> pageHolders = new ArrayList<AnnotationHolder>(getApplication().getActiveDocument().getPageAnnotations(getPageNum()));
        while (holderNum.hasNext()) {
            groupEditor.addEditor(getAnnotationInteractive().getEditorByHolder(pageHolders.get(holderNum.next())));
        }
    }

    protected Map<Integer, AnnotationProperties> getProps() {
        return getCommadState() == null || getCommadState().isUndo() ? props : newProps;
    }

    protected void saveGroupProperties() {
        final GroupAnnotationEditor group = ((GroupAnnotationEditor) getAnnotationInteractive().getEditorByHolder(getHolder()));
        for (int i = 0, len = group.getEditorsCount(); i < len; ++i) {
            final AnnotationProperties props = new AnnotationProperties();
            final AnnotationHolder holder = group.getEditor(i).getHolder();
            props.setFrom(holder.getProperties());
            getProps().put(holder.getIndex(), props);
        }
    }

    private Map<Integer, AnnotationProperties> props;
    private Map<Integer, AnnotationProperties> newProps;

}
