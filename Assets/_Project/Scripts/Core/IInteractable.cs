using EscapeGame.Data;

namespace EscapeGame.Core
{
    public interface IInteractable
    {
        string ObjectId { get; }
        bool IsEnabled { get; }
        void Interact();
        void UseItem(ItemData item);
    }
}
