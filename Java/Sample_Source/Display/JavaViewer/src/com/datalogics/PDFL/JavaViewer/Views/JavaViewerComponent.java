/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views;

import java.awt.BorderLayout;
import java.awt.Color;
import java.awt.Component;
import java.awt.Dimension;
import java.awt.Frame;
import java.awt.GraphicsEnvironment;
import java.awt.Point;
import java.awt.Toolkit;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.event.HierarchyEvent;
import java.awt.event.HierarchyListener;
import java.awt.event.MouseAdapter;
import java.awt.event.MouseEvent;
import java.lang.reflect.InvocationTargetException;
import java.util.Arrays;
import java.util.EnumSet;
import java.util.HashMap;
import java.util.List;

import javax.swing.AbstractAction;
import javax.swing.Action;
import javax.swing.BorderFactory;
import javax.swing.ButtonGroup;
import javax.swing.JButton;
import javax.swing.JCheckBoxMenuItem;
import javax.swing.JComboBox;
import javax.swing.JComponent;
import javax.swing.JFrame;
import javax.swing.JLabel;
import javax.swing.JMenu;
import javax.swing.JMenuBar;
import javax.swing.JMenuItem;
import javax.swing.JPopupMenu;
import javax.swing.JSplitPane;
import javax.swing.JTabbedPane;
import javax.swing.JTextField;
import javax.swing.JToolBar;
import javax.swing.SwingConstants;
import javax.swing.SwingUtilities;
import javax.swing.Timer;
import javax.swing.event.ChangeEvent;
import javax.swing.event.ChangeListener;
import javax.swing.plaf.basic.BasicArrowButton;

import com.datalogics.PDFL.HorizontalAlignment;
import com.datalogics.PDFL.LineEndingStyle;
import com.datalogics.PDFL.JavaViewer.MenuButton;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationConsts;
import com.datalogics.PDFL.JavaViewer.Presentation.ApplicationController;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.ApplicationMode;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.DialogMenuItem;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.MenuType;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.NavigateMode;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.PageViewMode;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.ZoomMode;
import com.datalogics.PDFL.JavaViewer.Views.Actions.AnnotationPropertyAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.AppendAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.ApplicationModeAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.AutosizeVerticallyAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.AvailableActions;
import com.datalogics.PDFL.JavaViewer.Views.Actions.ChangeAnnotationColor;
import com.datalogics.PDFL.JavaViewer.Views.Actions.ChangeArrowHead;
import com.datalogics.PDFL.JavaViewer.Views.Actions.ChangeDashPatternAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.ChangeLineWidthAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.CloseAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.CreateAnnotationAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.DeleteAnnotationAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.DialogMenuAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.FontAlignmentChangedAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.FontFaceChangedAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.FontSizeChangedAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.MonitorResolutionAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.NavigateAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.OpenAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.PrintAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.RedoAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.SaveAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.ShowBorderAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.SimpleAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.TextSearchAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.UndoAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.UnlockAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.ViewModeAction;
import com.datalogics.PDFL.JavaViewer.Views.Actions.ZoomAction;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.Bookmarks;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.ColorPicker;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.DialogMenuResultNotifier;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.Dialogs;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.Layers;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.PDF;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.Viewer;

public class JavaViewerComponent extends JComponent implements HierarchyListener, Viewer {
    public JavaViewerComponent(ApplicationController appController) {
        this.appController = appController;
        appController.setAvailableFonts(availableFonts);
        this.addHierarchyListener(this);
    }

    public void hierarchyChanged(HierarchyEvent e) {
        if (e.getID() == HierarchyEvent.HIERARCHY_CHANGED && (e.getChangeFlags() & HierarchyEvent.PARENT_CHANGED) != 0 && e.getComponent().getParent() != null) {
            Component parent = SwingUtilities.getRoot(e.getChanged());
            this.removeHierarchyListener(this);
            initComponent(parent);
        }
    }

    public boolean close() {
        return appController.close();
    }

    public void updateTitle(String title) {
        try {
            Frame frame = (Frame) SwingUtilities.getRoot(this);
            frame.setTitle("JavaViewer" + (title.length() == 0 ? "" : " - " + title));
        } catch (ClassCastException e) {
        }
    }

    public Dialogs getDialogs() {
        return dialogsView;
    }

    public PDF getPDF() {
        return pdfView;
    }

    public Bookmarks getBookmarks() {
        return bookmarksView;
    }

    public Layers getLayers() {
        return layersView;
    }

    public void updateControls() {
        for (AvailableActions actionKey : actions.keySet()) {
            AbstractAction action = getAction(actionKey);
            action.setEnabled(((SimpleAction) action).isPermitted());
        }
    }

    public void updateLineWidthButton(float width) {
        final int index = Arrays.binarySearch(lineWidths, width);
        lineWidthButton.setActiveMenuItem(index >= 0 ? index : 4);
    }

    public void updateLineStyleButton(float[] dashPattern) {
        int index = -1;
        if (dashPattern != null) {
            for (int i = 0, len = dashPatterns.length; i < len; ++i) {
                if (Arrays.equals(dashPattern, dashPatterns[i])) {
                    index = i;
                    break;
                }
            }
        }
        lineStyleButton.setActiveMenuItem(index + 1);
    }

    public void updateColorButton(ColorPicker colors) {
        final Action action = colorPickerButton.getAction();
        action.putValue(Action.SMALL_ICON, ((SimpleAction) action).isPermitted() ? new ColorPickerIcon(BUTTON_HEIGHT, BUTTON_HEIGHT, colors) : new ColorPickerIcon(BUTTON_HEIGHT, BUTTON_HEIGHT));
    }

    public void updateAnnotationButton(String subType) {
        AvailableActions action = null;
        for (AvailableActions act : AvailableActions.values()) {
            Object obj = actions.get(act);
            if (obj instanceof CreateAnnotationAction) {
                if (((CreateAnnotationAction) obj).getSubType().equals(subType)) {
                    action = act;
                    break;
                }
            }
        }
        if (action != null) {
            annotationButton.setActiveMenuItem(searchInArray(action.ordinal(), annotationOrder));
        } else {
            annotationButton.setActiveMenuItem(0);
        }
    }

