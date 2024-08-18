using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace FoobarNowPlaying
{
    public class NowPlayingViewModel : INotifyPropertyChanged
    {
        private readonly string foobarFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "foobar2000/foobar2000.exe");
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


        public NowPlayingViewModel()
        {
            CloseFoobarIfAlreadyOpen();
            _ = Task.Run(() =>
            {
                using Process foobarProcess = new();
                ProcessStartInfo startInfo = new()
                {
                    FileName = foobarFilePath
                };
                foobarProcess.EnableRaisingEvents = true;
                foobarProcess.Exited += OnFoobarClose;
                foobarProcess.Start();
                foobarProcess.WaitForInputIdle();
                while (!foobarProcess.HasExited)
                {
                    
                }
            });
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
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
