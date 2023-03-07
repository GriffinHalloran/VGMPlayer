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
        public event EventHandler playlistClicked;
        public PlaylistPage()
        {
            InitializeComponent();
            CheckLibraryStatus();
            libraryListView.ItemsSource = libraryList;

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
        private void LibraryListView_Clicked(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Playlist clicked"); // Used only for debuggin purposes
            playlistClicked?.Invoke(this, EventArgs.Empty);
        }

        private void RenameLibrary_Click(object sender, RoutedEventArgs e)
        {
            if (libraryListView.SelectedIndex == -1) return;
        }


        private void DeleteLibraryItem()
        {
            string currentPath = "C:/Users/griff/Desktop/VGM Library";
            Directory.Delete(currentPath + "/library/" + libraryListView.SelectedValue.ToString(), true); // Deletes the directory and all the files inside it.

            libraryList.RemoveAt(libraryListView.SelectedIndex);
            libraryListView.Items.Refresh();
        }

        // Check for music libraries in root library directory.
        private void CheckLibraryStatus()
        {
            string currentPath = "C:/Users/griff/Desktop/VGM Library";
            if (!Directory.Exists(currentPath + "/library")) // Creates a root directory for all music libraries.
            {
                Directory.CreateDirectory(currentPath + "/library");
            }
            var di = new DirectoryInfo(currentPath + "/library");
            Console.WriteLine(Directory.GetCurrentDirectory()); // Used only for debuggin purposes
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
            string currentPath = "C:/Users/griff/Desktop/VGM Library";

            if (!Directory.Exists(currentPath + "/library/" + libraryListView.SelectedValue.ToString())) return; // Can't delete library that doesn't exist.


            if (Directory.GetFileSystemEntries(currentPath + "/library/" + libraryListView.SelectedValue.ToString()).Length == 0) // If no songs in the list else warning
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

    }
}
