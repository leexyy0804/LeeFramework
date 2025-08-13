using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstBattle
{
    public class PlayerController : MonoBehaviour
    {
        [Header("移动设置")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float moveSmoothness = 0.1f;
        [SerializeField] private float animationTransitionDelay = 0.1f;

        [Header("组件引用")]
        [SerializeField] private Transform bodyTransform = null;
        [SerializeField] private Animator playerAnimator = null;

        private Vector3 currentVelocity;
        private Vector3 targetMovement;
        private string currentAnimationState = "StandingIdle";
        private Coroutine moveCoroutine;
        private bool isMoving = false;
        private static PlayerController s_Instance = null;

        public static PlayerController Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = FindObjectOfType<PlayerController>();
                    if (s_Instance == null)
                    {
                        GameObject go = new GameObject("PlayerController");
                        s_Instance = go.AddComponent<PlayerController>();
                    }
                }
                return s_Instance;
            }
        }

        private void Awake()
        {
            s_Instance = this;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            HandleMovement();
            HandleRotation();
            HandleActions();
        }

        private void HandleMovement()
        {
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");
            targetMovement = new Vector3(moveHorizontal, 0.0f, moveVertical);

            if (targetMovement.magnitude > 0.1f)
            {
                targetMovement.Normalize();
                UpdateMovementAnimation(targetMovement);
                
                if (!isMoving)
                {
                    if (moveCoroutine != null)
                    {
                        StopCoroutine(moveCoroutine);
                    }
                    moveCoroutine = StartCoroutine(DelayedMovement());
                }
            }
            else
            {
                if (currentAnimationState != "StandingIdle")
                {
                    playerAnimator.ResetTrigger(currentAnimationState);
                    playerAnimator.SetTrigger("StandingIdle");
                    currentAnimationState = "StandingIdle";
                }
                isMoving = false;
                if (moveCoroutine != null)
                {
                    StopCoroutine(moveCoroutine);
                    moveCoroutine = null;
                }
            }
        }

        private IEnumerator DelayedMovement()
        {
            isMoving = true;
            yield return new WaitForSeconds(animationTransitionDelay);

            while (isMoving)
            {
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    transform.position + targetMovement * moveSpeed * Time.deltaTime,
                    ref currentVelocity,
                    moveSmoothness
                );
                yield return null;
            }
        }

        private void HandleRotation()
        {
            if (bodyTransform != null)
            {
                float horizontal = Input.GetAxis("Mouse X");
                bodyTransform.Rotate(Vector3.up, horizontal * rotationSpeed);
            }
        }

        private void HandleActions()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("跳跃动作触发");
            }

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("攻击动作触发");
            }
        }

        private void UpdateMovementAnimation(Vector3 movement)
        {
            string newState;
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.z))
            {
                newState = movement.x > 0 ? "WalkRight" : "WalkLeft";
            }
            else
            {
                newState = movement.z > 0 ? "WalkForward" : "WalkBack";
            }

            if (newState != currentAnimationState)
            {
                playerAnimator.ResetTrigger(currentAnimationState);
                playerAnimator.SetTrigger(newState);
                currentAnimationState = newState;
            }
        }

        private void OnDisable()
        {
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }
        }
    }
}