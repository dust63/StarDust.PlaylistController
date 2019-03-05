


using StarDust.PlaylistControler;
using StarDust.PlaylistControler.Test;
using System.Linq;
using System.Threading;
using Xunit;

public class UnitTestPlaylistListCollection
{

    [Fact]
    public void CtorEnumerable()
    {
        var t = new[] { new TestPlaylistElement(), new TestPlaylistElement(), };

        var playlist = new PlaylistCollection<TestPlaylistElement>(t);

        Assert.True(playlist.Any());
    }

    [Fact]
    public void AddedEvent()
    {

        var playlist = new PlaylistCollection<TestPlaylistElement>();
        var waitHandle = new AutoResetEvent(false);



        playlist.ElementAdded += (sender, args) =>
        {
            waitHandle.Set();
        };

        playlist.Add(new TestPlaylistElement());

        if (!waitHandle.WaitOne(100, false))
        {
            Assert.True(false);
            return;
        }

        Assert.True(playlist.Any());
    }

    [Fact]
    public void RemovedEvent()
    {

        var playlist = new PlaylistCollection<TestPlaylistElement>();
        var waitHandle = new AutoResetEvent(false);


        playlist.Add(new TestPlaylistElement());
        playlist.Add(new TestPlaylistElement());

        playlist.ElementRemoved += (sender, args) =>
        {
            waitHandle.Set();
        };

        playlist.Remove(playlist.First());

        if (!waitHandle.WaitOne(100, false))
        {
            Assert.True(false);
            return;
        }

        Assert.True(playlist.Count == 1);
    }


    [Fact]
    public void ClearedEvent()
    {

        var playlist = new PlaylistCollection<TestPlaylistElement>();
        var waitHandle = new AutoResetEvent(false);


        playlist.Add(new TestPlaylistElement());
        playlist.Add(new TestPlaylistElement());

        playlist.PlaylistCleared += (sender, args) =>
        {
            waitHandle.Set();
        };

        playlist.Clear();

        if (!waitHandle.WaitOne(100, false))
        {
            Assert.True(false);
            return;
        }

        Assert.True(!playlist.Any());
    }
}