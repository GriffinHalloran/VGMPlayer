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
using MessageBox = System.Windows.MessageBox;
using MahApps.Metro.IconPacks;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;
using AngleSharp.Dom;
using System.Security.Policy;

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
        private bool renameSoundtrack;
        private bool viewingSongCollection;
        private bool viewingPlaylists;
        private bool viewingSoundtracks;
        private bool addingToPlaylists;

        private int currentPlayingIndex;
        private PlaylistPage playlistPage;
        private HomePage homePage;
        private SoundtrackPage soundtrackPage;
        private Rename_Window renameWindow;
        private AddSong_Window addSongWindow;

        public MainWindow()
        { 
            App.Current.Properties["libraryPath"] = "C:/Users/griff/Desktop/VGM Library/library/";
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(1);
            dt.Tick += dtTicker;
            dt.Start();

            isPlayingAudio = false;
            isLooping = false;
            renameSoundtrack = false;
            viewingSongCollection = false;
            viewingPlaylists = false;
            viewingSoundtracks = false;
            addingToPlaylists = false;

            mediaPlayer = new MediaPlayer();
            playlistPage = new PlaylistPage();
            homePage = new HomePage();
            soundtrackPage = new SoundtrackPage();
            renameWindow = new Rename_Window();
            addSongWindow = new AddSong_Window();

            playlistPage.PlaylistSelected += (s, e) => EnableSongButton();
            playlistPage.PlaylistDoubleClicked += (s, e) => CheckPlaylistStatus();
            soundtrackPage.SoundtrackDoubleClicked += (s, e) => CheckSoundtrackStatus();
            renameWindow.RenameSelected += (s, e) => Rename();
            addSongWindow.SongAdded += (s, e) => SongAdded();

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
                        audioPositionSlider.Value = 0; // Reset slider and label values back to zero.
                        mediaPlayer.Position = TimeSpan.Zero;
                        audioPositionLabel.Content = $"{mediaPlayer.Position.ToString(@"hh\:mm\:ss")} / {currentlyPlayingMusicList[currentPlayingIndex].Duration}";
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
                loopButton.Foreground = Brushes.Gray;
                isLooping = false;
            }
            else // Disables looping
            {
                loopButton.Foreground = Brushes.AliceBlue;
                isLooping = true;
            }
        }

        private void ViewSongCollection()
        {
            var libraryPath = App.Current.Properties["libraryPath"].ToString();
            if (libraryPath.Length <= 0)
            {
                MessageBox.Show("Error getting library path");
                return;
            }

            CC.Content = null;
            currentlyViewingMusicList.Clear();
            musicListView.Items.Refresh();
            string filePath = libraryPath + "Song Collection/";
            FileInfo[] files = new DirectoryInfo(filePath) // Gets songs in date added order
                        .GetFiles("*.mp3")
                        .OrderBy(f => f.CreationTime)
                        .ToArray();

            foreach (var directoryPath in files)
            {
                try
                {
                    string songInfo = directoryPath.Name.ToString();
                    int titleIndex = songInfo.IndexOf('-') - 1;
                    string title = songInfo.Substring(0, titleIndex);
                    int soundtrackIndex = songInfo.IndexOf('-', titleIndex + 2);
                    string soundtrack = songInfo.Substring(titleIndex + 3, soundtrackIndex - titleIndex - 4);
                    string duration = songInfo.Substring(songInfo.LastIndexOf('-') + 2, 8).Replace('.', ':');

                    currentlyViewingMusicList.Add(new MusicList { Filename = $"{title} - {soundtrack} - {duration.Replace(':', '.')}.mp3", Title = title, Soundtrack = soundtrack, Duration = TimeSpan.Parse(duration) });
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
            }
            addPlaylistButton.IsEnabled = false;
            musicListView.ItemsSource = currentlyViewingMusicList;
            musicListView.Items.Refresh();
        }


        // Check all songs in selected library.
        public void CheckPlaylistStatus()
        {
            if (addingToPlaylists)
                AddToPlaylist();
            else
            {
                addSongButton.IsEnabled = true;
                var libraryPath = App.Current.Properties["libraryPath"].ToString();

                CC.Content = null;
                currentlyViewingMusicList.Clear();
                musicListView.Items.Refresh();
                string filePath = libraryPath + "playlists/" + PlaylistPage().libraryListView.SelectedValue.ToString() + ".xml";
                XDocument document = XDocument.Load(filePath);

                foreach (XElement xe in document.Descendants("Songs").Descendants("Path"))
                {
                    try
                    {
                        string songPath = xe.Value;

                        FileInfo file = new FileInfo(libraryPath + "/Song Collection/" + songPath);
                        string songInfo = file.Name.ToString();
                        int titleIndex = songInfo.IndexOf('-') - 1;
                        string title = songInfo.Substring(0, titleIndex);
                        int soundtrackIndex = songInfo.IndexOf('-', titleIndex + 2);
                        string soundtrack = songInfo.Substring(titleIndex + 3, soundtrackIndex - titleIndex - 4);
                        string duration = songInfo.Substring(songInfo.LastIndexOf('-') + 2, 8).Replace('.', ':');

                        currentlyViewingMusicList.Add(new MusicList { Filename = $"{title} - {soundtrack} - {duration.Replace(':', '.')}.mp3", Title = title, Soundtrack = soundtrack, Duration = TimeSpan.Parse(duration) });

                    }
                    catch
                    {
                        MessageBox.Show("Error Checking Song Status");
                    }
                }
                addPlaylistButton.IsEnabled = false;
                musicListView.ItemsSource = currentlyViewingMusicList;
                musicListView.Items.Refresh();
            }
        }

        // Check all songs in selected soundtrack.
        public void CheckSoundtrackStatus()
        {
            addSongButton.IsEnabled = true;
            var libraryPath = App.Current.Properties["libraryPath"].ToString();

            CC.Content = null;
            currentlyViewingMusicList.Clear();
            musicListView.Items.Refresh();
            string filePath = libraryPath + "soundtracks/" + SoundtrackPage().SoundtrackListView.SelectedValue.ToString() + "/soundtrack.xml";
            XDocument document = XDocument.Load(filePath);

            foreach (XElement xe in document.Descendants("Songs").Descendants("Path"))
            {
                try
                {
                    string songPath = xe.Value;

                    FileInfo file = new FileInfo(libraryPath + "/Song Collection/" + songPath);
                    string songInfo = file.Name.ToString();
                    int titleIndex = songInfo.IndexOf('-') - 1;
                    string title = songInfo.Substring(0, titleIndex);
                    int soundtrackIndex = songInfo.IndexOf('-', titleIndex + 2);
                    string soundtrack = songInfo.Substring(titleIndex + 3, soundtrackIndex - titleIndex - 4);
                    string duration = songInfo.Substring(songInfo.LastIndexOf('-') + 2, 8).Replace('.', ':');

                    currentlyViewingMusicList.Add(new MusicList { Filename = $"{title} - {soundtrack} - {duration.Replace(':', '.')}.mp3", Title = title, Soundtrack = soundtrack, Duration = TimeSpan.Parse(duration) });

                }
                catch
                {
                    MessageBox.Show("Error Checking Song Status");
                }
            }
            addPlaylistButton.IsEnabled = false;
            musicListView.ItemsSource = currentlyViewingMusicList;
            musicListView.Items.Refresh();
        }

        private void SkipForward()
        {
            var libraryPath = App.Current.Properties["libraryPath"].ToString();
            if (libraryPath.Length <= 0)
            {
                MessageBox.Show("Error getting library path");
                return;
            }

            if (currentPlayingIndex == currentlyPlayingMusicList.Count - 1) // If last song in list, then pause.
            {
                PauseAudio();
                audioPositionSlider.Value = 0; // Reset slider and label values back to zero.
                mediaPlayer.Position = TimeSpan.Zero;
                audioPositionLabel.Content = $"{mediaPlayer.Position.ToString(@"hh\:mm\:ss")} / {currentlyPlayingMusicList[currentPlayingIndex].Duration}";
                return;
            }

            currentPlayingIndex++;
            var mediaFile = new Uri(libraryPath + "Song Collection/" + currentlyPlayingMusicList[currentPlayingIndex].Filename);
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
            var libraryPath = App.Current.Properties["libraryPath"].ToString();
            if (libraryPath.Length <= 0)
            {
                MessageBox.Show("Error getting library path");
                return;
            }

            if (currentPlayingIndex == 0) // If the first item, then doesn't accept to go negative.
            {
                PauseAudio();
                audioPositionSlider.Value = 0; // Reset slider and label values back to zero.
                mediaPlayer.Position = TimeSpan.Zero;
                audioPositionLabel.Content = $"{mediaPlayer.Position.ToString(@"hh\:mm\:ss")} / {currentlyPlayingMusicList[currentPlayingIndex].Duration}";
                return;
            }

            currentPlayingIndex--;
            var mediaFile = new Uri(libraryPath + "Song Collection/" + currentlyPlayingMusicList[currentPlayingIndex].Filename);
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
            var libraryPath = App.Current.Properties["libraryPath"].ToString();
            if (libraryPath.Length <= 0)
            {
                MessageBox.Show("Error getting library path");
                return;
            }

            if (musicListView.Items.Count != 0 && musicListView.SelectedItem != null)
            {
                if (currentPlayingLabel.Content.ToString() != currentlyViewingMusicList[musicListView.SelectedIndex].Title)
                {
                    currentlyPlayingMusicList.Clear();
                    currentlyPlayingMusicList = new List<MusicList>(musicListView.Items.Cast<MusicList>().ToList());
                    currentPlayingIndex = musicListView.SelectedIndex;
                    currentPlayingLabel.Content = currentlyPlayingMusicList[currentPlayingIndex].Title;
                    var mediaFile = new Uri(libraryPath + "Song Collection/" + currentlyPlayingMusicList[currentPlayingIndex].Filename);
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
        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (volumeSlider.IsVisible)
            {
                volumeSlider.Visibility = System.Windows.Visibility.Collapsed;
                volumeButton.Foreground = Brushes.Gray;
            }
            else
            {
                volumeSlider.Visibility = System.Windows.Visibility.Visible;
                volumeButton.Foreground = Brushes.AliceBlue;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBar.Text= string.Empty;
            if (SearchBar.Visibility == System.Windows.Visibility.Visible)
                SearchBar.Visibility= System.Windows.Visibility.Hidden;
            else
                SearchBar.Visibility = System.Windows.Visibility.Visible;
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

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Settings.Default["volume"] = volumeSlider.Value.ToString();
            Settings.Default.Save();

            mediaPlayer.Volume = volumeSlider.Value;
        }

        // For renaming a music library
        private void AddSongButton_Click(object sender, RoutedEventArgs e)
        {
            addSongWindow.ShowDialog();
            SearchBar.Visibility = System.Windows.Visibility.Hidden;
        }

        // For renaming a music library
        private void AddPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            var newNameWin = new CreatePlaylist_Window();
            newNameWin.Owner = this;
            newNameWin.ShowDialog();
            SearchBar.Visibility = System.Windows.Visibility.Hidden;
        }

        // For searching music library
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBar.Text.ToLowerInvariant();
            if (viewingSoundtracks)
            {
                var filteredSoundtrackList = SoundtrackPage().SoundtrackList().Where(m =>
                    m.Name.ToLowerInvariant().Contains(searchText));
                soundtrackPage.SoundtrackListView.ItemsSource = filteredSoundtrackList;
            }

                // Filter the music items based on the search text
                var filteredList = currentlyViewingMusicList.Where(m =>
                    m.Title.ToLowerInvariant().Contains(searchText) ||
                    m.Soundtrack.ToLowerInvariant().Contains(searchText)).ToList();

                musicListView.ItemsSource = filteredList;
                musicListView.Items.Refresh();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            CC.Content = HomePage();
            SearchBar.Visibility = System.Windows.Visibility.Hidden;
            addSongButton.IsEnabled = false;
            addPlaylistButton.IsEnabled = false;
            viewingSongCollection = false;
            viewingSoundtracks = false;
            viewingPlaylists = false;
            addingToPlaylists = false;
        }

        // For opening playlist view
        private void PlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            CC.Content = PlaylistPage();
            SearchBar.Visibility = System.Windows.Visibility.Hidden;
            addSongButton.IsEnabled = false;
            addPlaylistButton.IsEnabled = true;
            viewingSoundtracks = false;
            viewingPlaylists = true;
            viewingSongCollection = false;
            addingToPlaylists = false;
        }

        // For opening playlist view
        private void SoundtrackButton_Click(object sender, RoutedEventArgs e)
        {
            SoundtrackPage().GetSoundtracks();
            CC.Content = SoundtrackPage();
            SearchBar.Visibility = System.Windows.Visibility.Hidden;
            addSongButton.IsEnabled = false;
            addPlaylistButton.IsEnabled = false;
            viewingSongCollection = false;
            viewingPlaylists = false;
            viewingSoundtracks = true;
            addingToPlaylists = false;
        }

        // For opening song library view
        private void AllSongButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBar.Visibility = System.Windows.Visibility.Hidden;
            viewingSoundtracks = false;
            addPlaylistButton.IsEnabled = false;
            addSongButton.IsEnabled = true;
            viewingSongCollection = true;
            viewingPlaylists = false;
            viewingSoundtracks = false;
            addingToPlaylists = false;
            ViewSongCollection();
        }

        public PlaylistPage PlaylistPage()
        {
            return this.playlistPage;
        }

        public HomePage HomePage()
        {
            return this.homePage;
        }

        public SoundtrackPage SoundtrackPage()
        { 
            return this.soundtrackPage;
        }

        public void EnableSongButton()
        {
            addSongButton.IsEnabled = true;
        }

        private void PlayMenuItem_Click(object sender, RoutedEventArgs e)
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

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void CloseApp(object sender, MouseButtonEventArgs e)
        {
            playlistPage.renameWindow.Close();
            soundtrackPage.renameWindow.Close();
            renameWindow.Close();
            addSongWindow.Close();
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

        private void PlaySongMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PlayAudio();
        }
        private void SongAdded()
        {
            if (viewingPlaylists)
            {
                var libraryPath = App.Current.Properties["libraryPath"].ToString();

                CC.Content = null;
                currentlyViewingMusicList.Clear();
                musicListView.Items.Refresh();
                string filePath = libraryPath + "playlists/" + PlaylistPage().libraryListView.SelectedValue.ToString() + ".xml";
                XDocument document = XDocument.Load(filePath);

                document.Descendants("Songs").First().Add(new XElement("Path", addSongWindow.songPath()));
                document.Save(filePath);
                CheckPlaylistStatus();
            }
            else if (viewingSongCollection)
                ViewSongCollection();
            else if (viewingSoundtracks)
                CheckSoundtrackStatus();
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
            var libraryPath = App.Current.Properties["libraryPath"].ToString();
            if (libraryPath.Length <= 0)
            {
                MessageBox.Show("Error getting library path");
                return;
            } 

            if (musicListView.SelectedIndex == -1) return;

            string songName = currentlyViewingMusicList[musicListView.SelectedIndex].Filename;
            string songPath = libraryPath + "Song Collection/" + songName;

            if (currentlyPlayingMusicList.SequenceEqual(currentlyViewingMusicList) && currentPlayingLabel.Content.ToString().Length != 0)
            {
                currentlyPlayingMusicList.RemoveAt(musicListView.SelectedIndex);
            }

            currentlyViewingMusicList.RemoveAt(musicListView.SelectedIndex);
            musicListView.Items.Refresh();
            File.Delete(songPath);

            var files = Directory.GetFiles(libraryPath + "playlists", "*.xml", SearchOption.AllDirectories);

            foreach (var xml in files)
            {
                XDocument document = XDocument.Load(xml);

                var playlistSongs = from node in document.Descendants("Songs").Descendants("Path")
                                    where node != null && node.Value == songName
                                    select node;

                playlistSongs.ToList().ForEach(x => x.Remove());
                document.Save(xml);
            }

            files = Directory.GetFiles(libraryPath + "soundtracks", "*.xml", SearchOption.AllDirectories);

            foreach (var xml in files)
            {
                XDocument document = XDocument.Load(xml);

                var soundtrackSongs = from node in document.Descendants("Songs").Descendants("Path")
                                    where node != null && node.Value == songName
                                    select node;

                soundtrackSongs.ToList().ForEach(x => x.Remove());
                document.Save(xml);
            }
        }
        private void AddToPlaylist()
        {
            var libraryPath = App.Current.Properties["libraryPath"].ToString();
            if (libraryPath.Length <= 0)
            {
                MessageBox.Show("Error getting library path");
                return;
            }

            if (musicListView.SelectedIndex == -1) return;

            string songName = currentlyViewingMusicList[musicListView.SelectedIndex].Filename;

            CC.Content = null;
            currentlyViewingMusicList.Clear();
            musicListView.Items.Refresh();
            string filePath = libraryPath + "playlists/" + PlaylistPage().libraryListView.SelectedValue.ToString() + ".xml";
            XDocument document = XDocument.Load(filePath);

            document.Descendants("Songs").First().Add(new XElement("Path", songName));
            document.Save(filePath);

            addingToPlaylists = false;

            if (viewingSongCollection)
                ViewSongCollection();
            else if (viewingPlaylists)
                CheckPlaylistStatus();
            else if (viewingSoundtracks)
                CheckSoundtrackStatus();

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
        private void RenameSongMenuItem_Click(object sender, RoutedEventArgs e)
        {
            renameSoundtrack = false;
            renameWindow.ShowDialog();
        }

        private void Rename ()
        {
            if (renameSoundtrack)
                RenameSoundtrack();
            else
                RenameSong();
        }

        private void RenameSong()
        {
            var libraryPath = App.Current.Properties["libraryPath"].ToString();
            if (libraryPath.Length <= 0)
            {
                MessageBox.Show("Error getting library path");
                return;
            }

            if (musicListView.Items.Count != 0 && musicListView.SelectedItem != null && renameWindow.GetTitle().Length > 0)
            {
                string songPath = libraryPath + "Song Collection/";
                string curSong = currentlyViewingMusicList[musicListView.SelectedIndex].Filename;
                int titleIndex = curSong.IndexOf('-') - 1;
                int soundtrackIndex = curSong.IndexOf('-', titleIndex + 2);
                string soundtrack = curSong.Substring(titleIndex + 3, soundtrackIndex - titleIndex - 4);
                string duration = curSong.Substring(curSong.LastIndexOf('-') + 2, 8).Replace('.', ':');

                // Ensure the renamed song is not already in the library
                if (System.IO.File.Exists(songPath + $"{renameWindow.GetTitle()} - {soundtrack} - {duration.Replace(':', '.')}.mp3"))
                {
                    MessageBox.Show("The song is already in the library", "Error");
                    return;
                }

                System.IO.File.Move(songPath + curSong, songPath + $"{renameWindow.GetTitle()} - {soundtrack} - {duration.Replace(':', '.')}.mp3");

                var files = Directory.GetFiles(libraryPath + "playlists", "*.xml", SearchOption.AllDirectories);

                foreach (var xml in files)
                {
                    XDocument document = XDocument.Load(xml);

                    var playlistSongs = from node in document.Descendants("Songs").Descendants("Path")
                                        where node != null && node.Value == curSong
                                        select node;

                    playlistSongs.ToList().ForEach(x => x.Value = $"{renameWindow.GetTitle()} - {soundtrack} - {duration.Replace(':', '.')}.mp3");
                    document.Save(xml);
                }

                files = Directory.GetFiles(libraryPath + "soundtracks", "*.xml", SearchOption.AllDirectories);

                foreach (var xml in files)
                {
                    XDocument document = XDocument.Load(xml);

                    var soundtrackSongs = from node in document.Descendants("Songs").Descendants("Path")
                                        where node != null && node.Value == curSong
                                        select node;

                    soundtrackSongs.ToList().ForEach(x => x.Value = $"{renameWindow.GetTitle()} - {soundtrack} - {duration.Replace(':', '.')}.mp3");
                    document.Save(xml);
                }

                if (viewingSongCollection)
                    ViewSongCollection();
                else if (viewingPlaylists)
                    CheckPlaylistStatus();
                else if (viewingSoundtracks)
                    CheckSoundtrackStatus();
            }
        }
        private void SetSoundtrackMenuItem_Click(object sender, RoutedEventArgs e)
        {
            renameSoundtrack = true;
            renameWindow.ShowDialog();
        }

        private void AddToPlaylistMenuItem_Click(object sender, RoutedEventArgs e)
        {
            addingToPlaylists = true;
            CC.Content = PlaylistPage();
        }

        private void RenameSoundtrack()
        {
            var libraryPath = App.Current.Properties["libraryPath"].ToString();
            if (libraryPath.Length <= 0)
            {
                MessageBox.Show("Error getting library path");
                return;
            }

            if (musicListView.Items.Count != 0 && musicListView.SelectedItem != null && renameWindow.GetTitle().Length > 0)
            {
                string songPath = libraryPath + "Song Collection/";
                string curSong = currentlyViewingMusicList[musicListView.SelectedIndex].Filename;
                int titleIndex = curSong.IndexOf('-') - 1;
                string title = curSong.Substring(0, titleIndex);
                string duration = curSong.Substring(curSong.LastIndexOf('-') + 2, 8).Replace('.', ':');
                System.IO.File.Move(songPath + curSong, songPath + $"{title} - {renameWindow.GetTitle()} - {duration.Replace(':', '.')}.mp3");

                var files = Directory.GetFiles(libraryPath + "playlists", "*.xml", SearchOption.AllDirectories);

                foreach (var xml in files)
                {
                    XDocument document = XDocument.Load(xml);

                    var playlistSongs = from node in document.Descendants("Songs").Descendants("Path")
                                        where node != null && node.Value == curSong
                                        select node;

                    playlistSongs.ToList().ForEach(x => x.Value = $"{title} - {renameWindow.GetTitle()} - {duration.Replace(':', '.')}.mp3");
                    document.Save(xml);
                }
                files = Directory.GetFiles(libraryPath + "soundtracks", "*.xml", SearchOption.AllDirectories);

                foreach (var xml in files)
                {
                    XDocument document = XDocument.Load(xml);

                    var soundtrackSongs = from node in document.Descendants("Songs").Descendants("Path")
                                        where node != null && node.Value == curSong
                                        select node;

                    soundtrackSongs.ToList().ForEach(x => x.Remove());
                    document.Save(xml);
                }

                if (viewingSongCollection)
                    ViewSongCollection();
                else if (viewingPlaylists)
                    CheckPlaylistStatus();
                else if (viewingSoundtracks)
                    CheckSoundtrackStatus();
            }
        }
    }
}

