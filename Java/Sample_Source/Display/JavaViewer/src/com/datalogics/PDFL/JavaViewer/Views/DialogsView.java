/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views;

/*
 * DialogsView - it is implementation of Dialogs interface. It creates and shows
 * all dialogs which JavaViewer uses.
 */

import java.awt.BorderLayout;
import java.awt.Color;
import java.awt.Component;
import java.awt.Dimension;
import java.awt.FileDialog;
import java.awt.Frame;
import java.awt.GridBagConstraints;
import java.awt.GridBagLayout;
import java.awt.Insets;
import java.awt.Rectangle;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.event.ComponentAdapter;
import java.awt.event.ComponentEvent;
import java.awt.event.MouseEvent;
import java.awt.event.MouseListener;
import java.io.File;
import java.io.FilenameFilter;

import javax.swing.BorderFactory;
import javax.swing.Box;
import javax.swing.BoxLayout;
import javax.swing.ButtonGroup;
import javax.swing.JButton;
import javax.swing.JCheckBox;
import javax.swing.JColorChooser;
import javax.swing.JDialog;
import javax.swing.JLabel;
import javax.swing.JOptionPane;
import javax.swing.JPanel;
import javax.swing.JPasswordField;
import javax.swing.JRadioButton;
import javax.swing.JScrollPane;
import javax.swing.JSpinner;
import javax.swing.JTextArea;
import javax.swing.JTextField;
import javax.swing.SpinnerNumberModel;
import javax.swing.border.CompoundBorder;

import com.datalogics.PDFL.JavaViewer.Views.Interfaces.ColorPicker;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.Dialogs;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.LinkTargetDialog;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.PropertyDialog;

public class DialogsView implements Dialogs {
    public DialogsView(Component view) {
        this.view = view;
    }

    public String openDialog() {
        return dialog(FileDialog.LOAD, "open");
    }

    public String saveDialog() {
        final String pdfExt = ".pdf";
        final String fname = dialog(FileDialog.SAVE, "Save as");
        return fname != null ? (fname.toLowerCase().endsWith(pdfExt) ? fname : fname.concat(pdfExt)) : null;
    }

    public Result messageDialog(Dialogs.MessageType type) {
        final MessageDialogType mdt = MessageDialogType.fromType(type);
        Result res = Result.Cancel;
        if (mdt != null) {

            if (mdt.getMessageType() == true) {
                switch (JOptionPane.showConfirmDialog(view, mdt.getMessage(), mdt.getTitle(), mdt.getOption())) {
                case JOptionPane.YES_OPTION:
                    res = Result.Yes;
                    break;
                case JOptionPane.NO_OPTION:
                    res = Result.No;
                    break;
                case JOptionPane.CANCEL_OPTION:
                case JOptionPane.CLOSED_OPTION:
                    break;
                }
            } else {
                JOptionPane.showMessageDialog(view, mdt.getMessage(), mdt.getTitle(), mdt.getOption());
            }
        }
        return res;
    }

