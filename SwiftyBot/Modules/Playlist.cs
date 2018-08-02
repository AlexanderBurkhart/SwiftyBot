using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SwiftyBot.Modules
{
    class Playlist
    {
        private string _name;
        private ArrayList _songs = new ArrayList();

        public Playlist(string name)
        {
            _name = name;
            //_songs.Add("sample");
            SavePlaylist.save(this);
        }

        public Playlist(string name, ArrayList songs)
        {
            _name = name;
            _songs = songs;
            //SavePlaylist.save(this);
        }

        public void addSong(string song)
        {
            _songs.Add(song);
            SavePlaylist.save(this);
        }

        public void deleteSong(string song)
        {
            _songs.Remove(song);
            SavePlaylist.save(this);
        }
        public void deleteSong(int n)
        {
            _songs.RemoveAt(n);
            SavePlaylist.save(this);
        }

        public Boolean containsSong(string check)
        {
            foreach(string song in _songs)
            {
                if(song.Equals(check))
                {
                    return true;
                }
            }
            return false;
        }

        public string getName()
        {
            return _name;
        }

        public ArrayList getSongs()
        {
            return _songs;
        }

        public string ToString()
        {
            string s = getName() + "(";

            for(int i = 0; i < _songs.Count; i++)
            {
                s += _songs[i];
                if (i != _songs.Count - 1)
                    s += ",";
            }
            s += ")";

            return s;
        }

    }
}

