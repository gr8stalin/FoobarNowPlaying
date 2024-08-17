using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FoobarNowPlaying
{
    public class NowPlayingViewModel : INotifyPropertyChanged
    {
        private string songTitle;
        private string songAlum;
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
            get { return songAlum; }
            set 
            { 
                songAlum = value;
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
            
        }

        #region INPC
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
