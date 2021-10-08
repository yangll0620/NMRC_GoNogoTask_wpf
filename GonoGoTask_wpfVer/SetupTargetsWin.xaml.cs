using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using sd = System.Drawing;

namespace GonoGoTask_wpfVer
{
    /// <summary>
    /// Interaction logic for SetupTargetsWin.xaml
    /// </summary>
    public partial class SetupTargetsWin : Window
    {
        public MainWindow parent;

        private bool BtnStartState, BtnStopState;

        // Optional positions 
        ArrayList optPosString_List;
        private TextBox editBox_Pos;
        int indexSelected = 0;

        // Window and Grid showing the targets
        Window Win_allTargets;


        public SetupTargetsWin(MainWindow parentWindow)
        {
            InitializeComponent();
            parent = parentWindow;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DisableBtnStartStop();
            optPosString_List = new ArrayList();
            
            LoadInitTargetData();
        }

        private void LoadInitTargetData()
        {

            // Fill in target DiaInch and No of Positions
            textBox_targetDiaInch.Text = parent.targetDiaInch.ToString();
            textBox_targetNoOfPositions.Text = parent.targetNoOfPositions.ToString();

            // Fill in Pos List Box
            UpdatePosListBox(parent.optPostions_OCenter_List);


            // Editable TextBox for changing position
            editBox_Pos = new TextBox();
            editBox_Pos.Name = "editBox_Pos";
            editBox_Pos.Width = 0;
            editBox_Pos.Height = 0;
            editBox_Pos.Visibility = Visibility.Hidden;
            editBox_Pos.Text = "";
            editBox_Pos.Background = new SolidColorBrush(Colors.Beige);
            editBox_Pos.Foreground = new SolidColorBrush(Colors.Blue);
            Grid_setupTarget.Children.Add(editBox_Pos);
            Grid_setupTarget.RegisterName(editBox_Pos.Name, editBox_Pos);
            Grid_setupTarget.UpdateLayout();
        }

        private void UpdatePosListBox(List<int[]> optPostions_OCenter_List)
        {/*
                Generate the optional X, Y Positions (origin in center)

                Store into class member parent.optPostions_OCenter_List
                and Show on the control listBox_Positions
            */

            // Binding with listBox_Position
            optPosString_List.Clear();
            foreach (int[] xyPos in optPostions_OCenter_List)
            {
                optPosString_List.Add(xyPos[0].ToString() + ", " + xyPos[1].ToString());
            }
            listBox_Positions.ItemsSource = null;
            listBox_Positions.ItemsSource = optPosString_List;
        }

