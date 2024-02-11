using System;
using System.Windows;
using EasySchool.View.Utilities;

namespace EasySchool.View
{
    /// <summary>
    /// Interaction logic for ApplicationMainWindow.xaml
    /// </summary>
    public partial class ApplicationMainWindow : Window, IClosableScreen
    {
        public ApplicationMainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Close this window
        /// </summary>
        public void CloseScreen()
        {
            base.Close();
        }
    }
}
