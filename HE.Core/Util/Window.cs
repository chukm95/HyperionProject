using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HE.Core.Util
{
    public delegate void OnResize(int width, int height);
    public delegate void OnFocusChange(bool isFocused);
    public delegate void OnCloseRequested();

    public class Window
    {
        internal NativeWindow NativeWindow
        {
            get => nativeWindow;
        }

        public event OnResize OnResize;
        public event OnFocusChange OnFocusChange;
        public event OnCloseRequested OnCloseRequested;

        private NativeWindow nativeWindow; 

        internal Window(string title, WindowSettings windowSettings)
        {
            NativeWindowSettings nws = new NativeWindowSettings();
            nws.Title = title;
            nws.Size = new Vector2i(windowSettings.width, windowSettings.height);
            nws.WindowBorder = windowSettings.border;
            nws.WindowState = windowSettings.state;
            nws.StartFocused = true;
            nws.StartVisible = true;
            nws.Profile = ContextProfile.Core;
            nws.API = ContextAPI.OpenGL;
            nws.APIVersion = new Version(4, 5);

            nativeWindow = new NativeWindow(nws);

            nativeWindow.Resize += NativeWindow_Resize;
            nativeWindow.FocusedChanged += NativeWindow_FocusedChanged;
            nativeWindow.Closing += NativeWindow_Closing;
            nativeWindow.Context.MakeNoneCurrent();
            Core.LogHandle.WriteInfo("Window", "Window initialized!");
        }
        private void NativeWindow_Resize(OpenTK.Windowing.Common.ResizeEventArgs obj)
        {
            OnResize?.Invoke(obj.Width, obj.Height);
        }

        private void NativeWindow_FocusedChanged(OpenTK.Windowing.Common.FocusedChangedEventArgs obj)
        {
            OnFocusChange?.Invoke(obj.IsFocused);
        }

        private void NativeWindow_Closing(System.ComponentModel.CancelEventArgs obj)
        {
            obj.Cancel = true;
            OnCloseRequested?.Invoke();
        }

        internal void ProcessEvents()
        {
            NativeWindow.ProcessWindowEvents(false);
        }

        internal void Deinitialize()
        {
            nativeWindow.Close();
            nativeWindow.Dispose();

            Core.LogHandle.WriteInfo("Window", "Window deinitialized!");
        }
    }
}
