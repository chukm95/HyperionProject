using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HE.Logging
{
    public struct LoggerSettings
    {
        public readonly string logName;
        public readonly LogType fileOutput;
        public readonly LogType consoleOutput;

        public LoggerSettings(string logName)
        {
            this.logName = logName;
            fileOutput = LogType.ALL;
            consoleOutput = LogType.ALL;
        }

        public LoggerSettings(string logName, LogType fileOutput, LogType consoleOutput)
        {
            this.logName = logName;
            this.fileOutput = fileOutput;
            this.consoleOutput = consoleOutput;
        }

        public bool IsUsedInFile(LogType logType)
        {
            return (fileOutput & logType) == logType;
        }

        public bool IsFileUsed()
        {
            return (fileOutput & LogType.ALL) != 0;
        }

        public bool IsUsedInConsole(LogType logType)
        {
            return (consoleOutput & logType) == logType;
        }

        public bool IsUsed(LogType logType)
        {
            return ((fileOutput | consoleOutput) & logType) == logType;
        }
    }
}
