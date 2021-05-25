﻿using System;

namespace GShark.Core
{
    /// <summary>
    /// A simple parametric data representing an "interval" between two numbers.
    /// </summary>
    public class Interval
    {
        /// <summary>
        /// Creates an instance of an interval by the values.
        /// </summary>
        /// <param name="min">The minimum value of the interval.</param>
        /// <param name="max">The maximum value of the interval.</param>
        public Interval(double min, double max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// The minimum value of the interval.
        /// </summary>
        public double Min { get; }

        /// <summary>
        /// The maximum value of the interval.
        /// </summary>
        public double Max { get; }

        /// <summary>
        /// Gets the average value.
        /// </summary>
        public double Mid => Math.Abs(Min - Max) > GeoSharpMath.MAXTOLERANCE ? 0.5 * (Min + Max) : Min;

        /// <summary>
        /// Gets the length of the interval range.<br/>
        /// If the interval is decreasing, negative number will be returned.
        /// </summary>
        public double Length => Max - Min;

        /// <summary>
        /// Converts normalized parameter to interval value, or pair of values.
        /// </summary>
        /// <param name="normalizedParameter">The normalized parameter between 0-1.</param>
        /// <returns>Interval parameter min*(1.0-normalizedParameter) + max*normalizedParameter.</returns>
        public double ParameterAt(double normalizedParameter)
        {
            return !GeoSharpMath.IsValidDouble(normalizedParameter)
                ? -1.23432101234321E+308
                : (1.0 - normalizedParameter) * Min + normalizedParameter * Max;
        }
    }
}
