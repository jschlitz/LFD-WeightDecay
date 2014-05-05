using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace LFD_WeightDecay
{
  class Program
  {
    static double Sq(double x) { return x * x; }

    static void Main(string[] args)
    {
      if (args.Length < 2)
      {
        Console.WriteLine("Usage: LFD-WeightDecay insample.txt outsample.txt");
        return;
      }

      var original = GetFromFile(args[0]);
      var transformed = original.Select(a => new Tuple<double[], double>(Phi(a), a[2]));

      foreach (var item in original)
      {
        Console.WriteLine("{0}, {1}, {2}", item[0], item[1], item[2]);
      }

      Console.ReadKey(true);
    }

    private static double[] Phi(double[] a)
    {
      return new[] { 1.0, a[0], a[1], Sq(a[0]), Sq(a[1]), a[0] * a[1], Math.Abs(a[0] - a[1]), Math.Abs(a[0] + a[1]) };
    }

    private static List<double[]> GetFromFile(string fileName)
    {
      var data = new List<string>();
      using (var inf = new StreamReader(fileName))
      {
        while (inf.Peek() >= 0)
          data.Add(inf.ReadLine());
      }
      return data.Select(s => s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
        .Where(a => a.Length == 3)
        .Select(a => a.Select(s => double.Parse(s)).ToArray())
        .ToList();
    }



  }
}
