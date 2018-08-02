using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace SwiftyBot
{
    public class Bot
    {

        private DiscordSocketClient _client;

        private CommandHandler _handler;

        static void Main(string[] args) => new Bot().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            _client = new DiscordSocketClient();

            await _client.LoginAsync(TokenType.Bot, "Mzg1ODQ5MDAzNDY1NTA2ODE2.DQHbfg.si3AMwCva_K8nBp4N66Hxp-jERM");

            await _client.StartAsync();

            _handler = new CommandHandler(_client);

            Modules.SavePlaylist.load();

            await Task.Delay(-1);
        }
    }
}
