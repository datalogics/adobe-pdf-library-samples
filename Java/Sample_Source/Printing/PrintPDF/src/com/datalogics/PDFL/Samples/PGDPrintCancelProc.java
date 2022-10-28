package com.datalogics.pdfl.samples.Printing.PrintPDF;
/*
 * 
 * A sample which demonstrates the use of a cancel callback in a print job.
 *  
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
import com.datalogics.PDFL.PrintCancelProc;

public class PGDPrintCancelProc extends PrintCancelProc {
	private boolean called;
	private boolean canceled;
	private boolean should_cancel;
	
	PGDPrintCancelProc(boolean can) {
		called = false;
		canceled = false;
		should_cancel = can;
	}
	
	public boolean Call() {
		called = true;
		canceled = should_cancel;
		System.out.println("Cancel procedure is being called");
		return should_cancel;
	}
};