    public LinkTargetDialog linkTargetDialog(final NonModalDialogComplete dialogListener) {
        final JDialog dialog = new JDialog(new Frame(), "Link Annotation Properties");

        JButton setLinkButton = new JButton("Set Link");
        JButton closeButton = new JButton("Cancel");

        final JCheckBox usePageNumberCheckBox = new JCheckBox("Page Number");
        usePageNumberCheckBox.setEnabled(false);
        usePageNumberCheckBox.setSelected(true);

        JRadioButton useViewRectRadioButton = new JRadioButton("View Rect");
        useViewRectRadioButton.setSelected(true);

        final JRadioButton useCustomParamsRadioButton = new JRadioButton("Custom Params");

        final JCheckBox useZoomCheckBox = new JCheckBox("Zoom");
        useZoomCheckBox.setEnabled(false);
        useZoomCheckBox.setSelected(false);

        final JCheckBox usePositionCheckBox = new JCheckBox("Position");
        usePositionCheckBox.setEnabled(false);
        usePositionCheckBox.setSelected(false);

        setLinkButton.setSize(new Dimension(50, 100));
        closeButton.setSize(new Dimension(50, 100));

        ButtonGroup group = new ButtonGroup();
        group.add(useViewRectRadioButton);
        group.add(useCustomParamsRadioButton);

        JLabel label = new JLabel("<html><p align=left>" + "Use the scrollbars, mouse and zoom<br>" + "tools to select the target view, then<br>" + "press 'Set Link' to create the link<br> " + "destination.");
        label.setBorder(BorderFactory.createEmptyBorder(5, 20, 0, 20));

        JPanel paramsPanel = new JPanel();
        paramsPanel.setLayout(new BoxLayout(paramsPanel, BoxLayout.Y_AXIS));

        paramsPanel.setBorder(new CompoundBorder(BorderFactory.createEmptyBorder(5, 20, 20, 20), new CompoundBorder(BorderFactory.createTitledBorder("View Parameters"), BorderFactory.createEmptyBorder(0, 10, 0, 50))));
        paramsPanel.add(usePageNumberCheckBox);
        paramsPanel.add(useViewRectRadioButton);
        paramsPanel.add(useCustomParamsRadioButton);

        useZoomCheckBox.setBorder(BorderFactory.createEmptyBorder(5, 20, 0, 0));
        paramsPanel.add(useZoomCheckBox);
        usePositionCheckBox.setBorder(BorderFactory.createEmptyBorder(10, 20, 10, 0));
        paramsPanel.add(usePositionCheckBox);

        JPanel buttonsPanel = new JPanel();
        buttonsPanel.setLayout(new BoxLayout(buttonsPanel, BoxLayout.LINE_AXIS));

        buttonsPanel.add(setLinkButton);
        buttonsPanel.add(Box.createRigidArea(new Dimension(15, 0)));
        buttonsPanel.add(closeButton);
        buttonsPanel.setBorder(BorderFactory.createEmptyBorder(0, 30, 10, 5));

        JPanel contentPane = new JPanel(new BorderLayout());
        contentPane.add(label, BorderLayout.NORTH);

        contentPane.add(paramsPanel, BorderLayout.CENTER);
        contentPane.add(buttonsPanel, BorderLayout.PAGE_END);
        contentPane.setOpaque(true);
        dialog.setContentPane(contentPane);

        useViewRectRadioButton.addActionListener(new ActionListener() {
            public void actionPerformed(ActionEvent e) {
                final boolean enable = false;

                useZoomCheckBox.setEnabled(enable);
                useZoomCheckBox.setSelected(enable);
                usePositionCheckBox.setEnabled(enable);
                usePositionCheckBox.setSelected(enable);
            }
        });

        useCustomParamsRadioButton.addActionListener(new ActionListener() {
            public void actionPerformed(ActionEvent e) {
                final boolean enable = true;

                useZoomCheckBox.setEnabled(enable);
                useZoomCheckBox.setSelected(enable);
                usePositionCheckBox.setEnabled(enable);
                usePositionCheckBox.setSelected(enable);
            }
        });

        setLinkButton.addActionListener(new ActionListener() {
            public void actionPerformed(ActionEvent e) {
                dialog.dispose();
                dialogListener.complete(Result.Yes);
            }
        });

        closeButton.addActionListener(new ActionListener() {
            public void actionPerformed(ActionEvent e) {
                dialog.dispose();
                dialogListener.complete(Result.No);
            }
        });

        dialog.setResizable(false);
        dialog.pack();
        dialog.setVisible(true);

        return new LinkTargetDialog() {
            public boolean isZoom() {
                return isCustom() ? useZoomCheckBox.isSelected() : false;
            }

            public boolean isPosition() {
                return isCustom() ? usePositionCheckBox.isSelected() : false;
            }

            public boolean isCustom() {
                return useCustomParamsRadioButton.isSelected();
            }

            public boolean isPageNumber() {
                return usePageNumberCheckBox.isSelected();
            }

            public void cancel() {
                dialog.dispose();
            }
        };
    }

