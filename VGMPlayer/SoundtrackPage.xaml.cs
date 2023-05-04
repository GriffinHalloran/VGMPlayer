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
using System.Xml.Linq;
using System.Xml;
using System.Security.Policy;

namespace VGMPlayer
{
    /// <summary>
    /// Interaction logic for SoundtrackPage.xaml
    /// </summary>
    public partial class SoundtrackPage : UserControl
    {
        public static List<Soundtrack> soundtrackList = new List<Soundtrack>();
        public event EventHandler SoundtrackDoubleClicked;
        public Rename_Window renameWindow;
        public SoundtrackPage()
        {
            InitializeComponent();

            var libraryPath = App.Current.Properties["libraryPath"];

            // Creates the soundtrack library
            if (!Directory.Exists(libraryPath.ToString() + "/soundtracks"))
                Directory.CreateDirectory(libraryPath.ToString() + "/soundtracks");

            renameWindow = new Rename_Window();
            renameWindow.RenameSelected += (s, e) => RenameSoundtrack();
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

                    // If soundtrack doesn't already exist, create it
                    if (!Directory.Exists(libraryPath.ToString() + "soundtracks/" + soundtrack))
                        Directory.CreateDirectory(libraryPath.ToString() + "soundtracks/" + soundtrack);

                    // If soundtrack image exists, set the template image to it
                    if (File.Exists(libraryPath.ToString() + "soundtracks/" + soundtrack + "/Image.png"))
                    {
                        BitmapImage image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.UriSource = new Uri(libraryPath.ToString() + "soundtracks/" + soundtrack + "/Image.png");
                        image.EndInit();
                        soundtrackItem.TemplateImage = image;
                    }

                    if (!File.Exists(libraryPath.ToString() + "soundtracks/" + soundtrack + "/soundtrack.xml"))
                    {
                        XDocument xml = new XDocument(
                                            new XDeclaration("1.0", "utf-8", "yes"),
                                            new XComment("XML from plain code"),
                                            new XElement("Soundtrack",
                                            new XAttribute("Name", soundtrack),
                                            new XElement("Songs"))
                                 );
                        xml.Descendants("Songs").First().Add(new XElement("Path", songInfo));
                        xml.Save(libraryPath.ToString() + "soundtracks/" + soundtrack + "/soundtrack.xml");
                    }
                    else
                    {
                        string xmlPath = libraryPath + "soundtracks/" + soundtrack + "/soundtrack.xml";
                        XDocument document = XDocument.Load(xmlPath);

                        var soundtrackSongs = from node in document.Descendants("Songs").Descendants("Path")
                                            where node != null && node.Value == songInfo
                                            select node;

                        if (soundtrackSongs.ToList().Count == 0)
                        {
                            document.Descendants("Songs").First().Add(new XElement("Path", songInfo));
                            document.Save(libraryPath.ToString() + "soundtracks/" + soundtrack + "/soundtrack.xml");
                        }

                    }
                    bool soundtrackFound = false;
                    foreach( var soundtrackItem1 in soundtrackList)
                    {
                        if (soundtrackItem1.Name == soundtrack)
                        {
                            soundtrackFound = true;
                        }
                    }
                    if (!soundtrackFound)
                        soundtrackList.Add(soundtrackItem);
                }

                catch (Exception) // Happens if file name doesn't include duration of song.
                {
                    Mp3FileReader reader = new Mp3FileReader($"{directoryPath}");
                    string title = directoryPath.Name.ToString().Remove(directoryPath.Name.ToString().Length - 4);
                    TimeSpan duration = TimeSpan.Parse(reader.TotalTime.ToString(@"hh\:mm\:ss"));
                    reader.Dispose();
                    File.Move($"{directoryPath}", $"{filePath}/{title} - {duration.ToString().Replace(':', '.')}.mp3");
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
            if (SoundtrackListView.SelectedIndex == -1) return;
            renameWindow.ShowDialog();
        }

        private void RenameSoundtrack()
        {
            var libraryPath = App.Current.Properties["libraryPath"];
            string filePath = libraryPath + "soundtracks/" + SoundtrackListView.SelectedValue.ToString() + "/soundtrack.xml";
            XDocument document = XDocument.Load(filePath);

            foreach (var path in document.Descendants("Songs").Descendants("Path"))
            {
                string songPath = path.Value;
                int titleIndex = songPath.IndexOf('-') - 1;
                string title = songPath.Substring(0, titleIndex);
                string duration = songPath.Substring(songPath.LastIndexOf('-') + 2, 8).Replace('.', ':');
                System.IO.File.Move(libraryPath + "/Song Collection/" + songPath, libraryPath + "/Song Collection/" + $"{title} - {renameWindow.GetTitle()} - {duration.Replace(':', '.')}.mp3");
            }

            if (!Directory.Exists(libraryPath.ToString() + "soundtracks/" + renameWindow.GetTitle()))
            {
                Directory.CreateDirectory(libraryPath.ToString() + "soundtracks/" + renameWindow.GetTitle());
            }

            if (File.Exists(libraryPath.ToString() + "soundtracks/" + SoundtrackListView.SelectedValue.ToString() + "/Image.png"))
            {
                System.IO.File.Copy(libraryPath + "soundtracks/" + SoundtrackListView.SelectedValue.ToString() + "/Image.png", libraryPath.ToString() + "soundtracks/" + renameWindow.GetTitle() + "/Image.png", true);
            }


            GetSoundtracks();
        }

        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog sfd = new OpenFileDialog();
            sfd.Filter = "Images|*.png;*.bmp;*.jpg";
            ImageFormat format = ImageFormat.Png;
            bool? result = sfd.ShowDialog();
            if (result == true)
            {
                var libraryPath = App.Current.Properties["libraryPath"];
                System.IO.File.Copy(sfd.FileName, libraryPath + "soundtracks/" + SoundtrackListView.SelectedValue.ToString() + "/Image.png", true);
            }

            GetSoundtracks();
        }

        public List<Soundtrack> SoundtrackList()
        {
            return soundtrackList;
        }
    }
}
