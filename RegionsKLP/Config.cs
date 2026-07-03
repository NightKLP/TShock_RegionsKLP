using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace RegionsKLP
{
    public class Config
    {
        public CONFIG_MAIN Main;
        public CONFIG_PERMISSIONS Permissions;
        public CONFIG_DB DB;

        static string path = Path.Combine(TShock.SavePath, "RegionsKLP_Config.json");

        public static Config Read()
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(Default(), Formatting.Indented));
                return Default();
            }

            var args = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));

            if (args == null) return Default();

            if (args.Main == null) args.Main = new();
            args.Main.FixNull();

            if (args.Permissions == null) args.Permissions = new();
            args.Permissions.FixNull();

            if (args.DB == null) args.DB = new();
            args.DB.FixNull();

            File.WriteAllText(path, JsonConvert.SerializeObject(args, Formatting.Indented));
            return args;
        }

        /// <summary>
        /// changes config file
        /// </summary>
        /// <param name="config"></param>
        public void Changeall()
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(Default(), Formatting.Indented));
            }
            else
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
        }

        private static Config Default()
        {
            return new Config()
            {
                Main = new CONFIG_MAIN(),
                Permissions = new CONFIG_PERMISSIONS(),
                DB = new CONFIG_DB(),
            };
        }
    }

    #region [ Config Objects ]

    public struct UniqueRegionType
    {
        public string NameText;
        public bool CanEdit_Chest;
        public bool CanEdit_Mannequin;
        public bool CanEdit_Sign;
        public bool CanEdit_ItemFrame;
        public bool CanEdit_ItemRack;
        public bool CanEdit_ItemJar;
        public bool CanEdit_TileEntityMisc;
        public bool CanEdit_FoodPlatter;

        public UniqueRegionType(
            string NameText,
            bool CanEdit_Chest,
            bool CanEdit_Mannequin,
            bool CanEdit_Sign,
            bool CanEdit_ItemFrame,
            bool CanEdit_ItemRack,
            bool CanEdit_ItemJar,
            bool CanEdit_TileEntityMisc,
            bool CanEdit_FoodPlatter)
        {
            this.NameText = NameText;
            this.CanEdit_Chest = CanEdit_Chest;
            this.CanEdit_Mannequin = CanEdit_Mannequin;
            this.CanEdit_Sign = CanEdit_Sign;
            this.CanEdit_ItemFrame = CanEdit_ItemFrame;
            this.CanEdit_ItemRack = CanEdit_ItemRack;
            this.CanEdit_ItemJar = CanEdit_ItemJar;
            this.CanEdit_TileEntityMisc = CanEdit_TileEntityMisc;
            this.CanEdit_FoodPlatter = CanEdit_FoodPlatter;
        }
    }

    public class CONFIG_MAIN
    {

        public UniqueRegionType[] uniqueRegionsName = { new UniqueRegionType("Dumpster", true, false, false, false, false, false, false, false) };

        public UniqueRegionType[] uniqueRegionsName_StartsWith = { new UniqueRegionType("CommunitySigns=", false, false, true, false, false, false, false, false) };

        public CONFIG_Housing Housing = new();
        public CONFIG_MAIN() { }

        public void FixNull()
        {
            CONFIG_MAIN getdefault = new();

            if (uniqueRegionsName == null) uniqueRegionsName = getdefault.uniqueRegionsName;

            if (uniqueRegionsName_StartsWith == null) uniqueRegionsName_StartsWith = getdefault.uniqueRegionsName_StartsWith;

            if (Housing == null) { Housing = getdefault.Housing; } else { Housing.FixNull(); }
        }
    }

    public class CONFIG_Housing
    {
        public string Indicator = "*H*";
        public int? DefaultPrefix = 1;
        public bool? AllowTShockRegionOverlapping = false;



        public int? MaxHouseDefault = 1;

        public int? MinTileWidthDefault = 5;
        public int? MinTileHeightDefault = 5;
        public int? MinTotalTileDefault = 30;

        public int? MaxTileWidthDefault = 100;
        public int? MaxTileHeightDefault = 80;
        public int? MaxTotalTileDefault = 7000;

        public CONFIG_Housing() { }

        public void FixNull()
        {
            CONFIG_Housing getdefault = new();

            if (Indicator == null) Indicator = getdefault.Indicator;
            if (DefaultPrefix == null) DefaultPrefix = getdefault.DefaultPrefix;
            if (AllowTShockRegionOverlapping == null) AllowTShockRegionOverlapping = getdefault.AllowTShockRegionOverlapping;

            if (MaxHouseDefault == null) MaxHouseDefault = getdefault.MaxHouseDefault;

            if (MinTileWidthDefault == null) MinTileWidthDefault = getdefault.MinTileWidthDefault;
            if (MinTileHeightDefault == null) MinTileHeightDefault = getdefault.MinTileHeightDefault;
            if (MinTotalTileDefault == null) MinTotalTileDefault = getdefault.MinTotalTileDefault;

            if (MaxTileWidthDefault == null) MaxTileWidthDefault = getdefault.MaxTileWidthDefault;
            if (MaxTileHeightDefault == null) MaxTileHeightDefault = getdefault.MaxTileHeightDefault;
            if (MaxTotalTileDefault == null) MaxTotalTileDefault = getdefault.MaxTotalTileDefault;
        }
    }

    public class CONFIG_PERMISSIONS
    {
        public string HouseKLP_Define = "RegionKLP.house.default";
        public string HouseKLP_Delete = "RegionKLP.house.default";
        public string HouseKLP_Share = "RegionKLP.house.default";
        public string HouseKLP_ShareWithGroups = "RegionKLP.house.default";
        public string HouseKLP_TeleportOwnedClaim = "RegionKLP.house.teleport";

        public string HouseKLP_SlotsNoLimits = "RegionKLP.house.nolimitslot";
        public string HouseKLP_Admin = "RegionKLP.house.admin";
        public string Cfg_Permission = "RegionKLP.house.admin";

        public CONFIG_PERMISSIONS() { }

        public void FixNull()
        {
            CONFIG_PERMISSIONS getdefault = new();

            if (HouseKLP_Define == null) HouseKLP_Define = getdefault.HouseKLP_Define;
            if (HouseKLP_Delete == null) HouseKLP_Delete = getdefault.HouseKLP_Delete;
            if (HouseKLP_Share == null) HouseKLP_Share = getdefault.HouseKLP_Share;
            if (HouseKLP_ShareWithGroups == null) HouseKLP_ShareWithGroups = getdefault.HouseKLP_ShareWithGroups;
            if (HouseKLP_TeleportOwnedClaim == null) HouseKLP_TeleportOwnedClaim = getdefault.HouseKLP_TeleportOwnedClaim;

            if (HouseKLP_SlotsNoLimits == null) HouseKLP_SlotsNoLimits = getdefault.HouseKLP_SlotsNoLimits;
            if (HouseKLP_Admin == null) HouseKLP_Admin = getdefault.HouseKLP_Admin;
            if (Cfg_Permission == null) Cfg_Permission = getdefault.Cfg_Permission;
        }
    }
    public class CONFIG_DB
    {
        public string StorageType = "sqlite";
        public string SqliteDBPath = "RegionsKLP.sqlite";
        public string MySqlHost = "localhost:3306";
        public string MySqlDbName = "";
        public string MySqlUsername = "";
        public string MySqlPassword = "";
        public CONFIG_DB() { }

        public void FixNull()
        {
            CONFIG_DB getdefault = new();

            if (StorageType == null) StorageType = getdefault.StorageType;
            if (SqliteDBPath == null) SqliteDBPath = getdefault.SqliteDBPath;
            if (MySqlHost == null) MySqlHost = getdefault.MySqlHost;
            if (MySqlDbName == null) MySqlDbName = getdefault.MySqlDbName;
            if (MySqlUsername == null) MySqlUsername = getdefault.MySqlUsername;
            if (MySqlPassword == null) MySqlPassword = getdefault.MySqlPassword;
        }
    }

    #endregion
}
