# 🌌 Pluto: High-Octane Action Roguelike Core System

> **하데스(Hades) 스타일의 쾌속 전투와 정교한 성장 시스템을 구현하는 유니티 기반 로그라이크 프로젝트입니다.**

`Pluto`는 단순한 액션 게임을 넘어, 확장 가능하고 유지보수가 용이한 로그라이크 시스템의 정수를 담고 있습니다. `Data-Instance-View(DIV)` 아키텍처를 기반으로 기획 데이터와 런타임 로직 간의 관계를 정립하며, 고도의 타격감을 위한 전투 유틸리티를 제공합니다.

---

## 🚀 Key Features

### 🏗️ DIV Architecture (Data-Instance-View)
- **Data Layer (SO):** 불변의 기획 데이터(ScriptableObject) 관리.
- **Instance Layer (Class):** 런타임 계산 및 상태 관리 로직.
- **View Layer (Mono):** 시각적 연출 및 UI 피드백 담당.

### 📊 Advanced Stat Engine
- **Dynamic Stat Control:** 합연산/곱연산을 유기적으로 지원하는 `StatHandler` 탑재.
- **Decay Model:** 하데스식 **80-60-40** 감쇄 로그 수식 적용으로 정교한 성장 밸런싱.

### ⚔️ Hades-Style Combat
- **Fluid Movement:** 대시 캔슬(Dash Cancel) 및 기민한 8방향 이동 시스템.
- **Combat Juice:** 역경직(Hit Stop), 화면 흔들림(Screen Shake), White Flash 등 타격감 극대화 유틸리티.
- **Magic Cycle:** 설치형/비설치형 마법과 혈석(Bloodstone) 회수 매커니즘.

---

## 🛠️ Tech Stack
- **Engine:** Unity 6 (URP)
- **Language:** C#
- **Input:** New Input System (WASD + Mouse)
- **Architecture:** DIV Pattern, ScriptableObject Driven

---

## 📂 Document Map (Strategic Docs)

| 카테고리 | 주요 문서 | 설명 |
| :--- | :--- | :--- |
| **Strategic** | [blueprint.md](Docs/01_Strategic/blueprint.md) | 프로젝트 핵심 기술 사양 및 설계도 |
| **System** | [boon_architecture.md](Docs/02_Systems/boon_architecture.md) | 은혜 시스템의 기술적 아키텍처 |
| **Combat** | [rarity_system.md](Docs/04_Combat/rarity_system.md) | 희귀도 가중치 및 성장 모델 |
| **Guide** | [player_input_guide.md](Docs/05_Guides/player_input_guide.md) | 조작 체계 및 선입력 지침 |

---

## 🗺️ Project Roadmap

- **Phase 1: Foundation (DONE)** - 아키텍처 수립 및 데이터 표준화.
- **Phase 2: Core Implementation (IN PROGRESS)** - DIV 모델 구현 및 핵심 조작계 부활.
- **Phase 3: Juice & Polish** - 타격감 강화 및 시각 피드백 고도화.
- **Phase 4: Enemy & AI** - 상태 머신(FSM) 기반 적 캐릭터 및 발사체 시스템.
- **Phase 5: Game Loop** - 룸 매니저 및 보상 사이클 안정화.

---

## 🕹️ Controls
- **이동:** `W`, `A`, `S`, `D`
- **공격:** `Mouse Left`
- **특수 공격:** `Mouse Right` / `Shift`
- **마법(Magic):** `Q`
- **호출(Call):** `R`
- **상호작용:** `E`

---
**Last Updated:** 2026-03-31  
**Project Architect:** Pluto Dev Team  
**System Auditor:** Antigravity AI
