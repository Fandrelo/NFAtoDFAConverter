﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Automata.Homebrew
{
    public static class Utils
    {
        public static bool EqualsAll<T>(this IList<T> a, IList<T> b)
        {
            if (a == null || b == null)
                return (a == null && b == null);

            if (a.Count != b.Count)
                return false;

            return a.SequenceEqual(b);
        }
    }

    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _executeAction;
        private readonly Func<object, bool> _canExecuteAction;

        public DelegateCommand(Action<object> executeAction, Func<object, bool> canExecuteAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }

        public void Execute(object parameter) => _executeAction(parameter);

        public bool CanExecute(object parameter) => _canExecuteAction?.Invoke(parameter) ?? true;

        public event EventHandler CanExecuteChanged;

        public void InvokeCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName]string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }
    }

    public static class DropBehavior
    {
        private static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached
            (
                "Command",
                typeof(ICommand),
                typeof(DropBehavior),
                new PropertyMetadata(CommandPropertyChangedCallBack)
            );

        public static void SetCommand(this UIElement inUIElement, ICommand inCommand)
        {
            inUIElement.SetValue(CommandProperty, inCommand);
        }

        private static ICommand GetCommand(UIElement inUIElement)
        {
            return (ICommand)inUIElement.GetValue(CommandProperty);
        }

        private static void CommandPropertyChangedCallBack(
            DependencyObject inDependencyObject, DependencyPropertyChangedEventArgs inEventArgs)
        {
            if (!(inDependencyObject is UIElement uiElement)) return;

            uiElement.Drop += (sender, args) =>
            {
                GetCommand(uiElement).Execute(args.Data);
                args.Handled = true;
            };
        }
    }
}
