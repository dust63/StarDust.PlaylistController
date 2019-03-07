


using StarDust.PlaylistControler;
using StarDust.PlaylistControler.Test;
using System.Collections.Generic;
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

        TestPlaylistElement elementAdded = null;

        playlist.ElementAdded += (sender, args) =>
        {
            waitHandle.Set();
            elementAdded = (TestPlaylistElement)args.Element;
        };

        playlist.Add(new TestPlaylistElement());

        if (!waitHandle.WaitOne(100, false))
        {
            Assert.True(false);
            return;
        }

        Assert.True(playlist.Any());
        Assert.NotNull(elementAdded);
    }

    [Fact]
    public void AddedEventOnInsert()
    {

        var playlist = new PlaylistCollection<TestPlaylistElement>();
        var waitHandle = new AutoResetEvent(false);
        playlist.Add(new TestPlaylistElement());
        playlist.Add(new TestPlaylistElement());

        TestPlaylistElement elementAdded = null;
        playlist.ElementAdded += (sender, args) =>
        {
            waitHandle.Set();
            elementAdded = (TestPlaylistElement)args.Element;
        };

        playlist.Insert(1, new TestPlaylistElement());

        if (!waitHandle.WaitOne(100, false))
        {
            Assert.True(false);
            return;
        }

        Assert.True(playlist.Any());
        Assert.NotNull(elementAdded);
    }

    [Fact]
    public void RemovedEvent()
    {

        var playlist = new PlaylistCollection<TestPlaylistElement>();
        var waitHandle = new AutoResetEvent(false);


        playlist.Add(new TestPlaylistElement());
        playlist.Add(new TestPlaylistElement());
        TestPlaylistElement elementRemoved = null;

        playlist.ElementRemoved += (sender, args) =>
        {
            waitHandle.Set();
            elementRemoved = (TestPlaylistElement)args.Element;
        };

        playlist.Remove(playlist.First());

        if (!waitHandle.WaitOne(100, false))
        {
            Assert.True(false);
            return;
        }

        Assert.True(playlist.Count == 1);
        Assert.NotNull(elementRemoved);
    }


    [Fact]
    public void RemovedEventOnRemoveRange()
    {

        var playlist = new PlaylistCollection<TestPlaylistElement>();
        var waitHandle = new AutoResetEvent(false);


        playlist.Add(new TestPlaylistElement());
        playlist.Add(new TestPlaylistElement());
        playlist.Add(new TestPlaylistElement());
        playlist.Add(new TestPlaylistElement());

        var elementsRemoved = new List<TestPlaylistElement>();
        playlist.ElementsRemoved += (sender, args) =>
        {
            waitHandle.Set();
            elementsRemoved = (List<TestPlaylistElement>)args.Element;
        };

        playlist.RemoveRange(1, 3);

        if (!waitHandle.WaitOne(100, false))
        {
            Assert.True(false);
            return;
        }

        Assert.True(playlist.Count == 1);
        Assert.True(elementsRemoved.Count == 3);
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