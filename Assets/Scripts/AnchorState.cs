using MagicLeap.OpenXR.Subsystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;

public class AnchorState : MonoBehaviour
{
    private ARAnchor anchor;
    private MLXrAnchorSubsystem activeSubsystem;

    private void Start()
    {
        anchor = GetComponent<ARAnchor>();
        activeSubsystem = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRAnchorSubsystem>() as MLXrAnchorSubsystem;
    }

    private void Update()
    {
        if (activeSubsystem != null)
        {
            ulong magicLeapAnchorId = activeSubsystem.GetAnchorId(anchor);
            MLXrAnchorSubsystem.AnchorConfidence confidence = activeSubsystem.GetAnchorConfidence(anchor);
        }
    }
}
