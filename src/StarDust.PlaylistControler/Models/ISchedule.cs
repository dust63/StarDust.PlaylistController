using StarDust.PlaylistControler.Models;
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




    Status Status { get; set; }
    TimeSpan PrerollStart { get; }
    TimeSpan PrerollEnd { get; }
    DateTime? StartTime { get; set; }
    TimeSpan Duration { get; set; }
    StartMode StartMode { get; set; }

}


