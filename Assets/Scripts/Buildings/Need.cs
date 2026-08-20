using System;
using Agents;
using UnityEngine;

namespace Buildings
{
    /// <summary>
    ///     A need is any value that goes down over time. This class is used to simulate that are invoke actions when
    ///     certain thresholds are met.
    /// </summary>
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
            // reduce need overtime
            Amount -= Time.deltaTime;
            retryPollTimer += Time.deltaTime;

            // only invoke actions every x seconds, otherwise the action will be called every tick when the requirements are met
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
            return Amount > _upgradeThreshold * 1.1f; // slightly higher to avoid instantly downgrading
        }
        
        
    }
}