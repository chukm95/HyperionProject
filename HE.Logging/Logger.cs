using System.Collections.Concurrent;
using System.Reflection.PortableExecutable;
using System.Text;

namespace HE.Logging
{
    public class Logger
    {
        //consts
        private const string DATE_TIME_LOG_FILENAME_FORMAT = "dd_MM_yyyy_HH_mm_ss";
        private const string DATE_TIME_LOG_MESSAGE_FORMAT = "";
        private const string LOG_FILE_NAME = "{0}_{1}.log";

        //logger instance 
        private static Logger instance;

        /// <summary>
        /// Start the logger
        /// </summary>
        public static void Run(LoggerSettings loggerSettings)
        {
            if (instance == null)
            {
                instance = new Logger(loggerSettings);
            }
        }

        /// <summary>
        /// Stop the logger and dispose of any handles
        /// </summary>
        public static void Stop()
        {
            if(instance != null)
            {
                Volatile.Write(ref instance.isRunning, false);
                instance.loggingThread.Join();
            }
        }

        private LoggerSettings loggerSettings;
        private Thread loggingThread;
        private bool isRunning;

        private List<LogHandle> logHandles;
        private object logHandleCreationLock;

        private ConcurrentQueue<byte> messageQueue;

        private StreamWriter fileWriter;

        private Logger(LoggerSettings loggerSettings)
        {
            this.loggerSettings = loggerSettings;
            loggingThread = new Thread(LoggingLoop);
            isRunning = true;

            logHandles = new List<LogHandle>();
            logHandleCreationLock = new object();

            messageQueue = new ConcurrentQueue<byte>();

            //check if we write anything to file
            if (loggerSettings.IsFileUsed())
            {
                //get all logging path/dirs
                string logFileName = string.Format(LOG_FILE_NAME, loggerSettings.logName, DateTime.Now.ToString(DATE_TIME_LOG_FILENAME_FORMAT));
                string logFileDirectory = string.Concat(Directory.GetCurrentDirectory(), Path.DirectorySeparatorChar, "Logs", Path.DirectorySeparatorChar);
                string logFilePath = Path.Combine(logFileDirectory, logFileName);

                //create dir if not existing
                if (!Directory.Exists(logFileDirectory))
                    Directory.CreateDirectory(logFileDirectory);

                //create file if not existing
                if(!File.Exists(logFilePath))
                    fileWriter = new StreamWriter(File.Create(logFilePath));
                else
                    fileWriter = new StreamWriter(File.OpenWrite(logFilePath));
            }
            
            loggingThread.Start();
        }

        private void LoggingLoop()
        {
            LogType logType;
            DateTime dateTime;
            StringBuilder title_builder = new StringBuilder();
            StringBuilder message_builder = new StringBuilder();
            StringBuilder header_builder = new StringBuilder();

            using (fileWriter)
            {
                while (isRunning || !messageQueue.IsEmpty)
                {
                    byte id;
                    if (messageQueue.TryDequeue(out id))
                    {
                        //get the data
                        LogHandle handle = logHandles[id];
                        handle.GetMessage(out logType, out dateTime, title_builder, message_builder);

                        //write the data to console and file
                        header_builder.Append($"[{logType.ToString()}][{title_builder.ToString()}][{dateTime.ToString("HH:mm:ss:fff")}]");

                        WriteToFile(logType, header_builder, message_builder);
                        WriteToConsole(logType, header_builder, message_builder);

                        //clear the stringbuilders for next msg
                        title_builder.Clear();
                        message_builder.Clear();
                        header_builder.Clear();
                    }
                }
            }
        }

        private void WriteToFile(LogType logType, StringBuilder header_builder, StringBuilder message_builder)
        {
            if (!loggerSettings.IsUsedInFile(logType) || fileWriter == null)
                return;

            fileWriter.WriteLine(header_builder.ToString());
            fileWriter.WriteLine(message_builder.ToString());
        }

        private void WriteToConsole(LogType logType, StringBuilder header_builder, StringBuilder message_builder)
        {
            if (!loggerSettings.IsUsedInConsole(logType))
                return;

            switch(logType)
            {
                case LogType.INFO:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogType.WARNING:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case LogType.ERROR:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogType.FATAL:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogType.DEBUG:
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }

            Console.WriteLine(header_builder.ToString());
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message_builder.ToString());
        }

        public static LogHandle CreateLogHandle()
        {
            lock (instance.logHandleCreationLock)
            {
                LogHandle logHandle = new LogHandle(instance.loggerSettings, (byte)instance.logHandles.Count, instance.messageQueue);
                instance.logHandles.Add(logHandle);
                return logHandle;
            }
        }
    }
}