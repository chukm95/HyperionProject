using HE.Core.Util;
using HE.Logging;
using HE.Rendering;

namespace HE.Core
{
    public class Core
    {
        private const int SyncParticipants = 2;

        private static Core instance;

        public static void Run(Application application)
        {
            if(instance == null)
                instance = new Core(application);
        }

        public static void Stop()
        {
            if(instance!= null)
            {
                Volatile.Write(ref instance.isRunning, false);
            }
        }

        public static LogHandle LogHandle
        {
            get => instance.logHandle;
        }

        public static Window Window
        {
            get => instance.window;
        }

        private Application application;
        private LogHandle logHandle;
        private Barrier syncBarrier;
        private bool isRunning;

        private Window window;

        private Core(Application application)
        {
            instance = this;
            this.application = application;
            Initialize();
            while(isRunning)
            {
                PreUpdate();
                Update();
                PostUpdate();
            }
            Deinitialize();
        }

        private void Initialize()
        {
            //init the logger
            Logger.Run(application.LoggerSettings);
            logHandle = Logger.CreateLogHandle();

            //init msg
            logHandle.WriteInfo("Initialization", "Initialization started...");

            //set running
            isRunning = true;
            //create sync barrier for sync between audio rendering gamelogic
            syncBarrier = new Barrier(SyncParticipants);

            //init components
            window = new Window(application.Title, application.WindowSettings);

            Renderer.Start(window.NativeWindow, syncBarrier);

            //init  client side
            if (application.CloseOnRequested)
                window.OnCloseRequested += () => { Stop(); };

            application.Initialize();

            //init complete msg
            logHandle.WriteInfo("Initialization", "Initialization finished!");
        }

        private void PreUpdate()
        {
            window.ProcessEvents();
        }

        private void Update()
        {

        }

        private void PostUpdate()
        {
            syncBarrier.SignalAndWait();
        }

        private void Deinitialize()
        {
            //deinit msg
            logHandle.WriteInfo("Deinitialization", "Deinitialization started...");

            application.Deinitialize();

            Renderer.Stop();

            window.Deinitialize();

            //deinit complete msg
            logHandle.WriteInfo("Deinitialization", "Deinitialization finished!");
            Logger.Stop();
            Environment.Exit(0);
        }
    }
}