# 🤝 Pluto 프로젝트 에이전트 협업 지침 (Agent Working Rules)

이 문서는 AI 에이전트(Antigravity)와 사용자 간의 원활한 협업을 위한 작업 표준 및 소통 규칙을 정의합니다.

---

## 💬 1. 소통 및 보고 (Communication)

### 1-1. 간결성 원칙 (Conciseness)
- 모든 답변에서 불필요한 미사여구(인사, 감탄사 등)를 배제하고 **요점 중심으로 간결하게 직진**합니다.
- 복잡한 설명 대신 GitHub-style Markdown(표, 리스트, 코드 블록)을 적극 활용합니다.

~~### 1-2. 투명한 보고 (Transparency)
- 각 턴의 작업 종료 시 수행 내역을 명확히 요약 보고합니다.
- 주요 변경 사항은 반드시 `walkthrough.md` 또는 `task.md`를 통해 가시화합니다.~~

---

## 💻 2. 코딩 및 설계 표준 (Coding Standards)

### 2-1. DIV 아키텍처 (Data-Instance-View)
- 모든 시스템은 다음 계층 분리 원칙을 엄수합니다:
    1. **Data Layer (SO):** 불변의 기초 데이터 (`ScriptableObject`).
    2. **Instance Layer (Class):** 런타임 계산 및 상태 전이 로직.
    3. **View Layer (Mono):** 시각적 연출 및 UI 바인딩.

### 2-2. 코드 스타일 (Code Style)
- **네임스페이스:** `Pluto.[Category]` 형식을 유지합니다.
- **명명 규칙:**
    - 비공개 필드(Private): `_camelCase` (예: `_moveSpeed`)
    - 공개 멤버(Public): `PascalCase`
- **구체적 코딩 규칙:**
    - **접근 지정자 명시:** 모든 클래스, 메서드, 필드 선언 시 **접근 지정자(public, private, protected 등)**를 생략하지 않고 반드시 명시합니다.
    - **제어문 중괄호:** 1줄의 조건문/반복문이라도 반드시 중괄호(`{}`)를 사용합니다.
    - **Early Return:** 복잡한 중첩 구조 대신 `Early Return` 패턴을 최우선으로 추구합니다.
    - **명시적 타입:** `var` 사용을 자제하고, 변수의 타입을 명확히 명시합니다.
- **주석:** 클래스/메서드 수준에서는 **한글 XML 문서 주석**을, 로직 내부에서는 핵심 단계를 설명하는 인라인 주석을 사용합니다.

---

## 🕹️ 3. 조작 및 시스템 표준 (System Standards)

### 3-1. 조작키 매핑 (Input Mapping)
- 프로젝트 내 모든 문서와 인풋 에셋은 다음 표준을 따릅니다:
    - **공격:** `Mouse Left`
    - **특수 공격 (Special):** `Mouse Right` (마우스 오른쪽)
    - **마법 (Magic):** `Q Key`
    - **호출 (Call):** `R Key`
    - **상호작용 (Interact):** `E Key`
    - **돌진 (Dash):** `Space Bar` / `Left Shift`

### 3-2. 스케일 및 밸런스 (Scale & Balance)
- **표준 픽셀 비율:** 1 Unit = 100 Pixels.
- **성장 모델:** 로드맵에 명시된 **80-60-40 감쇄 가중치**를 전역적으로 적용합니다.

---

### 2-3. 유니티 최적화 및 안정성 (Unity & Stability)
- **직렬화 표준 (Serialization):** `[SerializeField] private` 필드 할당을 최우선으로 하며, 필드명은 `_` 접두사를 사용합니다. (예: `_moveSpeed`)
- **성능 최적화 (Performance):** 매 프레임 호출되는 `Update`/`FixedUpdate` 내에서의 **LINQ 사용** 및 **GetComponent 호출**을 엄격히 금지합니다. (초기화 시 캐싱 필수)
- **하드코딩 방지 (No Hard-coding):** 태그, 레이어, 키워드 등 반복되는 문자열 데이터는 반드시 `static readonly` 또는 `const` 상수로 정의하여 참조합니다.
- **에러 핸들링 (Failure Analysis):** 필수 컴포넌트 참조 실패 등 예외 발생 시 `Debug.LogError`로 상세 보고 후 즉시 `Early Return` 처리합니다.

---

## 🛠️ 4. 에이전트 작업 수행 규칙 (Agent Execution Rules)

1. **사전 조사 의무 (Pre-research):** 새로운 로직이나 시스템 작성 전, 반드시 관련이 있는 **기존 스크립트 2개 이상을 `view_file`로 정밀 분석**하여 아키텍처의 일관성을 유지합니다.
2. **사전 승인:** 대규모 아키텍처 변경이나 위험도가 높은 명령 수행 전 반드시 `implementation_plan.md`를 통해 승인을 득합니다.
3. **에셋 정합성:** GUID 충돌이나 씬 데이터 꼬임 발생 시, 임의 수정 대신 Git 히스토리를 기반으로 유전적(Genetic) 복원을 시도합니다.
~~4. **통신 우회:** 유니티 브릿지 연결 실패 시, IPv4(`127.0.0.1`) 직접 통신이나 수동 파일 패치를 통해 작업을 완수합니다.~~

---
**마지막 업데이트:** 2026-03-31  
**발행인:** Pluto Dev Team x Antigravity AI
