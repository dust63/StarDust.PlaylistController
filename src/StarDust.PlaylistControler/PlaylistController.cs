using StarDust.PlaylistControler.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace StarDust.PlaylistControler
{
    public class PlaylistController<T> where T : class, ISchedule
    {
        private readonly PlaylistCollection<T> _playlistToManage;
        private bool _initialized = false;

        public T CurrentPlayingItem { get; protected set; }
        public T PreparedItem { get; protected set; }
        public Action<object> ActionOnEndTimeNear { get; set; }
        public Action<object> ActionOnEndTimeReached { get; set; }
        public Action<object> ActionOnStartTimeNear { get; set; }
        public Action<object> ActionOnStartTimeReached { get; set; }
        public event EventHandler<ElementsSkippedEventArgs<T>> ElementsSkipped;
        public event EventHandler PlaylistStopped;

        public PlaylistController(PlaylistCollection<T> playlistToManage)
        {
            _playlistToManage = playlistToManage;
        }

        public void Initialize()
        {
            if (_initialized)
                return;

            _playlistToManage.ElementAdded += OnElementAdded;
            _playlistToManage.ElementRemoved += OnElementRemoved;
            _playlistToManage.PlaylistCleared += OnPlaylistCleared;


            Parallel.ForEach(_playlistToManage, AddEvent);

            _initialized = true;


            var scheduleElements =
                _playlistToManage.Where(x => x.StartMode == StartMode.Schedule && x.StartTime >= DateTime.Now);
            Parallel.ForEach(scheduleElements, (element) => element.StartCheckingTask());

        }



        private void OnPlaylistCleared(object sender, EventArgs e)
        {
            CurrentPlayingItem?.CancelCheckingTask();
            PreparedItem?.CancelCheckingTask();

            Parallel.ForEach(_playlistToManage, RemoveEvent);
            foreach (var element in _playlistToManage.Where(x => x.StartMode == StartMode.Schedule && x.StartTime >= DateTime.Now))
            {
                element.CancelCheckingTask();
            }

        }

        private void OnElementRemoved(object sender, CollectionChangeEventArgs e)
        {
            var element = (T)e.Element;
            Task.Run(() => element.CancelCheckingTask())
                .ContinueWith((t) => RemoveEvent((T)e.Element));
        }

        private void OnElementAdded(object sender, CollectionChangeEventArgs e)
        {
            var element = (T)e.Element;

            Task.Run(() => AddEvent(element))
                .ContinueWith((t) => StartScheduleElement(element));
        }



        protected void AddEvent(T element)
        {
            element.StartTimeReached += OnStartTimeReached;
            element.StartTimeNear += OnStartTimeNear;
            element.EndTimeNear += OnEndTimeNear;
            element.EndTimeReached += OnEndTimeReached;
            element.StartModeChanged += OnStartModeChanged;
        }

        protected void RemoveEvent(T element)
        {
            element.StartTimeReached -= OnStartTimeReached;
            element.StartTimeNear -= OnStartTimeNear;
            element.EndTimeNear -= OnEndTimeNear;
            element.EndTimeReached -= OnEndTimeReached;
            element.StartModeChanged -= OnStartModeChanged;
        }

        protected void OnStartModeChanged(object sender, StartModeChangedEventArgs e)
        {
            var element = (T)sender;


            if (e.OldStartMode == StartMode.Schedule && element.Status == Status.None)
            {
                element.CancelCheckingTask();
                return;
            }


            if (e.CurrentStartMode == StartMode.Schedule)
                StartScheduleElement(element);
        }
        protected void OnStartTimeNear(object sender, EventArgs e)
        {
            var element = (T)sender;

            //Launch action
            Task.Factory.StartNew(() => ActionOnStartTimeNear(element));

            //Unsubscribe event
            element.StartTimeNear -= OnStartTimeNear;
        }
        protected void OnStartTimeReached(object sender, EventArgs e)
        {

            var playedItem = CurrentPlayingItem;
            var element = (T)sender;
            CurrentPlayingItem = element;


            CheckingForSkip(playedItem, CurrentPlayingItem);

            //Launch action
            Task.Run(() => ActionOnStartTimeReached(element));

            //Unsubscribe event
            element.StartTimeReached -= OnStartTimeReached;
        }
        protected void OnEndTimeNear(object sender, EventArgs e)
        {
            var element = (T)sender;

            var indexCurrentElement = this.IndexOf(element);

            PrepareNextElement(element);

            //Launch action
            Task.Run(() => ActionOnEndTimeNear(CurrentPlayingItem));

            //Unsubscribe event
            element.EndTimeNear -= OnEndTimeNear;
        }



        protected void OnEndTimeReached(object sender, EventArgs e)
        {
            var element = (T)sender;

            Task.Factory.StartNew(() => ActionOnEndTimeReached(element))
                .ContinueWith((t) => CheckForPlaylistStopped());

            //Unsubscribe event
            element.EndTimeReached -= OnEndTimeReached;
        }

        protected void CheckForPlaylistStopped()
        {
            if (PreparedItem == null)
                PlaylistStopped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Prepare the next element if was set in AutoFollow
        /// </summary>
        /// <param name="playingElement"></param>
        private void PrepareNextElement(T playingElement)
        {
            var nextElement = _playlistToManage.ElementAtOrDefault(this.IndexOf(playingElement) + 1);
            if (nextElement == null || nextElement.StartMode != StartMode.AutoFollow)
            {
                PreparedItem = null;
                return;
            }

            PreparedItem = nextElement;
            PreparedItem.StartTime = playingElement.EndTime;
            PreparedItem?.StartCheckingTask();
        }

        /// <summary>
        /// Start a element if it schedule and Start time was > now
        /// </summary>
        /// <param name="element"></param>
        private void StartScheduleElement(T element)
        {
            if (!_initialized)
                return;

            if (element.StartMode == StartMode.Schedule && element.StartTime > DateTime.Now)
            {
                element.StartCheckingTask();
            }
        }

        /// <summary>
        /// Set element to status skipped and notify
        /// </summary>
        /// <param name="indexCurrentElement"></param>
        private void VerifySkippedElement(int indexCurrentElement)
        {
            if (ElementsSkipped == null)
                return;

            var previousElement = _playlistToManage.ElementAtOrDefault(indexCurrentElement - 1);
            if (previousElement == null || previousElement.Status == Status.Played)
                return;

            var skippedList = new List<T>();
            for (var i = indexCurrentElement - 1; i > 0; i--)
            {
                var element = _playlistToManage.ElementAt(i);
                if (element.Status == Status.Played)
                    break;
                element.Status = Status.Skipped;
                skippedList.Add(element);
            }

            //Notify for skipped elements
            if (skippedList.Any())
                ElementsSkipped?.Invoke(this, new ElementsSkippedEventArgs<T>(skippedList));
        }

        private void CheckingForSkip(T playedItem, T newPlayingItem)
        {
            if (CurrentPlayingItem.Equals(PreparedItem))
                return;

            playedItem?.CancelCheckingTask();
            PreparedItem?.CancelCheckingTask();

            if (playedItem == null)
                return;

            Task.Factory.StartNew(() => VerifySkippedElement(IndexOf(newPlayingItem)));
        }

        private int IndexOf(T element)
        {
            return _playlistToManage.ToList().IndexOf(element);
        }

    }
}