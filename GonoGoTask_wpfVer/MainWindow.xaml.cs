using System;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Windows;
using System.IO.Ports;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Media;
using System.IO;
using swf = System.Windows.Forms;
using sd = System.Drawing;
using Newtonsoft.Json;
using System.Reflection;
using System.Linq.Expressions;

namespace GonoGoTask_wpfVer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string serialPortIO8_name;

        public string savedFolder;
        public string file_saved;
        public string audioFile_Correct, audioFile_Error;

        public bool showCloseCircle;

        private List<Window> openedWins = new List<Window>();
        presentation taskPresentWin;

        // Strings stoing the Colors
        public string goFillColorStr, nogoFillColorStr, cueCrossingColorStr;
        public string BKWaitTrialColorStr, BKTrialColorStr;
        public string CorrFillColorStr, CorrOutlineColorStr, ErrorFillColorStr, ErrorOutlineColorStr;


        // Time Related Variables
        public float[] tRange_ReadyTimeS, tRange_CueTimeS, tRange_NogoShowTimeS;
        public float tMax_ReactionTimeS, tMax_ReachTimeS, t_VisfeedbackShowS, t_InterTrialS;
        public float t_JuicerCorrectGivenS;

        // Target Related Variables
        public float targetDiaInch;
        public int targetNoOfPositions;
        public int targetDiaPixal;
        public List<int[]> optPostions_OCenter_List;


        // Touch Screen Rectangle
        sd.Rectangle Rect_touchScreen;
        private string taskName;
        

        public MainWindow()
        {
            InitializeComponent();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            taskName = "GoNogo";


            // Get the first not Primary Screen 
            swf.Screen showMainScreen = Utility.Detect_oneNonPrimaryScreen();
            // Show the  MainWindow on the Touch Screen
            sd.Rectangle Rect_showMainScreen = showMainScreen.Bounds;
            this.Top = Rect_showMainScreen.Top;
            this.Left = Rect_showMainScreen.Left;


            // Check serial Port IO8 Connection
            CheckIO8Connection();


            // Load Default Config File
            LoadConfigFile("defaultConfig");

            if (textBox_NHPName.Text != "" && !String.Equals(serialPortIO8_name, ""))
            {
                btn_start.IsEnabled = true;
                btn_stop.IsEnabled = false;
            }
            else
            {
                btn_start.IsEnabled = false;
                btn_stop.IsEnabled = false;
            }


            // Get the touch Screen Rectangle
            swf.Screen PrimaryScreen = swf.Screen.PrimaryScreen;
            Rect_touchScreen = PrimaryScreen.Bounds;
        }

        private void CheckIO8Connection()
        {
            // locate serial Port Name
            serialPortIO8_name = SerialPortIO8.Locate_serialPortIO8();
            if (String.Equals(serialPortIO8_name, ""))
            {
                btn_start.IsEnabled = false;
                btn_comReconnect.Visibility = Visibility.Visible;
                btn_comReconnect.IsEnabled = true;
                textblock_comState.Visibility = Visibility.Visible;

                run_comState.Text = "Can't Find the COM Port for DLP-IO8!";
                run_comState.Background = new SolidColorBrush(Colors.Orange);
                run_comState.Foreground = new SolidColorBrush(Colors.Red);
                run_instruction.Text = "Please connect it correctly and reCheck!";
                run_instruction.Background = new SolidColorBrush(Colors.Orange);
                run_instruction.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                btn_comReconnect.Visibility = Visibility.Hidden;
                btn_comReconnect.IsEnabled = false;
                run_comState.Text = "Found the COM Port for DLP-IO8!";
                run_comState.Background = new SolidColorBrush(Colors.White);
                run_comState.Foreground = new SolidColorBrush(Colors.Green);
                run_instruction.Text = "Can start trials now";
                run_instruction.Background = new SolidColorBrush(Colors.White);
                run_instruction.Foreground = new SolidColorBrush(Colors.Green);
            }
        }

        private void Btn_comReconnect_Click(object sender, RoutedEventArgs e)
        {
            serialPortIO8_name = SerialPortIO8.Locate_serialPortIO8();
            if (String.Equals(serialPortIO8_name, ""))
            {
                run_comState.Text = "Can't Find the COM Port for DLP-IO8!";
                run_comState.Background = new SolidColorBrush(Colors.Orange);
                run_comState.Foreground = new SolidColorBrush(Colors.Red);
                run_instruction.Text = "Please connect it correctly and reCheck!";
                run_instruction.Background = new SolidColorBrush(Colors.Orange);
                run_instruction.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                btn_comReconnect.Visibility = Visibility.Hidden;
                btn_comReconnect.IsEnabled = false;
                run_comState.Text = "Found the COM Port for DLP-IO8!";
                run_comState.Background = new SolidColorBrush(Colors.White);
                run_comState.Foreground = new SolidColorBrush(Colors.Green);
                run_instruction.Text = "Can start trials now";
                run_instruction.Background = new SolidColorBrush(Colors.White);
                run_instruction.Foreground = new SolidColorBrush(Colors.Green);
            }

            if (textBox_NHPName.Text != "" && serialPortIO8_name != null)
            {
                btn_start.IsEnabled = true;
            }

        }

        private void TextBox_NHPName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(textBox_NHPName.Text != "" && serialPortIO8_name != null)
            {
                btn_start.IsEnabled = true;
            }
        }


        private void btnResume_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_nogoTrialNumPerPosSession_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_totalTrialNumPerPosSession_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void MenuItem_SaveConf_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDlg = new Microsoft.Win32.SaveFileDialog
            {

                // Set filter for file extension and default file extension 
                DefaultExt = ".json",
                Filter = "Json Files|*.json",
                FileName = "config"
            };

            Nullable<bool> result = saveFileDlg.ShowDialog();
            if (result == true)
            {
                // Open document 
                string saveConfigFile = saveFileDlg.FileName;
                SaveConfigFile(saveConfigFile);
            }
        }

        private void saveInputParameters()
        {
            DateTime time_now = DateTime.Now;

            // if saved_folder not exist, created!
            if (Directory.Exists(savedFolder) == false)
            {
                System.IO.Directory.CreateDirectory(savedFolder);
            }

            string filename_saved = textBox_NHPName.Text + time_now.ToString("-yyyyMMdd-HHmmss") + ".txt";
            file_saved = System.IO.Path.Combine(savedFolder, filename_saved);

            using (StreamWriter file = new StreamWriter(file_saved))
            {
                file.WriteLine("Date: " + time_now.ToString("MM/dd/yyyy hh:mm:ss tt"));
                file.WriteLine("NHP Name: " + textBox_NHPName.Text);
                file.WriteLine("Task: " + taskName);
                file.WriteLine("\n");


                file.WriteLine(String.Format("{0, -40}:  {1}", "Screen Resolution(Pixal)", Rect_touchScreen.Width.ToString() + " x " + Rect_touchScreen.Height.ToString()));
                file.WriteLine(String.Format("{0, -40}:  {1}", "CM to Pixal Ratio", Utility.ratioCM2Pixal.ToString()));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Inch to Pixal Ratio", Utility.ratioIn2Pixal.ToString()));
                file.WriteLine("\n");
                file.WriteLine("\n");


                file.WriteLine("Presentation Settings:");
                file.WriteLine("\n");

                // Trial Number Settings
                file.WriteLine(String.Format("{0, -40}:  {1}", "Total Trial Number Per Position Per Session", textBox_totalTrialNumPerPosSess.Text));
                file.WriteLine(String.Format("{0, -40}:  {1}", "noGo Trial Number Per Position Per Session", textBox_nogoTrialNumPerPosSess.Text));
                file.WriteLine("\n");


                // Save Target Settings
                file.WriteLine("\nTarget Position Settings:");
                file.WriteLine(String.Format("{0, -40}:  {1}", "Target Diameter (Inch)", targetDiaInch.ToString()));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Number of Target Positions", targetNoOfPositions.ToString()));
                file.WriteLine("Center Coordinates of Each Target (Pixal, (0,0) in Screen Center, Right and Down Direction is Positive):");
                for (int i = 0; i < optPostions_OCenter_List.Count; i++)
                {
                    int[] position = optPostions_OCenter_List[i];
                    file.WriteLine(String.Format("{0, -40}:{1}, {2}", "Postion " + i.ToString(), position[0], position[1]));
                }
                file.WriteLine("\n");


                // Save Color Settings
                file.WriteLine("\nColor Settings:");
                file.WriteLine(String.Format("{0, -40}:  {1}", "Wait Trial Start Background", BKWaitTrialColorStr));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Trial Background", BKTrialColorStr));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Crossing Cue Color", cueCrossingColorStr));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Go Target Fill Color", goFillColorStr));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Nogo Target Fill Color", nogoFillColorStr));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Fill Color for Correct Feedback", CorrFillColorStr));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Outline Color for Correct Feedback", CorrOutlineColorStr));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Fill Color for Error Feedback", ErrorFillColorStr));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Outline Color for Error Feedback", ErrorOutlineColorStr));
                file.WriteLine("\n");


                // Save Time Settings
                file.WriteLine("\nTime Settings:");
                file.WriteLine(String.Format("{0, -40}:  {1}", "Max Reaction Time (s)", tMax_ReactionTimeS.ToString()));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Max Reach Time (s)", tMax_ReachTimeS.ToString()));
                file.WriteLine(String.Format("{0, -40}:  [{1} {2}]", "Ready Interface Show Time Range (s)", tRange_ReadyTimeS[0].ToString(), tRange_ReadyTimeS[1].ToString()));
                file.WriteLine(String.Format("{0, -40}:  [{1} {2}]", "Cue Interface Show Time Range (s)", tRange_CueTimeS[0].ToString(), tRange_CueTimeS[1].ToString()));
                file.WriteLine(String.Format("{0, -40}:  [{1} {2}]", "Nogo Interface Show Range Time (s)", tRange_NogoShowTimeS[0].ToString(), tRange_NogoShowTimeS[1].ToString()));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Inter-Trial Time (s)", t_InterTrialS.ToString()));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Visual Feedback Time (s)", t_VisfeedbackShowS.ToString())); 
                file.WriteLine(String.Format("{0, -40}:  {1}", "Correct Given Juicer Time (s)", t_JuicerCorrectGivenS.ToString()));
                file.WriteLine("\n");

            }
        }


        public static TAttribute GetPropertyAttribute<TType, TAttribute>(Expression<Func<TType, object>> property)
            where TAttribute : Attribute
        {
            var memberExpression = property.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Expression must be a property");

            return memberExpression.Member
                .GetCustomAttribute<TAttribute>();
        }


        private void MenuItem_SetupTime(object sender, RoutedEventArgs e)
        {
            SetupTimeWin Win_SetupTime = new SetupTimeWin(this);

            // Get the first not Primary Screen 
            swf.Screen showMainScreen = Utility.Detect_oneNonPrimaryScreen();
            // Show the  MainWindow on the Touch Screen
            sd.Rectangle Rect_showMainScreen = showMainScreen.Bounds;
            Win_SetupTime.Top = Rect_showMainScreen.Top;
            Win_SetupTime.Left = Rect_showMainScreen.Left;

            // Set Owner
            Win_SetupTime.Owner = this;

            Win_SetupTime.Show();
        }

        private void MenuItem_SetupColors(object sender, RoutedEventArgs e)
        {
            SetupColorsWin Win_SetupColors = new SetupColorsWin(this);


            // Get the first not Primary Screen 
            swf.Screen showMainScreen = Utility.Detect_oneNonPrimaryScreen();
            // Show the  MainWindow on the Touch Screen
            sd.Rectangle Rect_showMainScreen = showMainScreen.Bounds;
            Win_SetupColors.Top = Rect_showMainScreen.Top;
            Win_SetupColors.Left = Rect_showMainScreen.Left;

            // Set Owner
            Win_SetupColors.Owner = this;

            Win_SetupColors.Show();


        }

        private void MenuItem_SetupTarget(object sender, RoutedEventArgs e)
        {
            SetupTargetsWin Win_SetupTarget = new SetupTargetsWin(this);


            // Get the first not Primary Screen 
            swf.Screen showMainScreen = Utility.Detect_oneNonPrimaryScreen();
            // Show the  MainWindow on the Touch Screen
            sd.Rectangle Rect_showMainScreen = showMainScreen.Bounds;
            Win_SetupTarget.Top = Rect_showMainScreen.Top;
            Win_SetupTarget.Left = Rect_showMainScreen.Left;

            // Set Owner
            Win_SetupTarget.Owner = this;

            Win_SetupTarget.Show();
        }


        private void MenuItem_SetupSaveFolderAudio(object sender, RoutedEventArgs e)
        {
            SetupSavefolderAudios Win_SetupSavefolderAudios = new SetupSavefolderAudios(this);


            // Get the first not Primary Screen 
            swf.Screen showMainScreen = Utility.Detect_oneNonPrimaryScreen();
            // Show the  MainWindow on the Touch Screen
            sd.Rectangle Rect_showMainScreen = showMainScreen.Bounds;
            Win_SetupSavefolderAudios.Top = Rect_showMainScreen.Top;
            Win_SetupSavefolderAudios.Left = Rect_showMainScreen.Left;

            // Set Owner
            Win_SetupSavefolderAudios.Owner = this;

            Win_SetupSavefolderAudios.Show();
        }

        private void LoadConfigFile(string configFile)
        {/*Load Config File .json 
            configFile == '': load the default Config File
            */

            // Read the Config. File and convert to JsonObject
            string jsonStr;
            if (String.Equals(configFile, "defaultConfig"))
            {
                var assembly = Assembly.GetExecutingAssembly();
                var defaultConfigFile = "GonoGoTask_wpfVer.Resources.ConfigFiles.defaultConfig.json";

                using (Stream stream = assembly.GetManifestResourceStream(defaultConfigFile))
                {
                    using (StreamReader r = new StreamReader(stream))
                    {
                        jsonStr = r.ReadToEnd();
                    }
                }

            }
            else
            {
                using (StreamReader r = new StreamReader(configFile))
                {
                    jsonStr = r.ReadToEnd();
                }
            }
                      
            dynamic config = JsonConvert.DeserializeObject(jsonStr);
            
            //Config into the Main Interface 
            textBox_NHPName.Text = (string)config["NHP Name"];
            textBox_totalTrialNumPerPosSess.Text = (string)config["Total Trial Num Per Position Per Session"];
            textBox_nogoTrialNumPerPosSess.Text = (string)config["noGo Trial Num Per Position Per Session"];



            // SaveFolder and Audio Section
            savedFolder = (string)config["saved folder"];
            audioFile_Correct = (string)config["audioFile_Correct"];
            audioFile_Error = (string)config["audioFile_Error"]; 
            if (String.Compare(savedFolder, "default", true) == 0)
            {
                savedFolder = System.IO.Path.GetFullPath(@"C:\\GoNogoTaskSave"); ;
            }



            // Times Sections
            var configTime = config["Times"];
            tRange_ReadyTimeS = new float[] {float.Parse((string)configTime["Ready Show Time Range"][0]), float.Parse((string)configTime["Ready Show Time Range"][1])};
            tRange_CueTimeS = new float[] {float.Parse((string)configTime["Cue Show Time Range"][0]), float.Parse((string)configTime["Cue Show Time Range"][1])};
            tRange_NogoShowTimeS = new float[] { float.Parse((string)configTime["Nogo Show Time Range"][0]), float.Parse((string)configTime["Nogo Show Time Range"][1]) };
            tMax_ReactionTimeS = float.Parse((string)configTime["Max Reaction Time"]);
            tMax_ReachTimeS = float.Parse((string)configTime["Max Reach Time"]);
            t_InterTrialS = float.Parse((string)configTime["Inter Trials Time"]);
            t_VisfeedbackShowS = float.Parse((string)configTime["Visual Feedback Show Time"]);
            t_JuicerCorrectGivenS = float.Parse((string)configTime["Juice Correct Given Time"]);


            // Color Sections
            var configColors = config["Colors"];
            goFillColorStr = configColors["Go Fill Color"];
            nogoFillColorStr = configColors["noGo Fill Color"];
            cueCrossingColorStr = configColors["Cue Crossing Color"];
            BKWaitTrialColorStr = configColors["Wait Trial Start Background"];
            BKTrialColorStr = configColors["Trial Background"];
            CorrFillColorStr = configColors["Correct Fill"];
            CorrOutlineColorStr = configColors["Correct Outline"];
            ErrorFillColorStr = configColors["Error Fill"];
            ErrorOutlineColorStr = configColors["Error Outline"];


            // Target Sections
            var configTarget = config["Target"];
            targetDiaInch = float.Parse((string)configTarget["Target Diameter (Inch)"]);
            targetNoOfPositions = configTarget["Target No of Positions"];
            optPostions_OCenter_List = new List<int[]>();
            dynamic tmp = configTarget["Optional Positions"];
            foreach (var xyPos in tmp)
            {
                int a = int.Parse((string)xyPos[0]);
                int b = int.Parse((string)xyPos[1]);
                optPostions_OCenter_List.Add(new int[] { a, b });
            } 
        }

        private void SaveConfigFile(string configFile)
        {/*Load Config File .json 
            configFile == '': load the default Config File
            */

            // config Times
            ConfigTimes configTimes = new ConfigTimes();
            configTimes.tRange_ReadyTime = tRange_ReadyTimeS;
            configTimes.tRange_CueTime = tRange_CueTimeS;
            configTimes.tRange_NogoShowTime = tRange_NogoShowTimeS;
            configTimes.tMax_ReactionTime = tMax_ReactionTimeS;
            configTimes.tMax_ReachTime = tMax_ReachTimeS;
            configTimes.t_InterTrial = t_InterTrialS;
            configTimes.t_VisfeedbackShow = t_VisfeedbackShowS;
            configTimes.t_JuicerCorrectGiven = t_JuicerCorrectGivenS;


            // config Target
            ConfigTarget configTarget = new ConfigTarget();
            configTarget.targetDiaInch = targetDiaInch;
            configTarget.targetNoOfPositions = targetNoOfPositions;
            configTarget.optPostions_OCenter_List = optPostions_OCenter_List;


            // config Colors
            ConfigColors configColors = new ConfigColors();
            configColors.goFillColorStr = goFillColorStr;
            configColors.nogoFillColorStr = nogoFillColorStr;
            configColors.cueCrossingColorStr = cueCrossingColorStr;
            configColors.BKWaitTrialColorStr = BKWaitTrialColorStr;
            configColors.BKTrialColorStr = BKTrialColorStr;
            configColors.CorrFillColorStr = CorrFillColorStr;
            configColors.CorrOutlineColorStr = CorrOutlineColorStr;
            configColors.ErrorFillColorStr = ErrorFillColorStr;
            configColors.ErrorOutlineColorStr = ErrorOutlineColorStr;



            // Combine all configs
            Config_GoNogoTask config = new Config_GoNogoTask();
            config.NHPName = textBox_NHPName.Text;
            config.TotalTrialNumPerPosSess = Int32.Parse(textBox_totalTrialNumPerPosSess.Text);
            config.NogoTrialNumPerPosSess = Int32.Parse(textBox_nogoTrialNumPerPosSess.Text);
            config.configTimes = configTimes;
            config.configTarget = configTarget;
            config.configColors = configColors;
            config.saved_folder = savedFolder;
            config.audioFile_Correct = audioFile_Correct;
            config.audioFile_Error = audioFile_Error;


            // Write to Json file
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configFile, json);
        }
        private void btnLoadConf_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            openFileDlg.DefaultExt = ".json";
            openFileDlg.Filter = "Json Files|*.json";

            Nullable<bool> result = openFileDlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string configFile = openFileDlg.FileName;
                LoadConfigFile(configFile);
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            // save all the Input parameters
            saveInputParameters();

            // btn_Start and btn_stop
            btn_start.IsEnabled = false;
            btn_stop.IsEnabled = true;


            // Show the taskpresent Window on the Touch Screen
            taskPresentWin = new presentation(this);
            taskPresentWin.Top = Rect_touchScreen.Top;
            taskPresentWin.Left = Rect_touchScreen.Left;

            taskPresentWin.Name = "childWin_Task";
            taskPresentWin.Owner = this;


            

            // Start the Task
            taskPresentWin.Show();
            taskPresentWin.Present_Start();
        }

        private void Btn_stop_Click(object sender, RoutedEventArgs e)
        {
            if (taskPresentWin != null)
            {
                taskPresentWin.Present_Stop();
                taskPresentWin.Close();
                taskPresentWin = null;
            }
            // btn_Start and btn_stop
            btn_start.IsEnabled = true;
            btn_stop.IsEnabled = false;
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (taskPresentWin != null)
            {
                taskPresentWin.Present_Stop();
            }
        }
    }
}
