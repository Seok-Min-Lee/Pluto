using System;
using UnityEngine;

namespace Pluto.Core
{
    /// <summary>
    /// 상태 이상(디버프/버프)의 정적 데이터를 정의하는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "StatusEffectData", menuName = "Pluto/Stat/StatusEffectData")]
    public class StatusEffectData : ScriptableObject
    {
        public string EffectName;
        public string EffectID; // Stat_Jolted 등
        public Sprite Icon;

        [Header("지속시간 및 중첩")]
        public float DefaultDuration = 4.0f;
        public int MaxStacks = 1;
        public bool ReapplyRefreshDuration = true;

        [Header("연동 스탯 효과 (선택 사항)")]
        [Tooltip("상태 이상이 활성화되었을 때 StatHandler에 부여할 스탯 타입")]
        public StatType TargetStat;
        public float StatValuePerStack;
        public ModifierType ModType = ModifierType.Multiplier;

        [Header("시각 효과")]
        public GameObject OverlayVFX; // 적 몸 위에 뜨는 이펙트
    }
}
