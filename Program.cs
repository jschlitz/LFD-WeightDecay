using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
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

      DenseMatrix xM;
      DenseVector yV;
      var fromFile = GetFromFile(args[0]);
      GetLinRegArgs(fromFile.Take(25), out xM, out yV);

      DenseMatrix xMval;
      DenseVector yVval;
      GetLinRegArgs(fromFile.Skip(25), out xMval, out yVval);

      DenseMatrix xMout;
      DenseVector yVout;
      GetLinRegArgs(GetFromFile(args[1]), out xMout, out yVout);

      for (int i = 3; i <= 8; i++)
			{
        
        var xMi = DenseMatrix.OfColumnVectors(
          xM.ColumnEnumerator().Select(t=>t.Item2).Take(i).ToArray());

        var weights = PseudoInverse(xMi) * yV;
        Console.WriteLine(weights);


        var xMvalI = DenseMatrix.OfColumnVectors(
          xMval.ColumnEnumerator().Select(t => t.Item2).Take(i).ToArray());
        var eVal = Error(weights, xMvalI, yVval);
        Console.WriteLine("Eval == {0}", eVal);

        var xMoutI = DenseMatrix.OfColumnVectors(
          xMout.ColumnEnumerator().Select(t => t.Item2).Take(i).ToArray());
        var eOut = Error(weights, xMoutI, yVout);
        Console.WriteLine("Eout == {0}", eOut);


			}
      //DoWeightDecay(args[1], xM, yV);





      Console.ReadKey(true);
    }

    private static void DoWeightDecay(string fileName, DenseMatrix xM, DenseVector yV)
    {
      for (int k = -3; k <= 3; k++)
      {
        Console.WriteLine(" ---------- {0} ----------", k);
        var weights = GetRegularizedWeights(xM, Math.Pow(10, k), yV);
        var eIn = Error(weights, xM, yV);

        Console.WriteLine(weights);
        Console.WriteLine("Ein == {0}", eIn);
        DenseMatrix xMout;
        DenseVector yVout;


        GetLinRegArgs(GetFromFile(fileName), out xMout, out yVout);
        var eOut = Error(weights, xMout, yVout);
        Console.WriteLine("Eout == {0}", eOut);

      }
    }

    private static double Error(DenseVector weights, DenseMatrix xM, DenseVector yV)
    {
      Debug.Assert(xM.RowCount == yV.Count);
      Debug.Assert(xM.ColumnCount == weights.Count);

      double wrong = 0.0;
      for (int i = 0; i < xM.RowCount; i++)
      {
        var p = xM.Row(i) * weights;
        if ((p < 0 ? -1 : 1) != yV[i])
          wrong += 1;
      }
      return wrong / xM.RowCount;
    }

    private static void GetLinRegArgs(IEnumerable<double[]> items, out DenseMatrix xM, out DenseVector yV)
    {
      var original = items;
      var transformed = original.Select(a => new Tuple<double[], double>(Phi(a), a[2])).ToList();
      xM = DenseMatrix.Create(transformed.Count, transformed[0].Item1.Length, (r, c) => transformed[r].Item1[c]);
      yV = DenseVector.OfEnumerable(transformed.Select(t => t.Item2));
    }

    public static DenseMatrix PseudoInverse(DenseMatrix m)
    {
      var mt = m.Transpose();
      var what = (mt * m);
      return (DenseMatrix)(what.Inverse() * mt);
    }

    public static DenseVector GetRegularizedWeights(DenseMatrix m, double lambda, DenseVector yV)
    {
      var mt = m.Transpose();
      var what = (mt * m);
      var identity = DenseMatrix.Identity(what.ColumnCount);
      return (DenseVector)((what + lambda * identity).Inverse() * mt * yV);
      //return (DenseMatrix)(what.Inverse() * mt);
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
