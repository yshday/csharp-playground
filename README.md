# C# Playground

`exercises/ex1.cs`, `ex2.cs` … 를 계속 늘려가며 C# 을 연습하는 단일 콘솔 프로젝트.
프로젝트가 하나뿐이므로 `Main` 고민도, 테스트 프로젝트 세팅도 없다.
러너가 리플렉션으로 `Run()` 을 가진 클래스를 전부 찾아서 이름으로 골라 실행한다.

## 쓰는 법

```bash
./new             # 다음 번호로 새 파일 (exercises/ex3.cs …) 만들고 VS Code 에서 열기
./new ex7         # 이름 지정
./new linq-basics # 이런 이름도 됨 (클래스 LinqBasics)

./run             # 연습문제 목록
./run ex1         # ex1 실행
./run all         # 전부 실행 + pass/fail 요약 (실패 시 exit 1)
./run -w ex1      # 저장할 때마다 자동 재실행 (dotnet watch)
```

## 파일 한 개의 모양

파일명 = 클래스명 = 실행 이름. 파라미터 없는 `Run()` 만 있으면 된다.
(`static` / 인스턴스 / `async Task` 다 지원)

```csharp
// exercises/ex3.cs  ->  ./run ex3
class Ex3
{
    public static void Run()
    {
        var sum = Enumerable.Range(1, 10).Sum();
        sum.Dump("sum");        // 출력
        Check.Equal(55, sum);   // 검증
    }

    // 필요한 보조 클래스/record 는 이 파일 안에 그냥 더 쓰면 된다
    record Point(double X, double Y);
}
```

## 헬퍼

검증 (`runner/Check.cs`) — 실패해도 예외를 던지지 않고 계속 진행하므로
한 번 실행에 모든 검증 결과를 다 본다. 표현식 원문과 줄 번호가 자동으로 찍힌다.

| | |
|---|---|
| `Check.Equal(expected, actual)` | 배열·리스트·딕셔너리는 원소 단위로 비교 |
| `Check.NotEqual(x, actual)` | |
| `Check.True(cond)` / `Check.False(cond)` | |
| `Check.Null(v)` / `Check.NotNull(v)` | |
| `Check.Throws<TException>(() => ...)` | 잡은 예외를 반환 |

출력 (`runner/Dumper.cs`)

| | |
|---|---|
| `value.Dump()` | 아무 값이나 한 줄로 예쁘게 출력하고 그 값을 그대로 반환 |
| `value.Dump("label")` | 라벨 붙여서 출력 |

## VS Code

`ms-dotnettools.csdevkit` (C# Dev Kit) 설치를 권장. 열면 추천 알림이 뜬다.

- `Cmd+Shift+B` → 빌드
- `Cmd+Shift+P` → `Tasks: Run Test Task` → 현재 열어둔 파일 실행
- `Tasks: Run Task` → `watch: 현재 파일` / `run: 전체` / `new: 연습문제 추가`
- `F5` → 현재 열어둔 파일을 디버거로 실행 (중단점 사용 가능)

## 구조

```
Playground.csproj   프로젝트 하나 (net10.0, nullable/implicit usings 켜짐)
exercises/          연습문제. 여기만 늘려가면 된다
runner/             러너 + 헬퍼. 건드릴 일 없음
  Program.cs        이름으로 Run() 찾아 실행하는 유일한 Main
  Check.cs          검증 헬퍼
  Dumper.cs         .Dump() 출력 헬퍼
  Ansi.cs           터미널 색상
run / new           실행 · 생성 스크립트
```
