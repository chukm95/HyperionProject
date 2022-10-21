using OpenTK.Windowing.Desktop;

namespace HE.Rendering
{
    public class Renderer
    {
        private static Renderer instance;

        public static void Start(NativeWindow nativeWindow, Barrier syncBarrier)
        {
            if(instance == null)
            {
                instance = new Renderer(nativeWindow, syncBarrier);
            }
        }

        public static void Stop()
        {
            if(instance != null)
            {
                Volatile.Write(ref instance.isRunning, false);
                instance.syncBarrier.RemoveParticipant();
                instance.renderThread.Join();
            }
        }

        private Thread renderThread;
        private NativeWindow nativeWindow;
        private Barrier syncBarrier;
        private bool isRunning;

        private Renderer(NativeWindow nativeWindow, Barrier syncBarrier)
        {
            renderThread = new Thread(RenderLoop);
            this.nativeWindow = nativeWindow;
            this.syncBarrier = syncBarrier;
            isRunning = true;
            renderThread.Start();
        }

        private void RenderLoop()
        {

        }

    }
}