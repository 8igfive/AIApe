using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buaa.AIBot.Utils
{
    public static class Tools
    {
        public static IEnumerable<int> Range(int end)
        {
            if (end < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            for (int i = 0; i < end; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<int> Range(int beg, int end)
        {
            if (beg > end)
            {
                throw new ArgumentOutOfRangeException();
            }
            for (int i = beg; i < end; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<int> Range(int beg, int end, int step)
        {
            if (step < 0)
            {
                if (beg < end)
                {
                    throw new ArgumentOutOfRangeException();
                }
                for (int i = beg; i > end; i += step)
                {
                    yield return i;
                }
            }
            else if (step > 0)
            {
                if (beg > end)
                {
                    throw new ArgumentOutOfRangeException();
                }
                for (int i = beg; i < end; i += step)
                {
                    yield return i;
                }
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}
