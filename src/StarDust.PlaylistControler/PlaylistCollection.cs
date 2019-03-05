using StarDust.PlaylistControler.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace StarDust.PlaylistControler
{
    public class PlaylistCollection : ObservableCollection<BasePlaylistItem>
    {

        public BasePlaylistItem CurrentPlayingItem { get; protected set; }
        public BasePlaylistItem PreparedBasePlaylistItem { get; protected set; }


        public event EventHandler<ElementsSkippedEventArgs> ElementsSkipped;
        public event EventHandler ElementStatusChanged;


        public PlaylistCollection() : this(new List<BasePlaylistItem>())
        {
        }

        public PlaylistCollection(IEnumerable<BasePlaylistItem> collection) : base(collection)
        {
            base.CollectionChanged += BaseCollectionChanged;
            DefineParentAndChild(this);
        }

        private void BaseCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnAddToPlaylist(e);
                    break;
                case NotifyCollectionChangedAction.Move:
                    OnMoveToPlaylist(e);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnRemove(e);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    OnReplace(e);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
            }
        }



        #region Define ParentAndChild
        protected void OnReplace(NotifyCollectionChangedEventArgs e)
        {
            DefineParentAndChild(e.OldStartingIndex - 1, 2);
        }

        protected void OnRemove(NotifyCollectionChangedEventArgs e)
        {
            DefineParentAndChild(e.OldStartingIndex - 1, 2);
        }

        protected void OnMoveToPlaylist(NotifyCollectionChangedEventArgs e)
        {

            DefineParentAndChild(e.NewStartingIndex - 1, e.NewItems.Count + 2);
            DefineParentAndChild(e.OldStartingIndex - 1, e.NewItems.Count + 2);
        }

        protected void OnAddToPlaylist(NotifyCollectionChangedEventArgs e)
        {
            DefineParentAndChild(e.NewStartingIndex - 1, 2);
        }



        protected void DefineParentAndChild(IEnumerable<BasePlaylistItem> list)
        {

            Parallel.ForEach(list, new ParallelOptions { MaxDegreeOfParallelism = -1 }, (currentElement) =>
              {
                  var indexCurrentElement = this.IndexOf(currentElement);
                  var nextElement = this.ElementAtOrDefault(indexCurrentElement + 1);
                  currentElement.StartTimeReached += CurrentElement_StartTimeReached;
                  currentElement.StartTimeNear += CurrentElementOnStartTimeNear;
                  currentElement.EndTimeNear += CurrentElementOnEndTimeNear;
                  currentElement.EndTimeReached += CurrentElement_EndTimeReached;

              });

        }

        protected void DefineParentAndChild(int startIndex, int count)
        {
            DefineParentAndChild(this.Skip(startIndex).Take(count));
        }


        #region Element Playing notification


        private void CurrentElementOnStartTimeNear(object sender, EventArgs e)
        {
            PreparedBasePlaylistItem = (BasePlaylistItem)sender;
            PreparedBasePlaylistItem.StartTimeNear -= CurrentElementOnStartTimeNear;

            PreparedBasePlaylistItem.Status = Status.Prepared;
            ElementStatusChanged?.Invoke(PreparedBasePlaylistItem, EventArgs.Empty);
        }
        private void CurrentElement_EndTimeReached(object sender, EventArgs e)
        {
            var playingItem = (BasePlaylistItem)sender;
            playingItem.EndTimeReached -= CurrentElement_EndTimeReached;
            playingItem.Status = Status.Played;
            ElementStatusChanged?.Invoke(playingItem, EventArgs.Empty);
        }
        private void CurrentElement_StartTimeReached(object sender, EventArgs e)
        {
            var newPlayingItem = (BasePlaylistItem)sender;
            newPlayingItem.StartTimeReached -= CurrentElement_StartTimeReached;

            if (newPlayingItem != PreparedBasePlaylistItem)
            {
                CurrentPlayingItem?.CancelCheckingTask();
                PreparedBasePlaylistItem?.CancelCheckingTask();
                if (CurrentPlayingItem != null)
                {
                    var indexCurrentElement = this.IndexOf(newPlayingItem);
                    Task.Factory.StartNew(() => VerifySkippedElement(indexCurrentElement));
                }
            }

            newPlayingItem.Status = Status.Playing;
            ElementStatusChanged?.Invoke(newPlayingItem, EventArgs.Empty);
            CurrentPlayingItem = newPlayingItem;
        }
        private void CurrentElementOnEndTimeNear(object sender, EventArgs e)
        {
            var currentElement = (BasePlaylistItem)sender;
            currentElement.EndTimeNear -= CurrentElementOnEndTimeNear;

            var indexCurrentElement = this.IndexOf(currentElement);
            var nextElement = this.ElementAtOrDefault(indexCurrentElement + 1);

            if (nextElement == null || nextElement.StartMode != StartMode.AutoFollow)
                return;

            nextElement.StartTime = currentElement.EndTime;
            nextElement?.SetCheckingTask(true);
        }

        private void VerifySkippedElement(int indexCurrentElement)
        {
            if (ElementsSkipped == null)
                return;

            var previousElement = this.ElementAtOrDefault(indexCurrentElement - 1);
            if (previousElement == null || previousElement.Status == Status.Played)
                return;

            var skippedList = new List<BasePlaylistItem>();
            for (var i = indexCurrentElement - 1; i > 0; i--)
            {
                var element = this[i];
                if (element.Status == Status.Played)
                    break;
                this[i].Status = Status.Skipped;
                skippedList.Add(element);
            }

            if (skippedList.Any())
                ElementsSkipped?.Invoke(this, new ElementsSkippedEventArgs(skippedList));
        }

        #endregion




        #endregion
    }
}
