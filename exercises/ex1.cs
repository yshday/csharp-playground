// exercises/ex1.cs  ->  실행: ./run ex1
//
// 파일 하나 = 클래스 하나. 이름이 곧 실행 이름이 된다.
// 헬퍼: .Dump()  값 예쁘게 출력
//       Check.Equal / True / False / Null / NotNull / NotEqual / Throws<T>
class Ex1
{
    public static void Run()
    {
        // 1. 그냥 출력해보기
        Console.WriteLine("hello c#");
        new[] { 1, 2, 3 }.Select(x => x * x).Dump("squares");

        // 2. 검증하기 (틀려도 예외 안 나고 다음 줄로 계속 진행)
        var sum = Enumerable.Range(1, 10).Sum();
        Check.Equal(55, sum);
        Check.Equal(new[] { 1, 4, 9 }, new[] { 1, 2, 3 }.Select(x => x * x).ToArray());
        Check.True("hello".StartsWith("he"));
        Check.Throws<DivideByZeroException>(() => { var z = 0; _ = 1 / z; });

        // 3. 클래스/record 도 이 파일 안에 그냥 더 쓰면 된다 (아래 참고)
        var p = new Point(3, 4);
        Check.Equal(5.0, p.Length);
        p.Dump("point");
    }

    record Point(double X, double Y)
    {
        public double Length => Math.Sqrt(X * X + Y * Y);
    }
}
