/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

/**
 * CustomEditComand - the command which is used for complex annotation
 * construction phase (relevant for PDFL.LinkAnnotation or
 * PDFL.FreeTextAnnotation).
 */
public class CustomEditCommand extends EditAnnotationCommand {
    private final static CommandType type = CommandType.EDIT;
}
