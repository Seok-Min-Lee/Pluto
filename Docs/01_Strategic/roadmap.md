# 🗺️ Pluto 프로젝트 로드맵 (Milestones)

이 문서는 설계도(`blueprint.md`)를 기반으로 한 단계별 개발 공정을 관리합니다.

---

## 📂 프로젝트 문서 맵 (Document Map)

| 카테고리 | 주요 문서 |
| :--- | :--- |
| **01_Strategic** | [blueprint.md](blueprint.md), [roadmap.md](roadmap.md), [docs_audit_report.md](docs_audit_report.md) |
| **02_Systems** | [boon_architecture.md](../02_Systems/boon_architecture.md), [game_loop_design.md](../02_Systems/game_loop_design.md), [room_system_design.md](../02_Systems/room_system_design.md) |
| **03_Data** | [boon_database.md](../03_Data/boon_database.md), [status_effect_database.md](../03_Data/status_effect_database.md), [resource_database.md](../03_Data/resource_database.md), [boon_list.md](../03_Data/boon_list.md) |
| **04_Combat** | [enemy_architecture.md](../04_Combat/enemy_architecture.md), [rarity_system.md](../04_Combat/rarity_system.md), [boon_system_design.md](../04_Combat/boon_system_design.md) |
| **05_Guides** | [design_standards.md](../05_Guides/design_standards.md), [player_input_guide.md](../05_Guides/player_input_guide.md), [asset_standards.md](../05_Guides/asset_standards.md) |

---

### Phase 1: 아키텍처 및 데이터 표준 (Foundation) - [DONE]
프로젝트의 뼈대와 규칙을 세우는 단계입니다.
*   [x] **데이터 정합성 보정:** Boon/Status/Resource DB 구축 및 용어 통일.
*   [x] **성장 공식 모델링:** 하데스식 80-60-40 감쇄 로그 및 희귀도 가중치 정립.
*   [x] **DIV 아키텍처 설계:** Data-Instance-View 삼단 구조 명문화.
*   [x] **플레이어 조작 정책:** 선입력, 대시 캔슬, 마법(Magic) 운용 가이드라인 수립.

### Phase 2: 핵심 시스템 구현 (Core Implementation) - [IN PROGRESS]
설계된 DIV 모델과 조작 가이드를 실제 코드로 변환합니다.
*   [/] **Boon/Status 시스템:** `StatHandler` 리팩토링 및 `BoonInstance` 로직 구현.
*   [ ] **Action & Hit:** 선입력이 적용된 콤보 시스템 및 `OnAttackHit` 이벤트 연동.
*   [ ] **View Interface:** 인스턴스 상태를 실시간 반영하는 HUD UI 및 VFX Controller 연동.
*   [ ] **Magic Cycle:** 설치형/비설치형 마법의 혈석(Bloodstone) 소모 및 회수 로직 구현.

### Phase 3: 타격감 및 시각적 피드백 (Juice & Polish)
*   [ ] **역경직(Hit Stop):** 피격 시 `Time.timeScale` 일시 정지 로직.
*   [ ] **카메라 피드백:** Cinemachine Impulse를 활용한 화면 흔들림(Screen Shake).
*   [ ] **피격 셰이더:** 피격된 적이 순간적으로 하얗게 변하는 White Flash 효과.
*   [ ] **잔상(Ghost Trail):** 대시 중 캐릭터 뒤에 남는 잔상 이펙트.

### Phase 4: 적(Enemy) 시스템 및 AI
*   [ ] **IDamageable 확장:** 적 캐릭터의 데이터 모델링 및 체력바 UI.
*   [ ] **상태 머신(FSM) 구축:** `Patrol`, `Chase`, `Anticipation`(공격 전조), `Attack` 상태 관리.
*   [ ] **기초 투사체(Projectile) 시스템:** 원거리 공격 발사체 및 오브젝트 풀링.

### Phase 5: 게임 루프(Game Loop) 및 안정화
*   [ ] **룸 매니저(Room Manager):** 방 진입 -> 적 섬멸 -> 보상 선택 -> 다음 방 이동 로직.
*   [ ] **최적화:** 오브젝트 풀링 적용 및 메모리 관리.

---
마지막 업데이트: 2026-03-31
감사자: Antigravity AI
