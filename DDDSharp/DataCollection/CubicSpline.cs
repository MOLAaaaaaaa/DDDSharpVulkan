//
// Author: Ryan Seghers
//
// Copyright (C) 2013-2014 Ryan Seghers
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the irrevocable, perpetual, worldwide, and royalty-free
// rights to use, copy, modify, merge, publish, distribute, sublicense, 
// display, perform, create derivative works from and/or sell copies of 
// the Software, both in source and object code form, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

//EXAMPLE 1
//// Create the data to be fitted
//float[] x = { 0.5f, 2.0f, 3.0f, 4.5f, 3.0f, 2.0f };
//float[] y = { 4.0f, 2.0f, 6.0f, 4.0f, 3.0f, 5.0f };
//float[] xs, ys;
//CubicSpline.FitParametric(x, y, 100, out xs, out ys);

using System;
using System.Collections.Generic;

namespace DataCollection
{
    /// <summary>
    /// Cubic spline interpolation.
    /// Call Fit (or use the corrector constructor) to compute spline coefficients, then Eval to evaluate the spline at other X coordinates.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is implemented based on the wikipedia article:
    /// http://en.wikipedia.org/wiki/Spline_interpolation
    /// I'm not sure I have the right to include a copy of the article so the equation numbers referenced in 
    /// comments will end up being wrong at some point.
    /// </para>
    /// <para>
    /// This is not optimized, and is not MT safe.
    /// This can extrapolate off the ends of the splines.
    /// You must provide points in X sort order.
    /// </para>
    /// </remarks>
    #region example
    /*
    private static void TestSpline()
    {
        int n = 6;

        // Create the data to be fitted
        float[] x = new float[n];
        float[] y = new float[n];
        Random rand = new Random(1);

        for (int i = 0; i < n; i++)
        {
            x[i] = i;
            y[i] = (float)rand.NextDouble() * 10;
        }

        // Compute the x values at which we will evaluate the spline.
        // Upsample the original data by a const factor.
        int upsampleFactor = 10;
        int nInterpolated = n * upsampleFactor;
        float[] xs = new float[nInterpolated];

        for (int i = 0; i < nInterpolated; i++)
        {
            xs[i] = (float)i * (n - 1) / (float)(nInterpolated - 1);
        }

        float[] ys = CubicSpline.Compute(x, y, xs, 0.0f, Single.NaN, true);

        string path = @"..\..\testSpline.png";
        PlotSplineSolution("Cubic Spline Interpolation - Random Data", x, y, xs, ys, path);
    }

    private static void TestPerf()
    {
        int n = 10000;

        // Create the data to be fitted
        float[] x = new float[n];
        float[] y = new float[n];
        Random rand = new Random(1);

        for (int i = 0; i < n; i++)
        {
            x[i] = i;
            y[i] = (float)rand.NextDouble() * 10;
        }

        // Compute the x values that we will evaluate the spline at.
        // Upsample the original data by a const factor.
        int upsampleFactor = 10;
        int nInterpolate = n * upsampleFactor;
        float[] xs = new float[nInterpolate];

        for (int i = 0; i < nInterpolate; i++)
        {
            xs[i] = (float)i / upsampleFactor;
        }

        // For perf, test multiple reps
        int reps = 100;
        DateTime start = DateTime.Now;

        for (int i = 0; i < reps; i++)
        {
            float[] ys = CubicSpline.Compute(x, y, xs);
        }

        TimeSpan duration = DateTime.Now - start;
        Console.WriteLine("CubicSpline upsample from {0:n0} to {1:n0} points took {2:0.00} ms for {3} iterations ({2:0.000} ms per iteration)",
            n, nInterpolate, duration.TotalMilliseconds, reps, duration.TotalMilliseconds / reps);

        // Compare to NRinC
        //float[] y2 = new float[n];
        //float[] ys2 = new float[nInterpolate];
        //start = DateTime.Now;

        //for (int i = 0; i < reps; i++)
        //{
        //	CubicSplineNR.Spline(x, y, y2);
        //	CubicSplineNR.EvalSpline(x, y, y2, xs, ys2);
        //}

        //duration = DateTime.Now - start;
        //Console.WriteLine("CubicSplineNR upsample from {0:n0} to {1:n0} points took {2:0.00} ms for {3} iterations ({2:0.000} ms per iteration)",
        //	n, nInterpolate, duration.TotalMilliseconds, reps, duration.TotalMilliseconds / reps);
    }

    /// <summary>
    /// This is the Wikipedia "Spline Interpolation" article example.
    /// </summary>
    private static void TestSplineOnWikipediaExample()
    {
        // Create the test points.
        float[] x = new float[] { -1.0f, 0.0f, 3.0f };
        float[] y = new float[] { 0.5f, 0.0f, 3.0f };

        Console.WriteLine("x: {0}", ArrayUtil.ToString(x));
        Console.WriteLine("y: {0}", ArrayUtil.ToString(y));

        // Create the upsampled X values to interpolate
        int n = 20;
        float[] xs = new float[n];
        float stepSize = (x[x.Length - 1] - x[0]) / (n - 1);

        for (int i = 0; i < n; i++)
        {
            xs[i] = x[0] + i * stepSize;
        }

        // Fit and eval
        CubicSpline spline = new CubicSpline();
        float[] ys = spline.FitAndEval(x, y, xs);

        Console.WriteLine("xs: {0}", ArrayUtil.ToString(xs));
        Console.WriteLine("ys: {0}", ArrayUtil.ToString(ys));

        // Plot
        string path = @"..\..\spline-wikipedia.png";
        PlotSplineSolution("Cubic Spline Interpolation - Wikipedia Example", x, y, xs, ys, path);

        // Try slope, spline is already computed at this point
        float[] slope = spline.EvalSlope(xs);
        string slopePath = @"..\..\spline-wikipedia-slope.png";
        PlotSplineSolution("Cubic Spline Interpolation - Wikipedia Example - Slope", x, y, xs, ys, slopePath, slope);
    }

    private static TriDiagonalMatrixF TestTdm()
    {
        TriDiagonalMatrixF m = new TriDiagonalMatrixF(10);

        for (int i = 0; i < m.N; i++)
        {
            m.A[i] = 1.111111f;
            m.B[i] = 2.222222f;
            m.C[i] = 3.333333f;
        }

        Console.WriteLine("Matrix:\n{0}", m.ToDisplayString(",4:0.00", "    "));

        for (int i = 0; i < m.N; i++)
        {
            m[i, i] = 4.4444f;
        }

        Console.WriteLine("Matrix:\n{0}", m.ToDisplayString(",4:0.00", "    "));

        // Solve
        Random rand = new Random(1);
        float[] d = new float[m.N];

        for (int i = 0; i < d.Length; i++)
        {
            d[i] = (float)rand.NextDouble();
        }

        float[] x = m.Solve(d);

        Console.WriteLine("Solve returned: ");

        for (int i = 0; i < x.Length; i++)
        {
            Console.Write("{0:0.0000}, ", x[i]);
        }

        Console.WriteLine();
        return m;
    }
    */
	# endregion

