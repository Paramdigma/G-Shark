﻿using GeometrySharp.Core;
using GeometrySharp.Core.BoundingBoxTree;
using GeometrySharp.Geometry;
using GeometrySharp.Optimization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace GeometrySharp.Operation
{
    /// <summary>
    /// Intersection provides various tools for all kinds of intersection
    /// </summary>
    public class Intersect
    {
        // ToDo: Curve-Plane
        // ToDo: Curve-Curve
        // ToDo: Curve-Line
        // ToDo: Curve-Self
        // ToDo: Curve-Surface
        // ToDo: Line-Bbox
        // ToDo: Surface-Surface

        /// <summary>
        /// Solves the intersection between two planes.
        /// This method returns true if intersection is found, false if the
        /// planes are parallel.
        /// </summary>
        /// <param name="p1">The first plane.</param>
        /// <param name="p2">The second plane.</param>
        /// <param name="line">The intersection as <see cref="Line"/>.</param>
        /// <returns>True if the intersection success.</returns>
        public static bool PlanePlane(Plane p1, Plane p2, out Line line)
        {
            Vector3 plNormal1 = p1.Normal;
            Vector3 plNormal2 = p2.Normal;
            line = new Line(new Vector3{0,0,0}, new Vector3{0,0,1});

            Vector3 directionVec = Vector3.Cross(plNormal1, plNormal2);
            if (Vector3.Dot(directionVec, directionVec) < GeoSharpMath.EPSILON)
            {
                return false;
            }

            // Find the largest index of directionVec of the line of intersection.
            int largeIndex = 0;
            double ai = Math.Abs(directionVec[0]);
            double ay = Math.Abs(directionVec[1]);
            double az = Math.Abs(directionVec[2]);

            if (ay > ai)
            {
                largeIndex = 1;
                ai = ay;
            }

            if (az > ai)
            {
                largeIndex = 2;
            }

            double a1, b1, a2, b2;

            if (largeIndex == 0)
            {
                a1 = plNormal1[1];
                b1 = plNormal1[2];
                a2 = plNormal2[1];
                b2 = plNormal2[2];
            }
            else if (largeIndex == 1)
            {
                a1 = plNormal1[0];
                b1 = plNormal1[2];
                a2 = plNormal2[0];
                b2 = plNormal2[2];
            }
            else
            {
                a1 = plNormal1[0];
                b1 = plNormal1[1];
                a2 = plNormal2[0];
                b2 = plNormal2[1];
            }

            double dot1 = -Vector3.Dot(p1.Origin, plNormal1);
            double dot2 = -Vector3.Dot(p2.Origin, plNormal2);

            double denominator = a1 * b2 - a2 * b1;

            double corX = (b1 * dot2 - b2 * dot1) / denominator;
            double corY = (a2 * dot1 - a1 * dot2) / denominator;

            Vector3 pt = new Vector3();

            if (largeIndex == 0)
            {
                pt.AddRange(new[] { 0.0, corX, corY });
            }
            else if (largeIndex == 1)
            {
                pt.AddRange(new[] { corX, 0.0, corY });
            }
            else
            {
                pt.AddRange(new[] { corX, corY, 0.0 });
            }

            line = new Line(pt, pt + directionVec.Unitize());
            return true;
        }

        /// <summary>
        /// Finds the unique point intersection of a line and a plane.
        /// This method returns true if intersection return the unique point, it returns
        /// false if the segment is parallel to the plane or lies in plane.
        /// http://geomalgorithms.com/a05-_intersect-1.html
        /// </summary>
        /// <param name="line">The segment to intersect. Assumed as infinite.</param>
        /// <param name="plane">The plane has to be intersected.</param>
        /// <param name="pt">The point representing the unique intersection.</param>
        /// <param name="t">The parameter on the line between 0.0 to 1.0</param>
        /// <returns>True if the intersection success.</returns>
        public static bool LinePlane(Line line, Plane plane, out Vector3 pt, out double t)
        {
            Vector3 lnDir = line.Direction;
            Vector3 ptPlane = plane.Origin - line.Start;
            double segmentLength = line.Length;

            double denominator = Vector3.Dot(plane.Normal, lnDir);
            double numerator = Vector3.Dot(plane.Normal, ptPlane);

            if (Math.Abs(denominator) < GeoSharpMath.EPSILON)
            {
                pt = Vector3.Unset;
                t = 0.0;
                return false;
            }

            // Compute the intersect parameter.
            double s = numerator / denominator;
            pt = line.Start + lnDir * s;
            // Parametrize the t value between 0.0 to 1.0.
            t = s / segmentLength;
            return true;
        }

        /// <summary>
        /// Solves the intersection between two lines, assumed as infinite.
        /// Returns as outputs two points describing the minimum distance between the two lines.
        /// Returns false if the segments are parallel.
        /// http://geomalgorithms.com/a07-_distance.html
        /// </summary>
        /// <param name="ln0">The first line.</param>
        /// <param name="ln1">The second line.</param>
        /// <param name="pt0">The output point of the first line.</param>
        /// <param name="pt1">The output point of the second line.</param>
        /// <param name="t0">The parameter on the first line between 0.0 to 1.0</param>
        /// <param name="t1">The parameter on the second line between 0.0 to 1.0</param>
        /// <returns>True if the intersection succeed.</returns>
        public static bool LineLine(Line ln0, Line ln1, out Vector3 pt0, out Vector3 pt1, out double t0, out double t1)
        {
            double ln0Length = ln0.Length;
            double ln1Length = ln1.Length;
            Vector3 lnDir0 = ln0.Direction;
            Vector3 lnDir1 = ln1.Direction;
            Vector3 ln0Ln1Dir = ln0.Start - ln1.Start;

            double a = Vector3.Dot(lnDir0, lnDir0);
            double b = Vector3.Dot(lnDir0, lnDir1);
            double c = Vector3.Dot(lnDir1, lnDir1);
            double d = Vector3.Dot(lnDir0, ln0Ln1Dir);
            double e = Vector3.Dot(lnDir1, ln0Ln1Dir);
            double div = a * c - b * b;

            if (Math.Abs(div) < GeoSharpMath.EPSILON)
            {
                pt0 = Vector3.Unset;
                pt1 = Vector3.Unset;
                t0 = 0.0;
                t1 = 0.0;
                return false;
            }

            double s = (b * e - c * d) / div;
            double t = (a * e - b * d) / div;

            pt0 = ln0.Start + lnDir0 * s;
            pt1 = ln1.Start + lnDir1 * t;
            t0 = s / ln0Length;
            t1 = t / ln1Length;
            return true;
        }

        /// <summary>
        /// Computes the intersection between a polyline and a plane.
        /// Under the hood, is intersecting each segment with the plane and storing the intersection point into a collection.
        /// If no intersections are found a empty collection is returned.
        /// </summary>
        /// <param name="poly">The polyline to intersect with.</param>
        /// <param name="pl">The section plane.</param>
        /// <returns>A collection of the unique intersection points.</returns>
        public static List<Vector3> PolylinePlane(Polyline poly, Plane pl)
        {
            List<Vector3> intersectionPts = new List<Vector3>();
            Line[] segments = poly.Segments();

            foreach (Line segment in segments)
            {
                if (!LinePlane(segment, pl, out Vector3 pt, out double t))
                {
                    continue;
                }

                if (t >= 0.0 && t <= 1.0)
                {
                    intersectionPts.Add(pt);
                }
            }

            return intersectionPts;
        }

        /// <summary>
        /// Computes the intersection between a circle and a line.
        /// If the intersection is computed the result points can be 1 or 2 depending on whether the line touches the circle tangentially or cuts through it.
        /// The intersection result false if the line misses the circle entirely.
        /// http://csharphelper.com/blog/2014/09/determine-where-a-line-intersects-a-circle-in-c/
        /// </summary>
        /// <param name="cl">The circle for intersection.</param>
        /// <param name="ln">The line for intersection.</param>
        /// <param name="pts">Output the intersection points.</param>
        /// <returns>True if intersection is computed.</returns>
        public static bool LineCircle(Circle cl, Line ln, out Vector3[] pts)
        {
            Vector3 pt0 = ln.Start;
            Vector3 ptCircle = cl.Center;
            Vector3 lnDir = ln.Direction;
            Vector3 pt0PtCir = pt0 - ptCircle;

            double a = Vector3.Dot(lnDir, lnDir);
            double b = Vector3.Dot(lnDir, pt0PtCir) * 2;
            double c = Vector3.Dot(pt0PtCir, pt0PtCir) - (cl.Radius * cl.Radius);

            double det = b * b - 4 * a * c;
            double t;

            if ((a <= GeoSharpMath.MAXTOLERANCE) || (det < 0))
            {
                pts = new Vector3[]{};
                return false;
            }
            else if (Math.Abs(det) < GeoSharpMath.MAXTOLERANCE)
            {
                t = -b / (2 * a);
                Vector3 intersection = pt0 + lnDir * t;
                pts = new Vector3[] { intersection};
                return true;
            }
            else
            {
                t = (-b + Math.Sqrt(det)) / (2 * a);
                double t1 = (-b - Math.Sqrt(det)) / (2 * a);
                Vector3 intersection0 = pt0 + lnDir * t;
                Vector3 intersection1 = pt0 + lnDir * t1;
                pts = new Vector3[] { intersection0, intersection1 };
                return true;
            }
        }

        /// <summary>
        /// Computes the intersection between a plane and a circle.
        /// If the intersection is computed the result points can be 1 or 2 depending on whether the plane touches the circle tangentially or cuts through it.
        /// The intersection result false if the plane is parallel to the circle or misses the circle entirely.
        /// </summary>
        /// <param name="pl">The plane for intersection.</param>
        /// <param name="cl">The circle for intersection.</param>
        /// <param name="pts">Output the intersection points.</param>
        /// <returns>True if intersection is computed.</returns>
        public static bool PlaneCircle(Plane pl, Circle cl, out Vector3[] pts)
        {
            pts = new Vector3[] { };
            Vector3 clPt = cl.Center;

            Vector3 cCross = Vector3.Cross(pl.Origin, clPt);
            if (Math.Abs(cCross.Length()) < GeoSharpMath.EPSILON)
            {
                return false;
            }

            bool intersection = PlanePlane(pl, cl.Plane, out Line intersectionLine);
            Vector3 closestPt = intersectionLine.ClosestPoint(clPt);
            double distance = clPt.DistanceTo(intersectionLine);

            if (Math.Abs(distance) < GeoSharpMath.EPSILON)
            {
                Vector3 pt = cl.ClosestPt(closestPt);
                pts = new[] {pt};
                return true;
            }

            return LineCircle(cl, intersectionLine, out pts);
        }

        public static (Vector3 pt0, Vector3 pt1, double t0, double t1) CurveCurve(NurbsCurve crv1, NurbsCurve crv2, double tolerance)
        {
            var bBoxTreeIntersections = BoundingBoxTree(new LazyCurveBBT(crv1), new LazyCurveBBT(crv2), 0);
            (Vector3 pt0, Vector3 pt1, double t0, double t1) seed = (Vector3.Unset, Vector3.Unset, 0.0, 0.0);

            //var t = CurvesWithEstimation(crv1, crv2, bBoxTreeIntersections[0].Item1.Knots[0],
            //    bBoxTreeIntersections[0].Item2.Knots[0], tolerance);

            var t = bBoxTreeIntersections
                .Select(x => CurvesWithEstimation(crv1, crv2, x.Item1.Knots[0], x.Item2.Knots[0], tolerance))
                .Where(tuple => (tuple.pt0 - tuple.pt1).SquaredLength() < tolerance)
                .Aggregate(seed, (a, b) =>
                {
                    if (!(Math.Abs(a.t0 - b.t0) < tolerance)) return a = b;
                    return a;
                });

            return t;
        }

        /// <summary>
        /// The core algorithm for bounding box tree intersection, supporting both lazy and pre-computed bounding box trees
        /// via the <see cref="IBoundingBoxTree{T}"/> interface.
        /// </summary>
        /// <param name="bbt1">The first Bounding box tree object.</param>
        /// <param name="bbt2">The second Bounding box tree object.</param>
        /// <param name="tolerance">Tolerance as per default set as 1e-9.</param>
        /// <returns>A collection of tuples extracted from the Yield method of the BoundingBoxTree.</returns>
        private static List<Tuple<T1, T2>> BoundingBoxTree<T1, T2>(IBoundingBoxTree<T1> bbt1, IBoundingBoxTree<T2> bbt2,
            double tolerance = 1e-9)
        {
            List<IBoundingBoxTree<T1>> aTrees = new List<IBoundingBoxTree<T1>>();
            List<IBoundingBoxTree<T2>> bTrees = new List<IBoundingBoxTree<T2>>();

            aTrees.Add(bbt1);
            bTrees.Add(bbt2);

            List<Tuple<T1, T2>> result = new List<Tuple<T1, T2>>();

            while (aTrees.Count > 0)
            {
                IBoundingBoxTree<T1> a = aTrees[^1];
                aTrees.RemoveAt(aTrees.Count - 1);
                IBoundingBoxTree<T2> b = bTrees[^1];
                bTrees.RemoveAt(bTrees.Count - 1);

                if (a.IsEmpty() || b.IsEmpty())
                {
                    continue;
                }

                if (BoundingBox.AreOverlapping(a.BoundingBox(), b.BoundingBox(), tolerance) == false)
                {
                    continue;
                }

                bool aIndivisible = a.IsIndivisible(tolerance);
                bool bIndivisible = b.IsIndivisible(tolerance);
                Tuple<IBoundingBoxTree<T2>, IBoundingBoxTree<T2>> bSplit = b.Split();
                Tuple<IBoundingBoxTree<T1>, IBoundingBoxTree<T1>> aSplit = a.Split();

                if (aIndivisible && bIndivisible)
                {
                    result.Add(new Tuple<T1, T2>(a.Yield(), b.Yield()));
                    continue;
                }
                if (aIndivisible)
                {
                    aTrees.Add(a);
                    bTrees.Add(bSplit.Item2);
                    aTrees.Add(a);
                    bTrees.Add(bSplit.Item1);
                    continue;
                }
                if (bIndivisible)
                {
                    aTrees.Add(aSplit.Item2);
                    bTrees.Add(b);
                    aTrees.Add(aSplit.Item1);
                    bTrees.Add(b);
                    continue;
                }

                aTrees.Add(aSplit.Item2);
                bTrees.Add(bSplit.Item2);

                aTrees.Add(aSplit.Item2);
                bTrees.Add(bSplit.Item1);

                aTrees.Add(aSplit.Item1);
                bTrees.Add(bSplit.Item2);

                aTrees.Add(aSplit.Item1);
                bTrees.Add(bSplit.Item1);
            }

            return result;
        }

        /// <summary>
        /// Refine an intersection pair for two curves given an initial guess. This is an unconstrained minimization,
        /// so the caller is responsible for providing a very good initial guess.
        /// </summary>
        /// <param name="crv0">The first curve.</param>
        /// <param name="crv1">The second curve.</param>
        /// <param name="firstGuess">The first guess parameter.</param>
        /// <param name="secondGuess">The second guess parameter.</param>
        /// <param name="tolerance">The value tolerance for the intersection.</param>
        /// <returns>The curves intersection, expressed as a tuple of two intersection points and the t parameters.</returns>
        private static (Vector3 pt0, Vector3 pt1, double t0, double t1) CurvesWithEstimation(NurbsCurve crv0, NurbsCurve crv1,
            double firstGuess, double secondGuess, double tolerance)
        {
            ObjectiveFunction functions = new ObjectiveFunction(crv0, crv1);
            Minimizer min = new Minimizer(functions.Value, functions.Gradient);
            MinimizationResult solution = min.UnconstrainedMinimizer(new Vector3 {firstGuess, secondGuess}, tolerance * tolerance);

            Vector3 pt1 = Evaluation.CurvePointAt(crv0, solution.SolutionPoint[0]);
            Vector3 pt2 = Evaluation.CurvePointAt(crv1, solution.SolutionPoint[1]);

            return (pt1, pt2, solution.SolutionPoint[0], solution.SolutionPoint[1]);
        }
    }
}