    public String promptPassword() {
        String password = null;

        final JLabel passwordPrompt1 = new JLabel("The requested operation requires additional permissions.");
        final JLabel passwordPrompt2 = new JLabel("Please enter the document password.");
        final JPasswordField jpf = new JPasswordField();
        JOptionPane jop = new JOptionPane(new Object[] { passwordPrompt1, passwordPrompt2, jpf }, JOptionPane.QUESTION_MESSAGE, JOptionPane.OK_CANCEL_OPTION);

        JDialog dialog = jop.createDialog(view, "Encrypted document");
        dialog.addComponentListener(new ComponentAdapter() {
            @Override
            public void componentShown(ComponentEvent e) {
                jpf.requestFocusInWindow();
            }
        });

        dialog.setVisible(true);
        Object value = jop.getValue();
        int result = value == null ? JOptionPane.CANCEL_OPTION : (Integer) value;
        dialog.dispose();
        if (result == JOptionPane.OK_OPTION)
            password = new String(jpf.getPassword());

        return password;
    }

    public int customLineWidth(int min, int max, int initValue) {
        int lineWidth = initValue;

        final JLabel message = new JLabel("Select line width.");
        final JSpinner jtf = new JSpinner(new SpinnerNumberModel(lineWidth, min, max, 1));
        JOptionPane jop = new JOptionPane(new Object[] { message, jtf }, JOptionPane.DEFAULT_OPTION, JOptionPane.OK_CANCEL_OPTION);

        JDialog dialog = jop.createDialog(view, "Line width");
        dialog.addComponentListener(new ComponentAdapter() {
            @Override
            public void componentShown(ComponentEvent e) {
                jtf.requestFocusInWindow();
            }
        });

        dialog.setVisible(true);
        Object value = jop.getValue();
        int result = value == null ? JOptionPane.CANCEL_OPTION : (Integer) value;
        dialog.dispose();
        if (result == JOptionPane.OK_OPTION) {
            lineWidth = (Integer) jtf.getValue();
        }

        return lineWidth;
    }

