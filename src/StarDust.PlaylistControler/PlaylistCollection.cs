﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace StarDust.PlaylistControler
{
    public class PlaylistCollection<T> : HashSet<T> where T : ISchedule
    {
        public event EventHandler<CollectionChangeEventArgs> ElementAdded;
        public event EventHandler<CollectionChangeEventArgs> ElementRemoved;
        public event EventHandler<CollectionChangeEventArgs> ElementsRemoved;
        public event EventHandler PlaylistCleared;



        public PlaylistCollection()
        {
        }
        public PlaylistCollection(IEnumerable<T> enumerable) : base(enumerable)
        {

        }

        public new void Add(T item)
        {

            base.Add(item);
            ElementAdded?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Add, item));
        }

        public new void Clear()
        {
            base.Clear();
            PlaylistCleared?.Invoke(this, EventArgs.Empty);
        }


        public new bool Remove(T item)
        {
            var state = base.Remove(item);
            ElementRemoved?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Remove, item));
            return state;
        }

        public new int RemoveWhere(Predicate<T> match)
        {
            var elementsRemoved = this.Where(match.Invoke);
            ElementsRemoved?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Remove, elementsRemoved));
            return base.RemoveWhere(match);
        }

    }
}
