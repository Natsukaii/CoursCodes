using Arena.AI;
using Arena.Combat;
using UnityEngine;

namespace Arena.AI
{
    [System.Serializable]
    public class AIRangeSetting
    {
        
    }
    
    public class AI_Range : AI
    {
        [SerializeField] ProjectileLauncher launcher;
        [SerializeField] AIRangeSetting setting = new AIRangeSetting();

        void Start()
        {
            base.Start();
        }

        void Update()
        {
            if (!FollowPlayer(basicSetting.RangeToStopFollow))
            {
                launcher.TryFire();
            }
        }


    }
}
