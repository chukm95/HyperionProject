// See https://aka.ms/new-console-template for more information
using HE.Logging;

LoggerSettings ls = new LoggerSettings("LogTest", LogType.ALL, LogType.ALL);
Logger.Run(ls);
LogHandle lh = Logger.CreateLogHandle();
lh.WriteInfo("INFO TEST", "info test");
lh.WriteWarning("WARNING TEST", "warning test");
lh.WriteError("ERROR TEST", "error test");
lh.WriteFatal("FATAL TEST", "fatal test");
lh.WriteDebug("DEBUG TEST", "debug test");
Logger.Stop();