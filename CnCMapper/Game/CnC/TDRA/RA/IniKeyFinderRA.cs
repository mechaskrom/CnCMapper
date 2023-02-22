using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    class IniKeyFinderRA : IniKeyFinderTDRA
    {
        //THIS DOES NOT CURRENTLY MATCH THE GAME!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        //Failed attempt to mimic effects of duplicate keys in Red Alert.
        //RA appears to do a binary search on a quicksorted list to find the INI-key line in an INI-section.
        //See "INIClass::INISection::Find_Entry()" in "INI.CPP".
        //And especially "IndexClass<T>::Is_Present() and "IndexClass<T>::Search_For_Node()" in "SEARCH.H".

        //TODO: This INI-key finder does not currently match RA. Figure out why? Not worth bothering with?
        //Probably because of differences in how the sorting algorithm handles duplicate entries.
        //Maybe also in how the search algorithm handles duplicate entries?
        //Difficult to find out without knowing how the "qsort()" and "bsearch()" used in RA work.

        private class KeyWithCrc
        {
            public readonly IniKey mIniKey;
            public readonly UInt32 mCrc; //RA creates a crc value from the key's id and uses it for sort and search.

            public KeyWithCrc(IniKey iniKey)
            {
                mIniKey = iniKey;
                mCrc = Crypt.Crc.crcTDRA(iniKey.Id);
            }
        }

        private class KeyComparer : IComparer<KeyWithCrc>
        {
            public int Compare(KeyWithCrc x, KeyWithCrc y)
            {
                return x.mCrc.CompareTo(y.mCrc);
            }
        }
        private static readonly KeyComparer mComparer = new KeyComparer();

        private KeyWithCrc mLastFound;
        private readonly List<KeyWithCrc> mKeysSorted;

        private IniKeyFinderRA(IniSection section)
            : base(section)
        {
            mLastFound = null;
            mKeysSorted = getKeysSorted(Keys);
        }

        public static IniKeyFinderRA create(IniSection section)
        {
            return section != null ? new IniKeyFinderRA(section) : null;
        }

        protected override IniKey find(IniKey keyToFind)
        {
            KeyWithCrc keyWithCrcToFind = new KeyWithCrc(keyToFind);
            if (mLastFound != null && mLastFound.mCrc == keyWithCrcToFind.mCrc) //Same as last key found?
            {
                return mLastFound.mIniKey;
            }

            int index = mKeysSorted.BinarySearch(keyWithCrcToFind, mComparer); //Binary search of quicksorted keys.
            if (index >= 0) //Key was found?
            {
                mLastFound = mKeysSorted[index];
                return mLastFound.mIniKey;
            }

            throw new ArgumentException("INI-key to find must be in INI-section!");
        }

        private static List<KeyWithCrc> getKeysSorted(List<IniKey> keys)
        {
            List<KeyWithCrc> keysSorted = new List<KeyWithCrc>();
            foreach (IniKey key in keys)
            {
                keysSorted.Add(new KeyWithCrc(key));
            }

            //List<T>.Sort() does not always use quicksort? See remarks at bottom of this page:
            //https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1.sort?view=net-5.0
            //keysSorted.Sort(mComparer); //Does not always use quicksort? Does not match game anyway.

            //This seems to be a bit closer to what "qsort()" does in Red Alert, but not exactly the same?
            Quicksort.sort(keysSorted);

            return keysSorted;
        }

        private static class Quicksort
        {
            //Adapted from version here: https://www.coderslexicon.com/quicksort-definitive-c-c-java-vb-net-php/

            public static void sort(List<KeyWithCrc> keys)
            {
                sort(keys, 0, keys.Count - 1);
            }

            //Quicksort controller function, it partitions the different pieces of our array.
            private static void sort(List<KeyWithCrc> keys, int left, int right)
            {
                if (right > left)
                {
                    int pivotIndex = left;
                    int pivotNewIndex = partition(keys, left, right, pivotIndex);

                    //Recursive call to quicksort to sort each half.
                    sort(keys, left, pivotNewIndex - 1);
                    sort(keys, pivotNewIndex + 1, right);
                }
            }

            //This function takes an array (or one half an array) and sorts it.
            //It then returns a new pivot index number back to quicksort.
            private static int partition(List<KeyWithCrc> keys, int left, int right, int pivot)
            {
                UInt32 pivotValue = keys[pivot].mCrc;

                //Swap pivot out all the way to the end of the array so we know where it always is.
                swap(keys, pivot, right);
                int storeIndex = left;

                //Move through the array from start to finish comparing each to value at pivot index.
                for (int i = left; i < right; i++)
                {
                    if (keys[i].mCrc <= pivotValue)
                    {
                        swap(keys, i, storeIndex);
                        storeIndex++;
                    }
                }
                swap(keys, storeIndex, right);
                return storeIndex;
            }

            private static void swap(List<KeyWithCrc> keys, int index1, int index2)
            {
                KeyWithCrc tmp = keys[index1];
                keys[index1] = keys[index2];
                keys[index2] = tmp;
            }
        }
    }
}
