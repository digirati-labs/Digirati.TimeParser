using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Digirati.TimeParser
{
    public static class TimeParser
    {
        private static readonly Dictionary<char, int> TimeSegmentLabels = new Dictionary<char, int>
        {
            {'d', 24 * 60 * 60},
            {'h', 60 * 60},
            {'m', 60},
            {'s', 1}
        };

        /// <summary>
        ///     Converts string like '3h 15m 25s' into a timespan of 3 hours, 15 minutes and 25 seconds.
        ///     Supported letter values: {d, h, m, s}.
        /// </summary>
        /// <remarks>
        ///     Negative values not supported.
        ///     Smaller time fractions not supported.
        ///     Fractional values supported using provided (<paramref name="cultureInfo" />) or current
        ///     <see cref="CultureInfo.NumberFormat" />'s NumberDecimalSeparator.
        ///     Abuse in the form of extremely precise fractions will have consequences in the form of invalid results.
        /// </remarks>
        /// <param name="s"></param>
        /// <param name="time"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        [PublicAPI]
        public static bool TryParseTimeString(string s, out TimeSpan time, CultureInfo cultureInfo = null)
        {
            try
            {
                time = ParseTimeString(s, cultureInfo);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Converts string like '3h 15m 25s' into a timespan of 3 hours, 15 minutes and 25 seconds.
        ///     Supported letter values: {h, m, s}.
        /// </summary>
        /// <remarks>
        ///     Negative values not supported.
        ///     Smaller time fractions not supported.
        ///     Fractional values supported using provided (<paramref name="cultureInfo" />) or current
        ///     <see cref="CultureInfo.NumberFormat" />'s NumberDecimalSeparator.
        ///     Abuse in the form of extremely precise fractions will have consequences in the form of invalid results.
        /// </remarks>
        /// <param name="s"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        [PublicAPI]
        public static TimeSpan ParseTimeString(string s, CultureInfo cultureInfo = null)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentNullException(nameof(s), "The input must not be null, empty or just whitespace");

            var decimalSeparator =
                Convert.ToChar((cultureInfo ?? CultureInfo.CurrentCulture).NumberFormat.NumberDecimalSeparator);

            var seconds = 0d;
            int? fraction = null;
            int? buffer = null;
            var position = 0;

            foreach (var c in s)
            {
                ++position;

                if (char.IsWhiteSpace(c)) continue;

                if (c > 127)
                    throw new FormatException(
                        $"Invalid non-ASCII character in string '{s}' on position {position}: '{c}'");
                if (c < 20)
                    throw new FormatException($"Invalid control character in string '{s}' on position {position}.");

                if (char.IsLetter(c))
                {
                    if (!buffer.HasValue)
                        throw new FormatException(
                            $"Unexpected letter character in string '{s}' on position {position}: '{c}'");

                    if (!TimeSegmentLabels.TryGetValue(c, out var multiplier))
                        throw new FormatException(
                            $"Unsupported letter value character in string '{s}' on position {position}: '{c}'. Supported characters: [{string.Join(", ", TimeSegmentLabels.Keys.Select(label => $"'{label}'"))}]");

                    var dBuffer = (double) buffer;
                    if (fraction.HasValue)
                    {
                        dBuffer += fraction.Value / (double) (Log10((uint) fraction.Value) + 1);
                    }

                    seconds += dBuffer * multiplier;

                    buffer = null;
                    fraction = null;

                    continue;
                }

                if (char.IsDigit(c))
                {
                    if (fraction.HasValue)
                    {
                        fraction *= 10;
                        fraction += c - '0';

                        continue;
                    }

                    if (buffer.HasValue)
                        buffer *= 10;
                    else
                        buffer = 0;

                    buffer += c - '0';


                    continue;
                }

                // ReSharper disable once InvertIf because it harshes my mellow
                if (c == decimalSeparator)
                {
                    if (fraction.HasValue)
                        throw new FormatException(
                            $"Duplicated decimal separator character in string '{s}' on position {position}: '{c}'");

                    fraction = 0;
                    continue;
                }

                // Unsupported character, somehow
                throw new FormatException($"Invalid character in string '{s}' on position {position}: '{c}'");
            }
            if(buffer.HasValue || fraction.HasValue)
                throw new FormatException("The input string ended unexpectedly");

            return TimeSpan.FromSeconds(seconds);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Log10(uint v)
            =>
                v >= 1000000000
                    ? 9
                    : v >= 100000000
                        ? 8
                        : v >= 10000000
                            ? 7
                            : v >= 1000000
                                ? 6
                                : v >= 100000
                                    ? 5
                                    : v >= 10000
                                        ? 4
                                        : v >= 1000
                                            ? 3
                                            : v >= 100
                                                ? 2
                                                : v >= 10
                                                    ? 1
                                                    : 0;
    }
}