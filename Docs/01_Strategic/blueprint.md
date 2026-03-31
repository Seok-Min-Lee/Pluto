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
* **대시(Dash):** 
    * 순간적인 가속 및 무적 프레임(I-frame) 제공.
    * 상세 명세: [player_input_guide.md](../05_Guides/player_input_guide.md) 참조.

### B. 은혜 시스템 (Boon System)
* **데이터 구조:** `ScriptableObject` 기반의 **Data-Instance-View (DIV)** 분리 시스템.
* **아키텍처:** [boon_architecture.md](../02_Systems/boon_architecture.md) 참조.
* **성장 공식:** **80-60-40** 감쇄 모델. [rarity_system.md](../04_Combat/rarity_system.md) 참조.

---

## 3. 리소스 및 시각적 피드백 (Resources & Juice)

### A. 타격감 유틸리티 (Combat Juice)
* **Hit Stop (역경직), Screen Shake, White Flash** 등을 지원.
* **표준 지침:** [design_standards.md](../05_Guides/design_standards.md) 참조.

### B. 에셋 관리 (Asset Management)
* **PPU 100** 표준 준수. [asset_standards.md](../05_Guides/asset_standards.md) 참조.

---

## 4. 기술적 아키텍처 (Technical Architecture)

### A. 클래스 구조 (Data-Instance-View)
1. **Data Layer (SO):** 불변의 기획 데이터. [boon_database.md](../03_Data/boon_database.md) 참조.
2. **Instance Layer (Class):** 런타임 계산 로직.
3. **View Layer (Mono):** 시각적 연출 및 UI.

### B. 게임 흐름 제어
* **게임 루프:** [game_loop_design.md](../02_Systems/game_loop_design.md) 참조.
* **방 시스템:** [room_system_design.md](../02_Systems/room_system_design.md) 참조.

---

마지막 업데이트: 2026-03-31
감사자: Antigravity AI