using System;
using Agents;
using UnityEngine;

namespace Buildings
{
    [Serializable]
    public class Need
    {
        [field:SerializeField] public AgentType Type { get; set; }
        [field:SerializeField] public float Amount { get; set; } = 50f;
        public Action<Need> BelowThreshold { get; set; }
        public Action<Need> GettingLow { get; set; }
        public Action<Need> AboveThreshold { get; set; }

        private float _upgradeThreshold = 90f;
        private float _downgradeThreshold = 10f;

        [SerializeField] private float retryPollTimer = 8f;
        [SerializeField] private float retryTime = 10f;

        public void Init(AgentType type)
        {
            Type = type;
            retryTime = type switch
            {
                AgentType.Student => 100f,
                _ => 10f
            };
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