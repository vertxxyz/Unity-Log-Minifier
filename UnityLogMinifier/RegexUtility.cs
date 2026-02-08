using System.Text.RegularExpressions;

namespace UnityLogMinifier;

public static partial class RegexUtility
{
	[GeneratedRegex(@"^\[ line -?\d{1,32}\]\r?\n")]
	public static partial Regex LineRegex { get; }
}