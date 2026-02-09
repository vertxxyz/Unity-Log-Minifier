namespace UnityLogMinifier;

[Flags]
public enum Passes
{
	/// <see cref="FirstPass"/>
	First = 1 << 0,
	/// <see cref="SecondPass"/>
	Second = 1 << 1,
	All = ~0
}

public interface IReducerPass
{
	void Run(ReadOnlySpan<char> text, TextWriter output);
}