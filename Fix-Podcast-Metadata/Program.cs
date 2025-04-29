// See https://aka.ms/new-console-template for more information

using TagLib;
using File = TagLib.File;

if (Environment.GetCommandLineArgs().Length < 2)
{
    Console.WriteLine("No watchpath set, please add the path to be watched as first argument");
    Console.ReadKey();
    return;
}

var timer = 30;
if (Environment.GetCommandLineArgs().Length == 3)
    timer = Convert.ToInt32(Environment.GetCommandLineArgs()[2]);

var watchingPath = Environment.GetCommandLineArgs()[1];
Console.WriteLine($"Watching path: {watchingPath}");
Console.WriteLine($"Timer: {timer} minutes");

while (true)
{
    Console.WriteLine($"Checking for files with missing metadata");

    var podcasts = Directory.GetDirectories(watchingPath + @"\Downloaded");
    foreach (var podcast in podcasts)
    {
        var files = Directory.GetFiles(podcast);
        files.ToList().Sort();
        uint track = 1;
        foreach (var file in files)
        {
            try
            {
                var fileName = Path.GetFileName(file);
                var fileSplitArr = fileName.Split("_");

                if (fileSplitArr.Length > 1)
                {
                    var title = fileSplitArr[1];
                    var dateArr = fileSplitArr[0].Split("-");
                    var year = Convert.ToInt32(dateArr[0]);
                    var month = Convert.ToInt32(dateArr[1]);
                    var day = Convert.ToInt32(dateArr[2]);
                    var parentPath = Path.GetDirectoryName(file);
                    var album = Path.GetFileName(parentPath);
                    var newPath = $@"{watchingPath}\Finished\{album}";
                    var newFile = $@"{newPath}\{title}.mp3";

                    var tfile = File.Create(file);
                    if (!System.IO.File.Exists(newFile) ||
                        (
                            tfile.Tag.Title != title ||
                            tfile.Tag.Album != album ||
                            tfile.Tag.AlbumId != album ||
                            tfile.Tag.Track != track)
                        )
                    {
                        tfile.Tag.Title = title;

                        if (tfile.Tag.Performers.Length > 0 && tfile.Tag.AlbumArtists.Length == 0)
                            tfile.Tag.AlbumArtists = tfile.Tag.Performers;

                        if (tfile.Tag.Performers.Length == 0 && tfile.Tag.AlbumArtists.Length > 0)
                            tfile.Tag.Performers = tfile.Tag.AlbumArtists;

                        var artists = string.Join(',', tfile.Tag.AlbumArtists);
                        tfile.Tag.Album = album;
                        tfile.Tag.Track = track;
                        tfile.Tag.Year = Convert.ToUInt32(year);
                        Console.WriteLine($"Changed metadata for {album} - {title} - {track}");

                        Directory.CreateDirectory(newPath);
                        Console.WriteLine($@"Moving file from {file} to {newFile}");
                        System.IO.File.Copy(file, newFile, true);
                        System.IO.File.SetCreationTime(newFile, new DateTime(year, month, day));
                    }
                    tfile.Save();
                    tfile.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Problem with file: {file} Exception: {ex.Message}");
            }

            track++;
        }
    }


    Console.WriteLine($"Done checking, sleeping for {timer} minutes");
    Thread.Sleep(timer * 60 * 1000);
}