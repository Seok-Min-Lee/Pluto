# 🤝 Pluto 프로젝트 에이전트 협업 지침 (Agent Working Rules)

이 문서는 AI 에이전트(Antigravity)와 사용자 간의 원활한 협업을 위한 작업 표준 및 소통 규칙을 정의합니다.

---

## 💬 1. 소통 및 보고 (Communication)

### 1-1. 간결성 원칙 (Conciseness)
| 항목 | 규칙 |
| :--- | :--- |
| **답변 스타일** | 불필요한 미사여구(인사, 감탄사 등) 배제, 요점 중심의 간결한 직진 |
| **요소 활용** | 복잡한 설명 대신 GitHub-style Markdown(표, 리스트, 코드 블록) 적극 활용 |

---

## 💻 2. 코딩 및 설계 표준 (Coding Standards)

### 2-1. DIV 아키텍처 (Data-Instance-View)
모든 시스템은 다음 계층 분리 원칙을 엄수합니다:
1. **Data Layer (SO):** 불변의 기초 데이터 (`ScriptableObject`)
2. **Instance Layer (Class):** 런타임 계산 및 상태 전이 로직
3. **View Layer (Mono):** 시각적 연출 및 UI 바인딩

### 2-2. 코드 스타일 (Code Style)
- **네임스페이스:** `Pluto.[Category]` 형식 유지
- **명칭 규칙:**
| 구분 | 스타일 | 예시 |
| :--- | :--- | :--- |
| **비공개 필드** | `_camelCase` | `private float _moveSpeed;` |
| **공개 멤버** | `PascalCase` | `public void TakeDamage();` |
| **타입 명시** | 명시적 타입 사용 | `var` 사용 자제 (Primitive 타입 제외) |

- **구체적 코딩 규칙:**
    - **접근 지정자:** 모든 선언 시 접근 지정자(`public`, `private` 등)를 생략하지 않고 명시
    - **제어문:** 1줄의 조건문/반복문이라도 반드시 중괄호 `{ }` 사용
    - **로직 구조:** 중첩 구조 최소화 및 `Early Return` 패턴 최우선 적용
    - **주석:** 클래스/메서드 상단엔 **한글 XML 문서 주석**, 내부엔 핵심 단계를 설명하는 인라인 주석 사용

---

## 🕹️ 3. 시스템 및 유니티 표준 (System Standards)

### 3-1. 조작키 매핑 (Input Mapping)
| 기능 | 키 매핑 |
| :--- | :--- |
| **기본 공격 / 선택** | `Mouse Left` |
| **특수 공격 (Special)** | `Mouse Right` |
| **마법 (Magic)** | `Q Key` |
| **호출 (Call)** | `R Key` |
| **상호작용 (Interact)** | `E Key` |
| **돌진 (Dash)** | `Space Bar` / `Left Shift` |

### 3-2. 유니티 최적화 및 안정성
- **직렬화:** `[SerializeField] private` 필드 할당 및 `_` 접두사 사용
- **성능:** `Update`/`FixedUpdate` 내 **LINQ 사용** 및 **GetComponent 호출** 엄격 금지 (초기화 시 캐싱)
- **정상화:** 모든 문자열(태그, 레이어 등)은 `static readonly` 또는 `const` 상수로 정의
- **안전성:** 예외 발생 시 `Debug.LogError` 상세 보고 후 즉시 `Early Return`

---

## 🛠️ 4. 에이전트 작업 수행 규칙 (Agent Execution Rules)

> [!IMPORTANT]
> **4-1. 사전 조사 의무 (Pre-research)**
> 신규 로직 작성 전, 반드시 관련이 있는 **기존 스크립트 2개 이상을 정밀 분석**하여 일관성을 유지합니다.

> [!IMPORTANT]
> **4-2. 정석 지향 원칙 (Standard First)**
> 엔진 및 언어 설계 의도에 부합하는 **정석적인 방법(Best Practice)**을 최우선으로 시도하며, 사용자 승인 없이 즉각적인 우회로(Workaround)를 선택하지 않습니다.

> [!IMPORTANT]
> **4-3. 할루시네이션 방지 (Anti-Hallucination)**
> - 존재하지 않는 파일 경로나 API를 추측하지 않으며, 반드시 도구로 실존 여부를 검증합니다.
> - 확실하지 않은 기술적 사실에 대해서는 "모름"을 명시하고 사용자에게 조언을 구합니다.

> [!WARNING]
> **4-4. 사전 승인 및 정합성**
> - 대규모 변경 전 반드시 `implementation_plan.md` 승인을 득합니다.
> - 데이터 꼬임 발생 시 임의 수정 대신 Git 히스토리 기반의 복원을 시도합니다.

> [!CAUTION]
> **4-5. 통신 우회 금지 (No Communication Bypass)**
> 유니티 브릿지 연결 실패 시, 독단적인 IPv4(`127.0.0.1`) 직접 통신이나 수동 파일 패치를 통해 작업을 강행하지 않습니다. 상황 발생 시 즉각 사용자에게 보고합니다.

---
**최종 업데이트:** 2026-04-01  
**발행인:** Pluto Dev Team x Antigravity AI
