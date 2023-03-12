using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VGMPlayer.Properties;

using NAudio.Wave;
using System.Windows.Input;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using MahApps.Metro.IconPacks;
using AngleSharp.Dom;
using Microsoft.VisualBasic.ApplicationServices;
using static System.Windows.Forms.AxHost;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace VGMPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static List<MusicList> currentlyPlayingMusicList = new List<MusicList>();
        public static List<MusicList> currentlyViewingMusicList = new List<MusicList>();

        private MediaPlayer mediaPlayer;

        private bool isPlayingAudio;
        private bool isLooping;

        private int currentPlayingIndex;
        private PlaylistPage playlistPage;
        private HomePage homePage;

        public MainWindow()
        {
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(1);
            dt.Tick += dtTicker;
            dt.Start();

            isPlayingAudio = false;
            isLooping = false;

            mediaPlayer = new MediaPlayer();
            playlistPage = new PlaylistPage();
            homePage = new HomePage();

            playlistPage.PlaylistSelected += (s, e) => EnableSongButton();
            playlistPage.PlaylistDoubleClicked += (s, e) => CheckSongStatus();


            InitializeComponent();

            try
            {
                volumeSlider.Value = Convert.ToDouble(Settings.Default["volume"]);
            }
            catch (Exception)
            {
                volumeSlider.Value = volumeSlider.Maximum / 2;
                Settings.Default["volume"] = volumeSlider.Value.ToString();
                Settings.Default.Save();
            }
            musicListView.ItemsSource = currentlyViewingMusicList;
            CC.Content = HomePage();
        }


        // Updates AudiopositionLabel and AudiopositionSlider every second.
        // If song is completed then loops or skips forward.
        private void dtTicker(object sender, EventArgs e)
        {
            if (isPlayingAudio)
            {
                audioPositionLabel.Content = $"{mediaPlayer.Position.ToString(@"hh\:mm\:ss")} / {currentlyPlayingMusicList[currentPlayingIndex].Duration}";
                audioPositionSlider.Value = mediaPlayer.Position.TotalSeconds;

                if (mediaPlayer.Position >= currentlyPlayingMusicList[currentPlayingIndex].Duration)
                {
                    mediaPlayer.Pause();
                    audioPositionSlider.Value = 0; // Reset slider and label values back to zero.
                    audioPositionLabel.Content = $"{mediaPlayer.Position.ToString(@"hh\:mm\:ss")} / {currentlyPlayingMusicList[currentPlayingIndex].Duration}";

                    if (isLooping)
                    {
                        mediaPlayer.Play();
                    }
                    else
                    {
                        SkipForward();
                    }
                }
            }
        }

        //Loops the song
        private void Loop()
        {
            if (isLooping) // Enables looping
            {
                loopButton.Background = Brushes.Gray;
                isLooping = false;
            }
            else // Disables looping
            {
                loopButton.Background = Brushes.AliceBlue;
                isLooping = true;
            }
        }
        // Check all songs in selected library.
        public void CheckSongStatus()
        {
            Console.WriteLine("Playlist clicked"); // Used only for debuggin purposes
            CC.Content = null;
            currentlyViewingMusicList.Clear();
            musicListView.Items.Refresh();
            string filePath = $"C:/Users/griff/Desktop/VGM Library/library/{PlaylistPage().libraryListView.SelectedValue.ToString()}/";
            FileInfo[] files = new DirectoryInfo(filePath) // Gets songs in date added order
                        .GetFiles("*.mp3")
                        .OrderBy(f => f.CreationTime)
                        .ToArray();

            foreach (var directoryPath in files)
            {
                try
                {
                    string titleAndDuration = directoryPath.Name.ToString();
                    string title = titleAndDuration.Substring(0, titleAndDuration.LastIndexOf('-') - 1);
                    string duration = titleAndDuration.Substring(titleAndDuration.LastIndexOf('-') + 2, 8).Replace('.', ':');

                    currentlyViewingMusicList.Add(new MusicList { Filename = $"{title} - {duration.Replace(':', '.')}.mp3", Title = title, Duration = TimeSpan.Parse(duration) });
                }

                catch (Exception) // Happens if file name doesn't include duration of song.
                {
                    Mp3FileReader reader = new Mp3FileReader($"{filePath}/{directoryPath}");
                    string title = directoryPath.Name.ToString().Remove(directoryPath.Name.ToString().Length - 4);
                    TimeSpan duration = TimeSpan.Parse(reader.TotalTime.ToString(@"hh\:mm\:ss"));
                    currentlyViewingMusicList.Add(new MusicList { Filename = $"{title} - {duration.ToString().Replace(':', '.')}.mp3", Title = title, Duration = duration });
                    reader.Dispose();
                    File.Move($"{filePath}/{directoryPath}", $"{filePath}/{title} - {duration.ToString().Replace(':', '.')}.mp3");
                }
                musicListView.Items.Refresh();
            }
        }

        // Check all songs in selected library.
        public void SongCollectionPage()
        {
            Console.WriteLine("Song Page"); // Used only for debuggin purposes
            CC.Content = null;
            currentlyViewingMusicList.Clear();
            musicListView.Items.Refresh();
            string filePath = "C:/Users/griff/Desktop/VGM Library/library/Song Collection/";
            FileInfo[] files = new DirectoryInfo(filePath) // Gets songs in date added order
                        .GetFiles("*.mp3")
                        .OrderBy(f => f.CreationTime)
                        .ToArray();

            foreach (var directoryPath in files)
            {
                try
                {
                    string titleAndDuration = directoryPath.Name.ToString();
                    string title = titleAndDuration.Substring(0, titleAndDuration.LastIndexOf('-') - 1);
                    string duration = titleAndDuration.Substring(titleAndDuration.LastIndexOf('-') + 2, 8).Replace('.', ':');

                    currentlyViewingMusicList.Add(new MusicList { Filename = $"{title} - {duration.Replace(':', '.')}.mp3", Title = title, Duration = TimeSpan.Parse(duration) });
                }

                catch (Exception) // Happens if file name doesn't include duration of song.
                {
                    Mp3FileReader reader = new Mp3FileReader($"{filePath}/{directoryPath}");
                    string title = directoryPath.Name.ToString().Remove(directoryPath.Name.ToString().Length - 4);
                    TimeSpan duration = TimeSpan.Parse(reader.TotalTime.ToString(@"hh\:mm\:ss"));
                    currentlyViewingMusicList.Add(new MusicList { Filename = $"{title} - {duration.ToString().Replace(':', '.')}.mp3", Title = title, Duration = duration });
                    reader.Dispose();
                    File.Move($"{filePath}/{directoryPath}", $"{filePath}/{title} - {duration.ToString().Replace(':', '.')}.mp3");
                }
                musicListView.Items.Refresh();
            }
        }

        private void SkipForward()
        {
            if (currentPlayingIndex == currentlyPlayingMusicList.Count - 1) // If last song in list, then pause.
            {
                PauseAudio();
                audioPositionSlider.Value = 0; // Reset slider and label values back to zero.
                mediaPlayer.Position = TimeSpan.Zero;
                audioPositionLabel.Content = $"{mediaPlayer.Position.ToString(@"hh\:mm\:ss")} / {currentlyPlayingMusicList[currentPlayingIndex].Duration}";
                return;
            }

            currentPlayingIndex++;
            var mediaFile = new Uri($"C:/Users/griff/Desktop/VGM Library/library/{PlaylistPage().libraryListView.SelectedValue.ToString()}/{currentlyPlayingMusicList[currentPlayingIndex].Filename}");
            mediaPlayer.Open(mediaFile);
            audioPositionSlider.Maximum = currentlyPlayingMusicList[currentPlayingIndex].Duration.TotalSeconds;
            if (isPlayingAudio)
            {
                mediaPlayer.Play();
            }
            currentPlayingLabel.Content = currentlyPlayingMusicList[currentPlayingIndex].Title;

            musicListView.SelectedItem = null;
        }

        private void SkipBackward()
        {
            if (currentPlayingIndex == 0) // If the first item, then doesn't accept to go negative.
            {
                PauseAudio();
                audioPositionSlider.Value = 0; // Reset slider and label values back to zero.
                mediaPlayer.Position = TimeSpan.Zero;
                audioPositionLabel.Content = $"{mediaPlayer.Position.ToString(@"hh\:mm\:ss")} / {currentlyPlayingMusicList[currentPlayingIndex].Duration}";
                return;
            }

            currentPlayingIndex--;
            var mediaFile = new Uri($"C:/Users/griff/Desktop/VGM Library/library/{PlaylistPage().libraryListView.SelectedValue.ToString()}/{currentlyPlayingMusicList[currentPlayingIndex].Filename}");
            mediaPlayer.Open(mediaFile);
            audioPositionSlider.Maximum = currentlyPlayingMusicList[currentPlayingIndex].Duration.TotalSeconds;
            if (isPlayingAudio)
            {
                mediaPlayer.Play();
            }
            currentPlayingLabel.Content = currentlyPlayingMusicList[currentPlayingIndex].Title;

            musicListView.SelectedItem = null;
        }

        private void PlayAudio()
        {
            if (musicListView.Items.Count != 0 && musicListView.SelectedItem != null)
            {
                if (currentPlayingLabel.Content.ToString() != currentlyViewingMusicList[musicListView.SelectedIndex].Title)
                {

                    currentlyPlayingMusicList.Clear();
                    currentlyPlayingMusicList = new List<MusicList>(currentlyViewingMusicList);
                    currentPlayingIndex = musicListView.SelectedIndex;
                    currentPlayingLabel.Content = currentlyPlayingMusicList[currentPlayingIndex].Title;
                    var mediaFile = new Uri($"C:/Users/griff/Desktop/VGM Library/library/{PlaylistPage().libraryListView.SelectedValue.ToString()}/{currentlyPlayingMusicList[currentPlayingIndex].Filename}");
                    mediaPlayer.Open(mediaFile);
                    audioPositionSlider.Maximum = currentlyPlayingMusicList[currentPlayingIndex].Duration.TotalSeconds;
                }
            }
            if (currentlyPlayingMusicList.Count == 0) return;
            mediaPlayer.Play();
            PlayIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Pause;
            isPlayingAudio = true;
        }
        private void AudioPositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (audioPositionSlider.IsFocused)
            {
                mediaPlayer.Position = TimeSpan.FromSeconds(audioPositionSlider.Value);
            }
        }
        private void PauseAudio()
        {
            mediaPlayer.Pause();
            PlayIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Play;
            isPlayingAudio = false;
        }

        private void PlayMedia()
        {
            if (!isPlayingAudio)
            {
                PlayAudio();
            }
            else
            {
                PauseAudio();
            }
        }

        private void PlayButton_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PlayMedia();
        }

        private void VolumeButton_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (volumePanel.IsVisible)
            {
                volumeButton.Background = Brushes.Gray;
            }
            else
            {
                volumeButton.Background = Brushes.AliceBlue;
            }
        }

        private void LoopButton_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Loop();
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Settings.Default["volume"] = volumeSlider.Value.ToString();
            Settings.Default.Save();

            mediaPlayer.Volume = volumeSlider.Value;
        }

        // For renaming a music library
        private void AddSongButton_Click(object sender, RoutedEventArgs e)
        {
            var addSongWin = new AddSong_Window();
            addSongWin.Owner = this;
            addSongWin.ShowDialog();
        }

        // For renaming a music library
        private void AddPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            var newNameWin = new CreatePlaylist_Window("Create new Library");
            newNameWin.Owner = this;
            newNameWin.ShowDialog();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            CC.Content = HomePage();
            addSongButton.IsEnabled = false;
            addPlaylistButton.IsEnabled = false;
        }
        
        // For opening playlist view
        private void PlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            CC.Content = PlaylistPage();
            addSongButton.IsEnabled = false;
            addPlaylistButton.IsEnabled = true;
        }
        // For opening playlist view
        private void AllSongButton_Click(object sender, RoutedEventArgs e)
        {
            SongCollectionPage();
            addPlaylistButton.IsEnabled = false;
        }


        public PlaylistPage PlaylistPage()
        {
            return this.playlistPage;
        }
        public HomePage HomePage()
        {
            return this.homePage;
        }

        public void DisableSongButton()
        {
            addSongButton.IsEnabled = false;
        }

        public void EnableSongButton()
        {
            addSongButton.IsEnabled = true;
        }

        private void MusicListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Console.WriteLine("MusicListView_SelectionChanged"); // Used only for debuggin purposes
        }

        private void PlayMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PlayMedia();
        }

        private void PauseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PlayMedia();
        }

        private void SkipForwardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SkipForward();
        }

        private void SkipBackwardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SkipBackward();
        }

        private void LoopMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Loop();
        }

        private void Volume100MenuItem_Click(object sender, RoutedEventArgs e)
        {
            volumeSlider.Value = volumeSlider.Maximum;
        }

        private void Volume75MenuItem_Click(object sender, RoutedEventArgs e)
        {
            volumeSlider.Value = volumeSlider.Maximum * 0.75f;
        }

        private void Volume50MenuItem_Click(object sender, RoutedEventArgs e)
        {
            volumeSlider.Value = volumeSlider.Maximum * 0.5f;
        }

        private void Volume25MenuItem_Click(object sender, RoutedEventArgs e)
        {
            volumeSlider.Value = volumeSlider.Maximum * 0.25f;
        }

        private void Volume0MenuItem_Click(object sender, RoutedEventArgs e)
        {
            volumeSlider.Value = 0;
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void CloseApp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
        private void MinimizeApp(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void MaximizeApp(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        private void SkipForward_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SkipForward();
        }

        private void SkipBackward_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SkipBackward();
        }

        private void PlaySongMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PlayAudio();
        }

        private void DeleteSongMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (currentlyPlayingMusicList.Count == 0)
            {
                DeleteSongItem();
            }
            else
            {
                if (currentlyPlayingMusicList[musicListView.SelectedIndex].Equals(currentlyViewingMusicList[musicListView.SelectedIndex])
                    && currentlyViewingMusicList[musicListView.SelectedIndex].Title == currentPlayingLabel.Content.ToString()) // Can't delete already playing song
                {
                    MessageBox.Show("Can't delete song that is playing.", "Erorr");
                }
                else
                {
                    DeleteSongItem();
                }
            }
        }

        private void DeleteSongItem()
        {
            if (musicListView.SelectedIndex == -1) return;

            string songPath = $"C:/Users/griff/Desktop/VGM Library/library/{PlaylistPage().libraryListView.SelectedValue.ToString()}/{currentlyViewingMusicList[musicListView.SelectedIndex].Filename}";

            if (currentlyPlayingMusicList.SequenceEqual(currentlyViewingMusicList) && currentPlayingLabel.Content.ToString().Length != 0)
            {
                currentlyPlayingMusicList.RemoveAt(musicListView.SelectedIndex);
            }

            currentlyViewingMusicList.RemoveAt(musicListView.SelectedIndex);
            musicListView.Items.Refresh();
            File.Delete(songPath);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
    }
}

