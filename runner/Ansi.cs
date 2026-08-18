/// <summary>터미널 색상. 파이프/리다이렉트 시에는 자동으로 색을 끈다.</summary>
public static class Ansi
{
    const string E = "\u001b";

    static readonly bool Enabled =
        !Console.IsOutputRedirected &&
        Environment.GetEnvironmentVariable("NO_COLOR") is null;

    static string Wrap(string code, string text) => Enabled ? $"{E}[{code}m{text}{E}[0m" : text;

    public static string Green(string text) => Wrap("32", text);
    public static string Red(string text) => Wrap("31", text);
    public static string Yellow(string text) => Wrap("33", text);
    public static string Cyan(string text) => Wrap("36", text);
    public static string Dim(string text) => Wrap("2", text);
    public static string Bold(string text) => Wrap("1", text);
}
