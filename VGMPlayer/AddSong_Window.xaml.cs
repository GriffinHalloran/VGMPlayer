using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

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

        public AddSong_Window()
        {
            youtube = new YoutubeClient();
            song = "N/A";

            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Closes the window
        }

        private async void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string youtubeID = songURL.Text;
            string songSoundtrack = soundtrack.Text;

            if (youtubeID.Length == 0) this.Close(); // Youtube URL/ID can't be null.

            if (songSoundtrack.Length == 0) songSoundtrack = "Unknown";

            try // Tries to download audio if valid url/id.
            {
                var video = await youtube.Videos.GetAsync(youtubeID);
                var cleanTitle = CleanTitle(songLabel.Text); // Cleans illegal characters to bypass errors
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

                if (File.Exists(libraryPath + $"Song Collection/{cleanTitle} - {cleanSoundtrack} - {cleanDuration}.mp3"))
                {
                    MessageBox.Show("The song is already in the library", "Error");
                    return;
                }

                MainWindow.currentlyViewingMusicList.Add(new MusicList { Filename = $"{cleanTitle} - {cleanSoundtrack} - {cleanDuration}.mp3", Title = cleanTitle, Soundtrack = cleanSoundtrack, Duration = (TimeSpan)duration });
                var destinationPath = Path.Combine(libraryPath + $"Song Collection/{cleanTitle} - {cleanSoundtrack} - {cleanDuration}.mp3");

                okButton.IsEnabled = false; // Disables buttons to notify user and to keep errors away.
                closeButton.IsEnabled = false;
                songLabel.IsEnabled = false;
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