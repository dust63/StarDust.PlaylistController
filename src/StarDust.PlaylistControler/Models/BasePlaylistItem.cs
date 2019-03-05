using StarDust.PlaylistControler.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;


public abstract class BasePlaylistItem : ISchedule
{

    private DateTime? _startTime;
    private TimeSpan _duration = TimeSpan.Zero;
    private StartMode _startMode = StartMode.AutoFollow;

    private CancellationTokenSource tokenSource;


    public event PropertyChangedEventHandler PropertyChanged;
    public event EventHandler EndTimeNear;
    public event EventHandler EndTimeReached;
    public event EventHandler StartTimeNear;
    public event EventHandler StartTimeReached;
    public event EventHandler<StartModeChangedEventArgs> StartModeChanged;


    public Status Status { get; set; }


    public TimeSpan PrerollStart => TimeSpan.FromSeconds(2);
    public TimeSpan PrerollEnd => PrerollStart.Add(TimeSpan.FromSeconds(0.2));


    public DateTime? StartTime
    {
        get => _startTime;
        set => SetProperty(ref _startTime, value);
    }


    /// <summary>
    /// Duration of element
    /// </summary>
    public TimeSpan Duration
    {
        get => _duration;
        set => SetProperty(ref _duration, value);
    }


    public DateTime? EndTime => StartTime?.Add(Duration);


    /// <summary>
    /// In wich mode we want to start this item
    /// </summary>
    public StartMode StartMode
    {
        get => _startMode;
        set => SetProperty(ref _startMode, value,
            (oldValue) =>
            {
                StartModeChanged?.Invoke(this, new StartModeChangedEventArgs(oldValue, _startMode));
            });
    }








    #region PropertyNotification

    /// <summary>
    /// Checks if a property already matches a desired value. Sets the property and
    /// notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="T">Type of the property.</typeparam>
    /// <param name="storage">Reference to a property with both getter and setter.</param>
    /// <param name="value">Desired value for the property.</param>
    /// <param name="propertyName">Name of the property used to notify listeners. This
    /// value is optional and can be provided automatically when invoked from compilers that
    /// support CallerMemberName.</param>
    /// <returns>True if the value was changed, false if the existing value matched the
    /// desired value.</returns>
    protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

        storage = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        return true;
    }

    /// <summary>
    /// Checks if a property already matches a desired value. Sets the property and
    /// notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="T">Type of the property.</typeparam>
    /// <param name="storage">Reference to a property with both getter and setter.</param>
    /// <param name="value">Desired value for the property.</param>
    /// <param name="propertyName">Name of the property used to notify listeners. This
    /// value is optional and can be provided automatically when invoked from compilers that
    /// support CallerMemberName.</param>
    /// <param name="actionOnChanged">Action that is called after the property value has been changed. Parameter are old value</param>
    /// <returns>True if the value was changed, false if the existing value matched the
    /// desired value.</returns>
    protected virtual bool SetProperty<T>(ref T storage, T value, Action<T> actionOnChanged, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

        var oldValue = storage;
        storage = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        actionOnChanged?.Invoke(oldValue);

        return true;
    }

    #endregion

    public void CancelCheckingTask()
    {
        if (tokenSource == null)
            return;

        tokenSource.Cancel();
        Status = Status.Aborted;
    }

    public void StartCheckingTask()
    {
        if (Status == Status.Skipped)
            return;

        tokenSource?.Cancel();
        tokenSource = new CancellationTokenSource();

        if (!StartTime.HasValue ||
            Duration <= TimeSpan.Zero ||
            StartMode == StartMode.None)
            return;


        Task.Factory.StartNew(async () =>
      {

          await CheckNearFromStartTime(tokenSource.Token);
          await CheckStartTimeReached(tokenSource.Token);
          await CheckEndTimeNear(tokenSource.Token);
          await CheckEndTimeReached(tokenSource.Token);
      }, tokenSource.Token);


    }

    protected async Task CheckNearFromStartTime(CancellationToken cancellationToken)
    {
        await Task.Factory.StartNew(() =>
        {

            while (StartTime?.Subtract(PrerollStart) >= DateTime.Now)
            {
                Task.Delay(10, cancellationToken).Wait(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }
            Status = Status.Prepared;
            StartTimeNear?.Invoke(this, EventArgs.Empty);
        }, cancellationToken);

    }

    protected async Task CheckStartTimeReached(CancellationToken cancellationToken)
    {
        await Task.Factory.StartNew(() =>
        {

            while (StartTime >= DateTime.Now)
            {
                Task.Delay(10, cancellationToken).Wait(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }
            Status = Status.Playing;
            StartTimeReached?.Invoke(this, EventArgs.Empty);
        }, cancellationToken);
    }

    protected async Task CheckEndTimeReached(CancellationToken cancellationToken)
    {

        await Task.Factory.StartNew(() =>
        {

            while (EndTime >= DateTime.Now)
            {
                Task.Delay(10, cancellationToken).Wait(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }

            Status = Status.Played;
            EndTimeReached?.Invoke(this, EventArgs.Empty);
        }, cancellationToken);
    }

    protected async Task CheckEndTimeNear(CancellationToken cancellationToken)
    {


        await Task.Factory.StartNew(() =>
        {

            while (EndTime?.Subtract(PrerollEnd) >= DateTime.Now)
            {
                Task.Delay(10, cancellationToken).Wait(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }

            EndTimeNear?.Invoke(this, EventArgs.Empty);
        }, cancellationToken);
    }







}

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
