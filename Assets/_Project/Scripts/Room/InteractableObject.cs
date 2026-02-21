using UnityEngine;
using UnityEngine.EventSystems;
using EscapeGame.Core;
using EscapeGame.Data;
using EscapeGame.Inventory;

namespace EscapeGame.Room
{
    /// <summary>全インタラクト可能オブジェクトの基底クラス</summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class InteractableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private string objectId;
        [SerializeField] private string displayName;
        [SerializeField] private bool isEnabled = true;

        public string ObjectId => objectId;
        public bool IsEnabled => isEnabled;

        protected virtual void OnMouseDown()
        {
            if (!isEnabled) return;

            // UIの上でのクリックは無視
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            var selectedItem = InventoryManager.Instance != null
                ? InventoryManager.Instance.SelectedItem
                : null;

            if (selectedItem != null)
                OnItemUsed(selectedItem);
            else
                OnInteract();
        }

        protected virtual void OnMouseEnter()
        {
            if (!isEnabled) return;
            // カーソル変更は今後実装
        }

        protected virtual void OnMouseExit()
        {
            // カーソル復元は今後実装
        }

        public void Interact() => OnInteract();
        public void UseItem(ItemData item) => OnItemUsed(item);

        protected abstract void OnInteract();
        protected abstract void OnItemUsed(ItemData item);

        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
        }
    }
}
