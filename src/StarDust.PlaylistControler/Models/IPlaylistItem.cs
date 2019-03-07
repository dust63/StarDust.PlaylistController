using StarDust.PlaylistControler.Models;
using System;
using System.ComponentModel;


public interface IPlaylistItem : IScheduleNotifications, INotifyPropertyChanged
{


    /// <summary>
    /// Time before the to trigger event before the start
    /// </summary>
    TimeSpan PrerollStart { get; set; }


    /// <summary>
    /// Preroll to trigger event before the end of the element
    /// </summary>
    TimeSpan PrerollEnd { get; set; }


    event EventHandler ScheduleInfoChanged;

    event EventHandler<StartModeChangedEventArgs> StartModeChanged;


    Status Status { get; set; }

    /// <summary>
    /// At wich start time the element should start
    /// </summary>
    DateTime? StartTime { get; set; }

    /// <summary>
    /// Duration of the element
    /// </summary>
    TimeSpan Duration { get; set; }

    /// <summary>
    /// Start time + duration
    /// </summary>
    DateTime? EndTime { get; }


    /// <summary>
    /// Define how the element will start
    /// </summary>
    StartMode StartMode { get; set; }

    /// <summary>
    /// Launch a routine that will check if start time or end time is reached
    /// </summary>
    void StartScheduling();

    /// <summary>
    /// Stop the routine that will check if start time or end time is reached
    /// </summary>
    void CancelScheduling();


}