    public class CubicSpline
	{
		#region Fields

		// N-1 spline coefficients for N points
		private double[] a;
		private double[] b;

		// Save the original x and y for Eval
		private double[] xOrig;
		private double[] yOrig;

		#endregion

		#region Ctor

		/// <summary>
		/// Default ctor.
		/// </summary>
		public CubicSpline()
		{
		}

		/// <summary>
		/// Construct and call Fit.
		/// </summary>
		/// <param name="x">Input. X coordinates to fit.</param>
		/// <param name="y">Input. Y coordinates to fit.</param>
		/// <param name="startSlope">Optional slope constraint for the first point. Single.NaN means no constraint.</param>
		/// <param name="endSlope">Optional slope constraint for the final point. Single.NaN means no constraint.</param>
		/// <param name="debug">Turn on console output. Default is false.</param>
		public CubicSpline(double[] x, double[] y, double startSlope = double.NaN, double endSlope = double.NaN)
		{
			Fit(x, y, startSlope, endSlope);
		}
		public List<Vector64> CreateSpline(List<Vector32> points, int interpolated = 100)
		{
			double[] xs, ys;
			double[] x = new double[points.Count];
			double[] y = new double[points.Count];
			for (int i = 0; i < points.Count; i++)
			{				
				x[i] = points[i].x;
				y[i] = points[i].y;
			}			
			FitParametric(x, y, interpolated, out xs, out ys);
			List<Vector64> outpoints = new List<Vector64>();
			for (int i = 0; i < ys.Length; i++)
			{
				outpoints.Add(new Vector64(xs[i], ys[i], 0));
			}
			xs = null;
			ys = null;
			return outpoints;
		}
		public List<Vector64> CreateSpline(List<Vector64> points, int interpolated = 100)
		{
			double[] xs, ys;
			double[] x = new double[points.Count];
			double[] y = new double[points.Count];
			for (int i = 0; i < points.Count; i++)
			{
				x[i] = points[i].x;
				y[i] = points[i].y;
			}
			FitParametric(x, y, interpolated, out xs, out ys);
			List<Vector64> outpoints = new List<Vector64>();
			for (int i = 0; i < ys.Length; i++)
			{
				outpoints.Add(new Vector64(xs[i], ys[i], 0));
			}
			xs = null;
			ys = null;
			return outpoints;
		}
		/*
		public List<Vector64>CreateSpline(List<Vector32>points,int interpolated = 10)
		{
			Vector32 p1, p2;
			double step=0,len = 0,xx;
			for(int i=0;i<points.Count-1;i++)
			{
				p1 = points[i];
				p2 = points[i+1];
				len += p1.Distance(p2);
			}
			
			step = len / (interpolated*points.Count);
			
			List<float> xs = new List<float>();		
			for (int i = 0; i < points.Count-1; i++)
			{
				p1 = points[i];
				p2 = points[i+1];
				len = p1.Distance(p2);
				if (len > step)
				{
					for( int j = 0; j < len / step; j++)
					 xs.Add( (float)(p1.x + j * step ) );				
				}
				else xs.Add(p1.x);
			}			
			xs.Add(points[points.Count-1].x);
			float[] x = new float[points.Count];
			float[] y = new float[points.Count];
			for (int i = 0; i < points.Count; i++)
			{
				p1 = points[i];
				x[i] = p1.x;
				y[i] = p1.y;
			}
			float[] ys = Compute(x, y, xs.ToArray());
			List<Vector64> outpoints = new List<Vector64>();

			for(int i=0;i<ys.Length;i++)
			{
				outpoints.Add(new Vector64(xs[i], ys[i], 0));
			}
			xs = null;
			ys = null;
			return outpoints;
		}
		*/
		#endregion

