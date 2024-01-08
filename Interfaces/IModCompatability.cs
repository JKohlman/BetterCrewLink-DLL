using System;

namespace BCLDLL.Interfaces
{
    public abstract class IModCompatability
    {
        public abstract string MOD_GUID { get; }
        internal IMapCompatability MapCompatability { get; }

    }
}