    public ColorPicker colorPickerDialog(final ColorPicker currentColors) {
        class DialogPicker extends ColorPickerIcon {
            public DialogPicker(int height, int width, ColorPicker colors) {
                super(height, width, colors);
            }

            @Override
            public Rectangle getBackRect() {
                return new Rectangle(0, 0, super.getIconHeight(), super.getIconHeight());
            }

            @Override
            public Rectangle getForeRect() {
                return new Rectangle(getBackRect().width / 2, getBackRect().height / 2, super.getIconHeight(), super.getIconHeight());
            }

            @Override
            public int getIconHeight() {
                return (getForeRect().height + getForeRect().height / 2) + 1;
            }

            @Override
            public int getIconWidth() {
                return (getForeRect().width + getForeRect().width / 2) + 1;
            }

        }
        final JDialog dialog = new JDialog(new Frame(), "Color picker");

        final JLabel helpLabel = new JLabel("Click on a color square to change its color");
        final JLabel mainLabel = new JLabel(new DialogPicker(100, 100, currentColors));
        final Rectangle foreRect = ((DialogPicker) mainLabel.getIcon()).getForeRect();
        final Rectangle backRect = ((DialogPicker) mainLabel.getIcon()).getBackRect();

        JButton transperButton = new JButton("Make Fill Transparent");
        transperButton.setMinimumSize(new Dimension(150, 25));
        transperButton.setPreferredSize(new Dimension(150, 25));
        transperButton.setMaximumSize(new Dimension(150, 25));
        transperButton.addActionListener(new ActionListener() {
            public void actionPerformed(ActionEvent e) {
                if (currentColors.backColor != null) {
                    currentColors.backColor = null;
                    mainLabel.setIcon(new DialogPicker(100, 100, currentColors));
                }
            }
        });

        JButton okButton = new JButton("OK");
        okButton.setMinimumSize(new Dimension(75, 25));
        okButton.setPreferredSize(new Dimension(75, 25));
        okButton.setMaximumSize(new Dimension(75, 25));
        JButton cancelButton = new JButton("Cancel");
        cancelButton.setMinimumSize(new Dimension(75, 25));
        cancelButton.setPreferredSize(new Dimension(75, 25));
        cancelButton.setMaximumSize(new Dimension(75, 25));
        JPanel buttonsPanel = new JPanel();
        buttonsPanel.add(okButton);
        buttonsPanel.add(cancelButton);

        JButton swapButton = new JButton("Swap Colors");
        swapButton.setMinimumSize(new Dimension(125, 25));
        swapButton.setPreferredSize(new Dimension(125, 25));
        swapButton.setMaximumSize(new Dimension(125, 25));
        swapButton.addActionListener(new ActionListener() {
            public void actionPerformed(ActionEvent e) {
                if (currentColors.backColor != null) {
                    currentColors.swap();
                    mainLabel.setIcon(new DialogPicker(100, 100, currentColors));
                }
            }

        });

        mainLabel.addMouseListener(new MouseListener() {
            public void mousePressed(MouseEvent e) {
                boolean isFore = foreRect.contains(e.getPoint());
                boolean isBack = backRect.contains(e.getPoint());
                if (isFore || isBack) {
                    isBack = isFore && isBack ? false : isBack;
                    final Color color = new JColorChooser(Color.WHITE).showDialog(dialog, "Color chooser", isFore ? currentColors.foreColor : currentColors.backColor);
                    if (color != null) {
                        currentColors.foreColor = isFore ? color : currentColors.foreColor;
                        currentColors.backColor = isBack ? color : currentColors.backColor;
                        mainLabel.setIcon(new DialogPicker(100, 100, currentColors));
                    }
                }
            }

            public void mouseReleased(MouseEvent arg0) {
            }

            public void mouseExited(MouseEvent arg0) {
            }

            public void mouseEntered(MouseEvent arg0) {
            }

            public void mouseClicked(MouseEvent arg0) {
            }
        });

        JPanel controlsPanel = new JPanel();
        controlsPanel.setLayout(new GridBagLayout());
        GridBagConstraints gBC = new GridBagConstraints();
        gBC.fill = GridBagConstraints.NONE;
        gBC.gridx = 0;
        gBC.gridy = 0;
        gBC.anchor = GridBagConstraints.NORTHWEST;
        gBC.insets = new Insets(15, 0, 0, 90);
        controlsPanel.add(helpLabel, gBC);
        gBC.gridx = 0;
        gBC.gridy = 0;
        gBC.anchor = GridBagConstraints.WEST;
        gBC.insets = new Insets(35, 55, 0, 0);
        controlsPanel.add(mainLabel, gBC);

        gBC.gridx = 0;
        gBC.gridy = 0;
        gBC.anchor = GridBagConstraints.NORTHEAST;
        gBC.insets = new Insets(35, 180, 0, 0);
        controlsPanel.add(transperButton, gBC);

        gBC.gridx = 0;
        gBC.gridy = 2;
        gBC.anchor = GridBagConstraints.WEST;
        gBC.insets = new Insets(15, 105, 0, 0);
        controlsPanel.add(swapButton, gBC);

        gBC.gridx = 0;
        gBC.gridy = 3;
        gBC.anchor = GridBagConstraints.EAST;
        gBC.insets = new Insets(5, 0, 0, 15);
        controlsPanel.add(buttonsPanel, gBC);

        okButton.addActionListener(new ActionListener() {
            public void actionPerformed(ActionEvent arg0) {
                dialog.dispose();
            }
        });

        cancelButton.addActionListener(new ActionListener() {
            public void actionPerformed(ActionEvent arg0) {
                dialog.dispose();
            }
        });

        JPanel contentPane = new JPanel();
        contentPane.setLayout(new BorderLayout());
        contentPane.add(controlsPanel, BorderLayout.BEFORE_FIRST_LINE);

        contentPane.setOpaque(true);
        dialog.setContentPane(contentPane);
        dialog.setResizable(false);
        dialog.setSize(new Dimension(350, 300));
        dialog.setModal(true);
        dialog.setVisible(true);

        return currentColors;
    }

