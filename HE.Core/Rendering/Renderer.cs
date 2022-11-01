using HE.Core.Rendering.Shaders;
using HE.Logging;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HE.Core.Rendering
{
    public class Renderer
    {
        private static Renderer instance;

        public static void Start(NativeWindow nativeWindow, Barrier syncBarrier)
        {
            if (instance == null)
            {
                instance = new Renderer(nativeWindow, syncBarrier);
            }
        }

        public static void Stop()
        {
            if (instance != null)
            {
                Volatile.Write(ref instance.isRunning, false);
                instance.syncBarrier.RemoveParticipant();
                instance.renderThread.Join();
            }
        }

        public static ShaderManager ShaderManager
        {
            get => instance.shaderManager;
        }

        private Thread renderThread;
        private NativeWindow nativeWindow;
        private Barrier syncBarrier;
        private LogHandle logHandle;

        private ShaderManager shaderManager;

        private bool isRunning;

        private Renderer(NativeWindow nativeWindow, Barrier syncBarrier)
        {
            renderThread = new Thread(RenderLoop);
            this.nativeWindow = nativeWindow;
            this.syncBarrier = syncBarrier;
            logHandle = Logger.CreateLogHandle();

            isRunning = true;
            renderThread.Start();
        }

        private void RenderLoop()
        {
            logHandle.WriteInfo("Renderer", "Renderer started!");
            nativeWindow.Context.MakeCurrent();

            shaderManager = new ShaderManager();

            //setup sync
            syncBarrier.SignalAndWait();

            while (isRunning)
            {
                PreUpdate();
                Update();
                PostUpdate();
            }

            shaderManager.Deinitialize();

            logHandle.WriteInfo("Renderer", "Renderer stopped!");
        }

        private void PreUpdate()
        {
            shaderManager.Update(logHandle);
        }

        private void Update()
        {

        }

        private void PostUpdate()
        {
            nativeWindow.Context.SwapBuffers();
            syncBarrier.SignalAndWait();

            syncBarrier.SignalAndWait();
        }

    }
}
