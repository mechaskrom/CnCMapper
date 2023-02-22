using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    class IniKeyFinderTD : IniKeyFinderTDRA
    {
        private IniKeyFinderTD(IniSection section)
            : base(section)
        {
        }

        public static IniKeyFinderTD create(IniSection section)
        {
            return section != null ? new IniKeyFinderTD(section) : null;
        }

        protected override IniKey find(IniKey keyToFind)
        {
            return findD2TD(Keys, keyToFind);
        }
    }
}
