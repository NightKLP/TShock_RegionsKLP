using Microsoft.Xna.Framework;
using RegionsKLP.Functions;
using RegionsKLP.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using TShockAPI;
using TShockAPI.DB;
using static RegionsKLP.Modules.HouseKLP;
using static TShockAPI.GetDataHandlers;

namespace RegionsKLP
{
    internal static class CommandsKLP
    {
        public static void Initialize()
        {

            Commands.ChatCommands.Add(new Command("", IGCommand_House, "house", "housing")
            {
                AllowServer = false,
                HelpText = ""
            });

            Commands.ChatCommands.Add(new Command(MainKLP.Config.Permissions.HouseKLP_Admin, IGCommand_HouseAdmin, "houseadmin", "housingadmin")
            {
                AllowServer = false,
                HelpText = ""
            });
        }


        #region [ Command /house ]
        private static void IGCommand_House(CommandArgs args)
        {
            var subcmdnames = HouseCommandKLP.SubCommands.Where(e => args.Player.HasPermission(e.permission)).Select(e => e.name);

            if (args.Parameters.Count >= 1)
            {
                string subCommand = args.Parameters[0].ToLowerInvariant();

                bool executed = false;
                foreach (var getsubcommand in HouseCommandKLP.SubCommands)
                {
                    if (getsubcommand.name == subCommand || getsubcommand.alias.Contains(subCommand))
                    {
                        if (!args.Player.HasPermission(getsubcommand.permission))
                        {
                            executed = true;
                            args.Player.SendErrorMessage("You do not have the necessary permission to do that.");
                            continue;
                        }
                        executed = true;
                        getsubcommand.action.Invoke(args);
                    }
                }
                if (!executed)
                {
                    args.Player.SendErrorMessage("Invalid Sub-Command");
                    args.Player.SendMessage(
                        "\nHouse sub-commands" +
                        "\n" + string.Join(",", subcmdnames), Color.Yellow);
                }
                return;
            }

            int playerHouseCount = HouseKLP.GetHouseRegionByOwnerID(args.Player.Account.ID).Count;

            PlayerHousingKLP getplayerdata;

            if (!MainKLP.MainDBManager.TryGetPlayerHouseDataByPlayer(args.Player, out getplayerdata))
            {
                args.Player.SendErrorMessage("Unable to get your playerhousedata!");
                return;
            }

            args.Player.SendMessage(
                $"You've defined {playerHouseCount} of {getplayerdata.Get_MaxHouseSlot()} possible houses so far." +
                "\nHouse sub-commands" +
                "\n" + string.Join(", ", subcmdnames) +
                (args.Player.HasPermission(MainKLP.Config.Permissions.HouseKLP_Admin) ? "\n\n" + TShock.Utils.ColorTag("(House Admin) Access Granted!", Color.Lime) : ""), Color.Yellow);
        }
        #endregion

        #region [ Command /houseadmin ]
        private static void IGCommand_HouseAdmin(CommandArgs args)
        {
            var subcmdnames = HouseAdminCommandKLP.SubCommands.Where(e => args.Player.HasPermission(e.permission)).Select(e => e.name);

            if (args.Parameters.Count >= 1)
            {
                string subCommand = args.Parameters[0].ToLowerInvariant();

                bool executed = false;
                foreach (var getsubcommand in HouseAdminCommandKLP.SubCommands)
                {
                    if (getsubcommand.name == subCommand || getsubcommand.alias.Contains(subCommand))
                    {
                        if (!args.Player.HasPermission(getsubcommand.permission))
                        {
                            executed = true;
                            args.Player.SendErrorMessage("You do not have the necessary permission to do that.");
                            continue;
                        }
                        executed = true;
                        getsubcommand.action.Invoke(args);
                    }
                }
                if (!executed)
                {
                    args.Player.SendErrorMessage("Invalid Sub-Command");
                    args.Player.SendMessage(
                        "\nHouse Admin sub-commands" +
                        "\n" + string.Join(",", subcmdnames), Color.Yellow);
                }
                return;
            }

            int playerHouseCount = HouseKLP.GetHouseRegionByOwnerID(args.Player.Account.ID).Count;

            PlayerHousingKLP getplayerdata;

            if (!MainKLP.MainDBManager.TryGetPlayerHouseDataByPlayer(args.Player, out getplayerdata))
            {
                args.Player.SendErrorMessage("Unable to get your playerhousedata!");
                return;
            }

            args.Player.SendMessage(
                $"You've defined {playerHouseCount} of {getplayerdata.Get_MaxHouseSlot()} possible houses so far." +
                "\nHouse Admin sub-commands" +
                "\n" + string.Join(", ", subcmdnames), Color.Yellow);
        }
        #endregion
    }

    public static class HouseCommandKLP
    {
        static string[] EmptySubAlias = { };

