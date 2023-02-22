using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA
{
    //TODO: Maybe use INI-key finder in all sprite sections (currently only used by smudges)?
    //Duplicate keys are rare though and probably an INI-file format error so not worth the trouble/overhead?

    //This is for mimicking how INI-keys in INI-sections are iterated by Tiberian Dawn and Red Alert.
    //Mainly to try to mimic the effect of duplicate keys (keys with the same id) in an INI-section.
    abstract class IniKeyFinderTDRA : IniKeyFinderCnC
    {
        protected IniKeyFinderTDRA(IniSection section)
            : base(section)
        {
        }
    }
}
