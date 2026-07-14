using System;
using Agents;
using UnityEngine;

namespace Buildings
{
    [Serializable]
    public class Need
    {
        [field:SerializeField] public AgentType Type { get; set; }
        [field:SerializeField] public float Amount { get; set; }
        public Action<Need> BelowThreshold { get; set; }
        public Action<Need> GettingLow { get; set; }
        public Action<Need> AboveThreshold { get; set; }

        private float _upgradeThreshold;
        private float _downgradeThreshold;
        private const float InitialDelay = 5f;

        [SerializeField] private float retryPollTimer;
        [SerializeField] private float retryTime;

        public void Init(Agent agent)
        {
            Type = agent.AgentType;
            retryTime = agent.RequestTime;
            _upgradeThreshold = agent.UpgradeAmount;
            _downgradeThreshold = agent.DowngradeAmount;
            retryPollTimer = agent.RequestTime - InitialDelay;
            Amount = (int)(_upgradeThreshold * .5f);
        }

        public void Increase(float amount)
        {
            Amount += amount;
        }

        public void Update()
        {
            Amount -= Time.deltaTime;
            retryPollTimer += Time.deltaTime;

            if (!(retryPollTimer > retryTime)) return;
            
            retryPollTimer = 0f;
            GettingLow?.Invoke(this);
            
            if (Amount < _downgradeThreshold)
            {
                BelowThreshold?.Invoke(this);
            }

            if (IsAboveThreshold())
            {
                AboveThreshold?.Invoke(this);
            }
            
        }

        public bool IsAboveThreshold()
        {
            return Amount > _upgradeThreshold;
        }
        
        
    }
}