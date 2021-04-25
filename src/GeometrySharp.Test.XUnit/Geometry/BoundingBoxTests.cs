using FluentAssertions;
using GeometrySharp.Geometry;
using GeometrySharp.Test.XUnit.Data;
using System.Collections.Generic;
using GeometrySharp.Core;
using Xunit;
using Xunit.Abstractions;

namespace GeometrySharp.Test.XUnit.Geometry
{
    public class BoundingBoxTests
    {
        private readonly ITestOutputHelper _testOutput;

        public BoundingBoxTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        [Theory]
        [MemberData(nameof(BoundingBoxCollection.BoundingBoxCollections), MemberType = typeof(BoundingBoxCollection))]
        public void It_Creates_A_BoundingBox_From_Points(Vector3[] pts, Vector3 min, Vector3 max)
        {
            // Act
            var bBox = new BoundingBox(pts);

            // Assert
            _testOutput.WriteLine(bBox.ToString());
            bBox.Should().NotBeNull();
            bBox.Min.Should().BeEquivalentTo(min);
            bBox.Max.Should().BeEquivalentTo(max);
        }

        [Fact]
        public void It_Creates_An_Aligned_BoundingBox()
        {
            // Arrange
            Plane orientedPlane = Plane.PlaneXY.Rotate(GeoSharpMath.ToRadians(30));
            Vector3 expectedMin = new Vector3 { 45.662928, 59.230957, -4.22451 };
            Vector3 expectedMax = new Vector3 { 77.622297, 78.520011, 3.812853 };

            // Act
            var bBox = new BoundingBox(BoundingBoxCollection.BoundingBox3D(), orientedPlane);

            // Assert
            _testOutput.WriteLine(bBox.ToString());
            bBox.Should().NotBeNull();
            bBox.Min.DistanceTo(expectedMin).Should().BeLessThan(GeoSharpMath.MAXTOLERANCE);
            bBox.Max.DistanceTo(expectedMax).Should().BeLessThan(GeoSharpMath.MAXTOLERANCE);
        }

        [Fact]
        public void It_Returns_True_If_A_Point_Is_Contained_Into_TheBoundingBox()
        {
            // Arrange
            var conteinedPt = new Vector3 {2.5, 4.5, 0.0};
            var bBox = new BoundingBox(BoundingBoxCollection.BoundingBox2D());

            // Act
            var containsResult = bBox.Contains(conteinedPt, false);

            // Assert
            containsResult.Should().BeTrue();
        }

        [Fact]
        public void It_Returns_False_IfAPoint_Is_Outside_TheBoundingBox()
        {
            // Arrange
            var externalPt = new Vector3 {12.4, 5.0, 0.0};
            var bBox = new BoundingBox(BoundingBoxCollection.BoundingBox2D());

            // Act
            var containsResult = bBox.Contains(externalPt, false);

            // Assert
            containsResult.Should().BeFalse();
        }

        [Theory]
        [MemberData(nameof(BoundingBoxCollection.BoundingBoxIntersections), MemberType = typeof(BoundingBoxCollection))]
        public void It_Returns_True_If_TwoBoundingBoxes_Intersect(Vector3[] ptsBBox1, Vector3[] ptsBBox2, bool result)
        {
            // Arrange
            var bBox1 = new BoundingBox(ptsBBox1);
            var bBox2 = new BoundingBox(ptsBBox2);

            // Act
            var intersectionResult = BoundingBox.AreOverlapping(bBox1, bBox2, 0.0);

            // Assert
            intersectionResult.Should().Be(result);
        }

        [Fact]
        public void It_Returns_BooleanUnion_Between_Two_BoundingBoxes()
        {
            // Arrange
            var pt1 = new Vector3 {5d, 5d, 0d};
            var pt2 = new Vector3 {-15d, -13d, -5d};
            var pts = new List<Vector3> {pt1, pt2};
            var pMax = new Vector3 {10, 10, 0};
            var bBox1 = new BoundingBox(BoundingBoxCollection.BoundingBox2D());
            var bBox2 = new BoundingBox(pts);

            // Act
            var bBoxResult = bBox1.Union(bBox2);

            // Assert
            bBoxResult.IsValid.Should().BeTrue();
            bBoxResult.Max.Should().BeEquivalentTo(pMax);
            bBoxResult.Min.Should().BeEquivalentTo(pt2);
        }

        [Theory]
        [MemberData(nameof(BoundingBoxCollection.BoundingBoxIntersectionsUnset),
            MemberType = typeof(BoundingBoxCollection))]
        public void Intersect_Returns_UnsetBBox_If_OneOfTheTwoBBoxes_IsNotInitialized_OrNotIntersection(
            Vector3[] bBoxPts, BoundingBox bBox2)
        {
            // Arrange
            var bBox1 = new BoundingBox(bBoxPts);

            // Act
            var bBoxIntersect = bBox1.Intersect(bBox2);

            // Assert
            bBoxIntersect.IsValid.Should().BeFalse();
            bBoxIntersect.Max.Should().BeEquivalentTo(Vector3.Unset);
            bBoxIntersect.Min.Should().BeEquivalentTo(Vector3.Unset);
        }

        [Fact]
        public void Intersect_Returns_BBox_As_Intersection_Of_Two_BBoxes()
        {
            // Arrange
            var pt1 = new Vector3 {5d, 5d, 0d};
            var pt2 = new Vector3 {15d, 15d, 0d};
            var pts2 = new List<Vector3> {pt1, pt2};
            var bBox1 = new BoundingBox(BoundingBoxCollection.BoundingBox2D());
            var bBox2 = new BoundingBox(pts2);

            // Act
            var bBoxResult = bBox1.Intersect(bBox2);

            // Assert
            bBoxResult.IsValid.Should().BeTrue();
            bBoxResult.Max.Should().BeEquivalentTo(bBox1.Max);
            bBoxResult.Min.Should().BeEquivalentTo(bBox2.Min);
        }

        [Theory]
        [MemberData(nameof(BoundingBoxCollection.BoundingBoxAxisLength), MemberType = typeof(BoundingBoxCollection))]
        public void It_Returns_ACollection_Of_GetAxisLength(Vector3[] pts, int index, double length)
        {
            // Arrange
            var bBox = new BoundingBox(pts);

            // Act
            var lengthResult = bBox.GetAxisLength(index);

            // Assert
            lengthResult.Should().Be(length);
        }

        [Fact]
        public void It_Returns_TheLongestAxis()
        {
            // Arrange
            var bBox = new BoundingBox(BoundingBoxCollection.BoundingBoxWithZValue());

            // Act
            var longestAxis = bBox.GetLongestAxis();

            // Assert
            longestAxis.Should().Be(1);
        }

        [Fact]
        public void Union_Returns_ValidBoundingBox_If_Other_IsNotValid()
        {
            // Arrange
            var bBox1 = BoundingBox.Unset;
            var bBox2 = new BoundingBox(BoundingBoxCollection.BoundingBox2D());

            // Act
            var bBoxResult = BoundingBox.Union(bBox1, bBox2);

            // Assert
            bBoxResult.Should().Be(bBox2);
        }

        [Fact]
        public void It_Returns_A_BoundingBox_In_Ascending_Way()
        {
            // Arrange
            var bBox = new BoundingBox(new Vector3{15,15,0}, new Vector3{5,0,0});

            // Act
            var bBoxMadeValid = bBox.MakeItValid();

            // Assert
            bBoxMadeValid.Min.Should().BeEquivalentTo(bBox.Max);
            bBoxMadeValid.Max.Should().BeEquivalentTo(bBox.Min);
        }
    }
}
