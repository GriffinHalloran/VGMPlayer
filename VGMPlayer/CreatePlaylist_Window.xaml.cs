using System;
using System.IO;
using System.Windows;

namespace VGMPlayer
{
    /// <summary>
    /// Interaction logic for CreatePlaylist_Window.xaml
    /// </summary>
    public partial class CreatePlaylist_Window : Window
    {
        string selectedLibrary;

        public CreatePlaylist_Window(string winTitle) // Used for creating libraries
        {
            InitializeComponent();

            window.Title = winTitle;
        }

        private void CreateNewLibrary()
        {
            var libraryPath = App.Current.Properties["libraryPath"];
            if (libraryPath.ToString().Length <= 0)
            {
                MessageBox.Show("Error getting library path");
                return;
            }

            var libraryName = nameLabel.Text;
            if (!Directory.Exists(libraryPath + libraryName)) // Checks if library already exists with that name
            {
                PlaylistPage.libraryList.Add(libraryName);

                Directory.CreateDirectory(libraryPath + libraryName);
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
                MessageBox.Show("Library already exists with that name.", "Error");
            }
        }

        private void RenameLibrary()
        {
            if (nameLabel.Text == selectedLibrary) return;

            var libraryPath = App.Current.Properties["libraryPath"];
            if (libraryPath.ToString().Length <= 0)
            {
                MessageBox.Show("Error getting library path");
                return;
            }

            var libraryName = nameLabel.Text;
            if (!Directory.Exists(libraryPath + libraryName)) // Checks if library already exists with that name
            {
                try
                {
                    Directory.Move(libraryPath + selectedLibrary, libraryPath + libraryName); // Renames the directory.

                    PlaylistPage.libraryList[(Owner as MainWindow).PlaylistPage().libraryListView.SelectedIndex] = libraryName;
                    (Owner as MainWindow).PlaylistPage().libraryListView.Items.Refresh();
                    Application.Current.MainWindow.Focus();

                    MainWindow.currentlyViewingMusicList.Clear(); // Clears currently viewing list to ensure that errors get eliminated.
                    (Owner as MainWindow).musicListView.Items.Refresh();
                }
                catch
                {
                    MessageBox.Show("Error with renaming the library, try again.", "Error");
                }
            }
            else
            {
                MessageBox.Show("Library already exists with that name.", "Error");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Closes the window
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (nameLabel.Text.Length == 0) return; // Don't accept blank value.

            switch (window.Title)
            {
                case ("Create new Library"):
                    CreateNewLibrary();
                    break;

                case ("Rename Library"):
                    RenameLibrary();
                    break;
            }
            this.Close(); // Closes the window
        }
    }
}

