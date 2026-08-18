using System.Collections;
using System.Reflection;
using System.Text;

/// <summary>어떤 값이든 한 줄로 예쁘게 찍어주는 헬퍼.</summary>
public static class Dumper
{
    /// <summary>확장 메서드: 아무 값에나 .Dump() 하면 콘솔에 찍고 그 값을 그대로 돌려준다.</summary>
    public static T Dump<T>(this T value, string? label = null)
    {
        Console.WriteLine(label is null ? Format(value) : $"{Ansi.Dim(label + ":")} {Format(value)}");
        return value;
    }

    public static string Format(object? value, int depth = 0)
    {
        switch (value)
        {
            case null: return "null";
            case string s: return $"\"{s}\"";
            case char c: return $"'{c}'";
            case bool b: return b ? "true" : "false";
            case double or float or decimal:
                return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)!;
            case Enum e: return $"{e.GetType().Name}.{e}";
            case IDictionary dict:
            {
                var parts = dict.Keys.Cast<object>().Take(30)
                    .Select(k => $"{Format(k, depth + 1)}: {Format(dict[k], depth + 1)}");
                return "{ " + string.Join(", ", parts) + (dict.Count > 30 ? ", ..." : "") + " }";
            }
            case IEnumerable seq:
            {
                var items = seq.Cast<object?>().Take(31).ToList();
                var shown = items.Take(30).Select(i => Format(i, depth + 1));
                return "[" + string.Join(", ", shown) + (items.Count > 30 ? ", ..." : "") + "]";
            }
        }

        var type = value.GetType();
        if (type.IsPrimitive || value is DateTime or DateOnly or TimeOnly or TimeSpan or Guid)
            return value.ToString()!;

        // ToString()을 직접 구현한 타입(record 포함)은 그걸 신뢰
        var toString = type.GetMethod("ToString", Type.EmptyTypes);
        if (toString?.DeclaringType != typeof(object) && toString?.DeclaringType != typeof(ValueType))
            return value.ToString()!;

        if (depth >= 3) return type.Name;

        var members = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .Select(p => (p.Name, Value: SafeGet(() => p.GetValue(value))))
            .Concat(type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Select(f => (f.Name, Value: SafeGet(() => f.GetValue(value)))));

        var sb = new StringBuilder(type.Name).Append(" { ");
        sb.Append(string.Join(", ", members.Select(m => $"{m.Name} = {Format(m.Value, depth + 1)}")));
        return sb.Append(" }").ToString();
    }

    static object? SafeGet(Func<object?> get)
    {
        try { return get(); } catch { return "<error>"; }
    }
}
