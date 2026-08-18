// exercises/ex2.cs  ->  실행: ./run ex2
//
// async 도 그대로 된다. Run() 이 Task 를 돌려주면 러너가 await 해준다.
class Ex2
{
    public static async Task Run()
    {
        var value = await FetchAsync();
        Check.Equal(42, value);

        // 인스턴스 메서드로 써도 됨: public void Run() / public async Task Run()
        Check.True(DateTime.UtcNow.Year >= 2024, "시계가 정상인가");
    }

    static async Task<int> FetchAsync()
    {
        await Task.Delay(10);
        return 42;
    }
}
