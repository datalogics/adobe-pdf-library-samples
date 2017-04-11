/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation;

import com.datalogics.PDFL.JavaViewer.Document.JavaDocument;
import com.datalogics.PDFL.JavaViewer.Document.Command.DocumentCommand;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Interactive;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.Viewer;

/**
 * Application - interface which represents commonly used application's
 * functionality.
 */
public interface Application {
    /**
     * Adds a document to the application and activates it if needed
     * 
     * @param document - the document to add to the application
     * @param activate - should it be activated immediately
     */
    void addDocument(JavaDocument document, boolean activate);

    /**
     * Removes a document from the application
     * 
     * @param document - the document to remove from the application, it may be
     * the active document as well
     */
    void removeDocument(JavaDocument document);

    /**
     * Allows to obtain current active document
     * 
     * @return active document of the application
     */
    JavaDocument getActiveDocument();

    /**
     * Main view interface of the application
     * 
     * @return main view interface of the application
     */
    Viewer getView();

    /**
     * Executes command within the application
     * 
     * @param command - a command to be executed. When passed 'null' will
     * execute pending command if available
     * @return a command really executed (may sometimes differ from passed one)
     */
    DocumentCommand executeCommand(DocumentCommand command);

    /**
     * Allows to register new interactive handler within the application, if the
     * handler has been already registered - it will be moved to the top of
     * processing chain
     * 
     * @param interactive - a handler to be registered
     * @param register - specifies whether to register a handler or remove it
     */
    void registerInteractive(Interactive interactive, boolean register);

}
