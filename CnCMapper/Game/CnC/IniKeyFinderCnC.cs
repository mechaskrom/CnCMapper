using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC
{
    //This is for mimicking how INI-keys in INI-sections are iterated by Dune 2, Tiberian Dawn and Red Alert.
    //Mainly to try to mimic the effect of duplicate keys (keys with the same id) in an INI-section.
    abstract class IniKeyFinderCnC
    {
        protected readonly IniSection mSection;

        protected IniKeyFinderCnC(IniSection section)
        {
            mSection = section;
        }

        public List<IniKey> Keys //"Normal" access to section keys.
        {
            get { return mSection.Keys; }
        }

        protected abstract IniKey find(IniKey keyToFind);

        public IEnumerable<IniKey> findKeys() //Mimic how game iterates section keys.
        {
            foreach (IniKey keyToFind in Keys) //Loop over all keys in section.
            {
                IniKey key = find(keyToFind); //Search for it in section.
                yield return key;
            }
        }

        protected static IniKey findD2TD(List<IniKey> keys, IniKey keyToFind)
        {
            //Emulate Dune 2 and Tiberian Dawn behavior and do a linear search for key in section.
            //Seems really wasteful as the values are in the same line we just got the key from.
            //But needed to mimic the effects of duplicate keys in a section.
            foreach (IniKey key in keys) //Search for key's id in section.
            {
                if (key.Id == keyToFind.Id)
                {
                    return key; //Return the first key found with the same id.
                }
            }
            throw new ArgumentException("INI-key to find must be in INI-section!");
        }
    }
}
