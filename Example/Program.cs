using System;
using System.Linq;
using DynamicLinq;

namespace Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const int n = 100000;
            Random r = new Random();

            Console.WriteLine("n: {0}", n);

            var s = Enumerable.Range(0, n).Select(i => Tuple.Create(i, r.Next(100, 200), r.NextDouble())).ToArray();

            using (Job.StartNew("OrderBy"))
            {
                foreach (var t in s.OrderBy("Item2").Take(5))
                    Console.WriteLine(t);
            }

            using (Job.StartNew("Where(typed)"))
            {
                Console.WriteLine(s.WhereReflection("Item2", (int p) => p < 150).Count());
            }

            using (Job.StartNew("Where(typed)"))
            {
                Console.WriteLine(s.Where("Item2", (int p) => p < 150).Count());
            }

            using (Job.StartNew("Where(dynamic)"))
            {
                Console.WriteLine(s.Where("Item2", p => p < 150).Count());
            }
        }
    }

    class Job : System.IDisposable
    {
        public static Job StartNew(string name)
        {
            return new Job(name);
        }

        private string _name;
        private System.Diagnostics.Stopwatch _stopwatch;

        private Job(string name)
        {
            this._name = name;
            this._stopwatch = System.Diagnostics.Stopwatch.StartNew();
            System.Console.WriteLine("[{0} started]", this._name);
        }

        public void Dispose()
        {
            this._stopwatch.Stop();
            System.Console.WriteLine("[{0} finished] {1}ms elapsed", this._name, this._stopwatch.ElapsedMilliseconds);
        }
    }
}
