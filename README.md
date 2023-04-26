This is a WPF application for a music app, specifically for video game music. I was running into the problem where all the songs I'se saved online kept getting taken down due to copywright, so I made an app to download, store, and play the music locally. This uses NAudio to play the music, and YoutubeExplode to download the files from Youtube. The audio files are stores at the path provides in the mainWindow.xaml.cs. Below is an example of what the app looks like with a large song collection.

![Alt Text](https://raw.githubusercontent.com/GriffinHalloran/VGMPlayer/master/Song%20Collection.png)

Playlist and Soundtrack functionality is present as well, where songs in either category are stored in xml files such that there is only one copy of a mp3 file in the library. These songs can be added to, deleted from, and moved between each of these simply by right clicking them and selecting the desired behavior. Search functionality is also present, which works on both any song list and the soundtrack page. Below is an example of what the soundtrack page looks like when filled with objects.

![Alt Text](https://raw.githubusercontent.com/GriffinHalloran/VGMPlayer/master/Soundtrack%20Page.png)

Lastly, mp3 files can be added locally as well by selecting the "From Computer" option when attempting to add songs. More functionality might be added in the future, but for now all these features are finished and working as intended
