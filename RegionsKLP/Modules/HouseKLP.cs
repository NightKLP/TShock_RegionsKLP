using Microsoft.Xna.Framework;
using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Asn1.X509;
using ReLogic.Peripherals.RGB.Logitech;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Biomes.CaveHouse;
using TShockAPI;
using TShockAPI.DB;

namespace RegionsKLP.Modules
{
    public static class HouseKLP
    {

        #region [ Get ]
        public static bool TryGetHouseRegionAtPlayer(TSPlayer player, out int ownerid, out string housename, out RegionKLP housedata, out Region region)
        {
            if (player == null) throw new ArgumentNullException();

            if (!TryGetHouseRegionAtPlayer(player, out ownerid, out housename, out housedata, out region))
            {
                return false;
            }

            return true;
        }
        public static bool TryGetHouseRegionAtPlayer(TSPlayer player, out int ownerid, out RegionKLP houseData, out Region region)
        {
            if (player == null)
            {
                ownerid = -1;
                houseData = null;
                region = null;
                return false;
            }

            for (int i = 0; i < TShock.Regions.Regions.Count; i++)
            {
                if (TShock.Regions.Regions[i].WorldID != Main.worldID.ToString()) continue;
                region = TShock.Regions.Regions[i];
                if (region.InArea(player.TileX, player.TileY) && TryGetHouseRegionKLP(region, out ownerid, out houseData))
                {

                    return true;
                }
            }

            ownerid = -1;
            houseData = null;
            region = null;
            return false;
        }

        public static bool TryGetHouseRegionKLP(Region region, out int ownerid, out RegionKLP HouseData)
        {
            ownerid = -1;
            HouseData = null;
            if (region == null) { return false; }

            RegionKLP regionklp;
            if (!MainKLP.MainDBManager.TryGetRegionKLPByID(region.ID, out regionklp)) { return false; }

            if (!MainKLP.MainDBManager.NameUserIDCache.ContainsKey(region.Owner)) { return false; }

            ownerid = MainKLP.MainDBManager.NameUserIDCache[region.Owner];
            HouseData = regionklp;
            return true;
        }

        public static bool IsHouseRegion(Region region)
        {
            return TryGetHouseRegionKLP(region, out _, out _);
        }



        public static bool TryGetAccessibleHouseRegionAtPlayer(TSPlayer player, out int owner, out Region region)
        {
            if (player == null) throw new ArgumentNullException();

            if (!TryGetHouseRegionAtPlayer(player, out owner, out _, out region))
                return false;

            if (player.Account.ID != owner && !player.Group.HasPermission(MainKLP.Config.Permissions.HouseKLP_Admin))
            {
                player.SendErrorMessage("You're not the owner of this house.");
                return false;
            }

            return true;
        }

        public static bool TryGetAccessibleHouseRegionAtPlayer(TSPlayer player, out int owner, out RegionKLP housedata, out Region region)
        {
            if (player == null) throw new ArgumentNullException();

            if (!TryGetHouseRegionAtPlayer(player, out owner, out housedata, out region))
                return false;

            if (player.Account.ID != owner && !player.Group.HasPermission(MainKLP.Config.Permissions.HouseKLP_Admin))
            {
                player.SendErrorMessage("You're not the owner of this house.");
                return false;
            }

            return true;
        }
        public static bool TryGetAccessibleHouseRegionAtPlayer(TSPlayer player, out int owner, out string housename, out RegionKLP housedata, out Region region)
        {
            if (player == null) throw new ArgumentNullException();

            if (!TryGetHouseRegionAtPlayer(player, out owner, out housename, out housedata, out region))
                return false;

            if (player.Account.ID != owner && !player.Group.HasPermission(MainKLP.Config.Permissions.HouseKLP_Admin))
            {
                player.SendErrorMessage("You're not the owner of this house.");
                return false;
            }

            return true;
        }

