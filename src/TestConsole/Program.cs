using StarDust.PlaylistControler;
using StarDust.PlaylistControler.Test;
using System;
using System.Linq;

namespace TestConsole
{
    class Program
    {
        private static PlaylistCollection playlist;

        static void Main(string[] args)
        {

            playlist = new PlaylistCollection
            {
                new TestPlaylistElement {
                    ID = 1,
                    StartTime = DateTime.Now.AddSeconds(10), Duration = TimeSpan.FromSeconds(15), ActionOnStartTimeReached = PrintStartTime,ActionOnStartTimeNear = PrintStartTimeNear, ActionOnEndTimeNear = PrintEndTimeNear,ActionOnEndTimeReached = PrintEndTimeReached},
                //new TestPlaylistElement { ID = 2, Duration = TimeSpan.FromSeconds(15),ActionOnStartTimeReached = PrintStartTime, ActionOnStartTimeNear = PrintStartTimeNear, ActionOnEndTimeNear = PrintEndTimeNear, ActionOnEndTimeReached = PrintEndTimeReached},
                //new TestPlaylistElement { ID = 3, Duration = TimeSpan.FromSeconds(15),ActionOnStartTimeReached = PrintStartTime, ActionOnStartTimeNear = PrintStartTimeNear, ActionOnEndTimeNear = PrintEndTimeNear, ActionOnEndTimeReached = PrintEndTimeReached },
                //new TestPlaylistElement { ID = 4, Duration = TimeSpan.FromSeconds(15),ActionOnStartTimeReached = PrintStartTime, ActionOnStartTimeNear = PrintStartTimeNear, ActionOnEndTimeNear = PrintEndTimeNear, ActionOnEndTimeReached = PrintEndTimeReached }
            };

            foreach (var i in Enumerable.Range(2, 100))
            {
                playlist.Add(new TestPlaylistElement
                {
                    ID = i,
                    Duration = TimeSpan.FromSeconds(15),
                    ActionOnStartTimeReached = PrintStartTime,
                    ActionOnStartTimeNear = PrintStartTimeNear,
                    ActionOnEndTimeNear = PrintEndTimeNear,
                    ActionOnEndTimeReached = PrintEndTimeReached
                });
            }

            Console.Read();

        }

        private static void PrintEndTimeReached(object obj)
        {
            Console.WriteLine($"{DateTime.Now} - Element end time reached: {obj}");
        }

        private static void PrintEndTimeNear(object obj)
        {
            Console.WriteLine($"{DateTime.Now} - Element end time near: {obj}");
        }

        private static void PrintStartTimeNear(object obj)
        {
            Console.WriteLine($"{DateTime.Now} - Element start near: {obj}");
        }

        private static void PrintStartTime(object o)
        {
            Console.WriteLine($"{DateTime.Now} Element started: {o}");
        }
    }
}
