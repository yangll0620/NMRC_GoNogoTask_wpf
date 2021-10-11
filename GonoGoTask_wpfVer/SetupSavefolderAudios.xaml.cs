using System;
using System.Windows;
using swf = System.Windows.Forms;

namespace GonoGoTask_wpfVer
{
    /// <summary>
    /// Interaction logic for SetupSavefolderAudios.xaml
    /// </summary>
    public partial class SetupSavefolderAudios : Window
    {
        private MainWindow parent;
        private bool BtnStartState, BtnStopState;

        public SetupSavefolderAudios(MainWindow parentWindow)
        {
            InitializeComponent();
            parent = parentWindow;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DisableBtnStartStop();
            LoadInitTargetData();
        }

        private void DisableBtnStartStop()
        {
            BtnStartState = parent.btn_start.IsEnabled;
            BtnStopState = parent.btn_stop.IsEnabled;
            parent.btn_start.IsEnabled = false;
            parent.btn_stop.IsEnabled = false;
        }

        private void LoadInitTargetData()
        {

            // Fill in saveFolder, audioFile_Correct and audioFile_Error Texts
            textBox_savedFolder.Text = parent.savedFolder;
            textBox_audioFile_Correct.Text = parent.audioFile_Correct;
            textBox_audioFile_Error.Text = parent.audioFile_Error;
        }
        private void ResumeBtnStartStop()
        {
            parent.btn_start.IsEnabled = BtnStartState;
            parent.btn_stop.IsEnabled = BtnStopState;
        }

        private void SaveData()
        {/* ---- Save all the Set Information back to MainWindow Variables ----- */

            parent.savedFolder = textBox_savedFolder.Text;
            parent.audioFile_Correct = textBox_audioFile_Correct.Text;
            parent.audioFile_Error = textBox_audioFile_Error.Text;
        }

        private void Btn_OK_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
            ResumeBtnStartStop();
            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            ResumeBtnStartStop();
            this.Close();
        }

        private void Btn_SelectSavefolder_Click(object sender, RoutedEventArgs e)
        {
            swf.FolderBrowserDialog folderBrowserDlg = new swf.FolderBrowserDialog();
            folderBrowserDlg.Description = "Select the directory for saving data";

            swf.DialogResult result = folderBrowserDlg.ShowDialog();
            if (result == swf.DialogResult.OK)
            {
                textBox_savedFolder.Text = folderBrowserDlg.SelectedPath;
            }
        }

        private void Btn_Select_AudioFile_Correct_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            openFileDlg.FileName = "Document";
            openFileDlg.DefaultExt = ".wav";
            openFileDlg.Filter = "Audio Files (.wav)|*.wav";
            openFileDlg.Title = "Selecting an Audio for Correct Trials";

            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result == true)
            {
                textBox_audioFile_Correct.Text = openFileDlg.FileName;
            }
        }


        private void Btn_Select_AudioFile_Error_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            openFileDlg.FileName = "Document";
            openFileDlg.DefaultExt = ".wav";
            openFileDlg.Filter = "Audio Files (.wav)|*.wav";
            openFileDlg.Title = "Selecting an Audio for Error Trials";

            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result == true)
            {
                textBox_audioFile_Error.Text = openFileDlg.FileName;
            }
        }
    }
}
