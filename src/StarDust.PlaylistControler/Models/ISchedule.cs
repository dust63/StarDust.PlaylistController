using System;
using System.ComponentModel;


public interface ISchedule : INotifyPropertyChanged
{

    DateTime? StartTime { get; set; }
    TimeSpan Duration { get; set; }
    StartMode StartMode { get; set; }


    ISchedule ParentElement { get; set; }
    ISchedule ChildElement { get; set; }

    void SetCurrentStartTime();
}


