using StarDust.PlaylistControler.Models;
using System;
using System.ComponentModel;


public interface ISchedule : INotifyPropertyChanged
{



    event EventHandler EndTimeNear;
    event EventHandler EndTimeReached;
    event EventHandler StartTimeNear;
    event EventHandler StartTimeReached;
    event EventHandler<StartModeChangedEventArgs> StartModeChanged;





    Status Status { get; set; }
    TimeSpan PrerollStart { get; }
    TimeSpan PrerollEnd { get; }
    DateTime? StartTime { get; set; }
    TimeSpan Duration { get; set; }
    DateTime? EndTime { get; }
    StartMode StartMode { get; set; }

    void StartCheckingTask();
    void CancelCheckingTask();

}


