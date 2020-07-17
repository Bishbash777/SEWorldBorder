using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Torch;
using Torch.API;
using Torch.Mod;
using Torch.Mod.Messages;

using System.IO;
using System.Linq;
using System.Text;
using Sandbox;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Torch.API.Managers;
using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Mod;
using Torch.Mod.Messages;
using VRage;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Game;
using VRage.ObjectBuilders;
using System.Collections.Concurrent;
using VRage.Groups;
using VRageMath;

namespace SEWorldBorder
{
    public class SEWorldBorder : TorchPluginBase
    {
        public SEWorldBorderConfig Config => _config?.Data;
        private Persistent<SEWorldBorderConfig> _config;
        public static readonly Logger Log = LogManager.GetLogger("SEWorldBorder");
        Dictionary<long, Vector3D> gridPositions = new Dictionary<long, Vector3D>();
        Dictionary<long, Vector3D> playerPositions = new Dictionary<long, Vector3D>();
        private int i = 0;
        double maxDistance = 0;
        public void Save() => _config?.Save();
        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            var configFile = Path.Combine(StoragePath, "SEWorldBorder.cfg");
            try {
                _config = Persistent<SEWorldBorderConfig>.Load(configFile);
            }
            catch (Exception e) {
                Log.Warn(e);
            }
            if (_config?.Data == null) {
                Log.Info("Creating default confuration file, because none was found!");
                _config = new Persistent<SEWorldBorderConfig>(configFile, new SEWorldBorderConfig());
                Save();
            }
        }

        public override async void Update()
        {
            int KMRADIUS = Config.Radius * 1000;
            maxDistance = Math.Pow(KMRADIUS, 2);
            i++;
            if (i == 16) {
                foreach (var player in MySession.Static.Players.GetOnlinePlayers()) {
                    IMyPlayer IPLAYER = (IMyPlayer)player;
                    double PlayerDistanceFromZero = Vector3D.DistanceSquared(player.GetPosition(), Vector3D.Zero);
                    if (IPLAYER.PromoteLevel != MyPromoteLevel.Admin || IPLAYER.PromoteLevel != MyPromoteLevel.Owner) {
                        foreach (var entity in MyEntities.GetEntities()) {
                            var grid = entity as MyCubeGrid;

                            if (grid == null)
                                continue;
                            double GridDistanceFromZero = Vector3D.DistanceSquared(grid.PositionComp.GetPosition(), Vector3D.Zero);
                            
                            if (KMRADIUS - GridDistanceFromZero < 250000) {
                                if (grid.BigOwners.Contains(player.Identity.IdentityId)) {
                                    utils.NotifyMessage("You are too close to the world border!", player.Id.SteamId);
                                }
                            }
                            else {
                                if (!gridPositions.ContainsKey(grid.EntityId)) {
                                    gridPositions.Add(grid.EntityId, grid.PositionComp.GetPosition());
                                }
                                else {
                                    gridPositions[grid.EntityId] = grid.PositionComp.GetPosition();
                                }
                            }
                            if (KMRADIUS - GridDistanceFromZero < 0) {
                                grid.Physics.ClearSpeed();
                                grid.Teleport(MatrixD.CreateWorld(gridPositions[grid.EntityId]));
                                utils.NotifyMessage("You have been moved to a safe position", player.Id.SteamId);
                            }
                        }


                        //Player detection
                        if (KMRADIUS - PlayerDistanceFromZero < 250000) {
                            utils.NotifyMessage("You are too close to the world border!", player.Id.SteamId);
                        }
                        else {
                            if (!playerPositions.ContainsKey(player.Identity.IdentityId)) {
                                playerPositions.Add(player.Identity.IdentityId, player.GetPosition());
                            }
                            else {
                                playerPositions[player.Identity.IdentityId] = player.GetPosition();
                            }
                        }
                        if (KMRADIUS - PlayerDistanceFromZero < 0) {
                            player.Identity.Character.Physics.ClearSpeed();
                            player.Identity.Character.Teleport(MatrixD.CreateWorld(playerPositions[player.Identity.IdentityId]));
                            utils.NotifyMessage("You have been moved to a safe position", player.Id.SteamId);

                        }
                    }
                    i = 0;
                }
            }
        }

    }
}