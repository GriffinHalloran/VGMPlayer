using System;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace VGMPlayer
{
    /// <summary>
    /// Interaction logic for CreatePlaylist_Window.xaml
    /// </summary>
    public partial class CreatePlaylist_Window : Window
    {
        string selectedLibrary;

        public CreatePlaylist_Window() // Used for creating libraries
        {
            InitializeComponent();
        }

        private void CreateNewLibrary()
        {
            var libraryPath = App.Current.Properties["libraryPath"];
            var libraryName = "playlists/" + nameLabel.Text + ".xml";

            if (!File.Exists(libraryPath.ToString() + libraryName)) // Checks if playlist xml already exists with that name
            {
                PlaylistPage.libraryList.Add(nameLabel.Text);

                XDocument xml = new XDocument(
                                    new XDeclaration("1.0", "utf-8", "yes"),
                                    new XComment("XML from plain code"),
                                    new XElement("Playlist",
                                    new XAttribute("Name", nameLabel.Text),
                                    new XElement("Songs"))
                         );

                xml.Save(libraryPath + libraryName);
                (Owner as MainWindow).PlaylistPage().libraryListView.Items.Refresh();
                Application.Current.MainWindow.Focus();


                if ((Owner as MainWindow).PlaylistPage().libraryListView.SelectedValue == null) return;

                if (libraryName == (Owner as MainWindow).PlaylistPage().libraryListView.SelectedValue.ToString())
                {
                    (Owner as MainWindow).musicListView.Items.Clear();
                    (Owner as MainWindow).musicListView.Items.Refresh();

                }
            }
            else
            {
                MessageBox.Show("Playlist already exists with that name.", "Error");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Closes the window
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (nameLabel.Text.Length == 0) return; // Don't accept blank value.
            CreateNewLibrary();

            this.Close(); // Closes the window
        }
    }
}

