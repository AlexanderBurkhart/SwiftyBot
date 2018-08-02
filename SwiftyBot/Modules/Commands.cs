using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.Rest;
using Discord.Rpc;
using Discord.Webhook;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using System.Linq;

namespace SwiftyBot.Modules
{

    public class Commands : ModuleBase<SocketCommandContext>
    {
        //command vars
        int joined = 0;
        int playerCap = 0;
        string[] names;
        ArrayList groups = new ArrayList();
        Random r = new Random();


        public Commands()
        {
            SavePlaylist.save();
            names = readInNames();
            Console.Write(names.Length);
        }

        public string[] readInNames()
        {
            //read in names from file
            return System.IO.File.ReadAllLines(@"C:\Users\alexa\source\repos\SwiftyBot\SwiftyBot\Names\names.txt");
        }

        [Command("test")]
        public async Task test(string name)
        {
            var user = Context.User;
            var role = await Context.Guild.CreateRoleAsync(name);
            await (user as IGuildUser).AddRoleAsync(role);
        }

        [Command("lfg")]
        public async Task createChannel(string action, string groupName)
        {
            groups = (ArrayList)SaveGroup.get();

            var category = getCategory();

            //catch error
            if (category.Equals("error"))
            {
                await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Error: must be using looking-for-group-## text channel in order to execute this command");
            }
            //if no error, execute command
            //***********************
            //COMMAND TO CREATE GROUP
            //***********************
            else if (action.Equals("create", StringComparison.CurrentCultureIgnoreCase))
            {
                await Context.Channel.SendMessageAsync("Creating **" + category + "** group called **" + groupName + "**...");

                //check if group name exists
                Boolean groupExist = groupFound(groupName);

                if (groupExist == false)
                {
                    //get player cap
                    playerCap = getPlayerCap(category);
                    Console.WriteLine(playerCap);

                    //create role for group and creator
                    var creatorRole = await Context.Guild.CreateRoleAsync("-" + groupName + " Leader-");
                    var role = await Context.Guild.CreateRoleAsync("-" + groupName + "-");

                    //create group
                    Group group = new Group(groupName, joined, playerCap, role, creatorRole);
                    Console.WriteLine(group.getPlayerCap());

                    //add creator name
                    SocketUser creator = Context.Message.Author;
                    group.addPlayer(creator);
                    group.setCreator(creator.Username);

                    //add group to group list
                    groups.Add(group);
                    SaveGroup.store(groups);

                    //assign role to creator
                    var user = Context.User;
                    await (user as IGuildUser).AddRoleAsync(group.getCreatorRole());

                    //creater embed
                    var builder = new EmbedBuilder();
                    builder.WithTitle(category + " Group " + groupName);
                    builder.AddInlineField("Players Joined: ", group.getJoined() + "/" + playerCap);
                    builder.WithColor(Color.Blue);
                    await Context.Channel.SendMessageAsync("", false, builder);
                }
                else
                {
                    await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Error: Group name already exists");
                }
            }
            //*********************
            //COMMAND TO JOIN GROUP
            //*********************
            else if (action.Equals("join", StringComparison.CurrentCultureIgnoreCase))
            {
                //await Context.Channel.SendMessageAsync("joining");
                //if group exists
                if (groupFound(groupName))
                {
                    await Context.Channel.SendMessageAsync("Joining group **" + groupName + "**...");
                    //find group
                    Console.WriteLine("BEFORE");
                    int index = findGroupIndex(groupName);
                    Console.WriteLine("AFTER");
                    Console.WriteLine(index);
                    if (index >= 0)
                    {
                        //await Context.Channel.SendMessageAsync("going inside group");
                        Group group = (Group)groups[index];
                        SocketUser playerJoined = Context.Message.Author;

                        //if there is space and player hasn't joined before
                        if (group.getPlayerCap() > group.getJoined() && !hasPlayerJoined(playerJoined, group))
                        {
                            //join group  
                            group.addPlayer(playerJoined);
                            await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " You have joined the group " + group.getName() + ".");

                            //assign role to player
                            var user = Context.User;
                            await (user as IGuildUser).AddRoleAsync(group.getRole());

                            //check if all playerCap is reached
                            if (group.getPlayerCap() == group.getJoined())
                            {
                                //create embed
                                var builder = new EmbedBuilder();
                                builder.WithTitle(category + " Group " + groupName);
                                builder.AddInlineField("Player joined...", playerJoined.Username);
                                builder.AddInlineField("Players Joined: ", group.getJoined() + "/" + group.getPlayerCap());
                                builder.WithColor(Color.Blue);
                                await Context.Channel.SendMessageAsync("", false, builder);

                                //make channel and mention members to join channel
                                ArrayList players = group.getAllPlayers();
                                String mentionLine = "";
                                for (int i = 0; i < players.Count; i++)
                                {
                                    SocketUser player = (SocketUser)players[i];
                                    mentionLine += player.Mention + " ";
                                }
                                await Context.Channel.SendMessageAsync(mentionLine + "Group is now full...creating voice channel");

                                string channelName = "Group " + names[r.Next(0, names.Length - 1)] + " (" + category + ")";
                                var _channel = await Context.Guild.CreateVoiceChannelAsync(groupName + ": " + channelName);
                                await Context.Channel.SendMessageAsync(mentionLine + "Created Channel called **" + channelName + "** for **" + groupName + "**");
                            }
                            else
                            {
                                //if not reached
                                //send updated embed
                                var builder = new EmbedBuilder();
                                builder.WithTitle(category + " Group " + groupName);
                                builder.AddInlineField("Player joined...", playerJoined.Username);
                                builder.AddInlineField("Players Joined: ", group.getJoined() + "/" + group.getPlayerCap());
                                builder.WithColor(Color.Blue);
                                await Context.Channel.SendMessageAsync("", false, builder);
                            }
                            groups[index] = group;
                        }
                        //error: no space or player has joined
                        else
                        {
                            if (hasPlayerJoined(playerJoined, group))
                                await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Error: Already joined group");
                            else
                                await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Error: Group is full, no space");
                        }
                    }
                }
                else
                {
                    //no group
                    //error: can't find group
                    await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Error: Group name does not exist");
                }
            }
            //********************************
            //COMMAND TO LIST PLAYERS IN GROUP
            //********************************
            else if (action.Equals("list", StringComparison.CurrentCultureIgnoreCase))
            {
                Boolean found = false;
                int gIndex = -1;

                //find group
                for (int i = 0; i < groups.Count; i++)
                {
                    Group group = (Group)groups[i];
                    if (group.getName().Equals(groupName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        found = true;
                        gIndex = i;
                        break;
                    }
                }

                //if group does not exist
                if (!found)
                {
                    //ERROR
                    await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Error: Group named **" + groupName + "** does not exist");
                }
                //if group exists
                else
                {
                    //get group and list names
                    Group group = (Group)groups[gIndex];

                    //creater embed
                    var builder = new EmbedBuilder();
                    builder.WithTitle(category + " Group " + groupName);
                    builder.AddInlineField("Player List: ", group.playersToString());
                    builder.WithColor(Color.Blue);
                    await Context.Channel.SendMessageAsync("", false, builder);
                }

            }
            //*****************************
            //COMMAND TO FORCE FINISH GROUP
            //*****************************
            else if (action.Equals("force", StringComparison.CurrentCultureIgnoreCase))
            {
                if (groupFound(groupName))
                {
                    int index = findGroupIndex(groupName);
                    Group group = (Group)groups[index];

                    //check that the user is the creator of this group
                    if (Context.Message.Author.Username == group.getCreator())
                    {
                        //make channel and mention members to join channel
                        ArrayList players = group.getAllPlayers();
                        String mentionLine = "";
                        for (int i = 0; i < players.Count; i++)
                        {
                            SocketUser player = (SocketUser)players[i];
                            mentionLine += player.Mention + " ";
                        }
                        await Context.Channel.SendMessageAsync(mentionLine + "Forcing group creation...creating voice channel");

                        string channelName = "Group " + names[r.Next(0, names.Length - 1)] + " (" + category + ")";
                        var _channel = await Context.Guild.CreateVoiceChannelAsync(channelName);
                        group.setChannel(_channel);
                        await Context.Channel.SendMessageAsync(mentionLine + "Created Channel called **" + channelName + "** for **" + groupName + "**");
                        groups[index] = group;
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Error: Only the creator, " + group.getCreator() + ", can force start the group.");
                    }


                }
            }
            SaveGroup.store(groups);
        }


