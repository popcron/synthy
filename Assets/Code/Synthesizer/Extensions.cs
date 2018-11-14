using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Popcron.Synth
{
    public static class Extensions
    {
        public static double Lerp(this double a, double b, double t)
        {
            double l = b * t + a * (1.0 - t);
            return l;
        }
    }
}
