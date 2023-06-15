
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using MusicLibrary_ConsoleApp1.Models;
using System.Text.RegularExpressions;

namespace MusicLibrary_ConsoleApp1
{
    internal class Program
    {

        MediaPlayer player = new MediaPlayer();
        MusicLibraryEntities db = new MusicLibraryEntities();
        dynamic duration;
        static void Main(string[] args)
        {
            var key = "";
            var program = new Program();
            do
            {
                Console.Clear();
                Console.WriteLine("Music Library Application");
                Console.WriteLine("1. Add new track");
                Console.WriteLine("2. See all track");
                Console.WriteLine("3. Create playlist");
                Console.WriteLine("4. See all playlist");
                Console.WriteLine("5. Find a track or playlist by Title/ Artist/ Album/ Genre ");
                Console.WriteLine();
                Console.Write("Choose action number: ");
                var keyMenu = Console.ReadLine();
                int menu = 0;
                bool canConverted = int.TryParse(keyMenu, out menu);
                if (keyMenu != null && canConverted)
                {
                    if (menu == 1)
                    {
                        program.AddTrack();
                        
                    }
                    if (menu == 2)
                    {
                        program.FindAllTrack();
                        
                    }
                    if (menu == 3)
                    {
                        program.AddPlaylist();
                        
                    }
                    if (menu == 4)
                    {
                        program.FindAllPlaylist();
                        
                    }
                    if (menu == 5)
                    {
                        program.SearchMenu();
                        
                    }
                }
                Console.Write("Press y to back to main menu ");
                key = Console.ReadLine();

            } while (key.Equals("y", StringComparison.OrdinalIgnoreCase));
        }



        //add new track method
        void AddTrack()
        {
            // input new track
            var track = new Track();
            InputTrack(track);

            // add data to database
            db.Tracks.Add(track);
            db.SaveChanges();

            // show data inserted
            Console.WriteLine();
            Console.WriteLine("New track added...");
            PrintTrack(track);
            Console.WriteLine();
        }

        // see all track in library method
        void FindAllTrack()
        {
            //print all track title
            Console.WriteLine("Total: " + db.Tracks.Count() + " track(s)");
            db.Tracks.ToList().ForEach(t =>
            {
                Console.WriteLine(t.id + ". " + t.title);

            });

            //search track in list by their Title or Genre 
            Console.WriteLine();
            Console.Write("Search by Title or Genre: ");
            string keyword = Console.ReadLine();
            Console.WriteLine();
            if (keyword != null)
            {
                db.Tracks.Where(t => t.title.Contains(keyword) || t.genre.Contains(keyword)).ToList().ForEach(t =>
                {
                    Console.WriteLine("ID: " + t.id);
                    Console.WriteLine("Title: " + t.title);
                    Console.WriteLine("Genre: " + t.genre);
                    Console.WriteLine();
                });
            }

            // select track and action menu
            var key = "";
            do
            {
                Console.WriteLine();
                Console.Write("Type track number to see track detail: ");
                var input = Console.ReadLine();
                int trackId = 0;
                bool canConverted = int.TryParse(input, out trackId);
                if (input != null && canConverted)
                {
                    Console.WriteLine();
                    SeeTrackDetails(trackId);
                }
                Console.Write("Press y to see other track details ");
                key = Console.ReadLine();
            }
            while (key.Equals("y", StringComparison.OrdinalIgnoreCase));

        }

