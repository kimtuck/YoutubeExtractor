using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExtractor;

namespace ExtractorUI
{
    public class AudioDownloader
    {
        public AudioDownloader(Action<double> progressCallback)
        {
            this.progressCallback = progressCallback;
        }
        Action<double> progressCallback;

        static string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        public bool Download(string url)
        {
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(url, false);
            return DownloadAudio(videoInfos);
        }

        private bool DownloadAudio(IEnumerable<VideoInfo> videoInfos)
        {
            VideoInfo video = videoInfos.First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 0);
            var path = Path.Combine(dir,
                       RemoveIllegalPathCharacters(video.Title) + ".mp3");
            var audioDownloader = new VideoDownloader(video, path);

            // Register the progress events. We treat the download progress as 85% of the progress
            // and the extraction progress only as 15% of the progress, because the download will
            // take much longer than the audio extraction.
            audioDownloader.DownloadProgressChanged += updateProgress;
            try
            {
                audioDownloader.Execute();
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        private void updateProgress(object sender, ProgressEventArgs args)
        {
            progressCallback(args.ProgressPercentage);
        }
        private static string RemoveIllegalPathCharacters(string path)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(path, "").Replace(" ","");
        }
    }
}
