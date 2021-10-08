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
using System.Windows.Shapes;

namespace GonoGoTask_wpfVer
{
    /// <summary>
    /// Interaction logic for SetupSavefolderAudios.xaml
    /// </summary>
    public partial class SetupSavefolderAudios : Window
    {
        private MainWindow parent;

        public SetupSavefolderAudios(MainWindow parentWindow)
        {
            InitializeComponent();
            parent = parentWindow;
        }

        private void Btn_Select_AudioFile_Correct_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_SelectSavefolder_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_OK_Click(object sender, RoutedEventArgs e)
        {
            parent.savedFolder = textBox_savedFolder.Text;
            parent.audioFile_Correct = textBox_audioFile_Correct.Text;
            parent.audioFile_Error = textBox_audioFile_Error.Text;
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_Select_AudioFile_Error_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBox_savedFolder.Text = parent.savedFolder;
            textBox_audioFile_Correct.Text = parent.audioFile_Correct;
            textBox_audioFile_Error.Text = parent.audioFile_Error;
        }
    }
}