        //track detail and action menu method
        void SeeTrackDetails(int id)
        {
            var track = db.Tracks.Find(id);
            if (track != null)
            {
                //print track detail
                PrintTrack(track);
                Console.WriteLine();
                //action menu
                Console.WriteLine("1. Update track info ");
                Console.WriteLine("2. Delete track");
                Console.WriteLine("3. Play");
                Console.WriteLine("4. Add track to playlist");
                Console.WriteLine();
                Console.Write("Choose action number: ");
                var keyMenu = Console.ReadLine();
                int menu = 0;
                bool canConverted = int.TryParse(keyMenu, out menu);
                if (keyMenu != null && canConverted)
                {
                    if (menu == 1)
                    {
                        UpdateTrackInfo(track);
                    }
                    if (menu == 2)
                    {
                        DeleteTrack(track);
                    }
                    if (menu == 3)
                    {
                        PlaySong(track);
                        Console.ReadKey();
                        Stop();
                    }
                    if (menu == 4)
                    {
                        AddTrackToPlaylist(track);
                    }
                }
            } else
            {
                Console.WriteLine("Track not found");
            }

        }

        // update track info method
        void UpdateTrackInfo(Track track)
        {
            Console.WriteLine();
            Console.WriteLine("Insert new data...");
            //input new data
            InputTrack(track);

            // update data to database
            db.Entry(track).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            // show data after user input
            Console.WriteLine();
            Console.WriteLine("Info updated...");
            Console.WriteLine();
            PrintTrack(track);
            Console.WriteLine();
        }

        // delete track method
        void DeleteTrack(Track track)
        {
            track.Playlists.Clear();
            db.Tracks.Remove(track);
            db.SaveChanges();
            Console.WriteLine("Deleted");
        }

