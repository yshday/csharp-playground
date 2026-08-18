using System.Collections;
using System.Runtime.CompilerServices;

/// <summary>
/// 가벼운 검증 헬퍼. 별도 테스트 프로젝트 없이 연습문제 안에서 바로 쓴다.
/// 실패해도 예외를 던지지 않고 계속 진행 -> 한 번 실행에 여러 검증 결과를 다 본다.
/// </summary>
public static class Check
{
    static int _pass, _fail;

    public static (int pass, int fail) Stats => (_pass, _fail);
    public static void Reset() => (_pass, _fail) = (0, 0);

    /// <summary>기대값과 실제값 비교. 배열/리스트는 원소 단위로 비교한다.</summary>
    public static void Equal<T>(
        T expected,
        T actual,
        string? label = null,
        [CallerArgumentExpression(nameof(actual))] string? expr = null,
        [CallerLineNumber] int line = 0)
    {
        var ok = DeepEquals(expected, actual);
        Report(ok, label ?? expr, line,
            detail: ok ? Fmt(actual) : $"기대 {Ansi.Green(Fmt(expected))} / 실제 {Ansi.Red(Fmt(actual))}");
    }

    /// <summary>같지 않아야 하는 경우.</summary>
    public static void NotEqual<T>(
        T unexpected,
        T actual,
        string? label = null,
        [CallerArgumentExpression(nameof(actual))] string? expr = null,
        [CallerLineNumber] int line = 0)
        => Report(!DeepEquals(unexpected, actual), label ?? expr, line, $"!= {Fmt(unexpected)}");

    public static void True(
        bool condition,
        string? label = null,
        [CallerArgumentExpression(nameof(condition))] string? expr = null,
        [CallerLineNumber] int line = 0)
        => Report(condition, label ?? expr, line, condition ? "true" : Ansi.Red("false"));

    public static void False(
        bool condition,
        string? label = null,
        [CallerArgumentExpression(nameof(condition))] string? expr = null,
        [CallerLineNumber] int line = 0)
        => Report(!condition, label ?? expr, line, !condition ? "false" : Ansi.Red("true"));

    public static void Null(
        object? value,
        string? label = null,
        [CallerArgumentExpression(nameof(value))] string? expr = null,
        [CallerLineNumber] int line = 0)
        => Report(value is null, label ?? expr, line, Fmt(value));

    public static void NotNull(
        object? value,
        string? label = null,
        [CallerArgumentExpression(nameof(value))] string? expr = null,
        [CallerLineNumber] int line = 0)
        => Report(value is not null, label ?? expr, line, Fmt(value));

    /// <summary>지정한 예외가 나야 통과. 잡은 예외를 돌려준다.</summary>
    public static TException? Throws<TException>(
        Action action,
        string? label = null,
        [CallerArgumentExpression(nameof(action))] string? expr = null,
        [CallerLineNumber] int line = 0) where TException : Exception
    {
        try
        {
            action();
            Report(false, label ?? expr, line, $"기대 {typeof(TException).Name} / {Ansi.Red("예외 없음")}");
            return null;
        }
        catch (TException ex)
        {
            Report(true, label ?? expr, line, $"{typeof(TException).Name}: {ex.Message}");
            return ex;
        }
        catch (Exception ex)
        {
            Report(false, label ?? expr, line, $"기대 {typeof(TException).Name} / 실제 {Ansi.Red(ex.GetType().Name)}");
            return null;
        }
    }

    static void Report(bool ok, string? what, int line, string detail)
    {
        if (ok) _pass++; else _fail++;
        var mark = ok ? Ansi.Green("PASS") : Ansi.Red("FAIL");
        Console.WriteLine($"  {mark} {Ansi.Dim($"L{line}")} {what}{(string.IsNullOrEmpty(detail) ? "" : $"  {Ansi.Dim("=>")} {detail}")}");
    }

    static bool DeepEquals(object? a, object? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        if (a is string || b is string) return a.Equals(b);
        if (a is IEnumerable ea && b is IEnumerable eb)
        {
            var la = ea.Cast<object?>().ToList();
            var lb = eb.Cast<object?>().ToList();
            return la.Count == lb.Count && la.Zip(lb).All(p => DeepEquals(p.First, p.Second));
        }
        return a.Equals(b);
    }

    static string Fmt(object? value) => Dumper.Format(value);
}
