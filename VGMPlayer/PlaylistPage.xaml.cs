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

        private void DeleteLibraryItem()
        {
            if (libraryListView.SelectedValue.ToString() == "Song Collection")
            {
                MessageBox.Show("Cannot Delete Main Library.", "Error");
                return;
            }
            var libraryPath = App.Current.Properties["libraryPath"];
            if (libraryPath.ToString().Length <= 0)
            {
                MessageBox.Show("Error getting library path");
                return;
            }

            Directory.Delete(libraryPath.ToString() + libraryListView.SelectedValue.ToString(), true); // Deletes the directory and all the files inside it.

            libraryList.RemoveAt(libraryListView.SelectedIndex);
            libraryListView.Items.Refresh();
        }

        // Check for music libraries in root library directory.
        private void CheckLibraryStatus()
        {
            var libraryPath = App.Current.Properties["libraryPath"];
            if (libraryPath.ToString().Length <= 0)
            {
                MessageBox.Show("Error getting library path");
                return;
            }

            if (!Directory.Exists(libraryPath.ToString())) // Creates a root directory for all music libraries.
            {
                Directory.CreateDirectory(libraryPath.ToString());
            }
            if (!Directory.Exists(libraryPath.ToString() + "/Song Collection")) // Creates a root directory for all music libraries.
            {
                Directory.CreateDirectory(libraryPath.ToString() + "/Song Collection");
            }
            var di = new DirectoryInfo(libraryPath.ToString());
            var directories = di.EnumerateDirectories() // Gets directories in date added order
                                .OrderBy(d => d.CreationTime)
                                .Select(d => d.Name)
                                .ToList();
            foreach (var dir in directories)
            {
                libraryList.Add(dir);
            }
        }

        private void DeleteLibrary_Click(object sender, RoutedEventArgs e)
        {
            if (libraryListView.SelectedIndex == -1) return;

            var libraryPath = App.Current.Properties["libraryPath"];
            if (libraryPath.ToString().Length <= 0)
            {
                MessageBox.Show("Error getting library path");
                return;
            }
            string currentPath = libraryPath.ToString();

            if (!Directory.Exists(libraryPath + libraryListView.SelectedValue.ToString())) return; // Can't delete library that doesn't exist.


            if (Directory.GetFileSystemEntries(libraryPath + libraryListView.SelectedValue.ToString()).Length == 0) // If no songs in the list else warning
            {
                DeleteLibraryItem();
            }
            else
            {
                if (MessageBox.Show("Are you sure you want to delete the library?", "Media Player", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return;
                }
                else
                {
                    try
                    {
                        DeleteLibraryItem();
                    }
                    catch (InvalidOperationException)
                    {
                        MessageBox.Show("Error with deleting the library, try again.", "Error");
                    }
                }
            }
        }

        private void RenamePlaylist()
        {
            if (libraryListView.Items.Count != 0 && libraryListView.SelectedItem != null && renameWindow.GetTitle().Length > 0)
            {
                var libraryPath = App.Current.Properties["libraryPath"];
                if (libraryPath.ToString().Length <= 0)
                {
                    MessageBox.Show("Error getting library path");
                    return;
                }

                if (Directory.Exists(libraryPath.ToString() + renameWindow.GetTitle()))
                {
                    MessageBox.Show("That is already a playlist name");
                    return; // Can't rename to a playlist already in the list
                }
                if (libraryListView.SelectedItem.ToString() == "Song Collection")
                {
                    MessageBox.Show("Can't Rename main library");
                    return; 
                }
                libraryList.RemoveAt(libraryListView.SelectedIndex);
                System.IO.Directory.Move(libraryPath.ToString() + libraryListView.SelectedValue.ToString(), libraryPath.ToString() + renameWindow.GetTitle());

                libraryList.Add(renameWindow.GetTitle());
                libraryListView.Items.Refresh(); // Refresh the list to update the UI
            }
        }

    }
}