        //**********************************************************
        //Clears all auto generated objects
        //**RUN THIS COMMAND EVERY TIME WHEN ABOUT TO SHUT OFF BOT**
        //**********************************************************
        [Command("clear")]
        public async Task clear()
        {
            Console.WriteLine(Context.Message.Author.Username);
            if (Context.Message.Author.Username == "hex")
            {

                await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Clearing all objects...");

                //iterate through groups and delete everything
                ArrayList groups = (ArrayList)SaveGroup.get();
                int count = groups.Count;
                if (count != 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        Group group = (Group)groups[i];

                        //delete group role
                        IRole role = group.getRole();
                        Console.WriteLine("Role: " + role);
                        //group role can not exist
                        try
                        {
                            await role.DeleteAsync();
                        }
                        catch (Exception e)
                        {

                        }

                        //delete creator role
                        IRole creatorRole = group.getCreatorRole();
                        Console.WriteLine("Creator Role: " + creatorRole);
                        await creatorRole.DeleteAsync();

                        //delete group channel
                        RestVoiceChannel channel = group.getChannel();
                        Console.WriteLine("Channel: " + channel);
                        //channel can not exist
                        try
                        {
                            await channel.DeleteAsync();
                        }
                        catch (Exception e)
                        {

                        }

                        //Store empty group object
                        SaveGroup.store(new ArrayList());
                    }
                    await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Cleared succesfully!");
                }
                else
                {
                    await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Nothing to clear.");
                }
            }
            else
            {
                //INVALID USER
                await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Error: You are not my creator...");
            }

        }

