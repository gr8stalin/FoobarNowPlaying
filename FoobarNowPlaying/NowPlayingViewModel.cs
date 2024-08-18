using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace FoobarNowPlaying
{
    public class NowPlayingViewModel : INotifyPropertyChanged
    {
        private readonly string foobarFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "foobar2000/foobar2000.exe");
        private readonly string musicalNoteIconFilepath = "pack://application:,,,/Images/note.png";
        private string songTitle;
        private string songAlbum;
        private string songArtist;

        public string SongTitle
        {
            get { return songTitle; }
            set 
            { 
                songTitle = value;
                OnPropertyChanged(nameof(SongTitle));
            }
        }

        public string SongAlbum
        {
            get { return songAlbum; }
            set 
            { 
                songAlbum = value;
                OnPropertyChanged(nameof(SongAlbum));
            }
        }

        public string SongArtist
        {
            get { return songArtist; }
            set 
            {
                songArtist = value;
                OnPropertyChanged(nameof(SongArtist));
            }
        }

        public Uri MusicalNoteIcon { get; set; }

        public NowPlayingViewModel()
        {
            MusicalNoteIcon = new Uri(musicalNoteIconFilepath);
            CloseFoobarIfAlreadyOpen();
            _ = Task.Run(() =>
            {
                using Process foobarProcess = new();
                ProcessStartInfo startInfo = new()
                {
                    FileName = foobarFilePath
                };

                foobarProcess.StartInfo = startInfo;
                foobarProcess.EnableRaisingEvents = true;
                foobarProcess.Exited += OnFoobarClose;
                foobarProcess.Start();
                foobarProcess.WaitForInputIdle();
                
                while (!foobarProcess.HasExited)
                {
                    FormatTrackData(foobarProcess);
                }
            });
        }

        private void FormatTrackData(Process process)
        {
            Thread.Sleep(250);
            process.Refresh();
            var foobarWindowTitle = process.MainWindowTitle;
            if (TitleIsAppName(foobarWindowTitle) || string.IsNullOrEmpty(foobarWindowTitle))
            {
                SongTitle = string.Empty;
                SongArtist = string.Empty;
                SongAlbum = string.Empty;
                return;
            }

            var titleSegments = foobarWindowTitle.Split(" | ").Select(x => FormatSegment(x)).ToArray();
            SongTitle = titleSegments[0];
            SongArtist = titleSegments[1];
            SongAlbum = titleSegments[2];

            string FormatSegment(string segment)
            {
                if (segment.Contains("foobar2000"))
                {
                    return Regex.Replace(segment, "\\[foobar2000\\]", "").Trim();
                }

                return segment.Trim();
            }

            bool TitleIsAppName(string title) => Regex.Match(title, "foobar2000 v\\d\\.*").Success;
        }

        private void OnFoobarClose(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void CloseFoobarIfAlreadyOpen()
        {
            var foobarProcess = Process.GetProcesses().Where(x => x.ProcessName.Contains("foobar2000", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            if (foobarProcess != null)
            {
                foobarProcess.CloseMainWindow();
                foobarProcess.Close();
            }
        }

        #region INPC
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
