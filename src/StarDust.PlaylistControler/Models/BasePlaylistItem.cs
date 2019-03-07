using StarDust.PlaylistControler.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;



public abstract class BasePlaylistItem : IPlaylistItem
{





    private DateTime? _startTime;
    private TimeSpan _duration = TimeSpan.Zero;
    private StartMode _startMode = StartMode.AutoFollow;

    private CancellationTokenSource _tokenSource;


    public event PropertyChangedEventHandler PropertyChanged;

    public event EventHandler ScheduleInfoChanged;
    public event EventHandler EndTimeNear;
    public event EventHandler EndTimeReached;
    public event EventHandler StartTimeNear;
    public event EventHandler StartTimeReached;
    public event EventHandler<StartModeChangedEventArgs> StartModeChanged;

    [DisplayName("Status")]
    public Status Status { get; set; }

    [TimespanRequired]
    [TimespanGreaterThan(ValueInMilliseconds = 500)]
    [DisplayName("Preroll from start")]
    public TimeSpan PrerollStart { get; set; } = TimeSpan.FromSeconds(2);


    [TimespanRequired]
    [TimespanGreaterThan(ValueInMilliseconds = 500)]
    [DisplayName("Preroll from end")]
    public TimeSpan PrerollEnd { get; set; } = TimeSpan.FromSeconds(2);


    public DateTime? StartTime
    {
        get => _startTime;
        set => SetProperty(ref _startTime, value, (v) => { RaisedTimingInfoChanged(); });
    }

    private void RaisedTimingInfoChanged()
    {
        ScheduleInfoChanged?.Invoke(this, EventArgs.Empty);
    }


    /// <summary>
    /// Duration of element
    /// </summary>
    public TimeSpan Duration
    {
        get => _duration;
        set => SetProperty(ref _duration, value, (v) => { RaisedTimingInfoChanged(); });
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
                RaisedTimingInfoChanged();
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

    public void CancelScheduling()
    {
        if (_tokenSource == null)
            return;

        _tokenSource.Cancel();
        Status = Status.Aborted;
    }

    public void StartScheduling()
    {
        if (Status == Status.Skipped)
            return;

        _tokenSource?.Cancel();
        _tokenSource = new CancellationTokenSource();

        if (!StartTime.HasValue ||
            Duration <= TimeSpan.Zero ||
            StartMode == StartMode.None)
            return;


        Task.Factory.StartNew(async () =>
      {

          await CheckNearFromStartTime(_tokenSource.Token);
          await CheckStartTimeReached(_tokenSource.Token);
          await CheckEndTimeNear(_tokenSource.Token);
          await CheckEndTimeReached(_tokenSource.Token);
          _tokenSource = null;
      }, _tokenSource.Token);


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
            Status = Status.WaitForStart;
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
            Status = Status.Started;
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

            Status = Status.Ended;
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