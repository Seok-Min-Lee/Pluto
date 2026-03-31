using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pluto.Core
{
    /// <summary>
    /// 플레이어나 적이 소유한 은혜들의 인스턴스를 관리하고 StatHandler와 수치를 동기화하는 컴포넌트.
    /// </summary>
    [RequireComponent(typeof(StatHandler))]
    public class BoonHandler : MonoBehaviour
    {
        private StatHandler _statHandler;
        
        // 슬롯별 활성화된 은혜 인스턴스 저장소
        private Dictionary<BoonSlot, BoonInstance> _activeBoons = new Dictionary<BoonSlot, BoonInstance>();

        private void Awake()
        {
            _statHandler = GetComponent<StatHandler>();
        }

        /// <summary>
        /// 새로운 은혜를 해당 슬롯에 장착합니다. 같은 슬롯에 이미 은혜가 있으면 교체(Replace)됩니다.
        /// </summary>
        public void EquipBoon(BoonData data)
        {
            if (data == null)
            {
                return;
            }

            // 1. 기존 슬롯에 은혜가 있다면 StatHandler에서 해당 수치 제거
            if (_activeBoons.TryGetValue(data.Slot, out BoonInstance oldBoon))
            {
                _statHandler.RemoveModifiersFromSource(oldBoon);
                Debug.Log($"[BoonHandler] Replaced {oldBoon.Data.BoonName} with {data.BoonName}");
            }

            // 2. 새 은혜 인스턴스 생성 및 등록
            BoonInstance newInstance = new BoonInstance(data);
            _activeBoons[data.Slot] = newInstance;

            // 3. StatHandler에 새 수치 적용
            newInstance.ApplyToHandler(_statHandler);
            
            Debug.Log($"[BoonHandler] Equipped <b>{data.BoonName}</b> in {data.Slot} slot.");
        }

        /// <summary>
        /// 특정 슬롯에 장착된 은혜의 레벨을 올립니다. (석류 아이템 등)
        /// </summary>
        public void LevelUpBoon(BoonSlot slot)
        {
            if (_activeBoons.TryGetValue(slot, out BoonInstance boon))
            {
                boon.LevelUp(_statHandler);
            }
        }

        /// <summary>
        /// 특정 슬롯에 장착된 은혜 데이터를 조회합니다. (공격 로직에서 참조용)
        /// </summary>
        public BoonInstance GetBoonInstance(BoonSlot slot)
        {
            _activeBoons.TryGetValue(slot, out BoonInstance instance);
            return instance;
        }

        /// <summary>
        /// 현재 캐릭터가 특정 은혜를 소유하고 있는지 확인합니다. (선행 조건 체크용)
        /// </summary>
        public bool HasBoon(string boonName)
        {
            foreach (BoonInstance boon in _activeBoons.Values)
            {
                if (boon.Data.BoonName == boonName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
