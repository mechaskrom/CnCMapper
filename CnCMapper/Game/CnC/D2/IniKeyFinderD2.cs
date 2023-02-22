using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.D2
{
    class IniKeyFinderD2 : IniKeyFinderCnC
    {
        private IniKeyFinderD2(IniSection section)
            : base(section)
        {
        }

        public static IniKeyFinderD2 create(IniSection section)
        {
            return section != null ? new IniKeyFinderD2(section) : null;
        }

        protected override IniKey find(IniKey keyToFind)
        {
            return findD2TD(Keys, keyToFind);
        }
    }
}
