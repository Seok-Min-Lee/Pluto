using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pluto.Core
{
    /// <summary>
    /// 캐릭터(플레이어/적)에 부착된 모든 상태 이상을 관리하는 컴포넌트.
    /// </summary>
    [RequireComponent(typeof(StatHandler))]
    public class StatusHandler : MonoBehaviour
    {
        private StatHandler _statHandler;
        
        // 활성화된 상태 이상 인스턴스 관리 (ID 기준)
        private Dictionary<string, StatusInstance> _activeStatusMap = new Dictionary<string, StatusInstance>();

        private void Awake()
        {
            _statHandler = GetComponent<StatHandler>();
        }

        private void Update()
        {
            UpdateStatusEffects();
        }

        /// <summary>
        /// 신규 상태 이상을 부여하거나 기존 상태 이상을 갱신합니다.
        /// </summary>
        public void AddStatusEffect(StatusEffectData data)
        {
            if (data == null) return;

            if (_activeStatusMap.TryGetValue(data.EffectID, out var instance))
            {
                // 이미 존재하면 갱신(지속시간 초기화 및 중첩 증가)
                instance.Refresh();
                Debug.Log($"[Status] Refreshed <b>{data.EffectName}</b>. Current Stacks: {instance.CurrentStacks}");
            }
            else
            {
                // 없으면 신규 생성
                instance = new StatusInstance(data);
                _activeStatusMap[data.EffectID] = instance;
                Debug.Log($"[Status] Inflicted <b>{data.EffectName}</b> on target.");
            }

            // StatHandler 수치 연동 (스탯 타입이 있을 경우)
            instance.ApplyToHandler(_statHandler);
        }

        private void UpdateStatusEffects()
        {
            List<string> expiredIDs = null;

            foreach (var kvp in _activeStatusMap)
            {
                var instance = kvp.Value;
                instance.Update(Time.deltaTime);

                if (instance.IsFinished)
                {
                    if (expiredIDs == null) expiredIDs = new List<string>();
                    expiredIDs.Add(kvp.Key);
                }
            }

            // 만료된 효과 제거
            if (expiredIDs != null)
            {
                foreach (var id in expiredIDs)
                {
                    RemoveStatusEffect(id);
                }
            }
        }

        private void RemoveStatusEffect(string id)
        {
            if (_activeStatusMap.TryGetValue(id, out var instance))
            {
                instance.Clear(_statHandler);
                _activeStatusMap.Remove(id);
                Debug.Log($"[Status] Effect <b>{id}</b> has expired.");
            }
        }

        /// <summary>
        /// 모든 상태 이상을 즉시 제거합니다. (스테이지 전환 등)
        /// </summary>
        public void ClearAllStatusEffects()
        {
            foreach (var instance in _activeStatusMap.Values)
            {
                instance.Clear(_statHandler);
            }
            _activeStatusMap.Clear();
        }

        public bool HasStatus(string id) => _activeStatusMap.ContainsKey(id);
        public StatusInstance GetStatus(string id) => _activeStatusMap.TryGetValue(id, out var instance) ? instance : null;
    }
}
