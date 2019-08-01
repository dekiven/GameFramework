using System;

namespace GameFramework
{
    public abstract class DisposableObj : IDisposable
    {
        bool mDisposed = false;
        /// <summary>
        /// 析构函数，既终结器，编译后变成 protected void Finalize()，GC会在回收对象前会调用调用该方法
        /// 在没有显示调用 Dispose 方法的情况下释放非托管资源
        /// </summary>
        ~DisposableObj()
        {
            _dispose(false);
        }

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
        void _dispose(bool disposing)
        {
            if (mDisposed)
            {
                return;
            }

            _disposUnmananged();

            if (disposing)
            {
                _disposMananged();
                //通知垃圾回收器不再调用终结器
                GC.SuppressFinalize(this);
            }

            mDisposed = true;
        }

        /// <summary>
        /// 回收非托管资源，如 WWW, FileStream 等
        /// </summary>
        protected abstract void _disposUnmananged();

        /// <summary>
        /// 回收托管资源，如 基本类型和不包含非托管对象的对象
        /// </summary>
        protected abstract void _disposMananged();

        // IDisposable 接口实现，显示释放资源，提高效率
        public void Dispose()
        {
            _dispose(true);
        }
    }
}
