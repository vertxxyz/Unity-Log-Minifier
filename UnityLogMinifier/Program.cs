using System.Diagnostics;
using UnityLogMinifier;

const string dontOpenArg = "--dont-open-file";
const string skipFirstPass = "--skip-first-pass";
const string skipSecondPass = "--skip-second-pass";

var openFiles = true;
var activePasses = Passes.All;

// Validate arguments.
List<string> files = [];
foreach (string arg in args)
{
	if (arg.Equals(dontOpenArg, StringComparison.OrdinalIgnoreCase))
	{
		openFiles = false;
		continue;
	}

	if (arg.Equals(skipFirstPass, StringComparison.OrdinalIgnoreCase))
	{
		activePasses &= ~Passes.First;
		continue;
	}
	
	if (arg.Equals(skipSecondPass, StringComparison.OrdinalIgnoreCase))
	{
		activePasses &= ~Passes.Second;
		continue;
	}

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
	
	files.Add(filePath);
}

// Validate input files.
if (files.Count == 0)
{
	Console.WriteLine("No files were provided. Provide a path to a .log file to run (drag a file onto the executable).");
	Console.WriteLine("Press any key to exit.");
	Console.ReadKey();
	return -1;
}

// Validate input passes.
var passes = new List<IReducerPass>(2);
if ((activePasses & Passes.First) != 0)
	passes.Add(new FirstPass());
if ((activePasses & Passes.Second) != 0)
	passes.Add(new SecondPass());

if (passes.Count == 0)
{
	Console.WriteLine("All passes are skipped, no logic will run.");
	Console.WriteLine("Press any key to exit.");
	Console.ReadKey();
	return -1;
}

// Run reducer on files.
foreach (string filePath in files)
{
	string outputPath = Path.Combine(Path.GetDirectoryName(filePath)!, $"{Path.GetFileNameWithoutExtension(filePath)}_minified.log");

	Reducer.Run(filePath, outputPath, passes);

	Console.WriteLine("Successfully minified log.");
	Console.WriteLine($"Output file path: {outputPath}");

	if (openFiles)
	{
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
}

return 0;