        private void SaveTargetData()
        {/* ---- Save all the Set Target Information back to MainWindow Variables ----- */

            parent.targetDiaInch = float.Parse(textBox_targetDiaInch.Text);
            parent.targetDiaPixal = Utility.Inch2Pixal(parent.targetDiaInch);
            parent.targetNoOfPositions = int.Parse(textBox_targetNoOfPositions.Text);


            // Extract parent.optPostions_OCenter_List from optPosString_List
            parent.optPostions_OCenter_List.Clear();
            for (int i = 0; i < optPosString_List.Count; i++)
            {
                try
                {
                    string xyPosString = (string)optPosString_List[i];
                    string[] strxy = xyPosString.Split(',');
                    parent.optPostions_OCenter_List.Add(new int[] { int.Parse(strxy[0]), int.Parse(strxy[1]) });
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private void DisableBtnStartStop()
        {
            BtnStartState = parent.btn_start.IsEnabled;
            BtnStopState = parent.btn_stop.IsEnabled;
            parent.btn_start.IsEnabled = false;
            parent.btn_stop.IsEnabled = false;
        }

        private void ResumeBtnStartStop()
        {
            parent.btn_start.IsEnabled = BtnStartState;
            parent.btn_stop.IsEnabled = BtnStopState;
        }

        private void Btn_OK_Click(object sender, RoutedEventArgs e)
        {
            SaveTargetData();
            ResumeBtnStartStop();
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ResumeBtnStartStop();
        }

        private void ListBox_Positions_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            indexSelected = listBox_Positions.SelectedIndex;
            CreateEditBox(sender);
        }

        private void CreateEditBox(object sender)
        {
            // Get the position and width/height of selected Item
            ListBoxItem lbi = (ListBoxItem)listBox_Positions.ItemContainerGenerator.ContainerFromItem(listBox_Positions.SelectedItem);
            Point pt = lbi.TransformToAncestor(this).Transform(new Point(0, 0));

            double delta = 3;
            editBox_Pos.HorizontalAlignment = HorizontalAlignment.Left;
            editBox_Pos.VerticalAlignment = VerticalAlignment.Top;
            editBox_Pos.Margin = new Thickness(pt.X + delta, pt.Y + delta, 0, 0);
            editBox_Pos.Width = lbi.ActualWidth;
            editBox_Pos.Height = lbi.ActualHeight;

            editBox_Pos.Visibility = Visibility.Visible;
            editBox_Pos.Focus();

            editBox_Pos.Text = (string)lbi.Content;
            Grid_setupTarget.UpdateLayout();

            editBox_Pos.KeyDown += new KeyEventHandler(this.EditOver);

        }

        private void EditOver(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string[] strxy = editBox_Pos.Text.Split(',');
                    parent.optPostions_OCenter_List[indexSelected] = new int[] { int.Parse(strxy[0]), int.Parse(strxy[1]) };

                    optPosString_List[indexSelected] = editBox_Pos.Text;
                    listBox_Positions.ItemsSource = null;
                    listBox_Positions.ItemsSource = optPosString_List;
                    editBox_Pos.Visibility = Visibility.Hidden;
                }
                catch
                {
                    editBox_Pos.Text = "";
                }
            }
        }

        private void ListBox_Positions_KeyDown(object sender, KeyEventArgs e)
        {
            indexSelected = listBox_Positions.SelectedIndex;
            if (e.Key == Key.F2)
                CreateEditBox(sender);
        }

        private void Btn_GenDefaultOptPos_Click(object sender, RoutedEventArgs e)
        {
            int targetNoOfPositions = int.Parse(textBox_targetNoOfPositions.Text);
            GenOptPos(targetNoOfPositions);
        }

        private void Btn_ClosePositions_Click(object sender, RoutedEventArgs e)
        {
            Win_allTargets.Close();

            btn_CheckPositions.IsEnabled = true;
            btn_ClosePositions.IsEnabled = false;
            listBox_Positions.IsEnabled = true;
            textBox_targetNoOfPositions.IsEnabled = true;
            textBox_targetDiaInch.IsEnabled = true;
        }

        private void GenOptPos(int targetNoOfPositions)
        {// Generate optional Positions based on targetNoOfPositions

            int targetRadius = Utility.Inch2Pixal(float.Parse(textBox_targetDiaInch.Text))/2;

            sd.Rectangle Rect_primaryScreen = Utility.Detect_PrimaryScreen_Rect();
            int shorter = (Rect_primaryScreen.Height > Rect_primaryScreen.Width) ? Rect_primaryScreen.Width : Rect_primaryScreen.Height;
            int radius = (shorter / 2) - targetRadius;
            List<int[]> optPostions_OCenter_List = Utility.GenDefaultPositions_GoNogoTask(targetNoOfPositions, radius);
            UpdatePosListBox(optPostions_OCenter_List);
        }


