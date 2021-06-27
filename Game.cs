using MiNET.Blocks;
using MiNET.Entities;
using MiNET.Items;
using MiNET.UI;
using MiNET.Utils.Vectors;
using OpenAPI;
using OpenAPI.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BedWars.NET
{
    public class Game
    {
        public OpenApi Context { get; }
        private readonly List<OpenPlayer> _players = new();
        public GameStatus Status = GameStatus.STARTING;
        public List<Team> Teams = new();
        private const string PREFIX = "§e[§4Bed§cWars§e]§f";
        public readonly List<PlayerLocation> ShopSellerLocations = new();
        public readonly List<Npc> ShopSeller = new();
        public readonly PlayerLocation PlayerPositionWaiting = new()
        {
            X = -243,
            Y = 64,
            Z = -72,
        };
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
                BedPosition = new[]
                {
                    new BlockCoordinates
                    {
                        X = -194,
                        Y = 62,
                        Z = -228
                    },
                    new BlockCoordinates
                    {
                        X = -195,
                        Y = 62,
                        Z = -228
                    }
                },
                SpawnPosition = new PlayerLocation
                {
                    X = -188,
                    Y = 47,
                    Z = -222,
                }
            });
            ShopSellerLocations.Add(new PlayerLocation
            {
                X = -197,
                Y = 47,
                Z = -237
            });
            foreach(PlayerLocation location in ShopSellerLocations)
            {
                Npc npc = new(Context.LevelManager.GetDefaultLevel());
                npc.KnownPosition = location;
                npc.NameTag = "§b§eShop Seller!";
                npc.NpcSkinType = Npc.NpcTypes.Npc2;
                npc.DialogText = "test?";
                npc.NoAi = true;
                npc.IsAlwaysShowName = true;
                npc.CanDespawn = false;
                npc.Helmet = ItemFactory.GetItem(310, 0, 1);
                npc.SpawnEntity();
                ShopSeller.Add(npc);
            }
        }

        public void Start()
        {
            Status = GameStatus.RUNNING;
            _players.ForEach(player =>
            {
                player.PermissionLevel = MiNET.Net.PermissionLevel.Member;
                player.ActionPermissions = MiNET.Net.ActionPermissions.Default;
                player.SendAdventureSettings();
                player.SetGamemode(MiNET.Worlds.GameMode.Survival);
                var team = AttributeTeam(player);
                player.Teleport(team.SpawnPosition);
                player.SpawnPosition = team.SpawnPosition;
            });
            BroadcastMessage("Game has started! Good Luck");
        }
        public void End(EndReason reason)
        {

        }

        public void BroadcastMessage(string message, MiNET.MessageType type = MiNET.MessageType.Chat)
        {
            _players.ForEach(p =>
            {
                p.SendMessage(message, type: type);
            });
        }

        public Team? AttributeTeam(OpenPlayer player)
        {
            Team? team = Teams.FirstOrDefault(t => t.Player is null);
            team.Player = player;
            player.SendMessage($"{PREFIX} you joined the team {team.GetTeamName()}");
            return team;
        }

        public Team? GetBedTeam(BlockCoordinates coo)
        {
            return Teams.FirstOrDefault(t =>
            {
                foreach(BlockCoordinates coord in t.BedPosition)
                {
                    if(coo.X == coord.X && coo.Y == coord.Y)
                    {
                        return true;
                    }
                }
                return false;
            });
        }
        public void JoinQueue(OpenPlayer player)
        {
            player.Teleport(PlayerPositionWaiting);
        }
        //public OpenPlayer? GetPlayer(TeamColors playerColor) => GetTeam(playerColor)?.Player;

        //public Team? GetPlayerTeam(OpenPlayer player) => Teams.Find(t => t.Player == player)?.Player;

        public Team? GetTeam(TeamColors color) => Teams.Find(t => t.Color == color);
        public Team? GetTeam(OpenPlayer player) => Teams.Find(t => t.Player == player);

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
                /*player.PermissionLevel = MiNET.Net.PermissionLevel.Visitor;
                player.ActionPermissions = MiNET.Net.ActionPermissions.Default;
                player.SendAdventureSettings();
                player.SetGamemode(MiNET.Worlds.GameMode.Adventure);*/
                _players.Add(player);
                BroadcastMessage($"{PREFIX} {player.Username} just joined there is now {_players.Count}/8");
                //JoinQueue(player);
                Start();
            }
        }

        public void LeaveGame(OpenPlayer player)
        {
            if (Status == GameStatus.RUNNING)
            {
                _players.Remove(player);
            }
            if (Status == GameStatus.STARTING)
            {
                BroadcastMessage($"{PREFIX} just left there is now {_players.Count}/8");
                _players.Remove(player);
            }
        }

        public bool BreakBlock(Block block, OpenPlayer player)
        {
            if (Status == GameStatus.STARTING)
                return false;
            if (block is Bed)
            {
                if(GetTeam(player) == GetBedTeam(block.Coordinates))
                {
                    player.SendMessage("You can't break your own bed!", type: MiNET.MessageType.Popup);
                    //Trying to break his own bed!
                    return false;
                }
            }
            return true;
        }
        public bool PlaceBlock(Block block, OpenPlayer player)
        {
            if(Status == GameStatus.RUNNING)
            {
                //Check if block is placed next to SpawnPoint!
            }
            else
            {
                player.SendMessage("This action is not allowed for now!", MiNET.MessageType.Popup);
                return false;
            }
            return true;
        }
        public bool OnPlayerDamage(OpenPlayer player, OpenPlayer damager)
        {
            if (Status == GameStatus.RUNNING)
                return true;
            damager.SendMessage("You can't damage another player!", MiNET.MessageType.Popup);
            return false;
        }
        public bool OnPlayerDamage(Npc npc, OpenPlayer damager)
        {
            if(Status == GameStatus.RUNNING)
            {
                damager.SendMessage("Opening Shop!");
                Console.WriteLine("Npc Frapper");
            }
            return false;
        }
        public void OnDeath(OpenPlayer player)
        {
            if(Status == GameStatus.STARTING)
            {
                player.HealthManager.Health = player.HealthManager.MaxHealth;
                player.IsSpawned = true;
                player.Teleport(PlayerPositionWaiting);
            }
            if(Status == GameStatus.RUNNING)
            {
                player.HealthManager.Health = player.HealthManager.MaxHealth;
                player.IsSpawned = true;
                Task.Delay(500);
                Console.WriteLine("Going to teleport entity!");
                player.Teleport(PlayerPositionWaiting);
            }
        }
        public void InteractShop(OpenPlayer player)
        {
            player.SendMessage("Opening shop..", MiNET.MessageType.Popup);
            ModalForm form = new()
            {
                Title = "Test",
                Id = 751,
                Content = "What a good Content",
                Button1 = "True",
                Button2 = "False",
                ExecuteAction = new Action<MiNET.Player, ModalForm>((pl, f) =>
                {
                    //What is the utility of this
                    pl.SendMessage($"You replied with {f.Title}");
                })
            };
            player.SendForm(form);
        }
    }
}