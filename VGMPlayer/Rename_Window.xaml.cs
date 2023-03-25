using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace VGMPlayer
{
    /// <summary>
    /// Interaction logic for AddSong_Window.xaml
    /// </summary>
    public partial class Rename_Window : Window
    {
        public event EventHandler RenameSelected;
        private string rename;
        public Rename_Window()
        {
            InitializeComponent();
        }

        private void RenameCloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide(); // Closes the window
        }

        public string GetTitle()
        {
            return this.rename; // Closes the window
        }

        private async void RenameOkButton_Click(object sender, RoutedEventArgs e)
        {
            var libraryName = songTitle.Text;
            if (libraryName.Length == 0) return; // Dont accept null values

            this.rename= libraryName;
            RenameSelected?.Invoke(libraryName, EventArgs.Empty);
            songTitle.Text = "";
            this.Hide();
        }
    }
}