        private static bool TryGetAccessibleHouseRegionAtPlayer(TSPlayer player, out RegionKLP housedata, out Region region)
        {
            return TryGetAccessibleHouseRegionAtPlayer(player, out _, out housedata, out region);
        }
        private static bool TryGetAccessibleHouseRegionAtPlayer(TSPlayer player, out Region region)
        {
            return TryGetAccessibleHouseRegionAtPlayer(player, out _, out _, out region);
        }

        #endregion

        public static string GetExplainInvalidRegionSizeText(PlayerHousingKLP userdata, Rectangle newarea, bool IsSmall)
        {
            if (IsSmall)
            {
                return "This region has no valid house size, it's too small:" +
                    $"\nMin width: {userdata.Get_MinTileWidth()} (you've tried to set {newarea.Width})." +
                    $"\nMin height: {userdata.Get_MinTileHeight()} (you've tried to set {newarea.Height})." +
                    $"\nMin total blocks: {userdata.Get_MinTotalTile()} (you've tried to set {newarea.Width * newarea.Height}).";
            }
            else
            {
                return "This region has no valid house size, it's too large:" +
                    $"\nMin width: {userdata.Get_MaxTileWidth()} (you've tried to set {newarea.Width})." +
                    $"\nMin height: {userdata.Get_MaxTileHeight()} (you've tried to set {newarea.Height})." +
                    $"\nMin total blocks: {userdata.Get_MaxTotalTile()} (you've tried to set {newarea.Width * newarea.Height}).";
            }
        }

        #region [ Create ]
        public static bool CreateHouseRegion(TSPlayer player, Rectangle area, out string msg, out Color clr, bool checkOverlaps = true, bool checkPermissions = false, bool checkDefinePermission = false)
        {
            if (player == null)
            {
                clr = Color.Red;
                msg = "unable to get your playerdata!";
                return false;
            }
            if (!player.IsLoggedIn)
            {
                clr = Color.Red;
                msg = "must be logged in to get your account data!";
                return false;
            }

            return CreateHouseRegion(player.Account, player.Group, area, out msg, out clr, checkOverlaps, checkPermissions, checkDefinePermission);
        }
        public static bool CreateHouseRegion(UserAccount user, Group group, Rectangle area, out string msg, out Color clr, bool checkOverlaps = true, bool checkPermissions = false, bool checkDefinePermission = false)
        {
            clr = Color.Red;
            if (user == null) { msg = "could not get user account!"; return false; }
            if (group == null) { msg = "could not get group data!"; return false; }
            if (!(area.Width > 0 && area.Height > 0)) { msg = "invalid housing area!"; return false; }


            if (checkPermissions)
            {
                if (!group.HasPermission(MainKLP.Config.Permissions.HouseKLP_Define))
                {
                    msg = "you do not have permission to define a house";
                    return false;
                }

                if (!group.HasPermission(MainKLP.Config.Permissions.HouseKLP_SlotsNoLimits))
                {
                    PlayerHousingKLP userdata;
                    if (!MainKLP.MainDBManager.TryGetPlayerHouseDataByAccountID(user.ID, out userdata))
                    {
                        msg = "could not get user datahouse!";
                        return false;
                    }

                    if (!CheckHouseRegionValidSize(area, userdata, out bool IsSmall))
                    {
                        msg = GetExplainInvalidRegionSizeText(userdata, area, IsSmall);
                        return false;
                    }
                }
            }

            if (checkOverlaps && CheckHouseRegionOverlap(user.ID, area))
            {
                msg = "The house would overlap with another house where you're not the owner of" +
                    ((bool)MainKLP.Config.Main.Housing.AllowTShockRegionOverlapping ? "." : "or\nit overlaps with a TShock region.");
                return false;
            }

            // Find a free house index.
            int houseIndex;
            string regionName = null;
            for (houseIndex = 1; houseIndex <= int.MaxValue; houseIndex++)
            {
                regionName =  $"{MainKLP.Config.Main.Housing.Indicator}_{user.Name}_house{houseIndex}";
                if (TShock.Regions.GetRegionByName(regionName) == null)
                    break;
            }
            if (HasReachedMaxHouseSlot(user.ID))
            {
                msg = "You have reached the maximum of {0} houses. Delete at least one of your other houses first.";
                return false;
            }

            if (!TShock.Regions.AddRegion(
                area.X, area.Y, area.Width, area.Height, regionName, user.Name, Main.worldID.ToString(),
                (int)MainKLP.Config.Main.Housing.DefaultPrefix
            ))
            {
                msg = "Unable to claim your area.";
                return false;
            }

            Region getregion = TShock.Regions.GetRegionByName(regionName);

            MainKLP.MainDBManager.CreateRegionKLP(getregion.ID, $"house{houseIndex}", "");

            msg = "House was successfully created. Other players can no longer change blocks inside the defined house region.";
            clr = Color.MediumSpringGreen;
            return true;
        }

