using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Discord.WebSocket;
using Discord;
using Discord.Rest;

namespace SwiftyBot.Modules
{
    class Group
    {

        private string _name, _creator;
        private RestVoiceChannel _channel;
        private int _joined, _playerCap;
        private IRole _role, _creatorRole;
        private ArrayList playerList = new ArrayList();

        public Group(string name, int joined, int playerCap, IRole role, IRole creatorRole)
        {
            _name = name;
            _joined = joined;
            _playerCap = playerCap;
            _role = role;
            _creatorRole = creatorRole;
        }

        public string getName()
        {
            return _name;
        }

        public int getJoined()
        {
            return _joined;
        }

        public int getPlayerCap()
        {
            return _playerCap;
        }

        public string getCreator()
        {
            return _creator;
        }

        public void setCreator(string creator)
        {
            _creator = creator;
        }

        public string getPlayer(int i)
        {
            return (string)playerList[i];
        }

        public ArrayList getAllPlayers()
        {
            return playerList;
        }

        public string playersToString()
        {
            string s = "";
            for (int i = 0; i < playerList.Count; i++)
            {
                SocketUser player = (SocketUser)playerList[i];
                if (i == playerList.Count - 1)
                {
                    s += player.Username;
                }
                else
                {
                    s += player.Username + "\n";
                }
            }
            return s;
        }

        public void addPlayer(SocketUser name)
        {
            playerList.Add(name);
            _joined++;
        }

        public IRole getRole()
        {
            return _role;
        }

        public IRole getCreatorRole()
        {
            return _creatorRole;
        }

        public void setChannel(RestVoiceChannel channel)
        {
            _channel = channel;
        }

        public RestVoiceChannel getChannel()
        {
            return _channel;
        }
    }
}
