using OpenAPI;
using OpenAPI.Player;
using System.Collections.Generic;
using System.Linq;

namespace BedWars.NET
{
    public class Game
    {
        public OpenApi Context;
        private List<OpenPlayer> _players;
        public GameStatus Status = GameStatus.STARTING;
        public List<Team> Teams;
        private const string PREFIX = "§e[§4Bed§cWars§e]§f";
        public Game(OpenApi api)
        {
            Context = api;
            Init();
        }
        public void Init()
        {
            Teams.Add(new Team
            {
                Color = TeamColors.YELLOW,
                BedPosition = new int[3]
                {
                    -194,
                    62,
                    -228
                }
            });
        }
        public void Start()
        {

            Status = GameStatus.RUNNING;
            _players.ForEach(player =>
            {
                player.PermissionLevel = MiNET.Net.PermissionLevel.Member;
                player.ActionPermissions = MiNET.Net.ActionPermissions.AttackPlayers | MiNET.Net.ActionPermissions.BuildAndMine | MiNET.Net.ActionPermissions.OpenContainers;
                player.SendAdventureSettings();
                player.SetGamemode(MiNET.Worlds.GameMode.Survival);
            });
        }
        public void BroadcastMessage(string message, MiNET.MessageType type= MiNET.MessageType.Chat)
        {
            _players.ForEach(p =>
            {
                p.SendMessage(message, type: type);
            });
        }
        public void AttributeTeam(OpenPlayer player)
        {
            Team? team = Teams.FirstOrDefault(t => t.Player is null);
            team.Player = player;
            player.SendMessage($"{PREFIX} you joined the ")
        }




        public OpenPlayer? GetPlayer(TeamColors playerColor) => GetTeam(playerColor)?.Player;
        public OpenPlayer? GetPlayerTeam(OpenPlayer player) => Teams.Find(t => t.Player == player)?.Player;
        public Team? GetTeam(TeamColors color) => Teams.Find(t => t.Color == color);
        // EVENT HANDLERS
        public void JoinGame(OpenPlayer player)
        {
            if (Status == GameStatus.RUNNING)
            {
                player.Disconnect("The game has already started please join later");
                return;
            }
            if (Status == GameStatus.ENDED)
            {
                player.Disconnect("The game has ended try again in a few seconds");
                return;
            }
            if (Status == GameStatus.STARTING)
            {
                player.PermissionLevel = MiNET.Net.PermissionLevel.Visitor;
                player.ActionPermissions = MiNET.Net.ActionPermissions.Default;
                player.SendAdventureSettings();
                player.SetGamemode(MiNET.Worlds.GameMode.Adventure);
                _players.Add(player);
                BroadcastMessage($"{PREFIX} {player.Username} just joined there is now {_players.Count}/8");
            }
        }
        public void LeaveGame(OpenPlayer player)
        {
            if(Status == GameStatus.RUNNING)
            {
                _players.Remove(player);
            }
            if(Status == GameStatus.STARTING)
            {
                BroadcastMessage($"{PREFIX} just left there is now {_players.Count}/8");
                _players.Remove(player);
            }
        }
    }
}
