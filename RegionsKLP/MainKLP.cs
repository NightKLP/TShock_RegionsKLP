using Microsoft.Xna.Framework;
using MySql.Data.MySqlClient;
using Mysqlx.Prepare;
using NuGet.Protocol.Plugins;
using RegionsKLP.Functions;
using RegionsKLP.Modules;
using System.Data;
using System.Diagnostics;
using System.Timers;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Hooks;
using static TShockAPI.GetDataHandlers;
using DPoint = System.Drawing.Point;

namespace RegionsKLP
{
    [ApiVersion(2, 1)]
    public class MainKLP : TerrariaPlugin
    {
        #region =[ Plugin Info ]=
        public override string Author => "NightKLP";

        public override string Description => "GSKLP HouseRegions And SmartRegions";

        public override string Name => "RegionskLP";

        public override Version Version => new System.Version(1, 1);
        #endregion


        public static Config Config = Config.Read(); //CONFIG


        public static IDbConnection MainIDBC = getsqlcon(Config.DB.StorageType.ToLower());
        public static MainDB MainDBManager = new(MainIDBC);

        public static IDbConnection getsqlcon(string StorageType)
        {
            if (StorageType == "sqlite")
            {
                string sql = Path.Combine(TShock.SavePath, Config.DB.SqliteDBPath);
                Directory.CreateDirectory(Path.GetDirectoryName(sql));
                return new Microsoft.Data.Sqlite.SqliteConnection(string.Format("Data Source={0}", sql));
            }
            else if (StorageType == "mysql")
            {
                try
                {
                    var hostport = Config.DB.MySqlHost.Split(':');
                    MySqlConnection DB = new MySqlConnection();
                    DB.ConnectionString =
                        String.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
                            hostport[0],
                            hostport.Length > 1 ? hostport[1] : "3306",
                            Config.DB.MySqlDbName,
                            Config.DB.MySqlUsername,
                            Config.DB.MySqlPassword
                            );
                    return DB;
                }
                catch (MySqlException ex)
                {
                    throw new Exception("MySql not setup correctly");
                }
            }
            else
            {
                throw new Exception("Invalid storage type");
            }
        }



        // Optimization ( Data Gathering )
        public static string[] RegionAllow_Chest = { };
        public static string[] RegionAllowStartsWith_Chest = { };

        public static string[] RegionAllow_Mannequin = { };
        public static string[] RegionAllowStartsWith_Mannequin = { };

        public static string[] RegionAllow_Sign = { };
        public static string[] RegionAllowStartsWith_Sign = { };

        public static string[] RegionAllow_ItemFrame = { };
        public static string[] RegionAllowStartsWith_ItemFrame = { };

        public static string[] RegionAllow_ItemRack = { };
        public static string[] RegionAllowStartsWith_ItemRack = { };

        public static string[] RegionAllow_ItemJar = { };
        public static string[] RegionAllowStartsWith_ItemJar = { };

        public static string[] RegionAllow_FoodPlatter = { };
        public static string[] RegionAllowStartsWith_FoodPlatter = { };

        public static string[] RegionAllow_TileEntityMisc = { };
        public static string[] RegionAllowStartsWith_TileEntityMisc = { };

        public static void SyncRegionAllowStrData(Config get)
        {
            List<string> R_RegionAllow_Chest = new();
            List<string> R_RegionAllowStartsWith_Chest = new();

            List<string> R_RegionAllow_Mannequin = new();
            List<string> R_RegionAllowStartsWith_Mannequin = new();

            List<string> R_RegionAllow_Sign = new();
            List<string> R_RegionAllowStartsWith_Sign = new();

            List<string> R_RegionAllow_ItemFrame = new();
            List<string> R_RegionAllowStartsWith_ItemFrame = new();

            List<string> R_RegionAllow_ItemRack = new();
            List<string> R_RegionAllowStartsWith_ItemRack = new();

            List<string> R_RegionAllow_ItemJar = new();
            List<string> R_RegionAllowStartsWith_ItemJar = new();

            List<string> R_RegionAllow_FoodPlatter = new();
            List<string> R_RegionAllowStartsWith_FoodPlatter = new();

            List<string> R_RegionAllow_TileEntityMisc = new();
            List<string> R_RegionAllowStartsWith_TileEntityMisc = new();

            #region [ gather ]
            foreach (UniqueRegionType getregion in get.Main.uniqueRegionsName)
            {
                if (getregion.CanEdit_Chest)
                {
                    R_RegionAllow_Chest.Add(getregion.NameText);
                }
                if (getregion.CanEdit_Mannequin)
                {
                    R_RegionAllow_Mannequin.Add(getregion.NameText);
                }
                if (getregion.CanEdit_Sign)
                {
                    R_RegionAllow_Sign.Add(getregion.NameText);
                }
                if (getregion.CanEdit_ItemFrame)
                {
                    R_RegionAllow_ItemFrame.Add(getregion.NameText);
                }
                if (getregion.CanEdit_ItemRack)
                {
                    R_RegionAllow_ItemRack.Add(getregion.NameText);
                }
                if (getregion.CanEdit_ItemJar)
                {
                    R_RegionAllow_ItemJar.Add(getregion.NameText);
                }
                if (getregion.CanEdit_FoodPlatter)
                {
                    R_RegionAllow_FoodPlatter.Add(getregion.NameText);
                }
                if (getregion.CanEdit_TileEntityMisc)
                {
                    R_RegionAllow_TileEntityMisc.Add(getregion.NameText);
                }
            }

            foreach (UniqueRegionType getregion in get.Main.uniqueRegionsName_StartsWith)
            {
                if (getregion.CanEdit_Chest)
                {
                    R_RegionAllowStartsWith_Chest.Add(getregion.NameText);
                }
                if (getregion.CanEdit_Mannequin)
                {
                    R_RegionAllowStartsWith_Mannequin.Add(getregion.NameText);
                }
                if (getregion.CanEdit_Sign)
                {
                    R_RegionAllowStartsWith_Sign.Add(getregion.NameText);
                }
                if (getregion.CanEdit_ItemFrame)
                {
                    R_RegionAllowStartsWith_ItemFrame.Add(getregion.NameText);
                }
                if (getregion.CanEdit_ItemRack)
                {
                    R_RegionAllowStartsWith_ItemRack.Add(getregion.NameText);
                }
                if (getregion.CanEdit_ItemJar)
                {
                    R_RegionAllowStartsWith_ItemJar.Add(getregion.NameText);
                }
                if (getregion.CanEdit_FoodPlatter)
                {
                    R_RegionAllowStartsWith_FoodPlatter.Add(getregion.NameText);
                }
                if (getregion.CanEdit_TileEntityMisc)
                {
                    R_RegionAllowStartsWith_TileEntityMisc.Add(getregion.NameText);
                }
            }
            #endregion

            RegionAllow_Chest = R_RegionAllow_Chest.ToArray();
            RegionAllowStartsWith_Chest = R_RegionAllowStartsWith_Chest.ToArray();

            RegionAllow_Mannequin = R_RegionAllow_Mannequin.ToArray();
            RegionAllowStartsWith_Mannequin = R_RegionAllowStartsWith_Mannequin.ToArray();

            RegionAllow_Sign = R_RegionAllow_Sign.ToArray();
            RegionAllowStartsWith_Sign = R_RegionAllowStartsWith_Sign.ToArray();

            RegionAllow_ItemFrame = R_RegionAllow_ItemFrame.ToArray();
            RegionAllowStartsWith_ItemFrame = R_RegionAllowStartsWith_ItemFrame.ToArray();

            RegionAllow_ItemRack = R_RegionAllow_ItemRack.ToArray();
            RegionAllowStartsWith_ItemRack = R_RegionAllowStartsWith_ItemRack.ToArray();

            RegionAllow_ItemJar = R_RegionAllow_ItemJar.ToArray();
            RegionAllowStartsWith_ItemJar = R_RegionAllowStartsWith_ItemJar.ToArray();

            RegionAllow_FoodPlatter = R_RegionAllow_FoodPlatter.ToArray();
            RegionAllowStartsWith_FoodPlatter = R_RegionAllowStartsWith_FoodPlatter.ToArray();

            RegionAllow_TileEntityMisc = R_RegionAllow_TileEntityMisc.ToArray();
            RegionAllowStartsWith_TileEntityMisc = R_RegionAllowStartsWith_TileEntityMisc.ToArray();
        }

