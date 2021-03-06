﻿// ActionCommand.cs
using System;
using System.Windows;
using System.Windows.Input;

namespace WpfBasics
{
    public class ActionCommand : ICommand
    {
        #region Fields

        private readonly Action action;

        private readonly Func<bool> canExecute;

        private bool canAlways;

        #endregion

        #region Constructors and Destructors

        public ActionCommand(Action action)
        {
            this.action = action;
            this.canAlways = true;
        }

        public ActionCommand(Action action, Func<bool> canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
            this.canAlways = false;
        }

        #endregion

        #region Public Events

        public event EventHandler CanExecuteChanged;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <returns>
        ///     true if this command can be executed; otherwise, false.
        /// </returns>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        public bool CanExecute(object parameter)
        {
            return this.canAlways || (this.canExecute != null && this.canExecute.Invoke());
        }

        /// <summary>
        ///     Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        public void Execute(object parameter)
        {
            this.action.Invoke();
        }

        public void TriggerExecuteChange()
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                this.CanExecuteChanged?.Invoke(this, null);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(
                    () => { this.CanExecuteChanged?.Invoke(this, null); });
            }
        }

        #endregion
    }
}