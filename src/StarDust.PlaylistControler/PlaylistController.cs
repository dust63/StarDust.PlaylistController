using StarDust.PlaylistControler.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace StarDust.PlaylistControler
{

    public class PlaylistController<T> : IScheduleNotifications where T : class, IPlaylistItem
    {
        private readonly PlaylistCollection<T> _playlistToManage;
        private bool _initialized = false;
        private readonly TimeSpan _prerollValue = TimeSpan.FromSeconds(1);

        public T CurrentPlayingItem { get; protected set; }
        public T PreparedItem { get; protected set; }

        public event EventHandler EndTimeNear;
        public event EventHandler EndTimeReached;
        public event EventHandler StartTimeNear;
        public event EventHandler StartTimeReached;


        public PlaylistController()
        {

        }

        /// <summary>
        /// Initialize playlit controler with preroll value
        /// </summary>
        /// <param name="prerollValue">Time before start and end of schedule element</param>
        public PlaylistController(TimeSpan prerollValue)
        {
            _prerollValue = prerollValue;
        }

        public TimeSpan PrerollStart => _prerollValue;
        public TimeSpan PrerollEnd => _prerollValue;

        public event EventHandler<ElementsSkippedEventArgs<T>> ElementsSkipped;
        public event EventHandler PlaylistStarted;
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
            _playlistToManage.ElementsRemoved += OnElementsRemoved;

            Parallel.ForEach(_playlistToManage,
                (item) =>
                {
                    AddEvent(item);
                    SetPreroll(item);
                });

            _initialized = true;


            var scheduleElements =
                _playlistToManage.Where(x => x.StartMode == StartMode.Schedule && x.StartTime >= DateTime.Now);
            Parallel.ForEach(scheduleElements, (element) => element.StartScheduling());

        }

        /// <summary>
        /// Define preroll for an element
        /// </summary>
        /// <param name="item"></param>
        private void SetPreroll(T item)
        {
            item.PrerollStart = PrerollStart;
            item.PrerollEnd = PrerollEnd;
        }


        private void OnElementsRemoved(object sender, CollectionChangeEventArgs e)
        {
            Parallel.ForEach((IEnumerable<T>)e.Element, (element) =>
             {

                 element.CancelScheduling();
                 RemoveEvent(element);
             });
        }


        private void OnPlaylistCleared(object sender, EventArgs e)
        {
            CurrentPlayingItem?.CancelScheduling();
            PreparedItem?.CancelScheduling();

            Parallel.ForEach(_playlistToManage.ToArray(), (x) =>
             {
                 RemoveEvent(x);
                 x.CancelScheduling();
             });
        }

        private void OnElementRemoved(object sender, CollectionChangeEventArgs e)
        {
            var element = (T)e.Element;
            Task.Run(() => element.CancelScheduling())
                .ContinueWith((t) => RemoveEvent((T)e.Element));
        }

        private void OnElementAdded(object sender, CollectionChangeEventArgs e)
        {
            var element = (T)e.Element;
            Task.Run(() => StartScheduleElement(element));
            AddEvent(element);
            SetPreroll(element);
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
                element.CancelScheduling();
                return;
            }


            if (e.CurrentStartMode == StartMode.Schedule)
                StartScheduleElement(element);
        }
        protected void OnStartTimeNear(object sender, EventArgs e)
        {
            var element = (T)sender;

            //Launch action
            Task.Factory.StartNew(() => StartTimeNear?.Invoke(element, e));

            //Unsubscribe event
            element.StartTimeNear -= OnStartTimeNear;
        }
        protected void OnStartTimeReached(object sender, EventArgs e)
        {

            var playedItem = CurrentPlayingItem;
            var element = (T)sender;
            CurrentPlayingItem = element;


            //Launch action
            Task.Run(() => StartTimeReached?.Invoke(sender, e));
            Task.Run(() => CheckingForSkip(playedItem, CurrentPlayingItem));

            CheckForPlaylistStart();

            //Unsubscribe event
            element.StartTimeReached -= OnStartTimeReached;
        }
        protected void OnEndTimeNear(object sender, EventArgs e)
        {
            var element = (T)sender;

            var indexCurrentElement = this.IndexOf(element);

            PrepareNextElement(element);

            //Launch action
            Task.Run(() => EndTimeNear?.Invoke(sender, e));

            //Unsubscribe event
            element.EndTimeNear -= OnEndTimeNear;
        }



        protected void OnEndTimeReached(object sender, EventArgs e)
        {
            var element = (T)sender;

            Task.Factory.StartNew(() => EndTimeReached?.Invoke(sender, e))
                .ContinueWith((t) => CheckForPlaylistStopped());

            //Unsubscribe event
            element.EndTimeReached -= OnEndTimeReached;
        }

        protected void CheckForPlaylistStopped()
        {
            if (PreparedItem == null)
                PlaylistStopped?.Invoke(this, EventArgs.Empty);
        }

        protected void CheckForPlaylistStart()
        {
            if (PreparedItem == null)
                PlaylistStarted?.Invoke(this, EventArgs.Empty);
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
            PreparedItem?.StartScheduling();
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
                element.StartScheduling();
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
            if (previousElement == null || previousElement.Status == Status.Ended)
                return;

            var skippedList = new List<T>();
            for (var i = indexCurrentElement - 1; i > 0; i--)
            {
                var element = _playlistToManage.ElementAt(i);
                if (element.Status == Status.Ended)
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

            playedItem?.CancelScheduling();
            PreparedItem?.CancelScheduling();


            Task.Factory.StartNew(() => VerifySkippedElement(IndexOf(newPlayingItem)));
        }

        private int IndexOf(T element)
        {
            return _playlistToManage.ToList().IndexOf(element);
        }

    }
}