using System;
using System.Collections.Generic;
using UnityEngine;
using EscapeGame.Data;

namespace EscapeGame.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [SerializeField] private int maxSlots = 16;

        private readonly List<ItemData> _items = new List<ItemData>();
        public ItemData SelectedItem { get; private set; }
        public IReadOnlyList<ItemData> Items => _items;

        public event Action<ItemData> OnItemAdded;
        public event Action<ItemData> OnItemRemoved;
        public event Action<ItemData> OnItemSelected;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool AddItem(ItemData item)
        {
            if (_items.Count >= maxSlots) return false;
            _items.Add(item);
            OnItemAdded?.Invoke(item);
            return true;
        }

        public bool RemoveItem(string itemId)
        {
            var item = _items.Find(i => i.itemId == itemId);
            if (item == null) return false;
            _items.Remove(item);
            if (SelectedItem == item) SelectItem(null);
            OnItemRemoved?.Invoke(item);
            return true;
        }

        public void SelectItem(ItemData item)
        {
            SelectedItem = item;
            OnItemSelected?.Invoke(item);
        }

        public bool HasItem(string itemId)
        {
            return _items.Exists(i => i.itemId == itemId);
        }

        public ItemData TryCombine(ItemData a, ItemData b)
        {
            // 合成レシピ探索（どちらの順番でも可）
            foreach (var item in _items)
            {
                if (item.combineIngredientA == null || item.combineIngredientB == null) continue;
                if ((item.combineIngredientA == a && item.combineIngredientB == b) ||
                    (item.combineIngredientA == b && item.combineIngredientB == a))
                {
                    RemoveItem(a.itemId);
                    RemoveItem(b.itemId);
                    AddItem(item);
                    return item;
                }
            }
            return null;
        }
    }
}
