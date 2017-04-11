/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views;

import java.util.HashMap;

import javax.swing.JOptionPane;

import com.datalogics.PDFL.JavaViewer.Views.Interfaces.Dialogs;

public enum MessageDialogType {
    SAVE_DIALOG(Dialogs.MessageType.PROMPT_SAVE, "Do you want to save the current document before closing?", "Save request", JOptionPane.YES_NO_CANCEL_OPTION, true), SAVE_FAIL_DIALOG(Dialogs.MessageType.SAVE_FAIL, "Save failed! The current document may be opened in another application. Do you want to save it with a different name?", "Save failed", JOptionPane.YES_NO_OPTION, true), OPEN_FAIL_DIALOG(Dialogs.MessageType.OPEN_FAIL, "Document open failed! You may have typed wrong password or the document is corrupted!", "Open failed", JOptionPane.YES_OPTION, false), PRINT_FAIL_DIALOG(Dialogs.MessageType.PRINT_FAIL, "A PDF must be open in order to print.", "Print failed", JOptionPane.YES_OPTION, false);
    MessageDialogType(Dialogs.MessageType type, String message, String title, int option, boolean messageType) {
        this.type = type;
        this.message = message;
        this.title = title;
        this.option = option;
        this.messageType = messageType;
    }

    public boolean getMessageType() {
        return messageType;
    }

    public String getMessage() {
        return message;
    }

    public String getTitle() {
        return title;
    }

    public int getOption() {
        return option;
    }

    public static MessageDialogType fromType(Dialogs.MessageType type) {
        return mapping.get(type);
    }

    private Dialogs.MessageType getType() {
        return type;
    }

    private static HashMap<Dialogs.MessageType, MessageDialogType> mapping;

    static {
        mapping = new HashMap<Dialogs.MessageType, MessageDialogType>(MessageDialogType.values().length);

        for (MessageDialogType type : MessageDialogType.values()) {
            mapping.put(type.getType(), type);
        }
    }

    private Dialogs.MessageType type;
    private String message;
    private String title;
    private int option;
    private boolean messageType;
}
