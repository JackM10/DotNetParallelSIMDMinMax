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
            private static int[] Array = new int[] { };
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
                FillArray(ref Array);
                //FillArray(ref inputB);
                //FillArray(ref inputC);
                // ParallelSIMDMinMax();
                //Console.WriteLine(ParallelArrSum());
                TaskParallelArrSum();
                Console.ReadKey();
            }

            public static int ArrSum()
            {
                int vectorSize = Vector<int>.Count;
                var accVector = Vector<int>.Zero;
                int i;
                var array = Array;
                for (i = 0; i < array.Length - vectorSize; i += vectorSize)
                {
                    var v = new Vector<int>(array, i);
                    accVector = Vector.Add(accVector, v);
                }
                int result = Vector.Dot(accVector, Vector<int>.One);
                for (; i < array.Length; i++)
                {
                    result += array[i];
                }
                return result;
            }

            public static int TaskParallelArrSum()
            {
                var task1 = new Task(() => TaskParallelArrSumHelper(0, 16392));
                task1.Start();
                var task2 = new Task(() => TaskParallelArrSumHelper(16392, 32784));
                task2.Start();
                var task3 = new Task(() => TaskParallelArrSumHelper(32784, 49176));
                task3.Start();
                var task4 = new Task(() => TaskParallelArrSumHelper(49176, 65568));
                task4.Start();

                Task.WaitAll(task1, task2);

                return 0;
            }

            private static int TaskParallelArrSumHelper(int i, int arrLengthToProcess)
            {
                int vectorSize = Vector<int>.Count;
                var accVector = Vector<int>.Zero;                
                var array = Array;
                for (; i < arrLengthToProcess - vectorSize; i += vectorSize)
                {
                    var v = new Vector<int>(array, i);
                    accVector = Vector.Add(accVector, v);
                }
                int result = Vector.Dot(accVector, Vector<int>.One);
                for (; i < arrLengthToProcess; i++)
                {
                    result += array[i];
                }

                return result;
            }

            public static int ParallelArrSum()
            {
                int simdLength = Vector<int>.Count;
                var accVector = Vector<int>.Zero;
                int i;
                var array = Array;
                //for (i = 0; i < array.Length - vectorSize; i += vectorSize)
                Parallel.ForEach(Enumerable.Range(0, array.Length - simdLength).Select(j => j * simdLength), new ParallelOptions { }, j =>
                {
                    var v = new Vector<int>(array, j);
                    accVector = Vector.Add(accVector, v);
                    // i = j;
                });
                int result = Vector.Dot(accVector, Vector<int>.One);
                //for (; i < array.Length; i++)
                //{
                //    result += array[i];
                //}
                return result;
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


            private static void FillArray(ref int[] arr)
            {
                ushort Min = 0;
                ushort Max = 65535;
                Random randNum = new Random();
                arr = Enumerable
                    .Repeat(0, 65568)
                    .Select(i => randNum.Next(Min, Max))
                    .ToArray();
            }
        }
    }
}
