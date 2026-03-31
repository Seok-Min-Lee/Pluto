using System;
using UnityEngine;

namespace Pluto.Core
{
    /// <summary>
    /// 실제 런타임에서 상태 이상의 남은 시간과 중첩을 관리하는 클래스. (Instance Data)
    /// </summary>
    public class StatusInstance
    {
        public StatusEffectData Data { get; private set; }
        public float RemainingTime { get; private set; }
        public int CurrentStacks { get; private set; } = 1;
        public bool IsFinished => RemainingTime <= 0;

        private StatModifier _statModifier;

        public StatusInstance(StatusEffectData data)
        {
            Data = data;
            RemainingTime = data.DefaultDuration;
        }

        /// <summary>
        /// 매 프레임 시간을 갱신합니다.
        /// </summary>
        public void Update(float deltaTime)
        {
            if (RemainingTime > 0)
            {
                RemainingTime -= deltaTime;
            }
        }

        /// <summary>
        /// 동일한 상태 이상이 다시 부여되었을 때의 처리 로직.
        /// </summary>
        public void Refresh()
        {
            if (Data.ReapplyRefreshDuration)
            {
                RemainingTime = Data.DefaultDuration;
            }

            if (CurrentStacks < Data.MaxStacks)
            {
                CurrentStacks++;
            }
        }

        /// <summary>
        /// 상태 이상 소멸 시 수정자를 해제합니다.
        /// </summary>
        public void Clear(StatHandler handler)
        {
            RemainingTime = 0;
            handler.RemoveModifiersFromSource(this);
        }

        /// <summary>
        /// StatHandler에 상태 이상 수치를 등록합니다.
        /// </summary>
        public void ApplyToHandler(StatHandler handler)
        {
            if (Data.TargetStat == default && Data.StatValuePerStack == 0) return;

            handler.RemoveModifiersFromSource(this);
            
            float finalVal = Data.StatValuePerStack * CurrentStacks;
            _statModifier = new StatModifier(finalVal, Data.ModType, this);
            
            handler.AddModifier(Data.TargetStat, _statModifier);
            
            // 태그 기반 지표도 함께 갱신 (예: Jolted_Damage 확인용)
            handler.SetAttribute(Data.EffectID, finalVal);
        }
    }
}
