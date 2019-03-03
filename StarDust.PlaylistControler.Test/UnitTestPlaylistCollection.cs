using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace StarDust.PlaylistControler.Test
{
    public class UnitTestPlaylistCollection
    {
        [Fact]
        public void AddItem()
        {

            var playlist = GetBaseCollection();

            Assert.NotNull(playlist);

            var newElement = new TestPlaylistElement { ID = 5 };
            playlist.Add(newElement);
            TestParentChild(playlist);


            var newElement2 = new TestPlaylistElement { ID = 6 };
            playlist.Add(newElement2);
            TestParentChild(playlist);
        }



        private void TestParentChild(PlaylistCollection collection)
        {
            var i = 0;
            foreach (var item in collection)
            {
                var previousElement = collection.ElementAtOrDefault(i - 1);
                var nextElement = collection.ElementAtOrDefault(i + 1);

                if (i == 0)
                    Assert.Null(item.ParentElement);

                if (item == collection.LastOrDefault())
                    Assert.Null(item.ChildElement);

                Assert.StrictEqual(previousElement, item.ParentElement);
                Assert.StrictEqual(nextElement, item.ChildElement);

                i++;

            }
        }


        [Fact]
        public void TestContructorWithEnum()
        {
            var playlist = GetBaseCollection();
            TestParentChild(playlist);
        }


        private PlaylistCollection GetBaseCollection()
        {
            var firstElement = new TestPlaylistElement { ID = 1 };
            var secondElement = new TestPlaylistElement { ID = 2 };
            var thirdElement = new TestPlaylistElement { ID = 3 };
            var fourthElement = new TestPlaylistElement { ID = 4 };


            var list = new List<BasePlaylistItem> { firstElement, secondElement, thirdElement, fourthElement };
            var playlist = new PlaylistCollection(list);
            Assert.NotNull(playlist);
            return playlist;
        }


        [Fact]
        public void MoveItem()
        {
            var playlist = GetBaseCollection();
            playlist.Move(2, 0);

            TestParentChild(playlist);
        }

        [Fact]
        public void RemoveItem()
        {
            var playlist = GetBaseCollection();
            playlist.Remove(playlist[2]);

            TestParentChild(playlist);

        }



        [Fact]
        public void ReplaceItem()
        {
            var playlist = GetBaseCollection();
            playlist[0] = new TestPlaylistElement { ID = 6 };

            TestParentChild(playlist);

        }

        [Fact]
        public void TestSkipTake()
        {
            var startIndex = 1;
            var count = 2;
            var playlist = GetBaseCollection();
            var t = playlist.Skip(startIndex).Take(count).ToArray();
        }



    }
}