        #endregion

        public static bool SyncHouseRegionsWithOwnerID(int ownerid, string newownername)
        {
            var getregions = GetHouseRegionByOwnerID(ownerid);

            foreach (var get in getregions)
            {
                TShock.DB.Query("UPDATE Regions SET Owner=@0 WHERE RegionName=@1 AND WorldID=@2", newownername, get.Name, Main.worldID.ToString());
                TShock.Regions.ChangeOwner(get.Name, newownername);
                get.Owner = newownername;
                return true;
            }
            return false;
        }

        public static List<Region> GetHouseRegionByOwnerID(int ownerid)
        {
            List<Region> result = new();

            if (!MainKLP.MainDBManager.UserIDNameCache.ContainsKey(ownerid)) { return result; }
            string ownername = MainKLP.MainDBManager.UserIDNameCache[ownerid];

            foreach (Region get in TShock.Regions.Regions)
            {
                if (get.WorldID != Main.worldID.ToString()) continue;
                if (get.Owner != ownername) continue;
                if (get.Name.StartsWith(MainKLP.Config.Main.Housing.Indicator))
                {
                    result.Add(get);
                }
            }

            return result;
        }
        public static bool GetHouseRegionByOwnerIDAndHouseName(int ownerid, string housename, out Region region)
        {
            if (!MainKLP.MainDBManager.UserIDNameCache.ContainsKey(ownerid))
            {
                region = null;
                return false;
            }
            string ownername = MainKLP.MainDBManager.UserIDNameCache[ownerid];

            foreach (Region get in TShock.Regions.Regions)
            {
                if (get.WorldID != Main.worldID.ToString()) continue;
                if (get.Owner != ownername) continue;
                if (get.Name.StartsWith(MainKLP.Config.Main.Housing.Indicator))
                {
                    RegionKLP gethousedata;
                    if (!MainKLP.MainDBManager.TryGetRegionKLPByID(get.ID, out gethousedata)) { continue; }

                    if (gethousedata.SubName != housename) { continue; }

                    region = get;
                    return true;
                }
            }

            region = null;
            return false;
        }

        #region [ Modify ]
        public static bool SetHouseRegionOwner(Region region, int newOwnerID)
        {
            if (region == null) { return false; }
            if (newOwnerID == -1) { return false; }

            int currentOwner;
            RegionKLP housedata;
            if (!TryGetHouseRegionKLP(region, out currentOwner, out housedata)) { return false; }

            if (currentOwner == newOwnerID) { return false; }

            if (CheckHouseIfItExistByOwnerID(newOwnerID, housedata.SubName))
            {
                return false;
            }

            if (!MainKLP.MainDBManager.UserIDNameCache.ContainsKey(newOwnerID)) { return false; }
            string newownername = MainKLP.MainDBManager.UserIDNameCache[newOwnerID];

            TShock.DB.Query("UPDATE Regions SET UserIds=@0,`Groups`=@1,Owner=@2 WHERE RegionName=@3 AND WorldID=@4", "", "", newownername, region.Name, Main.worldID.ToString());
            region.AllowedIDs = new();
            region.AllowedGroups = new();
            region.Owner = newownername;
            return true;
        }

