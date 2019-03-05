using System;
using System.Collections.Generic;
using System.Linq;

namespace StarDust.PlaylistControler
{
    public class ElementsSkippedEventArgs<T> : EventArgs where T : ISchedule
    {

        public T[] SkippedElements { get; }

        public ElementsSkippedEventArgs(IEnumerable<T> skippedElements)
        {
            SkippedElements = skippedElements.ToArray();
        }
    }
}