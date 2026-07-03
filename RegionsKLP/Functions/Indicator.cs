using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;
using DPoint = System.Drawing.Point;

namespace RegionsKLP.Functions
{
    internal static class Indicator
    {
        public static void SendFakeTilesCross(TSPlayer player, DPoint crossLocation, bool setOrUnset = true, byte TileColor = Terraria.ID.PaintID.DeepRedPaint)
        {
            SendFakeTile(player, crossLocation, setOrUnset, TileColor);
            crossLocation.Offset(-1, 0);
            SendFakeTile(player, crossLocation, setOrUnset, TileColor);
            crossLocation.Offset(1, 0);
            SendFakeTile(player, crossLocation, setOrUnset, TileColor);
            crossLocation.Offset(0, -1);
            SendFakeTile(player, crossLocation, setOrUnset, TileColor);
            crossLocation.Offset(0, 1);
            SendFakeTile(player, crossLocation, setOrUnset, TileColor);
        }
        public static void SendFakeTilesCrossTimed(TSPlayer player, DPoint crossLocation, int timeMs = 4000, byte TileColor = Terraria.ID.PaintID.DeepRedPaint)
        {
            SendE(true);

            System.Threading.Timer hideTimer = null;
            hideTimer = new System.Threading.Timer(state => {
                SendE(false);

                // ReSharper disable AccessToModifiedClosure
                Debug.Assert(hideTimer != null);
                hideTimer.Dispose();
                // ReSharper restore AccessToModifiedClosure
            },
              null, timeMs, Timeout.Infinite
            );

            void SendE(bool setOrUnset)
            {
                SendFakeTile(player, crossLocation, setOrUnset, TileColor);
                crossLocation.Offset(-1, 0);
                SendFakeTile(player, crossLocation, setOrUnset, TileColor);
                crossLocation.Offset(1, 0);
                SendFakeTile(player, crossLocation, setOrUnset, TileColor);
                crossLocation.Offset(0, -1);
                SendFakeTile(player, crossLocation, setOrUnset, TileColor);
                crossLocation.Offset(0, 1);
                SendFakeTile(player, crossLocation, setOrUnset, TileColor);
            }
        }
        public static void SendAreaDottedFakeTilesTimed(TSPlayer player, Rectangle area, int timeMs, byte TileColor = Terraria.ID.PaintID.DeepRedPaint)
        {
            SendAreaDottedFakeTiles(player, area, TileColor: TileColor);

            System.Threading.Timer hideTimer = null;
            hideTimer = new System.Threading.Timer(state => {
                SendAreaDottedFakeTiles(player, area, false, TileColor);

                // ReSharper disable AccessToModifiedClosure
                Debug.Assert(hideTimer != null);
                hideTimer.Dispose();
                // ReSharper restore AccessToModifiedClosure
            },
              null, timeMs, Timeout.Infinite
            );

        }
        public static void SendAreaDottedFakeTiles(TSPlayer player, Rectangle area, bool setOrUnset = true, byte TileColor = Terraria.ID.PaintID.DeepRedPaint)
        {
            foreach (Point boundaryPoint in TShock.Utils.EnumerateRegionBoundaries(area))
            {
                if ((boundaryPoint.X + boundaryPoint.Y & 1) == 0)
                {
                    SendFakeTile(player, new DPoint(boundaryPoint.X, boundaryPoint.Y), setOrUnset, TileColor);
                }
            }
        }

        public static void SendFakeTile(TSPlayer player, DPoint tileLocation, bool setOrUnset = true, byte TileColor = Terraria.ID.PaintID.DeepRedPaint)
        {
            #region [ Send FakeTile ]
            if (!setOrUnset)
            {
                player.SendTileSquareCentered(tileLocation.X, tileLocation.Y, 1);
                player.SendData(PacketTypes.PaintTile, number: tileLocation.X, number2: tileLocation.Y);
                return;
            }


            Tile previoustile = CopyTile((Tile)Main.tile[tileLocation.X, tileLocation.Y]);
            ushort previousstileheader = previoustile.sTileHeader;
            ushort previoustype = previoustile.type;
            byte previouscolor = previoustile.color();
            bool previousactuated = previoustile.actuator();
            bool previousbright = previoustile.fullbrightBlock();
            ITile CurrentTile = Main.tile[tileLocation.X, tileLocation.Y];

            if (!CurrentTile.active())
            {
                CurrentTile.type = 127;
                CurrentTile.active(true);
                CurrentTile.actuator(true);
                CurrentTile.inActive(true);

                CurrentTile.fullbrightBlock(true);

                CurrentTile.color(TileColor);


                player.SendTileSquareCentered(tileLocation.X, tileLocation.Y, 1);

                CurrentTile.color(previouscolor);
                CurrentTile.sTileHeader = previousstileheader;
                CurrentTile.actuator(previousactuated);
                CurrentTile.inActive(previousactuated);

                CurrentTile.fullbrightBlock(previousbright);

                CurrentTile.active(false);
                CurrentTile.type = previoustype;

            }
            else
            {
                CurrentTile.color(TileColor);
                CurrentTile.fullbrightBlock(true);

                player.SendTileSquareCentered(tileLocation.X, tileLocation.Y, 1);

                CurrentTile.sTileHeader = previousstileheader;
                CurrentTile.color(previouscolor);
                CurrentTile.fullbrightBlock(previousbright);
                CurrentTile.actuator(previousactuated);
                CurrentTile.inActive(previousactuated);

                CurrentTile = previoustile;
            }

            return;

            Tile CopyTile(Tile originalTile)
            {
                // Create a new tile
                Tile copiedTile = new Tile();

                // Copy all relevant properties from the original tile to the copied tile
                copiedTile.active(originalTile.active());
                copiedTile.type = originalTile.type;
                copiedTile.wall = originalTile.wall;
                copiedTile.sTileHeader = originalTile.sTileHeader;
                copiedTile.bTileHeader = originalTile.bTileHeader;
                copiedTile.bTileHeader2 = originalTile.bTileHeader2;
                copiedTile.bTileHeader3 = originalTile.bTileHeader3;
                copiedTile.frameX = originalTile.frameX;
                copiedTile.frameY = originalTile.frameY;
                copiedTile.color(originalTile.color());
                copiedTile.wallColor(originalTile.wallColor());
                copiedTile.liquid = originalTile.liquid;
                copiedTile.liquidType(originalTile.liquidType());
                copiedTile.wire(originalTile.wire());
                copiedTile.wire2(originalTile.wire2());
                copiedTile.wire3(originalTile.wire3());
                copiedTile.wire4(originalTile.wire4());
                copiedTile.halfBrick(originalTile.halfBrick());
                copiedTile.slope(originalTile.slope());
                copiedTile.actuator(originalTile.actuator());
                copiedTile.inActive(originalTile.inActive());
                copiedTile.wall = originalTile.wall;
                copiedTile.wallColor(originalTile.wallColor());
                //copiedTile.blockType(originalTile.blockType());

                // Return the copied tile
                return copiedTile;
            }
            #endregion
        }

    }
}
