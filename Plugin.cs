using MiNET.Entities;
using OpenAPI;
using OpenAPI.Events;
using OpenAPI.Events.Block;
using OpenAPI.Events.Entity;
using OpenAPI.Events.Player;
using OpenAPI.Player;
using OpenAPI.Plugins;
using System;

namespace BedWars.NET
{
    [OpenPluginInfo(Author = "Palente", Description = "BedWars Plugin!", Name = "BedWars", Version = "0.0.1")]
    public class Plugin : OpenPlugin, IEventHandler
    {
        public OpenApi Api;
        private Game _game;

        public override void Disabled(OpenApi api)
        {
            _game.End(EndReason.SERVER);
        }

        public override void Enabled(OpenApi api)
        {
            Api = api;
            api.EventDispatcher.RegisterEvents(this);
            _game = new Game(api);
        }

        [EventHandler]
        public void OnJoin(PlayerJoinEvent e)
        {
            if (!Api.LevelManager.HasDefaultLevel)
                Api.LevelManager.SetDefaultLevel(e.Player.Level);
            _game.JoinGame(e.Player);
        }

        [EventHandler]
        public void OnLeft(PlayerQuitEvent e)
        {
            _game.LeaveGame(e.Player);
        }

        [EventHandler]
        public void OnBreak(BlockBreakEvent e)
        {
            e.SetCancelled(!_game.BreakBlock(e.Block, (OpenPlayer)e.Source));
        }
        [EventHandler]
        public void OnPlace(BlockPlaceEvent e)
        {
            if (!_game.PlaceBlock(e.Block, e.Player))
                e.SetCancelled(true);
        }
        [EventHandler]
        public void OnHit(EntityDamageEvent e)
        {
            Console.WriteLine($"Damaged Cause: {e.Cause}");
            if(e.Entity is OpenPlayer player && e.Attacker is OpenPlayer attacker)
            {
                Console.WriteLine("PVP BETWEEN 2 PLAYER");
                if (!_game.OnPlayerDamage(player, attacker))
                    e.SetCancelled(true);
            }
            if (e.Entity is Npc npc && e.Attacker is OpenPlayer attackerEntity)
            {
                Console.WriteLine("PLAYER HIT NPC");
                if (!_game.OnPlayerDamage(npc, attackerEntity))
                    e.SetCancelled(true);
            }
        }
        [EventHandler]
        public void OnSpawned(PlayerSpawnedEvent e)
        {
            Console.WriteLine($"PlayerSpawnedEvent: {e.IsCancelled}");
        }
        [EventHandler]
        public void OnDeath(EntityKilledEvent e)
        {
            Console.WriteLine($"EntityKilledEvent: {e.IsCancelled}");
            if(e.Entity is OpenPlayer player)
            {
                _game.OnDeath(player);
            }
        }
        [EventHandler]
        public void OnInteract(EntityInteractEvent e)
        {
            if(e.Entity is Npc)
            {
                _game.InteractShop(e.SourcePlayer);
            }
        }
        [EventHandler]
        public void OnInteract(PlayerInteractEvent e)
        {
            Console.WriteLine("Interact Player");
        }
        [EventHandler]
        public void OnFood(FoodLevelChangeEvent e)
        {
            e.SetCancelled(true);
            e.Player.HungerManager.Hunger = e.Player.HungerManager.MaxHunger;
        }
    }
    public enum EndReason
    {
        FINISHED,
        SERVER
    }
}