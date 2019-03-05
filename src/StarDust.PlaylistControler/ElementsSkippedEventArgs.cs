using System;
using System.Collections.Generic;

namespace StarDust.PlaylistControler
{
    public class ElementsSkippedEventArgs : EventArgs
    {

        public List<BasePlaylistItem> SkippedElements { get; }

        public ElementsSkippedEventArgs(IEnumerable<BasePlaylistItem> skippedElements)
        {
            SkippedElements = new List<BasePlaylistItem>(skippedElements);
        }
    }
}