    public void updateFontProperty(String fonface, int fontSize, HorizontalAlignment alignment) {
        final int fontsCount = availableFonts.size();
        int fromIndex = availableFonts.indexOf(fonface) == -1 ? 0 : availableFonts.indexOf(fonface);
        final boolean canScroll = fontsMenuCount + fromIndex <= fontsCount;
        int fontSelectedIndex = canScroll ? 1 : ((fromIndex - fontsMenuCount) + (fontsCount - fontsMenuCount)) + 1;
        fromIndex = canScroll ? fromIndex : fontsCount - fontsMenuCount;

        List<String> fontsSubList = availableFonts.subList(fromIndex, fontsMenuCount + fromIndex);
        JMenuItem item = null;

        scrollUp.setEnabled(false);
        fontsMenu.removeAll();
        fontsMenu.add(scrollUp);
        scrollUp.setEnabled(true);
        for (int i = 0; i < fontsMenuCount; ++i) {
            item = new JCheckBoxMenuItem(fontsSubList.get(i));
            item.addActionListener(getAction(AvailableActions.ACTION_FONT_FACE));
            fontsMenu.add(item);
            familyGroup.add(item);
        }
        fontsMenu.add(scrollDown);
        fontsMenu.revalidate();
        fontsMenu.getItem(fontSelectedIndex).setSelected(true);

        fontSizeMenu.removeAll();
        for (int size : fontSizes) {
            item = new JCheckBoxMenuItem(String.valueOf(size));
            item.addActionListener(getAction(AvailableActions.ACTION_FONT_SIZE));
            fontSizeMenu.add(item);
            sizeGroup.add(item);
        }

        fontSizeMenu.getItem(fontSizes.indexOf(fontSize) != -1 ? fontSizes.indexOf(fontSize) : 2).setSelected(true);

        fontJustifyMenu.removeAll();
        for (String align : fontAlignment) {
            item = new JCheckBoxMenuItem(align);
            item.addActionListener(getAction(AvailableActions.ACTION_FONT_JUSTIFICATION));
            fontJustifyMenu.add(item);
            justifyGroup.add(item);
        }

        int index = -1;
        for (int i = 0; i < fontAlignment.size(); ++i) {
            index = fontAlignment.get(i).equalsIgnoreCase(alignment.name()) ? i : index;
        }

        fontJustifyMenu.getItem(index == -1 ? 0 : index).setSelected(true);

        freeTextAnnotationButton.updateDropDownState();
    }

    public void updateArrowMenus(String subType, LineEndingStyle startStyle, LineEndingStyle endStyle) {
        int startNum = searchInArray(startStyle.ordinal(), arrowType);
        int endNum = searchInArray(endStyle.ordinal(), arrowType);

        final boolean isLine = subType.equals(AnnotationConsts.Subtypes.LINE);
        final boolean allOther = !(subType.equals(AnnotationConsts.Subtypes.LINE) || subType.equals(AnnotationConsts.Subtypes.POLYLINE));

        startArrowMenu.getItem(isLine || allOther ? startNum : 0).setSelected(true);
        endArrowMenu.getItem(isLine || allOther ? endNum : 0).setSelected(true);

        startPolyMenu.getItem(!isLine || allOther ? startNum : 0).setSelected(true);
        endPolyMenu.getItem(!isLine || allOther ? endNum : 0).setSelected(true);
    }

    public void showContextMenu(MenuType menuType, Point location) {
        JPopupMenu contextMenu = new JPopupMenu();
        addMenu(JMenuItem.class, contextMenu, AvailableActions.ACTION_ANNOTATION_PROPERTIES, false);
        addMenu(JMenuItem.class, contextMenu, AvailableActions.ACTION_DELETE_ANNOTATION, false);
        contextMenu.addSeparator();
        addMenu(JMenuItem.class, contextMenu, AvailableActions.ACTION_MENU_COLOR, false);

        JMenu menu = null;
        if (menuType.isRectangular() || menuType.isLine() || menuType.isPolyLine() || menuType.isLink()) {
            menu = new MenuButton.ClickableMenu("Line width");
            createLineWidthMenu(menu, JCheckBoxMenuItem.class, false);
            menu.getItem(lineWidthButton.getActiveMenuItem()).setSelected(true);
            contextMenu.add(menu);

            menu = new MenuButton.ClickableMenu("Line style");
            createLineStyleMenu(menu, JCheckBoxMenuItem.class, false);
            menu.getItem(lineStyleButton.getActiveMenuItem()).setSelected(true);
            contextMenu.add(menu);
        }

        if (menuType.isFreeText()) {
            contextMenu.addSeparator();
            addMenu(JMenuItem.class, contextMenu, AvailableActions.ACTION_AUTOSIZE, false);
        }

        if (menuType.isLink()) {
            contextMenu.addSeparator();
            JCheckBoxMenuItem item = new JCheckBoxMenuItem(getAction(AvailableActions.ACTION_BORDER));
            item.setSelected(appController.getActiveProps().getShowBorder());
            contextMenu.add(item);
        }

        final Point pageOffset = pdfView.getViewOrigin();
        contextMenu.show(pdfView, location.x - pageOffset.x, location.y - pageOffset.y);
    }

    public void showDialogMenu(EnumSet<DialogMenuItem> items, final DialogMenuResultNotifier subscriber, Point location) {
        final JPopupMenu contextMenu = new JPopupMenu();
        for (final Enum<DialogMenuItem> item : items) {
            final DialogMenuItem dialogMenuItem = DialogMenuItem.valueOf(item.name());
            final JMenuItem menuItem = new JMenuItem(new DialogMenuAction(dialogMenuItem, subscriber, dialogMenuItem.getName()));

            contextMenu.add(menuItem);
        }

        final Point pageOffset = pdfView.getViewOrigin();
        contextMenu.show(pdfView, location.x - pageOffset.x, location.y - pageOffset.y);
    }