        public static void SyncUniqueRegion()
        {
            foreach (Region get in TShock.Regions.Regions)
            {
                if (!get.DisableBuild) { continue; }
                if (IsUniqueRegion(get.Name))
                {
                    TShock.Regions.SetRegionState(get.Name, false);
                }
            }
        }


        public const string HouseWarding = "WardingActive";


        public MainKLP(Main game) : base(game)
        {
            //amogus
        }

        public override void Initialize()
        {
            SyncRegionAllowStrData(Config);

            ServerApi.Hooks.NetGetData.Register(this, OnGetData);

            //check
            GetDataHandlers.PlayerUpdate += OnPlayerUpdate;
            TShockAPI.Hooks.PlayerHooks.PlayerPostLogin += OnPlayerPostLogin;
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);

            //breakchange
            GetDataHandlers.TileEdit += OnTileEdit;

            GetDataHandlers.PlaceObject += OnPlaceObject;

            GetDataHandlers.PaintTile += OnPaintTile;

            GetDataHandlers.PaintWall += OnPaintWall;


            GetDataHandlers.LiquidSet += OnLiquidSet;

            GetDataHandlers.PlaceChest += OnPlaceChest;

            GetDataHandlers.PlaceTileEntity += OnPlaceTileEntity;

            GetDataHandlers.MassWireOperation += OnMassWireOperation;
            //modify
            GetDataHandlers.ChestOpen += OnChestOpen;
            GetDataHandlers.ChestItemChange += OnChestItemChange;

            GetDataHandlers.DisplayDollPoseSync += OnDisplayDollPoseSync;
            GetDataHandlers.DisplayDollItemSync += OnDisplayDollItemSync;

            GetDataHandlers.Sign += OnSignChange;

            GetDataHandlers.PlaceItemFrame += OnPlaceItemFrame;

            GetDataHandlers.FoodPlatterTryPlacing += OnFoodPlatterTryPlacing;

            GetDataHandlers.RequestTileEntityInteraction += OnRequestTileEntityInteraction;

            OTAPI.Hooks.Chest.QuickStack += OnQuickStack;

            //entity
            ServerApi.Hooks.NpcAIUpdate.Register(this, OnNPCAIUpdate);

            //e
            RegionHooks.RegionCreated += OnRegionCreated;
            RegionHooks.RegionRenamed += OnRegionRenamed;
            RegionHooks.RegionDeleted += OnRegionDeleted;

            GeneralHooks.ReloadEvent += OnReload;

            MainDBManager.SyncUserIDNameCache();

            MainDBManager.PlayerHousingKLPSync();
            MainDBManager.RegionKLPSync();
            CommandsKLP.Initialize();

