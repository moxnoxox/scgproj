using UnityEngine;
using UnityEngine.UI;

public class partsUI : MonoBehaviour
{
    public static partsUI instance;
    public Image caseui;
    public Image peakui;
    public Image stringui;
    public bool isCase;
    public bool isPeak;
    public bool isString;

    void Awake() {
        instance = this;
        isCase = false;
        isPeak = false;
        isString = false;
    }
    void Start() {
        caseui.enabled = false;
        peakui.enabled = false;
        stringui.enabled = false;
    }

    void Update() {
        if(isCase) {
            caseui.enabled = true;
        }
        if(isPeak) {
            peakui.enabled = true;
        }
        if(isString) {
            stringui.enabled = true;
        }
    }

    public void OnCaseUIEnable() {
        isCase = true;
    }
    public void OnPeakUIEnable() {
        isPeak = true;
    }
    public void OnStringUIEnable() {
        isString = true;
    }
}
