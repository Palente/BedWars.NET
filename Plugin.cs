using OpenAPI;
using OpenAPI.Events;
using OpenAPI.Events.Player;
using OpenAPI.Plugins;

namespace BedWars.NET
{
    [OpenPluginInfo(Author = "Palente", Description ="BedWars Plugin!", Name ="BedWars", Version = "0.0.1")]
    class Plugin : OpenPlugin, IEventHandler
    {
        public OpenApi Api;
        private Game _game;
        public override void Disabled(OpenApi api)
        {

        }

        public override void Enabled(OpenApi api)
        {
            Api = api;
            api.EventDispatcher.RegisterEvents(this);
            _game = new Game(api);
        }

        [EventHandler]
        public void OnJoin(PlayerJoinEvent e){
            var player = e.Player;
            _game.JoinGame(player);
        }

        [EventHandler]
        public void OnLeft(PlayerQuitEvent e)
        {
            _game.LeaveGame(e.Player);
        }
    }
}
