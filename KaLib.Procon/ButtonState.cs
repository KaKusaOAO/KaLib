﻿using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace KaLib.Procon
{
    public struct ButtonState
    {
        public Vector2 LeftStick { get; }
        public Vector2 RightStick { get; }

        public List<(Button button, bool pressed)> Buttons { get; }
        internal InputPacket Source { get; }

        private static readonly Button[] ButtonLeftBitmap = {
            Button.DPadDown,
            Button.DPadUp,
            Button.DPadRight,
            Button.DPadLeft,
            Button.None,
            Button.None,
            Button.L,
            Button.ZL
        };
        
        private static readonly Button[] ButtonRightBitmap = {
            Button.Y,
            Button.X,
            Button.B,
            Button.A,
            Button.None,
            Button.None,
            Button.R,
            Button.ZR
        };
        
        private static readonly Button[] ButtonMiddleBitmap = {
            Button.Minus,
            Button.Plus,
            Button.RStick,
            Button.LStick,
            Button.Home,
            Button.Share,
            Button.None,
            Button.None
        };

        private Button[] GetButtonMap(ButtonSource source)
        {
            return source switch
            {
                ButtonSource.Left => ButtonLeftBitmap,
                ButtonSource.Middle => ButtonMiddleBitmap,
                ButtonSource.Right => ButtonRightBitmap,
                _ => throw new InvalidDataException()
            };
        }

        private List<(Button button, bool pressed)> PullButtonsFromByte(byte c, ButtonSource src)
        {
            var result = new List<(Button, bool)>();
            var map = GetButtonMap(src);
            for (var i = 0; i < map.Length; i++)
            {
                if (map[i] == Button.None) continue;
                result.Add((map[i], (c & (1 << i)) != 0));
            }
            return result;
        }

        internal ButtonState(InputPacket packet)
        {
            Source = packet;
            
            var lx = StickByteToDouble((byte) ((packet.Sticks[1] & 0xf) << 4 | (packet.Sticks[0] & 0xf0) >> 4));
            var ly = StickByteToDouble(packet.Sticks[2]);
            var rx = StickByteToDouble((byte) ((packet.Sticks[4] & 0xf) << 4 | (packet.Sticks[3] & 0xf0) >> 4));
            var ry = StickByteToDouble(packet.Sticks[5]);
            LeftStick = new Vector2(lx, ly);
            RightStick = new Vector2(rx, ry);

            Buttons = new();
            Buttons.AddRange(PullButtonsFromByte(packet.LeftButtons, ButtonSource.Left));
            Buttons.AddRange(PullButtonsFromByte(packet.MiddleButtons, ButtonSource.Middle));
            Buttons.AddRange(PullButtonsFromByte(packet.RightButtons, ButtonSource.Right));
        }

        private static float StickByteToDouble(byte b) => b / 255f * 2 - 1;
    }
}