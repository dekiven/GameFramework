using LuaInterface;
using System;
using System.Collections.Generic;

namespace GameFramework
{
    public class MsgBoxMgr : Singleton<MsgBoxMgr>
    {
        Stack<MsgBoxInfo> mStack = new Stack<MsgBoxInfo>();

        public MsgBoxInfo MsgInfo
        {
            get
            {
                if (mStack.Count > 0)
                {
                    return mStack.Peek();
                }
                return null;
            }
        }

        public void ShowMsg(string msg, Action<int> call)
        {
            ShowMsg(LanguageManager.GetStr("提示"), msg, call);
        }

        /// <summary>
        /// 显示默认的确认对话框
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="msg">消息内容</param>
        /// <param name="call">回调</param>
        /// <param name="luaCall">lua回调</param>
        public void ShowMsg(string title, string msg, Action<int> call)
        {
            var info = new MsgBoxInfo()
            {
                Title = title,
                Msg = msg,
                BtnTxtM = LanguageManager.GetStr("确定"),
                ClickCallback = call,
            };

            mStack.Push(info);
            _showMsg();
        }

        public void ShowMsg(MsgBoxInfo info)
        {
            mStack.Push(info);
            _showMsg();
        }

        public void ShowMsg(LuaTable table)
        {
            var info = new MsgBoxInfo(table);
            mStack.Push(info);
            _showMsg();
        }

        public void ShowNext()
        {
            MsgBoxInfo info = mStack.Pop();
            if (null != info)
            {
                info.Dispose();
            }
            if (mStack.Count > 0)
            {
                _showMsg();
            }
        }

        void _showMsg()
        {
            //TODO:
            GameUIManager.Instance.ShowView("BasicRes", "PlMsgBox");
        }
    }

    public class MsgBoxInfo : DisposableObj
    {
        public string Title;
        public string Msg;
        public string BtnTxtL;
        public string BtnTxtM;
        public string BtnTxtR;
        public Action<int> ClickCallback;
        public LuaFunction LuaFunc;

        LuaTable mTable;

        public MsgBoxInfo()
        {

        }

        public MsgBoxInfo(LuaTable lua)
        {
            mTable = lua;

            //必定有的key
            Msg = mTable.RawGet<string, string>("msg");
            LuaFunc = mTable.RawGet<string, LuaFunction>("func");

            //可选的key
            Title = mTable.RawGet<string, string>("title");
            BtnTxtL = mTable.RawGet<string, string>("btnTxtL");
            BtnTxtM = mTable.RawGet<string, string>("btnTxtM");
            BtnTxtR = mTable.RawGet<string, string>("btnTxtR");
        }

        protected override void _disposMananged()
        {
            Title = null;
            Msg = null;
            BtnTxtL = null;
            BtnTxtM = null;
            BtnTxtR = null;
            ClickCallback = null;
        }

        protected override void _disposUnmananged()
        {
            if (null != LuaFunc)
            {
                LuaFunc.Dispose();
                LuaFunc = null;
            }

            if(null != mTable)
            {
                mTable.Dispose();
                mTable = null;
            }
        }
    }

}