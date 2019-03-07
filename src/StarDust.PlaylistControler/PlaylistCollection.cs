using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace StarDust.PlaylistControler
{
    public class PlaylistCollection<T> : List<T> where T : IPlaylistItem
    {
        public event EventHandler<CollectionChangeEventArgs> ElementAdded;
        public event EventHandler<CollectionChangeEventArgs> ElementRemoved;
        public event EventHandler<CollectionChangeEventArgs> ElementsRemoved;
        public event EventHandler PlaylistCleared;
        public event EventHandler ScheduleInfoElementChanged;


        public PlaylistCollection()
        {
        }
        public PlaylistCollection(IEnumerable<T> enumerable) : base(enumerable)
        {

        }

        protected void OnScheduleInfoElementChanged(object sender, EventArgs e)
        {
            ScheduleInfoElementChanged?.Invoke(sender, e);
        }


        public new void Add(T item)
        {
            base.Add(item);
            OnAdd(item);
        }

        public new void AddRange(IEnumerable<T> items)
        {
            base.AddRange(items);
            Parallel.ForEach(items, OnAdd);
        }

        public new void Insert(int index, T item)
        {
            base.Insert(index, item);
            OnAdd(item);
        }

        public new void InsertRange(int index, IEnumerable<T> items)
        {
            base.InsertRange(index, items);
            Parallel.ForEach(items, OnAdd);
        }


        public new void Clear()
        {

            base.Clear();
            OnClear();
        }


        public new bool Remove(T item)
        {
            var state = base.Remove(item);
            OnElementRemove(item);
            return state;
        }

        public new void RemoveAt(int index)
        {
            var elementRemove = this[index];
            base.RemoveAt(index);
            OnElementRemove(elementRemove);
        }

        public new void RemoveRange(int index, int count)
        {
            var elementsRemoved = GetRange(index, count);
            base.RemoveRange(index, count);
            OnElementsRemoved(elementsRemoved);
        }

        public new void RemoveAll(Predicate<T> match)
        {
            var elementsRemoved = this.Where(match.Invoke);
            OnElementsRemoved(elementsRemoved);
            base.RemoveAll(match);
        }


        private void OnAdd(T element)
        {
            ElementAdded?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Add, element));
            element.ScheduleInfoChanged += OnScheduleInfoElementChanged;

        }


        private void OnClear()
        {
            PlaylistCleared?.Invoke(this, EventArgs.Empty);
        }

        private void OnElementRemove(T element)
        {
            ElementRemoved?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Remove, element));
            element.ScheduleInfoChanged -= OnScheduleInfoElementChanged;
        }


        private void OnElementsRemoved(IEnumerable<T> elements)
        {
            ElementsRemoved?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Remove, elements));
            Parallel.ForEach(elements, (i) => i.ScheduleInfoChanged -= OnScheduleInfoElementChanged);
        }





    }
}
