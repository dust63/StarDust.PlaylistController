using System;

public interface IScheduleNotifications
{


    event EventHandler EndTimeNear;
    event EventHandler EndTimeReached;
    event EventHandler StartTimeNear;
    event EventHandler StartTimeReached;

}