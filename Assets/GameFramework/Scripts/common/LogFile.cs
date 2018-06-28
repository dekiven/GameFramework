//using UnityEngine;

using System;
using System.IO;

public class LogFile
{
    private static object locker = new object();
    private static StreamWriter mSWriter;
    private static LogLevel mMinLevel;

    public enum LogLevel
    {
        L_Log = 1,
        L_Warning = 2,
        L_Error = 3,
    }

    public LogLevel MinLevel { set { mMinLevel = value; } get { return mMinLevel; } }

    public static void Init(string filePath, LogLevel minLevel = LogLevel.L_Log)
    {
        Y3Tools.CheckFileExists(filePath, true);
        mMinLevel = minLevel;
        FileStream stream = new FileStream(filePath, FileMode.Create);
        mSWriter = new StreamWriter(stream);
    }

    public static void CloseLog()
    {
        if (null != mSWriter)
        {
            mSWriter.Close();
            mSWriter = null;
        }
    }

    public static void Log(string msg)
    {
        //Debug.Log(msg);
        writeLine((int)LogLevel.L_Log, msg);
    }

    public static void Log(string format, params object[] args)
    {
        //Debug.LogFormat(format, args);
        writeLine((int)LogLevel.L_Log, format, args);
    }

    public static void Warn(string msg)
    {
        //Debug.LogWarning(msg);
        writeLine((int)LogLevel.L_Warning, msg);
    }

    public static void Warn(string format, params object[] args)
    {
        //Debug.LogWarningFormat(format, args);
        writeLine((int)LogLevel.L_Warning, format, args);
    }

    public static void Error(string msg)
    {
        //Debug.LogError(msg);
        writeLine((int)LogLevel.L_Error, msg);
    }

    public static void Error(string format, params object[] args)
    {
        //Debug.LogErrorFormat(format, args);
        writeLine((int)LogLevel.L_Error, format, args);
    }

    private static void writeLine(int level, string format, params object[] args)
    {
        writeLine(level, string.Format(format, args));
    }

    private static void writeLine(int level, string msg)
    {
        if (null != mSWriter && level >= (int)mMinLevel)
        {
            lock (locker)
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
                mSWriter.WriteLine(str);
                mSWriter.Flush();
            }
        }
    }

}
