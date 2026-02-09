namespace UnityLogMinifier;

public static class StringUtility
{
	public static bool SplitOnNextDoubleNewLine(ReadOnlySpan<char> value, out ReadOnlySpan<char> start, out ReadOnlySpan<char> remaining)
	{
		var i = 0;
		do
		{
			int indexOf = value[i..].IndexOf('\n');
			if (indexOf == -1)
			{
				start = default;
				remaining = value;
				return false;
			}
			
			i += indexOf + 1;
			// Would a double-newline be out of range?
			if (i >= value.Length)
			{
				start = default;
				remaining = value;
				return false;
			}

			// Is there a double-newline here?
			if (value[i] is not ('\r' or '\n'))
			{
				continue;
			}

			// Is there any text here?
			if (i + 1 >= value.Length)
			{
				start = default;
				remaining = value;
				return false;
			}

			remaining = value[(i + 1)..].TrimStartNewlines();
			// Merge [ line ...] with the previous section, as it's sometimes included with certain logs.
			if (RegexUtility.LineRegex.IsMatch(remaining))
			{
				int ii = remaining.IndexOf('\n');
				if (ii != -1)
				{
					i += ii + 2;
					remaining = value[(i + 1)..].TrimStartNewlines();
				}
			}

			start = value[..(i - 1)].TrimEndNewlines();
			return remaining.Length > 0;
		} while (true);
	}
	
	public static void SplitAndWrite(TextWriter textWriter, ReadOnlySpan<char> text)
	{
		// Iterate singular lines and compare for repeats.
		ReadOnlySpan<char> lastLine = default;
		var occurenceOfLast = 1;
		var lineCount = 0;
		foreach (ReadOnlySpan<char> line in text.EnumerateLines())
		{
			// If we have two lines to compare.
			if (lineCount > 0)
			{
				if (line.SequenceEqual(lastLine))
				{
					// Match found with the current line.
					occurenceOfLast++;
				}
				else if (occurenceOfLast > 1)
				{
					// No match found but the last line was repeated.
					textWriter.WriteLine(lastLine);
					textWriter.Write('\t');
					textWriter.Write("⤷ Repeated ");
					textWriter.Write(occurenceOfLast);
					textWriter.WriteLine(" times.");
					occurenceOfLast = 1;
				}
				else
				{
					// No match found, and no repetitions.
					textWriter.WriteLine(lastLine);
					occurenceOfLast = 1;
				}
			}

			lastLine = line;
			lineCount++;
		}

		// Write the remaining line/repeats.
		if (lineCount > 0)
		{
			textWriter.WriteLine(lastLine);
			if (occurenceOfLast > 1)
			{
				textWriter.Write('\t');
				textWriter.Write("⤷ last line repeated ");
				textWriter.Write(occurenceOfLast);
				textWriter.WriteLine(" times.");
			}
		}
	}

	extension(ReadOnlySpan<char> span)
	{
		public ReadOnlySpan<char> TrimEndNewlines() => span.TrimEnd("\r\n");
		public ReadOnlySpan<char> TrimStartNewlines() => span.TrimStart("\r\n");
	}
}