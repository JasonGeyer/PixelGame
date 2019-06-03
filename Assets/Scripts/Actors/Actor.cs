using UnityEngine;

public class Actor : MonoBehaviour
{
    public ActorMover actorMover;
    public ActorController actorController;

    private void Start()
    {
        actorController = GetComponent<ActorController>();
        actorMover.SetActor(this);
    }

    void Update()
    {
        actorMover.GetHorizontalInput(actorController);
        actorMover.GetVerticalInput(actorController);
    }
}
