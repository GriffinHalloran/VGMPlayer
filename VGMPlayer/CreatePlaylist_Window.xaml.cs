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

        public CreatePlaylist_Window(string winTitle, string selectedLibrary) // Used for renaming libraries
        {
            InitializeComponent();

            window.Title = winTitle;
            this.selectedLibrary = selectedLibrary;
        }

        private void CreateNewLibrary()
        {
            var libraryName = nameLabel.Text;
            string currentPath = "C:/Users/griff/Desktop/VGM Library"; ;
            if (!Directory.Exists(currentPath + "/library/" + libraryName)) // Checks if library already exists with that name
            {
                PlaylistPage.libraryList.Add(libraryName);

                Directory.CreateDirectory(currentPath + "/library/" + libraryName);
                (this.Owner as MainWindow).PlaylistPage().libraryListView.Items.Refresh();
                Application.Current.MainWindow.Focus();


                if ((this.Owner as MainWindow).PlaylistPage().libraryListView.SelectedValue == null) return;

                if (libraryName == (this.Owner as MainWindow).PlaylistPage().libraryListView.SelectedValue.ToString())
                {
                    (this.Owner as MainWindow).musicListView.Items.Clear();
                    (this.Owner as MainWindow).musicListView.Items.Refresh();
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

            var libraryName = nameLabel.Text;
            string currentPath = "C:/Users/griff/Desktop/VGM Library";
            if (!Directory.Exists(currentPath + "/library/" + libraryName)) // Checks if library already exists with that name
            {
                try
                {
                    Directory.Move(currentPath + "/library/" + selectedLibrary, currentPath + "/library/" + libraryName); // Renames the directory.

                    PlaylistPage.libraryList[(this.Owner as MainWindow).PlaylistPage().libraryListView.SelectedIndex] = libraryName;
                    (this.Owner as MainWindow).PlaylistPage().libraryListView.Items.Refresh();
                    Application.Current.MainWindow.Focus();

                    MainWindow.currentlyViewingMusicList.Clear(); // Clears currently viewing list to ensure that errors get eliminated.
                    (this.Owner as MainWindow).musicListView.Items.Refresh();
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

