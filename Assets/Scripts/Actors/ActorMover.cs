using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class controls the movement for actors. Actors move 1 pixel at a time by calling
/// MoveActor() - which alters timers which control when it will be called next. MoveActor()
/// recursively checks collosions and moves the actor, so if the timer is less than deltatime
/// it will make a second move. Overall this method of movement seems to be very stable so far.
/// </summary>
public class ActorMover : MonoBehaviour
{
    private Actor actor;
    public void SetActor(Actor setActor) { actor = setActor; }

    public float moveSpeed = 20f; //move 10 units per second.
    private float moveNextTime = 0;

    //falling variables
    private const float gravityForce = 20f;
    private const float minFallSpeed = 10;
    private const float maxFallSpeed = 100f;
    private float currentFallSpeed = 10;
    private float fallNextTime = 0;

    //jumping variables
    public float initialJumpSpeed = 200f;
    private const float minJumpSpeed = 0;

    private bool isAirborne = false;
    private bool isFalling = false;
    private float currentJumpSpeed = 0f;
    private float jumpMoveNextTime = 0;
    private bool isGrounded = false;

    //grabbing variables
    public bool isLedgeGrabbing = false;
    private float ledgeGrabTimer = 0;
    private float ledgeGrabWait = .5f;

    public Transform toeRightPosition;
    public Transform toeLeftPosition;
    public Transform headPosition;
    public Transform fallChecker1;
    public Transform fallChecker2;
    public Transform grabChecker1;

    public void GetHorizontalInput(ActorController actorController)
    {
        if (!isLedgeGrabbing)
        {
            if(actorController.GetActorInputData().GetHorizontalInput() < 0 && Time.time > moveNextTime)
            {
                Transform[] HitCheckers = new Transform[] { toeLeftPosition };
                bool moved = MoveActor(Vector3.left, HitCheckers, moveSpeed, ref moveNextTime);
            }
            
            if(actorController.GetActorInputData().GetHorizontalInput() > 0 && Time.time > moveNextTime)
            {
                Transform[] HitCheckers = new Transform[] { toeRightPosition };
                bool moved = MoveActor(Vector3.right, HitCheckers, moveSpeed, ref moveNextTime);
            }
        }
        else
        {
            //do something.
        }
    }

    public void GetVerticalInput(ActorController actorController)
    {
        if (!isLedgeGrabbing)
        {
            if ((Input.GetAxisRaw("Vertical") > 0) && !isAirborne && isGrounded)
            {
                isAirborne = true;
                currentJumpSpeed = initialJumpSpeed;
            }

            if (isAirborne && Time.time > jumpMoveNextTime)
            {
                Transform[] HitCheckers = new Transform[] { headPosition };
                bool moved = MoveActor(Vector3.up, HitCheckers, currentJumpSpeed, ref jumpMoveNextTime);
                currentJumpSpeed -= gravityForce;
                isGrounded = false;
                if (currentJumpSpeed <= 0 || !moved)
                {
                    isFalling = true;
                    isAirborne = false;
                    //to reset fall time
                    MoveActor(Vector3.zero, HitCheckers, currentFallSpeed, ref fallNextTime);
                }
            }

            if (!isAirborne && Time.time > fallNextTime)
            {
                Transform[] HitCheckers = new Transform[] { fallChecker1, fallChecker2 };
                bool moved = MoveActor(Vector3.down, HitCheckers, currentFallSpeed, ref fallNextTime);
                currentFallSpeed += gravityForce;
                if (!moved)
                {
                    currentFallSpeed = minFallSpeed;
                    isGrounded = true;
                    isFalling = false;
                }
                else
                {
                    isGrounded = false;
                }
            }
        }
        else
        {
            if(actorController.GetActorInputData().GetVerticalInput() > 0)
            {
                currentFallSpeed = minFallSpeed;
                currentJumpSpeed = minJumpSpeed;
                isLedgeGrabbing = false;
                ledgeGrabTimer = Time.time + ledgeGrabWait;
            }
            
            if(actorController.GetActorInputData().GetVerticalInput() < 0)
            {
                currentFallSpeed = minFallSpeed;
                currentJumpSpeed = minJumpSpeed;
                isLedgeGrabbing = false;
                ledgeGrabTimer = Time.time + ledgeGrabWait;
            }
        }
    }

    /// <summary>
    /// Checks of a ledge can be grabbed
    /// </summary>
    /// <returns>
    /// true if a ledge is grabbed
    /// </returns>
    private bool CheckLedgeGrab()
    {
        bool grabbed = false;
        //ledge grabbing
        if ((isFalling) && (Time.time > ledgeGrabTimer))
        {
            //see if the space above the grabber is empty and the space below is occupied.
            bool check1 = CheckEmpty(grabChecker1.position, Vector3.down);
            bool check2 = CheckEmpty(grabChecker1.position, Vector3.up);

            if (!check1 && check2)
            {
                isLedgeGrabbing = true;
                isAirborne = false;
                isGrounded = false;
                grabbed = true;
            }
        }
        return grabbed;
    }

    /// <summary>
    /// Move the actor recursively 1 pixel/unit at a time. This will call itself recursively
    /// if new waitTimer value would be increased by less than deltatime. This allows the function
    /// to move at a speed independent of the framerate while still preventing overshooting.
    /// </summary>
    /// <param name="moveDirection"> direction to move the actor.</param>
    /// <param name="CollisionNegative">position objects that do not want to collide.</param>
    /// <param name="moveSpeed">speed of the movement - changes the timer</param>
    /// <param name="waitTimer">timer that needs to be changed</param>
    /// <returns>
    /// True if the actor moved.
    /// </returns>
    public bool MoveActor(Vector3 moveDirection, Transform[] CollisionNegative, float moveSpeed, ref float waitTimer)
    {
        Vector3 oldPos = actor.transform.position;
        Vector3 targetPosition = oldPos + moveDirection;
        bool moveSucceed = true;
        foreach (Transform t in CollisionNegative)
        {
            if (!(CheckEmpty(t.position, moveDirection)))
            {
                moveSucceed = false;
            }
        }
        if (moveSucceed) actor.transform.position = targetPosition;
        float timeToWait = 1f / moveSpeed;

        if (waitTimer < Time.time)
        {
            waitTimer = Time.time + timeToWait;
        }
        else
        {
            waitTimer += timeToWait;
        }

        if (!CheckLedgeGrab())
        {
            if (waitTimer - Time.time < Time.deltaTime && moveSucceed)
            {
                MoveActor(moveDirection, CollisionNegative, moveSpeed, ref waitTimer);
            }
        }

        return moveSucceed;
    }

    /// <summary>
    /// Checks the positiontion for a hit.
    /// </summary>
    /// <param name="originPos"></param>
    /// <param name="pos"></param>
    /// <param name="direction"></param>
    /// <returns>
    /// returns false if the position/direction is empty, true if there's a hit.
    /// </returns>
    bool CheckEmpty(Vector3 originPos, Vector3 direction)
    {
        //toss a collder or raycast at the position and see if it's occupied
        RaycastHit2D hit = Physics2D.Raycast(originPos, direction, 1f);

        if (hit.collider == null)
        {
            return true;
        }
        return false;
    }
}
