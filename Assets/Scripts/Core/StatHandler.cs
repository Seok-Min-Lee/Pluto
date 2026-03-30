using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pluto.Core
{
    public enum StatType
    {
        MaxHp,
        MoveSpeed,
        AttackPower,
        AttackSpeed,
        CriticalChance,
        CriticalDamage
    }

    [Serializable]
    public class Stat
    {
        public StatType Type;
        public float BaseValue;
        public float Additive;
        public float Multiplier;

        public float TotalValue => (BaseValue + Additive) * (1 + Multiplier);
    }

    /// <summary>
    /// 캐릭터의 수치(스탯) 계산 및 관리를 담당하는 컴포넌트.
    /// </summary>
    public class StatHandler : MonoBehaviour
    {
        [SerializeField] 
        private List<Stat> stats = new List<Stat>();
        
        private Dictionary<StatType, Stat> _statMap = new Dictionary<StatType, Stat>();

        private void Awake()
        {
            InitializeMap();
        }

        private void InitializeMap()
        {
            _statMap.Clear();
            foreach (var stat in stats)
            {
                _statMap[stat.Type] = stat;
            }
        }

        public float GetValue(StatType type)
        {
            if (_statMap.TryGetValue(type, out var stat))
            {
                return stat.TotalValue;
            }
            return 0f;
        }

        public void AddAdditive(StatType type, float amount)
        {
            if (_statMap.TryGetValue(type, out var stat)) stat.Additive += amount;
        }

        public void AddMultiplier(StatType type, float amount)
        {
            if (_statMap.TryGetValue(type, out var stat)) stat.Multiplier += amount;
        }
    }
}
