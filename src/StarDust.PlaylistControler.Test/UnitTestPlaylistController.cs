using System;
using Xunit;

namespace StarDust.PlaylistControler.Test
{
    public class UnitTestPlaylistController
    {

        [Fact]
        public void TestPreroll()
        {
            var playlist = new PlaylistController<TestPlaylistElement>(new PlaylistCollection<TestPlaylistElement>());
            Assert.True(playlist.PrerollStart == TimeSpan.FromSeconds(2));
            Assert.True(playlist.PrerollStart == playlist.PrerollEnd);


            playlist = new PlaylistController<TestPlaylistElement>(new PlaylistCollection<TestPlaylistElement>(), TimeSpan.FromSeconds(1));
            Assert.True(playlist.PrerollStart == TimeSpan.FromSeconds(1));
            Assert.True(playlist.PrerollStart == playlist.PrerollEnd);

            Assert.Throws<ArgumentException>(() =>
                playlist = new PlaylistController<TestPlaylistElement>(new PlaylistCollection<TestPlaylistElement>(),
                    TimeSpan.Zero));
        }

        [Fact]
        public void TestPlaylistCollection()
        {
            var playlist = new PlaylistController<TestPlaylistElement>(new PlaylistCollection<TestPlaylistElement>());
            Assert.NotNull(playlist.PlaylistManaged);

        }
    }
}
