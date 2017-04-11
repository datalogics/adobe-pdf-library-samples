/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer;

import java.awt.BorderLayout;
import java.awt.Dimension;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.event.MouseAdapter;
import java.awt.event.MouseEvent;

import javax.swing.AbstractButton;
import javax.swing.Action;
import javax.swing.ImageIcon;
import javax.swing.JButton;
import javax.swing.JMenu;
import javax.swing.JMenuItem;
import javax.swing.JPopupMenu;

/**
 * MenuButton is used for creation of drop down button (button with pop-up
 * menu).
 * 
 * Drop down button is used for annotation create button, line width and line
 * style buttons. This class contains two buttons - mainButton and
 * dropDownButton. mainButton - can be also used as a button which shows pop-up
 * menu. dropDownButton - it is button with arrow which is used to show pop-up
 * menu.
 * 
 * MenuButton utilizes DropDownButtonModel to emulate behavior of drop down
 * button.
 */
public class MenuButton extends AbstractButton {
    public static class ClickableMenu extends JMenu {
        public ClickableMenu() {
            super();
            hasAction = true;
            addListener();
        }

        public ClickableMenu(Action action) {
            super(action);
            hasAction = true;
            addListener();
        }

        public ClickableMenu(String title) {
            super(title);
            hasAction = true;
            addListener();
        }

        public ClickableMenu(String title, boolean b) {
            super(title, b);
            hasAction = b;
            addListener();
        }

        private void addListener() {
            addMouseListener(new MouseAdapter() {
                @Override
                public void mouseClicked(MouseEvent e) {
                    if (hasAction) {
                        ActionEvent event = new ActionEvent(e.getSource(), ActionEvent.ACTION_PERFORMED, null);

                        for (ActionListener listener : getActionListeners()) {
                            listener.actionPerformed(event);
                        }
                        if (getAction() != null) {
                            getAction().actionPerformed(event);
                        }
                    }
                }

            });
        }

        private boolean hasAction;
    }

    public MenuButton(boolean useDropButton, Dimension buttonSize) {
        this(null, useDropButton, buttonSize);
    }

    public MenuButton(String text, boolean useDropButton, Dimension buttonSize) {
        super();
        this.useDropButton = useDropButton;
        this.mainButton = new JButton();
        this.mainButton.setPreferredSize(buttonSize);
        this.mainButton.setMinimumSize(buttonSize);
        this.mainButton.setMaximumSize(buttonSize);

        this.mainButton.setText(text);

        this.dropDownButton = new DropDown();
        final Dimension dropDownSize = new Dimension((int) (buttonSize.width / 100.0f * 25), buttonSize.height);
        this.dropDownButton.setIcon(new ImageIcon(getClass().getResource("/com/datalogics/PDFL/JavaViewer/Resources/TriangleArrow.gif")));
        this.dropDownButton.setPreferredSize(dropDownSize);
        this.dropDownButton.setMinimumSize(dropDownSize);
        this.dropDownButton.setMaximumSize(dropDownSize);

        this.setModel(new DropDownButtonModel(mainButton.getModel(), dropDownButton.getModel()));

        this.setLayout(new BorderLayout());
        this.add(mainButton, BorderLayout.CENTER);
        this.add(dropDownButton, BorderLayout.EAST);

        if (!this.useDropButton) {
            mainButton.addActionListener(new ActionListener() {
                public void actionPerformed(ActionEvent e) {
                    showDropDownMenu();
                }
            });
        }
    }

    public void setActiveMenuItem(int index) {
        selectedMenuItem = index;
        initFromItem((JMenuItem) dropDownMenu.getComponent(selectedMenuItem));
    }

    public void updateDropDownState() {
        dropDownButton.setEnabled(mainButton.getAction().isEnabled());
    }

    public int getActiveMenuItem() {
        return selectedMenuItem;
    }

    public void setMenu(JPopupMenu menu) {
        setMenu(menu, null);
    }

    public void setMenu(JPopupMenu menu, Action action) {
        this.dropDownMenu = menu;
        for (int i = 0; i < menu.getComponentCount(); i++) {
            JMenuItem item = (JMenuItem) menu.getComponent(i);
            item.addActionListener(new ActionListener() {
                public void actionPerformed(ActionEvent e) {
                    final JMenuItem eventSource = (JMenuItem) e.getSource();
                    initFromItem(eventSource);
                }
            });

            // first-time initialization
            if (i == 0) {
                initFromItem(action == null ? item : new JMenuItem(action));
            }
        }

        // set proper button's size
        this.setPreferredSize(mainButton.getPreferredSize());
        this.setMaximumSize(mainButton.getPreferredSize());
        this.validate();
    }

    public JButton getMainButton() {
        return mainButton;
    }

    public JButton getDropDownButton() {
        return dropDownButton;
    }

    public JPopupMenu getMenu() {
        return dropDownMenu;
    }

    private void initFromItem(JMenuItem item) {
        mainButton.setIcon(item.getIcon());
        if (useDropButton) {
            dropDownButton.setEnabled(item.getAction().isEnabled());
            dropDownButton.setText(null);
            mainButton.setAction(item.getAction());
            mainButton.setText(null);
        } else {
            this.setEnabled(item.getAction().isEnabled());
        }
    }

    private class DropDown extends JButton {
        @Override
        protected void fireActionPerformed(ActionEvent event) {
            showDropDownMenu();
        }

    }

    private void showDropDownMenu() {
        if (dropDownMenu != null)
            dropDownMenu.show(this, 0, this.getHeight());
    }

    private JButton mainButton;
    private JButton dropDownButton;
    private JPopupMenu dropDownMenu;
    private boolean useDropButton;

    private int selectedMenuItem;
}
