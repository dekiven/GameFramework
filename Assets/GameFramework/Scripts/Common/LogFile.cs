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
        }

        public static LogLevel MinLevel { set { mMinLevel = value; } get { return mMinLevel; } }

        public static void Init(string filePath, LogLevel minLevel = LogLevel.L_Log)
        {
            mPath = filePath;
            mMinLevel = minLevel;
        }

        public static void CloseLog()
        {
            if (null != mSWriter)
            {
                lock(locker)
                {
                    mSWriter.Close();
                    mSWriter = null;
                }
            }
        }

        public static void Log(string msg)
        {
            Debug.Log(msg);
            writeLine((int)LogLevel.L_Log, msg);
        }

        public static void Log(string format, params object[] args)
        {
            //return;
            Debug.LogFormat(format, args);
            writeLine((int)LogLevel.L_Log, format, args);
        }

        public static void Warn(string msg)
        {
            Debug.LogWarning(msg);
            writeLine((int)LogLevel.L_Warning, msg);
        }

        public static void Warn(string format, params object[] args)
        {
            Debug.LogWarningFormat(format, args);
            writeLine((int)LogLevel.L_Warning, format, args);
        }

        public static void Error(string msg)
        {
            Debug.LogError(msg);
            writeLine((int)LogLevel.L_Error, msg);
        }

        public static void Error(string format, params object[] args)
        {
            Debug.LogErrorFormat(format, args);
            writeLine((int)LogLevel.L_Error, format, args);
        }

        public static void WriteLine(string str)
        {
            lock (locker)
            {
                mSWriter.WriteLine(str);
                mSWriter.Flush();
            }
        }

        private static void writeLine(int level, string format, params object[] args)
        {
            writeLine(level, string.Format(format, args));
        }

        private static void writeLine(int level, string msg)
        {
            if (null == mSWriter)
            {
                lock(locker)
                {
                    checkeHasWriter();
                }
            }
            if (null != mSWriter && level >= (int)mMinLevel)
            {
                string l = "log";
                switch (level)
                {
                    case 1:
                        break;
                    case 2:
                        l = "warning";
                        break;
                    case 3:
                        l = "error";
                        break;
                }
                string str = string.Format("{0} [{1}] ---> {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff"), l, msg);
                WriteLine(str);
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
            Tools.CheckFileExists(mPath, true);
            FileStream stream = new FileStream(mPath, FileMode.Create);
            mSWriter = new StreamWriter(stream);
        }
    }

}