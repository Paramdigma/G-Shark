﻿using System.Collections.Generic;
using FluentAssertions;
using GeometrySharp.Core;
using GeometrySharp.Evaluation;
using GeometrySharp.Geometry;
using Xunit;
using Xunit.Abstractions;
using verb;

namespace GeometrySharp.Test.XUnit.Evaluation
{
    public class AnalyzeTests
    {
        private readonly ITestOutputHelper _testOutput;

        public AnalyzeTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        [Fact]
        public void RationalBezierCurveArcLength_Returns_The_Approximated_Length()
        {
            var degree = 3;
            var knots1 = new Knot() { 0, 0, 0, 0, 1, 1, 1, 1 };
            var knots2 = new Knot() { 1, 1, 1, 1, 4, 4, 4, 4 };
            var controlPts = new List<Vector3>()
            {
                new Vector3() {0, 0, 0},
                new Vector3() {0.5, 0, 0},
                new Vector3() {2.5, 0, 0},
                new Vector3() {3, 0, 0}
            };

            var curve1 = new NurbsCurve(degree, knots1, controlPts);
            var curve2 = new NurbsCurve(degree, knots2, controlPts);

            var curveLength1 = Analyze.RationalBezierCurveArcLength(curve1, 1);
            var curveLength2 = Analyze.RationalBezierCurveArcLength(curve2, 4);

            curveLength1.Should().BeApproximately(3.0, GeoSharpMath.TOLERANCE);
            curveLength2.Should().BeApproximately(3.0, GeoSharpMath.TOLERANCE);
        }

        [Fact]
        public void t()
        {
            var pts = new Array<object>();

            pts.push(new Array<double>(new double[] { 0, 0, 0, 1 }));
            pts.push(new Array<double>(new double[] { 0.5, 0, 0, 1 }));
            pts.push(new Array<double>(new double[] { 2.5, 0, 0, 1 }));
            pts.push(new Array<double>(new double[] { 3, 0, 0, 1 }));

            var knots = new Array<double>(new double[] { 0.0, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0 });
            var weights = new Array<double>(new double[] { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 });

            var curve = verb.geom.NurbsCurve.byPoints(pts, 3);

            var res = verb.eval.Analyze.rationalBezierCurveArcLength(curve._data,1, 16);

            _testOutput.WriteLine($"{res}");
        }
    }
}