using Newtonsoft.Json;
using System.Collections.Generic;

namespace GonoGoTask_wpfVer
{
    class Config_GoNogoTask
    {
        [JsonProperty(PropertyName = "NHP Name")]
        public string NHPName;

        [JsonProperty(PropertyName = "Go Trials Num")]
        public int GoTrialNum;

        [JsonProperty(PropertyName = "noGo Trials Num")]
        public int NogoTrialNum;

        [JsonProperty(PropertyName = "saved folder")]
        public string saved_folder;

        public string audioFile_Correct, audioFile_Error;

        [JsonProperty(PropertyName = "Times")]
        public ConfigTimes configTimes;


        [JsonProperty(PropertyName = "Target")]
        public ConfigTarget configTarget;

        [JsonProperty(PropertyName = "Colors")]
        public ConfigColors configColors;
 
    }


    class ConfigTimes
    {
        [JsonProperty(PropertyName = "Ready Show Time Range")]
        public float[] tRange_ReadyTime;

        [JsonProperty(PropertyName = "Cue Show Time Range")]
        public float[] tRange_CueTime;

        [JsonProperty(PropertyName = "Nogo Show Time Range")]
        public float[] tRange_NogoShowTime;

        [JsonProperty(PropertyName = "Max Reaction Time")]
        public float tMax_ReactionTime;

        [JsonProperty(PropertyName = "Max Reach Time")]
        public float tMax_ReachTime;

        [JsonProperty(PropertyName = "Visual Feedback Show Time")]
        public float t_VisfeedbackShow;

        [JsonProperty(PropertyName = "Inter Trials Time")]
        public float t_InterTrial;

        [JsonProperty(PropertyName = "Juice Correct Given Time")]
        public float t_JuicerCorrectGiven;

        [JsonProperty(PropertyName = "Juice Close Given Time")]
        public float t_JuicerCloseGiven;
    }


    class ConfigTarget
    {
        [JsonProperty(PropertyName = "Target Diameter (inch)")]
        public float targetDiaInch;

        [JsonProperty(PropertyName = "Target Distance from Center")]
        public float targetDisFromCenter;

        [JsonProperty(PropertyName = "Close Margin Percentage")]
        public int closeMarginPercentage;

        public List<int[]> optPostions_OCenter_List;
    }


    class ConfigColors
    {
        [JsonProperty(PropertyName = "Go Fill Color")]
        public string goFillColorStr;

        [JsonProperty(PropertyName = "noGo Fill Color")]
        public string nogoFillColorStr;

        [JsonProperty(PropertyName = "Cue Crossing Color")]
        public string cueCrossingColorStr;

        [JsonProperty(PropertyName = "Wait Start Background")]
        public string BKWaitTrialColorStr;


        [JsonProperty(PropertyName = "Trial Background")]
        public string BKTrialColorStr;


        [JsonProperty(PropertyName = "Correct Fill")]
        public string CorrFillColorStr;


        [JsonProperty(PropertyName = "Correct Outline")]
        public string CorrOutlineColorStr;

        [JsonProperty(PropertyName = "Error Fill")]
        public string ErrorFillColorStr;

        [JsonProperty(PropertyName = "Error Outline")]
        public string ErrorOutlineColorStr;

    }
}