    public void updateZoom(ZoomMode mode, double zoom) {
        boolean enabled = true;
        if (mode.isDirectMode()) {
            final int index = ZoomMode.getArrayPos(zoom, zoomArray, true);
            if (index != -1) {
                zoomList.setSelectedIndex(index);
            } else {
                zoomList.setSelectedItem(String.valueOf((int) (zoom * 100.0)) + "%");
            }
        } else if (mode.isFitMode()) {
            AvailableActions fitAction = null;
            switch (mode) {
            case ZOOM_FIT_PAGE:
                fitAction = AvailableActions.ACTION_ZOOM_FIT_PAGE;
                break;
            case ZOOM_FIT_WIDTH:
                fitAction = AvailableActions.ACTION_ZOOM_FIT_WIDTH;
                break;
            case ZOOM_FIT_HEIGHT:
                fitAction = AvailableActions.ACTION_ZOOM_FIT_HEIGHT;
                break;
            }
            if (fitAction != null)
                zoomList.setSelectedItem(getAction(fitAction));
        } else
            enabled = false;

        zoomList.setEnabled(enabled);
        getAction(AvailableActions.ACTION_ZOOM_DEC).setEnabled(enabled && zoom > zoomArray[0]);
        getAction(AvailableActions.ACTION_ZOOM_INC).setEnabled(enabled && zoom < zoomArray[zoomArray.length - 1]);
        this.requestFocus();
    }

    public void updatePage(int index, int count) {
        final boolean enabled = (index != -1);
        pageNumber.setText(String.valueOf(index + 1));
        pageCount.setText("of " + String.valueOf(count));

        pageNumber.setEnabled(enabled);
        getAction(AvailableActions.ACTION_FIRST_PAGE).setEnabled(enabled && index > 0);
        getAction(AvailableActions.ACTION_PREV_PAGE).setEnabled(enabled && index > 0);
        getAction(AvailableActions.ACTION_NEXT_PAGE).setEnabled(enabled && index < count - 1);
        getAction(AvailableActions.ACTION_LAST_PAGE).setEnabled(enabled && index < count - 1);
        this.requestFocus();
    }

    public void updateViewMode(PageViewMode mode) {
        getAction(AvailableActions.ACTION_SINGLE_PAGE_MODE).putValue(SimpleAction.SELECTED, mode.isSinglePage());
        getAction(AvailableActions.ACTION_CONTINUOUS_PAGE_MODE).putValue(SimpleAction.SELECTED, mode.isContinuousPage());
    }

    public void updateUnlock(boolean state) {
        getAction(AvailableActions.ACTION_UNLOCK).setEnabled(state);
    }

    public void updateApplicationMode(ApplicationMode applicationMode) {
        getAction(AvailableActions.ACTION_DEFAULT_MODE).putValue(SimpleAction.SELECTED, applicationMode.equals(appController.getDefaultMode()));
        getAction(AvailableActions.ACTION_ZOOM_MODE).putValue(SimpleAction.SELECTED, applicationMode.equals(ApplicationMode.ZOOM_MODE));
        getAction(AvailableActions.ACTION_MARQUEE_ZOOM_MODE).putValue(SimpleAction.SELECTED, applicationMode.equals(ApplicationMode.MARQUEE_ZOOM_MODE));
    }

    public void updateUndoRedoButtons(boolean hasUndo, boolean hasRedo) {
        getAction(AvailableActions.ACTION_UNDO).setEnabled(hasUndo);
        getAction(AvailableActions.ACTION_REDO).setEnabled(hasRedo);
    }

    public AbstractAction getAction(AvailableActions key) {
        return actions.get(key);
    }

    private int searchInArray(int what, int[] array) {
        int pos = -1;
        for (int i = 0, len = array.length; i < len && pos == -1; ++i) {
            pos = array[i] == what ? i : pos;
        }
        return pos == -1 ? 0 : pos;
    }