        public static List<(string name, string[] alias, Action<CommandArgs> action, string permission)> SubCommands = new List<(string, string[], Action<CommandArgs> action, string permission)>()
        {
            ("overview", EmptySubAlias, HouseOverviewSubCommand, ""),
            ("info", EmptySubAlias, HouseInfoSubCommand, ""),
            ("scan", EmptySubAlias, HouseScanSubCommand, ""),

            ("summary", EmptySubAlias, HouseSummarySubCommand, MainKLP.Config.Permissions.HouseKLP_Admin),
        
            ("teleport", new string[]{ "tp" }, HouseTPSubCommand, MainKLP.Config.Permissions.HouseKLP_TeleportOwnedClaim),

            ("define", EmptySubAlias, HouseDefineSubCommand, MainKLP.Config.Permissions.HouseKLP_Define),
            ("resize", EmptySubAlias, HouseResizeSubCommand, MainKLP.Config.Permissions.HouseKLP_Define),
            ("delete", EmptySubAlias, HouseDeleteSubCommand, MainKLP.Config.Permissions.HouseKLP_Define),
            ("rename", EmptySubAlias, HouseRenameSubCommand, MainKLP.Config.Permissions.HouseKLP_Define),

            ("setowner", EmptySubAlias, HouseSetOwnerSubCommand, MainKLP.Config.Permissions.HouseKLP_Share),
            ("share", EmptySubAlias, HouseShareSubCommand, MainKLP.Config.Permissions.HouseKLP_Share),
            ("unshare", EmptySubAlias, HouseUnshareSubCommand, MainKLP.Config.Permissions.HouseKLP_Share),

            ("sharegroup", EmptySubAlias, HouseShareGroupSubCommand, MainKLP.Config.Permissions.HouseKLP_ShareWithGroups),
            ("unsharegroup", EmptySubAlias, HouseUnshareGroupSubCommand, MainKLP.Config.Permissions.HouseKLP_ShareWithGroups),
        };

        #region [ Sub-Command : overview ]
        public static void HouseOverviewSubCommand(CommandArgs args)
        {

            int pageNumber;
            if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, null, out pageNumber))
                return;

            switch (pageNumber)
            {
                default:
                    args.Player.SendMessage(TShock.Utils.ColorTag("House Regions Overview (Page 1 of 2)", Color.Lime) +
                        "\nThis plugin provides players on TShock driven Terraria servers the possibility" +
                        "\nof defining houses in which other players can not alter any tiles." +
                        "\nFor more information about defining new houses write /house define help" +
                        "\n" +
                        "\nYou may also want to allow other players to change the tiles in your house," +
                        "\n" +
                        "\ntype '/house overview 2' for the next page.", Color.LightGray);
                    break;
                case 2:
                    args.Player.SendMessage(TShock.Utils.ColorTag("House Regions Overview (Page 2 of 2)", Color.Lime) +
                        "to do that, you can either add specific users or whole groups of users to your" +
                        "\nhouse. To get more information on how to sharing a house type /house share help or" +
                        "\n/house sharegroup help." +
                        "\n" +
                        "\nTo check for existing houses or to get general information about existing houses" +
                        "\nuse the /house info command.", Color.LightGray);
                    break;
            }

