using System;
using Needs.Agents;
using UnityEngine;

namespace Needs.Buildings
{
    public class Need
    {
        [field:SerializeField] public AgentType Type { get; set; }
        [field:SerializeField] public float Amount { get; set; } = 100f;
        public Action<Need> BelowThreshold { get; set; }
        public Action<Need> GettingLow { get; set; }
        public Action<Need> AboveThreshold { get; set; }

        private float _upgradeThreshold = 90f;
        private float _downgradeThreshold = 10f;
        private float _gettingLowThreshold = 50f;

        [SerializeField] private float _retryPollTimer = 0f;
        [SerializeField] private float _retryTime = 10f;

        public void Init(AgentType type)
        {
            Type = type;
        }

        public void Increase(float amount)
        {
            Amount += amount;
        }

        public void Update()
        {
            Amount -= Time.deltaTime;
            _retryPollTimer += Time.deltaTime;

            if (!(_retryPollTimer > _retryTime)) return;
            
            _retryPollTimer = 0f;
            GettingLow?.Invoke(this);
            
            if (Amount < _downgradeThreshold)
            {
                BelowThreshold?.Invoke(this);
            }

            if (Amount > _upgradeThreshold)
            {
                AboveThreshold?.Invoke(this);
            }
            
        }
        
        
    }
}