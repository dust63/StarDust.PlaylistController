using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;


public abstract class BasePlaylistItem : ISchedule
{
    private ISchedule _childElement;
    private ISchedule _parentElement;
    private DateTime? _startTime;
    private TimeSpan _duration;
    private StartMode _startMode;
    public event PropertyChangedEventHandler PropertyChanged;


    public DateTime? StartTime
    {
        get => _startTime;
        set => SetProperty(ref _startTime, value, (v) => SetChildStartTime());
    }



    public TimeSpan Duration
    {
        get => _duration;
        set => SetProperty(ref _duration, value, (v) => SetChildStartTime());
    }

    public StartMode StartMode
    {
        get => _startMode;
        set => SetProperty(ref _startMode, value, (v) => SetCurrentStartTime());
    }


    public ISchedule ParentElement
    {
        get => _parentElement;
        set
        {
            SetProperty(ref _parentElement, value, (oldValue) =>
            {
                if (_parentElement != null)
                {
                    _parentElement.ChildElement = this;
                }

                if (oldValue != null && _parentElement == null)
                    oldValue.ChildElement = null;
            });

        }
    }


    public ISchedule ChildElement
    {
        get => _childElement;
        set
        {
            SetProperty(ref _childElement, value, (oldValue) =>
            {
                if (_childElement != null)
                {
                    _childElement.ParentElement = this;
                    SetChildStartTime();
                }

                if (oldValue != null && _childElement == null)
                    oldValue.ParentElement = null;


            });
        }
    }


    protected virtual void SetChildStartTime()
    {
        _childElement?.SetCurrentStartTime();
    }

    public void SetCurrentStartTime()
    {
        switch (StartMode)
        {
            case StartMode.AutoFollow:
                StartTime = ParentElement?.StartTime?.Add(ParentElement.Duration);
                break;
            case StartMode.None:
                StartTime = null;
                break;
            case StartMode.Schedule:
                break;
            default:
                throw new NotImplementedException();
        }
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
    /// <param name="onChanged">Action that is called after the property value has been changed.</param>
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




}
