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
using Application = System.Windows.Forms.Application;
using System.Xml;
using System.Xml.Linq;
using System;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using Microsoft.VisualBasic.ApplicationServices;
using YoutubeExplode.Playlists;

namespace VGMPlayer
{
    /// <summary>
    /// Interaction logic for PlaylistPage.xaml
    /// </summary>
    public partial class PlaylistPage : UserControl
    {
        public static List<string> libraryList = new List<string>();
        public Rename_Window renameWindow;
        public event EventHandler PlaylistDoubleClicked;
        public event EventHandler PlaylistSelected;
        public PlaylistPage()
        {
            renameWindow = new Rename_Window();
            InitializeComponent();
            CheckLibraryStatus();
            libraryListView.ItemsSource = libraryList;
            renameWindow.RenameSelected += (s, e) => RenamePlaylist();

            var libraryPath = App.Current.Properties["libraryPath"];
            if (libraryPath.ToString().Length <= 0)
            {
                MessageBox.Show("Error getting library path");
                return;
            }

            if (!Directory.Exists(libraryPath.ToString())) // Creates a root directory for all music libraries
                Directory.CreateDirectory(libraryPath.ToString());

            if (!Directory.Exists(libraryPath.ToString() + "/Song Collection")) // Creates a directory for music files
                Directory.CreateDirectory(libraryPath.ToString() + "/Song Collection");

            if (!Directory.Exists(libraryPath.ToString() + "playlists/")) // Creates a directory for playlists
                Directory.CreateDirectory(libraryPath.ToString() + "playlists/");
        }

        // Changes the library order by one to up.
        private void MoveUpLibrary_Click(object sender, RoutedEventArgs e)
        {
            if (libraryListView.SelectedIndex <= 0) return; // Can't move first item and handles pontetial error.

            var item = libraryList[libraryListView.SelectedIndex];
            libraryList.RemoveAt(libraryListView.SelectedIndex);
            libraryList.Insert(libraryListView.SelectedIndex - 1, item);
            libraryListView.Items.Refresh(); // Refresh the list to update the UI
        }

        // Changes the library order by one to down.
        private void MoveDownLibrary_Click(object sender, RoutedEventArgs e)
        {
            if (libraryListView.SelectedIndex == libraryList.Count - 1 || libraryListView.SelectedIndex == -1) return; // Can't move last item

            var item = libraryList[libraryListView.SelectedIndex];
            libraryList.RemoveAt(libraryListView.SelectedIndex);
            libraryList.Insert(libraryListView.SelectedIndex + 1, item);
            libraryListView.Items.Refresh(); // Refresh the list to update the UI
        }

        // Checks songs in new selected library and updates them into currently viewing list.
        private void LibraryListView_DoubleClicked(object sender, MouseButtonEventArgs e)
        {
                PlaylistDoubleClicked?.Invoke(this, EventArgs.Empty);
        }

        private void RenameLibrary_Click(object sender, RoutedEventArgs e)
        {
            if (libraryListView.SelectedIndex == -1) return;
            renameWindow.ShowDialog();
        }

        // Checks songs in new selected library and updates them into currently viewing list.
        private void LibraryListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            PlaylistSelected?.Invoke(this, EventArgs.Empty);
        }

        // Check for music libraries in root library directory.
        private void CheckLibraryStatus()
        {
            var libraryPath = App.Current.Properties["libraryPath"];

            var files = Directory.GetFiles(libraryPath + "playlists", "*.xml", SearchOption.AllDirectories);

            foreach (var xml in files)
            {
                XDocument document = XDocument.Load(xml);

                string name = document.Root.Attribute("Name").Value;
                libraryList.Add(name);
            }
        }

        private void DeleteLibrary_Click(object sender, RoutedEventArgs e)
        {
            if (libraryListView.SelectedIndex == -1) return;

            string libraryPath = App.Current.Properties["libraryPath"].ToString();
            var libraryName = "playlists/" + libraryListView.SelectedValue.ToString() + ".xml";

            if (File.Exists(libraryPath.ToString() + libraryName)) // Checks if playlist xml already exists with that name
                File.Delete(libraryPath.ToString() + "playlists/" + libraryListView.SelectedValue.ToString() + ".xml"); // Deletes the directory and all the files inside it.

            libraryList.RemoveAt(libraryListView.SelectedIndex);
            libraryListView.Items.Refresh();
        }

        private void RenamePlaylist()
        {
            if (libraryListView.Items.Count != 0 && libraryListView.SelectedItem != null && renameWindow.GetTitle().Length > 0)
            {
                string libraryPath = App.Current.Properties["libraryPath"].ToString();
                var libraryName = "playlists/" + libraryListView.SelectedValue.ToString() + ".xml";
                if (File.Exists(libraryPath + "playlists/" + renameWindow.GetTitle() + ".xml"))
                {
                    MessageBox.Show("That is already a playlist name");
                    return; // Can't rename to a playlist already in the list
                }

                XDocument document = XDocument.Load(libraryPath + libraryName);

                document.Root.Attribute("Name").Value = renameWindow.GetTitle();
                document.Save(libraryPath + "playlists/" + renameWindow.GetTitle() + ".xml");

                libraryList.RemoveAt(libraryListView.SelectedIndex);
                System.IO.File.Delete(libraryPath + libraryName);

                libraryList.Add(renameWindow.GetTitle());
                libraryListView.Items.Refresh(); // Refresh the list to update the UI
            }
        }

    }
}