		#region Private Methods

		/// <summary>
		/// Throws if Fit has not been called.
		/// </summary>
		private void CheckAlreadyFitted()
		{
			if (a == null) throw new Exception("Fit must be called before you can evaluate.");
		}

		private int _lastIndex = 0;

		/// <summary>
		/// Find where in xOrig the specified x falls, by simultaneous traverse.
		/// This allows xs to be less than x[0] and/or greater than x[n-1]. So allows extrapolation.
		/// This keeps state, so requires that x be sorted and xs called in ascending order, and is not multi-thread safe.
		/// </summary>
		private int GetNextXIndex(double x)
		{
			if (x < xOrig[_lastIndex])
			{
				throw new ArgumentException("The X values to evaluate must be sorted.");
			}

			while ((_lastIndex < xOrig.Length - 2) && (x > xOrig[_lastIndex + 1]))
			{
				_lastIndex++;
			}

			return _lastIndex;
		}

		/// <summary>
		/// Evaluate the specified x value using the specified spline.
		/// </summary>
		/// <param name="x">The x value.</param>
		/// <param name="j">Which spline to use.</param>
		/// <param name="debug">Turn on console output. Default is false.</param>
		/// <returns>The y value.</returns>
		private double EvalSpline(double x, int j, bool debug = false)
		{
			double dx = xOrig[j + 1] - xOrig[j];
			double t = (x - xOrig[j]) / dx;
			double y = (1 - t) * yOrig[j] + t * yOrig[j + 1] + t * (1 - t) * (a[j] * (1 - t) + b[j] * t); // equation 9
			if (debug) Console.WriteLine("xs = {0}, j = {1}, t = {2}", x, j, t);
			return y;
		}

