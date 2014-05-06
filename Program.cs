using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

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
      var transformed = original.Select(a => new Tuple<double[], double>(Phi(a), a[2])).ToList();
      var xM = DenseMatrix.Create(transformed.Count, transformed[0].Item1.Length, (r, c) => transformed[r].Item1[c]);
      var yV = DenseVector.OfEnumerable(transformed.Select(t => t.Item2));
      var answer = PseudoInverse(xM) * yV;
      
      Console.WriteLine(answer);



      Console.ReadKey(true);
    }

    public static DenseMatrix PseudoInverse(DenseMatrix m)
    {
      var mt = m.Transpose();
      var what = (mt * m);
      return (DenseMatrix)(what.Inverse() * mt);
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
