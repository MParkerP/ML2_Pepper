using MagicLeap.OpenXR.Features.SpatialAnchors;
using MagicLeap.OpenXR.Subsystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.NativeTypes;

public class AnchorCreator : MonoBehaviour
{
    [SerializeField] private GameObject anchorPrefab;
    [SerializeField] private Transform controllerTransform;
    [SerializeField] private InputActionProperty bumperInputAction;
    [SerializeField] private InputActionProperty triggerInputAction;
    [SerializeField] private InputActionProperty menuInputAction;
    [SerializeField] private float queryAnchorRadius = 10.0f;

    private MagicLeapSpatialAnchorsStorageFeature storage;

    private struct StoredAnchor
    {
        public ulong AnchorId;
        public string AnchorMapPositionId;
        public ARAnchor AnchorObject;
    }

    public MLXrAnchorSubsystem ActiveSubsystem { get; private set; }
    private List<ARAnchor> localAnchors = new();

    private List<StoredAnchor> storedAnchors = new();

    private GameObject lastCreatedObjectForAnchor;


    private IEnumerator Start()
    {
        yield return new WaitUntil(IsMagicLeapAnchorSubsystemsLoaded);
        bumperInputAction.action.Enable();
        triggerInputAction.action.Enable();
        menuInputAction.action.Enable();

        bumperInputAction.action.canceled += OnBumperActionReleased;
        triggerInputAction.action.performed += OnTriggerActionPerformed;
        menuInputAction.action.performed += OnMenuActionPerformed;

        AttachStorageListeners();

        yield return new WaitForSeconds(5);
        QueryAnchor();
    }

    private void Update()
    {
        if(bumperInputAction.action.IsPressed())
        {
            if(lastCreatedObjectForAnchor == null)
            {
                lastCreatedObjectForAnchor = Instantiate(anchorPrefab, controllerTransform.position, anchorPrefab.transform.rotation);
            }
            else
            {
                lastCreatedObjectForAnchor.transform.SetPositionAndRotation(controllerTransform.position, anchorPrefab.transform.rotation);
            }
        }
    }

    private void OnBumperActionReleased(InputAction.CallbackContext _)
    {
        if (localAnchors.Count == 0 && lastCreatedObjectForAnchor != null)
        {
            CreateAnchor(persist : true);
        }
    }

    private void OnTriggerActionPerformed(InputAction.CallbackContext obj)
    {

        ClearAllAnchors();
        //if (localAnchors.Count > 0)
        //{
        //    var lastAnchor = localAnchors[^1];
        //    Destroy(lastAnchor.gameObject);
        //    localAnchors.RemoveAt(localAnchors.Count - 1);
        //}
    }

    private void OnMenuActionPerformed(InputAction.CallbackContext obj)
    {
        throw new System.NotImplementedException();
    }

    //anchor storage
    private void QueryAnchor()
    {
        if(!storage.QueryStoredSpatialAnchors(controllerTransform.transform.position, queryAnchorRadius))
        {
            //There was a problem querying for anchors
        }
        else
        {
            Debug.Log("Query Successful");
        }
    }

    private void OnQueryCompleted(List<string> anchorMapPositionIDs)
    {
        foreach(var anchorMapPositionId in anchorMapPositionIDs)
        {
            var foundStoredAnchorMatch = storedAnchors.Where(a => a.AnchorMapPositionId == anchorMapPositionId);

            if(!foundStoredAnchorMatch.Any())
            {
                if(!storage.CreateSpatialAnchorsFromStorage(new List<string> { anchorMapPositionId }))
                {
                    //Could not create anchor
                }
            }
        }
    }

    private void OnCompletedCreation(Pose pose, ulong anchorID, string anchorMapPositionId, XrResult result)
    {
        if(result != XrResult.Success)
        {
            //failed
            return;
        }

        var newAnchor = Instantiate(anchorPrefab, pose.position, pose.rotation);
        ARAnchor newAnchorComponent = newAnchor.AddComponent<ARAnchor>();
        newAnchor.AddComponent<AnchorState>();

        StoredAnchor newStoredAnchor = new StoredAnchor
        { 
            AnchorId = anchorID,
            AnchorMapPositionId = anchorMapPositionId,
            AnchorObject = newAnchorComponent
        };

        storedAnchors.Add(newStoredAnchor);
    }