            return;
        }

        #endregion

        #region [ Sub-Command : summary ]
        private static void HouseSummarySubCommand(CommandArgs args)
        {
            int pageNumber = 1;

            if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber)) { return; }

            var ownerHouses = new Dictionary<string, int>(TShock.Regions.Regions.Count);
            for (int i = 0; i < TShock.Regions.Regions.Count; i++)
            {
                Region tsRegion = TShock.Regions.Regions[i];
                int ownerid;
                if (!HouseKLP.TryGetHouseRegionKLP(tsRegion, out ownerid, out _))
                    continue;

                int houseCount;
                if (!ownerHouses.TryGetValue($"{tsRegion.Owner} ({ownerid})", out houseCount))
                    ownerHouses.Add($"{tsRegion.Owner} ({ownerid})", 1);
                else
                    ownerHouses[$"{tsRegion.Owner} ({ownerid})"] = houseCount + 1;
            }

            IEnumerable<string> ownerHousesTermSelector = ownerHouses.Select(
              pair => string.Concat(pair.Key, " (", pair.Value, ")")
            );

            PaginationTools.SendPage(
              args.Player, pageNumber, PaginationTools.BuildLinesFromTerms(ownerHousesTermSelector), new PaginationTools.Settings
              {
                  HeaderFormat = string.Format("House Owners ({{0}}/{{1}}):"),
                  FooterFormat = string.Format("Type '/house summary {{0}}' for more."),
                  NothingToDisplayString = "There are no house regions in this world."
              }
            );
        }
        #endregion

        #region [ Sub-Command : rename ]
        public static void HouseRenameSubCommand(CommandArgs args)
        {
            if (args.Parameters.Count >= 2)
            {
                string newhousename = args.Parameters[1];

                RegionKLP regionklp;
                if (!HouseKLP.TryGetAccessibleHouseRegionAtPlayer(args.Player, out _, out regionklp, out _)) { return; }

                if (regionklp.SubName.ToLower() == newhousename.ToLower())
                {
                    args.Player.SendErrorMessage("The house name your trying to change is the same!");
                    return;
                }


                if (HouseKLP.RenameHouseRegionWithOwner(args.Player.Account.ID, regionklp, newhousename, out bool duplicate))
                {
                    args.Player.SendSuccessMessage("Successfully change your house name to " + newhousename);
                } else if (duplicate)
                {
                    args.Player.SendErrorMessage("one of your houses has already name " + newhousename);
                }
                return;
            }

            args.Player.SendErrorMessage("Proper syntax: /house rename [newhousename]" +
                "\nType /house rename help to get more information about this command.");
            return;
        }

        #endregion

        #region [ Sub-Command : tp/teleport ]
        public static void HouseTPSubCommand(CommandArgs args)
        {
            if (args.Parameters.Count >= 2)
            {
                Region region;
                if (!HouseKLP.GetHouseRegionByOwnerIDAndHouseName(args.Player.Account.ID, args.Parameters[1], out region))
                {
                    args.Player.SendErrorMessage($"House Name {args.Parameters[1]} doesn't exist!");
                    return;
                }

                args.Player.SendSuccessMessage("Successfully Teleported to your claim " +
                    "\nname: " + args.Parameters[1]);
                args.Player.Teleport(region.Area.Center.X * 16, region.Area.Center.Y * 16);
                return;
            }

            args.Player.SendErrorMessage("Proper syntax: /house tp <housename>");
            return;
        }
        #endregion

        #region [ Sub-Command : info ]
        private static void HouseInfoSubCommand(CommandArgs args)
        {

            int ownerid;
            Region region;
            RegionKLP regionklp;
            if (!HouseKLP.TryGetHouseRegionAtPlayer(args.Player, out ownerid, out regionklp, out region))
            {
                args.Player.SendErrorMessage("There's no house on your current position.");
                return;
            }

            UserAccount AccOwner = TShock.UserAccounts.GetUserAccountByID(ownerid);

            string sharedwith = "";
            string sharegroup = "";

            if (region.AllowedIDs.Count > 0)
            {
                sharedwith = string.Join(",", region.AllowedIDs.Select(e =>
                {
                    UserAccount user = TShock.UserAccounts.GetUserAccountByID(e);
                    if (user != null)
                        return user.Name;
                    else
                        return string.Concat("{ID: ", e, "}");
                }));
            }
            else
            {
                sharedwith += "House is not shared with any users.";
            }

            if (region.AllowedGroups.Count > 0)
            {
                sharegroup = string.Join(",", region.AllowedGroups);
            }
            else
            {
                sharegroup += "House is not shared with any groups.";
            }

            string message =
                $"Information About This House" +
                $"\n[c/ffffff:House Name:] [c/2dd0a6:{regionklp.SubName}]" +
                $"\n[c/ffffff:Owner:] [c/2dd057:{AccOwner.Name}]" +
                $"\n" +
                $"\n[c/ffffff:Shared with:] [c/f5f216:{sharedwith}]" +
                $"\n[c/ffffff:Shared with Groups:] [c/c2f516:{sharegroup}]" +
                $"{(regionklp.RegionType.Contains(MainKLP.HouseWarding) ? "\n\n[c/2d7ed0:this house has warding on it!]" : "")}";

            args.Player.SendMessage(message, Color.Gray);

            Indicator.SendAreaDottedFakeTilesTimed(args.Player, region.Area, 5000,
                TileColor: regionklp.RegionType.Contains(MainKLP.HouseWarding) ? Terraria.ID.PaintID.DeepBluePaint : Terraria.ID.PaintID.DeepRedPaint);
        }

        #endregion

        #region [ Sub-Command : scan ]
        private static void HouseScanSubCommand(CommandArgs args)
        {
            Point playerLocation = new Point(args.Player.TileX, args.Player.TileY);
            List<(Rectangle, string[], bool)> houseAreasToDisplay = new List<(Rectangle, string[], bool)>(
              from r in TShock.Regions.Regions
              where Math.Sqrt(Math.Pow(playerLocation.X - r.Area.Center.X, 2) + Math.Pow(playerLocation.Y - r.Area.Center.Y, 2)) <= 200
              select (r.Area, GetTypes(r.ID), r.Name.StartsWith(MainKLP.Config.Main.Housing.Indicator))
            );

            string[] GetTypes(int RegionID)
            {
                if (MainKLP.MainDBManager.TryGetRegionKLPByID(RegionID, out RegionKLP get))
                {
                    return get.RegionType;
                }
                return new string[] { };
            }

            if (houseAreasToDisplay.Count == 0)
            {
                args.Player.SendSuccessMessage("There are no nearby house regions.");
                return;
            }

            foreach (var getRegion in houseAreasToDisplay)
            {
                if (!getRegion.Item3) continue;
                Indicator.SendAreaDottedFakeTiles(args.Player, getRegion.Item1, TileColor: getRegion.Item2.Contains(MainKLP.HouseWarding) ? Terraria.ID.PaintID.DeepBluePaint : Terraria.ID.PaintID.DeepRedPaint);
            }
            args.Player.SendInfoMessage("Here are nearby house regions.");


            System.Threading.Timer hideTimer = null;
            hideTimer = new System.Threading.Timer(state => {
                foreach (var getRegion in houseAreasToDisplay)
                {
                    if (!getRegion.Item3) continue;
                    Indicator.SendAreaDottedFakeTiles(args.Player, getRegion.Item1, false);
                }

                // ReSharper disable AccessToModifiedClosure
                Debug.Assert(hideTimer != null);
                hideTimer.Dispose();
                // ReSharper restore AccessToModifiedClosure
            },
              null, 10000, Timeout.Infinite
            );

        }
        #endregion

        #region [ Sub-Command : define ]
        public static void HouseDefineSubCommand(CommandArgs args)
        {
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendErrorMessage("You have to be logged in in order to define houses.");
                return;
            }

            MainKLP.AddOnCreateHouse(args.Player);

            args.Player.SendMessage(TShock.Utils.ColorTag("First Mark", Color.IndianRed) + "\n" +
                "Mark the top left tile of your house by interact any blocks" + "\n" +
                "or by altering the tile otherwise.", Color.MediumSpringGreen);
        }
        #endregion

        #region [ Sub-Command : resize ]
        public static void HouseResizeSubCommand(CommandArgs args)
        {
            Action invalidSyntax = () => {
                args.Player.SendErrorMessage("Proper syntax: /house resize <up|down|left|right>[...] <amount>");
                args.Player.SendInfoMessage("Type /house resize help to get more information about this command.");
            };

            if (args.Parameters.Count >= 2 && args.Parameters[1].Equals("help", StringComparison.InvariantCultureIgnoreCase))
            {
                int pageNumber;
                if (!PaginationTools.TryParsePageNumber(args.Parameters, 2, args.Player, out pageNumber))
                    return;

                SendHelpText(pageNumber);
                return;
            }

            PlayerHousingKLP getplayerdata;
            if (!MainKLP.MainDBManager.TryGetPlayerHouseDataByPlayer(args.Player, out getplayerdata))
            {
                args.Player.SendErrorMessage("unable to get your data!");
                return;
            }

            Region region;
            RegionKLP housedata;
            int ownerid;
            if (!HouseKLP.TryGetAccessibleHouseRegionAtPlayer(args.Player, out ownerid, out housedata, out region))
                return;

            int amount;
            if (args.Parameters.Count < 3 || !int.TryParse(args.Parameters[args.Parameters.Count - 1], out amount))
            {
                invalidSyntax();
                return;
            }

            Rectangle newArea = region.Area;
            List<int> directions = new List<int>();
            //0 = up
            //1 = right
            //2 = down
            //3 = left
            for (int i = 1; i < args.Parameters.Count - 1; i++)
            {
                switch (args.Parameters[i].ToLowerInvariant())
                {
                    case "up":
                    case "u":
                        newArea.Y -= amount;
                        newArea.Height += amount;
                        directions.Add(0);
                        break;
                    case "down":
                    case "d":
                        newArea.Height += amount;
                        directions.Add(2);
                        break;
                    case "left":
                    case "l":
                        newArea.X -= amount;
                        newArea.Width += amount;
                        directions.Add(3);
                        break;
                    case "right":
                    case "r":
                        newArea.Width += amount;
                        directions.Add(1);
                        break;
                }
            }

            if (newArea.Width < 0)
                newArea.Width = 1;
            if (newArea.Height < 0)
                newArea.Height = 1;

            bool IsSmall;
            if (!HouseKLP.CheckHouseRegionValidSize(newArea, getplayerdata, out IsSmall))
            {
                args.Player.SendErrorMessage(HouseKLP.GetExplainInvalidRegionSizeText(getplayerdata, newArea, IsSmall));
                return;
            }

            if (HouseKLP.CheckHouseRegionOverlap(ownerid, newArea))
            {
                if ((bool)MainKLP.Config.Main.Housing.AllowTShockRegionOverlapping)
                {
                    args.Player.SendErrorMessage("The house region would overlap either with another house not owned by you or");
                    args.Player.SendErrorMessage("with a TShock region.");
                }
                else
                {
                    args.Player.SendErrorMessage("The house region would overlap with another house not owned by you.");
                }

                return;
            }

            Rectangle oldArea = region.Area;
            region.Area = newArea;
            foreach (int direction in directions)
            {
                if (!TShock.Regions.ResizeRegion(region.Name, amount, direction))
                {
                    args.Player.SendErrorMessage("Internal error has occured.");
                    region.Area = oldArea;
                    return;
                }
            }

            args.Player.SendSuccessMessage("House was successfully resized.");
            Indicator.SendAreaDottedFakeTiles(args.Player, oldArea, false, housedata.RegionType.Contains(MainKLP.HouseWarding) ? Terraria.ID.PaintID.DeepBluePaint : Terraria.ID.PaintID.DeepRedPaint);
            Indicator.SendAreaDottedFakeTilesTimed(args.Player, newArea, 2000, housedata.RegionType.Contains(MainKLP.HouseWarding) ? Terraria.ID.PaintID.DeepBluePaint : Terraria.ID.PaintID.DeepRedPaint);

            void SendHelpText(int getpage)
            {

                switch (getpage)
                {
                    default:
                        args.Player.SendMessage(
                            TShock.Utils.ColorTag("Command reference for /house resize (Page 1 of 3)", Color.Lime) + "\n" +
                            TShock.Utils.ColorTag("/house resize <up|down|left|right>[...] <amount>", Color.White) + "\n\n" +
                            "\nResizes the current house to one direction by the given amount." +
                            "\nu|d|l|r = The directions to resize to (up, left, down, right).", Color.LightGray);
                        break;
                    case 2:
                        args.Player.SendMessage(
                            "amount = The amount of tiles to expand, can also be negative to shrink\n" +
                            "         the house region.", Color.LightGray);
                        args.Player.SendMessage(
                            "\nNOTE: If you hold a wire or wire tool, then you can see the new boundaries" +
                            "\nof the house region after the resize.", Color.IndianRed);
                        break;
                    case 3:
                        args.Player.SendMessage(
                            "NOTE: You have to own a house in order to resize it, just having" +
                            "\nbuild access is not sufficient.", Color.IndianRed);
                        return;
                }
            }
        }
        #endregion

        #region [ Sub-Command : delete ]
        private static void HouseDeleteSubCommand(CommandArgs args)
        {
            Region region;
            if (!HouseKLP.TryGetAccessibleHouseRegionAtPlayer(args.Player, out _, out region))
                return;

            if (!TShock.Regions.DeleteRegion(region.Name))
            {
                args.Player.SendErrorMessage("Internal error has occured.");
                return;
            }

            args.Player.SendSuccessMessage("The house was successfully deleted.");
        }
        #endregion

        #region [ Sub-Command : setowner ]
        private static void HouseSetOwnerSubCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Proper syntax: /house setowner <user name>" +
                    "\nNote: a player must be online!");
                return;
            }

            string newOwnerRaw = string.Join(" ", args.Parameters.ToArray(), 1, args.Parameters.Count - 1);
            
            var players = TSPlayer.FindByNameOrID(newOwnerRaw);
            if (players.Count > 1)
            {
                args.Player.SendMultipleMatchError(players.Select(p => p.Name));
                return;
            }
            if (players.Count <= 0)
            {
                args.Player.SendErrorMessage("Invalid Player!");
                return;
            }
            if (!players[0].IsLoggedIn)
            {
                args.Player.SendErrorMessage("A player must be logged in!");
                return;
            }

            TSPlayer target = players[0];

            Region region;
            RegionKLP regionklp;
            if (!HouseKLP.TryGetAccessibleHouseRegionAtPlayer(args.Player, out _, out regionklp, out region))
                return;

            if (target.Account.Name == region.Owner)
            {
                args.Player.SendErrorMessage($"{target.Account.Name} is already the owner of this region.");
                return;
            }

            Group tsGroup = TShock.Groups.GetGroupByName(target.Account.Group);
            if (tsGroup == null)
            {
                args.Player.SendErrorMessage("The new owner's TShock group could not be determined.");
                return;
            }

            if (HouseKLP.HasReachedMaxHouseSlot(target.Account.ID))
            {
                args.Player.SendErrorMessage("The new owner of the house would exceed their house limit.");
                return;
            }

            if (target.AwaitingResponse.ContainsKey("houseyes") && target.AwaitingResponse.ContainsKey("houseno"))
            {
                args.Player.SendErrorMessage($"{target.Name} currently has ongoing house claim request!");
                return;
            }

            args.Player.SendMessage($"Sent a house ownership request to \"{target.Account.Name}\".", Color.Coral);
            target.SendMessage(
                $"{args.Player.Name} has offered to transfer ownership of their house to you." +
                $"\nType {TShock.Utils.ColorTag("'/houseyes'", Color.LimeGreen)} to accept the transfer or {TShock.Utils.ColorTag("'/houseno'", Color.Red)} to decline.",
                Color.LightGray);

            target.AddResponse("houseyes", (e =>
            {
                target.AwaitingResponse.Remove("houseno");

                if (HouseKLP.SetHouseRegionOwner(region, target.Account.ID))
                {
                    args.Player.SendSuccessMessage($"The owner of this house named {regionklp.SubName} has been set to \"{target.Account.Name}\" and all shared users and groups were deleted from it.");

                    target.SendSuccessMessage($"you claimed {args.Player.Name} house named {regionklp.SubName}!");
                } else
                {
                    args.Player.SendErrorMessage($"Error occur giving your claim to {target.Name}.");
                    target.SendErrorMessage($"Error occur claiming {args.Player.Name} House.");
                }
                
            }));

            target.AddResponse("houseno", (e =>
            {
                target.AwaitingResponse.Remove("houseyes");

                args.Player.SendErrorMessage($"{target.Name} Denied your request.");
                target.SendWarningMessage("House Claim Request has been denied.");
            }));

        }
        #endregion

        #region [ Sub-Command : share ]
        private static void HouseShareSubCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Proper syntax: /house share <username>");
                args.Player.SendInfoMessage("Type /house share help to get more information about this command.");
                return;
            }

            string shareTargetRaw = string.Join(" ", args.Parameters.ToArray(), 1, args.Parameters.Count - 1);

            UserAccount tsUser = TShock.UserAccounts.GetUserAccountByName(shareTargetRaw);
            TSPlayer target = null;

            if (tsUser != null)
            {
                // See if this account is currently online.
                target = TShock.Players.FirstOrDefault(p =>
                    p != null &&
                    p.IsLoggedIn &&
                    p.Account?.ID == tsUser.ID);
            }
            else
            {
                // Fall back to searching online players.
                var players = TSPlayer.FindByNameOrID(shareTargetRaw);

                if (players.Count > 1)
                {
                    args.Player.SendMultipleMatchError(players.Select(p => p.Name));
                    return;
                }

                if (players.Count == 0)
                {
                    args.Player.SendErrorMessage("Invalid player.");
                    return;
                }

                target = players[0];

                if (!target.IsLoggedIn)
                {
                    args.Player.SendErrorMessage("That player is not logged in.");
                    return;
                }

                tsUser = target.Account;
            }

            if (tsUser == null)
            {
                args.Player.SendErrorMessage("Invalid player.");
                return;
            }

            Region region;
            RegionKLP regionklp;
            if (!HouseKLP.TryGetAccessibleHouseRegionAtPlayer(args.Player, out _, out regionklp, out region)) { return; }

            if (!TShock.Regions.AddNewUser(region.Name, tsUser.Name))
            {
                args.Player.SendErrorMessage("Internal error has occured.");
                return;
            }

            args.Player.SendSuccessMessage($"User \"{tsUser.Name}\" has build access to this house now.");
            if (target != null)
            {
                target.SendMessage($"{args.Player.Name} has gave you access to their house named {regionklp.SubName}", Color.GreenYellow);
            }
        }
        #endregion

        #region [ Sub-Command : unshare ]
        private static void HouseUnshareSubCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Proper syntax: /house unshare <user name>");
                args.Player.SendInfoMessage("Type /house unshare help to get more information about this command.");
                return;
            }

            string shareTargetRaw = string.Join(" ", args.Parameters.ToArray(), 1, args.Parameters.Count - 1);

            UserAccount tsUser = TShock.UserAccounts.GetUserAccountByName(shareTargetRaw);
            TSPlayer target = null;

            if (tsUser != null)
            {
                // See if this account is currently online.
                target = TShock.Players.FirstOrDefault(p =>
                    p != null &&
                    p.IsLoggedIn &&
                    p.Account?.ID == tsUser.ID);
            }
            else
            {
                // Fall back to searching online players.
                var players = TSPlayer.FindByNameOrID(shareTargetRaw);

                if (players.Count > 1)
                {
                    args.Player.SendMultipleMatchError(players.Select(p => p.Name));
                    return;
                }

                if (players.Count == 0)
                {
                    args.Player.SendErrorMessage("Invalid player.");
                    return;
                }

                target = players[0];

                if (!target.IsLoggedIn)
                {
                    args.Player.SendErrorMessage("That player is not logged in.");
                    return;
                }

                tsUser = target.Account;
            }

            if (tsUser == null)
            {
                args.Player.SendErrorMessage("Invalid player.");
                return;
            }

            Region region;
            RegionKLP regionklp;
            if (!HouseKLP.TryGetAccessibleHouseRegionAtPlayer(args.Player, out _, out regionklp, out region)) { return; }

            if (!TShock.Regions.RemoveUser(region.Name, tsUser.Name))
            {
                args.Player.SendErrorMessage("Internal error has occured.");
                return;
            }

            args.Player.SendSuccessMessage($"User \"{tsUser.Name}\" has no more build access to this house anymore.");
            if (target != null)
            {
                target.SendMessage($"{args.Player.Name} has remove you access to their house named {regionklp.SubName}", Color.GreenYellow);
            }
        }

        #endregion

        #region [ Sub-Command : sharegroup ]
        public static void HouseShareGroupSubCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Proper syntax: /house sharegroup <group name>");
                args.Player.SendInfoMessage("Type /house sharegroup help to get more information about this command.");
                return;
            }

            string shareTargetRaw = string.Join(" ", args.Parameters.ToArray(), 1, args.Parameters.Count - 1);
            
            Group tsGroup = TShock.Groups.GetGroupByName(shareTargetRaw);
            if (tsGroup == null)
            {
                args.Player.SendErrorMessage($"A group with the name \"{shareTargetRaw}\" does not exist.");
                return;
            }

            Region region;
            if (!HouseKLP.TryGetAccessibleHouseRegionAtPlayer(args.Player, out _, out region))
                return;

            if (!TShock.Regions.AllowGroup(region.Name, tsGroup.Name))
            {
                args.Player.SendErrorMessage("Internal error has occured.");
                return;
            }

            args.Player.SendSuccessMessage($"All users of group \"{tsGroup.Name}\" have build access to this house now.");
        }
        #endregion

        #region [ Sub-Command : unsharegroup ]
        public static void HouseUnshareGroupSubCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Proper syntax: /house unsharegroup <group name>");
                args.Player.SendInfoMessage("Type /house unsharegroup help to get more information about this command.");
                return;
            }

            string shareTargetRaw = string.Join(" ", args.Parameters.ToArray(), 1, args.Parameters.Count - 1);
            
            Group tsGroup = TShock.Groups.GetGroupByName(shareTargetRaw);
            if (tsGroup == null)
            {
                args.Player.SendErrorMessage("A group with the name \"{0}\" does not exist.", shareTargetRaw);
                return;
            }

            Region region;
            if (!HouseKLP.TryGetAccessibleHouseRegionAtPlayer(args.Player, out _, out region))
                return;

            if (!TShock.Regions.RemoveGroup(region.Name, tsGroup.Name))
            {
                args.Player.SendErrorMessage("Internal error has occured.");
                return;
            }

            args.Player.SendSuccessMessage($"Users of group \"{tsGroup.Name}\" have no more build access to this house anymore.");
        }
        #endregion
    }

    public static class HouseAdminCommandKLP
    {
        static string[] EmptySubAlias = { };

        public static List<(string name, string[] alias, Action<CommandArgs> action, string permission)> SubCommands = new List<(string, string[], Action<CommandArgs> action, string permission)>()
        {
            ("playerdatalist", new string[]{ "plrdatalist" }, HousePlayerDataListSubCommand, ""),
            ("givehouseslot", new string[]{ "giveslot" }, HouseGiveHouseSlotSubCommand, ""),
            ("removehouseslot", new string[]{ "removeslot" }, HouseRemoveHouseSlotSubCommand, "")
        };

        #region [ Sub-Command : playerdatalist ]
        private static void HousePlayerDataListSubCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 3)
            {
                args.Player.SendErrorMessage("Proper Syntax: /houseadmin playerdatalist [page]");
                return;
            }

            int pageNumber;
            if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber)) { return; }


            var getlisttxt = MainKLP.MainDBManager.PlayerHouseData.Select(plr =>
            {
                string name = plr.ID.ToString();
                if (MainKLP.MainDBManager.UserIDNameCache.ContainsKey(plr.ID))
                {
                    name = MainKLP.MainDBManager.UserIDNameCache[plr.ID];
                }

                return
                    $"== {name} ==" +
                    $"\nMaxHouseSlot: {plr.Get_MaxHouseSlot()}" +
                    $"\nMaxTileHeight: {plr.Get_MaxTileHeight()}" +
                    $"\nMaxTileWidth: {plr.Get_MaxTileWidth()}" +
                    $"\nMaxTotalTile: {plr.Get_MaxTotalTile()}";
            });


            PaginationTools.SendPage(
              args.Player, pageNumber, PaginationTools.BuildLinesFromTerms(getlisttxt), new PaginationTools.Settings
              {
                  HeaderFormat = string.Format("Player HouseData ({{0}}/{{1}}):"),
                  FooterFormat = string.Format("Type '/houseadmin playerdatalist {{0}}' for more."),
                  NothingToDisplayString = "No player data currently"
              }
            );
            return;
        }

        #endregion

        #region [ Sub-Command : givehouseslot ]
        public static void HouseGiveHouseSlotSubCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 3)
            {
                args.Player.SendErrorMessage("Proper Syntax: /houseadmin givehouseslot [account] [amount]");
                return;
            }

            UserAccount tsUser = TShock.UserAccounts.GetUserAccountByName(args.Parameters[1]);
            TSPlayer target = null;

            if (tsUser != null)
            {
                // See if this account is currently online.
                target = TShock.Players.FirstOrDefault(p =>
                    p != null &&
                    p.IsLoggedIn &&
                    p.Account?.ID == tsUser.ID);
            }
            else
            {
                // Fall back to searching online players.
                var players = TSPlayer.FindByNameOrID(args.Parameters[1]);

                if (players.Count > 1)
                {
                    args.Player.SendMultipleMatchError(players.Select(p => p.Name));
                    return;
                }

                if (players.Count == 0)
                {
                    args.Player.SendErrorMessage("Invalid player.");
                    return;
                }

                target = players[0];

                if (!target.IsLoggedIn)
                {
                    args.Player.SendErrorMessage("That player is not logged in.");
                    return;
                }

                tsUser = target.Account;
            }

            if (tsUser == null)
            {
                args.Player.SendErrorMessage("Invalid player.");
                return;
            }

            PlayerHousingKLP accdata;
            if (!MainKLP.MainDBManager.TryGetPlayerHouseDataByAccountID(tsUser.ID, out accdata))
            {
                args.Player.SendErrorMessage("Unable to get User Data!");
                return;
            }

            int amount;
            if (!int.TryParse(args.Parameters[2], out amount))
            {
                args.Player.SendErrorMessage("Invalid Amount!");
                return;
            }
            if (amount <= 0)
            {
                args.Player.SendErrorMessage("Invalid Amount!");
                return;
            }


            if (accdata.AddSlot(amount))
            {
                args.Player.SendSuccessMessage($"Successfully add {amount} maxhouseslot to {target.Name}.");
                if (target != null) { target.SendMessage($"you were given {amount} maxhouseslot from {args.Player.Name}", Color.LightSteelBlue); }
            } else
            {
                args.Player.SendErrorMessage($"Unable to give {amount} maxhouseslot to {target.Name}");
            }

            return;
        }

        #endregion

        #region [ Sub-Command : removehouseslot ]
        public static void HouseRemoveHouseSlotSubCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 3)
            {
                args.Player.SendErrorMessage("Proper Syntax: /houseadmin removehouseslot [account] [amount]");
                return;
            }

            UserAccount tsUser = TShock.UserAccounts.GetUserAccountByName(args.Parameters[1]);
            TSPlayer target = null;

            if (tsUser != null)
            {
                // See if this account is currently online.
                target = TShock.Players.FirstOrDefault(p =>
                    p != null &&
                    p.IsLoggedIn &&
                    p.Account?.ID == tsUser.ID);
            }
            else
            {
                // Fall back to searching online players.
                var players = TSPlayer.FindByNameOrID(args.Parameters[1]);

                if (players.Count > 1)
                {
                    args.Player.SendMultipleMatchError(players.Select(p => p.Name));
                    return;
                }

                if (players.Count == 0)
                {
                    args.Player.SendErrorMessage("Invalid player.");
                    return;
                }

                target = players[0];

                if (!target.IsLoggedIn)
                {
                    args.Player.SendErrorMessage("That player is not logged in.");
                    return;
                }

                tsUser = target.Account;
            }

            if (tsUser == null)
            {
                args.Player.SendErrorMessage("Invalid player.");
                return;
            }

            PlayerHousingKLP accdata;
            if (!MainKLP.MainDBManager.TryGetPlayerHouseDataByAccountID(tsUser.ID, out accdata))
            {
                args.Player.SendErrorMessage("Unable to get User Data!");
                return;
            }

            int amount;
            if (!int.TryParse(args.Parameters[2], out amount))
            {
                args.Player.SendErrorMessage("Invalid Amount!");
                return;
            }
            if (amount <= 0)
            {
                args.Player.SendErrorMessage("Invalid Amount!");
                return;
            }


            if (accdata.RemoveSlot(amount))
            {
                args.Player.SendSuccessMessage($"Successfully remove {amount} maxhouseslot to {target.Name}.");
                if (target != null) { target.SendMessage($"your {amount} maxhouseslot was taken away from {args.Player.Name}", Color.LightSteelBlue); }
            } else
            {
                args.Player.SendErrorMessage($"Unable to remove {amount} maxhouseslot to {target.Name}");
            }

            return;
        }

        #endregion
    }
}
