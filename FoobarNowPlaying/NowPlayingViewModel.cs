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
        private readonly string FoobarFilepath = 
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "foobar2000", 
                "foobar2000.exe"
                );

        private readonly int OneSecondDelayInMS = 1000;

        private string songName;
        private string artist;
        private string album;

        public string SongName
        {
            get => songName;
            set 
            { 
                songName = value;
                OnPropertyChanged(nameof(SongName));
            }
        }

        public string Artist
        {
            get => artist;
            set 
            { 
                artist = value; 
                OnPropertyChanged(nameof(Artist));
            }
        }

        public string Album
        {
            get => album;
            set 
            { 
                album = value;
                OnPropertyChanged(nameof(Album));
            }
        }

        public NowPlayingViewModel()
        {
            new Thread(() =>
            {
                using Process foobarProcess = new();
                ProcessStartInfo startInfo = new()
                {
                    FileName = FoobarFilepath
                };

                foobarProcess.StartInfo = startInfo;
                foobarProcess.Start();

                foobarProcess.WaitForInputIdle();

                Thread.Sleep(OneSecondDelayInMS);

                while (!foobarProcess.HasExited)
                {
                    FormatTrackTitleFromWindowTitle(foobarProcess);
                }
            })
            { 
                IsBackground = true
            }.Start();
        }

        private void FormatTrackTitleFromWindowTitle(Process process)
        {
            Thread.Sleep(OneSecondDelayInMS);
            process.Refresh();

            string foobarWindowTitle = process.MainWindowTitle;

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

            string ProcessTitleSegment(string titleSegment)
            {
                if (titleSegment.Contains("foobar2000"))
                {
                    return Regex.Replace(titleSegment, @"\[foobar2000\]", "").Trim();
                }

                return titleSegment.Trim();
            }

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
