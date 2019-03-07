using StarDust.PlaylistControler.Models;
using System;
using System.Threading;
using Xunit;

namespace StarDust.PlaylistControler.Test
{

    public class UnitTestBasePlaylistItem
    {



        [Fact]
        private TestPlaylistElement CheckProperties()
        {
            var expectedStartTime = DateTime.Now.AddSeconds(5);

            var element = new TestPlaylistElement
            {
                StartTime = expectedStartTime,
                Duration = TimeSpan.FromSeconds(5),
                StartMode = StartMode.AutoFollow,
                Status = Status.Started
            };



            Assert.True(element.StartTime == expectedStartTime);
            Assert.True(element.Duration == TimeSpan.FromSeconds(5));
            Assert.True(element.EndTime == expectedStartTime.Add(TimeSpan.FromSeconds(5)));
            Assert.True(element.StartMode == StartMode.AutoFollow);
            Assert.True(element.Status == Status.Started);
            return element;
        }


        [Fact]
        public void None_NotStartChecking()
        {
            var element = CheckProperties();
            element.StartMode = StartMode.None;
            element.StartTime = DateTime.Now.AddSeconds(2);

            var waitHandle = new AutoResetEvent(false);


            element.StartTimeReached += (sender, args) =>
            {
                waitHandle.Set();
            };
            element.StartScheduling();
            if (!waitHandle.WaitOne(3000, false))
            {
                Assert.True(true);
                return;
            }


            Assert.True(false, "Event was fired, should not be");
        }




        #region AutoFollow

        [Fact]
        public void AutoFollow_StartTimeReachedIsFired()
        {

            var element = CheckProperties();
            var toWait = (element.StartTime.Value - DateTime.Now).Add(TimeSpan.FromMilliseconds(100));
            var waitHandle = new AutoResetEvent(false);


            element.StartTimeReached += (sender, args) =>
            {
                waitHandle.Set();
            };

            element.StartScheduling();

            if (!waitHandle.WaitOne(toWait, false))
            {
                Assert.True(false, "Event not fired");
                return;
            }


            Assert.True(DateTime.Now >= element.StartTime, $"Difference = {DateTime.Now - element.StartTime.Value}");
        }

        [Fact]
        public void AutoFollow_StartTimeReachedIsNotFired()
        {

            var element = CheckProperties();
            var toWait = (element.StartTime.Value - DateTime.Now).Subtract(TimeSpan.FromMilliseconds(100));
            var waitHandle = new AutoResetEvent(false);


            element.StartTimeReached += (sender, args) =>
            {
                waitHandle.Set();
            };
            element.StartScheduling();
            if (!waitHandle.WaitOne(toWait, false))
            {
                Assert.True(DateTime.Now <= element.StartTime);
                return;
            }


            Assert.True(false, "Event was fired, should not be");


        }


        [Fact]
        public void AutoFollow_StartTimeNearIsFired()
        {

            var element = CheckProperties();

            var waitHandle = new AutoResetEvent(false);
            var toWait = (element.StartTime.Value - DateTime.Now).Subtract(element.PrerollStart).Add(TimeSpan.FromMilliseconds(100));


            element.StartTimeNear += (sender, args) =>
            {
                waitHandle.Set();
            };
            element.StartScheduling();
            if (!waitHandle.WaitOne(toWait, false))
            {
                Assert.True(false, "Event not fired");
                return;
            }


            Assert.True(DateTime.Now >= element.StartTime?.Subtract(element.PrerollStart));
        }

        [Fact]
        public void AutoFollow_StartTimeNearIsNotFired()
        {

            var element = CheckProperties();
            element.StartTime = DateTime.Now.AddSeconds(15);
            var waitHandle = new AutoResetEvent(false);
            var toWait = (element.StartTime.Value - DateTime.Now).Subtract(element.PrerollStart).Subtract(TimeSpan.FromMilliseconds(500));

            element.StartTimeReached += (sender, args) =>
            {
                waitHandle.Set();
            };
            element.StartScheduling();
            if (!waitHandle.WaitOne(toWait, false))
            {
                Assert.True(DateTime.Now <= element.StartTime?.Subtract(element.PrerollStart));
                return;
            }


            Assert.True(false, "Event was fired, should not be");
        }


        [Fact]
        public void AutoFollow_EndTimeNearIsFired()
        {

            var element = CheckProperties();
            var waitHandle = new AutoResetEvent(false);
            var toWait = (element.EndTime.Value - DateTime.Now).Subtract(element.PrerollEnd).Add(TimeSpan.FromMilliseconds(100));

            element.EndTimeNear += (sender, args) =>
            {
                waitHandle.Set();
            };
            element.StartScheduling();
            if (!waitHandle.WaitOne(toWait, false))
            {
                Assert.True(false, "Event not fired");
                return;
            }


            Assert.True(DateTime.Now >= element.EndTime?.Subtract(element.PrerollEnd));
        }

