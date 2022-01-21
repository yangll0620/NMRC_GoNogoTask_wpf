using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO.Ports;
using System.IO;
using System.Reflection;
using sd = System.Drawing;
using swf =System.Windows.Forms;

namespace GonoGoTask_wpfVer
{
    /// <summary>
    /// Interaction logic for presentation.xaml
    /// </summary>
    /// 
    public partial class presentation : Window
    {

        /***** predefined parameters*******/
        int wpoints_radius = 15;



        /***********enumerate *****************/
        public enum TargetType
        {
            Nogo,
            Go,
        }

        public enum InterfaceState
        {
            beforeStart,
            Ready,
            TargetCue,
            GoNogo,
            Reward,
        }

        public enum TrialExeResult
        {
            readyWaitTooShort,
            cueWaitTooShort,
            goReactionTimeToolong,
            goReachTimeToolong,
            goHit,
            goClose,
            goMiss,
            nogoMoved,
            nogoSuccess
        }

        public enum ScreenTouchState
        {
            Idle,
            Touched
        }

        private enum GoTargetTouchState
        {
            goHit, // at least one finger inside the circleGo
            goMissed // touched with longer distance 
        }

        /*startpad related enumerate*/
        private enum ReadStartpad
        {
            No,
            Yes
        }

        private enum PressedStartpad
        {
            No,
            Yes
        }

        private enum GiveJuicerState
        {
            No,
            CorrectGiven
        }

        private enum StartPadHoldState
        {
            HoldEnough,
            HoldTooShort
        }



        /*****************parameters ************************/
        MainWindow parent;


        string file_saved;

        // Trial and Target Setup Variables
        int totalTrialNumPerPosSess, nogoTrialNumPerPosSess;
        int targetPosNum;

        // diameter for crossing, circle, square and white points
        int targetDiameterPixal;
        int crossingLineThickness = 3;

        TargetType targetType;

        // randomized Go noGo tag list, tag_gonogo ==1: go, ==0: nogo
        List<TargetType> targetType_List = new List<TargetType>();
        // randomized t_Ready list
        List<float> t_Ready_List = new List<float>();
        // randomized t_Cue list
        List<float> t_Cue_List = new List<float>();
        // randomized t_Cue list
        List<float> t_noGoShow_List = new List<float>();


        // objects of Go cirle, nogo Rectangle, lines of the crossing, and two white points
        Ellipse circleGo;
        Rectangle rectNogo;
        Line vertLine, horiLine;
        Ellipse point1, point2;

        double currCircleGo_Radius_Pixal;

        // ColorBrushes 
        private SolidColorBrush brush_goCircleFill, brush_nogoRectFill;
        private SolidColorBrush brush_BKWaitTrialStart, brush_BKTrial;
        private SolidColorBrush brush_CorrectFill, brush_CorrOutline, brush_ErrorFill, brush_ErrorOutline;
        private SolidColorBrush brush_BDWaitTrialStart;
        private SolidColorBrush brush_CueCrossing;


        // audio feedback
        private string audioFile_Correct, audioFile_Error;
        System.Media.SoundPlayer player_Correct, player_Error;


        // name of all the objects
        string name_circleGo = "circleGo";
        string name_rectNogo = "rectNogo";
        string name_vLine = "vLine", name_hLine = "hLine";
        string name_point1 = "wpoint1", name_point2 = "wpoint2";


        private List<int[]> optPostions_OTopLeft_List;

        // Target Information (posIndex, goNogoType) List for Each Trial Per Session
        private List<int[]> trialTargetInfo_PerSess_List;


        //TargetExeFeedback_List: totalGoTrials, successGoTrials, totalNogoTrials, successNogoTrials
        private List<int[]> TargetExeFeedback_List;

        // Wait Time Range for Each Event, and Max Reaction and Reach Time
        float[] tRange_ReadyTime, tRange_CueTime, tRange_NogoShowTime;
        float tMax_ReactionTimeMS, tMax_ReachTimeMS;
        Int32 t_VisfeedbackShow, t_InterTrialMS; // Visual Feedback Show Time (ms)

        bool PresentTrial;

        // selected Target Position Index for Current Presented Trial
        int currTrialTargetPosInd;

        // time stamp
        long timestamp_0;


        // set storing the touch point id (no replicates)
        HashSet<int> touchPoints_Id = new HashSet<int>();

        // list storing the position/Timepoint of the touch points when touched down
        List<double[]> downPoints_Pos = new List<double[]>();

        // list storing the position, touched and left Timepoints of the touch points
        // one element: [point_id, touched_timepoint, touched_x, touched_y, left_timepoint, left_x, left_y, disFromTarget]
        List<double[]> touchPoints_PosTimeDis = new List<double[]>();

        // Stop Watch for recording the time interval between the first touchpoint and the last touchpoint within One Touch
        Stopwatch tpoints1TouchWatch;
        // the Max Duration for One Touch (ms)
        long tMax_1Touch = 100;
        GoTargetTouchState gotargetTouchstate;


        // executeresult of each trial
        TrialExeResult trialExeResult;

        ScreenTouchState screenTouchstate;


        // hold states for Ready, Cue Interfaces
        StartPadHoldState startpadHoldstate;



        // serial port for DLP-IO8-G
        SerialPort serialPort_IO8;
        int baudRate = 115200;
        Thread thread_ReadWrite_IO8;


        // commands for setting dig out high/low for channels
        static string cmdDigIn1 = "A";
        static string cmdHigh3 = "3";
        static string cmdLow3 = "E";

        // Channel 1 Digital Input for Startpad, Channel 3 for Juicer 
        static string startpad_In = cmdDigIn1;
        static string codeHigh_JuicerPin = cmdHigh3, codeLow_JuicerPin = cmdLow3;

        static int startpad_DigIn_Pressed = 0;
        static int startpad_DigIn_Unpressed = 1;


        static string Code_InitState = "0000";
        static string Code_TouchTriggerTrial = "1110";
        static string Code_ReadyShown = "0110";
        static string Code_ReadyWaitTooShort = "0011";
        static string Code_GoTargetShown = "1010";
        static string Code_GoReactionTooLong = "1100";
        static string Code_GoReachTooLong = "1011";
        static string Code_GoTouched = "1101";
        static string Code_GoTouchedHit = "0100";
        static string Code_GoTouchedMiss = "1000";
        static string Code_CueShown = "0001";
        static string Code_CueWaitTooShort = "0101";
        static string Code_noGoTargetShown = "0010";
        static string Code_noGoEnoughTCorrectFeedback = "0111";
        static string Code_LeaveStartpad = "1001";


        string TDTCmd_InitState, TDTCmd_TouchTriggerTrial, TDTCmd_ReadyShown, TDTCmd_ReadyWaitTooShort, TDTCmd_LeaveStartpad; 
        string TDTCmd_GoTargetShown, TDTCmd_GoReactionTooLong, TDTCmd_GoReachTooLong, TDTCmd_GoTouched, TDTCmd_GoTouchedHit, TDTCmd_GoTouchedMiss;
        string TDTCmd_CueShown, TDTCmd_CueWaitTooShort;
        string TDTCmd_noGoTargetShown, TDTCmd_noGoEnoughTCorrectFeedback;


        /* startpad parameters */
        PressedStartpad pressedStartpad;
        public delegate void UpdateTextCallback(string message);

        /*Juicer Parameters*/
        GiveJuicerState giveJuicerState;
        // juiver given duration(ms)
        int t_JuicerCorrectGiven;


        // Global stopwatch
        Stopwatch globalWatch;


        // Variables for Various Time Points during trials
        long timePoint_StartpadTouched, timePoint_StartpadLeft;
        long timePoint_Interface_ReadyOnset, timePoint_Interface_CueOnset, timePoint_Interface_TargetOnset;


        // Varaibles for Current Session, Trials Left in Current Session (Realtime Feedback)
        int currSessi, trialNLeftInSess;

        /*****Methods*******/
        public presentation(MainWindow mainWindow)
        {
            InitializeComponent();

            Touch.FrameReported += new TouchFrameEventHandler(Touch_FrameReported);

            // parent
            parent = mainWindow;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;

            // get the setup from the parent interface
            GetSetupParameters();

            // Create 
            trialTargetInfo_PerSess_List = Create_TrialTargetInfo_List(totalTrialNumPerPosSess, nogoTrialNumPerPosSess, targetPosNum);


            // Create necessary elements: go circle, nogo rect, two white points and one crossing
            Create_GoCircle();
            Create_NogoRect();
            Create_TwoWhitePoints();
            Create_OneCrossing();

            // Set audio Feedback related members 
            SetAudioFeedback();

            //
            Generate_IO8EventTDTCmd();

            PrepBef_Present();
        }

