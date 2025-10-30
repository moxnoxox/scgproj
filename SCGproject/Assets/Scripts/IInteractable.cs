// Pass the interactor to the Interact method
// so the object knows who is interacting with it.
public interface IInteractable
{
    void Interact(PlayerMove player);
}