    private void initActions(Component topParent) {
        actions = new HashMap<AvailableActions, AbstractAction>(AvailableActions.values().length);
        actions.put(AvailableActions.ACTION_OPEN, new OpenAction());
        actions.put(AvailableActions.ACTION_SAVE, new SaveAction(false));
        actions.put(AvailableActions.ACTION_SAVE_AS, new SaveAction(true));
        actions.put(AvailableActions.ACTION_APPEND, new AppendAction());
        actions.put(AvailableActions.ACTION_UNLOCK, new UnlockAction());
        actions.put(AvailableActions.ACTION_PRINT, new PrintAction());
        actions.put(AvailableActions.ACTION_CLOSE, new CloseAction());

        actions.put(AvailableActions.ACTION_ZOOM_INC, new ZoomAction(zoomArray, true));
        actions.put(AvailableActions.ACTION_ZOOM_DEC, new ZoomAction(zoomArray, false));

        actions.put(AvailableActions.ACTION_ZOOM_FIT_PAGE, new ZoomAction(true, true));
        actions.put(AvailableActions.ACTION_ZOOM_FIT_WIDTH, new ZoomAction(true, false));
        actions.put(AvailableActions.ACTION_ZOOM_FIT_HEIGHT, new ZoomAction(false, true));

        actions.put(AvailableActions.ACTION_NEXT_PAGE, new NavigateAction(NavigateMode.NAVIGATE_NEXT));
        actions.put(AvailableActions.ACTION_PREV_PAGE, new NavigateAction(NavigateMode.NAVIGATE_PREV));
        actions.put(AvailableActions.ACTION_LAST_PAGE, new NavigateAction(NavigateMode.NAVIGATE_LAST));
        actions.put(AvailableActions.ACTION_FIRST_PAGE, new NavigateAction(NavigateMode.NAVIGATE_FIRST));

        actions.put(AvailableActions.ACTION_SINGLE_PAGE_MODE, new ViewModeAction(PageViewMode.PAGE_MODE_SINGLE));
        actions.put(AvailableActions.ACTION_CONTINUOUS_PAGE_MODE, new ViewModeAction(PageViewMode.PAGE_MODE_CONTINUOUS));

        actions.put(AvailableActions.ACTION_DEFAULT_MODE, new ApplicationModeAction());
        actions.put(AvailableActions.ACTION_ZOOM_MODE, new ApplicationModeAction(ApplicationMode.ZOOM_MODE));
        actions.put(AvailableActions.ACTION_MARQUEE_ZOOM_MODE, new ApplicationModeAction(ApplicationMode.MARQUEE_ZOOM_MODE));

        actions.put(AvailableActions.ACTION_TEXTSEARCH, new TextSearchAction());

        actions.put(AvailableActions.ACTION_CIRCLE_ANNOTATION, new CreateAnnotationAction(AnnotationConsts.Subtypes.CIRCLE));
        actions.put(AvailableActions.ACTION_LINE_ANNOTATION, new CreateAnnotationAction(AnnotationConsts.Subtypes.LINE));
        actions.put(AvailableActions.ACTION_ARROW_ANNOTATION, new CreateAnnotationAction(AnnotationConsts.Subtypes.ARROW));
        actions.put(AvailableActions.ACTION_SQUARE_ANNOTATION, new CreateAnnotationAction(AnnotationConsts.Subtypes.SQUARE));
        actions.put(AvailableActions.ACTION_POLYLINE_ANNOTATION, new CreateAnnotationAction(AnnotationConsts.Subtypes.POLYLINE));
        actions.put(AvailableActions.ACTION_POLYGON_ANNOTATION, new CreateAnnotationAction(AnnotationConsts.Subtypes.POLYGON));
        actions.put(AvailableActions.ACTION_FREETEXT_ANNOTATION, new CreateAnnotationAction(AnnotationConsts.Subtypes.FREETEXT));
        actions.put(AvailableActions.ACTION_INK_ANNOTATION, new CreateAnnotationAction(AnnotationConsts.Subtypes.INK));
        actions.put(AvailableActions.ACTION_LINK_ANNOTATION, new CreateAnnotationAction(AnnotationConsts.Subtypes.LINK));
        actions.put(AvailableActions.ACTION_HIGHLIGHT_ANNOTATION, new CreateAnnotationAction(AnnotationConsts.Subtypes.HIGHLIGHT));
        actions.put(AvailableActions.ACTION_UNDERLINE_ANNOTATION, new CreateAnnotationAction(AnnotationConsts.Subtypes.UNDERLINE));
        actions.put(AvailableActions.ACTION_UNDO, new UndoAction());
        actions.put(AvailableActions.ACTION_REDO, new RedoAction());

        actions.put(AvailableActions.ACTION_FONT_FACE, new FontFaceChangedAction());
        actions.put(AvailableActions.ACTION_FONT_SIZE, new FontSizeChangedAction());
        actions.put(AvailableActions.ACTION_FONT_JUSTIFICATION, new FontAlignmentChangedAction());

        actions.put(AvailableActions.ACTION_LINE_STYLE_SOLID, new ChangeDashPatternAction());
        actions.put(AvailableActions.ACTION_LINE_STYLE_DASHED1, new ChangeDashPatternAction(dashPatterns[0]));
        actions.put(AvailableActions.ACTION_LINE_STYLE_DASHED2, new ChangeDashPatternAction(dashPatterns[1]));
        actions.put(AvailableActions.ACTION_LINE_STYLE_DASHED3, new ChangeDashPatternAction(dashPatterns[2]));
        actions.put(AvailableActions.ACTION_LINE_STYLE_DASHED4, new ChangeDashPatternAction(dashPatterns[3]));

        actions.put(AvailableActions.ACTION_LINE_WIDTH1, new ChangeLineWidthAction(lineWidths[0]));
        actions.put(AvailableActions.ACTION_LINE_WIDTH2, new ChangeLineWidthAction(lineWidths[1]));
        actions.put(AvailableActions.ACTION_LINE_WIDTH3, new ChangeLineWidthAction(lineWidths[2]));
        actions.put(AvailableActions.ACTION_LINE_WIDTH5, new ChangeLineWidthAction(lineWidths[3]));
        actions.put(AvailableActions.ACTION_LINE_WIDTH_OTHER, new ChangeLineWidthAction(lineWidths[4]));

        actions.put(AvailableActions.ACTION_ANNOTATION_COLOR, new ChangeAnnotationColor());
        actions.put(AvailableActions.ACTION_MENU_COLOR, new ChangeAnnotationColor());

        actions.put(AvailableActions.ACTION_START_ARROW_NONE, new ChangeArrowHead(LineEndingStyle.NONE, true));
        actions.put(AvailableActions.ACTION_START_ARROW_OPEN, new ChangeArrowHead(LineEndingStyle.OPEN_ARROW, true));
        actions.put(AvailableActions.ACTION_START_ARROW_CLOSED, new ChangeArrowHead(LineEndingStyle.CLOSED_ARROW, true));
        actions.put(AvailableActions.ACTION_START_ARROW_CIRCLE, new ChangeArrowHead(LineEndingStyle.CIRCLE, true));

        actions.put(AvailableActions.ACTION_END_ARROW_NONE, new ChangeArrowHead(LineEndingStyle.NONE, false));
        actions.put(AvailableActions.ACTION_END_ARROW_OPEN, new ChangeArrowHead(LineEndingStyle.OPEN_ARROW, false));
        actions.put(AvailableActions.ACTION_END_ARROW_CLOSED, new ChangeArrowHead(LineEndingStyle.CLOSED_ARROW, false));
        actions.put(AvailableActions.ACTION_END_ARROW_CIRCLE, new ChangeArrowHead(LineEndingStyle.CIRCLE, false));

        actions.put(AvailableActions.ACTION_DELETE_ANNOTATION, new DeleteAnnotationAction());
        actions.put(AvailableActions.ACTION_ANNOTATION_PROPERTIES, new AnnotationPropertyAction());

        actions.put(AvailableActions.ACTION_AUTOSIZE, new AutosizeVerticallyAction());
        actions.put(AvailableActions.ACTION_BORDER, new ShowBorderAction());

        actions.put(AvailableActions.ACTION_RESOLUTION, new MonitorResolutionAction());

        for (AvailableActions act : AvailableActions.values()) {
            SimpleAction action = (SimpleAction) getAction(act);
            if (action != null) {
                action.setApplication(appController);
                action.putValue(AbstractAction.NAME, act.getName());
                action.putValue(AbstractAction.SMALL_ICON, act.getIconResource());
                action.putValue(AbstractAction.ACCELERATOR_KEY, act.getAccelerator());
            }
        }
    }

    private void initComponent(Component topParent) {
        this.dialogsView = new DialogsView(topParent);

        initActions(topParent);
        setLayout(new BorderLayout());

        addMenuBar(topParent);
        addToolbar();
        addSplitter();

        this.appController.setView(this);
        this.appController.getPDFPresenter().setZoomBounds(zoomArray[0], zoomArray[zoomArray.length - 1]);
        this.appController.setZoomArray(zoomArray);
        this.appController.setDpi(Toolkit.getDefaultToolkit().getScreenResolution());
    }

