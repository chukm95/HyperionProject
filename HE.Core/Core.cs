using HE.Core.FileManagement;
using HE.Core.Rendering;
using HE.Core.Rendering.Shaders;
using HE.Core.TaskManagement;
using HE.Core.Util;
using HE.Logging;
using OpenTK.Windowing.Desktop;
using System.Text;

namespace HE.Core
{
    public class Core
    {
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

        public static TaskManager TaskManager
        {
            get => instance.taskManager;
        }

        public static FileManager FileManager
        {
            get => instance.fileManager;
        }

        public static Window Window
        {
            get => instance.window;
        }

        public static GameTime GameTime
        {
            get => instance.gameTime;
        }

        private Application application;
        private Barrier syncBarrier;
        private LogHandle logHandle;
        private bool isRunning;

        private TaskManager taskManager;
        private FileManager fileManager;
        private Window window;
        private GameTime gameTime;

        private Core(Application application)
        {
            instance = this;
            this.application = application;
            syncBarrier = new Barrier(2);
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

            //init components
            taskManager = new TaskManager();
            fileManager = new FileManager();
            window = new Window(application.Title, application.WindowSettings);
            gameTime = new GameTime();
            Renderer.Start(window.NativeWindow, syncBarrier);

            //init  client side
            if (application.CloseOnRequested)
                window.OnCloseRequested += () => { Stop(); };

            //renderer setup sync
            syncBarrier.SignalAndWait();

            application.Initialize();

            //init complete msg
            logHandle.WriteInfo("Initialization", "Initialization finished!");

            Shader sh = Renderer.ShaderManager.CreateShader("*/DefaultShader.hsf");
        }

        private void PreUpdate()
        {
            gameTime.Update();
            fileManager.Update();
            NativeWindow.ProcessWindowEvents(false);
        }

        private void Update()
        {

        }

        private void PostUpdate()
        {
            syncBarrier.SignalAndWait();
            syncBarrier.SignalAndWait();
        }

        private void Deinitialize()
        {
            //deinit msg
            logHandle.WriteInfo("Deinitialization", "Deinitialization started...");

            application.Deinitialize();

            Renderer.Stop();

            window.Deinitialize();

            taskManager.Deinitialize();

            //deinit complete msg
            logHandle.WriteInfo("Deinitialization", "Deinitialization finished!");
            Logger.Stop();
            Environment.Exit(0);
        }
    }
}