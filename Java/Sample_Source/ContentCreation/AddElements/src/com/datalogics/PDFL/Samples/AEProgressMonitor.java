/*
 *
 * A sample which demonstrates the use of the DLE API to create a new
 * PDF file and add two pages containing a series of Path and Text elements
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
package com.datalogics.pdfl.samples.ContentCreation.AddElements;

import com.datalogics.PDFL.ProgressMonitor;

/**
 *
 * @author kam
 */
public class AEProgressMonitor extends ProgressMonitor {

    protected int duration;
    protected int currentValue;

    @Override
    public void endOperation() {
        super.endOperation();
        if (currentValue != duration) {
            System.out.append("100%");
        }
        System.out.println();
    }

    /**
     * Get the value of duration
     *
     * @return the value of duration
     */
    @Override
    public int getDuration() {
        return duration;
    }

    /**
     * Set the value of duration
     *
     * @param duration new value of duration
     */
    @Override
    public void setDuration(int duration) {
        this.duration = duration;
    }

    /**
     * Get the value of currentValue
     *
     * @return the value of currentValue
     */
    @Override
    public int getCurrentValue() {
        return currentValue;
    }

    /**
     * Set the value of currentValue
     *
     * @param currentValue new value of currentValue
     */
    @Override
    public void setCurrentValue(int currentValue) {
        this.currentValue = currentValue;
        System.out.print(currentValue * 100 / duration);
        System.out.print("%...");
        System.out.flush();
    }

    @Override
    public void setText(String arg0) {
        super.setText(arg0);
        System.out.print(arg0);
        System.out.print("...");
        System.out.flush();
    }
}
