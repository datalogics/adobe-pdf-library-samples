/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views;

/*
 * LayersView - it is an implementation of Layer interface. It makes layers
 * panel visible or invisible, and updates layers state.
 */

import java.awt.GridLayout;
import java.awt.event.ItemEvent;
import java.awt.event.ItemListener;
import java.util.HashMap;
import java.util.Map;

import javax.swing.AbstractButton;
import javax.swing.ImageIcon;
import javax.swing.JCheckBox;
import javax.swing.JComponent;
import javax.swing.JScrollPane;
import javax.swing.JTabbedPane;

import com.datalogics.PDFL.JavaViewer.Presentation.LayersPresenter.LayerInfo;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.Layers;

public class LayersView extends JComponent implements Layers {
    public LayersView(JTabbedPane tabbedPane) {
        this.tabbedPane = tabbedPane;
        this.scrollable = new JScrollPane(this);
        this.showCheckBoxMap = new HashMap<LayerInfo, AbstractButton>();
        this.printCheckBoxMap = new HashMap<LayerInfo, AbstractButton>();

        setLayout(new GridLayout(0, 2));
    }

    public void setLayersVisible(boolean visible) {
        final boolean isVisible = (tabbedPane.indexOfComponent(this.scrollable) != -1);
        if (!visible && isVisible)
            tabbedPane.removeTabAt(tabbedPane.indexOfComponent(this.scrollable));
        else if (visible && !isVisible) {
            tabbedPane.addTab("Layers", this.scrollable);
        }
    }

    public void updateLayer(Object info) {
        final LayerInfo layerInfo = (LayerInfo) info;
        final String resourcePath = "/com/datalogics/PDFL/JavaViewer/Resources/";
        if (!this.showCheckBoxMap.containsKey(layerInfo)) {
            JCheckBox showCheckBox = new JCheckBox(layerInfo.toString(), new ImageIcon(getClass().getResource(resourcePath + "CrossEyeCheckbox.gif")));
            showCheckBox.setSelectedIcon(new ImageIcon(getClass().getResource(resourcePath + "EyeCheckbox.gif")));
            this.add(showCheckBox);
            this.showCheckBoxMap.put(layerInfo, showCheckBox);

            showCheckBox.addItemListener(new ItemListener() {
                public void itemStateChanged(ItemEvent e) {
                    if (showCheckBoxMap.get(layerInfo).isSelected() != layerInfo.getShowState())
                        layerInfo.setShowState(showCheckBoxMap.get(layerInfo).isSelected());
                }
            });
        }

        if (!this.printCheckBoxMap.containsKey(layerInfo)) {
            JCheckBox printCheckBox = new JCheckBox(layerInfo.toString(), new ImageIcon(getClass().getResource(resourcePath + "CrossPrintMenu.gif")));
            printCheckBox.setSelectedIcon(new ImageIcon(getClass().getResource(resourcePath + "PrintMenu2.gif")));
            this.add(printCheckBox);
            this.printCheckBoxMap.put(layerInfo, printCheckBox);

            printCheckBox.addItemListener(new ItemListener() {
                public void itemStateChanged(ItemEvent e) {
                    if (printCheckBoxMap.get(layerInfo).isSelected() != layerInfo.getPrintState())
                        layerInfo.setPrintState(printCheckBoxMap.get(layerInfo).isSelected());
                }
            });
        }

        this.showCheckBoxMap.get(layerInfo).setSelected(layerInfo.getShowState());
        this.printCheckBoxMap.get(layerInfo).setSelected(layerInfo.getPrintState());
    }

    public void clearLayers() {
        this.removeAll();

        showCheckBoxMap.clear();
        printCheckBoxMap.clear();
    }

    private JTabbedPane tabbedPane;
    private JScrollPane scrollable;

    private Map<LayerInfo, AbstractButton> showCheckBoxMap;
    private Map<LayerInfo, AbstractButton> printCheckBoxMap;
}