    private void addSplitter() {
        final JSplitPane splitPane = new JSplitPane(JSplitPane.HORIZONTAL_SPLIT);
        final JTabbedPane tabbedPane = new JTabbedPane();
        bookmarksView = new BookmarksView(appController.getBookmarksPresenter(), tabbedPane);
        layersView = new LayersView(tabbedPane);

        final ChangeListener changeListener = new ChangeListener() {
            public void stateChanged(ChangeEvent e) {
                if (tabbedPane.getSelectedIndex() == -1 && !Boolean.FALSE.equals(hasTabs)) {
                    remove(splitPane);
                    add(pdfView, BorderLayout.CENTER);
                    pdfView.setBorder(BorderFactory.createLoweredBevelBorder());
                    hasTabs = false;
                } else if (!Boolean.TRUE.equals(hasTabs)) {
                    remove(pdfView);
                    splitPane.setRightComponent(pdfView);
                    add(splitPane, BorderLayout.CENTER);
                    pdfView.setBorder(null);
                    hasTabs = true;
                }
                bookmarLayerMenu.setSelected(hasTabs);
                validate();
                doLayout();
            }

            private Boolean hasTabs; // initial value is null to make proper initialization
        };
        splitPane.setLeftComponent(tabbedPane);
        pdfView = new PDFView(appController);

        tabbedPane.addChangeListener(changeListener);
        changeListener.stateChanged(new ChangeEvent(tabbedPane)); // perform initial re-layout of split divider
    }

    private void addToolbar() {
        JToolBar toolBar = new JToolBar();
        toolBar.setPreferredSize(new Dimension(toolBar.getPreferredSize().width, BUTTON_HEIGHT));
        toolBar.setFloatable(false);

        // configure separator dimensions
        final int separatorWidth = 5;
        final int separatorHeight = toolBar.getPreferredSize().height;

        addButton(toolBar, AvailableActions.ACTION_OPEN);
        addButton(toolBar, AvailableActions.ACTION_SAVE);
        addButton(toolBar, AvailableActions.ACTION_PRINT);
        addSeparator(toolBar, separatorWidth, separatorHeight);

        addButton(toolBar, AvailableActions.ACTION_FIRST_PAGE);
        addButton(toolBar, AvailableActions.ACTION_PREV_PAGE);
        addPageFields(toolBar);
        addButton(toolBar, AvailableActions.ACTION_NEXT_PAGE);
        addButton(toolBar, AvailableActions.ACTION_LAST_PAGE);
        addSeparator(toolBar, separatorWidth, separatorHeight);

        addButton(toolBar, AvailableActions.ACTION_ZOOM_DEC);
        addZoomList(toolBar);
        addButton(toolBar, AvailableActions.ACTION_ZOOM_INC);
        addSeparator(toolBar, separatorWidth, separatorHeight);

        addFreeTextDropDownButton(toolBar);
        addSeparator(toolBar, separatorWidth, separatorHeight);
        addAnnotationDropDownButton(toolBar);
        addSeparator(toolBar, separatorWidth, separatorHeight);
        addLineStyleDropDownButton(toolBar);
        addSeparator(toolBar, separatorWidth, separatorHeight);
        addLineWidthDropDownButton(toolBar);
        addSeparator(toolBar, separatorWidth, separatorHeight);
        addColorPickerButton(toolBar, AvailableActions.ACTION_ANNOTATION_COLOR);

        addSeparator(toolBar, separatorWidth, separatorHeight);
        addButton(toolBar, AvailableActions.ACTION_DEFAULT_MODE);
        addButton(toolBar, AvailableActions.ACTION_ZOOM_MODE);
        addButton(toolBar, AvailableActions.ACTION_MARQUEE_ZOOM_MODE);

        addSearchControls(toolBar, AvailableActions.ACTION_TEXTSEARCH);

        addButton(toolBar, AvailableActions.ACTION_UNDO);
        addButton(toolBar, AvailableActions.ACTION_REDO);
        this.add(toolBar, BorderLayout.NORTH);
    }

    private void addMenuBar(Component component) {
        if (component instanceof JFrame) {
            JFrame frame = (JFrame) component;

            JMenuBar jm = new JMenuBar();
            JMenu jmFile = addMenu(JMenu.class, jm, "File");
            JMenu jmEdit = addMenu(JMenu.class, jm, "Edit");
            JMenu jmOptions = addMenu(JMenu.class, jm, "Options");

            addMenu(JMenuItem.class, jmFile, AvailableActions.ACTION_OPEN, false);
            addMenu(JMenuItem.class, jmFile, AvailableActions.ACTION_SAVE, false);
            addMenu(JMenuItem.class, jmFile, AvailableActions.ACTION_SAVE_AS, false);

            jmFile.addSeparator();
            addMenu(JMenuItem.class, jmFile, AvailableActions.ACTION_APPEND, false);
            addMenu(JMenuItem.class, jmFile, AvailableActions.ACTION_PRINT, false);
            jmFile.addSeparator();
            addMenu(JMenuItem.class, jmFile, AvailableActions.ACTION_UNLOCK, false);
            jmFile.addSeparator();
            addMenu(JMenuItem.class, jmFile, AvailableActions.ACTION_CLOSE, false);

            addMenu(JMenuItem.class, jmEdit, AvailableActions.ACTION_UNDO, false);
            addMenu(JMenuItem.class, jmEdit, AvailableActions.ACTION_REDO, false);

            JMenu jmViewMode = addMenu(JMenu.class, jmOptions, "View/Print mode");
            addMenu(JCheckBoxMenuItem.class, jmViewMode, AvailableActions.ACTION_SINGLE_PAGE_MODE, false);
            addMenu(JCheckBoxMenuItem.class, jmViewMode, AvailableActions.ACTION_CONTINUOUS_PAGE_MODE, false);

            addMenu(JMenuItem.class, jmOptions, AvailableActions.ACTION_RESOLUTION, false);

            bookmarLayerMenu = addMenu(JCheckBoxMenuItem.class, jmOptions, "Show Bookmark/Layer panel");

            bookmarLayerMenu.addActionListener(new ActionListener() {
                public void actionPerformed(ActionEvent e) {
                    final boolean state = bookmarLayerMenu.getState();
                    final boolean noDocument = appController.getActiveDocument() == null;

                    if (noDocument || appController.getActiveDocument().hasBookmarks())
                        bookmarksView.setBookmarksVisible(state);
                    if (noDocument || appController.getActiveDocument().hasLayers())
                        layersView.setLayersVisible(state);
                }

            });

            frame.setJMenuBar(jm);
        }
    }