        private void Btn_CheckPositions_Click(object sender, RoutedEventArgs e)
        {
            Color BKColor = (Color)(typeof(Colors).GetProperty(parent.BKTrialColorStr) as PropertyInfo).GetValue(null, null);
            Color targetColor = (Color)(typeof(Colors).GetProperty(parent.goFillColorStr) as PropertyInfo).GetValue(null, null);

            // Get the target diameter from textBox_targetDiaCM
            int targetDiaPixal = Utility.Inch2Pixal(float.Parse(textBox_targetDiaInch.Text));

            // if a new targetNoOfPositions
            if (optPosString_List.Count != int.Parse(textBox_targetNoOfPositions.Text))
            {
                GenOptPos(int.Parse(textBox_targetNoOfPositions.Text));
            }

            Win_allTargets = ShowAllTargets(targetDiaPixal, optPosString_List, targetColor, BKColor);

            btn_CheckPositions.IsEnabled = false;
            btn_ClosePositions.IsEnabled = true;
            listBox_Positions.IsEnabled = false;
            textBox_targetNoOfPositions.IsEnabled = false;
            textBox_targetDiaInch.IsEnabled = false;
        }


        private Window ShowAllTargets(int targetDiaPixal, ArrayList optPosString_List, Color targetColor, Color BKColor)
        {/* 
            Show all the targets 

            Args:
                targetDiaPixal: Target Diameter in Pixal

                postions_OriginCenter_List: x,y Position for Each Target (Origin in Screen Center)

                targetColor: the target Color

                BKColor: the Background Color
            */


            //Show the Win_allTargets on the Touch Screen
            Window Win_allTargets = new Window();
            sd.Rectangle Rect_primaryScreen = Utility.Detect_PrimaryScreen_Rect();
            Win_allTargets.Top = Rect_primaryScreen.Top;
            Win_allTargets.Left = Rect_primaryScreen.Left;


            // Show Background
            Win_allTargets.Background = new SolidColorBrush(BKColor);
            Win_allTargets.WindowState = WindowState.Maximized;
            Win_allTargets.Name = "childWin_ShowAllTargets";
            Win_allTargets.WindowStyle = WindowStyle.None;
            Win_allTargets.Show();



            // Add a Grid
            Grid wholeGrid = new Grid();
            wholeGrid.Height = Win_allTargets.ActualHeight;
            wholeGrid.Width = Win_allTargets.ActualWidth;
            Win_allTargets.Content = wholeGrid;
            wholeGrid.UpdateLayout();

            // Close Button
            Button btn_Close = new Button();
            btn_Close.Width = 100;
            btn_Close.Height = 25;
            btn_Close.VerticalAlignment = VerticalAlignment.Top;
            btn_Close.HorizontalAlignment = HorizontalAlignment.Right;
            btn_Close.Margin = new Thickness(0, 0, 0, 0);
            btn_Close.Content = "Close";
            btn_Close.Click += new RoutedEventHandler(Btn_ClosePositions_Click);
            wholeGrid.Children.Add(btn_Close);

            // Extract postions_OriginCenter_List from optPosString_List
            List<int[]> postions_OriginCenter_List = new List<int[]>();
            for (int i = 0; i < optPosString_List.Count; i++)
            {
                try
                {
                    string xyPosString = (string)optPosString_List[i];
                    string[] strxy = xyPosString.Split(',');
                    postions_OriginCenter_List.Add(new int[] { int.Parse(strxy[0]), int.Parse(strxy[1]) });
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            // Show All Targets
            foreach (int[] cPoint_Pos_OCenter in postions_OriginCenter_List)
            {
                // Change the cPoint  into Top Left Coordinate System
                sd.Rectangle Rect_touchScreen = Utility.Detect_PrimaryScreen_Rect();
                int[] cPoint_Pos_OTopLeft = new int[] { cPoint_Pos_OCenter[0] + Rect_touchScreen.Width / 2, cPoint_Pos_OCenter[1] + Rect_touchScreen.Height / 2 };

                Ellipse circle = Utility.Create_Circle((double)targetDiaPixal, new SolidColorBrush(targetColor));
                Utility.Move_Circle_OTopLeft(circle, cPoint_Pos_OTopLeft);

                wholeGrid.Children.Add(circle);
            }
            wholeGrid.UpdateLayout();

            Win_allTargets.Owner = this;

            return Win_allTargets;
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            ResumeBtnStartStop();
            this.Close();
        }
    }
}