    private IEnumerator PublishAnchor(ARAnchor toPublish)
    {
        while(toPublish.trackingState != TrackingState.Tracking)
        {
            yield return null;
        }
        storage.PublishSpatialAnchorsToStorage(new List<ARAnchor> { toPublish }, 0);

    }

    private void OnPublishCompleted(ulong anchorID, string anchorMapPositionID)
    {
        for(int i = localAnchors.Count - 1; i >=0; i--)
        {
            if (ActiveSubsystem.GetAnchorId(localAnchors[i]) == anchorID)
            {
                StoredAnchor newStoredAnchor = new StoredAnchor
                {
                    AnchorId = anchorID,
                    AnchorMapPositionId = anchorMapPositionID,
                    AnchorObject = localAnchors[i]
                };

                storedAnchors.Add(newStoredAnchor);
                localAnchors.RemoveAt(i);
                break;
            }
        }
    }

    private void OnDeleteCompleted(List<string> anchorMapPositionIDs)
    {
        foreach (var anchorMapPositionID in  anchorMapPositionIDs)
        {
            var storedAnchorIndex = storedAnchors.FindIndex(a => a.AnchorMapPositionId == anchorMapPositionID);
            if (storedAnchorIndex >= 0)
            {
                Destroy(storedAnchors[storedAnchorIndex].AnchorObject.gameObject);
                storedAnchors.RemoveAt(storedAnchorIndex);
            }
        }
    }


    private void CreateAnchor(bool persist = false)
    {
        ARAnchor newAnchor = lastCreatedObjectForAnchor.AddComponent<ARAnchor>();
        lastCreatedObjectForAnchor.AddComponent<AnchorState>();

        localAnchors.Add(newAnchor);
        lastCreatedObjectForAnchor = null;

        if(persist)
        {
            StartCoroutine(PublishAnchor(newAnchor));
        }

        //Pose pose = new Pose(controllerTransform.position, controllerTransform.rotation);
        //var newAnchorObject = Instantiate(anchorPrefab, pose.position, anchorPrefab.transform.rotation);
        //ARAnchor newAnchor = newAnchorObject.AddComponent<ARAnchor>();
        //newAnchorObject.AddComponent <AnchorState>();

        //localAnchors.Add(newAnchor);
    }

    private void ClearAllAnchors()
    {
        //clear local anchors
        if (localAnchors.Count > 0)
        {
            for (int i = localAnchors.Count - 1; i >= 0; i--)
            {
                Destroy(localAnchors[i].gameObject);
                localAnchors.RemoveAt(i);
            }
        }

        //clear stored anchors
        if(storedAnchors.Count > 0)
        {
            for (int i = storedAnchors.Count - 1; i >= 0; i--)
            {
                storage.DeleteStoredSpatialAnchors(new List<string> { storedAnchors[i].AnchorMapPositionId });
            }
        }
    }

    private void AttachStorageListeners()
    {
        storage = OpenXRSettings.Instance.GetFeature<MagicLeapSpatialAnchorsStorageFeature>();

        //Querying storage for list of publish anchors
        storage.OnQueryComplete += OnQueryCompleted;

        //Anchors created from a list of map location IDs from querying storage
        storage.OnCreationCompleteFromStorage += OnCompletedCreation;

        //Publishing local anchor to storage
        storage.OnPublishComplete += OnPublishCompleted;

        //Deleting a puiblished/stored anchor from storage
        storage.OnDeletedComplete += OnDeleteCompleted;
    }
    
    
    private bool IsMagicLeapAnchorSubsystemsLoaded()
    {
        if (XRGeneralSettings.Instance == null || XRGeneralSettings.Instance.Manager == null || XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            return false;
        }
        ActiveSubsystem = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRAnchorSubsystem>()
            as MLXrAnchorSubsystem;

        return ActiveSubsystem != null;
    }
}
