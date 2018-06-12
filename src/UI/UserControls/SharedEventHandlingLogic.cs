﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UI.ViewModels.Base;
using UI.Windows;

namespace UI.UserControls
{
    /// <summary>
    /// Contains event-handling logic used by multiple code-behinds.
    /// </summary>
    public static class SharedEventHandlingLogic
    {
        private static readonly Regex AspectInput = new Regex("^[0-9]*:?[0-9]*$");

        /// <summary>
        /// Constrains the text written in a container to something that can be parsed as an integer.
        /// </summary>
        public static void InputValidation_ConstrainToInt(object sender, TextCompositionEventArgs e)
        {
            int num;
            e.Handled = !int.TryParse(e.Text, out num);
        }

        /// <summary>
        /// Constrains the text pasted in a container to something that can be parsed as an integer.
        /// </summary>
        public static void InputValidationOnPaste_ConstrainToInt(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                int num;
                if (!int.TryParse((string)e.DataObject.GetData(typeof(string)), out num))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        /// <summary>
        /// Constrains the text written in a container to something that is of the form "int:int".
        /// </summary>
        public static void InputValidation_ConstrainToAspectRatio(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !AspectInput.IsMatch(e.Text);
        }

        /// <summary>
        /// Constrains the text pasted in a container to something that is of the form "int:int".
        /// </summary>
        public static void InputValidationOnPaste_ConstrainToAspectRatio(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var input = (string) e.DataObject.GetData(typeof(string));
                
                if (!AspectInput.IsMatch(input))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        public static RelayCommand CreateLogCommand(BaseControlProperties vm, UserControl control, string header)
        {
            return new RelayCommand(o =>
            {
                if (vm.Log.Count > 10000)
                {
                    var messagebox = MessageBox.Show($"The log has {vm.Log.Count} entries. The log textbox may take a long time to render these, during which the program will appear to be frozen. \n\nAre you sure you want to continue?",
                        "Warning: Large log.", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    switch (messagebox)
                    {
                        case MessageBoxResult.Yes:
                        case MessageBoxResult.OK:
                            break;
                        case MessageBoxResult.None:
                        case MessageBoxResult.Cancel:
                        case MessageBoxResult.No:
                        default:
                            return;
                    }
                }

                var logWindow = new LogWindow(vm, header)
                {
                    Height = control.ActualHeight,
                    Width = control.ActualWidth
                };
                logWindow.Show();
            });
        }
    }
}
