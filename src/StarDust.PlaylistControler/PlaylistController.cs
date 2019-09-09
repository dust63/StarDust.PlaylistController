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
        private bool _initialized = false;

        public T CurrentPlayingItem { get; protected set; }
        public T PreparedItem { get; protected set; }

        public event EventHandler EndTimeNear;
        public event EventHandler EndTimeReached;
        public event EventHandler StartTimeNear;
        public event EventHandler StartTimeReached;




        public TimeSpan PrerollStart { get; } = TimeSpan.FromSeconds(2);
        public TimeSpan PrerollEnd { get; } = TimeSpan.FromSeconds(2);

        public event EventHandler<ElementsSkippedEventArgs<T>> ElementsSkipped;
        public event EventHandler PlaylistStarted;
        public event EventHandler PlaylistStopped;

        public PlaylistCollection<T> PlaylistManaged { get; }

        public PlaylistController(PlaylistCollection<T> playlistToManage)
        {
            PlaylistManaged = playlistToManage;
        }

        public PlaylistController(PlaylistCollection<T> playlistToManage, TimeSpan prerollValue) : this(playlistToManage)
        {
            if (prerollValue <= TimeSpan.Zero)
                throw new ArgumentException("Preroll must be positive and not zero", nameof(prerollValue));
            PrerollStart = prerollValue;
            PrerollEnd = prerollValue;
        }

        /// <summary>
        /// Prepare the playlist to be run
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
                return;

            PlaylistManaged.ElementAdded += OnElementAdded;
            PlaylistManaged.ElementRemoved += OnElementRemoved;
            PlaylistManaged.PlaylistCleared += OnPlaylistCleared;
            PlaylistManaged.ElementsRemoved += OnElementsRemoved;

            Parallel.ForEach(PlaylistManaged,
                (item) =>
                {
                    AddEvent(item);
                    SetPreroll(item);
                });

            _initialized = true;


            var scheduleElements =
                PlaylistManaged.Where(x => x.StartMode == StartMode.Schedule && x.StartTime >= DateTime.Now);
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

            Parallel.ForEach(PlaylistManaged.ToArray(), (x) =>
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
            var nextElement = PlaylistManaged.ElementAtOrDefault(this.IndexOf(playingElement) + 1);
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

            var previousElement = PlaylistManaged.ElementAtOrDefault(indexCurrentElement - 1);
            if (previousElement == null || previousElement.Status == Status.Ended)
                return;

            var skippedList = new List<T>();
            for (var i = indexCurrentElement - 1; i > 0; i--)
            {
                var element = PlaylistManaged.ElementAt(i);
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
            return PlaylistManaged.ToList().IndexOf(element);
        }

    }
}