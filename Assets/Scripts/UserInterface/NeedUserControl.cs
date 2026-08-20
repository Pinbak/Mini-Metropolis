using System;
using Agents;
using Buildings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface
{
    /// <summary>
    ///     A need that is shown to the player through hovering over a building with no placement state.
    /// </summary>
    public class NeedUserControl : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI needText;
        [SerializeField] private Image sliderImage;
        [SerializeField] private Image sliderImageBackground;
        [SerializeField] private ColourSampler colourSampler;
        [SerializeField] private Slider needAmountSlider;

        private Need _need;

        public void Init(Need need)
        {
            _need = need;
            need.AmountChanged += UpdateNeedSlider;
            needText.text = need.Type.ToString();
            SetColour(need);
            SetSliderValue(need);
        }

        private void UpdateNeedSlider(float amount)
        {
            // when the need is changed, but not too regularly, as that would hit the performance, the need is updated on the UI
            needAmountSlider.value = amount;
        }

        private void SetColour(Need need)
        {
            sliderImage.color = colourSampler.GetColourByNeed(need);
            // as the colour of the police need is black, the background which is black by default has to be changed to keep the contrast
            if (need.Type is AgentType.Police)
                sliderImageBackground.color = colourSampler.GetRoadColour();
        }

        private void SetSliderValue(Need need)
        {
            needAmountSlider.value = need.AmountAsPercentage();
        }

        public void UnsubscribeFromEvents()
        {
            _need.AmountChanged -= UpdateNeedSlider;
        }
    }
}