		#endregion

		#region Fit*

		/// <summary>
		/// Fit x,y and then eval at points xs and return the corresponding y's.
		/// This does the "natural spline" style for ends.
		/// This can extrapolate off the ends of the splines.
		/// You must provide points in X sort order.
		/// </summary>
		/// <param name="x">Input. X coordinates to fit.</param>
		/// <param name="y">Input. Y coordinates to fit.</param>
		/// <param name="xs">Input. X coordinates to evaluate the fitted curve at.</param>
		/// <param name="startSlope">Optional slope constraint for the first point. Single.NaN means no constraint.</param>
		/// <param name="endSlope">Optional slope constraint for the final point. Single.NaN means no constraint.</param>
		/// <param name="debug">Turn on console output. Default is false.</param>
		/// <returns>The computed y values for each xs.</returns>
		public double[] FitAndEval(double[] x, double[] y, double[] xs, double startSlope = double.NaN, double endSlope = double.NaN)
		{
			Fit(x, y, startSlope, endSlope);
			return Eval(xs);
		}

		/// <summary>
		/// Compute spline coefficients for the specified x,y points.
		/// This does the "natural spline" style for ends.
		/// This can extrapolate off the ends of the splines.
		/// You must provide points in X sort order.
		/// </summary>
		/// <param name="x">Input. X coordinates to fit.</param>
		/// <param name="y">Input. Y coordinates to fit.</param>
		/// <param name="startSlope">Optional slope constraint for the first point. Single.NaN means no constraint.</param>
		/// <param name="endSlope">Optional slope constraint for the final point. Single.NaN means no constraint.</param>		
		public void Fit(double[] x, double[] y, double startSlope = double.NaN, double endSlope = double.NaN)
		{
			if (double.IsInfinity(startSlope) || double.IsInfinity(endSlope))
			{
				throw new Exception("startSlope and endSlope cannot be infinity.");
			}

			// Save x and y for eval
			this.xOrig = x;
			this.yOrig = y;

			int n = x.Length;
			double[] r = new double[n]; // the right hand side numbers: wikipedia page overloads b

			TriDiagonalMatrixF m = new TriDiagonalMatrixF(n);
			double dx1, dx2, dy1, dy2;

			// First row is different (equation 16 from the article)
			if (double.IsNaN(startSlope))
			{
				dx1 = x[1] - x[0];
				m.C[0] = 1.0f / dx1;
				m.B[0] = 2.0f * m.C[0];
				r[0] = 3 * (y[1] - y[0]) / (dx1 * dx1);
			}
			else
			{
				m.B[0] = 1;
				r[0] = startSlope;
			}

			// Body rows (equation 15 from the article)
			for (int i = 1; i < n - 1; i++)
			{
				dx1 = x[i] - x[i - 1];
				dx2 = x[i + 1] - x[i];

				m.A[i] = 1.0 / dx1;
				m.C[i] = 1.0 / dx2;
				m.B[i] = 2.0 * (m.A[i] + m.C[i]);

				dy1 = y[i] - y[i - 1];
				dy2 = y[i + 1] - y[i];
				r[i] = 3 * (dy1 / (dx1 * dx1) + dy2 / (dx2 * dx2));
			}

			// Last row also different (equation 17 from the article)
			if (double.IsNaN(endSlope))
			{
				dx1 = x[n - 1] - x[n - 2];
				dy1 = y[n - 1] - y[n - 2];
				m.A[n - 1] = 1.0f / dx1;
				m.B[n - 1] = 2.0f * m.A[n - 1];
				r[n - 1] = 3 * (dy1 / (dx1 * dx1));
			}
			else
			{
				m.B[n - 1] = 1;
				r[n - 1] = endSlope;
			}

			// k is the solution to the matrix
			double[] k = m.Solve(r);
			// a and b are each spline's coefficients
			this.a = new double[n - 1];
			this.b = new double[n - 1];

			for (int i = 1; i < n; i++)
			{
				dx1 = x[i] - x[i - 1];
				dy1 = y[i] - y[i - 1];
				a[i - 1] = k[i - 1] * dx1 - dy1; // equation 10 from the article
				b[i - 1] = -k[i] * dx1 + dy1; // equation 11 from the article
			}			
		}

