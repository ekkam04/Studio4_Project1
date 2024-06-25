using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.UI;

namespace Ekkam
{
    public class Player : MonoBehaviour
    {
        public enum MovementState
        {
            Walking,
            Sprinting,
            Air
        }

        [Header("--- Player Settings and References ---")]
        public Rigidbody rb;
        public Animator anim;
        public MovementState movementState;

        private bool allowMovement = true;
        public bool allowFall = true;

        public Vector3 viewDirection;
        public Transform orientation;

        public Transform cameraPos;
        public Transform cameraObj;

        [Header("--- Movement Settings ---")]
        public float rotationSpeed = 5f;
        public float horizontalInput = 0f;
        public float verticalInput = 0f;
        public float moveX;
        public float moveZ;
        Vector3 moveDirection;
        Vector3 combatRotationDirection;
        
        private float speed = 3.0f;
        private float maxSpeed = 5.0f;
        public float walkSpeed = 3.0f;
        public float sprintSpeed = 5.0f;
        public float maxSpeedOffset = 2.0f;
        public float groundDrag = 3f;
        public float groundDistance = 0.5f;
        
        public bool isGrounded;
        public bool isJumping;
        public bool isMoving;
        public bool isSprinting;
        public bool allowDoubleJump;
        public bool doubleJumped;
        public bool hasLanded;

        public float jumpHeightApex = 2f;
        public float jumpDuration = 1f;
        float currentJumpDuration;
        private float downwardsGravityMultiplier = 1.5f;
        public float normalDownwardsGravityMultiplier = 1.5f;
        float gravity;
        private float initialJumpVelocity;
        private float jumpStartTime;
        
        [Header("--- Networking Settings ---")]
        private float networkSendRate = 0.05f;
        private float networkPositionSendTimer;
        private float networkRotationSendTimer;
        private float networkAnimationSendTimer;
        public float syncSmoothness = 20f;
        public NetworkComponent networkComponent;
        public bool isMine;
        public Vector3 lastSentPosition;
        private bool sendingPosition;
        public float lastSentRotationY;
        private bool isMovingUpdateSent;

        void Start()
        {
            anim = GetComponent<Animator>();
            networkComponent = GetComponent<NetworkComponent>();
            
            isMine = networkComponent.IsMine();
            if (!isMine)
            {
                Destroy(rb);
                return;
            }
            
            rb = GetComponent<Rigidbody>();
            
            cameraObj = Camera.main.transform;

            gravity = -2 * jumpHeightApex / (jumpDuration * jumpDuration);
            initialJumpVelocity = Mathf.Abs(gravity) * jumpDuration;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            networkPositionSendTimer += Time.deltaTime;
            networkRotationSendTimer += Time.deltaTime;
            networkAnimationSendTimer += Time.deltaTime;
            
            if (!isMine)
            {
                transform.position = Vector3.Lerp(transform.position, lastSentPosition, Time.deltaTime * syncSmoothness);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, lastSentRotationY, 0), Time.deltaTime * syncSmoothness);
                return;
            }
            
            // Camera and orientation
            viewDirection = cameraObj.forward;
            viewDirection.y = 0;
            orientation.forward = viewDirection.normalized;

            // Movement animation
            moveX = Mathf.Lerp(moveX, horizontalInput, Time.deltaTime * 5f);
            moveZ = Mathf.Lerp(moveZ, verticalInput, Time.deltaTime * 5f);
            anim.SetFloat("moveX", moveX);
            anim.SetFloat("moveZ", moveZ);
            if (networkAnimationSendTimer >= networkSendRate + 0.1f)
            {
                networkAnimationSendTimer = 0f;
                Client.instance.SendAnimationState(AnimationStatePacket.AnimationCommandType.Float, "moveX", false, moveX);
                Client.instance.SendAnimationState(AnimationStatePacket.AnimationCommandType.Float, "moveZ", false, moveZ);
            }
            
            // Movement
            speed = isSprinting ? sprintSpeed : walkSpeed;
            maxSpeed = speed + maxSpeedOffset;
            downwardsGravityMultiplier = normalDownwardsGravityMultiplier;
            ControlSpeed();
            CheckForGround();
            MovementStateHandler();
            
