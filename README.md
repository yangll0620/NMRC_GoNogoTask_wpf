# NMRC_GoNogoTask
Presentation in the touch screen of Go noGO task in NMRC
This is a C# Wpf version of the presentation program in the touch screen of the GonoGO task of NMRC. Only work in Windows as used C# WPF.

1. Multiple touch
2. Touch to start a new trial
3. Show go and nogo trials based on the preset ratio. 
	Total Trial Num/Position/Session: only the showed go or nogo trials counted 
		(i.e. if abandon during ready or cue phase, trialnum stays the same)



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
3. Change event code according to COT task (Done)
	3-1 Write to the save file of the event codes
4. Add 0000 after each trial (Done)
5. Implement the new main interface as Guo drawed
	(1) Each Target Realtime Info Feedback
	(2) Session Realtime Info Feedback
6. Move savefolder/audiosetup into menu
7. Add Block Inf in saved file name
8. Generate, modify and save optPostions_OCenter_List function
9. Show circleGo and rectNogo the same as those in COT task
10. Resources debug/release used Problem.
11. Cue position Incorrect
12. Test Show Target and Actual Target Position during Task


Advanced Functions:
1. Dynamic draw the target feedback grid accroding to the target num.
2. Automatically get the property name
3. Use table for each target realtime info feedback