		#endregion

		#region Eval*

		/// <summary>
		/// Evaluate the spline at the specified x coordinates.
		/// This can extrapolate off the ends of the splines.
		/// You must provide X's in ascending order.
		/// The spline must already be computed before calling this, meaning you must have already called Fit() or FitAndEval().
		/// </summary>
		/// <param name="x">Input. X coordinates to evaluate the fitted curve at.</param>
		/// <param name="debug">Turn on console output. Default is false.</param>
		/// <returns>The computed y values for each x.</returns>
		public double[] Eval(double[] x, bool debug = false)
		{
			CheckAlreadyFitted();

			int n = x.Length;
			double[] y = new double[n];
			_lastIndex = 0; // Reset simultaneous traversal in case there are multiple calls

			for (int i = 0; i < n; i++)
			{
				// Find which spline can be used to compute this x (by simultaneous traverse)
				int j = GetNextXIndex(x[i]);

				// Evaluate using j'th spline
				y[i] = EvalSpline(x[i], j, debug);
			}

			return y;
		}

		/// <summary>
		/// Evaluate (compute) the slope of the spline at the specified x coordinates.
		/// This can extrapolate off the ends of the splines.
		/// You must provide X's in ascending order.
		/// The spline must already be computed before calling this, meaning you must have already called Fit() or FitAndEval().
		/// </summary>
		/// <param name="x">Input. X coordinates to evaluate the fitted curve at.</param>
		/// <param name="debug">Turn on console output. Default is false.</param>
		/// <returns>The computed y values for each x.</returns>
		public double[] EvalSlope(double[] x, bool debug = false)
		{
			CheckAlreadyFitted();

			int n = x.Length;
			double[] qPrime = new double[n];
			_lastIndex = 0; // Reset simultaneous traversal in case there are multiple calls

			for (int i = 0; i < n; i++)
			{
				// Find which spline can be used to compute this x (by simultaneous traverse)
				int j = GetNextXIndex(x[i]);

				// Evaluate using j'th spline
				double dx = xOrig[j + 1] - xOrig[j];
				double dy = yOrig[j + 1] - yOrig[j];
				double t = (x[i] - xOrig[j]) / dx;

				// From equation 5 we could also compute q' (qp) which is the slope at this x
				qPrime[i] = dy / dx
					+ (1 - 2 * t) * (a[j] * (1 - t) + b[j] * t) / dx
					+ t * (1 - t) * (b[j] - a[j]) / dx;

				if (debug) Console.WriteLine("[{0}]: xs = {1}, j = {2}, t = {3}", i, x[i], j, t);
			}

			return qPrime;
		}

		#endregion

		#region Static Methods

