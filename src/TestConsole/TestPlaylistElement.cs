namespace StarDust.PlaylistControler.Test
{
    class TestPlaylistElement : BasePlaylistItem
    {

        public int ID { get; set; }

        public override string ToString()
        {
            return ID.ToString();
        }
    }
}
