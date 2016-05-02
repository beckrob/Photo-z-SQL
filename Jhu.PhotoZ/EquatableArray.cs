using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.PhotoZ
{
    public class EquatableArray<T> : IEquatable<EquatableArray<T>>
    {

        public T[] array;

        public EquatableArray()
        {
            array = null;
        }

        public EquatableArray(T[] aArray)
        {
            array = aArray;
        }

        public override bool Equals(object aOther)
        {
            return aOther is EquatableArray<T> && Equals((EquatableArray<T>)aOther);
        }

        public bool Equals(EquatableArray<T> aOther)
        {
            bool thisNull = ReferenceEquals(array, null);
            bool otherNull = ReferenceEquals(aOther.array, null);

            if (thisNull && otherNull)
            {
                return true;
            }

            if (!thisNull && !otherNull)
            {
                return array.SequenceEqual(aOther.array);
            }

            return false;
        }

        public override int GetHashCode()
        {
            int hash = 0;

            if (!ReferenceEquals(array, null))
            {
                hash = 17;

                for (int i=0; i<array.Length; ++i)
                {
                    hash = hash * 23 + array[i].GetHashCode();
                }
            }

            return hash;
        }

    }
}
