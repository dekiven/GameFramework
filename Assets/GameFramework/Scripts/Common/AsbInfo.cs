using System;
namespace GameFramework
{
    public class AsbInfo : IEquatable<AsbInfo>
    {
        public string asbName;
        public string assetName;
        public string extral;

        public AsbInfo()
        {
        }

        public AsbInfo(string asbName, string assetName)
        {
            Set(asbName, assetName);
        }

        public AsbInfo(string asbName, string assetName, string extral)
        {
            Set(asbName, assetName, extral);
        }

        public bool Equals(string asbName, string assetName)
        {
            return this.asbName.Equals(asbName) && this.assetName.Equals(assetName);
        }

        public bool Equals(AsbInfo other)
        {
            return this.asbName.Equals(other.asbName) && this.assetName.Equals(other.assetName);
        }

        public static bool Equals(AsbInfo info, AsbInfo info2)
        {
            return info.asbName.Equals(info2.asbName) && info.assetName.Equals(info2.assetName);
        }

        public void Set(string asbName, string assetName, string extral = null)
        {
            this.asbName = asbName;
            this.assetName = assetName;
            this.extral = extral;
        }
    }
}
