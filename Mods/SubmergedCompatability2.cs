using BCLDLL.Interfaces;
using BCLDLL.Maps;
using System;

namespace BCLDLL.Mods
{
    public class SubmergedCompatability2 : IModCompatability
    {
        public override string MOD_GUID => "Submerged";


        internal IMapCompatability MapCompatability = null;
    }
}
