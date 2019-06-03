using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class acts as a switch to take input from different control sources
/// ControllerType needs to be set to change the control source. By setting
/// The controller type, a different methods can be added to the 'controller' 
/// delegate which controls the behaviour of the actor.
/// </summary>
public class ActorController : MonoBehaviour
{
    private ActorInputData ActorInput = new ActorInputData();
    public ActorInputData GetActorInputData() { return ActorInput; }

    public class ActorInputData
    {
        private float HorizontalInput = 0;
        public float GetHorizontalInput() { return HorizontalInput; }
        public void SetHorizontalInput(float f) { HorizontalInput = f; }

        private float VerticalInput = 0;
        public float GetVerticalInput() { return VerticalInput; }
        public void SetVerticalInput(float f) { VerticalInput = f; }
    }

    public ControllerType controllerType;

    public enum ControllerType
    {
        HumanPlayer,
        PatrollerAI
    }

    Controller controller;
    private delegate void Controller(ActorInputData ActorInput);

    private void Start()
    {
        SetControllerType();
    }

    private void SetControllerType()
    {
        //add delegates to 'controller' to control what is updated.
        //will allow additional behaviours to be included with different 
        //AI types.
        if (controllerType == ControllerType.HumanPlayer)
        {
            controller += GetPlayerAxis;
        }

        if (controllerType == ControllerType.PatrollerAI)
        {
            controller += ErrorMessage;
        }
    }

    private void Update()
    {
        controller.Invoke(ActorInput);
    }

    private void GetPlayerAxis(ActorInputData aInput)
    {
        aInput.SetHorizontalInput(Input.GetAxisRaw("Horizontal"));
        aInput.SetVerticalInput(Input.GetAxisRaw("Vertical"));
    }

    //havent made any ai's yet. Will do them here. Probably need to put these behaviours elsewhere.
    private void ErrorMessage(ActorInputData aInput)
    {
        Debug.Log("This ControllerType has not been setup yet. Please change the ControllerType in your ActorController.");
    }
}
