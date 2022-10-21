using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HE.Logging
{
    [Flags]
    public enum LogType : byte
    {
        INFO = 1,
        WARNING = 2,
        ERROR = 4,
        FATAL = 8,
        DEBUG = 16,
        ALL = 31,
        NONE = 0
    }

    public class LogHandle
    {
        private LoggerSettings loggerSettings;
        private byte handleId;
        private ConcurrentQueue<byte> logHandleMessageQueue;

        private AnonymousPipeServerStream pipe_in;
        private AnonymousPipeClientStream pipe_out;

        /// <summary>
        /// Creates a new Log handle that allows to write log messages
        /// </summary>
        /// <param name="handleID">unique id</param>
        /// <param name="logHandleMessageQueue">stores loghandles that have a message</param>
        internal LogHandle(LoggerSettings loggerSettings, byte handleID, ConcurrentQueue<byte> logHandleMessageQueue)
        {
            this.loggerSettings = loggerSettings;
            this.handleId = handleID;
            this.logHandleMessageQueue = logHandleMessageQueue;

            pipe_in = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            pipe_out = new AnonymousPipeClientStream(PipeDirection.Out, pipe_in.ClientSafePipeHandle);
        }

        public void WriteInfo(string title, string message)
        {
            if (loggerSettings.IsUsed(LogType.INFO))
                WriteMessage(LogType.INFO, title, message);
        }

        public void WriteWarning(string title, string message)
        {
            if (loggerSettings.IsUsed(LogType.WARNING))
                WriteMessage(LogType.WARNING, title, message);
        }

        public void WriteError(string title, string message)
        {
            if (loggerSettings.IsUsed(LogType.ERROR))
                WriteMessage(LogType.ERROR, title, message);
        }

        public void WriteFatal(string title, string message)
        {
            if (loggerSettings.IsUsed(LogType.FATAL))
                WriteMessage(LogType.FATAL, title, message);
        }

        public void WriteDebug(string title, string message)
        {
            if (loggerSettings.IsUsed(LogType.DEBUG))
                WriteMessage(LogType.DEBUG, title, message);
        }

        /// <summary>
        /// Client side methode for sending message over pipe to the logger thread
        /// </summary>
        /// <param name="type">message type</param>
        /// <param name="title">message title</param>
        /// <param name="message">message</param>
        private void WriteMessage(LogType type, string title, string message)
        {
            //send the type
            byte typeId = (byte)type;
            pipe_out.WriteByte(typeId);

            //send the Datetime
            DateTime timestamp = DateTime.Now;
            Span<byte> timestamp_data = stackalloc byte[sizeof(long)];
            BitConverter.TryWriteBytes(timestamp_data, timestamp.ToBinary());
            pipe_out.Write(timestamp_data);

            //send the length of the title
            Span<byte> title_length_data = stackalloc byte[sizeof(int)];
            int title_length = Encoding.Unicode.GetByteCount(title);
            BitConverter.TryWriteBytes(title_length_data, title_length);
            pipe_out.Write(title_length_data);

            //send the title
            Span<byte> title_data = stackalloc byte[title_length];
            Encoding.Unicode.GetBytes(title.AsSpan(), title_data);
            pipe_out.Write(title_data);

            //send the length of the message
            Span<byte> message_length_data = stackalloc byte[sizeof(int)];
            int message_length = Encoding.Unicode.GetByteCount(message);
            BitConverter.TryWriteBytes(message_length_data, message_length);
            pipe_out.Write(message_length_data);

            //send the message data
            Span<byte> message_data = stackalloc byte[message_length];
            Encoding.Unicode.GetBytes(message.AsSpan(), message_data);
            pipe_out.Write(message_data);

            //tell the logger we have just submitted a message and it needs to be processed
            logHandleMessageQueue.Enqueue(handleId);
        }

        /// <summary>
        /// Read a LogMessage on the logger thread through the pipe
        /// </summary>
        /// <param name="messageData"></param>
        internal void GetMessage(out LogType logType, out DateTime dateTime, StringBuilder titleBuilder, StringBuilder messageBuilder)
        {
            //read the log type
            logType = (LogType)pipe_in.ReadByte();

            //read the datetime
            Span<byte> dateTimeData = stackalloc byte[sizeof(long)];
            pipe_in.Read(dateTimeData);
            dateTime = DateTime.FromBinary(BitConverter.ToInt64(dateTimeData));

            //read title length
            Span<byte> title_length_data = stackalloc byte[sizeof(int)];
            pipe_in.Read(title_length_data);
            int title_length = BitConverter.ToInt32(title_length_data);

            //read the number of bytes indicated we would recieve in title length
            Span<byte> title_data = stackalloc byte[title_length];
            pipe_in.Read(title_data);
            titleBuilder.Append(Encoding.Unicode.GetString(title_data));

            //read the message length
            Span<byte> message_length_data = stackalloc byte[sizeof(int)];
            pipe_in.Read(message_length_data);
            int message_length = BitConverter.ToInt32(message_length_data);

            //read the number of bytes indicated we would recieve in message length
            Span<byte> message_data = stackalloc byte[message_length];
            pipe_in.Read(message_data);
            messageBuilder.Append(Encoding.Unicode.GetString(message_data));
        }

        internal void Close()
        {
            pipe_in.Close();
            pipe_out.Close();
        }
    }
}
