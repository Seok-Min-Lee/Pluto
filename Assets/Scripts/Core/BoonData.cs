using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pluto.Core
{
    public enum BoonSlot
    {
        Attack,
        Special,
        Magic,
        Dash,
        Call,
        Passive
    }

    /// <summary>
    /// 은혜의 고유 데이터를 정의하는 ScriptableObject. (Static Data)
    /// </summary>
    [CreateAssetMenu(fileName = "BoonData", menuName = "Pluto/Boon/BoonData")]
    public class BoonData : ScriptableObject
    {
        public string BoonName;
        public BoonSlot Slot;
        public Sprite Icon;

        [Header("기본 수치 및 타입")]
        public StatType TargetStat;
        public float BaseValue;
        public ModifierType ModType = ModifierType.Multiplier;

        [Header("성장 가중치 (80-60-40 공식 자동 적용)")]
        [Tooltip("레벨업 시 BaseValue의 몇 %가 가산되는지 정의 (기본값 0.8, 0.6, 0.4 등)")]
        public List<float> LevelScalingWeights = new List<float> { 0.8f, 0.6f, 0.4f };

        [Header("추가 태그 지표 (Extra Attributes)")]
        public List<StatEntry> ExtraAttributes = new List<StatEntry>();
    }
}
