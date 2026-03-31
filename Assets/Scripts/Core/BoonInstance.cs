using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pluto.Core
{
    /// <summary>
    /// 플레이어가 실제로 장착한 은혜의 상태를 관리하는 가변형 클래스. (Instance Data)
    /// </summary>
    public class BoonInstance
    {
        public BoonData Data { get; private set; }
        public int CurrentLevel { get; private set; } = 1;

        private StatModifier _currentModifier;

        public BoonInstance(BoonData data)
        {
            Data = data;
        }

        /// <summary>
        /// 80-60-40 성장 공식을 적용하여 레벨에 따른 최종 수치를 계산합니다.
        /// </summary>
        public float CalculateFinalValue()
        {
            float totalValue = Data.BaseValue;
            
            // 레벨 1은 BaseValue 그대로 사용.
            // 레벨 2부터 가중치 리스트에 따라 누적 합산.
            for (int i = 1; i < CurrentLevel; i++)
            {
                float weight = 0.3f; // 기본 최소 가중치 (고정)
                
                // 지정된 가중치 리스트(80, 60, 40)가 있으면 해당 값 사용
                if (Data.LevelScalingWeights != null && i - 1 < Data.LevelScalingWeights.Count)
                {
                    weight = Data.LevelScalingWeights[i - 1];
                }
                
                totalValue += Data.BaseValue * weight;
            }
            
            return (float)Math.Round(totalValue, 2); // 소수점 정렬
        }

        /// <summary>
        /// 계산된 수치를 StatHandler에 등록/갱신합니다.
        /// </summary>
        public void ApplyToHandler(StatHandler handler)
        {
            // 기존 수정자 먼저 제거
            handler.RemoveModifiersFromSource(this);

            float finalVal = CalculateFinalValue();
            _currentModifier = new StatModifier(finalVal, Data.ModType, this);
            
            // 스탯 수정자 등록
            handler.AddModifier(Data.TargetStat, _currentModifier);

            // 추가 태그 속성(Exposed 등) 업데이트
            foreach (var extra in Data.ExtraAttributes)
            {
                handler.SetAttribute(extra.Tag, extra.Value);
            }
        }

        public void LevelUp(StatHandler handler)
        {
            CurrentLevel++;
            ApplyToHandler(handler);
            Debug.Log($"[Boon] {Data.BoonName} Leveled Up to {CurrentLevel}. Final Value: {CalculateFinalValue()}");
        }
    }
}
