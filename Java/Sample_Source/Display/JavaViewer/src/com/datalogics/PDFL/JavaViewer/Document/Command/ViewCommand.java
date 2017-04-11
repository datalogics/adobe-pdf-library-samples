/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import com.datalogics.PDFL.JavaViewer.Presentation.PDFPresenter;

/**
 * ViewCommand - the base class for all commands which responsible for the
 * document's representation change.
 * 
 * This command has PDFPresenter object for document view manipulation.
 */
public abstract class ViewCommand extends DocumentCommand {
    public void setPDFPresenter(PDFPresenter presenter) {
        this.pdfPresenter = presenter;
    }

    final protected PDFPresenter getPDFPresenter() {
        return this.pdfPresenter;
    }

    private PDFPresenter pdfPresenter;
}
