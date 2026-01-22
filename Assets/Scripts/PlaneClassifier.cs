using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[Serializable]
public class ClassifierMaterialInfo
{
    public PlaneClassification classification;

    public Material material;
}

public class PlaneClassifier : MonoBehaviour
{
    [SerializeField] private ClassifierMaterialInfo[] planeMaterials;

    private void Start()
    {
        ColorClassify();
    }

    private void ColorClassify()
    {
        var plane = GetComponent<ARPlane>();

        var classifierInfo = planeMaterials.FirstOrDefault(m => m.classification
                                                                == plane.classification);
        var meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = classifierInfo?.material ?? meshRenderer.material;
        LearnXR.Core.Logger.Instance.LogInfo($"({plane.transform.name} classified as {plane.classification}");
    }
}
