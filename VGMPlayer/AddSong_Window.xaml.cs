using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using Microsoft.Win32;
using YoutubeExplode.Videos;
using System.Diagnostics;
using NAudio.Wave;
using System.Globalization;

namespace VGMPlayer
{
    /// <summary>
    /// Interaction logic for AddSong_Window.xaml
    /// </summary>
    public partial class AddSong_Window : Window
    {
        private YoutubeClient youtube;
        public event EventHandler SongAdded;
        private string song;
        private bool uploadingFromComputer;

        public AddSong_Window()
        {
            youtube = new YoutubeClient();
            song = "N/A";
            this.Top = 100;
            uploadingFromComputer= false;

            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            setInvisible();
            this.Hide(); // Closes the window
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (uploadingFromComputer)
                uploadFromComputer();
            else
                uploadFromYoutube();
        }

        private void uploadFromComputer()
        {
            string songSoundtrack = soundtrack.Text;
            if (songSoundtrack.Length == 0) songSoundtrack = "Unknown";

            var libraryPath = App.Current.Properties["libraryPath"];
            var cleanTitle = "";
            if (songLabel.Text != string.Empty)
                cleanTitle = CleanTitle(songLabel.Text); // Cleans illegal characters to bypass errors
            else
                cleanTitle = CleanTitle(System.IO.Path.GetFileNameWithoutExtension(songURL.Text));

            var cleanSoundtrack = CleanTitle(songSoundtrack);

            var audioFile = new AudioFileReader(songURL.Text);
            var duration = audioFile.TotalTime;

            var cleanDuration = CleanDuration(new TimeSpan(duration.Hours, duration.Minutes, duration.Seconds));
            MessageBox.Show(cleanDuration.ToString());

            MainWindow.currentlyViewingMusicList.Add(new MusicList { Filename = $"{cleanTitle} - {cleanSoundtrack} - {cleanDuration}.mp3", Title = cleanTitle, Soundtrack = cleanSoundtrack, Duration = duration });
            System.IO.File.Copy(songURL.Text, libraryPath + $"Song Collection/{cleanTitle} - {cleanSoundtrack} - {cleanDuration}.mp3", true);

            MainWindow.currentlyPlayingMusicList = new List<MusicList>(MainWindow.currentlyViewingMusicList);
            song = $"{cleanTitle} - {cleanSoundtrack} - {cleanDuration}.mp3";
            SongAdded?.Invoke(this, EventArgs.Empty);
            songLabel.Text = "";
            soundtrack.Text = "";
            downloadingLabel.Visibility = Visibility.Collapsed;
            titleLabel.Visibility = Visibility.Collapsed;
            songDurationLabel.Visibility = Visibility.Collapsed;
            rectangle2.Visibility = Visibility.Collapsed;
            rectangle3.Visibility = Visibility.Collapsed;
            songDuration.Text = "";
            songTitle.Text = "";
            okButton.IsEnabled = true; // Disables buttons to notify user and to keep errors away.
            closeButton.IsEnabled = true;
            songLabel.IsEnabled = true;

            this.Hide(); // Lastly hide the window
        }

        private async void uploadFromYoutube()
        { 
            string youtubeID = songURL.Text;
            string songSoundtrack = soundtrack.Text;

            if (youtubeID.Length == 0) this.Close(); // Youtube URL/ID can't be null.

            if (songSoundtrack.Length == 0) songSoundtrack = "Unknown";

            try // Tries to download audio if valid url/id.
            {
                okButton.IsEnabled = false; // Disables buttons to notify user and to keep errors away.
                songLabel.IsEnabled = false;
                songURL.IsEnabled = false;
                soundtrack.IsEnabled = false;
                var video = await youtube.Videos.GetAsync(youtubeID);
                var cleanTitle = "";

                if (songLabel.Text != string.Empty)
                    cleanTitle = CleanTitle(songLabel.Text); // Cleans illegal characters to bypass errors
                else
                    cleanTitle = video.Title;

                var cleanSoundtrack = CleanTitle(songSoundtrack);
                var duration = video.Duration;
                var cleanDuration = CleanDuration((TimeSpan)duration); // Converts duration to legal filename e.g (00.37.12) instead of (00:37:12)
                CheckLibraryStatus(); // Check that root directory exists
                titleLabel.Visibility = Visibility.Visible;
                songDurationLabel.Visibility = Visibility.Visible;
                rectangle2.Visibility = Visibility.Visible;
                rectangle3.Visibility = Visibility.Visible;
                songDuration.Text = cleanDuration;
                songTitle.Text = cleanTitle;

                string libraryPath = App.Current.Properties["libraryPath"].ToString();
                if (libraryPath.Length <= 0)
                {
                    MessageBox.Show("Error getting library path");
                    return;
                }

                if (System.IO.File.Exists(libraryPath + $"Song Collection/{cleanTitle} - {cleanSoundtrack} - {cleanDuration}.mp3"))
                {
                    MessageBox.Show("The song is already in the library", "Error");
                    return;
                }

                MainWindow.currentlyViewingMusicList.Add(new MusicList { Filename = $"{cleanTitle} - {cleanSoundtrack} - {cleanDuration}.mp3", Title = cleanTitle, Soundtrack = cleanSoundtrack, Duration = (TimeSpan)duration });
                var destinationPath = Path.Combine(libraryPath + $"Song Collection/{cleanTitle} - {cleanSoundtrack} - {cleanDuration}.mp3");

                closeButton.IsEnabled = false;
                downloadingLabel.Visibility = Visibility.Visible;

                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(youtubeID);
                var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                if (streamInfo != null)
                {
                    // Get the actual stream
                    var stream = await youtube.Videos.Streams.GetAsync(streamInfo);

                    // Download the stream to file
                    await youtube.Videos.Streams.DownloadAsync(streamInfo, destinationPath);
                }

                MainWindow.currentlyPlayingMusicList = new List<MusicList>(MainWindow.currentlyViewingMusicList);
                song = $"{cleanTitle} - {cleanSoundtrack} - {cleanDuration}.mp3";
                SongAdded?.Invoke(this, EventArgs.Empty);
                songLabel.Text = "";
                soundtrack.Text = "";
                downloadingLabel.Visibility = Visibility.Collapsed;
                titleLabel.Visibility = Visibility.Collapsed;
                songDurationLabel.Visibility = Visibility.Collapsed;
                rectangle2.Visibility = Visibility.Collapsed;
                rectangle3.Visibility = Visibility.Collapsed;
                songDuration.Text = "";
                songTitle.Text = "";
                okButton.IsEnabled = true; // Disables buttons to notify user and to keep errors away.
                closeButton.IsEnabled = true;
                songLabel.IsEnabled = true;
            }
            catch (IOException)
            {
                MessageBox.Show("The song is already in the library", "Error");
            }
            catch (InvalidOperationException er)
            {
                MessageBox.Show(er.ToString(), "error");
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Invalid Youtube video id or url545435.", "Error");
            }
            catch (Exception er)
            {
                MessageBox.Show("Invalid Youtube video id or url2323.", er.ToString());
            }

            this.Hide(); // Lastly hide the window
        }

