using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ContinousMovementPhysics : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 1;
    
    public Rigidbody rb;
    public LayerMask groundLayer;
    public Transform directionSource;

    public float turnSpeed = 60;
    private float jumpVelocity = 7;
    public float jumpHeight = 1.5f;

    public InputActionProperty moveInputSource;
    public InputActionProperty turnInputSource;
    public InputActionProperty jumpInputSource;

    public CapsuleCollider bodyCollider;
    private Vector2 inputMoveAxis;
    private float inputTurnAxis;

    private bool isGrounded;
    public bool onlyMoveWhenGrounded = false;

    public Transform turnSource;
    // Update is called once per frame

    public AudioSource footstepAudioSource;

    public AudioClip jumpSound; 
    public AudioClip stepSound;
  
    void Update()
    {
        // Введение системы управления
        inputMoveAxis = moveInputSource.action.ReadValue<Vector2>();
        inputTurnAxis = turnInputSource.action.ReadValue<Vector2>().x;

        bool jumpInput = jumpInputSource.action.WasPressedThisFrame();

        // Срабатывание прыжка, если игрок на земле и нажата кнопка прыжка
        if(jumpInput && isGrounded)
        {
            jumpVelocity = Mathf.Sqrt(2*-Physics.gravity.y*jumpHeight);
            rb.velocity = Vector3.up * jumpVelocity;

            footstepAudioSource.PlayOneShot(jumpSound);
        }
    }

    // Проверка, на земле ли игрок
    public bool CheckIfGrounded()
    {
        Vector3 start = bodyCollider.transform.TransformPoint(bodyCollider.center);

        float rayLength = bodyCollider.height / 2 - bodyCollider.radius + 0.05f;

        bool hasHit = Physics.SphereCast(
            start, 
            bodyCollider.radius, 
            Vector3.down, 
            out RaycastHit hitInfo, 
            rayLength, 
            groundLayer
        );
        
        return hasHit;
    }
 
    private void FixedUpdate()
    {
        // Срабатывание перемещения
        isGrounded = CheckIfGrounded();
        
        if(!onlyMoveWhenGrounded || (onlyMoveWhenGrounded && isGrounded))
        {
            Quaternion yaw = Quaternion.Euler(0, directionSource.eulerAngles.y, 0);
            Vector3 direction = yaw *new Vector3(inputMoveAxis.x, 0, inputMoveAxis.y);

            Vector3 targetMovePosition = rb.position + direction * Time.fixedDeltaTime * speed;

            Vector3 axis = Vector3.up;
            float angle = turnSpeed * Time.fixedDeltaTime * inputTurnAxis;

            Quaternion q = Quaternion.AngleAxis(angle, axis);

            rb.MoveRotation(rb.rotation* q); 

            Vector3 newPosition = q * (targetMovePosition - turnSource.position) + turnSource.position;

            rb.MovePosition(newPosition);

            if (direction.magnitude > 0 && isGrounded)
            {
                if (!footstepAudioSource.isPlaying)
                {
                    footstepAudioSource.PlayOneShot(stepSound);
                }
            }
        }
    }

}
