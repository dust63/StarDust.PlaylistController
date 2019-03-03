using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace StarDust.PlaylistControler
{
    public class PlaylistCollection : ObservableCollection<BasePlaylistItem>
    {


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
            foreach (var currentElement in list)
            {
                var indexCurrentElement = this.IndexOf(currentElement);
                var previousElement = this.ElementAtOrDefault(indexCurrentElement - 1);
                var nextElement = this.ElementAtOrDefault(indexCurrentElement + 1);

                currentElement.ParentElement = previousElement;
                currentElement.ChildElement = nextElement;
            }
        }


        protected void DefineParentAndChild(int startIndex, int count)
        {
            DefineParentAndChild(this.Skip(startIndex).Take(count));
        }


        #endregion
    }
}
