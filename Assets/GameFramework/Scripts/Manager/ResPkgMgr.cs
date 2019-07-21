using System.Collections.Generic;

namespace GameFramework
{
    public class ResPkgMgr : Singleton<ResPkgMgr>
    {
        public List<ResPkgInfo> Pkgs { get { return new List<ResPkgInfo>(mPkgs); }}
        List<ResPkgInfo> mPkgs;

        public List<string> AllPkgNames { get { return mAllPkgNames; }}
        List<string> mAllPkgNames;

    }


    public struct ResPkgInfo
    {
        string name;
        string version;
    }
}