using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class AvatarController : MonoBehaviour
{
    public float RotateAnglePerUpdate;
    public AnimationCurve TranslationPercCurve;
    public float MaxTranslationMeterPerUpdate;
    public float FixedAnimatorUpdateValuePerUpdate;
    public float StoppingMaxDuration;

    [SerializeField] private Transform _cameraTransform;

    private Vector2 _moveInput;
    private PlayerInput _playerInput;
    private CharacterController _characterController;
    private Transform _characterTransform;
    private Animator _characterAnimator;

    private bool _isRunning = false;
    private float _accumTranslationPerc = 0;
    private float _lastTranslationPercFromCurve = 0;
    private float _lastUpdateTranslationAmount = 0;
    private float _currentStoppingDuration = 0;
    private Vector3 _characterForwardVectorOnXZPlane;
    private Vector3 _cameraVectorOnXZPlane;

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
        _moveInput = ctx.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        OnScreenDebugText.Instance.Log(OnScreenDebugText.OnScreenTextEnum.MoveInput, _moveInput);
        OnScreenDebugText.Instance.Log(OnScreenDebugText.OnScreenTextEnum.MoveInputMagnitude, _moveInput.magnitude);
        OnScreenDebugText.Instance.Log(OnScreenDebugText.OnScreenTextEnum.IsRunning, _isRunning);
        if (_isRunning && Mathf.Approximately(_moveInput.magnitude, 0)) //Sudden Stop
        {
            if(Mathf.Approximately(_lastUpdateTranslationAmount, MaxTranslationMeterPerUpdate))
            {
                _isRunning = false;
                _currentStoppingDuration = StoppingMaxDuration;
                _characterAnimator.SetTrigger("stop_running");
            }
        }

        _characterForwardVectorOnXZPlane = Vector3.ProjectOnPlane(_characterTransform.forward, Vector3.up);
        OnScreenDebugText.Instance.Log(OnScreenDebugText.OnScreenTextEnum.CharacterForwardVectorOnXZPlane, _characterForwardVectorOnXZPlane);
        _cameraVectorOnXZPlane = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up);
        OnScreenDebugText.Instance.Log(OnScreenDebugText.OnScreenTextEnum.CameraVectorOnXZPlane, _cameraVectorOnXZPlane);
        RotateCharacter();
        TranslateCharacter();
        _characterAnimator.SetFloat("forward_velocity", _lastTranslationPercFromCurve);
    }

    private void RotateCharacter()
    {
        if (!_isRunning || Mathf.Approximately(_moveInput.magnitude, 0))
        {
            return;
        }

        float inputAngleDiffWithCamera = Vector3.SignedAngle(Vector3.forward, new Vector3(_moveInput.x, 0, _moveInput.y), Vector3.up);
        float characterAngleDiffWithCamera = Vector3.SignedAngle(_characterForwardVectorOnXZPlane, _cameraVectorOnXZPlane, Vector3.up);
        float targetRotationAngle = inputAngleDiffWithCamera + characterAngleDiffWithCamera;
        if (targetRotationAngle > 180)
        {
            targetRotationAngle = targetRotationAngle - 360;
        }
        else if (targetRotationAngle < -180)
        {
            targetRotationAngle = targetRotationAngle + 360;
        }
        OnScreenDebugText.Instance.Log(OnScreenDebugText.OnScreenTextEnum.TargetRotationAngle, targetRotationAngle);

        float fixedrotateAngle = RotateAnglePerUpdate;
        if (Mathf.Abs(fixedrotateAngle) > Mathf.Abs(targetRotationAngle))
        {
            fixedrotateAngle = targetRotationAngle;
        }
        else if (targetRotationAngle < 0)
        {
            fixedrotateAngle *= -1;
        }

        OnScreenDebugText.Instance.Log(OnScreenDebugText.OnScreenTextEnum.FixedRotateAmount, fixedrotateAngle);
        _characterTransform.Rotate(new Vector3(0, fixedrotateAngle, 0));
    }

    private void TranslateCharacter()
    {
        float maxVeloPerc = _moveInput.magnitude;
        OnScreenDebugText.Instance.Log(OnScreenDebugText.OnScreenTextEnum.MaxVeloPerc, maxVeloPerc);
        if (maxVeloPerc > 0 && _currentStoppingDuration == 0)
        {
            _isRunning = true;
            _accumTranslationPerc += Time.fixedDeltaTime;
            _accumTranslationPerc = Mathf.Min(_accumTranslationPerc, maxVeloPerc);
        }
        else
        {
            _isRunning = false;
            _currentStoppingDuration -= _currentStoppingDuration < 0 ? _currentStoppingDuration : Time.fixedDeltaTime;
            _accumTranslationPerc -= Time.fixedDeltaTime * 1.5f;
            _accumTranslationPerc = Mathf.Max(_accumTranslationPerc, 0);
        }

        OnScreenDebugText.Instance.Log(OnScreenDebugText.OnScreenTextEnum.AccumTranslationPerc, _accumTranslationPerc);
        _lastTranslationPercFromCurve = TranslationPercCurve.Evaluate(_accumTranslationPerc);
        OnScreenDebugText.Instance.Log(OnScreenDebugText.OnScreenTextEnum.TranslationPercFromCurve, _lastTranslationPercFromCurve);
        _lastUpdateTranslationAmount = MaxTranslationMeterPerUpdate * _lastTranslationPercFromCurve;
        _characterController.Move(_characterForwardVectorOnXZPlane.normalized * _lastUpdateTranslationAmount);
    }
    
}
