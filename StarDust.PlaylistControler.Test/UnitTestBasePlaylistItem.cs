using Xunit;

namespace StarDust.PlaylistControler.Test
{
    public class UnitTestBasePlaylistItem
    {

        [Fact]
        public void TestChildRelation()
        {
            var element1 = new TestPlaylistElement { ID = 1 };
            var element2 = new TestPlaylistElement { ID = 2 };
            var element3 = new TestPlaylistElement { ID = 3 };


            element1.ChildElement = element2;
            element2.ChildElement = element3;

            Assert.Null(element1.ParentElement);
            Assert.StrictEqual(element1.ChildElement, element2);
            Assert.StrictEqual(element2.ParentElement, element1);


            element1.ChildElement = null;
            Assert.Null(element2.ParentElement);
            Assert.Null(element1.ChildElement);


            Assert.StrictEqual(element2.ChildElement, element3);
            Assert.StrictEqual(element3.ParentElement, element2);

            Assert.Null(element3.ChildElement);
        }

        [Fact]
        public void TestParentRelation()
        {
            var element1 = new TestPlaylistElement { ID = 1 };
            var element2 = new TestPlaylistElement { ID = 2 };
            var element3 = new TestPlaylistElement { ID = 3 };


            element2.ParentElement = element1;
            element3.ParentElement = element2;

            Assert.Null(element1.ParentElement);
            Assert.StrictEqual(element1.ChildElement, element2);
            Assert.StrictEqual(element2.ParentElement, element1);

            element2.ParentElement = null;
            Assert.Null(element2.ParentElement);
            Assert.Null(element1.ChildElement);

            Assert.StrictEqual(element2.ChildElement, element3);
            Assert.StrictEqual(element3.ParentElement, element2);

            Assert.Null(element3.ChildElement);
        }


        [Fact]
        public void TestMixedRelation()
        {

            var element1 = new TestPlaylistElement { ID = 1 };
            var element2 = new TestPlaylistElement { ID = 2 };
            var element3 = new TestPlaylistElement { ID = 3 };


            element3.ParentElement = element2;
            element1.ChildElement = element2;


            Assert.StrictEqual(element1.ChildElement, element2);
            Assert.StrictEqual(element2.ParentElement, element1);

            Assert.StrictEqual(element2.ChildElement, element3);
            Assert.StrictEqual(element3.ParentElement, element2);

            Assert.Null(element3.ChildElement);
        }
    }
}
