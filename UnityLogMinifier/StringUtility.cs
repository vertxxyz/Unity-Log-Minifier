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

	extension(ReadOnlySpan<char> span)
	{
		private ReadOnlySpan<char> TrimEndNewlines() => span.TrimEnd("\r\n");
		private ReadOnlySpan<char> TrimStartNewlines() => span.TrimStart("\r\n");
	}
}