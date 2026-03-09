# Project Rules

**지금은 핵심 시스템을 구현하기 위해서, MainScene은 비워둔 채로 각자 이름으로 Scene을 생성한 뒤에 테스트를 해주시면 되겠습니다.**

## 1. 브랜치 규칙
- 각자 자신의 이름 브랜치에서 작업합니다.
- 예시: `woosung`, `geunryeol`, `sanghyeon`...
- 최종 빌드와 제출은 `main` 기준으로 진행합니다.

## 2. 머지 규칙
- 작업 후 `main`에 합치기 전에 최신 내용을 먼저 확인합니다.
- 충돌이 나면 충돌 난 사람이 직접 해결합니다.
- 컴파일 에러가 있는 상태로는 merge 하지 않습니다.

## 3. 코드 규칙
- C# 기본 네이밍 규칙을 따릅니다.
- `private` 필드는 `_camelCase`를 사용합니다.
- 클래스, 메서드, 프로퍼티는 `PascalCase`를 사용합니다.
- 지역 변수와 파라미터는 `camelCase`를 사용합니다.

```csharp
[SerializeField] private float _moveSpeed;
private bool _isDead;

public float MoveSpeed => _moveSpeed;

public void Move()
{
    float moveInput = 0f;
}