        //**************************************************
        //JAILS SELECTED USER
        //MOVES USER TO JAILED CHANNEL IF IN ANOTHER CHANNEL
        //**************************************************
        [Command("jail")]
        public async Task jail(SocketGuildUser jailUser)
        {

            var user = Context.User as SocketGuildUser;

            //roles allowed to jail
            var founderRole = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Founder");
            var adminRole = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Server Administrator");
            var moderatorRole = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Server Moderator");

            //if user contains these roles
            if (user.Roles.Contains(founderRole) || user.Roles.Contains(adminRole) || user.Roles.Contains(moderatorRole))
            {
                await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Jailing " + jailUser.Username + "...");

                //if does then set jail role
                var jailRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == "jailed");
                await (jailUser as IGuildUser).AddRoleAsync(jailRole);

                //move to jail channel
                var jailChannel = (IVoiceChannel)Context.Guild.Channels.FirstOrDefault(x => x.Name == "jail");
                await (jailUser as IGuildUser).ModifyAsync(x => x.Channel = new Optional<IVoiceChannel>(jailChannel));

                await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " " + jailUser.Username + " jailed.");
            }
            else
            {
                await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Error: Hey, only the founder, server administrators, and moderators are allowed to jail.");
            }

        }

        //*********************
        //UNJAILS SELECTED USER
        //*********************
        [Command("unjail")]
        public async Task unjail(SocketGuildUser jailedUser)
        {
            var user = Context.User as SocketGuildUser;
            SocketUser _jailedUser = (SocketUser)jailedUser;
            string mentionLine = _jailedUser.Mention + " ";

            //roles allowed to unjail
            var founderRole = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Founder");
            var adminRole = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Server Administrator");
            var moderatorRole = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Server Moderator");

            //jail role
            var jailRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == "jailed");

            //if user contains these roles
            if ((user.Roles.Contains(founderRole) || user.Roles.Contains(adminRole) || user.Roles.Contains(moderatorRole)) && jailedUser.Roles.Contains(jailRole))
            {
                //unjail
                await (jailedUser as IGuildUser).RemoveRoleAsync(jailRole);

                //alert to user that is unjailed
                await Context.Channel.SendMessageAsync(mentionLine + " Thanks to " + Context.Message.Author.Username + ", you're free!");
            }
            //error not allowed to unjail
            else if(!jailedUser.Roles.Contains(jailRole))
            {
                await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Error: user is not jailed.");
            }
            else
            {
                await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Error: Hey, only the founder, server administrators, and moderators are allowed to jail.");
            }
        }

        //***********************
        //QUEUES UP SET PLAYLISTS
        //***********************
        [Command("playlist")]
        public async Task playlist(string action, string modifier)
        {
            ArrayList playlists = (ArrayList)SavePlaylist.get();
            /********************
             ******CREATE********
             ********************/
            if(action.Equals("create", StringComparison.InvariantCultureIgnoreCase))
            {   
                //playlist does not exist
                if(isPlaylist(modifier) == -1)
                {
                    await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Creating playlist " + modifier + "...");
                    Playlist playlist = new Playlist(modifier);
                    SavePlaylist.store(playlist);
                    await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Created playlist " + modifier + ".");
                }
                else
                {
                    await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Error: Playlist already exists.");
                }
            }
            /********************
            *******DELETE********
            ********************/
            else if (action.Equals("delete", StringComparison.InvariantCultureIgnoreCase))
            {
                //playlist does not exist
                int index = isPlaylist(modifier);
                if (index != -1)
                {
                    await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Deleting playlist " + modifier + "...");
                    SavePlaylist.delete(index);
                    await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Deleted playlist " + modifier + ".");
                }
                else
                {
                    await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Error: Playlist does not exists.");
                }
            }

            /********************
             ********LIST********
             ********************/
            else if (action.Equals("list", StringComparison.InvariantCultureIgnoreCase))
            {
                var builder = new EmbedBuilder();

                // 0 = playlist, 1 = all
                int listType = -1;

                if (modifier.Equals("all", StringComparison.InvariantCultureIgnoreCase))
                {
                    listType = 1;
                }
                else
                {
                    listType = 0;
                }

                if (listType == 1)
                {
                    builder.WithTitle("All Playlists");
                    for (int i = 0; i < playlists.Count; i++)
                    {
                        Playlist playlist = (Playlist)playlists[i];
                        builder.AddInlineField("Playlist " + (i + 1) + ": ", playlist.getName() + "\n");
                    }
                    builder.WithColor(Color.Blue);
                    await Context.Channel.SendMessageAsync("", false, builder);
                }

                if (listType == 0)
                {
                    int index = isPlaylist(modifier);
                    if (index != -1)
                    {
                        Playlist playlist = (Playlist)playlists[index];
                        builder.WithTitle("Playlist " + modifier + " songs");
                        ArrayList songs = playlist.getSongs();
                        Console.WriteLine("(" + songs.Count + ")");
                        if (songs.Count == 0)
                        {
                            builder.AddInlineField("No Songs", "---");
                        }
                        else
                        {
                            for (int i = 0; i < songs.Count; i++)
                            {
                                builder.AddInlineField("Song " + (i + 1) + ": ", songs[i]);
                            }
                        }
                        builder.WithColor(Color.Teal);
                        await Context.Channel.SendMessageAsync("", false, builder);
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Error: Playlist not found. Try !playlist list all, to list all playlists.");
                    }
                }
            }
            /********************
             ******PLAY**********
             ********************/
            else if(action.Equals("play", StringComparison.InvariantCultureIgnoreCase))
            {
                int index = isPlaylist(modifier);
                if(index != -1)
                {
                    Playlist playlist = (Playlist)playlists[index];
                    ArrayList songs = playlist.getSongs();
                    string url = "https://www.youtube.com/watch_videos?video_ids=";
                    for (int i = 0; i < songs.Count; i++)
                    {
                        string song = (string)songs[i];
                        string id = "";
                        int n = song.IndexOf("watch?v=");
                        if(n == -1)
                        {
                            await Context.Channel.SendMessageAsync("Song " + (i+1) + "invalid. Skipping song...");
                        }
                        else
                        {
                            id = song.Substring(n + 8);
                            if(!(i == songs.Count-1))
                                id += ",";
                            url += id;
                        }
                    }
                    await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + "To play this playlist, open this link in your browser and use that browser url for Rythm. \n" + url);
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Error: this playlist command does not exist.");
            }
        }
        /********************
        ********ADD*********
        ********************/
        [Command("playlist")]
        public async Task playlistEdit(string name, string action, string song)
        {
            ArrayList playlists = (ArrayList)SavePlaylist.get();

            if (action.Equals("add", StringComparison.InvariantCultureIgnoreCase))
            {
                int index = isPlaylist(name);
                if(index != -1)
                {
                    if(song.IndexOf("youtube") != -1)
                    {
                        Console.WriteLine("((" + index + "))");
                        Console.WriteLine("((" + playlists.Count + ")) count");
                        Playlist playlist = (Playlist)playlists[index];

                        playlist.addSong(song);

                        await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Added song: " + song);
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Error: Song must be a youtube link like: www.youtube.com/watch?v=4Tr0otuiQuU");
                    }
                }
            }
            else if (action.Equals("delete", StringComparison.InvariantCultureIgnoreCase))
            {
                int index = isPlaylist(name);
                if (index != -1)
                {
                    Playlist playlist = (Playlist)playlists[index];
                    if(playlist.containsSong(song))
                    {
                        //delete song
                        playlist.deleteSong(song);
                        await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Deleted song: " + song);
                    }
                    else if (int.TryParse(song, out int n))
                    {
                        if (n - 1 < playlists.Count)
                        {
                            await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Deleted song at index " + n + " : " + playlist.getSongs()[n - 1]);
                            playlist.deleteSong(n - 1);       
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Error: Playlist " + playlist.getName() + " only has " + playlist.getSongs().Count + " songs.");
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Error: Song does not exist in " + playlist.getName() + ".");
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Error: Playlist does not exist.");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + " Error: this playlist command does not exist.");
            }
        }

        //TODO: ADD ROLES FROM REACTIONS

        /*
         * 
         * LFG methods 
         * 
        */
        public string getCategory()
        {
            string suffix;

            //if in wrong channel
            try
            {
                suffix = Context.Channel.Name.Substring(18);
            }
            catch (Exception e)
            {
                return "error";
            }

            Console.Write(suffix);
            if (suffix.Equals("d2"))
            {
                return "Destiny 2";
            }
            else if (suffix.Equals("pubg"))
            {
                return "PUBG";
            }
            else if (suffix.Equals("csgo"))
            {
                return "CSGO";
            }
            else if (suffix.Equals("ow"))
            {
                return "Overwatch";
            }
            else //if channel has 18 or more characters and is not the correct channel
            {
                return "error";
            }
        }

        public int getPlayerCap(string category)
        {
            switch (category)
            {
                case "Destiny 2":
                    return 5;
                case "PUBG":
                    return 4;
                case "CSGO":
                    return 5;
                case "Overwatch":
                    return 6;
            }
            return 0;
        }

        public Boolean groupFound(string groupName)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                Group group = (Group)groups[i];
                Console.WriteLine("'" + group.getName() + "'" + "  " + i);
                if (group.getName() == groupName)
                {
                    return true;
                }
            }
            return false;
        }

        public int findGroupIndex(string groupName)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                Group group = (Group)groups[i];
                if (group.getName().Equals(groupName))
                {
                    return i;
                }
            }
            return -1;
        }

        public Boolean hasPlayerJoined(SocketUser player, Object groupObj)
        {
            Group group = (Group)groupObj;
            ArrayList players = group.getAllPlayers();
            for (int i = 0; i < players.Count; i++)
            {
                if (player.Equals(players[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /*
         * 
         * Playlist methods
         * 
         */ 
         public int isPlaylist(string name)
        {
            ArrayList playlists = (ArrayList)SavePlaylist.get();
            for(int i = 0; i < playlists.Count; i++)
            {
                Playlist playlist = (Playlist)playlists[i];
                Console.WriteLine("(" + playlist.getName() + "==" + name +")");
                if(playlist.getName().Equals(name))
                {
                    return i;
                }
            }
            return -1;
        }

    }
}
