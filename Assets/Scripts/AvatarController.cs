using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class AvatarController : MonoBehaviour
{
    public AnimationCurve AnimationCurve; //still not used, test only
    public float MaxMoveSpeedPerSec; // m/sec
    public float RotateAnglePerSec; //fixed secs from current to target

    [SerializeField] private Transform _cameraTransform;
    private CharacterController _characterController;
    private PlayerInput _playerInput;
    private Vector2 _moveInput;
    private Transform _characterTransform;
    private Animator _characterAnimator;
    private float _currentMoveSpeed;
    private float _currentRotateSpeed;
    private float _currentMoveForwardLerpTime = 0;
    private float _currentRotateLerpTime = 0;
    private float _lastFixedUpdateSpeed = 0;

    private bool _isMoving = false;

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
        _isMoving = true;
        //Debug.Log($"ctx: {ctx.ReadValue<Vector2>()}");
        _moveInput = ctx.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        /*
        //Debug.Log($"move input: { _moveInput }");
        if (_moveInput.magnitude < 0.1f || _characterAnimator.GetCurrentAnimatorStateInfo(1).IsName("Stop"))
        {
            #region Stop State Update
            if (_isMoving)
            {
                _isMoving = false;
                if (_lastFixedUpdateSpeed > MaxMoveSpeedPerSec / 2f)
                {
                    _characterAnimator.SetTrigger("sudden_stop_running");
                }
            }

            var fullTime = 0.5f;
            var currentPlayingTime = (_characterAnimator.GetCurrentAnimatorStateInfo(1).normalizedTime / _characterAnimator.GetCurrentAnimatorStateInfo(1).length) * fullTime;
            _characterAnimator.SetFloat("stopping_velocity", fullTime - currentPlayingTime / fullTime);
            _lastFixedUpdateSpeed = 0;
            #endregion
            return;
        }
        else
        {
            _characterAnimator.SetFloat("stopping_velocity", 0.5f);
        }
        */

        #region Character Rotation
        RotateCharacter(_moveInput);
        #endregion
        /*
        #region Character Translation
        var forwardVector = InputVectorToMoveForwardVector();
        float moveForwardLerpTime = 1;
        _currentMoveForwardLerpTime += Time.fixedDeltaTime;
        if (forwardVector.magnitude < 0.1f)
        {
            _currentMoveForwardLerpTime = 0;
        }
        else if (_currentMoveForwardLerpTime > moveForwardLerpTime)
        {
            _currentMoveForwardLerpTime = moveForwardLerpTime;
        }
        //Debug.Log($"_currentLerpTime: {_currentMoveForwardLerpTime}");

        //lerp!
        float perc = _currentMoveForwardLerpTime / moveForwardLerpTime;
        _currentMoveSpeed = Mathf.Lerp(0, MaxMoveSpeedPerSec, perc);
        _lastFixedUpdateSpeed = _currentMoveSpeed;


        //Move
        _characterController.Move(forwardVector * _currentMoveSpeed * Time.fixedDeltaTime);
        #endregion

        #region Set Animator Values
        _characterAnimator.SetFloat("forward_velocity", (forwardVector.magnitude * _currentMoveSpeed) / MaxMoveSpeedPerSec);
        //Debug.Log($"velocity: {forwardVector * _currentMoveSpeed}");
        #endregion
        */
    }

    private void RotateCharacter(Vector2 moveInput)
    {
        if(Mathf.Approximately(moveInput.magnitude, 0f))
        {
            return;
        }

        float inputAngleDiffWithCamera = Vector3.SignedAngle(new Vector3(0, 0, 1), new Vector3(moveInput.x, 0, moveInput.y), Vector3.up); //Mathf.Acos(Vector2.Dot(new Vector2(0, 1), inputDir) / inputDir.magnitude) * Mathf.Rad2Deg;
        Vector3 characterVectorOnXZPlane = Vector3.ProjectOnPlane(_characterTransform.forward, Vector3.up);
        Vector3 cameraVectorOnXZPlane = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up);
        float characterAngleDiffWithCamera = Vector3.SignedAngle(characterVectorOnXZPlane, cameraVectorOnXZPlane, Vector3.up);
        float targetRotationAngle = inputAngleDiffWithCamera + characterAngleDiffWithCamera;

        _characterTransform.Rotate(new Vector3(0, targetRotationAngle, 0));
    }

    private Vector3 InputVectorToMoveForwardVector()
    {
        Vector3 projVector = Vector3.Project(new Vector3(_moveInput.x, 0, _moveInput.y), _characterTransform.forward);
        //Debug.Log($"projVector: {projVector}");
        //Debug.Log($"projVector magnitude: {projVector.magnitude}");


        Vector3 outputVector = new Vector3(0, 0, projVector.magnitude);
        outputVector = _characterTransform.TransformDirection(outputVector);
        return outputVector;
    }
}
