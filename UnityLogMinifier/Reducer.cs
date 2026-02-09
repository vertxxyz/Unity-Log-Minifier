namespace UnityLogMinifier;

public static class Reducer
{
	public static void Run(string inputFilePath, string outputPath, List<IReducerPass> passes)
	{
		string text = File.ReadAllText(inputFilePath);
		for (var i = 0; i < passes.Count; i++)
		{
			ReadOnlySpan<char> span = text.AsSpan();
			if (i != passes.Count - 1)
			{
				using var stringWriter = new StringWriter();
				passes[i].Run(span, stringWriter);
				text = stringWriter.ToString();
			}
			else
			{
				using var fileStream = File.Open(outputPath, FileMode.Create, FileAccess.Write);
				using var textWriter = new StreamWriter(fileStream);
				passes[i].Run(text.AsSpan(), textWriter);
				textWriter.Flush();
			}
		}
	}
}