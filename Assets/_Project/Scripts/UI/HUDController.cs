using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EscapeGame.Inventory;
using EscapeGame.Data;

namespace EscapeGame.UI
{
    /// <summary>画面下部のインベントリバーとHUDを管理</summary>
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private Transform inventoryBar;
        [SerializeField] private GameObject itemSlotPrefab;

        private readonly List<GameObject> _slots = new List<GameObject>();

        void Start()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnItemAdded += _ => RefreshAll();
                InventoryManager.Instance.OnItemRemoved += _ => RefreshAll();
                InventoryManager.Instance.OnItemSelected += _ => RefreshAll();
                RefreshAll();
            }
        }

        private void RefreshAll()
        {
            foreach (var slot in _slots) Destroy(slot);
            _slots.Clear();

            if (InventoryManager.Instance == null || itemSlotPrefab == null) return;

            var items = InventoryManager.Instance.Items;
            for (int i = 0; i < items.Count; i++)
            {
                var slot = Instantiate(itemSlotPrefab, inventoryBar);
                _slots.Add(slot);

                var txt = slot.GetComponentInChildren<Text>();
                if (txt != null)
                    txt.text = items[i].displayName;

                bool isSelected = InventoryManager.Instance.SelectedItem == items[i];
                var slotImg = slot.GetComponent<Image>();
                if (slotImg != null)
                    slotImg.color = isSelected ? new Color(1f, 0.9f, 0.3f) : Color.white;

                var item = items[i];
                var btn = slot.GetComponent<Button>();
                if (btn != null)
                    btn.onClick.AddListener(() => OnSlotClicked(item));
            }
        }

        private void OnSlotClicked(ItemData item)
        {
            if (InventoryManager.Instance.SelectedItem == item)
                InventoryManager.Instance.SelectItem(null);
            else
                InventoryManager.Instance.SelectItem(item);
        }

        public void OnMenuButtonClicked()
        {
            Core.GameManager.Instance?.ReturnToMainMenu();
        }
    }
}
