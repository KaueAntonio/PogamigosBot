namespace YoutubeVideoDownloader.Domain.Objects
{
    public class YtVideo
    {
        public string Title { get; set; }
        public string TitleWithExtension { get; set; }
        public byte[] File { get; set; }
        public string Path { get; set; }
    }
}
