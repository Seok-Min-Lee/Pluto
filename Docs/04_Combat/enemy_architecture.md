# 👺 적군 아키텍처 및 AI 설계 (Enemy Architecture)

이 문서는 `Pluto` 프로젝트의 적 캐릭터 구조, 보호막 시스템(Armor), 그리고 AI 상태 머신을 정의합니다.

---

## 1. 클래스 구조 (Hierarchy)

모든 적군은 `EnemyBase` 클래스를 상속받으며, `IDamageable` 인터페이스를 구현합니다.

- **`EnemyBase` (Mono):** 체력, 이름, 시각적 효과(VFX), 피격 판정 로직.
- **`EnemyAI` (Mono):** 상태 머신(FSM) 및 행동 트리 관리.
- **`EnemyStats` (Data):** 적의 기본 공격력, 이동 속도, 체력 SO.

---

## 2. 보호막 및 경직 시스템 (Armor & Hit Stun)

하데스의 핵심 전투 기믹인 보호막 시스템을 구현합니다.

### [A] 노란색 보호막 (Armor)
- **효과:** 보호막이 있는 동안 모든 경직 및 밀쳐내기 효과를 무시합니다 (Super Armor).
- **파괴:** 보호막 수치가 0이 되면 파괴 연출(Armor Break)과 함께 짧은 기절 상태에 빠집니다.
- **파괴 후:** 일반 체력 상태에서는 공격 시마다 프레임 경직(Stun)이 발생합니다.

### [B] 피격 피드백 (Hit Feedback)
1. **White Flash:** 피격 시 0.1초간 하얗게 빛남.
2. **Hit Stop:** 타격 시 0.05초간 전역 시간 정지.
3. **Knockback:** 공격 속성 및 방향에 따른 물리적 밀쳐내기.

---

## 3. AI 상태 머신 (Enemy State Machine)

적군은 5가지 핵심 상태를 가집니다.

1.  **`Idle`:** 플레이어 미발견 시 정지 상태.
2.  **`Chase`:** 플레이어 감지(`AggroRange`) 시 추격 상태.
3.  **`Anticipation`:** 공격 전조 단계. 시각적/청각적 경고(예: 눈빛 반짝임) 발생.
4.  **`Attack`:** 공격 동작 실행 및 판정 생성.
5.  **`Stun` (Hit):** 피격 시나 아머 파괴 시 일시적 행동 불능 상태.

---

## 4. 데이터 기반 스폰 (Data-Driven Spawning)

- **`SpawnPoint`:** 맵에 고정된 소환 지점.
- **`EncounterData`:** 현재 방의 레벨(Tier)에 따라 소환될 적의 종류와 수량을 결정.

---

마지막 업데이트: 2026-03-31
