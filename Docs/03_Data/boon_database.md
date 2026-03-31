# 📂 개발용 은혜 데이터베이스 (Boon Database)

이 문서는 모든 은혜(Boon)를 **슬롯(Slot)**별로 분류하고, 실제 코드(C#)에서 사용할 수 있는 기술적 매개변수를 추출하여 정의합니다.

---

## 1. 슬롯 체계 (Boon Slots)
각 슬롯당 단 하나의 신의 은혜만 장착할 수 있습니다 (Passive 제외).

| 슬롯 ID | 입력 | 설명 | 연동 클래스 |
| :--- | :--- | :--- | :--- |
| `Slot_Attack` | Mouse Left | 기본 공격 개조 | `PlayerCombat.OnAttack` |
| `Slot_Special` | Q Key | 특수 공격 개조 | `PlayerCombat.OnSpecial` |
| `Slot_Magic` | Mouse Right | 마법(Magic) 시전 | `PlayerCombat.OnMagic` |
| `Slot_Dash` | Space Bar | 돌진 효과 부여 | `PlayerController.OnDash` |
| `Slot_Call` | R Key | 신의 원조 발동 | `PlayerCombat.OnCall` |
| `Slot_Passive` | N/A | 상시 적용 보조 은혜 | `BoonHandler.Passives` |

---

## 2. 로직 유형 정의 (Logic Types)
'종류' 열에 정의된 값들은 다음의 구현 방식을 의미합니다.

*   **`StatModifier`**: `StatHandler`를 통해 캐릭터의 수치를 직접 가산/승산합니다.
*   **`EffectSpawner`**: 타격이나 특정 행동 시 번개, 파도 등의 VFX 및 물리 판정을 생성합니다.
*   **`StatusApplier`**: 적중한 대상에게 상태 저주(Doom, Jolted 등)를 부여합니다.
*   **`EntitySpawner`**: 칼날 균열(Ares)이나 수정탑(Demeter)처럼 독립적인 행동을 하는 객체를 소환합니다.

---

## 3. 주 은혜 목록 (Primary Boons - Examples)

| 은혜 ID | 신(God ID) | 슬롯 | 종류 | 연동 효과 | 선행 조건 | 운용 방식 | BoonIcon | VFX_Prefab | Active_SFX |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `ZEUS_ATK_01` | `Zeus` | Attack | `EffectSpawner` | `Stat_Jolted` | None | N/A | `Icon_B_Zeus_Atk` | `VFX_Zeus_Atk` | `SFX_Zeus_Atk` |
| `ZEUS_SP_01` | `Zeus` | Special | `EffectSpawner` | N/A | None | N/A | `Icon_B_Zeus_Sp` | `VFX_Zeus_Sp` | `SFX_Zeus_Sp` |
| `ZEUS_MG_01` | `Zeus` | Magic | `EffectSpawner` | N/A | None | **비설치형** | `Icon_B_Zeus_Mg` | `VFX_Zeus_Mg` | `SFX_Zeus_Mg` |
| `ARES_ATK_01` | `Ares` | Attack | `StatusApplier` | `Stat_Doom` | None | N/A | `Icon_B_Ares_Atk` | `VFX_Ares_Atk` | `SFX_Ares_Atk` |
| `ARES_MG_01` | `Ares` | Magic | `EntitySpawner` | `Field_BladeRift` | None | **설치형** | `Icon_B_Ares_Mg` | `VFX_Ares_Mg` | `SFX_Ares_Mg` |
| `HERMES_ATK_01`| `Hermes`| Attack | `StatModifier` | N/A | None | N/A | `Icon_B_Her_Atk` | N/A | `SFX_Her_Atk` |
| `POS_DS_01` | `Poseidon`| Dash | `EffectSpawner` | `Stat_Ruptured` | None | N/A | `Icon_B_Pos_Ds` | `VFX_Pos_Ds` | `SFX_Pos_Ds` |
| `DIO_MG_01` | `Dionysus`| Magic | `EntitySpawner` | `Field_FestiveFog` | None | **설치형** | `Icon_B_Dio_Mg` | `VFX_Dio_Mg` | `SFX_Dio_Mg` |

---

## 3. 기술적 필드 매핑 가이드 (Technical Mapping)

은혜 데이터를 `BoonData` ScriptableObject에 반영할 때 다음 명칭을 권장합니다.

| 기획 문구 | 코드 필드명 (권장) | 타입 | 비고 |
| :--- | :--- | :--- | :--- |
| 벼락 피해 / 공격력 | `BaseValue` | `float` | 기본 수치 |
| 튕김 횟수 | `BounceCount` | `int` | 제우스 전용 |
| 공격 속도 / 이속 % | `SpeedMultiplier` | `float` | 가산(Additive) 또는 승산(Multi) |
| 효과 범위 | `EffectRange` | `float` | 유닛 단위 (Standard 1:100) |
| 지속 시간 | `Duration` | `float` | 초(Seconds) |
| 치명타 확률 | `CritChance` | `float` | 0.0 ~ 1.0 (Artemis 전용) |
| 회피 확률 | `DodgeRate` | `float` | 0.0 ~ 1.0 (Hermes 전용) |
| 피해 감소 % | `DamageReduction` | `float` | 0.0 ~ 1.0 |
| 자원 획득량 | `Amount` | `int` | 은화, 체력 등 고정 수치 |

---

## 4. 데이터 임포트 원칙
1.  **변동 수치:** 모든 수치는 `BoonData` 에셋의 `LevelScalingData` 테이블에 의해 레벨별로 결정됩니다.
2.  **전설/듀오:** 특수한 연출이나 전역적인 로직 변경이 필요한 경우 별도의 `SpecialEffect` 클래스를 상속받아 구현합니다.
3.  **예외 처리:** "기타" 열에 명시된 예외(예: 특정 무기 양상 시 효과 변경)는 `BoonEffect.OnApply` 시점에 체크합니다.

---

마지막 업데이트: 2026-03-31
