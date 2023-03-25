using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Drawing.Imaging;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using DialogResult = System.Windows.Forms.DialogResult;
using System.ComponentModel;

namespace VGMPlayer
{
    /// <summary>
    /// Interaction logic for SoundtrackPage.xaml
    /// </summary>
    public partial class SoundtrackPage : UserControl
    {
        public static List<Soundtrack> soundtrackList = new List<Soundtrack>();
        public event EventHandler SoundtrackDoubleClicked;
        public SoundtrackPage()
        {
            InitializeComponent();
            var libraryPath = App.Current.Properties["libraryPath"];
            if (libraryPath.ToString().Length <= 0)
            {
                MessageBox.Show("Error getting library path");
                return;
            }

            if (!Directory.Exists(libraryPath.ToString() + "/soundtracks"))
            {
                Directory.CreateDirectory(libraryPath.ToString() + "/soundtracks");
            }

            GetSoundtracks();
        }

        public void GetSoundtracks()
        {
            SoundtrackListView.ItemsSource = null;
            soundtrackList.Clear();
            var libraryPath = App.Current.Properties["libraryPath"];
            string filePath = libraryPath + "Song Collection";
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

                    Soundtrack soundtrackItem = new Soundtrack();
                    soundtrackItem.Name = soundtrack;

                    if (!Directory.Exists(libraryPath.ToString() + "soundtracks/" + soundtrack))
                    {
                        Directory.CreateDirectory(libraryPath.ToString() + "soundtracks/" + soundtrack);
                    }

                    if (File.Exists(libraryPath.ToString() + "soundtracks/" + soundtrack + "/Image.png"))
                    {
                        BitmapImage image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.UriSource = new Uri(libraryPath.ToString() + "soundtracks/" + soundtrack + "/Image.png");
                        image.EndInit();
                        soundtrackItem.TemplateImage = image;
                    }

                    if (!File.Exists(libraryPath.ToString() + "soundtracks/" + soundtrack + "/" + songInfo))
                    {
                            System.IO.File.Copy(libraryPath + "Song Collection/" + songInfo, libraryPath + "soundtracks/" + soundtrack + "/" + songInfo);
                    }
                    soundtrackList.Add(soundtrackItem);
                }

                catch (Exception) // Happens if file name doesn't include duration of song.
                {
                    Mp3FileReader reader = new Mp3FileReader($"{filePath}/{directoryPath}");
                    string title = directoryPath.Name.ToString().Remove(directoryPath.Name.ToString().Length - 4);
                    TimeSpan duration = TimeSpan.Parse(reader.TotalTime.ToString(@"hh\:mm\:ss"));
                    reader.Dispose();
                    File.Move($"{filePath}/{directoryPath}", $"{filePath}/{title} - {duration.ToString().Replace(':', '.')}.mp3");
                }
            }
            if (soundtrackList.Count > 0)
            {
                SoundtrackListView.ItemsSource = soundtrackList;
                SoundtrackListView.Items.Refresh();
                Application.Current.MainWindow.Focus();
            }
        }

        // Checks songs in new selected library and updates them into currently viewing list.
        private void SoundtrackListView_DoubleClicked(object sender, MouseButtonEventArgs e)
        {
            SoundtrackDoubleClicked?.Invoke(this, EventArgs.Empty);
        }

        private void RenameSoundtrack_Click(object sender, RoutedEventArgs e)
        {
        }
        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog sfd = new OpenFileDialog();
            sfd.Filter = "Images|*.png;*.bmp;*.jpg";
            ImageFormat format = ImageFormat.Png;
            sfd.ShowDialog();

            var libraryPath = App.Current.Properties["libraryPath"];
            System.IO.File.Copy(sfd.FileName, libraryPath + "soundtracks/" + SoundtrackListView.SelectedValue.ToString() + "/Image.png", true);

            GetSoundtracks();
        }

        private void SoundtrackListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
