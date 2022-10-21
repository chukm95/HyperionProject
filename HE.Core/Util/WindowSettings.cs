using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HE.Core.Util
{
    public struct WindowSettings
    {
        public readonly int width;
        public readonly int height;
        public readonly WindowBorder border;
        public readonly WindowState state;

        public WindowSettings()
        {
            width = 1280;
            height = 720;
            border = WindowBorder.Fixed;
            state = WindowState.Normal;
        }

        public WindowSettings(int width, int height)
        {
            this.width = width;
            this.height = height;
            border = WindowBorder.Fixed;
            state = WindowState.Normal;
        }

        public WindowSettings(int width, int height, WindowBorder border, WindowState state)
        {
            this.width = width;
            this.height = height;
            this.border = border;
            this.state = state;
        }
    }
}
