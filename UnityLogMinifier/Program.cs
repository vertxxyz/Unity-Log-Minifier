using System.Diagnostics;
using UnityLogMinifier;

if (args.Length == 0)
{
	Console.WriteLine("No argument was provided. Provide a path to a .log file to run (drag a file onto the executable).");
	Console.WriteLine("Press any key to exit.");
	Console.ReadKey();
	return -1;
}

// Validate arguments.
foreach (string arg in args)
{
	string filePath = arg.Trim('\"');

	if (!File.Exists(filePath))
	{
		Console.WriteLine($"No file exists at argument {filePath}.");
		Console.WriteLine("Press any key to exit.");
		Console.ReadKey();
		return -1;
	}

	if (!filePath.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
	{
		Console.WriteLine($"File at {filePath} is not a .log file.");
		Console.WriteLine("Press any key to exit.");
		Console.ReadKey();
		return -1;
	}
}

foreach (string arg in args)
{
	string filePath = arg.Trim('\"');

	string outputPath = Path.Combine(Path.GetDirectoryName(filePath)!, $"{Path.GetFileNameWithoutExtension(filePath)}_minified.log");

	using (var fileStream = File.Open(outputPath, FileMode.Create, FileAccess.Write))
	using (var textWriter = new StreamWriter(fileStream))
	{
		ReadOnlySpan<char> text = File.ReadAllText(filePath).AsSpan();
		ReadOnlySpan<char> lastSection = default;
		var occurenceOfLast = 1;
		var sectionCount = 0;

		// Collect the section of text before the next double line (or more if special logic runs).
		while (StringUtility.SplitOnNextDoubleNewLine(text, out ReadOnlySpan<char> section, out text))
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
						SplitAndWriteToFile(lastSection[..^section.Length]);
						// Match found with the remaining section.
						section = query;
						occurenceOfLast++;
						goto Next;
					}
				}

				if (occurenceOfLast > 1)
				{
					// No match found but the last section had repeats.
					textWriter.Write("--- Repeated ");
					textWriter.Write(occurenceOfLast);
					textWriter.WriteLine(" times ---");
					textWriter.WriteLine(lastSection);
					textWriter.WriteLine("--- End repeat ---");
				}
				else
				{
					// No match found and there were no repeats.
					SplitAndWriteToFile(lastSection);
				}

				textWriter.WriteLine();

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
				textWriter.Write("--- Repeated ");
				textWriter.Write(occurenceOfLast);
				textWriter.WriteLine(" times ---");
				textWriter.WriteLine(lastSection);
				textWriter.WriteLine("--- End repeat ---");
			}
			else
			{
				SplitAndWriteToFile(lastSection);
			}
		}

		textWriter.Flush();

		void SplitAndWriteToFile(ReadOnlySpan<char> text)
		{
			// Iterate singular lines and compare for repeats.
			ReadOnlySpan<char> lastLine = default;
			var occurenceOfLast = 1;
			var lineCount = 0;
			foreach (ReadOnlySpan<char> line in text.EnumerateLines())
			{
				// If we have two lines to compare.
				if (!lastLine.IsEmpty)
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
	}

	Console.WriteLine("Successfully minified log.");
	Console.WriteLine($"Output file path: {outputPath}");

	try
	{
		Process.Start("explorer", $"\"{outputPath}\"");
	}
	catch (Exception e)
	{
#if DEBUG
		Console.WriteLine(e);
#endif
		Console.WriteLine("Could not open file, press any key to continue.");
		Console.ReadKey();
	}
}

return 0;