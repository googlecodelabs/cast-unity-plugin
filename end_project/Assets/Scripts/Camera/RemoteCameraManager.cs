using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Google.Cast.RemoteDisplay;

public class RemoteCameraManager : MonoBehaviour {
  public CastRemoteDisplayManager displayManager;
  public Camera RemoteDisplayCamera;
  public Camera MainCamera;

  void Start() {
    if (!displayManager) {
      displayManager = CastRemoteDisplayManager.GetInstance();
    }

    if (!displayManager) {
      Debug.LogError("No CastRemoteDisplayManager found!");
      Destroy(gameObject);
      return;
    }
 
    displayManager.RemoteDisplaySessionStartEvent
        .AddListener(OnRemoteDisplaySessionStart);
   
    displayManager.RemoteDisplaySessionEndEvent
        .AddListener(OnRemoteDisplaySessionEnd);
    displayManager.RemoteDisplayErrorEvent
        .AddListener(OnRemoteDisplayError);
    if (displayManager.GetSelectedCastDevice() != null) {
      RemoteDisplayCamera.enabled = true;
      displayManager.RemoteDisplayCamera = MainCamera;
    }

    MainCamera.enabled = true;
  }

  private void OnDestroy() {
    displayManager.RemoteDisplaySessionStartEvent
        .RemoveListener(OnRemoteDisplaySessionStart);
    displayManager.RemoteDisplaySessionEndEvent
        .RemoveListener(OnRemoteDisplaySessionEnd);
    displayManager.RemoteDisplayErrorEvent
        .RemoveListener(OnRemoteDisplayError);
  }

  public void OnRemoteDisplaySessionStart(
      CastRemoteDisplayManager manager) {
    displayManager.RemoteDisplayCamera = MainCamera;
    RemoteDisplayCamera.enabled = true;
  }

  public void OnRemoteDisplaySessionEnd(CastRemoteDisplayManager manager) {
    displayManager.RemoteDisplayCamera = null;
    RemoteDisplayCamera.enabled = false;
    MainCamera.enabled = true;
  }

  public void OnRemoteDisplayError(CastRemoteDisplayManager manager) {
    RemoteDisplayCamera.enabled = false;
    MainCamera.enabled = true;
  }
}