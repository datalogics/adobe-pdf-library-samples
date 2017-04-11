
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
package com.datalogics.PDFL.Samples;

import com.datalogics.PDFL.ReportProc;
import com.datalogics.PDFL.ReportType;

/**
 *
 * @author kam
 */
public class AEReportProc extends ReportProc {

    @Override
    public void Call(ReportType reportType, String message, String replacementText) {
        if (reportType == ReportType.ERROR) {
            System.out.print("Error: ");
        } else if (reportType == ReportType.NOTE) {
            System.out.print("Note: ");
        } else if (reportType == ReportType.WARNING) {
            System.out.print("Warning:");
        }

        if (message != null) {
            if (replacementText != null) {
                message.replace("%s", replacementText);
            }
            System.out.println(message);
        } else if (replacementText != null) {
            System.out.println(replacementText);
        } else {
            System.out.println("Unknown");
        }
    }
}
