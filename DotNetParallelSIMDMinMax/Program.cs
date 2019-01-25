using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace DotNetParallelSIMDMinMax
{
    class Program
    {
        public class ConsoleAppNetCore21
        {
            private static ushort[] inputA = new ushort[] { };
            private static ushort[] inputB = new ushort[] { };
            private static ushort[] inputC = new ushort[] { };
            private static ushort minA;
            private static ushort maxA;
            private static ushort minB;
            private static ushort maxB;
            private static ushort minC;
            private static ushort maxC;

            public static void Main()
            {
                FillArray(ref inputA);
                FillArray(ref inputB);
                FillArray(ref inputC);
                ParallelSIMDMinMax();
            }

            public static void ParallelSIMDMinMax()
            {
                var simdLength = Vector<ushort>.Count;
                var vmin = new Vector<ushort>(ushort.MaxValue);
                var vmax = new Vector<ushort>(ushort.MinValue);
                var i = 0;

                // Find the max and min for each of Vector<ushort>.Count sub-arrays 
                Parallel.ForEach(Enumerable.Range(0, inputC.Length - simdLength).Select(j => j * simdLength), new ParallelOptions { }, j =>
                // Next line is working non-parallel SIMD version:
                //for (i = 0; i <= inputC.Length - simdLength; i += simdLength)
                {
                    var va = new Vector<ushort>(inputC, j);
                    vmin = Vector.Min(va, vmin);
                    vmax = Vector.Max(va, vmax);
                });

                // Find the max and min of all sub-arrays
                minC = ushort.MaxValue;
                maxC = ushort.MinValue;
                for (var j = 0; j < simdLength; ++j)
                {
                    minC = Math.Min(minC, vmin[j]);
                    maxC = Math.Max(maxC, vmax[j]);
                }

                // Process any remaining elements
                for (; i < inputC.Length; ++i)
                {
                    minC = Math.Min(minC, inputC[i]);
                    maxC = Math.Max(maxC, inputC[i]);
                }

                Console.WriteLine($"minC = {minC}, maxC = {maxC}");
            }


            private static void FillArray(ref ushort[] arr)
            {
                ushort Min = 0;
                ushort Max = 65535;
                Random randNum = new Random();
                arr = Enumerable
                    .Repeat(0, 65568)
                    .Select(i => (ushort)randNum.Next(Min, Max))
                    .ToArray();
            }
        }
    }
}
