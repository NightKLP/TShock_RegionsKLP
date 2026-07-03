using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace RegionsKLP
{
    public class MainDB
    {
        public IDbConnection _db;

        public MainDB(IDbConnection db)
        {
            _db = db;

            var sqlCreator = new SqlTableCreator(db, db.GetSqlQueryBuilder());

            sqlCreator.EnsureTableStructure(new SqlTable(PlayerHousingKLP.TableName,
                new SqlColumn("ID", MySqlDbType.Int32) { Unique = true },
                new SqlColumn("Name", MySqlDbType.VarChar, 38),

                new SqlColumn("MaxHouseSlot", MySqlDbType.Int32) { DefaultValue = "0" },

                new SqlColumn("MaxTileWidth", MySqlDbType.Int32) { DefaultValue = "0" },
                new SqlColumn("MaxTileHeight", MySqlDbType.Int32) { DefaultValue = "0" },

                new SqlColumn("MaxTotalTile", MySqlDbType.Int32) { DefaultValue = "0" }));

            sqlCreator.EnsureTableStructure(new SqlTable(RegionKLP.TableName,
                new SqlColumn("RegionID", MySqlDbType.Int32) { Unique = true },
                new SqlColumn("SubName", MySqlDbType.VarChar, 40),
                new SqlColumn("RegionType", MySqlDbType.VarChar, 38)));

        }

        /// <exception cref="NullReferenceException"></exception>

        #region [ QUERYREADER ]

        public IDbConnection getDB()
        {
            return _db;
        }

        #endregion


        #region [ UserCache Names ]

        public Dictionary<int, string> UserIDNameCache = new();
        public Dictionary<string, int> NameUserIDCache = new();

        public void SyncUserIDNameCache()
        {
            foreach (UserAccount get in TShock.UserAccounts.GetUserAccounts())
            {
                if (!UserIDNameCache.ContainsKey(get.ID))
                {
                    UserIDNameCache.Add(get.ID, get.Name);
                }
                if (!NameUserIDCache.ContainsKey(get.Name))
                {
                    NameUserIDCache.Add(get.Name, get.ID);
                }
            }
        }

        #endregion

        #region {[ PlayerHousingKLP ]}

        public PlayerHousingKLP[] PlayerHouseData = {};

        public void PlayerHousingKLPSync()
        {
            List<PlayerHousingKLP> result = new();

            using var reader = _db.QueryReader($"SELECT * FROM {PlayerHousingKLP.TableName}");

            while (reader.Read())
            {
                result.Add(new PlayerHousingKLP(
                    reader.Get<int>("ID"),
                    reader.Get<int>("MaxHouseSlot"),
                    reader.Get<int>("MaxTileWidth"),
                    reader.Get<int>("MaxTileHeight"),
                    reader.Get<int>("MaxTotalTile")
                ));
            }
            PlayerHouseData = result.ToArray();
        }
        public bool TryGetPlayerHouseDataByPlayer(TSPlayer player, out PlayerHousingKLP account)
        {
            account = null;
            if (!player.IsLoggedIn) { return false; }

            return TryGetPlayerHouseDataByAccountID(player.Account.ID, out account);
        }

        public bool TryGetPlayerHouseDataByAccountID(int accountid, out PlayerHousingKLP account)
        {
            for (int i = 0; i < PlayerHouseData.Length; i++)
            {
                if (PlayerHouseData[i].ID != accountid) continue;
                account = PlayerHouseData[i];
                return true;
            }

            return PlayerHousingKLP.TryGetAccount(accountid, out account);
        }



        public bool TryCreatePlayerHouseData(TSPlayer player, int MaxHouseSlot = 0, int MaxTileWidth = 0, int MaxTileHeight = 0, int MaxTotalTile = 0)
        {

            if (player.Account == null)
            {
                //account = null;
                return false;
            }
            List<PlayerHousingKLP> result = PlayerHouseData.ToList();

            if (PlayerHousingKLP.Create(player.Account.ID, out PlayerHousingKLP stats, MaxHouseSlot, MaxTileWidth, MaxTileHeight, MaxTotalTile))
            {
                result.Add(stats);
                PlayerHouseData = result.ToArray();
                return true;
            }
            return false;
        }

        public bool TryDeletePlayerHouseData(TSPlayer player)
        {

            if (player.Account == null)
            {
                //account = null;
                return false;
            }

            return TryDeletePlayerHouseData(player.Account.ID);
        }

        public bool TryDeletePlayerHouseData(int AccountID)
        {

            List<PlayerHousingKLP> result = PlayerHouseData.ToList();
            for (int i = 0; i < result.Count; i++)
            {
                if (PlayerHouseData[i].ID != AccountID) continue;
                result.RemoveAt(i);
                PlayerHousingKLP.DeleteAccount(AccountID);
                PlayerHouseData = result.ToArray();
                return true;
            }
            return false;
        }

        #endregion



        #region {[ RegionKLP ]}

        public RegionKLP[] Regions = { };


        public void RegionKLPSync()
        {
            List<RegionKLP> result = new();

            using var reader = _db.QueryReader($"SELECT * FROM {RegionKLP.TableName}");

            while (reader.Read())
            {
                result.Add(new RegionKLP(
                    reader.Get<int>("RegionID"),
                    reader.Get<string>("SubName") ?? "",
                    reader.Get<string>("RegionType")
                    ));
            }
            Regions = result.ToArray();
        }


        public bool TryGetRegionKLPByID(int RegionID, out RegionKLP regionklp)
        {
            foreach (RegionKLP get in Regions)
            {
                if (get.ID == RegionID)
                {
                    regionklp = get;
                    return true;
                }
            }
            regionklp = null;
            return false;
        }

        public bool DeleteRegionKLP(int RegionID)
        {
            bool success = false;
            List<RegionKLP> result = new();
            foreach (RegionKLP get in Regions)
            {
                if (get.ID == RegionID)
                {
                    success = get.DeleteThis();
                } else
                {
                    result.Add(get);
                }
            }

            Regions = result.ToArray();
            return success;
        }

        public bool CreateRegionKLP(int RegionID, string Name, string RegionType)
        {
            bool success = RegionKLP.Create(RegionID, Name, RegionType, out RegionKLP data);
            if (success)
            {
                List<RegionKLP> result = Regions.ToList();

                result.Add(data);

                Regions = result.ToArray();
            }

            return success;
        }
        #endregion



    }

    #region [ Player House Data ]
    public class PlayerHousingKLP
    {
        public const string TableName = "HousePlayerData";

        public int ID;

        //slot
        private int MaxHouseSlot;

        //max
        private int MaxTileWidth;
        private int MaxTileHeight;

        //total
        private int MaxTotalTile;

        private IDbConnection _db = Getsqlcon();

        #region [ Create ]
        public static bool Create(int AccountID, out PlayerHousingKLP playeraccount, int MaxHouseSlot = 0, int MaxTileWidth = 0, int MaxTileHeight = 0, int MaxTotalTile = 0)
        {
            IDbConnection _db = Getsqlcon();
            bool isadded = _db.Query($"INSERT INTO {TableName} (" +
                "ID, " +
                "MaxHouseSlot, " +
                "MaxTileWidth, " +
                "MaxTileHeight, " +
                "MaxTotalTile) " +
                "VALUES (@0, @1, @2, @3, @4)",
                AccountID,//name
                MaxHouseSlot,
                MaxTileWidth,
                MaxTileHeight,
                MaxTotalTile
                ) != 0;



            if (isadded)
            {
                playeraccount = new(AccountID,
                    MaxHouseSlot,
                    MaxTileWidth,
                    MaxTileHeight,
                    MaxTotalTile);
            }
            else
            {
                playeraccount = null;
            }

            return isadded;
        }


        #endregion

        #region It Exist

        public static bool ItExist(int ID)
        {
            IDbConnection _db = Getsqlcon();

            using var reader = _db.QueryReader($"SELECT * FROM {TableName} WHERE ID = @0", ID);

            while (reader.Read())
            {
                return true;
            }
            return false;
        }

        #endregion
        public PlayerHousingKLP(int ID, int MaxHouseSlot, int MaxTileWidth, int MaxTileHeight, int MaxTotalTile)
        {
            this.ID = ID;
            this.MaxHouseSlot = MaxHouseSlot;
            this.MaxTileWidth = MaxTileWidth;
            this.MaxTileHeight = MaxTileHeight;
            this.MaxTotalTile = MaxTotalTile;
        }

        public static bool TryGetAccount(int AccountID, out PlayerHousingKLP playeraccount)
        {
            try
            {
                playeraccount = new PlayerHousingKLP(AccountID);
                return true;
            } catch (NullReferenceException)
            {
                playeraccount = null;
                return false;
            }
        }

        private PlayerHousingKLP(int AccountID)
        {
            using var reader = _db.QueryReader($"SELECT * FROM {TableName} WHERE ID = @0", AccountID);

            while (reader.Read())
            {
                this.ID = AccountID;
                //slot
                MaxHouseSlot = reader.Get<int>("MaxHouseSlot");

                //max
                MaxTileWidth = (int)MainKLP.Config.Main.Housing.MaxTileWidthDefault + reader.Get<int>("MaxTileWidth");
                MaxTileHeight = (int)MainKLP.Config.Main.Housing.MaxTileHeightDefault + reader.Get<int>("MaxTileHeight");

                //total
                MaxTotalTile = (int)MainKLP.Config.Main.Housing.MaxTotalTileDefault + reader.Get<int>("MaxTotalTile");

                return;
            }

            throw new NullReferenceException();
        }

        #region [ Get ]

        public int Get_MaxHouseSlot()
        {
            return (int)MainKLP.Config.Main.Housing.MaxHouseDefault + MaxHouseSlot;
        }


        public int Get_MinTileWidth()
        {
            return (int)MainKLP.Config.Main.Housing.MinTileWidthDefault;
        }
        public int Get_MinTileHeight()
        {
            return (int)MainKLP.Config.Main.Housing.MinTileHeightDefault;
        }


        public int Get_MaxTileWidth()
        {
            return (int)MainKLP.Config.Main.Housing.MaxTileWidthDefault + MaxTileWidth;
        }
        public int Get_MaxTileHeight()
        {
            return (int)MainKLP.Config.Main.Housing.MaxTileHeightDefault + MaxTileHeight;
        }


        public int Get_MinTotalTile()
        {
            return (int)MainKLP.Config.Main.Housing.MinTotalTileDefault;
        }
        public int Get_MaxTotalTile()
        {
            return (int)MainKLP.Config.Main.Housing.MaxTotalTileDefault + MaxTotalTile;
        }
        #endregion

        public static bool DeleteAccount(int AccountID)
        {
            IDbConnection _db = Getsqlcon();
            return _db.Query($"DELETE FROM {TableName} WHERE ID = @0", AccountID) != 0;
        }
        public bool DeleteThis()
        {
            return _db.Query($"DELETE FROM {TableName} WHERE ID = @0", ID) != 0;
        }

        public static bool Exist(int AccountID)
        {
            IDbConnection _db = Getsqlcon();
            try
            {
                using var reader = _db.QueryReader($"SELECT * FROM {TableName} WHERE ID = @0", AccountID);
                while (reader.Read())
                {
                    if (reader.Get<int>("ID") == AccountID) return true;
                }

                return false;
            }
            catch (NullReferenceException)
            {
                return false;
            }

        }

        public bool SetSlot(int slot)
        {
            MaxHouseSlot = slot;
            return _db.Query($"UPDATE {TableName} SET MaxHouseSlot = @0 WHERE ID = @1", MaxHouseSlot, ID) != 0;
        }
        public bool AddSlot(int slot)
        {
            MaxHouseSlot += slot;
            return _db.Query($"UPDATE {TableName} SET MaxHouseSlot = @0 WHERE ID = @1", MaxHouseSlot, ID) != 0;
        }
        public bool RemoveSlot(int slot)
        {
            MaxHouseSlot -= slot;
            return _db.Query($"UPDATE {TableName} SET MaxHouseSlot = @0 WHERE ID = @1", MaxHouseSlot, ID) != 0;
        }
        protected static IDbConnection Getsqlcon()
        {
            return MainKLP.MainIDBC;
        }
    }

    #endregion

    #region [ RegionKLP ]
    public class RegionKLP
    {
        public const string TableName = "HouseRegionStats";

        public int ID;
        public string SubName;
        public string[] RegionType;

        private IDbConnection _db = MainKLP.MainIDBC;

        #region New RegionKLP

        public static bool Create(int ID, string Name, string RegionType)
        {
            return Create(ID, Name, RegionType, out _);
        }

        public static bool Create(int ID, string Name, string RegionType, out RegionKLP stats)
        {
            IDbConnection _db = MainKLP.MainIDBC;
            bool isadded = _db.Query($"INSERT INTO {TableName} (" +
                "RegionID, " + // 0
                "SubName, " + // 1
                "RegionType) " + // 2
                "VALUES (@0, @1, @2);",
                ID, //RegionID
                Name, //SubName
                "" //PerkType
                ) != 0;

            if (isadded)
            {
                stats = new(ID, Name, RegionType);
            }
            else
            {
                stats = null;
            }

            return isadded;
        }

        #endregion

        #region It Exist

        public static bool StatsExist(int ID)
        {
            IDbConnection _db = MainKLP.MainIDBC;

            using var reader = _db.QueryReader($"SELECT * FROM {TableName} WHERE RegionID = @0", ID);

            while (reader.Read())
            {
                return true;
            }
            return false;
        }

        #endregion

        public RegionKLP() { }

        public RegionKLP(int ID, string SubName, string RegionType)
        {

            this.ID = ID;
            this.SubName = SubName;

            if (string.IsNullOrEmpty(RegionType))
            {
                this.RegionType = new string[] { };
            }
            else if (RegionType.Contains(","))
            {
                this.RegionType = RegionType.Split(",");
            }
            else
            {
                this.RegionType = new string[] { RegionType };
            }
        }

        //public RegionKLP(int ID)
        //{
        //    using var reader = _db.QueryReader($"SELECT * FROM {TableName} WHERE RegionID = @0", ID);

        //    while (reader.Read())
        //    {
        //        //set
        //        this.ID = reader.Get<int>("RegionID");
        //        SubName = reader.Get<string>("SubName") ?? "";
        //        string gettypes = reader.Get<string>("RegionType") ?? "";

        //        if (string.IsNullOrEmpty(gettypes))
        //        {
        //            RegionType = new string[] { };
        //        } else if (gettypes.Contains(","))
        //        {
        //            RegionType = gettypes.Split(",");
        //        } else
        //        {
        //            RegionType = new string[] { gettypes };
        //        }

        //        return;
        //    }

        //    throw new NullReferenceException();
        //}

        public bool TryGetRegion(out Region region)
        {
            region = TShock.Regions.GetRegionByID(ID);
            return region != null;
        }

        public bool ChangeSubName(string NewName)
        {
            SubName = NewName;
            return _db.Query($"UPDATE {TableName} SET SubName = @0 WHERE RegionID = @1", SubName, ID) != 0;
        }

        #region Modify Region Types
        public bool RemoveRegionType(string type)
        {
            #region code
            if (string.IsNullOrEmpty(type) || string.IsNullOrWhiteSpace(type))
            {
                return false;
            }

            List<string> result = RegionType.ToList();

            if (!result.Contains(type)) { return true; }

            result.Remove(type);

            RegionType = result.ToArray();
            return _db.Query($"UPDATE {TableName} SET RegionType = @0 WHERE RegionID = @1", string.Join(",", RegionType), ID) != 0;
            #endregion
        }
        public bool AddRegionType(string type)
        {
            #region code
            if (string.IsNullOrEmpty(type) || string.IsNullOrWhiteSpace(type))
            {
                return false;
            }

            List<string> result = RegionType.ToList();

            if (result.Contains(type)) { return true; }

            result.Add(type);

            RegionType = result.ToArray();
            return _db.Query($"UPDATE {TableName} SET RegionType = @0 WHERE RegionID = @1", string.Join(",", RegionType), ID) != 0;
            #endregion
        }
        public bool SetRegionType(string[] regiontypes)
        {
            #region code

            RegionType = regiontypes;
            return _db.Query($"UPDATE {TableName} SET RegionType = @0 WHERE RegionID = @1", string.Join(",", RegionType), ID) != 0;
            #endregion
        }
        #endregion



        public static bool Delete(int AccountID)
        {
            IDbConnection _db = MainKLP.MainIDBC;
            return _db.Query($"DELETE FROM {TableName} WHERE RegionID = @0", AccountID) != 0;
        }
        public bool DeleteThis()
        {
            return _db.Query($"DELETE FROM {TableName} WHERE RegionID = @0", ID) != 0;
        }

        public static bool RESETALL()
        {
            IDbConnection _db = MainKLP.MainIDBC;
            return _db.Query($"DELETE FROM {TableName}") != 0;
        }
    }
    #endregion
}
