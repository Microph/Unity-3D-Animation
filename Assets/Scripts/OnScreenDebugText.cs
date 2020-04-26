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
        DontDestroyOnLoad(this);
    }
    #endregion

    public TMP_Text[] TextObjList;
    public enum OnScreenTextEnum
    {
        MoveInput
        , TargetRotationAngle
        , FixedRotateAmount
    }

    public void Log(OnScreenTextEnum enumType, object toBeLog)
    {
        TextObjList[enumType.GetHashCode()].text = $"{ enumType }: { toBeLog.ToString() }";
    }
}