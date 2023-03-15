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

        public AddSong_Window()
        {
            youtube = new YoutubeClient();

            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Closes the window
        }

        private async void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string youtubeID = songLabel.Text;

            if (youtubeID.Length == 0) this.Close(); // Youtube URL/ID can't be null.

            try // Tries to download audio if valid url/id.
            {
                var video = await youtube.Videos.GetAsync(youtubeID);
                var cleanTitle = CleanTitle(video.Title); // Cleans illegal characters to bypass errors
                var duration = video.Duration;
                var cleanDuration = CleanDuration((TimeSpan)duration); // Converts duration to legal filename e.g (00.37.12) instead of (00:37:12)
                CheckLibraryStatus(); // Check that root directory exists
                titleLabel.Visibility = Visibility.Visible;
                songDurationLabel.Visibility = Visibility.Visible;
                rectangle2.Visibility = Visibility.Visible;
                rectangle3.Visibility = Visibility.Visible;
                songDuration.Text = cleanDuration;
                songTitle.Text = cleanTitle;

                var libraryPath = App.Current.Properties["libraryPath"];
                if (libraryPath.ToString().Length <= 0)
                {
                    MessageBox.Show("Error getting library path");
                    return;
                }

                if (File.Exists(libraryPath.ToString() + (this.Owner as MainWindow).PlaylistPage().libraryListView.SelectedValue.ToString() + $"/{cleanTitle} - {cleanDuration}.mp3"))
                {
                    MessageBox.Show("The song is already in the library", "Error");
                    return;
                }

                MainWindow.currentlyViewingMusicList.Add(new MusicList { Filename = $"{cleanTitle} - {cleanDuration}.mp3", Title = cleanTitle, Duration = (TimeSpan)duration });
                var destinationPath = Path.Combine(libraryPath.ToString() + (this.Owner as MainWindow).PlaylistPage().libraryListView.SelectedValue.ToString() + $"/{cleanTitle} - {cleanDuration}.mp3");

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

                    if (!File.Exists(libraryPath + $"/Song Collection/{cleanTitle} - {cleanDuration}.mp3"))
                    {
                        System.IO.File.Copy(destinationPath, libraryPath + $"/Song Collection/{cleanTitle} - {cleanDuration}.mp3");
                    }
                }

                MainWindow.currentlyPlayingMusicList = new List<MusicList>(MainWindow.currentlyViewingMusicList);
                (this.Owner as MainWindow).musicListView.Items.Refresh();
                (this.Owner as MainWindow).PlaylistPage().libraryListView.Items.Refresh(); // Refresh libraryListView to make sure song is added to list and is visible.
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

            this.Close(); // Lastly closes the window
        }


        private void CheckLibraryStatus()
        {
            if (!Directory.Exists("./library"))
            {
                Directory.CreateDirectory("./library");
            }
        }

        private string CleanDuration(TimeSpan duration)
        {
            return duration.ToString().Replace(':', '.');
        }

        private string CleanTitle(string title)
        {
            return string.Join("_", title.Split(Path.GetInvalidFileNameChars()));
        }

        private void songLabel_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}