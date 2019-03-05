namespace StarDust.PlaylistControler.Test
{
    class TestPlaylistElement : BasePlaylistItem
    {

        public string ID { get; set; }

        public override string ToString()
        {
            return ID.ToString();
        }
    }
}
