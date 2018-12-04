//using UnityEngine;

using System;
using System.IO;
using UnityEngine;

namespace GameFramework
{
    public class LogFile
    {
        private static object locker = new object();
        private static StreamWriter mSWriter;
        private static LogLevel mMinLevel;
        private static string mPath;
        private static bool mHasShowInitErr = false;

        public enum LogLevel
        {
            L_Log = 1,
            L_Warning = 2,
            L_Error = 3,
            L_Assert = 4,
            L_Exception = 5,
        }

        public static LogLevel MinLevel { set { mMinLevel = value; } get { return mMinLevel; } }

        public static void Init(string filePath, LogLevel minLevel = LogLevel.L_Log)
        {
            lock (locker)
            {
                CloseLog();

                mPath = filePath;
                mMinLevel = minLevel;

                Application.logMessageReceived += handleLogCallback;
            }
        }

        public static void CloseLog()
        {
            if (null != mSWriter)
            {
                lock(locker)
                {
                    mSWriter.Close();
                    mSWriter = null;
                    mPath = null;
                    mMinLevel = LogLevel.L_Log;
                }
            }
        }

        public static void Log(string msg)
        {
            Debug.Log(msg);
        }

        public static void Log(string format, params object[] args)
        {
            Debug.LogFormat(format, args);
        }

        public static void Warn(string msg)
        {
            Debug.LogWarning(msg);
        }

        public static void Warn(string format, params object[] args)
        {
            Debug.LogWarningFormat(format, args);
        }

        public static void Error(string msg)
        {
            Debug.LogError(msg);
        }

        public static void Error(string format, params object[] args)
        {
            Debug.LogErrorFormat(format, args);
            WriteLine((int)LogLevel.L_Error, format, args);
        }

        public static void WriteLine(string str)
        {
            if (null == mSWriter)
            {
                lock (locker)
                {
                    checkeHasWriter();
                }
            }
            if (null != mSWriter)
            {
                lock (locker)
                {
                    mSWriter.WriteLine(str);
                    mSWriter.Flush();
                }
            }
        }

        public static void WriteLine(LogLevel level, string format, params object[] args)
        {
            WriteLine((int)level, format, args);
        }

        public static void WriteLine(LogLevel level, string msg)
        {
            WriteLine((int)level, msg);
        }

        public static void WriteLine(int level, string format, params object[] args)
        {
            WriteLine(level, string.Format(format, args));
        }

        public static void WriteLine(int level, string msg)
        {
            if (level >= (int)mMinLevel)
            {
                string l = "   log   ";
                switch (level)
                {
                    case 1:
                        break;
                    case 2:
                        l = " warning ";
                        break;
                    case 3:
                        l = "  error  ";
                        break;
                    case 4:
                        l = " assert  ";
                        break;
                    case 5:
                        l = "exception";
                        break;
                }
                WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff") + " [" + l + "] ===> " + msg);
            }
        }

        private static void checkeHasWriter()
        {
            if(string.IsNullOrEmpty(mPath))
            {
                if(!mHasShowInitErr)
                {
                    Debug.LogWarning("不能写入日志文件，请先调用Init方法初始化LogFile。");
                    mHasShowInitErr = true;
                }
                return;
            }
            //TODO:IOException：Sharing Violation on Path 解决

            lock (locker)
            {
                if (Tools.CheckFileExists(mPath))
                {
                    String fileName = mPath;
                    fileName = fileName.Insert(mPath.LastIndexOf('/') + 1, "old_");
                    Tools.RenameFile(mPath, fileName);
                }
                if (Tools.CheckFileExists(mPath))
                {
                    File.Delete(mPath);
                }
                Tools.CheckFileExists(mPath, true);
                FileStream stream = new FileStream(mPath, FileMode.Create);
                mSWriter = new StreamWriter(stream);
            }
        }

        private static void handleLogCallback(string condition, string stackTrace, LogType type)
        {
            switch(type)
            {
                case LogType.Log :
                    WriteLine(LogLevel.L_Log, condition);
                    break;
                case LogType.Warning :
                    WriteLine(LogLevel.L_Warning, condition);
                    break;
                case LogType.Error :
                    WriteLine(LogLevel.L_Error, condition+"\n\nstackTrace: --> "+stackTrace);
                    break;
                case LogType.Exception :
                    WriteLine(LogLevel.L_Exception, condition + "\n\nstackTrace: --> " + stackTrace);
                    break;
                case LogType.Assert :
                    WriteLine(LogLevel.L_Assert, condition + "\n\nstackTrace: --> " + stackTrace);
                    break;
            }
        }
    }

}