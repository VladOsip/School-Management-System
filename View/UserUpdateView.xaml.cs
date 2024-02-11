﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasySchool.View
{
    /// <summary>
    /// Interaction logic for UserUpdateView.xaml
    /// </summary>
    public partial class UserUpdateView : UserControl
    {
        public UserUpdateView()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Check if a text is only numbers and the '-' char
        /// </summary>
        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that checks unallowed texts
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        /// <summary>
        /// Make sure that the no invalid text is pasted into the Phone field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsPhoneFieldPasteAllowed(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
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
        ///  Ensures that only valid text is pasted into the phone field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsPhoneFieldPreviewAllowed(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }
    }
}
