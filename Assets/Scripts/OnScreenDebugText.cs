using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OnScreenDebugText : MonoBehaviour
{
    #region Singleton
    public static OnScreenDebugText Instance
    {
        get
        {
            return _instance;
        }
    }

    private static OnScreenDebugText _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        Setup();
    }
    #endregion
    
    public enum OnScreenTextEnum
    {
        MoveInput,
        MoveInputMagnitude,
        IsRunning,
        TargetRotationAngle,
        FixedRotateAmount,
        MaxVeloPerc,
        AccumTranslationPerc,
        TranslationPercFromCurve,
        CharacterForwardVectorOnXZPlane,
        CameraVectorOnXZPlane,
        InputVectorProjectedOnCharacterForwardVectorOnXZPlane,
        MaxPercMagnitude,
    }

    private TMP_Text[] _textObjList;

    public void Log(OnScreenTextEnum enumType, object toBeLog)
    {
        _textObjList[enumType.GetHashCode()].text = $"{ enumType }: { toBeLog.ToString() }";
    }

    private void Setup()
    {
        _textObjList = transform.GetComponentsInChildren<TMP_Text>();
    }
}