        private void setVisible()
        {
            importText.Visibility = Visibility.Visible;
            songName.Visibility = Visibility.Visible;
            albumTitle.Visibility = Visibility.Visible;
            okButton.Visibility = Visibility.Visible;
            closeButton.Visibility = Visibility.Visible;
            songURL.Visibility = Visibility.Visible;
            songUrlBorder.Background = Brushes.LightGray;
            soundtrack.Visibility = Visibility.Visible;
            soundtrackBorder.Background = Brushes.LightGray;
            songLabel.Visibility = Visibility.Visible;
            songLabelBorder.Background = Brushes.LightGray;
        }

        private void setInvisible()
        {
            importText.Visibility = Visibility.Hidden;
            songName.Visibility = Visibility.Hidden;
            albumTitle.Visibility = Visibility.Hidden;
            okButton.Visibility = Visibility.Hidden;
            closeButton.Visibility = Visibility.Hidden;
            songURL.Visibility = Visibility.Hidden;
            songURL.Text= string.Empty;
            SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFrom("#272B2F");
            songUrlBorder.Background = brush;
            soundtrack.Visibility = Visibility.Hidden;
            soundtrackBorder.Background = brush;
            songLabel.Visibility = Visibility.Hidden;
            songLabelBorder.Background = brush;
            brush = (SolidColorBrush)new BrushConverter().ConvertFrom("#4C4E52");
            youtubeButton.Background = brush;
            localButton.Background = brush;
        }

        private void songUrl_Click(object sender, MouseButtonEventArgs e)
        {
            if (uploadingFromComputer)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3";
                if (openFileDialog.ShowDialog() == true)
                {
                    songURL.Text = openFileDialog.FileName;
                }
            }
        }
        private void YoutubeButton_Click(object sender, RoutedEventArgs e)
        {
            uploadingFromComputer = false;
            importText.Content = "Youtube video ID / URL:";
            youtubeButton.Background = Brushes.White;
            SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFrom("#4C4E52");
            localButton.Background = brush;
            setVisible();
        }

        private void LocalButton_Click(object sender, RoutedEventArgs e)
        {
            uploadingFromComputer = true;
            localButton.Background = Brushes.White;
            SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFrom("#4C4E52");
            youtubeButton.Background = brush;
            importText.Content = "Upload From Computer:";
            songURL.Text = "Click Me";
            setVisible();
        }
        private void CheckLibraryStatus()
        {
            var libraryPath = App.Current.Properties["libraryPath"];
            if (libraryPath.ToString().Length <= 0)
            {
                MessageBox.Show("Error getting library path");
                return;
            }

            if (!Directory.Exists(libraryPath.ToString()))
            {
                Directory.CreateDirectory(libraryPath.ToString());
            }
        }

        private string CleanDuration(TimeSpan duration)
        {
            return duration.ToString().Replace(':', '.');
        }

        private string CleanTitle(string title)
        {
            return string.Join("_", title.Split(Path.GetInvalidFileNameChars())).Replace('-', '_');
        }

        private void songLabel_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        public string songPath ()
        {
            if (this.song != "N/A")
                 return this.song;
            else
            {
                MessageBox.Show("This should not happen");
                return "";
            }
        }
    }
}