        [Fact]
        public void AutoFollow_EndTimeNearIsNotFired()
        {

            var element = CheckProperties();
            var waitHandle = new AutoResetEvent(false);
            var toWait = (element.EndTime.Value - DateTime.Now).Subtract(element.PrerollEnd).Subtract(TimeSpan.FromMilliseconds(500));

            element.EndTimeNear += (sender, args) =>
            {
                waitHandle.Set();
            };
            element.StartScheduling();
            if (!waitHandle.WaitOne(toWait, false))
            {
                Assert.True(DateTime.Now <= element.EndTime?.Subtract(element.PrerollStart));
                return;
            }


            Assert.True(false, "Event was fired, should not be");
        }


        [Fact]
        public void AutoFollow_EndTimeIsFired()
        {

            var element = CheckProperties();
            var toWait = (element.EndTime.Value - DateTime.Now).Add(TimeSpan.FromMilliseconds(100));
            var waitHandle = new AutoResetEvent(false);


            element.EndTimeReached += (sender, args) =>
            {
                waitHandle.Set();
            };
            element.StartScheduling();
            if (!waitHandle.WaitOne(toWait, false))
            {
                Assert.True(false, "Event not fired");
                return;
            }


            Assert.True(DateTime.Now >= element.EndTime);
        }

        [Fact]
        public void AutoFollow_EndTimeIsNotFired()
        {

            var element = CheckProperties();
            var toWait = (element.EndTime.Value - DateTime.Now).Subtract(TimeSpan.FromMilliseconds(100));
            var waitHandle = new AutoResetEvent(false);


            element.EndTimeReached += (sender, args) =>
            {
                waitHandle.Set();
            };
            element.StartScheduling();
            if (!waitHandle.WaitOne(toWait, false))
            {
                Assert.True(DateTime.Now <= element.EndTime);
                return;
            }


            Assert.True(false, "Event was fired, should not be");
        }

        #endregion


        #region Schedule


        [Fact]
        public void Schedule_StartTimeReachedIsFired()
        {

            var element = CheckProperties();
            element.StartMode = StartMode.Schedule;
            var scheduleDate = DateTime.Now.AddSeconds(1);
            element.StartTime = scheduleDate;

            var waitHandle = new AutoResetEvent(false);


            element.StartTimeReached += (sender, args) =>
            {
                waitHandle.Set();
            };

            element.StartScheduling();

            if (!waitHandle.WaitOne(1100, false))
            {
                Assert.True(false, "Event not fired");
                return;
            }


            Assert.True(DateTime.Now >= element.StartTime);
        }

        [Fact]
        public void Schedule_StartTimeReachedIsNotFired()
        {

            var element = CheckProperties();
            element.StartMode = StartMode.Schedule;
            var scheduleDate = DateTime.Now.AddSeconds(5);
            element.StartTime = scheduleDate;

            var waitHandle = new AutoResetEvent(false);


            element.StartTimeReached += (sender, args) =>
            {
                waitHandle.Set();
            };
            element.StartScheduling();
            if (!waitHandle.WaitOne(4100, false))
            {
                Assert.True(DateTime.Now <= element.StartTime);
                return;
            }


            Assert.True(false, "Event was fired, should not be");


        }


        [Fact]
        public void Schedule_StartTimeNearIsFired()
        {

            var element = CheckProperties();
            element.StartMode = StartMode.Schedule;
            var scheduleDate = DateTime.Now.AddSeconds(5);
            element.StartTime = scheduleDate;

            var waitHandle = new AutoResetEvent(false);


            element.StartTimeNear += (sender, args) =>
            {
                waitHandle.Set();
            };
            element.StartScheduling();
            if (!waitHandle.WaitOne(3100, false))
            {
                Assert.True(false, "Event not fired");
                return;
            }


            Assert.True(DateTime.Now >= element.StartTime?.Subtract(element.PrerollStart));
        }

        [Fact]
        public void Schedule_StartTimeNearIsNotFired()
        {

            var element = CheckProperties();
            element.StartMode = StartMode.Schedule;
            var scheduleDate = DateTime.Now.AddSeconds(5);
            element.StartTime = scheduleDate;

            var waitHandle = new AutoResetEvent(false);


            element.StartTimeReached += (sender, args) =>
            {
                waitHandle.Set();
            };
            element.StartScheduling();
            if (!waitHandle.WaitOne(2500, false))
            {
                Assert.True(DateTime.Now <= element.StartTime?.Subtract(element.PrerollStart));
                return;
            }


            Assert.True(false, "Event was fired, should not be");
        }


        #endregion


    }
}
