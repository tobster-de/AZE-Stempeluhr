using System;
using System.Windows;
using System.Windows.Input;

namespace AZE.View
{
    /// <summary>
    /// Attached property of the Activated event to use it as commands.
    /// </summary>
    public class ActivatedBehavior
    {
        /// <summary>
        /// The DependencyProperty for the Activated event.
        /// </summary>
        public static DependencyProperty ActivatedProperty =
            DependencyProperty.RegisterAttached(
                "Activated",
                typeof(ICommand),
                typeof(ActivatedBehavior),
                new UIPropertyMetadata(ActivatedBehavior.ActivatedFired));

        /// <summary>
        /// Setter of the AttachedProperty.
        /// </summary>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public static void SetActivated(DependencyObject target, ICommand value)
        {
            target.SetValue(ActivatedBehavior.ActivatedProperty, value);
        }

        private static void ActivatedFired(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (!(target is Window window))
            {
                return;
            }

            if (e.NewValue != null && e.OldValue == null)
            {
                window.Activated += ActivatedBehavior.Activated;
                window.GotFocus += ActivatedBehavior.Activated;
            }
            else if (e.NewValue == null && e.OldValue != null)
            {
                window.Activated -= ActivatedBehavior.Activated;
                window.GotFocus -= ActivatedBehavior.Activated;
            }
        }

        private static void Activated(object sender, EventArgs e)
        {
            if (!(sender is Window window))
            {
                return;
            }

            ICommand command = (ICommand)window.GetValue(ActivatedBehavior.ActivatedProperty);
            command.Execute(e);
        }
    }
}