		/// <summary>
		/// Static all-in-one method to fit the splines and evaluate at X coordinates.
		/// </summary>
		/// <param name="x">Input. X coordinates to fit.</param>
		/// <param name="y">Input. Y coordinates to fit.</param>
		/// <param name="xs">Input. X coordinates to evaluate the fitted curve at.</param>
		/// <param name="startSlope">Optional slope constraint for the first point. Single.NaN means no constraint.</param>
		/// <param name="endSlope">Optional slope constraint for the final point. Single.NaN means no constraint.</param>
		/// <param name="debug">Turn on console output. Default is false.</param>
		/// <returns>The computed y values for each xs.</returns>
		public static double[] Compute(double[] x, double[] y, double[] xs, double startSlope = double.NaN, double endSlope = double.NaN)
		{
			CubicSpline spline = new CubicSpline();
			return spline.FitAndEval(x, y, xs, startSlope, endSlope);
		}

        /// <summary>
        /// Fit the input x,y points using the parametric approach, so that y does not have to be an explicit
        /// function of x, meaning there does not need to be a single value of y for each x.
        /// </summary>
        /// <param name="x">Input x coordinates.</param>
        /// <param name="y">Input y coordinates.</param>
        /// <param name="nOutputPoints">How many output points to create.</param>
        /// <param name="xs">Output (interpolated) x values.</param>
        /// <param name="ys">Output (interpolated) y values.</param>
        /// <param name="firstDx">Optionally specifies the first point's slope in combination with firstDy. Together they
        /// are a vector describing the direction of the parametric spline of the starting point. The vector does
        /// not need to be normalized. If either is NaN then neither is used.</param>
        /// <param name="firstDy">See description of dx0.</param>
        /// <param name="lastDx">Optionally specifies the last point's slope in combination with lastDy. Together they
        /// are a vector describing the direction of the parametric spline of the last point. The vector does
        /// not need to be normalized. If either is NaN then neither is used.</param>
        /// <param name="lastDy">See description of dxN.</param>
        public static void FitParametric(double[] x, double[] y, int nOutputPoints, out double[] xs, out double[] ys,
			double firstDx = double.NaN, double firstDy = double.NaN, double lastDx = double.NaN, double lastDy = double.NaN)
		{
			// Compute distances
			int n = x.Length;
			double[] dists = new double[n]; // cumulative distance
			dists[0] = 0;
			double totalDist = 0;

			for (int i = 1; i < n; i++)
			{
				double dx = x[i] - x[i - 1];
				double dy = y[i] - y[i - 1];
				double dist = Math.Sqrt(dx * dx + dy * dy);
				totalDist += dist;
				dists[i] = totalDist;
			}

			// Create 'times' to interpolate to
			double dt = totalDist / (nOutputPoints - 1);
			double[] times = new double[nOutputPoints];
			times[0] = 0;

			for (int i = 1; i < nOutputPoints; i++)
			{
				times[i] = times[i - 1] + dt;
			}

            // Normalize the slopes, if specified
            NormalizeVector(ref firstDx, ref firstDy);
            NormalizeVector(ref lastDx, ref lastDy);

			// Spline fit both x and y to times
			CubicSpline xSpline = new CubicSpline();
			xs = xSpline.FitAndEval(dists, x, times, firstDx / dt, lastDx / dt);

			CubicSpline ySpline = new CubicSpline();
			ys = ySpline.FitAndEval(dists, y, times, firstDy / dt, lastDy / dt);
		}

        private static void NormalizeVector(ref double dx, ref double dy)
        {
            if (!double.IsNaN(dx) && !double.IsNaN(dy))
            {
				double d = Math.Sqrt(dx * dx + dy * dy);

                if (d > double.Epsilon) // probably not conservative enough, but catches the (0,0) case at least
                {
                    dx = dx / d;
                    dy = dy / d;
                }
                else
                {
                    throw new ArgumentException("The input vector is too small to be normalized.");
                }
            }
            else
            {
                // In case one is NaN and not the other
                dx = dy = Single.NaN;
            }
        }

        #endregion
    }
}
