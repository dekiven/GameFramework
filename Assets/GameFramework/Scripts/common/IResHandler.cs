using System;
namespace GameFramework
{
    public interface IResHandler<T> 
    {
        void Load(string asbName, string resName);
        void Load(string asbName, string[] resNames);
        T Get(string asbName, string resName);
        void SetCurGroup(string group);
        void ClearGroup(string group);
        string FixResName(string name);
    }
}