            if (
                networkPositionSendTimer >= networkSendRate &&
                Client.instance != null
            )
            {
                networkPositionSendTimer = 0f;
                
                if (rb.velocity.magnitude > 0.1f)
                {
                    sendingPosition = true;
                    Client.instance.SendPosition(transform.position);
                }
                else if (sendingPosition) // Send position one last time when player stops
                {
                    sendingPosition = false;
                    Client.instance.SendPosition(transform.position);
                }
            }
        }

        void FixedUpdate()
        {
            if (!isMine) return;
            
            // Move player
            MovePlayer();

            // Orient player
            Quaternion targetRotation = Quaternion.LookRotation(viewDirection.normalized, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
            // rb.MoveRotation(targetRotation);

            // Jumping
            if (isJumping)
            {
                rb.AddForce(Vector3.up * gravity, ForceMode.Acceleration);

                if (Time.time - jumpStartTime >= currentJumpDuration)
                {
                    isJumping = false;
                    hasLanded = false;
                }
            }
            else
            {
                if (!allowFall || isGrounded) return;
                rb.AddForce(Vector3.down * -gravity * downwardsGravityMultiplier, ForceMode.Acceleration);
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            if (!this.enabled || !isMine) return;
            Vector2 input = context.ReadValue<Vector2>();
            horizontalInput = input.x;
            verticalInput = input.y;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (!this.enabled || !isMine) return;
                if (!isGrounded && allowDoubleJump && !doubleJumped)
                {
                    doubleJumped = true;
                    // anim.SetTrigger("doubleJump");
                    StartJump(jumpHeightApex, jumpDuration);
                }
                else if (isGrounded)
                {
                    doubleJumped = false;
                    StartJump(jumpHeightApex, jumpDuration);
                }
            }
        }
        
        public void OnLook(InputAction.CallbackContext context)
        {
            if (!this.enabled || !isMine) return;
            if (
                networkRotationSendTimer >= networkSendRate &&
                Client.instance != null
            )
            {
                networkRotationSendTimer = 0f;
                Client.instance.SendRotationY(transform.rotation.eulerAngles.y);
            }
        }

        void MovePlayer()
        {
            if (!allowMovement) return;
            
            // Calculate movement direction
            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

            float moveSpeed = speed;
            rb.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);
        }

        void ControlSpeed()
        {
            // Limit velocity if needed
            Vector3 flatVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            if (flatVelocity.magnitude > maxSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * maxSpeed;
                rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
            }
        }

        void StartJump(float heightApex, float duration)
        {
            // Recalculate gravity and initial velocity
            gravity = -2 * heightApex / (duration * duration);
            initialJumpVelocity = Mathf.Abs(gravity) * duration;
            currentJumpDuration = duration;

            isJumping = true;
            // anim.SetBool("isJumping", true);
            jumpStartTime = Time.time;
            rb.velocity = Vector3.up * initialJumpVelocity;
        }

        void CheckForGround()
        {
            RaycastHit hit;
            bool foundGround = Physics.Raycast(transform.position + new Vector3(0, 0.1f, 0), Vector3.down, out hit, groundDistance);
            if (foundGround)
            {
                isGrounded = true;
                rb.drag = groundDrag;

                if (!hasLanded)
                {
                    hasLanded = true;
                    // anim.SetBool("isJumping", false);
                }

                // if (hit.collider.CompareTag("Movable"))
                // {
                //     transform.parent = hit.transform;
                //     rb.interpolation = RigidbodyInterpolation.None;
                // }
            }
            else
            {
                isGrounded = false;
                rb.drag = 0;
                // transform.parent = null;
                // rb.interpolation = RigidbodyInterpolation.Interpolate;
            }
            
            //draw ray
            Debug.DrawRay(transform.position + new Vector3(0, 0.1f, 0), Vector3.down * groundDistance, foundGround ? Color.green : Color.red);
        }

        void MovementStateHandler()
        {
            if (isGrounded && isSprinting)
            {
                movementState = MovementState.Sprinting;
            }
            else if (isGrounded)
            {
                movementState = MovementState.Walking;
            }
            else
            {
                movementState = MovementState.Air;
            }
        }
    }
}