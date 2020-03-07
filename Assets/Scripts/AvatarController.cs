using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class AvatarController : MonoBehaviour
{
    public AnimationCurve AnimationCurve; //still not used, test only
    public float MoveSpeed; // m/sec
    public float RotationSpeed; //fixed secs from current to target

    [SerializeField] private Transform _cameraTransform;
    private CharacterController _characterController;
    private PlayerInput _playerInput;
    private Vector2 _moveInput;
    private Transform _characterTransform;
    private Animator _characterAnimator;

    private void Awake()
    {
        _playerInput = new PlayerInput();
        _characterController = GetComponent<CharacterController>();
        _characterTransform = GetComponent<Transform>();
        _characterAnimator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (_playerInput != null)
        {
            _playerInput.PlayerControls.Move.performed += UpdatePlayerInput;
            _playerInput.Enable();
        }
    }

    private void OnDisable()
    {
        if (_playerInput != null)
        {
            _playerInput.PlayerControls.Move.performed -= UpdatePlayerInput;
            _playerInput.Disable();
        }
    }

    private void UpdatePlayerInput(CallbackContext ctx)
    {
        Debug.Log($"ctx: {ctx.ReadValue<Vector2>()}");
        _moveInput = ctx.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Debug.Log($"move input: { _moveInput }");
        //RotateCharacter(_moveInput);
        _characterController.Move(InputVectorToMoveVector(_moveInput));

        //Animator values
        _characterAnimator.SetFloat("forward_velocity", Mathf.Clamp(_moveInput.sqrMagnitude, 0, 1));
    }

    private void RotateCharacter(Vector2 moveInput)
    {
        float inputZ = moveInput.y;
        float inputX = moveInput.x;
        float cameraAngleDiffWithInput = Vector3.Angle(new Vector2(0, 1), moveInput); //Mathf.Acos(Vector2.Dot(new Vector2(0, 1), inputDir) / inputDir.magnitude) * Mathf.Rad2Deg;
        float targetRotationYDegree = _cameraTransform.eulerAngles.y + (inputX > 0 ? cameraAngleDiffWithInput : -cameraAngleDiffWithInput);
        float targetRotationZDegree = Mathf.Clamp(_characterTransform.eulerAngles.y - targetRotationYDegree, -10f, 10f); //For tilting when sharp turnfloat inputMagnitude = Mathf.Clamp(moveInput.magnitude, 0, 1);
        Quaternion targetRotationQuarternion = Quaternion.Euler(0, _characterTransform.rotation.y + targetRotationYDegree, _characterTransform.rotation.z + targetRotationZDegree);
        _characterTransform.rotation = Quaternion.Lerp(_characterTransform.rotation, targetRotationQuarternion, Time.fixedDeltaTime / RotationSpeed);
    }

    private Vector3 InputVectorToMoveVector(Vector2 moveInput)
    {
        Vector3 velocity = new Vector3(0, 0, Mathf.Clamp(_moveInput.sqrMagnitude, 0, 1));
        velocity = _characterTransform.TransformDirection(velocity);
        return velocity * MoveSpeed * Time.fixedDeltaTime;
    }
}
