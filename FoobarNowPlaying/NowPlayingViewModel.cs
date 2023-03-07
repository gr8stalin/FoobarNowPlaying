using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace FoobarNowPlaying
{
    public class NowPlayingViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// I have Foobar2000 installed in Program Files (x86).
        /// Foobar can be installed in other locations.
        /// </summary>
        private readonly string FoobarFilepath = 
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "foobar2000", 
                "foobar2000.exe"
                );

        /// <summary>
        /// The bundled location for the small musical note glyph
        /// </summary>
        private readonly string MusicalNoteIconFilepath = "pack://application:,,,/Images/note.png";

        /// <summary>
        /// This delay is used to wait for the Foobar2000 window title to
        /// update as the track changes
        /// </summary>
        private readonly int HalfSecondInMS = 500;

        private string songName;
        private string artist;
        private string album;

        /// <summary>
        /// The name of the song currently playing
        /// </summary>
        public string SongName
        {
            get => songName;
            set 
            { 
                songName = value;
                OnPropertyChanged(nameof(SongName));
            }
        }

        /// <summary>
        /// The artist of the current song
        /// </summary>
        public string Artist
        {
            get => artist;
            set 
            { 
                artist = value; 
                OnPropertyChanged(nameof(Artist));
            }
        }

        /// <summary>
        /// The album the song comes from
        /// </summary>
        public string Album
        {
            get => album;
            set 
            { 
                album = value;
                OnPropertyChanged(nameof(Album));
            }
        }

        /// <summary>
        /// XAML binding path for the musical note icon
        /// </summary>
        public Uri MusicalNoteIcon { get; set; }

        public NowPlayingViewModel()
        {
            MusicalNoteIcon = new Uri(MusicalNoteIconFilepath);

            CloseFoobarIfAlreadyOpen();

            new Thread(() =>
            {
                using Process foobarProcess = new();
                ProcessStartInfo startInfo = new()
                {
                    FileName = FoobarFilepath
                };

                foobarProcess.StartInfo = startInfo;
                foobarProcess.EnableRaisingEvents = true;
                foobarProcess.Exited += new EventHandler(OnFoobarClose);
                foobarProcess.Start();

                foobarProcess.WaitForInputIdle();

                while (!foobarProcess.HasExited)
                {
                    FormatTrackTitleFromWindowTitle(foobarProcess);
                }
            })
            { 
                IsBackground = true
            }.Start();
        }

        /// <summary>
        /// Event handler for closing FoobarNowPlaying when Foobar2000 closes
        /// </summary>
        private void OnFoobarClose(object sender, EventArgs eventArgs)
        {
            Environment.Exit(0);
        }

        private void CloseFoobarIfAlreadyOpen()
        {
            var foobarProcess = 
                Process.GetProcesses()
                    .Where(x => x.ProcessName.ToLower().Contains("foobar2000"))
                    .Select(y => y)
                    .FirstOrDefault();

            if (foobarProcess is not null) 
            { 
                foobarProcess.CloseMainWindow();
                foobarProcess.Close();
            }
        }

        private void FormatTrackTitleFromWindowTitle(Process process)
        {
            // Waiting for the Foobar2000 window title to update
            Thread.Sleep(HalfSecondInMS);
            process.Refresh();

            string foobarWindowTitle = process.MainWindowTitle;

            // We don't want to display anything if nothing is currently playing
            if (TitleIsAppName(foobarWindowTitle) || string.IsNullOrEmpty(foobarWindowTitle))
            {
                SongName = string.Empty;
                Artist = string.Empty;
                Album = string.Empty;
                return;
            }

            var splitTitle =
                Regex.Split(foobarWindowTitle, @"\s\|\s")
                    .Select(segment => ProcessTitleSegment(segment))
                    .ToList();

            SongName = splitTitle[0];
            Artist = splitTitle[1];
            Album = splitTitle[2];

            // The window title is always going to contain "[Foobar2000]" when
            // something is playing. We're going to want to trim everything
            // we parse from the title as well.
            string ProcessTitleSegment(string titleSegment)
            {
                if (titleSegment.Contains("foobar2000"))
                {
                    return Regex.Replace(titleSegment, @"\[foobar2000\]", "").Trim();
                }

                return titleSegment.Trim();
            }

            // This is a regex because I'm eventually going to update my
            // Foobar2000 installation and I want this to continue working.
            bool TitleIsAppName(string title)
            {
                return Regex.Match(title, @"foobar2000 v\d\.\d\.\d").Success;
            }
        }

        #region INPC Implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