        private string Convert2_IO8EventCmd_Bit5to8(string EventCode)
        {/*
            Generate IO8 Event Command based on EventCode using bit 5-8
            E.g. "0000" -> "TYUI", "1111" -> "5678", "1010" -> "5Y7I"
            */

            string cmdHigh5 = "5";
            string cmdLow5 = "T";
            string cmdHigh6 = "6";
            string cmdLow6 = "Y";
            string cmdHigh7 = "7";
            string cmdLow7 = "U";
            string cmdHigh8 = "8";
            string cmdLow8 = "I";

            string IO8EventCmd = cmdLow5 + cmdLow6 + cmdLow7 + cmdLow8;
            if (EventCode[0] == '1')
                IO8EventCmd  = IO8EventCmd.Remove(0, 1).Insert(0, cmdHigh5);
            if (EventCode[1] == '1')
                IO8EventCmd = IO8EventCmd.Remove(1, 1).Insert(1, cmdHigh6);
            if (EventCode[2] == '1')
                IO8EventCmd = IO8EventCmd.Remove(2, 1).Insert(2, cmdHigh7);
            if (EventCode[3] == '1')
                IO8EventCmd = IO8EventCmd.Remove(3, 1).Insert(3, cmdHigh8);

            return IO8EventCmd;
        }
        private void Generate_IO8EventTDTCmd()
        {
            TDTCmd_InitState = Convert2_IO8EventCmd_Bit5to8(Code_InitState);
            TDTCmd_TouchTriggerTrial = Convert2_IO8EventCmd_Bit5to8(Code_TouchTriggerTrial);
            TDTCmd_ReadyShown = Convert2_IO8EventCmd_Bit5to8(Code_ReadyShown);
            TDTCmd_ReadyWaitTooShort = Convert2_IO8EventCmd_Bit5to8(Code_ReadyWaitTooShort);
            TDTCmd_GoTargetShown = Convert2_IO8EventCmd_Bit5to8(Code_GoTargetShown);
            TDTCmd_GoReactionTooLong = Convert2_IO8EventCmd_Bit5to8(Code_GoReactionTooLong);
            TDTCmd_GoReachTooLong = Convert2_IO8EventCmd_Bit5to8(Code_GoReachTooLong);
            TDTCmd_GoTouched = Convert2_IO8EventCmd_Bit5to8(Code_GoTouched);
            TDTCmd_GoTouchedHit = Convert2_IO8EventCmd_Bit5to8(Code_GoTouchedHit);
            TDTCmd_GoTouchedMiss = Convert2_IO8EventCmd_Bit5to8(Code_GoTouchedMiss);
            TDTCmd_CueShown = Convert2_IO8EventCmd_Bit5to8(Code_CueShown);
            TDTCmd_CueWaitTooShort = Convert2_IO8EventCmd_Bit5to8(Code_CueWaitTooShort);
            TDTCmd_noGoTargetShown = Convert2_IO8EventCmd_Bit5to8(Code_noGoTargetShown);
            TDTCmd_noGoEnoughTCorrectFeedback = Convert2_IO8EventCmd_Bit5to8(Code_noGoEnoughTCorrectFeedback);
            TDTCmd_LeaveStartpad = Convert2_IO8EventCmd_Bit5to8(Code_LeaveStartpad);
        }
        private void PrepBef_Present()
        {

            // create a serial Port IO8 instance, and open it
            serialPort_IO8 = new SerialPort();
            try
            {
                serialPort_SetOpen(parent.serialPortIO8_name, baudRate);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            // Thread for Read and Write IO8
            thread_ReadWrite_IO8 = new Thread(new ThreadStart(Thread_ReadWrite_IO8));


            // init a global stopwatch
            globalWatch = new Stopwatch();
            tpoints1TouchWatch = new Stopwatch();

            // Init Trial Information
            Init_FeedbackTrialsInformation();

            //Write Trial Setup Information
            Save_TrialSetupInformation();
        }

        private void Save_TrialSetupInformation()
        {
            using (StreamWriter file = File.AppendText(file_saved))
            {
                file.WriteLine("\n\n");

                file.WriteLine(String.Format("{0, -40}", "Trial Information"));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Unit of Touch Point X Y Position", "Pixal"));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Touch Point X Y Coordinate System", "(0,0) in Top Left Corner, Right and Down Direction is Positive"));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Unit of Event TimePoint/Time", "Second"));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Center Coordinates of Each Target", "((0,0) in Top Left Corner, Right and Down Direction is Positive)"));
                for (int i = 0; i < optPostions_OTopLeft_List.Count; i++)
                {
                    int[] position = optPostions_OTopLeft_List[i];
                    file.WriteLine(String.Format("{0, -40}:{1}, {2}", "Postion " + i.ToString(), position[0], position[1]));
                }
                file.WriteLine("\n");