    private <TMenuItem extends JMenuItem> TMenuItem addMenu(Class<TMenuItem> childClass, JComponent parent, final String text) {
        return addMenu(childClass, parent, new AbstractAction() {
            {
                putValue(NAME, text);
            }

            public void actionPerformed(ActionEvent e) {
            }
        }, false);
    }

    private <TMenuItem extends JMenuItem> TMenuItem addMenu(Class<TMenuItem> childClass, JComponent parent, AvailableActions action, boolean useIcon) {
        final AbstractAction abstractAction = getAction(action);
        final TMenuItem child = addMenu(childClass, parent, abstractAction, useIcon);
        if (child instanceof JCheckBoxMenuItem)
            abstractAction.addPropertyChangeListener(new SelectionListener(child));
        return child;
    }

    private <TMenuItem extends JMenuItem> TMenuItem addMenu(Class<TMenuItem> childClass, JComponent parent, Action action, boolean useIcon) {
        TMenuItem child = null;
        try {
            child = childClass.getConstructor(new Class[] { Action.class }).newInstance(action);
            if (!useIcon)
                child.setIcon(null);

            parent.add(child);
        } catch (IllegalArgumentException e) {
        } catch (SecurityException e) {
        } catch (InstantiationException e) {
        } catch (IllegalAccessException e) {
        } catch (InvocationTargetException e) {
        } catch (NoSuchMethodException e) {
        }
        return child;
    }

    private void addAnnotationDropDownButton(JComponent component) {
        JPopupMenu popupMenu = new JPopupMenu();

        startArrowMenu = new JMenu("Start Arrowhead");
        endArrowMenu = new JMenu("End Arrowhead");

        startPolyMenu = new JMenu("Start Arrowhead");
        endPolyMenu = new JMenu("End Arrowhead");

        AvailableActions[] startActs = new AvailableActions[] { AvailableActions.ACTION_START_ARROW_NONE, AvailableActions.ACTION_START_ARROW_OPEN, AvailableActions.ACTION_START_ARROW_CLOSED, AvailableActions.ACTION_START_ARROW_CIRCLE };

        AvailableActions[] endActs = new AvailableActions[] { AvailableActions.ACTION_END_ARROW_NONE, AvailableActions.ACTION_END_ARROW_OPEN, AvailableActions.ACTION_END_ARROW_CLOSED, AvailableActions.ACTION_END_ARROW_CIRCLE };

        final ButtonGroup startGroup = new ButtonGroup();
        final ButtonGroup endGroup = new ButtonGroup();
        final ButtonGroup startPolyGroup = new ButtonGroup();
        final ButtonGroup endPolyGroup = new ButtonGroup();
        for (int i = 0, len = startActs.length; i < len; ++i) {
            final JCheckBoxMenuItem item = new JCheckBoxMenuItem(getAction(startActs[i]));
            startGroup.add(item);
            startArrowMenu.add(item);

            final JCheckBoxMenuItem item1 = new JCheckBoxMenuItem(getAction(endActs[i]));
            endGroup.add(item1);
            endArrowMenu.add(item1);

            final JCheckBoxMenuItem item2 = new JCheckBoxMenuItem(getAction(startActs[i]));
            startPolyGroup.add(item2);
            startPolyMenu.add(item2);

            final JCheckBoxMenuItem item3 = new JCheckBoxMenuItem(getAction(endActs[i]));
            endPolyGroup.add(item3);
            endPolyMenu.add(item3);
        }

        addMenu(JMenuItem.class, popupMenu, AvailableActions.ACTION_CIRCLE_ANNOTATION, true);
        addMenu(JMenuItem.class, popupMenu, AvailableActions.ACTION_LINE_ANNOTATION, true);

        MenuButton.ClickableMenu arrowMenu = addMenu(MenuButton.ClickableMenu.class, popupMenu, AvailableActions.ACTION_ARROW_ANNOTATION, true);
        arrowMenu.add(startArrowMenu);
        arrowMenu.add(endArrowMenu);

        addMenu(JMenuItem.class, popupMenu, AvailableActions.ACTION_SQUARE_ANNOTATION, true);

        MenuButton.ClickableMenu polylineMenu = addMenu(MenuButton.ClickableMenu.class, popupMenu, AvailableActions.ACTION_POLYLINE_ANNOTATION, true);
        polylineMenu.add(startPolyMenu);
        polylineMenu.add(endPolyMenu);

        addMenu(JMenuItem.class, popupMenu, AvailableActions.ACTION_POLYGON_ANNOTATION, true);
        addMenu(JMenuItem.class, popupMenu, AvailableActions.ACTION_INK_ANNOTATION, true);
        addMenu(JMenuItem.class, popupMenu, AvailableActions.ACTION_LINK_ANNOTATION, true);
        addMenu(JMenuItem.class, popupMenu, AvailableActions.ACTION_HIGHLIGHT_ANNOTATION, true);
        addMenu(JMenuItem.class, popupMenu, AvailableActions.ACTION_UNDERLINE_ANNOTATION, true);

        annotationButton = new MenuButton(true, new Dimension(BUTTON_WIDTH, BUTTON_HEIGHT));
        annotationButton.setMenu(popupMenu);

        component.add(annotationButton);
    }

