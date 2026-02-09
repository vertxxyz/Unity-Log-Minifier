using System.Buffers;

namespace UnityLogMinifier;

/// <summary>
/// Collapses any remaining repeats.
/// </summary>
public sealed class SecondPass : IReducerPass
{
	private static readonly SearchValues<char> s_newLineChars =
		SearchValues.Create("\r\f\u0085\u2028\u2029\n");

	public void Run(ReadOnlySpan<char> text, TextWriter output)
	{
		// Iterate over all lines in text.
		ReadOnlySpan<char> remaining = text;
		do
		{
			int idx = remaining.IndexOfAny(s_newLineChars);

			if ((uint)idx < (uint)remaining.Length)
			{
				var stride = 1;

				if (remaining[idx] == '\r' && idx + 1 < remaining.Length && remaining[idx + 1] == '\n')
				{
					stride = 2;
				}

				ReadOnlySpan<char> currentAndRemaining = remaining;
				ReadOnlySpan<char> current = currentAndRemaining[..idx];
				remaining = currentAndRemaining[(idx + stride)..];

				if (current.Length == 0)
				{
					// Just a newline.
					output.WriteLine();
					continue;
				}

				// Find the next occurence of line, so we can
				// test to see if the span between this line and that line
				// is the same as the following span of equal length.
				int indexOfLine = remaining.IndexOf(current);
				int sectionLength = idx + stride + indexOfLine;
				if (
					// This line is never repeated.
					indexOfLine == -1 ||
					// This section can't repeat as it would be out of range.
					indexOfLine + sectionLength >= remaining.Length
				)
				{
					output.WriteLine(current);
					continue;
				}

				ReadOnlySpan<char> query = currentAndRemaining[..sectionLength].TrimEndNewlines();


				ReadOnlySpan<char> remainingTemp;
				var occurrences = 0;
				ReadOnlySpan<char> repeat;
				do
				{
					remainingTemp = remaining;
					repeat = remaining[indexOfLine..(indexOfLine + query.Length)];
					remaining = remaining[(indexOfLine + query.Length)..].TrimStartNewlines();
					occurrences++;
					indexOfLine = 0;
				} while (query.Length < remaining.Length && query.SequenceEqual(repeat));

				remaining = remainingTemp;

				if (occurrences == 1)
				{
					// This line is not repeated.
					output.WriteLine(current);
				}
				else
				{
					output.Write("--- Repeated ");
					output.Write(occurrences);
					output.WriteLine(" times ---");
					output.WriteLine(query);
					output.WriteLine("--- End repeat ---");
				}
			}
			else
			{
				// We've reached EOF.
				output.Write(remaining);
				return;
			}
		} while (true);
	}
}