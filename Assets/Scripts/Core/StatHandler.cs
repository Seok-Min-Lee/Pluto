using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pluto.Core
{
    /// <summary>
    /// 캐릭터가 가질 수 있는 핵심 물리량 유형.
    /// </summary>
    public enum StatType
    {
        MaxHp,
        MoveSpeed,
        DashRange,
        AttackPower,
        AttackSpeed,
        CriticalChance,
        CriticalDamage,
        MaxCallGauge,
        MaxCastCount        // 혈석(Bloodstone) 최대 보유량
    }

    public enum ModifierType { Additive, Multiplier }

    /// <summary>
    /// 수치 변경의 최소 단위. 출처(Source)를 추적하여 정확한 삭제/관리가 가능합니다. (Dynamic Data)
    /// </summary>
    public class StatModifier
    {
        public float Value;
        public ModifierType Type;
        public object Source; // 수정자를 부여한 은혜(BoonInstance) 등

        public StatModifier(float value, ModifierType type, object source)
        {
            Value = value;
            Type = type;
            Source = source;
        }
    }

    /// <summary>
    /// 단일 스탯의 정적 베이스(Static)와 동적 수정자(Instance)를 관리하는 클래스.
    /// </summary>
    [Serializable]
    public class CoreStat
    {
        public StatType Type;
        public float BaseValue; // 정적 데이터 (BaseStatData에서 주입)
        
        private readonly List<StatModifier> _modifiers = new List<StatModifier>();

        public void AddModifier(StatModifier mod) => _modifiers.Add(mod);
        public void RemoveModifiersFromSource(object source) => _modifiers.RemoveAll(m => m.Source == source);

        public float FinalValue
        {
            get
            {
                float additiveSum = 0f;
                float multiplierSum = 0f;

                for (int i = 0; i < _modifiers.Count; i++)
                {
                    StatModifier mod = _modifiers[i];
                    if (mod.Type == ModifierType.Additive)
                    {
                        additiveSum += mod.Value;
                    }
                    else if (mod.Type == ModifierType.Multiplier)
                    {
                        multiplierSum += mod.Value;
                    }
                }

                // 공식: (기본치 + 합산치) * (1 + 배율치)
                return (BaseValue + additiveSum) * (1 + multiplierSum);
            }
        }
    }

    /// <summary>
    /// 캐릭터의 모든 수치(정적+동적)를 실시간으로 연산 및 제공하는 핵심 엔진.
    /// </summary>
    public class StatHandler : MonoBehaviour
    {
        private Dictionary<StatType, CoreStat> _coreStatMap = new Dictionary<StatType, CoreStat>();
        private Dictionary<string, float> _extraAttributes = new Dictionary<string, float>();

        // 초기화 시 정적 데이터 로드전까지 사용할 기본 데이터 에셋
        [SerializeField] private BaseStatData _defaultBaseData;

        private void Awake()
        {
            if (_defaultBaseData != null)
            {
                Initialize(_defaultBaseData);
            }
        }

        /// <summary>
        /// 정적 기초 데이터(SO)를 주입받아 시스템을 초기화합니다. (Static Data Loading)
        /// </summary>
        public void Initialize(BaseStatData data)
        {
            _coreStatMap.Clear();
            AddCoreStat(StatType.MaxHp, data.MaxHp);
            AddCoreStat(StatType.MoveSpeed, data.MoveSpeed);
            AddCoreStat(StatType.DashRange, data.DashRange);
            AddCoreStat(StatType.AttackPower, data.AttackPower);
            AddCoreStat(StatType.AttackSpeed, data.AttackSpeed);
            AddCoreStat(StatType.CriticalChance, data.CriticalChance);
            AddCoreStat(StatType.CriticalDamage, data.CriticalDamage);
            AddCoreStat(StatType.MaxCallGauge, data.MaxCallGauge);
            AddCoreStat(StatType.MaxCastCount, data.MaxCastCount);

            // 추가 속성 가중치 로드 (Extra Attributes)
            _extraAttributes.Clear();
            foreach (StatExtraWeight extra in data.ExtraWeights)
            {
                _extraAttributes[extra.Tag] = extra.Value;
            }
        }

        private void AddCoreStat(StatType type, float baseVal)
        {
            _coreStatMap[type] = new CoreStat { Type = type, BaseValue = baseVal };
        }

        public float GetStatValue(StatType type)
        {
            if (_coreStatMap.TryGetValue(type, out CoreStat stat))
            {
                return stat.FinalValue;
            }
            return 0f;
        }

        public float GetAttributeValue(string tag)
        {
            if (_extraAttributes.TryGetValue(tag, out float val))
            {
                return val;
            }
            return 0f;
        }

        public void AddModifier(StatType type, StatModifier mod)
        {
            if (_coreStatMap.TryGetValue(type, out CoreStat stat))
            {
                stat.AddModifier(mod);
            }
        }

        public void RemoveModifiersFromSource(object source)
        {
            foreach (CoreStat stat in _coreStatMap.Values)
            {
                stat.RemoveModifiersFromSource(source);
            }
        }

        /// <summary>
        /// 태그 기반의 추가 속성을 동적으로 업데이트합니다. (Boon/Status 연동용)
        /// </summary>
        public void SetAttribute(string tag, float value) => _extraAttributes[tag] = value;
    }
}
