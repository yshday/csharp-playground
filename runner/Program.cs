using System.Diagnostics;
using System.Reflection;

// ─────────────────────────────────────────────────────────────
// 연습문제 러너.
//   ./run          -> 목록 보기
//   ./run ex1      -> ex1 실행
//   ./run all      -> 전부 실행 + 요약
//   ./run -w ex1   -> 파일 저장할 때마다 자동 재실행
//
// exercises/*.cs 안에 파라미터 없는 Run() 메서드를 가진 클래스를 만들면
// 자동으로 목록에 잡힌다. (static / 인스턴스 / async 다 됨)
// ─────────────────────────────────────────────────────────────

var all = Assembly.GetExecutingAssembly()
    .GetTypes()
    .Where(t => t.IsClass && !t.IsAbstract && FindRun(t) is not null)
    .OrderBy(t => t.Name, NaturalComparer.Instance)
    .ToList();

var target = args.FirstOrDefault();

if (all.Count == 0)
{
    Console.WriteLine("연습문제가 없습니다. `./new ex1` 로 만들어 보세요.");
    return 0;
}

if (target is null)
{
    Console.WriteLine($"연습문제 {all.Count}개:");
    foreach (var t in all) Console.WriteLine($"  {t.Name.ToLowerInvariant()}");
    Console.WriteLine();
    Console.WriteLine(Ansi.Dim("실행: ./run ex1   전체: ./run all   감시: ./run -w ex1   새로 만들기: ./new"));
    return 0;
}

if (target is "all" or "-a" or "--all")
{
    var results = new List<(string name, bool ok, int pass, int fail)>();
    foreach (var t in all) results.Add(await RunOne(t, header: true));

    var failed = results.Where(r => !r.ok).Select(r => r.name).ToList();
    var totalPass = results.Sum(r => r.pass);
    var totalFail = results.Sum(r => r.fail);

    Console.WriteLine();
    Console.WriteLine(Ansi.Dim(new string('-', 48)));
    Console.Write($"{results.Count}개 실행 · 검증 {Ansi.Green($"{totalPass} pass")}");
    if (totalFail > 0) Console.Write($" / {Ansi.Red($"{totalFail} fail")}");
    if (failed.Count > 0) Console.Write($" · 문제: {Ansi.Red(string.Join(", ", failed))}");
    Console.WriteLine();
    return failed.Count == 0 ? 0 : 1;
}

var match = all.FirstOrDefault(t => Normalize(t.Name) == Normalize(target));
if (match is null)
{
    Console.WriteLine(Ansi.Red($"'{target}' 을(를) 못 찾았습니다."));
    Console.WriteLine($"있는 것: {string.Join(", ", all.Select(t => t.Name.ToLowerInvariant()))}");
    return 1;
}

var single = await RunOne(match, header: false);
return single.ok ? 0 : 1;

// ── helpers ──────────────────────────────────────────────────

static string Normalize(string s) =>
    new(s.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

static MethodInfo? FindRun(Type t) =>
    t.GetMethod("Run",
        BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy,
        binder: null, types: Type.EmptyTypes, modifiers: null);

static async Task<(string name, bool ok, int pass, int fail)> RunOne(Type t, bool header)
{
    var name = t.Name.ToLowerInvariant();
    if (header)
        Console.WriteLine($"\n{Ansi.Bold(name)} {Ansi.Dim(new string('-', Math.Max(2, 40 - name.Length)))}");

    Check.Reset();
    var sw = Stopwatch.StartNew();
    var crashed = false;

    try
    {
        var m = FindRun(t)!;
        var instance = m.IsStatic ? null : Activator.CreateInstance(t);
        var result = m.Invoke(instance, null);
        if (result is Task task) await task;
        else if (result is ValueTask vt) await vt;
    }
    catch (Exception ex)
    {
        crashed = true;
        var real = ex is TargetInvocationException { InnerException: { } inner } ? inner : ex;
        Console.WriteLine($"  {Ansi.Red("THROW")} {real.GetType().Name}: {real.Message}");
        Console.WriteLine(Ansi.Dim(TrimStack(real.StackTrace)));
    }

    sw.Stop();
    var (pass, fail) = Check.Stats;

    if (pass + fail > 0)
    {
        var summary = fail == 0
            ? Ansi.Green($"{pass} pass")
            : $"{Ansi.Red($"{fail} fail")} / {pass} pass";
        Console.WriteLine($"  {summary} {Ansi.Dim($"({sw.ElapsedMilliseconds}ms)")}");
    }

    return (name, !crashed && fail == 0, pass, fail);
}

// 리플렉션 호출 프레임은 잘라내고 사용자 코드 프레임만 보여준다.
static string TrimStack(string? stack)
{
    if (string.IsNullOrEmpty(stack)) return "";
    var lines = stack.Split('\n')
        .TakeWhile(l => !l.Contains("System.Reflection.MethodBase") && !l.Contains("System.RuntimeMethodHandle"))
        .Select(l => l.TrimEnd());
    return string.Join("\n", lines);
}