    private void addFreeTextDropDownButton(JComponent component) {
        JPopupMenu popupMenu = new JPopupMenu();
        fontsMenu = new MenuButton.ClickableMenu("Font");
        fontSizeMenu = new MenuButton.ClickableMenu("Size");
        fontJustifyMenu = new MenuButton.ClickableMenu("Justification");

        familyGroup = new ButtonGroup();
        sizeGroup = new ButtonGroup();
        justifyGroup = new ButtonGroup();

        scrollUp = new BasicArrowButton(BasicArrowButton.NORTH);
        scrollUp.setEnabled(false);
        scrollDown = new BasicArrowButton(BasicArrowButton.SOUTH);

        class MenuScroll extends MouseAdapter {
            public MenuScroll(boolean upDown) {
                this.upDown = upDown;
            }

            @Override
            public void mousePressed(MouseEvent e) {
                clickTimer = new Timer(70, new ActionListener() {
                    public void actionPerformed(ActionEvent e) {
                        for (int i = 0; i < fontsMenu.getItemCount(); ++i) {
                            final JMenuItem item = fontsMenu.getItem(i);
                            if (item != null) {
                                if (fontsMenu.getItem(i).isSelected()) {
                                    relativeIndex = i;
                                    break;
                                }
                            }
                        }

                        int index = availableFonts.indexOf(fontsMenu.getItem(upDown ? 1 : fontsMenuCount).getText());
                        final int removeIndex = upDown ? fontsMenuCount : 1;
                        final int addIndex = upDown ? 1 : fontsMenuCount;
                        final boolean upEnable = index - 1 > 0;
                        final boolean downEnable = index < availableFonts.size() - 1;
                        scrollUp.setEnabled(upEnable);
                        scrollDown.setEnabled(downEnable);
                        index = upDown ? index - 1 : index + 1;
                        if (index >= 0 && index < availableFonts.size()) {
                            fontsMenu.remove(removeIndex);
                            JCheckBoxMenuItem item = new JCheckBoxMenuItem(availableFonts.get(index));
                            item.addActionListener(getAction(AvailableActions.ACTION_FONT_FACE));
                            fontsMenu.add(item, addIndex);
                            familyGroup.add(item);
                            relativeIndex = upDown ? relativeIndex + 1 : relativeIndex - 1;
                            if (relativeIndex >= 0 && relativeIndex <= fontsMenuCount) {
                                JMenuItem menuItem = fontsMenu.getItem(relativeIndex);
                                if (menuItem != null)
                                    menuItem.setSelected(true);
                            }
                            fontsMenu.revalidate();
                        }
                    }
                });
                clickTimer.start();
            }

            @Override
            public void mouseReleased(MouseEvent e) {
                if (clickTimer != null) {
                    clickTimer.stop();
                    clickTimer = null;
                }
            }

            private Timer clickTimer;
            private boolean upDown;

        }

        scrollUp.addMouseListener(new MenuScroll(true));
        scrollUp.setEnabled(false);
        scrollDown.addMouseListener(new MenuScroll(false));

        popupMenu.add(fontsMenu);
        popupMenu.add(fontSizeMenu);
        popupMenu.add(fontJustifyMenu);

        freeTextAnnotationButton = new MenuButton(true, new Dimension(BUTTON_WIDTH, BUTTON_HEIGHT));
        freeTextAnnotationButton.setMenu(popupMenu, getAction(AvailableActions.ACTION_FREETEXT_ANNOTATION));

        component.add(freeTextAnnotationButton);
    }

    private void addLineStyleDropDownButton(JComponent component) {
        JPopupMenu popupMenu = new JPopupMenu();
        createLineStyleMenu(popupMenu, JMenuItem.class, true);

        lineStyleButton = new MenuButton(false, new Dimension(BUTTON_WIDTH, BUTTON_HEIGHT));
        lineStyleButton.setMenu(popupMenu);
        component.add(lineStyleButton);
    }

    private void addLineWidthDropDownButton(JComponent component) {
        JPopupMenu popupMenu = new JPopupMenu();
        createLineWidthMenu(popupMenu, JMenuItem.class, true);

        lineWidthButton = new MenuButton(false, new Dimension(BUTTON_WIDTH, BUTTON_HEIGHT));
        lineWidthButton.setMenu(popupMenu);
        component.add(lineWidthButton);
    }

    private <TMenu extends JComponent> TMenu createLineWidthMenu(TMenu menu, Class<? extends JMenuItem> childClass, boolean useIcon) {
        addMenu(childClass, menu, AvailableActions.ACTION_LINE_WIDTH1, useIcon);
        addMenu(childClass, menu, AvailableActions.ACTION_LINE_WIDTH2, useIcon);
        addMenu(childClass, menu, AvailableActions.ACTION_LINE_WIDTH3, useIcon);
        addMenu(childClass, menu, AvailableActions.ACTION_LINE_WIDTH5, useIcon);
        addMenu(childClass, menu, AvailableActions.ACTION_LINE_WIDTH_OTHER, useIcon);

        return menu;
    }

    private <TMenu extends JComponent> TMenu createLineStyleMenu(TMenu menu, Class<? extends JMenuItem> childClass, boolean useIcon) {
        addMenu(childClass, menu, AvailableActions.ACTION_LINE_STYLE_SOLID, useIcon);
        addMenu(childClass, menu, AvailableActions.ACTION_LINE_STYLE_DASHED1, useIcon);
        addMenu(childClass, menu, AvailableActions.ACTION_LINE_STYLE_DASHED2, useIcon);
        addMenu(childClass, menu, AvailableActions.ACTION_LINE_STYLE_DASHED3, useIcon);
        addMenu(childClass, menu, AvailableActions.ACTION_LINE_STYLE_DASHED4, useIcon);

        return menu;
    }

    private void addZoomList(JComponent component) {
        zoomList = new JComboBox();
        zoomList.setEditable(true);
        configureComponentSize(zoomList, 100, 100, component.getPreferredSize().height);

        for (double zoom : zoomArray) {
            SimpleAction zoomAction = new ZoomAction(zoom); // direct zoom values
            zoomAction.setApplication(appController);
            zoomList.addItem(zoomAction);
        }

        zoomList.addItem(getAction(AvailableActions.ACTION_ZOOM_FIT_PAGE)); // fit page
        zoomList.addItem(getAction(AvailableActions.ACTION_ZOOM_FIT_WIDTH)); // fit width
        zoomList.addItem(getAction(AvailableActions.ACTION_ZOOM_FIT_HEIGHT)); // fit height

        SimpleAction zoomAction = new ZoomAction();
        zoomAction.setApplication(appController);
        zoomList.setAction(zoomAction);

        component.add(zoomList);
    }