        public static bool RenameHouseRegionWithOwner(int ownerid, RegionKLP regionklp, string newname)
        {
            return RenameHouseRegionWithOwner(ownerid, regionklp, newname, out _);
        }

        public static bool RenameHouseRegionWithOwner(int ownerid, RegionKLP regionklp, string newname, out bool hasOwnDuplicateName)
        {
            Region region;
            if (!regionklp.TryGetRegion(out region))
            {
                hasOwnDuplicateName = false;
                return false;
            }

            if (CheckHouseIfItExistByOwnerID(ownerid, newname))
            {
                hasOwnDuplicateName = true;
                return false;
            }


            hasOwnDuplicateName = false;
            return regionklp.ChangeSubName(newname);
        }

        #endregion

        #region [ Check ]

        public static bool HasReachedMaxHouseSlot(int AccountID)
        {
            PlayerHousingKLP userdata;
            if (!MainKLP.MainDBManager.TryGetPlayerHouseDataByAccountID(AccountID, out userdata))
            {
                return false;
            }

            if (!MainKLP.MainDBManager.UserIDNameCache.ContainsKey(AccountID)) { return true; }
            string ownername = MainKLP.MainDBManager.UserIDNameCache[AccountID];

            int count = 0;
            foreach (Region get in TShock.Regions.Regions)
            {
                if (get.WorldID != Main.worldID.ToString()) continue;
                if (get.Owner != ownername) continue;
                if (get.Name.StartsWith(MainKLP.Config.Main.Housing.Indicator))
                {
                    count++;
                }
            }

            return userdata.Get_MaxHouseSlot() <= count;
        }
        public static bool CheckHouseIfItExistByOwnerID(int ownerid, string housename)
        {
            foreach (Region check in GetHouseRegionByOwnerID(ownerid))
            {
                RegionKLP gethousedata;
                if (!MainKLP.MainDBManager.TryGetRegionKLPByID(check.ID, out gethousedata)) { continue; }

                if (gethousedata.SubName == housename)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CheckHouseRegionOverlap(int ownerid, Rectangle regionArea)
        {
            for (int i = 0; i < TShock.Regions.Regions.Count; i++)
            {
                Region tsRegion = TShock.Regions.Regions[i];
                if (
                    regionArea.Right < tsRegion.Area.Left || regionArea.X > tsRegion.Area.Right ||
                    regionArea.Bottom < tsRegion.Area.Top || regionArea.Y > tsRegion.Area.Bottom
                )
                    continue;

                int houseOwnerID;
                if (!TryGetHouseRegionKLP(tsRegion, out houseOwnerID, out _))
                {
                    if ((bool)MainKLP.Config.Main.Housing.AllowTShockRegionOverlapping || tsRegion.Name.StartsWith("*"))
                        continue;

                    return true;
                }
                if (houseOwnerID == ownerid)
                    continue;

                return true;
            }

            return false;
        }

        public static bool CheckHouseRegionValidSize(Rectangle regionArea, PlayerHousingKLP AccountData, out bool IsSmall)
        {
            int areaTotalTiles = regionArea.Width * regionArea.Height;

            if (
                regionArea.Width < AccountData.Get_MinTileWidth() || regionArea.Height < AccountData.Get_MinTileHeight() ||
                areaTotalTiles < AccountData.Get_MinTotalTile()
            )
            {
                IsSmall = true;
                return false;
            }

            if (
                regionArea.Width > AccountData.Get_MaxTileWidth() || regionArea.Height > AccountData.Get_MaxTileHeight() ||
                areaTotalTiles > AccountData.Get_MaxTotalTile()
            )
            {
                IsSmall = false;
                return false;
            }

            IsSmall = false;
            return true;
        }

        public static bool CheckHouseRegionValidSize(Rectangle regionArea, PlayerHousingKLP AccountData)
        {
            return CheckHouseRegionValidSize(regionArea, AccountData, out _);
        }

        #endregion


    }
}