    public PropertyDialog propertyDialog(final String title, final String contents, boolean editable) {
        final JDialog dialog = new JDialog(new Frame(), "Annotation Properties");
        final PropertyDialog newProperties = new PropertyDialog();
        final JTextField titleField = new JTextField(20);
        titleField.setText(title);

        final JTextArea contentArea = new JTextArea(contents);
        contentArea.setPreferredSize(new Dimension(260, 100));
        contentArea.setEditable(editable);

        JScrollPane scrollPane = new JScrollPane(contentArea);
        scrollPane.setPreferredSize(new Dimension(280, 120));
        JButton okButton = new JButton("OK");
        okButton.setPreferredSize(new Dimension(75, 25));
        okButton.addActionListener(new ActionListener() {
            public void actionPerformed(ActionEvent arg0) {
                newProperties.setProperties(titleField.getText(), contentArea.getText());
                dialog.dispose();
            }
        });

        JButton cancelButton = new JButton("Cancel");
        cancelButton.setPreferredSize(new Dimension(75, 25));
        cancelButton.addActionListener(new ActionListener() {
            public void actionPerformed(ActionEvent e) {
                dialog.dispose();
            }
        });

        JPanel controlsPanel = new JPanel(new GridBagLayout());
        GridBagConstraints gridConstrains = new GridBagConstraints();
        gridConstrains.fill = GridBagConstraints.VERTICAL;
        gridConstrains.gridx = 0;
        gridConstrains.gridy = 0;
        gridConstrains.ipady = 20;
        gridConstrains.anchor = GridBagConstraints.WEST;
        controlsPanel.add(new JLabel("Title :"), gridConstrains);

        gridConstrains.ipady = 0;
        gridConstrains.gridx = 0;
        gridConstrains.gridy = 1;
        controlsPanel.add(titleField, gridConstrains);

        gridConstrains.ipady = 20;
        gridConstrains.gridx = 0;
        gridConstrains.gridy = 2;
        controlsPanel.add(new JLabel("Contents :"), gridConstrains);

        gridConstrains.ipady = 0;
        gridConstrains.gridx = 0;
        gridConstrains.gridy = 3;
        controlsPanel.add(scrollPane, gridConstrains);

        gridConstrains.gridx = 0;
        gridConstrains.gridy = 4;

        gridConstrains.insets = new Insets(15, 0, 0, 50);
        controlsPanel.add(okButton, gridConstrains);

        gridConstrains.gridx = 0;
        gridConstrains.gridy = 4;
        gridConstrains.anchor = GridBagConstraints.EAST;
        gridConstrains.insets = new Insets(15, 0, 0, 0);
        controlsPanel.add(cancelButton, gridConstrains);

        JPanel contentPane = new JPanel();
        contentPane.setLayout(new BorderLayout());
        contentPane.add(controlsPanel, BorderLayout.BEFORE_FIRST_LINE);

        contentPane.setOpaque(true);
        dialog.setContentPane(contentPane);
        dialog.setResizable(false);
        dialog.setSize(new Dimension(300, 290));
        dialog.setModal(true);
        dialog.setVisible(true);

        return newProperties;
    }

    public String dialog(int type, String title) {
        // Create a file dialog. FileDialog shows a native file open dialog
        // and tends to look nicer than a JFileChooser.
        FileDialog fd = new FileDialog(new Frame(), title, type);
        fd.setDirectory(".");
        // tell the dialog to only accept .pdf files
        fd.setFilenameFilter(new FilenameFilter() {
            final String pdfExt = ".pdf";

            public boolean accept(File dir, String filename) {
                return dir.isDirectory() || filename.toLowerCase().endsWith(pdfExt);
            }
        });

        fd.setVisible(true);
        String path = null;
        if (fd.getFile() != null) {
            path = new File(fd.getDirectory(), fd.getFile()).getAbsolutePath();
        }

        return path;
    }

    public int resolutionDialog(int currentResolution) {
        int resolution = currentResolution;

        final JLabel message = new JLabel("Resolution(DPI). Eneter value between 36-288");
        final JSpinner spinner = new JSpinner(new SpinnerNumberModel(resolution, 36, 288, 1));

        JOptionPane optionPane = new JOptionPane(new Object[] { message, spinner }, JOptionPane.DEFAULT_OPTION, JOptionPane.OK_CANCEL_OPTION);

        JDialog dialog = optionPane.createDialog(view, "Monitor resulotion");
        dialog.addComponentListener(new ComponentAdapter() {
            @Override
            public void componentShown(ComponentEvent e) {
                spinner.requestFocusInWindow();
            }
        });

        dialog.setVisible(true);
        Object value = optionPane.getValue();
        int result = value == null ? JOptionPane.CANCEL_OPTION : (Integer) value;
        dialog.dispose();
        if (result == JOptionPane.OK_OPTION) {
            resolution = (Integer) spinner.getValue();
        }

        return resolution;
    }

    private Component view;
}
