using StarDust.PlaylistControler;
using StarDust.PlaylistControler.Test;
using System;
using System.Linq;

namespace TestConsole
{
    class Program
    {
        private static PlaylistCollection playlist;
        private static readonly PlaylistCollection playlist2;

        static void Main(string[] args)
        {

            playlist = new PlaylistCollection(Enumerable.Range(1, 10000).Select(x => new TestPlaylistElement
            {
                ID = x.ToString(),
                Duration = TimeSpan.FromSeconds(15),
                ActionOnStartTimeReached = PrintStartTime,
                ActionOnStartTimeNear = PrintStartTimeNear,
                ActionOnEndTimeNear = PrintEndTimeNear,
                ActionOnEndTimeReached = PrintEndTimeReached
            }));

            playlist.First().StartTime = DateTime.Now;
            playlist.First().StartMode = StartMode.Schedule;


            playlist[1000].StartMode = StartMode.Schedule;
            playlist[1000].StartTime = DateTime.Now.AddSeconds(10);

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


        private static void PrintEndTimeReached2(object obj)
        {
            Console.WriteLine($"{DateTime.Now} - Element end time reached: {obj}");
        }

        private static void PrintEndTimeNear2(object obj)
        {
            Console.WriteLine($"{DateTime.Now} - Element end time near: {obj}");
        }

        private static void PrintStartTimeNear2(object obj)
        {
            Console.WriteLine($"{DateTime.Now} - Element start near: {obj}");
        }

        private static void PrintStartTime2(object o)
        {
            Console.WriteLine($"{DateTime.Now} Element started: {o}");
        }
    }
}