                file.WriteLine(String.Format("{0, -40}", "Event Codes in TDT System:"));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(Code_InitState), Code_InitState));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(Code_TouchTriggerTrial), Code_TouchTriggerTrial));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(Code_LeaveStartpad), Code_LeaveStartpad));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(Code_ReadyShown), Code_ReadyShown));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(Code_ReadyWaitTooShort), Code_ReadyWaitTooShort));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(Code_CueShown), Code_CueShown));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(Code_CueWaitTooShort), Code_CueWaitTooShort));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(Code_GoTargetShown), Code_GoTargetShown));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(Code_GoReactionTooLong), Code_GoReactionTooLong));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(Code_GoReachTooLong), Code_GoReachTooLong));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(Code_GoTouched), Code_GoTouched));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(Code_GoTouchedHit), Code_GoTouchedHit));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(Code_GoTouchedMiss), Code_GoTouchedMiss));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(Code_noGoTargetShown), Code_noGoTargetShown));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(Code_noGoEnoughTCorrectFeedback), Code_noGoEnoughTCorrectFeedback));
                file.WriteLine("\n");


                file.WriteLine(String.Format("{0, -40}", "IO8 Commands:"));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(TDTCmd_InitState), TDTCmd_InitState));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(TDTCmd_TouchTriggerTrial), TDTCmd_TouchTriggerTrial));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(TDTCmd_LeaveStartpad), TDTCmd_LeaveStartpad));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(TDTCmd_ReadyShown), TDTCmd_ReadyShown));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(TDTCmd_ReadyWaitTooShort), TDTCmd_ReadyWaitTooShort));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(TDTCmd_CueShown), TDTCmd_CueShown));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(TDTCmd_CueWaitTooShort), TDTCmd_CueWaitTooShort));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(TDTCmd_GoTargetShown), TDTCmd_GoTargetShown));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(TDTCmd_GoReactionTooLong), TDTCmd_GoReactionTooLong));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(TDTCmd_GoReachTooLong), TDTCmd_GoReachTooLong));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(TDTCmd_GoTouched), TDTCmd_GoTouched));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(TDTCmd_GoTouchedHit), TDTCmd_GoTouchedHit));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(TDTCmd_GoTouchedMiss), TDTCmd_GoTouchedMiss));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(TDTCmd_noGoTargetShown), TDTCmd_noGoTargetShown));
                file.WriteLine(String.Format("{0, -40}:  {1}", nameof(TDTCmd_noGoEnoughTCorrectFeedback), TDTCmd_noGoEnoughTCorrectFeedback));
                file.WriteLine("\n");
            }
        }

        private void Init_FeedbackTrialsInformation()
        {/* Init the Feedback Trial Information in the Mainwindow */


            currSessi = 0;
            trialNLeftInSess = totalTrialNumPerPosSess * targetPosNum;

            TargetExeFeedback_List = new List<int[]>();
            for (int i = 0; i < targetPosNum; i++)
            {
                TargetExeFeedback_List.Add(new int[] {0, 0, 0, 0});
            }

            // Update Main Window Feedback 
            parent.textBox_feedback_currSessioni.Text = currSessi.ToString();
            parent.textBox_feedback_TrialsLeftCurrSess.Text = trialNLeftInSess.ToString();

            int[] targetExeFeedback;
            targetExeFeedback = TargetExeFeedback_List[0];
            parent.textBox_feedback_Targ0TotalGo.Text = targetExeFeedback[0].ToString();
            parent.textBox_feedback_Targ0SuccessGo.Text = targetExeFeedback[1].ToString();
            parent.textBox_feedback_Targ0TotalNogo.Text = targetExeFeedback[2].ToString();
            parent.textBox_feedback_Targ0SuccessNogo.Text = targetExeFeedback[3].ToString();
            targetExeFeedback = TargetExeFeedback_List[1];
            parent.textBox_feedback_Targ1TotalGo.Text = targetExeFeedback[0].ToString();
            parent.textBox_feedback_Targ1SuccessGo.Text = targetExeFeedback[1].ToString();
            parent.textBox_feedback_Targ1TotalNogo.Text = targetExeFeedback[2].ToString();
            parent.textBox_feedback_Targ1SuccessNogo.Text = targetExeFeedback[3].ToString();
            targetExeFeedback = TargetExeFeedback_List[2];
            parent.textBox_feedback_Targ2TotalGo.Text = targetExeFeedback[0].ToString();
            parent.textBox_feedback_Targ2SuccessGo.Text = targetExeFeedback[1].ToString();
            parent.textBox_feedback_Targ2TotalNogo.Text = targetExeFeedback[2].ToString();
            parent.textBox_feedback_Targ2SuccessNogo.Text = targetExeFeedback[3].ToString();
        }


        public async void Present_Start()
        {                 
            float t_Cue, t_Ready, t_noGoShow;
            int[] pos_Taget_OTopLeft;
            int totalTrialNumPerSess = totalTrialNumPerPosSess * targetPosNum;

            // Present Each Trial
            globalWatch.Restart();
            thread_ReadWrite_IO8.Start();
            timestamp_0 = DateTime.Now.Ticks;
            int totalTriali = 0;
            PresentTrial = true;
            while (PresentTrial)
            {
                currSessi++;

                // Write Current Session number 
                using (StreamWriter file = File.AppendText(file_saved))
                {
                    file.WriteLine("\n\n");
                    file.WriteLine(String.Format("{0, -40}: {1}", "Session", currSessi.ToString()));
                }

                
                ShuffleTrials_GenRandomTime();

                int sessTriali = 0;
                trialNLeftInSess = totalTrialNumPerSess - sessTriali;
                while (sessTriali < trialTargetInfo_PerSess_List.Count)
                {
                    // Extract trial parameters
                    int[] targetInfo = trialTargetInfo_PerSess_List[sessTriali];
                    currTrialTargetPosInd = targetInfo[0];
                    pos_Taget_OTopLeft = optPostions_OTopLeft_List[currTrialTargetPosInd];
                    targetType = (TargetType)targetInfo[1];
                    t_Cue = t_Cue_List[sessTriali];
                    t_Ready = t_Ready_List[sessTriali];
                    t_noGoShow = t_noGoShow_List[sessTriali];

                    try
                    {
                        serialPort_IO8.WriteLine(TDTCmd_InitState);
                    }
                    catch (InvalidOperationException) { }
                        


                    /*----- WaitStartTrial Interface ------*/
                    pressedStartpad = PressedStartpad.No;
                    await Interface_WaitStartTrial();

                    if (PresentTrial == false)
                    {
                        break;
                    }

                    /*-------- Trial Interfaces -------*/
                    try
                    {
                        // Ready Interface
                        await Interface_Ready(t_Ready);

                        if (PresentTrial == false)
                        {
                            break;
                        }

                        // Cue Interface
                        await Interface_Cue(t_Cue, pos_Taget_OTopLeft);

                        if (PresentTrial == false)
                        {
                            break;
                        }

                        // Go or noGo Target Interface
                        sessTriali++;
                        if (targetType == TargetType.Go)
                        {
                            await Interface_Go(pos_Taget_OTopLeft);
                            if (PresentTrial == false)
                            {
                                break;
                            }
                        }
                        else
                        {
                            await Interface_noGo(t_noGoShow, pos_Taget_OTopLeft);
                            if (PresentTrial == false)
                            {
                                break;
                            }
                        }

                        Remove_All();
                    }
                    catch (TaskCanceledException)
                    {
                        Remove_All();
                    }

                    try
                    {
                        serialPort_IO8.WriteLine(TDTCmd_InitState);
                    }
                    catch (InvalidOperationException) { }


                    // totalTriali including trials fail during Ready and Cue Phases
                    totalTriali++;
                    trialNLeftInSess = totalTrialNumPerSess - sessTriali;
                    Update_FeedbackTrialsInformation();

                    /*-------- Write Trial Information ------*/
                    List<String> strExeSubResult = new List<String>();
                    strExeSubResult.Add("readyWaitTooShort");
                    strExeSubResult.Add("cueWaitTooShort");
                    strExeSubResult.Add("goReactionTimeToolong");
                    strExeSubResult.Add("goReachTimeToolong");
                    strExeSubResult.Add("goMiss");
                    strExeSubResult.Add("goSuccess");
                    strExeSubResult.Add("nogoMoved");
                    strExeSubResult.Add("nogoSuccess");
                    String strExeFail = "Failed";
                    String strExeSuccess = "Success";
                    using (StreamWriter file = File.AppendText(file_saved))
                    {
                        decimal ms2sRatio = 1000;

                        if (totalTriali > 1)
                        { // Startpad touched in trial i+1 treated as the return point as in trial i        

                            file.WriteLine(String.Format("{0, -40}: {1}", "Returned to Startpad TimePoint", (timePoint_StartpadTouched / ms2sRatio).ToString()));
                        }


                        /* Current Trial Written Inf*/
                        file.WriteLine("\n");

                        // Trial Num
                        file.WriteLine(String.Format("{0, -40}: {1}", "TrialNum", totalTriali.ToString()));

                        // the timepoint when touching the startpad to initial a new trial
                        file.WriteLine(String.Format("{0, -40}: {1}", "Startpad Touched TimePoint", (timePoint_StartpadTouched / ms2sRatio).ToString()));

                        // Start Interface showed TimePoint
                        file.WriteLine(String.Format("{0, -40}: {1}", "Ready Start TimePoint", (timePoint_Interface_ReadyOnset / ms2sRatio).ToString()));

                        // Ready Time
                        file.WriteLine(String.Format("{0, -40}: {1}", "Ready Interface Time", t_Ready.ToString()));


                        // Various Cases
                        if (trialExeResult == TrialExeResult.readyWaitTooShort)
                        {// case: ready WaitTooShort

                            // Left startpad early during ready
                            file.WriteLine(String.Format("{0, -40}: {1}", "Startpad Left TimePoint", (timePoint_StartpadLeft / ms2sRatio).ToString()));

                            // trial exe result : success or fail
                            file.WriteLine(String.Format("{0, -40}: {1}, {2}", "Trial Result", strExeFail, strExeSubResult[0]));
                        }
                        else if (trialExeResult == TrialExeResult.cueWaitTooShort)
                        {// case: Cue WaitTooShort

                            // Cue Interface Timepoint, Cue Time and Left startpad early during Cue
                            file.WriteLine(String.Format("{0, -40}: {1}", "Cue Start TimePoint", (timePoint_Interface_CueOnset / ms2sRatio).ToString()));
                            file.WriteLine(String.Format("{0, -40}: {1}", "Cue Interface Time", t_Cue.ToString()));
                            file.WriteLine(String.Format("{0, -40}: {1}", "Startpad Left TimePoint", (timePoint_StartpadLeft / ms2sRatio).ToString()));

                            // trial exe result : success or fail
                            file.WriteLine(String.Format("{0, -40}: {1}, {2}", "Trial Result", strExeFail, strExeSubResult[1]));
                        }
                        else if (trialExeResult == TrialExeResult.goReactionTimeToolong)
                        {// case : goReactionTimeToolong 

                            // Cue Interface Timepoint and Cue Time
                            file.WriteLine(String.Format("{0, -40}: {1}", "Cue Start TimePoint", (timePoint_Interface_CueOnset / ms2sRatio).ToString()));
                            file.WriteLine(String.Format("{0, -40}: {1}", "Cue Interface Time", t_Cue.ToString()));

                            // Target Interface Timepoint, Target type: Go, and Target position index: 0 (1, 2)
                            file.WriteLine(String.Format("{0, -40}: {1}", "Target Start TimePoint", (timePoint_Interface_TargetOnset / ms2sRatio).ToString()));
                            file.WriteLine(String.Format("{0, -40}: {1}", "TargetType", targetType.ToString()));
                            file.WriteLine(String.Format("{0, -40}: {1}", "TargetPositionIndex", currTrialTargetPosInd.ToString()));


                            // trial exe result : success or fail
                            file.WriteLine(String.Format("{0, -40}: {1}, {2}", "Trial Result", strExeFail, strExeSubResult[2]));
                        }
                        else if (trialExeResult == TrialExeResult.goReachTimeToolong)
                        {// case : goReachTimeToolong

                            // Cue Interface Timepoint and Cue Time
                            file.WriteLine(String.Format("{0, -40}: {1}", "Cue Start TimePoint", (timePoint_Interface_CueOnset / ms2sRatio).ToString()));
                            file.WriteLine(String.Format("{0, -40}: {1}", "Cue Interface Time", t_Cue.ToString()));

                            // Target Interface Timepoint, Target type: Go, and Target position index: 0 (1, 2)
                            file.WriteLine(String.Format("{0, -40}: {1}", "Target Start TimePoint", (timePoint_Interface_TargetOnset / ms2sRatio).ToString()));
                            file.WriteLine(String.Format("{0, -40}: {1}", "TargetType", targetType.ToString()));
                            file.WriteLine(String.Format("{0, -40}: {1}", "TargetPositionIndex", currTrialTargetPosInd.ToString()));
                            // Target interface:  Left Startpad Time Point
                            file.WriteLine(String.Format("{0, -40}: {1}", "Startpad Left TimePoint", (timePoint_StartpadLeft / ms2sRatio).ToString()));


                            // trial exe result : success or fail
                            file.WriteLine(String.Format("{0, -40}: {1}, {2}", "Trial Result", strExeFail, strExeSubResult[3]));
                        }
                        else if (trialExeResult == TrialExeResult.goClose | trialExeResult == TrialExeResult.goHit | trialExeResult == TrialExeResult.goMiss)
                        {// case: Go success (goClose or goHit) or goMiss

                            // Cue Interface Timepoint and Cue Time
                            file.WriteLine(String.Format("{0, -40}: {1}", "Cue Start TimePoint", (timePoint_Interface_CueOnset / ms2sRatio).ToString()));
                            file.WriteLine(String.Format("{0, -40}: {1}", "Cue Interface Time", t_Cue.ToString()));

                            // Target Interface Timepoint, Target type: Go, and Target position index: 0 (1, 2)
                            file.WriteLine(String.Format("{0, -40}: {1}", "Target Start TimePoint", (timePoint_Interface_TargetOnset / ms2sRatio).ToString()));
                            file.WriteLine(String.Format("{0, -40}: {1}", "TargetType", targetType.ToString()));
                            file.WriteLine(String.Format("{0, -40}: {1}", "TargetPositionIndex", currTrialTargetPosInd.ToString()));

                            // Target interface:  Left Startpad Time Point
                            file.WriteLine(String.Format("{0, -40}: {1}", "Startpad Left TimePoint", (timePoint_StartpadLeft / ms2sRatio).ToString()));

                            //  Target interface:  touched  timepoint and (x, y position) of all touch points
                            for (int pointi = 0; pointi < touchPoints_PosTimeDis.Count; pointi++)
                            {
                                double[] downPoint = touchPoints_PosTimeDis[pointi];
                                
                                // touched pointi touchpoint
                                file.WriteLine(String.Format("{0, -40}: {1, -40}", "Touch Point " + pointi.ToString() + " TimePoint", ((decimal)downPoint[1] / ms2sRatio).ToString()));

                                // touched pointi position
                                file.WriteLine(String.Format("{0, -40}: {1}", "Touch Point " + pointi.ToString() + " XY Position (Distance)", downPoint[2].ToString() + ", " + downPoint[3].ToString() + "(" + downPoint[7].ToString() + ")"));

                            }

                            //  Target interface:  left timepoint and (x, y position) of all touch points
                            for (int pointi = 0; pointi < touchPoints_PosTimeDis.Count; pointi++)
                            {
                                double[] downPoint = touchPoints_PosTimeDis[pointi];

                                // left pointi touchpoint
                                file.WriteLine(String.Format("{0, -40}: {1, -40}", "Left Point " + pointi.ToString() + " TimePoint", ((decimal)downPoint[4] / ms2sRatio).ToString()));

                                // left pointi position
                                file.WriteLine(String.Format("{0, -40}: {1}", "Left Point " + pointi.ToString() + " XY Position", downPoint[5].ToString() + ", " + downPoint[6].ToString()));
                            }


                            // trial exe result : success or fail
                            if (trialExeResult == TrialExeResult.goMiss)
                                file.WriteLine(String.Format("{0, -40}: {1}, {2}", "Trial Result", strExeFail, strExeSubResult[4]));
                            else
                                file.WriteLine(String.Format("{0, -40}: {1}, {2}", "Trial Result", strExeSuccess, strExeSubResult[5]));

                        }
                        else if (trialExeResult == TrialExeResult.nogoMoved)
                        { // case: noGo moved 

                            // Cue Interface Timepoint and Cue Time
                            file.WriteLine(String.Format("{0, -40}: {1}", "Cue Start TimePoint", (timePoint_Interface_CueOnset / ms2sRatio).ToString()));
                            file.WriteLine(String.Format("{0, -40}: {1}", "Cue Interface Time", t_Cue.ToString()));

                            // Cue Interface Timepoint, Target type: Go, and Target position index: 0 (1, 2)
                            file.WriteLine(String.Format("{0, -40}: {1}", "Target Start TimePoint", (timePoint_Interface_TargetOnset / ms2sRatio).ToString()));
                            file.WriteLine(String.Format("{0, -40}: {1}", "TargetType", targetType.ToString()));
                            file.WriteLine(String.Format("{0, -40}: {1}", "TargetPositionIndex", currTrialTargetPosInd.ToString()));

                            // Target nogo interface show time
                            file.WriteLine(String.Format("{0, -40}: {1}", "Nogo Interface Show Time", t_noGoShow.ToString()));

                            // Target interface:  Left Startpad Time Point
                            file.WriteLine(String.Format("{0, -40}: {1}", "Startpad Left TimePoint", (timePoint_StartpadLeft / ms2sRatio).ToString()));



                            // trial exe result : success or fail
                            file.WriteLine(String.Format("{0, -40}: {1}, {2}", "Trial Result", strExeFail, strExeSubResult[6]));

                        }
                        else if (trialExeResult == TrialExeResult.nogoSuccess)
                        { // case: noGo success 

                            // Cue Interface Timepoint and Cue Time
                            file.WriteLine(String.Format("{0, -40}: {1}", "Cue Start TimePoint", (timePoint_Interface_CueOnset / ms2sRatio).ToString()));
                            file.WriteLine(String.Format("{0, -40}: {1}", "Cue Interface Time", t_Cue.ToString()));

                            // Cue Interface Timepoint, Target type: Go, and Target position index: 0 (1, 2)
                            file.WriteLine(String.Format("{0, -40}: {1}", "Target Start TimePoint", (timePoint_Interface_TargetOnset / ms2sRatio).ToString()));
                            file.WriteLine(String.Format("{0, -40}: {1}", "TargetType", targetType.ToString()));
                            file.WriteLine(String.Format("{0, -40}: {1}", "TargetPositionIndex", currTrialTargetPosInd.ToString()));
                            // Target nogo interface show time
                            file.WriteLine(String.Format("{0, -40}: {1}", "Nogo Interface Show Time", t_noGoShow.ToString()));


                            // trial exe result : success or fail
                            file.WriteLine(String.Format("{0, -40}: {1}, {2}", "Trial Result", strExeSuccess, strExeSubResult[7]));
                        }

                    }
                }
            }

            // Detect the return to startpad timepoint for the last trial
            pressedStartpad = PressedStartpad.No;
            try
            {
                await Wait_Return2StartPad(1);
            }
            catch (TaskCanceledException)
            {
                using (StreamWriter file = File.AppendText(file_saved))
                {
                    file.WriteLine(String.Format("{0, -40}: {1}", "Returned to Startpad TimePoint", timePoint_StartpadTouched.ToString()));
                }
            }

            // save the summary of exp
            SaveSummaryofExp();
        }


        private void SaveSummaryofExp()
        {
            /*Save the summary information of the exp*/

            using (StreamWriter file = File.AppendText(file_saved))
            {
                file.WriteLine("\n\n");
            }

        }


        public void Update_FeedbackTrialsInformation()
        {/* Update the Feedback Trial Information in the Mainwindow */

            parent.textBox_feedback_currSessioni.Text = currSessi.ToString();
            parent.textBox_feedback_TrialsLeftCurrSess.Text = trialNLeftInSess.ToString();

            int[] targetExeFeedback;
            targetExeFeedback = TargetExeFeedback_List[0];
            parent.textBox_feedback_Targ0TotalGo.Text = targetExeFeedback[0].ToString();
            parent.textBox_feedback_Targ0SuccessGo.Text = targetExeFeedback[1].ToString();
            parent.textBox_feedback_Targ0TotalNogo.Text = targetExeFeedback[2].ToString();
            parent.textBox_feedback_Targ0SuccessNogo.Text = targetExeFeedback[3].ToString();
            targetExeFeedback = TargetExeFeedback_List[1];
            parent.textBox_feedback_Targ1TotalGo.Text = targetExeFeedback[0].ToString();
            parent.textBox_feedback_Targ1SuccessGo.Text = targetExeFeedback[1].ToString();
            parent.textBox_feedback_Targ1TotalNogo.Text = targetExeFeedback[2].ToString();
            parent.textBox_feedback_Targ1SuccessNogo.Text = targetExeFeedback[3].ToString();
            targetExeFeedback = TargetExeFeedback_List[2];
            parent.textBox_feedback_Targ2TotalGo.Text = targetExeFeedback[0].ToString();
            parent.textBox_feedback_Targ2SuccessGo.Text = targetExeFeedback[1].ToString();
            parent.textBox_feedback_Targ2TotalNogo.Text = targetExeFeedback[2].ToString();
            parent.textBox_feedback_Targ2SuccessNogo.Text = targetExeFeedback[3].ToString();
        }

        public void Present_Stop()
        {
            PresentTrial = false;
            thread_ReadWrite_IO8.Abort();
            globalWatch.Stop();

            // After Trials Presentation
            if (serialPort_IO8.IsOpen)
                serialPort_IO8.Close();
            tpoints1TouchWatch.Stop();
        }


        private void SetAudioFeedback()
        {/*set the player_Correct and player_Error members
            */

            player_Correct = new System.Media.SoundPlayer(Properties.Resources.Correct);
            player_Error = new System.Media.SoundPlayer(Properties.Resources.Error);


            // Assign new audios
            if (String.Compare(audioFile_Correct, "default", true) != 0)
            {
                player_Correct.SoundLocation = audioFile_Correct;
            }
            if (String.Compare(audioFile_Error, "default", true) != 0)
            {
                player_Error.SoundLocation = audioFile_Error;
            }

            

        }


        private void serialPort_SetOpen (string portName, int baudRate)
        {
            try
            {
                serialPort_IO8.PortName = portName;
                serialPort_IO8.BaudRate = baudRate;
                serialPort_IO8.Open();  
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<int[]> Create_TrialTargetInfo_List(int trialNumPerPos, int nogoTrialNumPerPos, int posNum)
        {/*
            Parameters:
                trialNumPerPos: total Trial Number Per Position(trialNum = goTrialNum + nogoTrialNum)
                nogoTrialNumPerPos: nogo Trial Number
                posNum: total Position Number

            Return:
                trialTargetInfo_List: Each Trial Target Information List ([posIndex, gonogoIndex]), gonogoIndex = 0(nogo), 1(go)
                e.g. trialNumPerPos = 5; nogoTrialNumPerPos = 2; posNum = 3;
                [   [0, 0], [0, 0], [0, 1], [0, 1], [0, 1],
                    [1, 0], [1, 0], [1, 1], [1, 1], [1, 1],
                    [2, 0], [2, 0], [2, 1], [2, 1], [2, 1]
                ], 
            */

            List<int[]> trialTargetInfo_List = new List<int[]>();

            for(int posi = 0; posi < posNum; posi++)
            {
                for(int goNogoi = 0; goNogoi < trialNumPerPos; goNogoi++)
                {
                    int goNogoIndex = (int)((goNogoi < nogoTrialNumPerPos) ? TargetType.Nogo : TargetType.Go);
                    trialTargetInfo_List.Add(new int[] { posi, goNogoIndex });
                }
                    
            }

            return trialTargetInfo_List;
        }

        private void ShuffleTrials_GenRandomTime()
        {/* ---- 
            1. shuffle trials, i.e Shuffle trialTargetInfo_PerSess_List
            2. Generate the random t_Ready, t_Cue, t_noGoShow for each trial, stored in t_Ready_List, t_Cue_List, t_noGoShow_List;
             */

            // Shuffle trialTargetInfo_PerSess_List
            trialTargetInfo_PerSess_List = Utility.Shuffle(trialTargetInfo_PerSess_List);


            // generate a random t_Ready and t_Cue, and and them into t_Ready_List and t_Cue_List individually
            Random rand = new Random();
            for (int i = 0; i< trialTargetInfo_PerSess_List.Count; i++)
            {
                t_Cue_List.Add(TransferTo((float)rand.NextDouble(), tRange_CueTime[0], tRange_CueTime[1]));
                t_Ready_List.Add(TransferTo((float)rand.NextDouble(), tRange_ReadyTime[0], tRange_ReadyTime[1]));
                t_noGoShow_List.Add(TransferTo((float)rand.NextDouble(), tRange_NogoShowTime[0], tRange_NogoShowTime[1]));
            }
        }


        private void GetSetupParameters()
        {/* get the setup from the parent interface */

            swf.Screen PrimaryScreen = Utility.TaskPresentTouchScreen();
            sd.Rectangle Rect_touchScreen = PrimaryScreen.Bounds;
            int deltax = Rect_touchScreen.Width / 2;
            int deltay = Rect_touchScreen.Height / 2;
            optPostions_OTopLeft_List = new List<int[]>();
            foreach (int[] xyPos in parent.optPostions_OCenter_List)
            {
                optPostions_OTopLeft_List.Add(new int[] {xyPos[0] + deltax ,  xyPos[1] + deltay});
            } 

            targetPosNum = optPostions_OTopLeft_List.Count;
            totalTrialNumPerPosSess = int.Parse(parent.textBox_totalTrialNumPerPosSess.Text);
            nogoTrialNumPerPosSess = int.Parse(parent.textBox_nogoTrialNumPerPosSess.Text);

            // object size and distance parameters
            targetDiameterPixal = Utility.Inch2Pixal(parent.targetDiaInch);
            currCircleGo_Radius_Pixal = targetDiameterPixal / 2;

            // interfaces time related parameters
            tRange_ReadyTime = parent.tRange_ReadyTimeS;
            tRange_CueTime = parent.tRange_CueTimeS;
            tRange_NogoShowTime = parent.tRange_NogoShowTimeS;
            tMax_ReactionTimeMS = parent.tMax_ReactionTimeS * 1000;
            tMax_ReachTimeMS = parent.tMax_ReachTimeS * 1000;
            t_InterTrialMS = (Int32)(parent.t_InterTrialS * 1000);
            t_VisfeedbackShow = (Int32)(parent.t_VisfeedbackShowS * 1000);
            

            // Juicer Time
            t_JuicerCorrectGiven = (Int32)(parent.t_JuicerCorrectGivenS * 1000);


            /* ---- Get all the Set Colors ----- */
            Color selectedColor;
            // goCircle Color
            selectedColor = (Color)(typeof(Colors).GetProperty(parent.goFillColorStr) as PropertyInfo).GetValue(null, null);
            brush_goCircleFill = new SolidColorBrush(selectedColor);

            // nogoRect Color
            selectedColor = (Color)(typeof(Colors).GetProperty(parent.nogoFillColorStr) as PropertyInfo).GetValue(null, null);
            brush_nogoRectFill = new SolidColorBrush(selectedColor);

            // Cue Crossing Color
            selectedColor = (Color)(typeof(Colors).GetProperty(parent.cueCrossingColorStr) as PropertyInfo).GetValue(null, null);
            brush_CueCrossing = new SolidColorBrush(selectedColor);


            // Wait Background 
            selectedColor = (Color)(typeof(Colors).GetProperty(parent.BKWaitTrialColorStr) as PropertyInfo).GetValue(null, null);
            brush_BKWaitTrialStart = new SolidColorBrush(selectedColor);
            // Wait Boarder
            brush_BDWaitTrialStart = brush_BKWaitTrialStart;

            // Trial Background
            selectedColor = (Color)(typeof(Colors).GetProperty(parent.BKTrialColorStr) as PropertyInfo).GetValue(null, null);
            brush_BKTrial = new SolidColorBrush(selectedColor);

            // Correct Fill Color
            selectedColor = (Color)(typeof(Colors).GetProperty(parent.CorrFillColorStr) as PropertyInfo).GetValue(null, null);
            brush_CorrectFill = new SolidColorBrush(selectedColor);

            // Correct Outline Color
            selectedColor = (Color)(typeof(Colors).GetProperty(parent.CorrOutlineColorStr) as PropertyInfo).GetValue(null, null);
            brush_CorrOutline = new SolidColorBrush(selectedColor);

            // Error Fill Color
            selectedColor = (Color)(typeof(Colors).GetProperty(parent.ErrorFillColorStr) as PropertyInfo).GetValue(null, null);
            brush_ErrorFill = new SolidColorBrush(selectedColor);

            // Error Outline Color
            selectedColor = (Color)(typeof(Colors).GetProperty(parent.ErrorOutlineColorStr) as PropertyInfo).GetValue(null, null);
            brush_ErrorOutline = new SolidColorBrush(selectedColor);

            
            // get the file for saving 
            file_saved = parent.file_saved;
            audioFile_Correct = parent.audioFile_Correct;
            audioFile_Error = parent.audioFile_Error;
        }


        private void Create_GoCircle()
        {/*
            Create the go circle: circleGo

            */

            // Create an Ellipse  
            circleGo = new Ellipse();
                   
            circleGo.Fill = brush_goCircleFill;

            // set the size, position of circleGo
            circleGo.Height = targetDiameterPixal;
            circleGo.Width = targetDiameterPixal;
            circleGo.VerticalAlignment = VerticalAlignment.Top;
            circleGo.HorizontalAlignment = HorizontalAlignment.Left;

            circleGo.Name = name_circleGo;
            circleGo.Visibility = Visibility.Hidden;
            circleGo.IsEnabled = false;

            // add to myGrid
            myGrid.Children.Add(circleGo);
            myGrid.RegisterName(circleGo.Name, circleGo);
            myGrid.UpdateLayout();
        }

        private void Show_GoCircle(int[] centerPoint_Pos_OTopLeft)
        {/*
            Show the GoCircle into cPoint_Pos_OCenter (Origin in the center of the Screen)

            Arg:
                centerPoint_Pos_OTopLeft: the x, y Positions of the Circle center in Pixal (Origin at TopLeft)

             */

            circleGo = Utility.Move_Circle_OTopLeft(circleGo, centerPoint_Pos_OTopLeft);
            circleGo.Fill = brush_goCircleFill;
            circleGo.Stroke = brush_goCircleFill;
            circleGo.Visibility = Visibility.Visible;
            circleGo.IsEnabled = true;
            myGrid.UpdateLayout();
        }
       
        private void Remove_GoCircle()
        {
            circleGo.Visibility = Visibility.Hidden;
            circleGo.IsEnabled = false;
            myGrid.UpdateLayout();
        }

        private void Create_NogoRect()
        {/*Create the red nogo rectangle: rectNogo*/

            // Create an Ellipse  
            rectNogo = new Rectangle();

            rectNogo.Fill = brush_nogoRectFill;

            // set the size, position of circleGo
            int square_width = targetDiameterPixal;
            int square_height = targetDiameterPixal;
            rectNogo.Height = square_height;
            rectNogo.Width = square_width;
            rectNogo.VerticalAlignment = VerticalAlignment.Top;
            rectNogo.HorizontalAlignment = HorizontalAlignment.Left;

            // name
            rectNogo.Name = name_rectNogo;

            // hidden and not enabled at first
            rectNogo.Visibility = Visibility.Hidden;
            rectNogo.IsEnabled = false;

            // add to myGrid   
            myGrid.Children.Add(rectNogo);
            myGrid.RegisterName(rectNogo.Name, rectNogo);
            myGrid.UpdateLayout();
        }

        private void Show_noGoRect(int[] centerPoint_Pos_OTopLeft)
        {/*
            Show the nogoRect into centerPoint_Pos_OTopLeft (Origin in the center of the Screen)

            Arg:
                centerPoint_Pos_OTopLeft: the x, y Positions of the Rectangle center in Pixal (Origin in TopLeft)

             */

            rectNogo = Utility.Move_Rect_OTopLeft(rectNogo, centerPoint_Pos_OTopLeft);
            rectNogo.Fill = brush_nogoRectFill;
            rectNogo.Stroke = brush_nogoRectFill;
            rectNogo.Visibility = Visibility.Visible;
            rectNogo.IsEnabled = true;
            myGrid.UpdateLayout();
        }

        private void Remove_NogoRect()
        {
            rectNogo.Visibility = Visibility.Hidden;
            rectNogo.IsEnabled = false;
            myGrid.UpdateLayout();
        }


        private void Create_TwoWhitePoints()
        {/* Create draw the two write points: point1, point2 */

            // Create a while Brush    
            SolidColorBrush whiteBrush = new SolidColorBrush();
            whiteBrush.Color = Colors.White;

            // the left white point
            point1 = new Ellipse();
            point1.Fill = whiteBrush;
            point1.Height = wpoints_radius;
            point1.Width = wpoints_radius;
            point1.HorizontalAlignment = HorizontalAlignment.Center;
            point1.VerticalAlignment = VerticalAlignment.Center;
            

            point1.Name = name_point1;

            point1.Visibility = Visibility.Hidden;
            point1.IsEnabled = false;
            myGrid.Children.Add(point1);
            myGrid.RegisterName(point1.Name, point1);


            // the top white point
            point2 = new Ellipse();
            point2.Fill = whiteBrush;
            point2.Height = wpoints_radius;
            point2.Width = wpoints_radius;
            point2.HorizontalAlignment = HorizontalAlignment.Center;
            point2.VerticalAlignment = VerticalAlignment.Center;
            

            point2.Name = name_point2;
            point2.Visibility = Visibility.Hidden;
            point2.IsEnabled = false;
            myGrid.Children.Add(point2);
            myGrid.RegisterName(point2.Name, point2);
            myGrid.UpdateLayout();



        }


        private void Remove_TwoWhitePoints()
        {// add nogo rectangle to myGrid

            point1.Visibility = Visibility.Hidden;
            point2.Visibility = Visibility.Hidden;

            myGrid.UpdateLayout();
        }

        private void Create_OneCrossing()
        {/*create the crossing cue*/

            // the line length of the crossing
            int len = targetDiameterPixal;

            // Create a while Brush    


            // Create the horizontal line
            horiLine = new Line();
            horiLine.X1 = 0;
            horiLine.Y1 = 0;
            horiLine.X2 = len;
            horiLine.Y2 = horiLine.Y1;        
            
            // horizontal line position
            horiLine.HorizontalAlignment = HorizontalAlignment.Left;
            horiLine.VerticalAlignment = VerticalAlignment.Top;
            
            // horizontal line color
            horiLine.Stroke = brush_CueCrossing;
            // horizontal line stroke thickness
            horiLine.StrokeThickness = crossingLineThickness;
            // name
            horiLine.Name = name_hLine;
            horiLine.Visibility = Visibility.Hidden;
            horiLine.IsEnabled = false;
            myGrid.Children.Add(horiLine);
            myGrid.RegisterName(horiLine.Name, horiLine);


            // Create the vertical line
            vertLine = new Line();
            vertLine.X1 = 0;
            vertLine.Y1 = 0;
            vertLine.X2 = vertLine.X1;
            vertLine.Y2 = len;
            // vertical line position
            vertLine.HorizontalAlignment = HorizontalAlignment.Left;
            vertLine.VerticalAlignment = VerticalAlignment.Top;
            
            // vertical line color
            vertLine.Stroke = brush_CueCrossing;
            // vertical line stroke thickness
            vertLine.StrokeThickness = crossingLineThickness;
            //name
            vertLine.Name = name_vLine;

            vertLine.Visibility = Visibility.Hidden;
            vertLine.IsEnabled = false;
            myGrid.Children.Add(vertLine);
            myGrid.RegisterName(vertLine.Name, vertLine);
            myGrid.UpdateLayout();
        }

        private void Show_OneCrossing(int[] centerPoint_Pos_OTopLeft)
        {/*     Show One Crossing Containing One Horizontal Line and One Vertical Line
            *   centerPoint_Pos_OTopLeft: The Center Point X, Y Position of the Two Lines Intersect, Origin at Screen TopLeft
            * 
             */

            int centerPoint_X = centerPoint_Pos_OTopLeft[0];
            int centerPoint_Y = centerPoint_Pos_OTopLeft[1];

            horiLine.Margin = new Thickness(centerPoint_X - targetDiameterPixal/2, centerPoint_Y, 0, 0);
            vertLine.Margin = new Thickness(centerPoint_X, centerPoint_Y - targetDiameterPixal / 2, 0, 0);

            horiLine.Visibility = Visibility.Visible;
            vertLine.Visibility = Visibility.Visible;
            myGrid.UpdateLayout();
        }

        private void Remove_OneCrossing()
        {
            horiLine.Visibility = Visibility.Hidden;
            vertLine.Visibility = Visibility.Hidden;
            myGrid.UpdateLayout();
        }

        private void Remove_All()
        {
            Remove_OneCrossing();
            Remove_TwoWhitePoints();
            Remove_GoCircle();
            Remove_NogoRect();
        }


        public float TransferTo(float value, float lower, float upper)
        {// transform value (0=<value<1) into a valueT (lower=<valueT<upper)

            float rndTime;
            rndTime = value * (upper - lower) + lower;

            return rndTime;
        }


        private void Thread_ReadWrite_IO8()
        {/* Thread for reading/writing serial port IO8*/

            Stopwatch startpadReadWatch = new Stopwatch();
            long startpadReadInterval = 30;

            try
            {
                serialPort_IO8.WriteLine(codeLow_JuicerPin);
            }
            catch (InvalidOperationException) { }
            
            startpadReadWatch.Start();
            while (serialPort_IO8.IsOpen)
            {
                try
                { 
                    // ----- Juicer Control
                    if (giveJuicerState == GiveJuicerState.CorrectGiven)
                    {
                        
                        serialPort_IO8.WriteLine(codeHigh_JuicerPin);
                        Thread.Sleep(t_JuicerCorrectGiven);
                        serialPort_IO8.WriteLine(codeLow_JuicerPin);
                        giveJuicerState = GiveJuicerState.No;
                    }
                    //--- End of Juicer Control

                    //--- Startpad Read
                    if (startpadReadWatch.ElapsedMilliseconds >= startpadReadInterval)
                    {
                        serialPort_IO8.WriteLine(startpad_In);

                        // Read the Startpad Voltage
                        string str_Read = serialPort_IO8.ReadExisting();

                        // Restart the startpadReadWatch
                        startpadReadWatch.Restart();

                        // parse the start pad voltage 
                        string[] str_DigIn = str_Read.Split();

                        if (!String.IsNullOrEmpty(str_DigIn[0]))
                        {
                            int digIn = int.Parse(str_DigIn[0]);

                            if (digIn == startpad_DigIn_Pressed && pressedStartpad == PressedStartpad.No)
                            {/* time point from notouched state to touched state */

                                pressedStartpad = PressedStartpad.Yes;
                            }
                            else if (digIn == startpad_DigIn_Unpressed && pressedStartpad == PressedStartpad.Yes)
                            {/* time point from touched state to notouched state */

                                // the time point for leaving startpad
                                timePoint_StartpadLeft = globalWatch.ElapsedMilliseconds;
                                serialPort_IO8.WriteLine(TDTCmd_LeaveStartpad);
                                pressedStartpad = PressedStartpad.No;
                            }
                        }
                    }
                }
                catch(InvalidOperationException) { }
            }

            startpadReadWatch.Stop();
        }



        private Task Interface_WaitStartTrial()
        {
            /* task for WaitStart interface
             * 
             * Wait for Touching Startpad to trigger a new Trial
             */

            Remove_All();
            myGrid.Background = brush_BKWaitTrialStart;
            //myGridBorder.BorderBrush = brush_BDWaitTrialStart;

            Task task_WaitStart = Task.Run(() =>
            {
                while (PresentTrial && pressedStartpad == PressedStartpad.No) ;


                if (pressedStartpad == PressedStartpad.Yes)
                {
                    // the time point for startpad touched
                    serialPort_IO8.WriteLine(TDTCmd_TouchTriggerTrial);
                    timePoint_StartpadTouched = globalWatch.ElapsedMilliseconds;
                }

            });

            return task_WaitStart;
        }


        private Task Wait_Return2StartPad(float t_maxWait)
        {
            /* 
             * Wait for Returning Back to Startpad 
             * 
             * Input: 
             *    t_maxWait: the maximum wait time for returning back (s)  
             */


            return Task.Run(() =>
            {
                Stopwatch waitWatch = new Stopwatch();
                waitWatch.Restart();
                bool waitEnoughTag = false;
                while (pressedStartpad == PressedStartpad.No && !waitEnoughTag)
                {
                    if (waitWatch.ElapsedMilliseconds >= t_maxWait * 1000)
                    {// Wait for t_maxWait
                        waitEnoughTag  = true;
                    }
                }

                waitWatch.Stop();


                if (pressedStartpad == PressedStartpad.Yes)
                {
                    throw new TaskCanceledException("A return touched occurred");
                }

            });
        }


        private Task Wait_EnoughTouch(float t_EnoughTouch)
        {
            /* 
             * Wait for Enough Touch Time
             * 
             * Input: 
             *    t_EnoughTouch: the required Touch time (s)  
             */

            Task task = null;

            // start a task and return it
            return Task.Run(() =>
            {
                Stopwatch touchedWatch = new Stopwatch();
                touchedWatch.Restart();
                
                while (PresentTrial && pressedStartpad == PressedStartpad.Yes && startpadHoldstate != StartPadHoldState.HoldEnough)
                {
                    if (touchedWatch.ElapsedMilliseconds >= t_EnoughTouch * 1000)
                    {/* touched with enough time */
                        startpadHoldstate = StartPadHoldState.HoldEnough;
                    }
                }
                touchedWatch.Stop();
                if (startpadHoldstate != StartPadHoldState.HoldEnough)
                {
                    throw new TaskCanceledException(task);
                }

            });
        }

        private async Task Interface_Ready(float t_Ready)
        {/* task for Ready interface:
            Show the Ready Interface while Listen to the state of the startpad. 
            * 
            * Output:
            *   startPadHoldstate_Ready = 
            *       StartPadHoldState.HoldEnough (if startpad is touched lasting t_Ready)
            *       StartPadHoldState.HoldTooShort (if startpad is released before t_Ready) 
            */
            
            try
            {
                myGrid.Background = brush_BKTrial;
                serialPort_IO8.WriteLine(TDTCmd_ReadyShown);
                timePoint_Interface_ReadyOnset = globalWatch.ElapsedMilliseconds;

                // Wait Startpad Hold Enough Time
                startpadHoldstate = StartPadHoldState.HoldTooShort;
                await Wait_EnoughTouch(t_Ready);

            }
            catch (TaskCanceledException)
            {
                // trial execute result: waitReadyTooShort 
                serialPort_IO8.WriteLine(TDTCmd_ReadyWaitTooShort);
                trialExeResult = TrialExeResult.readyWaitTooShort;
                
                Task task = null;
                throw new TaskCanceledException(task);
            }
            catch (InvalidOperationException) { }
        }

 
        public async Task Interface_Cue(float t_Cue, int[] onecrossingPos_OTopLeft)
        {/* task for Cue Interface 
            Show the Cue Interface while Listen to the state of the startpad. 
            
            Args:
                t_Cue: Cue interface showes duration(s)
                onecrossingPos_OTopLeft: the center X, Y position of the one crossing, Origin at Top Left

            * Output:
            *   startPadHoldstate_Cue = 
            *       StartPadHoldState.HoldEnough (if startpad is touched lasting t_Cue)
            *       StartPadHoldState.HoldTooShort (if startpad is released before t_Cue) 
            */

            try
            {
                //myGrid.Children.Clear();
                Remove_All();

                // add one crossing on the right middle
                Show_OneCrossing(onecrossingPos_OTopLeft);

                serialPort_IO8.WriteLine(TDTCmd_CueShown);
                timePoint_Interface_CueOnset = globalWatch.ElapsedMilliseconds;
                

                // wait target cue for several seconds
                startpadHoldstate = StartPadHoldState.HoldTooShort;
                await Wait_EnoughTouch(t_Cue);

            }
            catch (TaskCanceledException)
            {
             
                // Audio Feedback
                player_Error.Play();

                // trial execute result: waitCueTooShort 
                serialPort_IO8.WriteLine(TDTCmd_CueWaitTooShort);
                trialExeResult = TrialExeResult.cueWaitTooShort;
                

                Task task = null;
                throw new TaskCanceledException(task);
            }
            
        }


        private Task Wait_Reaction()
        {/* Wait for Reaction within tMax_ReactionTime */

            // start a task and return it
            return Task.Run(() =>
            {
                Stopwatch waitWatch = new Stopwatch();
                waitWatch.Start();
                while (PresentTrial && pressedStartpad == PressedStartpad.Yes)
                {
                    if (waitWatch.ElapsedMilliseconds >= tMax_ReactionTimeMS)
                    {/* No release Startpad within tMax_ReactionTime */
                        waitWatch.Stop();

                        serialPort_IO8.WriteLine(TDTCmd_GoReactionTooLong);
                        trialExeResult = TrialExeResult.goReactionTimeToolong;
                        

                        throw new TaskCanceledException("No Reaction within the Max Reaction Time");
                    }
                }
                waitWatch.Stop();
            });
        }

        private Task Wait_Reach()
        {/* Wait for Reach within tMax_ReachTime*/

            return Task.Run(() =>
            {
                Stopwatch waitWatch = new Stopwatch();
                waitWatch.Start();
                while (PresentTrial && screenTouchstate == ScreenTouchState.Idle)
                {
                    if (waitWatch.ElapsedMilliseconds >= tMax_ReachTimeMS)
                    {/*No Screen Touched within tMax_ReachTime*/
                        waitWatch.Stop();

                        serialPort_IO8.WriteLine(TDTCmd_GoReachTooLong);
                        trialExeResult = TrialExeResult.goReachTimeToolong;
                        

                        throw new TaskCanceledException("No Reach within the Max Reach Time");
                    }
                }
                downPoints_Pos.Clear();
                touchPoints_PosTimeDis.Clear();
                waitWatch.Restart();
                while (waitWatch.ElapsedMilliseconds <= tMax_1Touch) ;
                waitWatch.Stop();
                calc_GoTargetTouchState();
            }); 
        }

        private void calc_GoTargetTouchState()
        {/* Calculate GoTargetTouchState  
            1. based on the Touch Down Positions in  List downPoints_Pos and circleGo_centerPoint
            2. Assign the calculated target touch state to the GoTargetTouchState variable gotargetTouchstate
            */

            double distance;
            int[] currPosTarget_OTopLeft = new int[] { optPostions_OTopLeft_List[currTrialTargetPosInd][0], optPostions_OTopLeft_List[currTrialTargetPosInd][1] };

            gotargetTouchstate = GoTargetTouchState.goMissed;
            for(int i = 0; i < touchPoints_PosTimeDis.Count; i++)
            {
                distance = Math.Sqrt(Math.Pow((touchPoints_PosTimeDis[i][2] - currPosTarget_OTopLeft[0]), 2) + Math.Pow((touchPoints_PosTimeDis[i][3] - currPosTarget_OTopLeft[1]), 2));
                distance = Math.Round(distance);
                touchPoints_PosTimeDis[i][7] = distance;
                if (distance <= currCircleGo_Radius_Pixal)
                {// Hit 

                    serialPort_IO8.WriteLine(TDTCmd_GoTouchedHit);
                    gotargetTouchstate = GoTargetTouchState.goHit;
                }
            }

            if (gotargetTouchstate == GoTargetTouchState.goMissed)
            {
                serialPort_IO8.WriteLine(TDTCmd_GoTouchedMiss);
            }
        }
          

        private async Task Interface_Go(int[] pos_Target_OTopLeft)
        {/* task for Go Interface: Show the Go Interface while Listen to the state of the startpad.
            * 1. If Reaction time < Max Reaction Time or Reach Time < Max Reach Time, end up with long reaction or reach time ERROR Interface
            * 2. Within proper reaction time && reach time, detect the touch point and end up with hit, near and miss.
            
            * Args:
            *    pos_Target_OTopLeft: the center position of the Go Target, Origin at TopLeft

            * Output:
            *   startPadHoldstate_Cue = 
            *       StartPadHoldState.HoldEnough (if startpad is touched lasting t_Cue)
            *       StartPadHoldState.HoldTooShort (if startpad is released before t_Cue) 
            */

            try
            {
                // Remove the Crossing and Show the Go Circle
                Remove_OneCrossing();
                Show_GoCircle(pos_Target_OTopLeft);

                // Increased Total Go Trial Number of currTrialTargetPosInd
                TargetExeFeedback_List[currTrialTargetPosInd][0]++;

                // go target Onset Time Point
                timePoint_Interface_TargetOnset = globalWatch.ElapsedMilliseconds;
                serialPort_IO8.WriteLine(TDTCmd_GoTargetShown);

                // Wait for Reaction within tMax_ReactionTime
                pressedStartpad = PressedStartpad.Yes;
                await Wait_Reaction();

                // Wait for Touch within tMax_ReachTime and Calcuate the gotargetTouchstate
                screenTouchstate = ScreenTouchState.Idle;
                await Wait_Reach();


                /*---- Go Target Touch States ----*/
                if (gotargetTouchstate == GoTargetTouchState.goHit)
                {/*Hit */

                    Feedback_GoCorrect_Hit();

                    // Increased Success Go Trial Number of currTrialTargetPosInd
                    TargetExeFeedback_List[currTrialTargetPosInd][1]++;

                    // trial execute result: goHit 
                    trialExeResult = TrialExeResult.goHit;
                    
                }
                else if (gotargetTouchstate == GoTargetTouchState.goMissed)
                {/* touch missed*/
                    
                    Feedback_GoERROR_Miss();

                    // trial execute result: goMiss 
                    trialExeResult = TrialExeResult.goMiss;
                }
                
                await Task.Delay(t_VisfeedbackShow);
            }
            catch(TaskCanceledException)
            {
                Interface_GoERROR_LongReactionReach();
                await Task.Delay(t_VisfeedbackShow);
                throw new TaskCanceledException("Not Reaction Within the Max Reaction Time.");
            }
            
        }


        private async Task Interface_noGo(float t_noGoShow, int[] pos_Target_OTopLeft)
        {/* task for noGo Interface: Show the noGo Interface while Listen to the state of the startpad.
            * If StartpadTouched off within t_nogoshow, go to noGo Interface; Otherwise, noGo Correct Interface
            
            * Args:
            *    t_noGoShow: noGo interface shows duration(s)
            *    pos_Target_OTopLeft: the center position of the noGo Target, Origin at TopLeft

            * Output:
            *   startPadHoldstate_Cue = 
            *       StartPadHoldState.HoldEnough (if startpad is touched lasting t_Cue)
            *       StartPadHoldState.HoldTooShort (if startpad is released before t_Cue) 
            */

            try
            {
                // Remove the Crossing and Show the noGo Rect
                Remove_OneCrossing();
                Show_noGoRect(pos_Target_OTopLeft);

                // Increased Total noGo Trial Number of currTrialTargetPosInd
                TargetExeFeedback_List[currTrialTargetPosInd][2]++;

                // noGo target Onset Time Point
                serialPort_IO8.WriteLine(TDTCmd_noGoTargetShown);
                timePoint_Interface_TargetOnset = globalWatch.ElapsedMilliseconds;


                // Wait Startpad TouchedOn  for t_noGoShow
                startpadHoldstate = StartPadHoldState.HoldTooShort;
                await Wait_EnoughTouch(t_noGoShow);
                serialPort_IO8.WriteLine(TDTCmd_noGoEnoughTCorrectFeedback);
                
                // Increased Total noGo Trial Number of currTrialTargetPosInd
                TargetExeFeedback_List[currTrialTargetPosInd][3]++;

                // noGo trial success when running here
                Feedback_noGoCorrect();

                trialExeResult = TrialExeResult.nogoSuccess;

                await Task.Delay(t_VisfeedbackShow);
            }
            catch (TaskCanceledException)
            {
                Feedback_noGoError();

                trialExeResult = TrialExeResult.nogoMoved;

                await Task.Delay(t_VisfeedbackShow);
                throw new TaskCanceledException("Startpad Touched off within t_nogoshow");
            }

        }


        private void Feedback_GoERROR()
        {
            // Visual Feedback
            //myGridBorder.BorderBrush = brush_ErrorFill;
            circleGo.Fill = brush_ErrorFill;
            circleGo.Stroke = brush_ErrorOutline;
            myGrid.UpdateLayout();


            //Juicer Feedback
            giveJuicerState = GiveJuicerState.No;

            // Audio Feedback
            player_Error.Play()
;            
        }
        private void Interface_GoERROR_LongReactionReach()
        {
            Feedback_GoERROR();
        }

        private void Feedback_GoERROR_Miss()
        {
            Feedback_GoERROR();
        }

        private void Feedback_GoCorrect_Hit()
        {
            // Visual Feedback
            circleGo.Fill = brush_CorrectFill;
            myGrid.UpdateLayout();


            //Juicer Feedback
            giveJuicerState = GiveJuicerState.CorrectGiven;

            // Audio Feedback
            player_Correct.Play();
        }


        private void Feedback_noGoError()
        {
            // Visual Feedback
            //myGridBorder.BorderBrush = brush_ErrorFill;
            rectNogo.Fill = brush_ErrorFill;
            rectNogo.Stroke = brush_ErrorOutline;
            myGrid.UpdateLayout();


            //Juicer Feedback
            giveJuicerState = GiveJuicerState.No;

            // Audio Feedback
            player_Error.Play();
        }

        private void Feedback_noGoCorrect()
        {
            // Visual Feedback
            //myGridBorder.BorderBrush = brush_CorrectFill;
            rectNogo.Fill = brush_CorrectFill;
            myGrid.UpdateLayout();


            //Juicer Feedback
            giveJuicerState = GiveJuicerState.CorrectGiven;

            // Audio Feedback
            player_Correct.Play();
        }


        public void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Present_Stop();
            parent.btn_start.IsEnabled = true;
            parent.btn_stop.IsEnabled = false;
        }


        void Touch_FrameReported(object sender, TouchFrameEventArgs e)
        {/* Add the Id of New Touch Points into Hashset touchPoints_Id 
            and the Corresponding Touch Down Positions into List downPoints_Pos (no replicates)*/
            screenTouchstate = ScreenTouchState.Touched;
            TouchPointCollection touchPoints = e.GetTouchPoints(myGrid);
            bool addedNew;
            long time = tpoints1TouchWatch.ElapsedMilliseconds;
            long timestamp_now = (DateTime.Now.Ticks - timestamp_0) / TimeSpan.TicksPerMillisecond;
            for (int i = 0; i < touchPoints.Count; i++)
            {
                TouchPoint _touchPoint = touchPoints[i];
                if (_touchPoint.Action == TouchAction.Down)
                { /* TouchAction.Down */

                    if (touchPoints_Id.Count == 0)
                    {// the first touch point for one touch
                        tpoints1TouchWatch.Restart();
                        serialPort_IO8.WriteLine(TDTCmd_GoTouched);
                    }
                    lock (touchPoints_Id)
                    {
                        // Add the touchPoint to the Hashset touchPoints_Id, Return true if added, otherwise false.
                        addedNew = touchPoints_Id.Add(_touchPoint.TouchDevice.Id);
                    }
                    if (addedNew)
                    {/* deal with the New Added TouchPoint*/

                        // store the pos of the point with down action
                        lock (downPoints_Pos)
                        {
                            downPoints_Pos.Add(new double[2] { _touchPoint.Position.X, _touchPoint.Position.Y });
                        }

                        // store the pos and time of the point with down action, used for file writing
                        lock (touchPoints_PosTimeDis)
                        {
                            touchPoints_PosTimeDis.Add(new double[8] { _touchPoint.TouchDevice.Id, timestamp_now, _touchPoint.Position.X, _touchPoint.Position.Y, 0, 0, 0, 0});
                        }
                    }
                }
                else if (_touchPoint.Action == TouchAction.Up)
                {
                    // remove the id of the point with up action
                    lock (touchPoints_Id)
                    {
                        touchPoints_Id.Remove(_touchPoint.TouchDevice.Id);
                    }

                    // add the left points timepoint, and x,y positions of the current _touchPoint.TouchDevice.Id
                    lock (touchPoints_PosTimeDis)
                    {
                        for (int pointi = 0; pointi < touchPoints_PosTimeDis.Count; pointi++)
                        {
                            if (touchPoints_PosTimeDis[pointi][0] == _touchPoint.TouchDevice.Id)
                            {
                                touchPoints_PosTimeDis[pointi][4] = timestamp_now;
                                touchPoints_PosTimeDis[pointi][5] = _touchPoint.Position.X;
                                touchPoints_PosTimeDis[pointi][6] = _touchPoint.Position.Y;
                            }
                        }
                    }
                }
            }

        }
    }
}
