using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pluto.Core
{
    /// <summary>
    /// 캐릭터의 변하지 않는 기초 수치(Static Data)를 정의하는 ScriptableObject.
    /// 구체적인 수치는 유니티 에디터의 인스펙터에서 에셋으로 작성합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "BaseStatData", menuName = "Pluto/Stat/BaseStatData")]
    public class BaseStatData : ScriptableObject
    {
        [Header("기초 물리량 (Core Stats)")]
        public float MaxHp = 100f;
        public float MoveSpeed = 500f;
        public float DashRange = 3.5f;

        [Header("전투 지표 (Combat Stats)")]
        public float AttackPower = 20f;
        public float AttackSpeed = 1.0f;
        public float CriticalChance = 0.05f; // 5%
        public float CriticalDamage = 2.0f;   // 200%

        [Header("특수 자원 (Special Resources)")]
        public float MaxCallGauge = 100f;
        public float MaxCastCount = 1f; // 혈석(Bloodstone) 최대 보유량
        
        /// <summary>
        /// 특정 속성(벼락 피해 등)의 기본 가중치를 설정할 때 사용합니다.
        /// </summary>
        [Header("추가 속성 가중치 (Extra Weights)")]
        public List<StatEntry> ExtraWeights = new List<StatEntry>();
    }

    [Serializable]
    public class StatEntry
    {
        public string Tag;
        public float Value;
    }
}
