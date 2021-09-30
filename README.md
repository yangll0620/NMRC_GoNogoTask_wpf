# NMRC_GoNogoTask
Presentation in the touch screen of Go noGO task in NMRC
This is a C# Wpf version of the presentation program in the touch screen of the GonoGO task of NMRC. Only work in Windows as used C# WPF.

1. Multiple touch
2. Touch to start a new trial



Issues:

1. Stop Bug.
2. Load and Save config. Functions
3. Load function from file
4. Change event code according to COT task


ToDo List
1. Change trial end defination into late(visual feedback ends, all touch point left)
2. Run in session, stop until the experimenter click stop button
	trialNumPerSession = totalTrialNum/pos/Session * totalPosNum, 
	totalTrialNum = goTrialNum/pos/Session + noGoTrialNum/pos/Session
3. Change event code according to COT task
4. Add 0000 after each trial
5. Implement the new main interface as Guo drawed
	(1) Each Target Realtime Info Feedback
	(2) Session Realtime Info Feedback
6. Move savefolder/audiosetup into menu

Advanced Functions:
1. Dynamic draw the target feedback grid accroding to the target num.
2. Automatically get the property name
3. Use table for each target realtime info feedback