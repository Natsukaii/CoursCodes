using Arena.AI;
using Arena.Combat;
using UnityEngine;

namespace Arena.AI
{
    [System.Serializable]
    public class AIMeleSetting
    {
        public int Amount;
        public DamageType Type;
    }
    
    public class AI_Mele : AI
    {
        [SerializeField] private AIMeleSetting _meleSettingSetting;
        private DamageInfo DamageInfo;
        
        void Start()
        {
            base.Start();
            DamageInfo = new DamageInfo(_meleSettingSetting.Amount, _meleSettingSetting.Type, gameObject);
        }

        void Update()
        {
            if (!FollowPlayer(basicSetting.RangeToStopFollow))
            {
                AttackPlayer();
            }

        }

        private void AttackPlayer()
        {
            player.TakeDamage(DamageInfo);
        }
    }
}
