using System;
using System.ComponentModel;


public interface ISchedule : INotifyPropertyChanged
{



    event EventHandler EndTimeNear;
    event EventHandler EndTimeReached;
    event EventHandler StartTimeNear;
    event EventHandler StartTimeReached;

    Action<object> ActionOnEndTimeNear { get; set; }
    Action<object> ActionOnEndTimeReached { get; set; }
    Action<object> ActionOnStartTimeNear { get; set; }
    Action<object> ActionOnStartTimeReached { get; set; }



    TimeSpan Preroll { get; }
    DateTime? StartTime { get; set; }
    TimeSpan Duration { get; set; }
    StartMode StartMode { get; set; }


    ISchedule ParentElement { get; set; }
    ISchedule ChildElement { get; set; }

    void SetCurrentStartTime();
}


