namespace UnityLogMinifier;

/// <summary>
/// A broad pass collapsing and annotating the following:
/// <list type="bullet">
///	<item>Repeated sections of text separated by multiple newlines.</item>
///	<item>Repeated single line text.</item>
/// </list>
/// </summary>
public sealed class FirstPass : IReducerPass
{
	public void Run(ReadOnlySpan<char> text, TextWriter output)
	{
		ReadOnlySpan<char> lastSection = default;
		var occurenceOfLast = 1;
		var sectionCount = 0;

		ReadOnlySpan<char> remaining = text;
		// Collect the section of text before the next double line (or more if special logic runs).
		while (StringUtility.SplitOnNextDoubleNewLine(remaining, out ReadOnlySpan<char> section, out remaining))
		{
			// If we have two sections to compare.
			if (!lastSection.IsEmpty)
			{
				if (lastSection.SequenceEqual(section))
				{
					// Match found with the current section.
					occurenceOfLast++;
					goto Next;
				}

				if (lastSection.Length > section.Length)
				{
					// Get the previous section that could equal the next and compare them.
					ReadOnlySpan<char> query = lastSection[^section.Length..];
					if (query.SequenceEqual(section))
					{
						// Write the section we're not comparing to the file.
						StringUtility.SplitAndWrite(output, lastSection[..^section.Length]);
						// Match found with the remaining section.
						section = query;
						occurenceOfLast++;
						goto Next;
					}
				}

				if (occurenceOfLast > 1)
				{
					// No match found but the last section had repeats.
					output.Write("--- Repeated ");
					output.Write(occurenceOfLast);
					output.WriteLine(" times ---");
					output.WriteLine(lastSection);
					output.WriteLine("--- End repeat ---");
				}
				else
				{
					// No match found and there were no repeats.
					StringUtility.SplitAndWrite(output, lastSection);
				}

				output.WriteLine();

				occurenceOfLast = 1;
			}

			Next:
			lastSection = section;
			sectionCount++;
		}

		// Write the remaining section/repeats.
		if (sectionCount > 0)
		{
			if (occurenceOfLast > 1)
			{
				output.Write("--- Repeated ");
				output.Write(occurenceOfLast);
				output.WriteLine(" times ---");
				output.WriteLine(lastSection);
				output.WriteLine("--- End repeat ---");
			}
			else
			{
				StringUtility.SplitAndWrite(output, lastSection);
			}

			output.WriteLine();
		}

		StringUtility.SplitAndWrite(output, remaining);
	}
}