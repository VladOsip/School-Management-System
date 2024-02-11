using EasySchool.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for GradesReportView.xaml
    /// </summary>
    public partial class GradesReportView : UserControl
    {
        public GradesReportView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Check that the field is only numbers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsValidGrade(object sender, TextCompositionEventArgs e)
        {
            try
            {
                // Check if number
                Convert.ToInt32(e.Text);

                // Get the text of the textbox after adding the previewed character, using the current selection in the textbox to determine the location
                // of the new character
                TextBox grade = sender as TextBox;
                string updatedGrade =
                    grade.Text.Substring(0, grade.SelectionStart) + e.Text + grade.Text.Substring(grade.SelectionStart + grade.SelectionLength);

                // Check that the full grade text is a valid grade score
                int inputGrade = Convert.ToInt32(updatedGrade);

                // Check if the score is outside the valid grade score
                if ((inputGrade > Globals.GRADE_MAX_VALUE || inputGrade < Globals.GRADE_MIN_VALUE) && inputGrade != Globals.GRADE_NO_VALUE)
                {
                    e.Handled = true;
                }
            }
            catch
            {
                e.Handled = true;
            };
        }
    }
}
