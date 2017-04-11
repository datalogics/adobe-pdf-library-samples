/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views.Interfaces;

public interface Dialogs {
    public interface NonModalDialogComplete {
        public void complete(Result result);
    }

    public static enum Result {
        Yes(true), No(false), Cancel(null);

        private Result(Boolean status) {
            this.status = status;
        }

        public boolean isSucceed() {
            return !isCanceled() && status == true;
        }

        public boolean isRefused() {
            return !isCanceled() && status != true;
        }

        public boolean isCanceled() {
            return status == null;
        }

        private Boolean status;
    }

    public static enum MessageType {
        PROMPT_SAVE, SAVE_FAIL, OPEN_FAIL, PRINT_FAIL;
    }

    String openDialog();

    String saveDialog();

    String promptPassword();

    int customLineWidth(int min, int max, int initValue);

    ColorPicker colorPickerDialog(ColorPicker currentColors);

    PropertyDialog propertyDialog(String title, String contents, boolean editable);

    Result messageDialog(MessageType type);

    LinkTargetDialog linkTargetDialog(NonModalDialogComplete callBack);

    int resolutionDialog(int currentResolution);
}
