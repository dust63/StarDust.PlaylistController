using StarDust.PlaylistControler;
using StarDust.PlaylistControler.Test;
using System;
using System.Linq;

namespace TestConsole
{
    class Program
    {
        private static PlaylistController<TestPlaylistElement> PlaylistController;


        static void Main(string[] args)
        {

            var playlist = new PlaylistCollection<TestPlaylistElement>(Enumerable.Range(1, 100000).Select(x => new TestPlaylistElement
            {
                ID = x,
                Duration = TimeSpan.FromSeconds(15),

            }));



            playlist.First().StartTime = DateTime.Now;
            playlist.First().StartMode = StartMode.Schedule;

            playlist.RemoveWhere(x => x.ID > 8000);

            playlist.ElementAt(1000).StartMode = StartMode.Schedule;
            playlist.ElementAt(1000).StartTime = DateTime.Now.AddSeconds(10);




            PlaylistController = new PlaylistController<TestPlaylistElement>(playlist)
            {
                ActionOnStartTimeReached = PrintStartTime,
                ActionOnStartTimeNear = PrintStartTimeNear,
                ActionOnEndTimeNear = PrintEndTimeNear,
                ActionOnEndTimeReached = PrintEndTimeReached
            };

            PlaylistController.Initialize();
            PlaylistController.PlaylistStopped += (sender, eventArgs) => Console.WriteLine("Playlist stopped");
            PlaylistController.ElementsSkipped += (sender, eventArgs) =>
                Console.WriteLine($"{eventArgs.SkippedElements.Length} elements was skipped");

            playlist.ElementAt(5000).StartTime = DateTime.Now.AddSeconds(30);
            playlist.ElementAt(5000).StartMode = StartMode.Schedule;


            playlist.ElementAt(5002).StartMode = StartMode.None;


            Console.WriteLine("Playlist ready to run");
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