        // play song method
        void PlaySong(Track track)
        {
            player.MediaOpened += Player_MediaOpened;
            player.MediaFailed += Player_MediaFailed;
            player.Open(new Uri(track.trackPath));
            
            //get track duration
            if (player.NaturalDuration.HasTimeSpan)
            {
                var stringDuration = player.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                track.duration = stringDuration;
                Console.WriteLine("Duration: " + track.duration);
                db.Entry(track).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

            player.Play();
            Console.WriteLine("Playing...");

            //error message if user input wrong file path or corrupted file 
            if (string.IsNullOrEmpty(track.duration))
            {
                Console.WriteLine("Cannot access file ...");
            }
        }

        private void Player_MediaOpened(object sender, EventArgs e)
        {
            if (player.NaturalDuration.HasTimeSpan)
            {
                duration = player.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
            }
        }

        private void Player_MediaFailed(object sender, ExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        //stop playing method
        void Stop()
        {
            player.Stop();
        }

        //print track info method
        static void PrintTrack(Track track)
        {
            Console.WriteLine("ID: " + track.id);
            Console.WriteLine("Title: " + track.title);
            Console.WriteLine("Artis: " + track.artist);
            Console.WriteLine("Album: " + track.album);
            Console.WriteLine("Genre: " + track.genre);
            Console.WriteLine("Year: " + track.year);
            if (track.duration != null)
            {
                Console.WriteLine("Duration: " + track.duration);
            }
            Console.WriteLine("File location: " + track.trackPath);
        }

        // input track info method
        void InputTrack(Track track)
        {
            // input track title, artist, album, genre
            Console.Write("Title: ");
            track.title = Console.ReadLine();
            Console.Write("Artist: ");
            track.artist = Console.ReadLine();
            Console.Write("Album: ");
            track.album = Console.ReadLine();
            Console.Write("Genre: ");
            track.genre = Console.ReadLine();

            //input year with validation YYYY format
            Console.Write("Year: ");
            track.year = Console.ReadLine();
            string pattern = "^(\\d{4}$)";
            while (!Regex.Match(track.year, pattern).Success)
            {
                Console.WriteLine("Year should be 4 digits, please type again");
                Console.Write("Year: ");
                track.year = Console.ReadLine();
            }

            //input media file path for playing if applicable
            Console.Write("If you have media file, input file path: ");
            track.trackPath = Console.ReadLine();


        }

        


        // add a track to a selected playlist method
        void AddTrackToPlaylist(Track track)
        {
            Console.WriteLine();
            // show all playlist
            Console.WriteLine("Total: " + db.Playlists.Count() + " playlist(s)");
            db.Playlists.ToList().ForEach(p =>
            {
                Console.WriteLine(p.id + ". " + p.title);
            });
            Console.WriteLine();

            // select playlist to add
            Console.Write("Choose playlist# to be added: ");

            var keyword = Console.ReadLine();
            int id = 0;
            bool canConverted = int.TryParse(keyword, out id);
            var playlist = db.Playlists.Find(id);
            //check if existed playlist is select and add track to the playlist
            if (playlist != null)
            {
                track.Playlists.Add(playlist);
                db.Entry(track).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                Console.WriteLine("Added...");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Playlist does not exist. Cancelled.");
            }

        }

        // create new playlist method
        void AddPlaylist()
        {
            Console.WriteLine();
            Console.WriteLine("Add new playlist info: ");
            var playlist = new Playlist();
            InputPlaylist(playlist);
            db.Playlists.Add(playlist);
            db.SaveChanges();
            Console.WriteLine();
            Console.WriteLine("Playlist created...");
            PrintPlaylist(playlist);
            Console.WriteLine();

        }

        // show all playlist method
        void FindAllPlaylist()
        {
            Console.WriteLine();
            //show list of all playlist
            Console.WriteLine("Total: " + db.Playlists.Count() + " playlist(s)");
            db.Playlists.ToList().ForEach(p =>
            {
                PrintPlaylist(p);
            });
            Console.WriteLine();

            //search playlist in list by their Title 
            Console.Write("Search by Title: ");
            string keyword = Console.ReadLine();
            Console.WriteLine();
            if (keyword != null)
            {
                db.Playlists.Where(t => t.title.Contains(keyword)).ToList().ForEach(t =>
                {
                    PrintPlaylist(t);
                });
            }
            Console.WriteLine();
            //select playlist and action menu
            var key = "";
            do
            {
                Console.Write("Type playlist number to see playlist detail: ");
                var input = Console.ReadLine();
                Console.WriteLine();
                int playlistId = 0;
                bool canConverted = int.TryParse(input, out playlistId);
                if (input != null && canConverted)
                {
                    SeePlaylistDetails(playlistId);
                }
                Console.Write("Press y to see other playlist details ");
                key = Console.ReadLine();
            }
            while (key.Equals("y", StringComparison.OrdinalIgnoreCase));
        }

        // playlist detail and action menu method
        void SeePlaylistDetails(int id)
        {
            var playlist = db.Playlists.Find(id);
            if(playlist != null)
            {
                //show playlist detail
                PrintPlaylist(playlist);
                //action menu
                Console.WriteLine();
                Console.WriteLine("1. Update playlist info ");
                Console.WriteLine("2. Delete playlist");
                Console.WriteLine("3. See all track in playlist");
                Console.WriteLine();
                Console.Write("Choose action number: ");
                var keyMenu = Console.ReadLine();
                int menu = 0;
                bool canConverted = int.TryParse(keyMenu, out menu);
                if (keyMenu != null && canConverted)
                {
                    if (menu == 1)
                    {
                        UpdatePlaylistInfo(playlist);
                    }
                    if (menu == 2)
                    {
                        DeletePlaylist(playlist);
                    }
                    if (menu == 3)
                    {
                        FindAllTrackInPlaylist(playlist);
                    }
                }
            }
        }

        //insert data for playlist method
        void InputPlaylist(Playlist playlist)
        {
            // input playlist title
            Console.Write("Title: ");
            playlist.title = Console.ReadLine();

        }

        // print playlist data method
        void PrintPlaylist(Playlist playlist)
        {
            Console.WriteLine(playlist.id + ". " + playlist.title);
        }

        //update playlist method
        void UpdatePlaylistInfo(Playlist playlist)
        {
            //insert new data for playlist 
            InputPlaylist(playlist);

            // update data to database
            db.Entry(playlist).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            // show data after user input
            Console.WriteLine();
            Console.WriteLine("Info updated...");
            PrintPlaylist(playlist);
            Console.WriteLine();
        }

        //delete playlist method
        void DeletePlaylist(Playlist playlist)
        {
            playlist.Tracks.Clear();
            db.Playlists.Remove(playlist);
            db.SaveChanges();
            Console.WriteLine("Deleted");
        }

        // show list of track in a selected playlist and action menu method
        void FindAllTrackInPlaylist(Playlist playlist)
        {
            Console.WriteLine();
            Console.WriteLine("Total: " + playlist.Tracks.Count() + " track(s)");
            playlist.Tracks.ToList().ForEach(t =>
            {
                Console.WriteLine(t.id + ". " + t.title);

            });

            var key = "";
            do
            {
                Console.WriteLine();
                Console.WriteLine("1. Add track to playlist");
                Console.WriteLine("2. Remove track from playlist");
                Console.Write("Choose action for playlist: ");
                var keyMenu = Console.ReadLine();
                int menu = 0;
                bool canConverted = int.TryParse(keyMenu, out menu);
                if (keyMenu != null && canConverted)
                {
                    if (menu == 1)
                    {
                        ImportTrackToPlaylist(playlist);
                    }
                    if (menu == 2)
                    {
                        RemoveTrackPlaylist(playlist);
                    }

                    Console.Write("Press y to add/ remove other track ");
                    key = Console.ReadLine();
                }
                else
                {
                    Console.Write("Press y to add/ remove other track ");
                    key = Console.ReadLine();
                }

            }
            while (key.Equals("y", StringComparison.OrdinalIgnoreCase));

        }

        //add track to selected playlist method
        void ImportTrackToPlaylist(Playlist playlist)
        {
            Console.Write("Input track# to be added: ");
            string keyword = Console.ReadLine();
            int id = 0;
            bool canConverted = int.TryParse(keyword, out id);
            var track = db.Tracks.Find(id);
            if (track != null)
            {
                playlist.Tracks.Add(track);
                db.Entry(playlist).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                Console.WriteLine("Added...");
                playlist.Tracks.ToList().ForEach(t =>
                {
                    Console.WriteLine(t.id + ". " + t.title);

                });
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("No track found...");
            }
        }

        //remove track from selected playlist method
        void RemoveTrackPlaylist(Playlist playlist)
        {
            Console.Write("Input track# to be deleted: ");
            string keyword = Console.ReadLine();
            int id = 0;
            bool canConverted = int.TryParse(keyword, out id);
            var track = db.Tracks.Find(id);
            if (track != null)
            {
                playlist.Tracks.Remove(track);
                db.Entry(playlist).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                Console.WriteLine("Deleted...");
            }
            else
            {
                Console.WriteLine("No track found...");
            }
        }

        //search for track/ playlist by playlist title, track title, track artist, track album, track genre 
        void SearchMenu()
        {
            Console.Write("Insert keyword: ");
            var keyword = Console.ReadLine().ToLower();
            var dataTrack = db.Tracks.Where(t => t.title.Contains(keyword) || t.artist.Contains(keyword) || t.album.Contains(keyword) || t.genre.Contains(keyword));
            var dataPlaylist = db.Playlists.Where(p => p.title.Contains(keyword));
            


            Console.WriteLine("Result: ");
            foreach (var item in dataTrack)
            {
                Console.WriteLine("Track");
                Console.WriteLine("Id: " + item.id);
                Console.WriteLine("Title: " + item.title);
                Console.WriteLine("Artist: " + item.artist);
                Console.WriteLine("Album: " + item.album);
                Console.WriteLine("Genre: " + item.genre);
                Console.WriteLine();
            }
            foreach (var item in dataPlaylist)
            {
                Console.WriteLine("Playlist Id: " + item.id);
                Console.WriteLine("Playlist Title: " + item.title);
                Console.WriteLine();
            }


        }
    }
}
