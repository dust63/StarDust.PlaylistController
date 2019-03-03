using System;
using System.Threading;
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




        private TestPlaylistElement GetElementForTiming()
        {
            var element = new TestPlaylistElement
            {
                StartTime = DateTime.Now.AddSeconds(5),
                Duration = TimeSpan.FromSeconds(5)
            };


            Assert.True(element.EndTime == element.StartTime?.Add(element.Duration));

            return element;

        }

        [Fact]
        public void StartTimeReachedIsFired()
        {

            var element = GetElementForTiming();

            var waitHandle = new AutoResetEvent(false);


            element.StartTimeReached += (sender, args) =>
            {
                waitHandle.Set();
            };

            if (!waitHandle.WaitOne(5100, false))
            {
                Assert.True(false, "Event not fired");
                return;
            }


            Assert.True(DateTime.Now >= element.StartTime);
        }

        [Fact]
        public void StartTimeReachedIsNotFired()
        {

            var element = GetElementForTiming();
            element.StartTime = DateTime.Now.AddSeconds(15);
            var waitHandle = new AutoResetEvent(false);


            element.StartTimeReached += (sender, args) =>
            {
                waitHandle.Set();
            };

            if (!waitHandle.WaitOne(5100, false))
            {
                Assert.True(DateTime.Now <= element.StartTime);
                return;
            }


            Assert.True(false, "Event was fired, should not be");
        }


        [Fact]
        public void StartTimeNearIsFired()
        {

            var element = GetElementForTiming();

            var waitHandle = new AutoResetEvent(false);


            element.StartTimeNear += (sender, args) =>
            {
                waitHandle.Set();
            };

            if (!waitHandle.WaitOne(5100, false))
            {
                Assert.True(false, "Event not fired");
                return;
            }


            Assert.True(DateTime.Now >= element.StartTime?.Subtract(element.Preroll));
        }

        [Fact]
        public void StartTimeNearIsNotFired()
        {

            var element = GetElementForTiming();
            element.StartTime = DateTime.Now.AddSeconds(15);
            var waitHandle = new AutoResetEvent(false);


            element.StartTimeReached += (sender, args) =>
            {
                waitHandle.Set();
            };

            if (!waitHandle.WaitOne(2500, false))
            {
                Assert.True(DateTime.Now <= element.StartTime?.Subtract(element.Preroll));
                return;
            }


            Assert.True(false, "Event was fired, should not be");
        }


        [Fact]
        public void EndTimeNearIsFired()
        {

            var element = GetElementForTiming();

            var waitHandle = new AutoResetEvent(false);


            element.EndTimeNear += (sender, args) =>
            {
                waitHandle.Set();
            };

            if (!waitHandle.WaitOne(8100, false))
            {
                Assert.True(false, "Event not fired");
                return;
            }


            Assert.True(DateTime.Now >= element.EndTime?.Subtract(element.Preroll));
        }

        [Fact]
        public void EndTimeNearIsNotFired()
        {

            var element = GetElementForTiming();
            element.StartTime = DateTime.Now.AddSeconds(15);
            var waitHandle = new AutoResetEvent(false);


            element.StartTimeReached += (sender, args) =>
            {
                waitHandle.Set();
            };

            if (!waitHandle.WaitOne(5000, false))
            {
                Assert.True(DateTime.Now <= element.EndTime?.Subtract(element.Preroll));
                return;
            }


            Assert.True(false, "Event was fired, should not be");
        }


        [Fact]
        public void EndTimeIsFired()
        {

            var element = GetElementForTiming();

            var waitHandle = new AutoResetEvent(false);


            element.EndTimeReached += (sender, args) =>
            {
                waitHandle.Set();
            };

            if (!waitHandle.WaitOne(10100, false))
            {
                Assert.True(false, "Event not fired");
                return;
            }


            Assert.True(DateTime.Now >= element.EndTime);
        }

        [Fact]
        public void EndTimeIsNotFired()
        {

            var element = GetElementForTiming();
            element.StartTime = DateTime.Now.AddSeconds(15);
            var waitHandle = new AutoResetEvent(false);


            element.EndTimeReached += (sender, args) =>
            {
                waitHandle.Set();
            };

            if (!waitHandle.WaitOne(5000, false))
            {
                Assert.True(DateTime.Now <= element.EndTime);
                return;
            }


            Assert.True(false, "Event was fired, should not be");
        }
    }
}
