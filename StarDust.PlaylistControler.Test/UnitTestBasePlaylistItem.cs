using System;
using System.Threading;
using Xunit;

namespace StarDust.PlaylistControler.Test
{
    public class UnitTestBasePlaylistItem
    {




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


            Assert.True(DateTime.Now >= element.StartTime?.Subtract(element.PrerollStart));
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
                Assert.True(DateTime.Now <= element.StartTime?.Subtract(element.PrerollStart));
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


            Assert.True(DateTime.Now >= element.EndTime?.Subtract(element.PrerollStart));
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
                Assert.True(DateTime.Now <= element.EndTime?.Subtract(element.PrerollStart));
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
