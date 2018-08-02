using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SwiftyBot.Modules
{
    class SavePlaylist
    {

        static ArrayList saveObjs = new ArrayList();

        static string path = @"C:\Users\alexa\source\repos\SwiftyBot\SwiftyBot\Modules\Saves\playlists.txt";

        public SavePlaylist(Playlist save)
        {
            store(save);
        }

        public static void store(Playlist variable)
        {
            saveObjs.Add(variable);
        }

        public static void save(Playlist playlist)
        {
            string name = playlist.getName();
            for(int i = 0; i < saveObjs.Count; i++)
            {
                Playlist oldPlaylist = (Playlist)saveObjs[i];
                if(name.Equals(oldPlaylist.getName()))
                {
                    saveObjs[i] = playlist;
                }
            }
            save();
        }

        public static object get()
        {
            return saveObjs;
        }

        public static void delete(int i)
        {
            saveObjs.RemoveAt(i);
            save();
        }

        public static void save()
        {
            using (StreamWriter writetext = new StreamWriter(path))
            {
                writetext.Flush();
                for (int i = 0; i < saveObjs.Count; i++)
                {
                    Playlist playlist = (Playlist)saveObjs[i];
                    writetext.WriteLine(playlist.ToString());
                }
            }
        }

        public static void load()
        {
            Console.WriteLine("LOADING...");
            if(File.Exists(path))
            {
                int lines = File.ReadAllLines(path).Length; ;
                using (StreamReader readtext = new StreamReader(path))
                {
                    for (int i = 0; i < lines; i++)
                    {
                        string s_playlist = readtext.ReadLine();
                        int i_endName = s_playlist.IndexOf("(");
                        string name = s_playlist.Substring(0, i_endName);

                        ArrayList songs = new ArrayList();

                        string s_songs = s_playlist.Substring(s_playlist.IndexOf("(")+1, s_playlist.IndexOf(")")- s_playlist.IndexOf("(")-1);

                        string[] s = s_songs.Split(',');
                        songs.AddRange(s);

                        store(new Playlist(name, songs));
                    }
                }
            }
        }

    }
}
