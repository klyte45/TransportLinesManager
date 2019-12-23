using System;

namespace Klyte.Commons.Utils
{
    public class Range<T> where T : IComparable<T>
    {
        /// <summary>
        /// Minimum value of the range
        /// </summary>
        public T Minimum { get; set; }

        /// <summary>
        /// Maximum value of the range
        /// </summary>
        public T Maximum { get; set; }

        public Range(T min, T max)
        {
            if (min.CompareTo(max) >= 0)
            {
                var temp = min;
                min = max;
                max = temp;
            }
            Minimum = min;
            Maximum = max;
        }

        /// <summary>
        /// Presents the Range in readable format
        /// </summary>
        /// <returns>String representation of the Range</returns>
        public override string ToString() => $"[{Minimum} - {Maximum}]";

        /// <summary>
        /// Determines if the range is valid
        /// </summary>
        /// <returns>True if range is valid, else false</returns>
        public Boolean IsValid() => Minimum.CompareTo(Maximum) <= 0;

        /// <summary>
        /// Determines if the provided value is inside the range
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <returns>True if the value is inside Range, else false</returns>
        public Boolean ContainsValue(T value) => (Minimum.CompareTo(value) <= 0) && (value.CompareTo(Maximum) <= 0);


        /// <summary>
        /// Determines if the provided value is inside the range
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <returns>True if the value is inside Range, else false</returns>
        public Boolean IsBetweenLimits(T value) => (Minimum.CompareTo(value) < 0) && (value.CompareTo(Maximum) < 0);

        /// <summary>
        /// Determines if this Range is inside the bounds of another range
        /// </summary>
        /// <param name="Range">The parent range to test on</param>
        /// <returns>True if range is inclusive, else false</returns>
        public Boolean IsInsideRange(Range<T> Range) => this.IsValid() && Range.IsValid() && Range.ContainsValue(this.Minimum) && Range.ContainsValue(this.Maximum);



        /// <summary>
        /// Determines if another range is inside the bounds of this range
        /// </summary>
        /// <param name="Range">The child range to test</param>
        /// <returns>True if range is inside, else false</returns>
        public Boolean ContainsRange(Range<T> Range) => this.IsValid() && Range.IsValid() && this.ContainsValue(Range.Minimum) && this.ContainsValue(Range.Maximum);

        /// <summary>
        /// Determines if another range intersect this range
        /// </summary>
        /// <param name="Range">The child range to test</param>
        /// <returns>True if range is inside, else false</returns>
        public Boolean IntersectRange(Range<T> Range) => this.IsValid() && Range.IsValid() && (this.ContainsValue(Range.Minimum) || this.ContainsValue(Range.Maximum) || Range.ContainsValue(this.Maximum) || Range.ContainsValue(this.Maximum));

        public Boolean IsBorderSequence(Range<T> Range) => this.IsValid() && Range.IsValid() && (this.Maximum.Equals(Range.Minimum) || this.Minimum.Equals(Range.Maximum));
    }
}
