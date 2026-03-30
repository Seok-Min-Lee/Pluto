# 📄 Pluto(Hades-Style Action Roguelike Project) Blueprint

## 1. 프로젝트 개요 (Project Overview)
* **장르:** 쿼터뷰(Isometric) 액션 로그라이크
* **핵심 참조:** <하데스(Hades)>의 빠른 전투 템포와 성장 시스템
* **개발 환경:** Unity 6 URP
* **입력 체계:** Keyboard(WASD) + Mouse(Aim/Attack)

---

## 2. 핵심 시스템 설계 (Core System Design)

### A. 플레이어 컨트롤러 (Player Controller)
* **이동 및 회전:** 8방향 이동을 지원하며, 캐릭터는 항상 마우스 커서 방향을 응시함.
* **대시(Dash):** * 순간적인 가속 및 무적 프레임(I-frame) 제공.
    * 연속 대시 횟수(Charges) 및 재충전 쿨다운 시스템.
* **전투(Combat):**
    * **3단 콤보:** 애니메이션 프레임과 동기화된 공격 판정.
    * **입력 버퍼링(Input Buffering):** 선입력을 저장하여 공격 연계를 부드럽게 처리.
    * **마이크로 대시:** 공격 시 공격 방향으로 미세하게 전진하여 타격감 부여.

### B. 은혜 시스템 (Boon System)
* **데이터 구조:** `ScriptableObject` 기반의 능력치 변조 시스템.
* **수치 계산:** `(BaseValue + Additive) * (1 + Multiplier)` 공식을 적용한 실시간 스탯 반영.
* **효과 유형:**
    * **OnHit:** 타격 시 상태 이상(번개, 중독, 밀쳐내기 등) 부여.
    * **DashEffect:** 대시 경로에 데미지 장판 또는 이펙트 생성.

---

## 3. 리소스 및 시각적 피드백 (Resources & Juice)

### A. 타격감 유틸리티 (Combat Juice)
* **Hit Stop (역경직):** 공격 적중 시 `Time.timeScale`을 일시적으로 조절 (예: 0.05초).
* **Screen Shake:** Cinemachine Impulse를 활용한 상황별 카메라 진동.
* **White Flash:** 피격된 적의 Material을 순간적으로 흰색으로 변경하는 셰이더 로직.
* **Ghost Trail:** 대시 시 캐릭터의 뒤에 남는 잔상 이펙트.

### B. 에셋 관리 (Asset Management)
* **Object Pooling:** 투사체(Projectile), 파티클, 데미지 텍스트의 성능 최적화.
* **Animation Events:** 애니메이션 특정 프레임에서 사운드(SFX) 및 이펙트 호출.
* **NavMesh:** 적 AI의 장애물 회피 및 추적을 위한 네비게이션 데이터.

---

## 4. 기술적 아키텍처 (Technical Architecture)

### A. 클래스 구조 (Class Structure)
* `IDamageable`: 데미지를 입을 수 있는 모든 개체의 공통 인터페이스.
* `Actor` (Base): 플레이어와 적의 공통 기능을 담은 추상 클래스.
* `StatHandler`: 수치 계산 및 버프/디버프(상태 이상) 관리 담당.
* `RoomManager`: 방 진입 -> 전투 -> 보상(Boon) 선택 -> 다음 방 이동 로직 제어.

### B. 상태 머신 (State Machine)
* **PlayerStates:** Idle, Move, Dash, Attack, Hit, Die.
* **EnemyAI:** Patrol, Chase, Anticipation(공격 전조), Attack, Stun.

---

## 5. Agent 수행 지침 (Instructions for Agent)

1.  **Clean Code:** 모든 클래스는 단일 책임 원칙(SRP)을 준수하며 모듈화할 것.
2.  **Scalability:** 새로운 무기나 은혜를 추가하기 쉬운 구조(`Interface`, `Abstract Class`)로 설계할 것.
3.  **Data-Driven:** 하드코딩을 지양하고 `ScriptableObject`를 적극 활용할 것.
4.  **Comments:** 핵심 로직에는 반드시 한글 주석을 달아 설명할 것.