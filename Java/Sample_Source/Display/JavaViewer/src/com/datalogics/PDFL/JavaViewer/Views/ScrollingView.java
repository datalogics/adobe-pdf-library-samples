/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views;

/*
 * ScrollingView - it is an implementation of Scrolling interface. It creates
 * scrollbar for an application and sends updates when scroll is changed.
 */

import java.awt.event.AdjustmentEvent;
import java.awt.event.AdjustmentListener;
import java.util.ArrayList;
import java.util.List;

import javax.swing.JScrollBar;

import com.datalogics.PDFL.JavaViewer.Views.Interfaces.PDF.Scrolling;

public class ScrollingView extends JScrollBar implements Scrolling, AdjustmentListener {
    public ScrollingView() {
        listeners = new ArrayList<AdjustmentListener>();
        addAdjustmentListener(this);
    }

    public void setScrollVisible(boolean visible) {
        this.setVisible(visible);
    }

    public void setCurrent(int value) {
        this.setValue(value);
    }

    public int getCurrent() {
        return this.getValue();
    }

    public void setScrollValues(int value, int extent, int min, int max) {
        this.setValues(value, extent, min, max);
        this.setBlockIncrement(extent - 5);
    }

    public boolean inProgress() {
        return inProgress;
    }

    public void adjustmentValueChanged(AdjustmentEvent e) {
        final int delta = Math.abs(e.getValue() - lastValue);
        inProgress = (e.getValueIsAdjusting() && this.getBlockIncrement() != delta && this.getUnitIncrement() != delta);

        lastValue = e.getValue();
        for (AdjustmentListener l : getAdjustmentListeners()) {
            l.adjustmentValueChanged(e);
        }
    }

    @Override
    public void addAdjustmentListener(AdjustmentListener l) {
        if (l.equals(this)) {
            super.addAdjustmentListener(l);
        } else {
            listeners.add(l);
        }
    }

    @Override
    public AdjustmentListener[] getAdjustmentListeners() {
        return listeners.toArray(new AdjustmentListener[0]);
    }

    @Override
    public void removeAdjustmentListener(AdjustmentListener l) {
        if (l.equals(this)) {
            super.removeAdjustmentListener(l);
        } else {
            listeners.remove(l);
        }
    }

    private boolean inProgress;
    private int lastValue;
    private final List<AdjustmentListener> listeners;
}