            SyncUniqueRegion();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);

                //check
                GetDataHandlers.PlayerUpdate -= OnPlayerUpdate;
                TShockAPI.Hooks.PlayerHooks.PlayerPostLogin -= OnPlayerPostLogin;

                //breakchange
                GetDataHandlers.TileEdit -= OnTileEdit;

                GetDataHandlers.PlaceObject -= OnPlaceObject;

                GetDataHandlers.PaintTile -= OnPaintTile;

                GetDataHandlers.PaintWall -= OnPaintWall;


                GetDataHandlers.LiquidSet -= OnLiquidSet;

                GetDataHandlers.PlaceChest -= OnPlaceChest;

                GetDataHandlers.PlaceTileEntity -= OnPlaceTileEntity;

                GetDataHandlers.MassWireOperation -= OnMassWireOperation;
                //modify
                GetDataHandlers.ChestOpen -= OnChestOpen;
                GetDataHandlers.ChestItemChange -= OnChestItemChange;

                GetDataHandlers.DisplayDollPoseSync -= OnDisplayDollPoseSync;
                GetDataHandlers.DisplayDollItemSync -= OnDisplayDollItemSync;

                GetDataHandlers.Sign -= OnSignChange;

                GetDataHandlers.PlaceItemFrame -= OnPlaceItemFrame;

                GetDataHandlers.FoodPlatterTryPlacing -= OnFoodPlatterTryPlacing;

                GetDataHandlers.RequestTileEntityInteraction -= OnRequestTileEntityInteraction;

                OTAPI.Hooks.Chest.QuickStack -= OnQuickStack;

                //entity
                ServerApi.Hooks.NpcAIUpdate.Deregister(this, OnNPCAIUpdate);

                //e
                RegionHooks.RegionCreated -= OnRegionCreated;
                RegionHooks.RegionRenamed -= OnRegionRenamed;
                RegionHooks.RegionDeleted -= OnRegionDeleted;

                GeneralHooks.ReloadEvent -= OnReload;
            }
        }

        private void OnRegionCreated(RegionHooks.RegionCreatedEventArgs args)
        {
            if (IsUniqueRegion(args.Region.Name))
            {
                TShock.Regions.SetRegionState(args.Region.ID, false);
            }
        }

        private void OnRegionRenamed(RegionHooks.RegionRenamedEventArgs args)
        {
            if (IsUniqueRegion(args.NewName))
            {
                TShock.Regions.SetRegionState(args.Region.ID, false);
            }
        }

        private void OnRegionDeleted(RegionHooks.RegionDeletedEventArgs args)
        {
            MainDBManager.DeleteRegionKLP(args.Region.ID);
        }

        #region Check

        private static Vector2[] lasplayerpos_update = new Vector2[Main.player.Length];

        private static int[][] lasplayerinregions_update = Enumerable.Range(0, Main.player.Length).Select(_ => Array.Empty<int>()).ToArray();
        private static async void OnPlayerUpdate(object? sender, GetDataHandlers.PlayerUpdateEventArgs args)
        {
            if (args.Player.State != 10) { return; }
            if (args.Player.TPlayer.Center == lasplayerpos_update[args.PlayerId]) { return; }
            lasplayerpos_update[args.PlayerId] = args.Player.TPlayer.Center;

            Vector2 tilepos = new Vector2((int)args.Player.TPlayer.Center.X / 16, (int)args.Player.TPlayer.Center.Y / 16);

            var regions = TShock.Regions.InAreaRegionID((int)tilepos.X, (int)tilepos.Y).ToArray();

            var enteredregion = regions.Where(e => !lasplayerinregions_update[args.PlayerId].Contains(e)).ToArray();
            var leftregion = lasplayerinregions_update[args.PlayerId].Where(e => !regions.Contains(e)).ToArray();

            if (enteredregion.Length != 0)
            {
                foreach (int regid in enteredregion)
                {
                    Region get = TShock.Regions.GetRegionByID(regid);

                    if (get == null) { continue; }

                    if (!args.Player.HasPermission(Permissions.editregion) &&
                        !args.Player.HasPermission(Permissions.manageregion) &&
                        !args.Player.HasPermission(Config.Permissions.HouseKLP_Admin) &&
                        !args.Player.HasPermission(Config.Permissions.HouseKLP_ByPassBlackList))
                    {
                        if (MainDBManager.TryGetRegionKLPByID(regid, out RegionKLP regionklp))
                        {
                            if (regionklp.IsBlackListed(args.Player))
                            {
                                args.Player.Teleport(lasplayerpos_update[args.PlayerId]);

                                args.Player.SendData(PacketTypes.CreateCombatTextExtended,
                                    $"Entered a {(get.Name.StartsWith(Config.Main.Housing.Indicator) ? $"house{(args.Player.Name == get.Owner ? "" : $" owned by {get.Owner}")}" : $"region named: {get.Name}")}" +
                                    $" [ Access Denied! ]"
                                    , (int)Color.OrangeRed.packedValue, args.Player.X, args.Player.Y + 50);
                                continue;
                            }
                        }
                    }
                    await PlayAni(SpreadFromBorder(get.Area, tilepos));

                    if ((bool)Config.Main.Housing.RegionEnterIndication)
                    {
                        args.Player.SendData(PacketTypes.CreateCombatTextExtended,
                            $"Entered a {(get.Name.StartsWith(Config.Main.Housing.Indicator) ? $"house{(args.Player.Name == get.Owner ? "" : $" owned by {get.Owner}")}" : $"region named: {get.Name}")}" +
                            (get.HasPermissionToBuildInRegion(args.Player) ? " [ Access Granted! ]" : "")
                            , (int)Color.Aquamarine.packedValue, args.Player.X, args.Player.Y + 30);
                    }
                }
            }

            if (leftregion.Length != 0)
            {
                foreach (int regid in leftregion)
                {
                    Region get = TShock.Regions.GetRegionByID(regid);

                    if (get == null) { continue; }

                    await PlayAni(SpreadFromBorder(get.Area, tilepos));

                    if ((bool)Config.Main.Housing.RegionEnterIndication)
                    {
                        string msg = $"You left a {(get.Name.StartsWith(Config.Main.Housing.Indicator) ? $"house{(args.Player.Name == get.Owner ? "" : $" owned by {get.Owner}")}" : $"region named: {get.Name}")}";


                        args.Player.SendData(PacketTypes.CreateCombatTextExtended,
                            msg, (int)Color.Aquamarine.packedValue, args.Player.X, args.Player.Y);
                    }
                }
            }

            lasplayerinregions_update[args.PlayerId] = regions;

            return;

            async Task PlayAni(List<Vector2> pointsets)
            {
                foreach (Vector2 p in pointsets)
                {
                    ParticleOrchestraSettings settings = new()
                    {
                        IndexOfPlayerWhoInvokedThis = args.PlayerId,
                        MovementVector = new(0, 0),
                        PositionInWorld = (p * 16) + new Vector2(8, 8),
                        UniqueInfoPiece = -1
                    };

                    ParticleOrchestrator.BroadcastParticleSpawn(ParticleOrchestraType.ChlorophyteLeafCrystalPassive, settings);

                    await Task.Delay(20);
                }
            }

            List<Vector2> SpreadFromBorder(Rectangle rect, Vector2 position)
            {
                int steps = 2;
                float spacing = 1;
                List<Vector2> points = new();

                Vector2 origin = GetBorderPosition(rect, position);

                float perimeter =
                    2f * (rect.Width + rect.Height);

                float startDistance =
                    origin.Y == rect.Top
                        ? origin.X - rect.Left
                        : origin.X == rect.Right
                            ? rect.Width + origin.Y - rect.Top
                            : origin.Y == rect.Bottom
                                ? rect.Width + rect.Height + rect.Right - origin.X
                                : 2f * rect.Width + rect.Height + rect.Bottom - origin.Y;

                // Middle point first
                points.Add(GetPointAtDistance(startDistance, rect));

                for (int i = 1; i <= steps; i++)
                {
                    // Left
                    points.Add(GetPointAtDistance(
                        startDistance - i * spacing,
                        rect
                    ));

                    // Right
                    points.Add(GetPointAtDistance(
                        startDistance + i * spacing,
                        rect
                    ));
                }

                return points;
            }

            Vector2 GetPointAtDistance(float distance, Rectangle rect)
            {
                float perimeter = 2f * (rect.Width + rect.Height);

                distance = ((distance % perimeter) + perimeter) % perimeter;

                if (distance <= rect.Width)
                    return new Vector2(rect.Left + distance, rect.Top);

                distance -= rect.Width;

                if (distance <= rect.Height)
                    return new Vector2(rect.Right, rect.Top + distance);

                distance -= rect.Height;

                if (distance <= rect.Width)
                    return new Vector2(rect.Right - distance, rect.Bottom);

                distance -= rect.Width;

                return new Vector2(rect.Left, rect.Bottom - distance);
            }

            Vector2 GetBorderPosition(Rectangle rect, Vector2 position)
            {
                byte border = GetNearestBorder(rect, position);

                return border switch
                {
                    0 => new Vector2(
                        MathHelper.Clamp(position.X, rect.Left, rect.Right),
                        rect.Top
                    ),

                    1 => new Vector2(
                        MathHelper.Clamp(position.X, rect.Left, rect.Right),
                        rect.Bottom
                    ),

                    2 => new Vector2(
                        rect.Left,
                        MathHelper.Clamp(position.Y, rect.Top, rect.Bottom)
                    ),

                    3 => new Vector2(
                        rect.Right,
                        MathHelper.Clamp(position.Y, rect.Top, rect.Bottom)
                    ),

                    _ => position
                };
            }

            byte GetNearestBorder(Rectangle rect, Vector2 position)
            {
                float top = Math.Abs(position.Y - rect.Top);
                float bottom = Math.Abs(rect.Bottom - position.Y);
                float left = Math.Abs(position.X - rect.Left);
                float right = Math.Abs(rect.Right - position.X);

                float min = Math.Min(Math.Min(top, bottom), Math.Min(left, right));

                if (min == top) return 0;
                if (min == bottom) return 1;
                if (min == left) return 2;

                return 3;
            }
        }
        private static void OnPlayerPostLogin(PlayerPostLoginEventArgs args)
        {
            if (!PlayerHousingKLP.ItExist(args.Player.Account.ID))
            {
                MainDBManager.TryCreatePlayerHouseData(args.Player);
            }

            if (!MainDBManager.UserIDNameCache.ContainsKey(args.Player.Account.ID))
            {
                MainDBManager.UserIDNameCache.Add(args.Player.Account.ID, args.Player.Account.Name);
            }

            if (!MainDBManager.NameUserIDCache.ContainsKey(args.Player.Account.Name))
            {
                MainDBManager.NameUserIDCache.Add(args.Player.Account.Name, args.Player.Account.ID);
            }
        }

        private void OnServerLeave(LeaveEventArgs args)
        {
            if (Netplay.Clients[args.Who].State < 3) return;

            RemoveOnCreateHouse(TShock.Players[args.Who]);
        }
        #endregion

        #region {[ OnGetData ]}
        private void OnGetData(GetDataEventArgs args)
        {
            if (args.Handled)
                return;

            #region { Weapons Rack Try Placing }
            if (args.MsgID == PacketTypes.WeaponsRackTryPlacing)
            {
                TSPlayer player = TShock.Players[args.Msg.whoAmI];
                using (var stream = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        short X = reader.ReadInt16();
                        short Y = reader.ReadInt16();
                        short ItemID = reader.ReadInt16();
                        byte Prefix = reader.ReadByte();
                        short Stack = reader.ReadInt16();
                        TEWeaponsRack WeaponRack = (TEWeaponsRack)TileEntity.ByID[TEWeaponsRack.Find(X, Y)];

                        if (!CanModify(player, X, Y, TileModifyType.ItemRack))
                        {
                            NetMessage.SendData((int)PacketTypes.UpdateTileEntity, -1, -1, NetworkText.Empty, WeaponRack.ID, 0, 1);
                            args.Handled = true;
                            return;
                        }
                    }
                }
                return;
            }
            #endregion

            #region { TEDeadCells Display Jar }
            if (args.MsgID == PacketTypes.TEDeadCellsDisplayJar)
            {
                TSPlayer player = TShock.Players[args.Msg.whoAmI];
                using (var stream = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        short X = reader.ReadInt16();
                        short Y = reader.ReadInt16();
                        short ItemID = reader.ReadInt16();
                        byte Prefix = reader.ReadByte();
                        short Stack = reader.ReadInt16();
                        TEDeadCellsDisplayJar DisplayJar = (TEDeadCellsDisplayJar)TileEntity.ByID[TEDeadCellsDisplayJar.Find(X, Y)];

                        if (!CanModify(player, X, Y, TileModifyType.ItemJar))
                        {
                            NetMessage.SendData((int)PacketTypes.UpdateTileEntity, -1, -1, NetworkText.Empty, DisplayJar.ID, 0, 1);
                            args.Handled = true;
                            return;
                        }
                    }
                }
                return;
            }
            #endregion
        }
        #endregion

        #region - Breakable Tiles -

        private static ushort[] breakableTiles =
        {
            //plants
            TileID.Plants,
            TileID.Plants2,
            TileID.AshPlants,
            TileID.CorruptPlants,
            TileID.CrimsonPlants,
            TileID.HallowedPlants,
            TileID.HallowedPlants2,
            TileID.JunglePlants,
            TileID.JunglePlants2,
            TileID.LifeFruit,
            TileID.MushroomPlants,
            TileID.OasisPlants,
            TileID.PlantDetritus,
            TileID.Pumpkins,

            //herbs
            TileID.ImmatureHerbs,
            TileID.MatureHerbs,
            TileID.BloomingHerbs,

            //vines
            TileID.VineFlowers,
            TileID.Vines,
            TileID.AshVines,
            TileID.CorruptVines,
            TileID.CrimsonVines,
            TileID.HallowedVines,
            TileID.JungleVines,
            TileID.MushroomVines,

            //stone
            TileID.LongMoss,
            TileID.ArgonMoss,
            TileID.BlueMoss,
            TileID.BrownMoss,
            TileID.GreenMoss,
            TileID.KryptonMoss,
            TileID.LavaMoss,
            TileID.PurpleMoss,
            TileID.RainbowMoss,
            TileID.RedMoss,
            TileID.VioletMoss,
            TileID.XenonMoss,

            //dye
            TileID.DyePlants,

            //pots
            TileID.Pots,


            //misc
            TileID.PlanteraThorns,
            TileID.Seaweed,
            TileID.LilyPad,
            TileID.Cattail,
            TileID.SeaOats,
            TileID.Cobweb,
            TileID.Pigronata,

        };

        #endregion

        #region [ BreakPlace ]
        private void OnTileEdit(object? sender, GetDataHandlers.TileEditEventArgs args)
        {
            #region code
            int tileX = args.X;
            int tileY = args.Y;
            if (!CanModify(args.Player, tileX, tileY, TileModifyType.BlockWall))
            {
                args.Player.SendTileSquareCentered(tileX, tileY, 4);
                args.Handled = true;
                return;
            }

            bool ignore = args.Action == GetDataHandlers.EditAction.KillTile &&
                breakableTiles.Contains(Main.tile[tileX, tileY].type);

            if (!ignore)
            {
                DPoint point1 = new(tileX, tileY);
                if (HouseDefineCommand_OnGoing(args.Player, point1, DPoint.Empty))
                {
                    args.Player.SendTileSquareCentered(tileX, tileY, 4);
                    args.Handled = true;
                    return;
                }
            }

            #endregion
        }
        private void OnPlaceObject(object? sender, GetDataHandlers.PlaceObjectEventArgs args)
        {
            #region code
            short x = args.X;
            short y = args.Y;
            short type = args.Type;
            short style = args.Style;

            TileObjectData tileData = TileObjectData.GetTileData(type, style, 0);
            if (tileData == null)
            {
                args.Handled = true;
                return;
            }

            x -= tileData.Origin.X;
            y -= tileData.Origin.Y;

            for (int i = x; i < x + tileData.Width; i++)
            {
                for (int j = y; j < y + tileData.Height; j++)
                {
                    if (!CanModify(args.Player, i, j, TileModifyType.BlockWall))
                    {
                        args.Player.SendTileSquareCentered(i, j, 4);
                        args.Handled = true;
                        return;
                    }
                }
            }

            #endregion
        }

        private void OnPaintTile(object? sender, GetDataHandlers.PaintTileEventArgs args)
        {
            #region code
            int tileX = args.X;
            int tileY = args.Y;
            if (!CanModify(args.Player, tileX, tileY, TileModifyType.BlockWall))
            {
                args.Player.SendTileSquareCentered(tileX, tileY, 4);
                args.Handled = true;
                return;
            }

            DPoint point1 = new(tileX, tileY);
            if (HouseDefineCommand_OnGoing(args.Player, point1, DPoint.Empty))
            {
                args.Player.SendTileSquareCentered(tileX, tileY, 4);
                args.Handled = true;
                return;
            }
            #endregion
        }

        private void OnPaintWall(object? sender, GetDataHandlers.PaintWallEventArgs args)
        {
            #region code
            int tileX = args.X;
            int tileY = args.Y;
            if (!CanModify(args.Player, tileX, tileY, TileModifyType.BlockWall))
            {
                args.Player.SendTileSquareCentered(tileX, tileY, 4);
                args.Handled = true;
                return;
            }

            DPoint point1 = new(tileX, tileY);
            if (HouseDefineCommand_OnGoing(args.Player, point1, DPoint.Empty))
            {
                args.Player.SendTileSquareCentered(tileX, tileY, 4);
                args.Handled = true;
                return;
            }
            #endregion
        }

        private void OnLiquidSet(object? sender, GetDataHandlers.LiquidSetEventArgs args)
        {
            #region code
            int tileX = args.TileX;
            int tileY = args.TileY;
            if (!CanModify(args.Player, tileX, tileY, TileModifyType.BlockWall))
            {
                args.Player.SendTileSquareCentered(tileX, tileY, 4);
                args.Handled = true;
                return;
            }

            DPoint point1 = new(tileX, tileY);
            if (HouseDefineCommand_OnGoing(args.Player, point1, DPoint.Empty))
            {
                args.Player.SendTileSquareCentered(tileX, tileY, 4);
                args.Handled = true;
                return;
            }
            #endregion
        }

        private void OnPlaceChest(object? sender, GetDataHandlers.PlaceChestEventArgs args)
        {
            #region code
            int tileX = args.TileX;
            int tileY = args.TileY;
            if (!CanModify(args.Player, tileX, tileY, TileModifyType.BlockWall))
            {
                args.Player.SendTileSquareCentered(tileX, tileY, 4);
                args.Handled = true;
                return;
            }

            DPoint point1 = new(tileX, tileY);
            if (HouseDefineCommand_OnGoing(args.Player, point1, DPoint.Empty))
            {
                args.Player.SendTileSquareCentered(tileX, tileY, 4);
                args.Handled = true;
                return;
            }
            #endregion
        }

        private void OnPlaceTileEntity(object? sender, GetDataHandlers.PlaceTileEntityEventArgs args)
        {
            #region code
            int tileX = args.X;
            int tileY = args.Y;
            if (!CanModify(args.Player, tileX, tileY, TileModifyType.BlockWall))
            {
                args.Player.SendTileSquareCentered(tileX, tileY, 4);
                args.Handled = true;
                return;
            }

            DPoint point1 = new(tileX, tileY);
            if (HouseDefineCommand_OnGoing(args.Player, point1, DPoint.Empty))
            {
                args.Player.SendTileSquareCentered(tileX, tileY, 4);
                args.Handled = true;
                return;
            }
            #endregion
        }

        public void OnMassWireOperation(object? sender, GetDataHandlers.MassWireOperationEventArgs args)
        {
            #region code
            short startX = args.StartX;
            short startY = args.StartY;
            short endX = args.EndX;
            short endY = args.EndY;

            List<Point> points = TShockAPI.Utils.Instance.GetMassWireOperationRange(
                new Point(startX, startY),
                new Point(endX, endY),
                args.Player.TPlayer.direction == 1);

            foreach (Point p in points)
            {
                if (!CanModify(args.Player, p.X, p.Y, TileModifyType.BlockWall))
                {
                    args.Handled = true;
                    return;
                }
            }

            DPoint point1 = new(args.StartX, args.StartY);
            DPoint point2 = new(args.EndX, args.EndY);
            if (HouseDefineCommand_OnGoing(args.Player, point1, point1 == point2 ? DPoint.Empty : point2))
            {
                args.Handled = true;
                return;
            }

            #endregion
        }

        #endregion

        #region [ Modify ]

        private void OnChestOpen(object? sender, GetDataHandlers.ChestOpenEventArgs args)
        {
            #region code
            int tileX = args.X;
            int tileY = args.Y;
            if (!CanModify(args.Player, tileX, tileY, TileModifyType.Container))
            {
                args.Handled = true;
                return;
            }
            #endregion
        }

        private void OnChestItemChange(object? sender, GetDataHandlers.ChestItemEventArgs args)
        {
            #region code
            int ID = args.ID;
            int Slot = args.Slot;
            Chest chest = Main.chest[ID];
            if (chest == null)
            {
                args.Handled = true;
                return;
            }
            if (!CanModify(args.Player, chest.x, chest.y, TileModifyType.Container))
            {
                args.Player.SendData(PacketTypes.ChestItem, "", ID, Slot);
                args.Handled = true;
                return;
            }
            #endregion
        }

        public void OnDisplayDollPoseSync(object? sender, GetDataHandlers.DisplayDollPoseSyncEventArgs args)
        {
            #region code
            if (!CanModify(args.Player, args.DisplayDollEntity.Position.X, args.DisplayDollEntity.Position.Y, TileModifyType.Mannequin))
            {
                args.Player.SendErrorMessage("You do not have permission to modify a Mannequin in a protected area!");
                // Note - itemIndex is unused, so it remains 0 here.
                args.Player.SendData(PacketTypes.TileEntityDisplayDollItemSync, "", 255, args.TileEntityID, 0, (int)2);//DisplayDollInventoryID.Pose
                args.Handled = true;
                return;
            }
            #endregion
        }
        public void OnDisplayDollItemSync(object? sender, GetDataHandlers.DisplayDollItemSyncEventArgs args)
        {
            #region code
            if (!CanModify(args.Player, args.DisplayDollEntity.Position.X, args.DisplayDollEntity.Position.Y, TileModifyType.Mannequin))
            {
                args.Player.SendErrorMessage("You do not have permission to modify a Mannequin in a protected area!");
                args.Handled = true;
                return;
            }
            #endregion
        }
        public void OnSignChange(object? sender, GetDataHandlers.SignEventArgs args)
        {
            #region code
            int tileX = args.X;
            int tileY = args.Y;
            if (!CanModify(args.Player, tileX, tileY, TileModifyType.Sign))
            {
                args.Player.SendData(PacketTypes.SignNew, "", args.ID);
                args.Handled = true;
                return;
            }
            #endregion
        }
        public void OnPlaceItemFrame(object? sender, GetDataHandlers.PlaceItemFrameEventArgs args)
        {
            #region code
            int tileX = args.X;
            int tileY = args.Y;

            if (!CanModify(args.Player, tileX, tileY, TileModifyType.ItemFrame))
            {
                NetMessage.SendData((int)PacketTypes.UpdateTileEntity, -1, -1, NetworkText.Empty, args.ItemFrame.ID, 0, 1);
                args.Handled = true;
                return;
            }
            #endregion
        }

        internal void OnFoodPlatterTryPlacing(object? sender, GetDataHandlers.FoodPlatterTryPlacingEventArgs args)
        {
            #region code
            int tileX = args.TileX;
            int tileY = args.TileY;

            if (!CanModify(args.Player, tileX, tileY, TileModifyType.FoodPlatter))
            {
                args.Player.SendTileSquareCentered(args.TileX, args.TileY, 1);
                args.Handled = true;
                return;
            }
            #endregion
        }

        public void OnRequestTileEntityInteraction(object? sender, RequestTileEntityInteractionEventArgs args)
        {
            #region code
            if (args.TileEntity is TEHatRack && !Check(args.TileEntity.Position.X, args.TileEntity.Position.Y, TEHatRack.entityTileWidth, TEHatRack.entityTileHeight, TileModifyType.Mannequin))
            {
                args.Handled = true;
                return;
            }
            else if (args.TileEntity is TEDisplayDoll && !Check(args.TileEntity.Position.X, args.TileEntity.Position.Y, TEDisplayDoll.entityTileWidth, TEDisplayDoll.entityTileHeight, TileModifyType.Mannequin))
            {

                args.Handled = true;
                return;
            }
            else if (!CanModify(args.Player, args.TileEntity.Position.X, args.TileEntity.Position.Y, TileModifyType.TileEntityMisc))
            {
                args.Handled = true;
                return;
            }
            bool Check(int x, int y, int width, int height, TileModifyType type)
            {
                for (int i = x; i < x + width; i++)
                {
                    for (int j = y; j < y + height; j++)
                    {
                        if (!CanModify(args.Player, i, j, type))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            #endregion
        }

        private void OnQuickStack(object? sender, OTAPI.Hooks.Chest.QuickStackEventArgs args)
        {
            #region code

            TSPlayer player = TShock.Players[args.PlayerId];

            if (player == null) { return; }

            Chest getchest = Main.chest[args.ChestIndex];

            if (getchest == null) { return; }

            if (!CanModify(player, getchest.x, getchest.y, TileModifyType.Container))
            {
                args.Result = OTAPI.HookResult.Cancel;
                return;
            }
            #endregion
        }
        #endregion

        #region [ Entity ]

        static int[] NPC_ID_BLKLST =
        {
            //ooa
            NPCID.DD2EterniaCrystal,
            NPCID.DD2LanePortal,
            NPCID.DD2Betsy,
            NPCID.DD2GoblinT1,
            NPCID.DD2GoblinT2,
            NPCID.DD2GoblinT3,
            NPCID.DD2GoblinBomberT1,
            NPCID.DD2GoblinBomberT2,
            NPCID.DD2GoblinBomberT3,
            NPCID.DD2WyvernT1,
            NPCID.DD2WyvernT2,
            NPCID.DD2WyvernT3,
            NPCID.DD2JavelinstT1,
            NPCID.DD2JavelinstT2,
            NPCID.DD2JavelinstT3,
            NPCID.DD2DarkMageT1,
            NPCID.DD2DarkMageT3,
            NPCID.DD2SkeletonT1,
            NPCID.DD2SkeletonT3,
            NPCID.DD2WitherBeastT2,
            NPCID.DD2WitherBeastT3,
            NPCID.DD2DrakinT2,
            NPCID.DD2DrakinT3,
            NPCID.DD2KoboldWalkerT2,
            NPCID.DD2KoboldWalkerT3,
            NPCID.DD2KoboldFlyerT2,
            NPCID.DD2KoboldFlyerT3,
            NPCID.DD2OgreT2,
            NPCID.DD2OgreT3,
            NPCID.DD2LightningBugT3,

            //main
            NPCID.SkeletonMerchant,

            NPCID.KingSlime,
            NPCID.SlimeSpiked,

            NPCID.EyeofCthulhu,
            NPCID.ServantofCthulhu,

            NPCID.EaterofWorldsHead,
            NPCID.EaterofWorldsBody,
            NPCID.EaterofWorldsTail,

            NPCID.BrainofCthulhu,
            NPCID.Creeper,

            NPCID.Deerclops,

            NPCID.QueenBee,
            NPCID.Bee,
            NPCID.BeeSmall,


            NPCID.SkeletronHead,
            NPCID.SkeletronHand,
            NPCID.DarkCaster,
            NPCID.WaterSphere,

            NPCID.DungeonGuardian,

            NPCID.WallofFlesh,
            NPCID.WallofFleshEye,
            NPCID.LeechTail,
            NPCID.LeechBody,
            NPCID.LeechHead,
            NPCID.TheHungry,
            NPCID.TheHungryII,

            NPCID.Retinazer,
            NPCID.Spazmatism,

            NPCID.TheDestroyer,
            NPCID.TheDestroyerBody,
            NPCID.TheDestroyerTail,
            NPCID.Probe,

            NPCID.SkeletronPrime,
            NPCID.PrimeCannon,
            NPCID.PrimeLaser,
            NPCID.PrimeSaw,
            NPCID.PrimeVice,


            NPCID.QueenSlimeBoss,
            NPCID.QueenSlimeMinionBlue,
            NPCID.QueenSlimeMinionPink,
            NPCID.QueenSlimeMinionPurple,

            NPCID.DukeFishron,
            NPCID.DetonatingBubble,
            NPCID.Sharkron,
            NPCID.Sharkron2,

            NPCID.Plantera,
            NPCID.PlanterasHook,
            NPCID.PlanterasTentacle,
            NPCID.Spore,

            NPCID.Golem,
            NPCID.GolemFistLeft,
            NPCID.GolemFistRight,
            NPCID.GolemHead,
            NPCID.GolemHeadFree,

            NPCID.MartianSaucerCannon,
            NPCID.MartianSaucerTurret,
            NPCID.MartianSaucerCore,
            NPCID.MartianSaucer,

            NPCID.CultistBoss,
            NPCID.CultistBossClone,
            NPCID.CultistDragonHead,
            NPCID.CultistDragonBody1,
            NPCID.CultistDragonBody2,
            NPCID.CultistDragonBody3,
            NPCID.CultistDragonBody4,
            NPCID.CultistDragonTail,
            NPCID.AncientCultistSquidhead,
            NPCID.AncientLight,
            NPCID.AncientDoom,

            NPCID.LunarTowerSolar,
            NPCID.SolarGoop,
            NPCID.LunarTowerNebula,
            NPCID.LunarTowerStardust,
            NPCID.LunarTowerVortex,

            NPCID.MoonLordFreeEye,
            NPCID.MoonLordLeechBlob,
        };

        private static void OnNPCAIUpdate(NpcAiUpdateEventArgs args)
        {

            if (!args.Npc.active) { return; }


            if (args.Npc.boss) return;

            if (NPC_ID_BLKLST.Contains(args.Npc.type)) return;

            if (args.Npc.CountsAsACritter || args.Npc.townNPC) return;

            var regions = TShock.Regions.InAreaRegionID((int)args.Npc.position.X / 16, (int)args.Npc.position.Y / 16);

            foreach (int get in regions)
            {
                if (MainDBManager.TryGetRegionKLPByID(get, out RegionKLP regionklp))
                {
                    if (regionklp.RegionType.Contains(HouseWarding))
                    {
                        args.Npc.active = false;
                        args.Npc.type = 0;
                        TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", args.Npc.whoAmI);
                        Projectile.NewProjectile(Projectile.GetNoneSource(), args.Npc.position.X, args.Npc.position.Y, 0f, -0f, ProjectileID.PrincessWeapon, 0, 0);
                        return;
                    }
                }
            }
            
        }

        #endregion

        private static void OnReload(ReloadEventArgs args)
        {
            #region code
            MainKLP.Config = Config.Read();

            SyncRegionAllowStrData(MainKLP.Config);

            args.Player.SendInfoMessage("[RegionsKLP] Config reloaded!");

            MainDBManager.RegionKLPSync();
            args.Player.SendInfoMessage("[RegionsKLP] Unique regions synced!");
            #endregion
        }


        public enum TileModifyType
        {
            BlockWall,
            Container,//chest, dressers, storage tiles etc...
            Mannequin,
            Sign,
            ItemFrame,
            ItemRack,
            ItemJar,
            FoodPlatter,
            TileEntityMisc
        }

        public bool CanModify(TSPlayer player, int TileX, int TileY, TileModifyType modifytype)
        {
            var getregions = TShock.Regions.InAreaRegion(TileX, TileY);

            if (player.HasPermission(Permissions.editregion)) { return true; }
            switch (modifytype)
            {
                case TileModifyType.Container:
                    {

                        foreach (Region get in getregions)
                        {
                            if (!IsUniqueRegion(get.Name)) { continue; }

                            if (RegionAllow_Chest.Contains(get.Name)) { continue; }
                            if (RegionAllowStartsWith_Chest.Any(r => get.Name.StartsWith(r))) { continue; }

                            if (!get.AllowedIDs.Contains(player.Account.ID) && !get.AllowedGroups.Contains(player.Group.Name))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                case TileModifyType.Mannequin:
                    {

                        foreach (Region get in getregions)
                        {
                            if (!IsUniqueRegion(get.Name)) { continue; }

                            if (RegionAllow_Mannequin.Contains(get.Name)) { continue; }
                            if (RegionAllowStartsWith_Mannequin.Any(r => get.Name.StartsWith(r))) { continue; }

                            if (!get.AllowedIDs.Contains(player.Account.ID) && !get.AllowedGroups.Contains(player.Group.Name))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                case TileModifyType.Sign:
                    {

                        foreach (Region get in getregions)
                        {
                            if (!IsUniqueRegion(get.Name)) { continue; }

                            if (RegionAllow_Sign.Contains(get.Name)) { continue; }
                            if (RegionAllowStartsWith_Sign.Any(r => get.Name.StartsWith(r))) { continue; }

                            if (!get.AllowedIDs.Contains(player.Account.ID) && !get.AllowedGroups.Contains(player.Group.Name))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                case TileModifyType.ItemFrame:
                    {

                        foreach (Region get in getregions)
                        {
                            if (!IsUniqueRegion(get.Name)) { continue; }

                            if (RegionAllow_ItemFrame.Contains(get.Name)) { continue; }
                            if (RegionAllowStartsWith_ItemFrame.Any(r => get.Name.StartsWith(r))) { continue; }

                            if (!get.AllowedIDs.Contains(player.Account.ID) && !get.AllowedGroups.Contains(player.Group.Name))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                case TileModifyType.ItemRack:
                    {

                        foreach (Region get in getregions)
                        {
                            if (!IsUniqueRegion(get.Name)) { continue; }

                            if (RegionAllow_ItemRack.Contains(get.Name)) { continue; }
                            if (RegionAllowStartsWith_ItemRack.Any(r => get.Name.StartsWith(r))) { continue; }

                            if (!get.AllowedIDs.Contains(player.Account.ID) && !get.AllowedGroups.Contains(player.Group.Name))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                case TileModifyType.ItemJar:
                    {

                        foreach (Region get in getregions)
                        {
                            if (!IsUniqueRegion(get.Name)) { continue; }

                            if (RegionAllow_ItemJar.Contains(get.Name)) { continue; }
                            if (RegionAllowStartsWith_ItemJar.Any(r => get.Name.StartsWith(r))) { continue; }

                            if (!get.AllowedIDs.Contains(player.Account.ID) && !get.AllowedGroups.Contains(player.Group.Name))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                case TileModifyType.FoodPlatter:
                    {

                        foreach (Region get in getregions)
                        {
                            if (!IsUniqueRegion(get.Name)) { continue; }

                            if (RegionAllow_FoodPlatter.Contains(get.Name)) { continue; }
                            if (RegionAllowStartsWith_FoodPlatter.Any(r => get.Name.StartsWith(r))) { continue; }

                            if (!get.AllowedIDs.Contains(player.Account.ID) && !get.AllowedGroups.Contains(player.Group.Name))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                case TileModifyType.TileEntityMisc:
                    {

                        foreach (Region get in getregions)
                        {
                            if (!IsUniqueRegion(get.Name)) { continue; }

                            if (RegionAllow_TileEntityMisc.Contains(get.Name)) { continue; }
                            if (RegionAllowStartsWith_TileEntityMisc.Any(r => get.Name.StartsWith(r))) { continue; }

                            if (!get.AllowedIDs.Contains(player.Account.ID) && !get.AllowedGroups.Contains(player.Group.Name))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                case TileModifyType.BlockWall:
                default:
                    {
                        foreach (Region get in getregions)
                        {
                            if (!IsUniqueRegion(get.Name)) { continue; }

                            if (!get.AllowedIDs.Contains(player.Account.ID) && !get.AllowedGroups.Contains(player.Group.Name))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
            }
        }



        #region [ House Define ]

        public const int DefaultAbortTimeOut_Seconds = 60;
        public static (Define define, System.Threading.Timer timer)?[] OnCreateHouse = new (Define define, System.Threading.Timer timer)?[Main.player.Length];
        public static void AddOnCreateHouse(TSPlayer Executer, int Seconds = DefaultAbortTimeOut_Seconds)
        {
            System.Threading.Timer timer = null;
            timer =  new(state => {
                RemoveOnCreateHouse(Executer, true);

                // ReSharper disable AccessToModifiedClosure
                Debug.Assert(timer != null);
                timer.Dispose();
                // ReSharper restore AccessToModifiedClosure
            },
              null, Seconds * 1000, Timeout.Infinite
            );
            OnCreateHouse[Executer.Index] = new(new Define(), timer);
        }
        public static void RemoveOnCreateHouse(TSPlayer Executer, bool sendMessage = false)
        {
            var data = OnCreateHouse[Executer.Index];

            if (data == null)
                return;

            data.Value.timer.Dispose();

            OnCreateHouse[Executer.Index] = null;

            if (sendMessage)
            {
                try { Executer.SendErrorMessage("House define has been aborted. (Timeout)"); } catch { }
            }
        }
        public class Define
        {
            public DPoint point1 = DPoint.Empty;
            public DPoint point2 = DPoint.Empty;
            public Rectangle Area = Rectangle.Empty;

            public Define()
            {

            }
        }

        public bool HouseDefineCommand_OnGoing(TSPlayer Executer, DPoint point1, DPoint point2, bool removepoint = false)
        {
            if (OnCreateHouse[Executer.Index] == null) { return false; }

            (Define define, System.Threading.Timer timer) gdata = OnCreateHouse[Executer.Index].Value;

            bool D2points = point2 != DPoint.Empty;

            if (removepoint)
            {
                DPoint mainpoint = point2 != DPoint.Empty ? point2 : point1;

                if (gdata.define.point1 == mainpoint)
                {
                    if (gdata.define.Area != Rectangle.Empty)
                        Indicator.SendAreaDottedFakeTiles(Executer, gdata.define.Area, false);

                    if (gdata.define.point2 != DPoint.Empty)
                    {
                        Indicator.SendFakeTilesCross(Executer, gdata.define.point2, false);
                        gdata.define.point2 = DPoint.Empty;
                    } else
                    {
                        Indicator.SendFakeTilesCross(Executer, gdata.define.point1, false);
                        gdata.define.point1 = DPoint.Empty;
                    }

                    SendInstruction(countPoint());
                    return true;
                }
                else if (gdata.define.point2 == mainpoint)
                {
                    Indicator.SendFakeTilesCross(Executer, gdata.define.point2, false);
                    gdata.define.point2 = DPoint.Empty;

                    if (gdata.define.Area != Rectangle.Empty)
                        Indicator.SendAreaDottedFakeTiles(Executer, gdata.define.Area, false);

                    SendInstruction(countPoint());
                    return true;
                }
                return true;
            } else
            {
                if (gdata.define.point1 == DPoint.Empty && gdata.define.point2 == DPoint.Empty && D2points)
                {
                    gdata.define.point1 = point1;
                    gdata.define.point2 = point2;

                    gdata.define.Area = new Rectangle(
                      Math.Min(gdata.define.point1.X, gdata.define.point2.X), Math.Min(gdata.define.point1.Y, gdata.define.point2.Y),
                      Math.Abs(gdata.define.point1.X - gdata.define.point2.X), Math.Abs(gdata.define.point1.Y - gdata.define.point2.Y)
                    );
                    Indicator.SendAreaDottedFakeTiles(Executer, gdata.define.Area, TileColor: PaintID.DeepYellowPaint);

                    SendInstruction(3);
                }
                else if (gdata.define.point1 == DPoint.Empty || gdata.define.point2 == DPoint.Empty)
                {
                    if (gdata.define.point1 == DPoint.Empty)
                        gdata.define.point1 = point1;
                    else
                        gdata.define.point2 = point1;

                    Indicator.SendFakeTilesCross(Executer, gdata.define.point1, TileColor: PaintID.DeepLimePaint);

                    if (gdata.define.point1 != DPoint.Empty && gdata.define.point2 != DPoint.Empty)
                    {
                        gdata.define.Area = new Rectangle(
                          Math.Min(gdata.define.point1.X, gdata.define.point2.X), Math.Min(gdata.define.point1.Y, gdata.define.point2.Y),
                          Math.Abs(gdata.define.point1.X - gdata.define.point2.X), Math.Abs(gdata.define.point1.Y - gdata.define.point2.Y)
                        );
                        Indicator.SendAreaDottedFakeTiles(Executer, gdata.define.Area, TileColor: PaintID.DeepYellowPaint);

                        SendInstruction(3);
                    }
                    else
                    {
                        if (gdata.define.point2 == DPoint.Empty)
                        {
                            SendInstruction(2);
                        }
                        else
                        {
                            SendInstruction(1);
                        }
                    }
                    return true;
                }
                else
                {
                    // Final Mark
                    Indicator.SendFakeTilesCross(Executer, gdata.define.point1, false);
                    Indicator.SendFakeTilesCross(Executer, gdata.define.point2, false);
                    Indicator.SendAreaDottedFakeTiles(Executer, gdata.define.Area, false);
                    Indicator.SendFakeTile(Executer, point1, false);

                    DPoint mainpoint = point2 != DPoint.Empty ? point2 : point1;

                    if (
                      mainpoint.X >= gdata.define.Area.Left && mainpoint.X <= gdata.define.Area.Right &&
                      mainpoint.Y >= gdata.define.Area.Top && mainpoint.Y <= gdata.define.Area.Bottom
                    )
                    {
                        if (gdata.define.Area.Width <= 0 || gdata.define.Area.Height <= 0)
                        {
                            Executer.SendErrorMessage("The house has to be at least one block high and wide.");
                        }
                        else if (HouseKLP.CreateHouseRegion(Executer, gdata.define.Area, out string msg, out Color clr, true, true))
                        {
                            Executer.SendMessage(msg, clr);
                        } else
                        {
                            Executer.SendMessage(msg, clr);
                        }
                        RemoveOnCreateHouse(Executer);
                    }
                    else
                    {
                        Executer.SendWarningMessage("Defining of house was aborted.");
                        RemoveOnCreateHouse(Executer);
                    }

                    return true;
                }
                return true;
            }

            int countPoint()
            {
                return 1 + (gdata.define.point1 != DPoint.Empty ? 1 : 0) + (gdata.define.point2 != DPoint.Empty ? 1 : 0);
            }

            // 1 = first mark | 2 = 2nd mark | 3 = final mark
            void SendInstruction(int marks)
            {
                switch (marks)
                {
                    case 1:
                        {
                            Executer.SendMessage( TShock.Utils.ColorTag("First Mark", Color.IndianRed) + "\n" +
                                "Mark the top left tile of your house by interact any blocks" + "\n" +
                                "or by altering the tile otherwise.", Color.MediumSpringGreen);
                            return;
                        }
                    case 2:
                        {
                            Executer.SendMessage(TShock.Utils.ColorTag("Second Mark", Color.IndianRed) + "\n" +
                                "Mark the bottom right tile of your house by interact any blocks" + "\n" +
                                "or by altering the tile otherwise.", Color.MediumSpringGreen);
                            return;
                        }
                    case 3:
                        {

                            Executer.SendMessage(TShock.Utils.ColorTag("Final Mark", Color.IndianRed) + "\n" +
                                "Mark any point inside your house to accept, or any point outside the house to cancel."
                                , Color.MediumSpringGreen);
                            return;
                        }
                    case -1:
                        {
                            //playerLocal.SendMessage("House was successfully created. Other players can no longer change blocks", Color.MediumSpringGreen);
                            //playerLocal.SendMessage("inside the defined house region.", Color.MediumSpringGreen);
                            return;
                        }
                }
            }
        }

        #endregion


        public static bool IsUniqueRegion(string regionName)
        {
            if (regionName.StartsWith(Config.Main.Housing.Indicator)) { return false; }

            if (Config.Main.uniqueRegionsName.Any(r => r.NameText == regionName)) { return true; }

            if (Config.Main.uniqueRegionsName_StartsWith.Any(r => regionName.StartsWith(r.NameText))) { return true; }

            return false;
        }

    }
}
