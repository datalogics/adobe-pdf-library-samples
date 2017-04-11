/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import java.lang.reflect.Field;

import com.datalogics.PDFL.JavaViewer.Presentation.Application;

/**
 * DocumentCommand - base class for all application commands.
 * 
 * It contains execute(), undo(), and redo() methods. This class also keeps
 * command's state. These states are used during command execution process.
 */
public abstract class DocumentCommand {
    public static enum State {
        New, Prepared, Pending, Executing, Succeeded, Failed, Canceled, Rejected;

        public boolean isNew() {
            return this.equals(New);
        }

        public boolean isPrepared() {
            return this.equals(Prepared);
        }

        public boolean isPending() {
            return this.equals(Pending);
        }

        public boolean isExecuting() {
            return this.equals(Executing);
        }

        public boolean isFinished() {
            return this.equals(Succeeded) || this.equals(Failed) || this.equals(Canceled) || this.equals(Rejected);
        }
    }

    protected class FinalState extends Throwable {
        public FinalState(State state) {
            this.state = state;
        }

        public State getState() {
            return state;
        }

        private State state;
    }

    private static interface ExecuteDelegate {
        void execute() throws FinalState;
    }

    public static CommandType getType(Class<? extends DocumentCommand> commandClass) {
        final String typeField = "type";
        CommandType type = null;

        try {
            Field field = commandClass.getDeclaredField(typeField);
            field.setAccessible(true);

            type = (CommandType) field.get(null);
        } catch (SecurityException e) {
        } catch (NoSuchFieldException e) {
            System.out.println("Declare private final static field named '" + typeField + "' in the " + commandClass.getSimpleName() + " class!");
        } catch (IllegalArgumentException e) {
        } catch (IllegalAccessException e) {
        } catch (ClassCastException e) {
            System.out.println("Wrong type of field named '" + typeField + "'!");
        }
        return type;
    }

    final public CommandType getType() {
        return getType(this.getClass());
    }

    final public State getState() {
        return this.state;
    }

    final public void execute() {
        execute(new ExecuteDelegate() {
            public void execute() throws FinalState {
                doInner();
            }
        }, State.Succeeded);
    }

    final public void undo() {
        if (getState() == State.Succeeded) {
            setState(State.Prepared);
            undo(State.Succeeded);
        } else {
            System.out.println("Wrong undo from state " + getState() + "!");
        }
    }

    final public void redo() {
        if (getState() == State.Succeeded) {
            setState(State.Prepared);
            redo(State.Succeeded);
        } else {
            System.out.println("Wrong redo from state " + getState() + "!");
        }
    }

    public void reject() {
        if (!getState().isNew() && !getState().isFinished()) {
            undo(State.Rejected);
        }
    }

    public void cancel() {
        if (!getState().isNew() && !getState().isFinished()) {
            undo(State.Canceled);
        }
    }

    public void setApplication(Application application) {
        this.application = application;
    }

    final protected Application getApplication() {
        return this.application;
    }

    protected abstract void prepare() throws FinalState;

    protected abstract void doInner() throws FinalState;

    protected void undoInner() throws FinalState {
    }

    protected void redoInner() throws FinalState {
        doInner();
    }

    protected void cleanup() {
    }

    private void setState(State newState) {
        // System.out.println(this.getClass().getSimpleName() + " || From: " +
        // state + "\tTo: " + newState);
        state = newState;
    }

    private void execute(ExecuteDelegate delegate, State finalState) {
        if (!finalState.isFinished() || finalState == State.Failed)
            System.out.println("Wrong final state passed to the executeInner!");

        if (!getState().isFinished()) {
            try {
                if (!getState().isPrepared()) {
                    // preparation phase
                    State prepareState = State.Prepared;
                    try {
                        if (!getState().isPending()) // no preparation for pending state, as the command has been prepared before
                            prepare();
                    } catch (FinalState e) {
                        prepareState = e.getState();
                    }
                    setState(prepareState);
                }

                // execution phase
                if (getState().isPrepared() && !getState().isExecuting()) {
                    State executeState = finalState;
                    try {
                        setState(State.Executing);
                        delegate.execute();
                    } catch (FinalState e) {
                        executeState = e.getState();
                    }
                    setState(executeState);
                }
            } catch (RuntimeException e) {
                setState(State.Failed);
                e.printStackTrace();
            }
        }

        if (!getState().isFinished() && !getState().isPending())
            System.out.println("Wrong final state for command " + this.toString());
    }

    private void undo(State finalState) {
        execute(new ExecuteDelegate() {
            public void execute() throws FinalState {
                undoInner();
            }
        }, finalState);
    }

    private void redo(State finalState) {
        execute(new ExecuteDelegate() {
            public void execute() throws FinalState {
                redoInner();
            }
        }, finalState);
    }

    private State state = State.New;
    private Application application;
}