    private void addSearchControls(JComponent component, AvailableActions action) {
        JTextField textField = new JTextField();

        try {
            TextSearchAction textSearchAction = (TextSearchAction) getAction(action);
            textSearchAction.setTextField(textField);
        } catch (ClassCastException e) {
        }

        configureComponentSize(textField, 50, 75, component.getPreferredSize().height);
        textField.setAction(getAction(action));

        component.add(textField);
        addButton(component, action);
    }

    private JButton addButton(JComponent component, AvailableActions action) {
        final AbstractAction abstractAction = getAction(action);
        JButton button = new JButton(abstractAction);
        button.setPreferredSize(new Dimension(BUTTON_HEIGHT, BUTTON_HEIGHT));
        button.setSelectedIcon(action.getSelectedIconResource());
        button.setText(null);
        component.add(button);
        abstractAction.addPropertyChangeListener(new SelectionListener(button));

        return button;
    }

    private void addColorPickerButton(JComponent component, AvailableActions action) {
        colorPickerButton = addButton(component, action);
        colorPickerButton.setIcon(new ColorPickerIcon(BUTTON_HEIGHT, BUTTON_HEIGHT, new ColorPicker(Color.BLACK, Color.WHITE)));
        colorPickerButton.setMaximumSize(new Dimension(BUTTON_HEIGHT, BUTTON_HEIGHT));
        colorPickerButton.setMinimumSize(new Dimension(BUTTON_HEIGHT, BUTTON_HEIGHT));
    }

    private void addSeparator(JToolBar toolBar, int width, int height) {
        toolBar.addSeparator(new Dimension(width, height));
    }

    private void addPageFields(JComponent component) {
        pageNumber = new JTextField();
        pageNumber.setColumns(5); // page filed may contain up to 5 characters
        configureComponentSize(pageNumber, 50, 75, component.getPreferredSize().height);

        SimpleAction naviAction = new NavigateAction();
        naviAction.setApplication(appController);
        pageNumber.setAction(naviAction);

        pageCount = new JLabel("", SwingConstants.CENTER);
        configureComponentSize(pageCount, 50, 75, component.getPreferredSize().height);

        component.add(pageNumber);
        component.add(pageCount);
    }

    private void configureComponentSize(JComponent component, int minWidth, int maxWidth, int height) {
        component.setMinimumSize(new Dimension(minWidth, height));
        component.setPreferredSize(new Dimension(minWidth, height));
        component.setMaximumSize(new Dimension(maxWidth, height));
    }

    private static final int BUTTON_HEIGHT = 46;
    private static final int BUTTON_WIDTH = 60;

    private ApplicationController appController;
    private HashMap<AvailableActions, AbstractAction> actions;

    private List<String> availableFonts = Arrays.asList(GraphicsEnvironment.getLocalGraphicsEnvironment().getAvailableFontFamilyNames());

    private JComboBox zoomList;
    private JTextField pageNumber;
    private JLabel pageCount;

    private MenuButton annotationButton;
    private MenuButton lineStyleButton;
    private MenuButton lineWidthButton;
    private JButton colorPickerButton;

    private MenuButton freeTextAnnotationButton;
    MenuButton.ClickableMenu fontsMenu = new MenuButton.ClickableMenu("Font");
    MenuButton.ClickableMenu fontSizeMenu = new MenuButton.ClickableMenu("Size");
    MenuButton.ClickableMenu fontJustifyMenu = new MenuButton.ClickableMenu("Justification");

    JMenu startArrowMenu = new JMenu("Start Arrowhead");
    JMenu endArrowMenu = new JMenu("End Arrowhead");
    JMenu startPolyMenu = new JMenu("Start Arrowhead");
    JMenu endPolyMenu = new JMenu("End Arrowhead");

    private JCheckBoxMenuItem bookmarLayerMenu;

    BasicArrowButton scrollUp = new BasicArrowButton(BasicArrowButton.NORTH);
    BasicArrowButton scrollDown = new BasicArrowButton(BasicArrowButton.SOUTH);

    ButtonGroup familyGroup = new ButtonGroup();
    ButtonGroup sizeGroup = new ButtonGroup();
    ButtonGroup justifyGroup = new ButtonGroup();

    private PDFView pdfView;
    private DialogsView dialogsView;
    private BookmarksView bookmarksView;
    private LayersView layersView;
    private double[] zoomArray = new double[] { 0.25, 0.5, 0.75, 1.0, 1.25, 1.5, 2.0, 3.0, 4.0, 6.0, 8.0 };

    private List<Integer> fontSizes = Arrays.asList(8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72);

    private List<String> fontAlignment = Arrays.asList("Left", "Center", "Right");

    private int[] arrowType = new int[] { LineEndingStyle.NONE.ordinal(), LineEndingStyle.OPEN_ARROW.ordinal(), LineEndingStyle.CLOSED_ARROW.ordinal(), LineEndingStyle.CIRCLE.ordinal() };

    private int[] annotationOrder = new int[] { AvailableActions.ACTION_CIRCLE_ANNOTATION.ordinal(), AvailableActions.ACTION_LINE_ANNOTATION.ordinal(), AvailableActions.ACTION_ARROW_ANNOTATION.ordinal(), AvailableActions.ACTION_SQUARE_ANNOTATION.ordinal(), AvailableActions.ACTION_POLYLINE_ANNOTATION.ordinal(), AvailableActions.ACTION_POLYGON_ANNOTATION.ordinal(), AvailableActions.ACTION_INK_ANNOTATION.ordinal(), AvailableActions.ACTION_LINK_ANNOTATION.ordinal(), AvailableActions.ACTION_HIGHLIGHT_ANNOTATION.ordinal(), AvailableActions.ACTION_UNDERLINE_ANNOTATION.ordinal() };

    private float[][] dashPatterns = new float[][] { { 2.0f, 2.0f }, { 3.0f, 3.0f }, { 4.0f, 4.0f }, { 4.0f, 3.0f, 2.0f, 3.0f } };

    private float[] lineWidths = new float[] { 1, 2, 3, 5, -1 };

    private final int fontsMenuCount = 20;
    private int relativeIndex = 0;
}
