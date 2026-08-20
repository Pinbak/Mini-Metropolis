using System.Collections.Generic;
using Buildings;
using TMPro;
using UnityEngine;

namespace UserInterface
{
    /// <summary>
    ///     A class that manages the popup that shows when the player hovers over a building.
    /// </summary>
    public class BuildingHoverUserControl : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI buildingTitle;
        [SerializeField] private NeedUserControl needUserControlPrefab;
        [SerializeField] private GameObject suppliesContainer;
        [SerializeField] private GameObject demandsContainer;
        [SerializeField] private GameObject supplyHeader;
        [SerializeField] private GameObject demandHeader;

        private readonly List<NeedUserControl> _supplies = new();
        private readonly List<NeedUserControl> _demands = new();
        private const float Height = 1.3f;
        
        /// <summary>
        ///     Show a building's information in a popup style.
        /// </summary>
        public void Show(Building buildingToDisplayInformationFor, float xPosition, float yPosition)
        {
            transform.position = new Vector3(xPosition, Height, yPosition);
            gameObject.SetActive(true);
            var titleText = buildingToDisplayInformationFor.Type.ToString();
            buildingTitle.text = $"{titleText} Building";
            supplyHeader.gameObject.SetActive(true);
            demandHeader.gameObject.SetActive(true);

            // loop through all the supplies and demands needs and show them in the UI
            foreach (var supply in buildingToDisplayInformationFor.Supplies)
            {
                var need = Instantiate(needUserControlPrefab, suppliesContainer.transform);
                need.Init(supply);
                _supplies.Add(need);
            }
            
            foreach (var demand in buildingToDisplayInformationFor.Demands)
            {
                var need = Instantiate(needUserControlPrefab, demandsContainer.transform);
                need.Init(demand);
                _demands.Add(need);
            }

            // hide the heading if there are no corresponding needs
            if (_supplies.Count == 0) supplyHeader.gameObject.SetActive(false);
            if (_demands.Count == 0) demandHeader.gameObject.SetActive(false);
        }

        /// <summary>
        ///     Disable this popup.
        /// </summary>
        public void Hide()
        {
            // remove everything that was created during the Show() method
            foreach (var needUserControl in _supplies)
            {
                needUserControl.UnsubscribeFromEvents();
                Destroy(needUserControl.gameObject);
            }
            foreach (var needUserControl in _demands)
            {
                needUserControl.UnsubscribeFromEvents();
                Destroy(needUserControl.gameObject);
            }
            _supplies.Clear();
            _demands.Clear();
            gameObject.SetActive(false); // hide from player
        }
    }
}