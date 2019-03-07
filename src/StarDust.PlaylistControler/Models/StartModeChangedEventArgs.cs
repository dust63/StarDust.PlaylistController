using System;

public class StartModeChangedEventArgs : EventArgs
{

    public StartMode OldStartMode { get; }
    public StartMode CurrentStartMode { get; }

    public StartModeChangedEventArgs(StartMode oldStartMode, StartMode currentStartMode)
    {
        OldStartMode = oldStartMode;
        CurrentStartMode = currentStartMode;
    }
}