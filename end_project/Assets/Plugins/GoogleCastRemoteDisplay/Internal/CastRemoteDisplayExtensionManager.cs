/*
 * Copyright 2015 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Google.Cast.RemoteDisplay.Internal {
  public class CastRemoteDisplayExtensionManager : MonoBehaviour {

    // We need this in order to tell the CastRemoteDisplayManager to fire events for us.
    public delegate void CastEventHandler();

    public CastRemoteDisplayManager CastRemoteDisplayManager {
      get {
        return CastRemoteDisplayManager.GetInstance();
      }
    }

    private CastEventHandler onCastDevicesUpdatedCallback;
    private CastEventHandler onRemoteDisplaySessionStartCallback;
    private CastEventHandler onRemoteDisplaySessionEndCallback;
    private CastEventHandler onErrorCallback;

    private bool isApplicationPaused = false;
    private bool isCasting = false;

    private List<CastDevice> castDevices = new List<CastDevice>();
    private CastDevice connectedCastDevice = null;
    private CastError lastError = null;
    private ICastRemoteDisplayExtension castRemoteDisplayExtension = null;

    // When Unity specifies a camera but no render texture, we must generate one and assign it.
    // When casting is complete, we must reclaim it - this bool tracks that possibility.
    private bool renderTextureGenerated = false;

    // True if remote display texture was updated by #UpdateRemoteDisplayTexture should be rendered
    // if possible.
    private bool remoteDisplayTextureUpdated = false;

    /**
     * Sets the event handlers that should be invoked by this class.
     */
    public void SetEventHandlers(CastEventHandler onCastDevicesUpdatedCallback,
        CastEventHandler onRemoteDisplaySessionStartCallback,
        CastEventHandler onRemoteDisplaySessionEndCallback,
        CastEventHandler onErrorCallback) {
      this.onCastDevicesUpdatedCallback = onCastDevicesUpdatedCallback;
      this.onRemoteDisplaySessionStartCallback = onRemoteDisplaySessionStartCallback;
      this.onRemoteDisplaySessionEndCallback = onRemoteDisplaySessionEndCallback;
      this.onErrorCallback = onErrorCallback;
    }

    /**
     * Unity callback pre-scene initialization.
     */
    void Awake() {
      // Do not initialize or perform checks yet, but if there is a remote display camera disable
      // it so that it doesn't become the main camera.
      if (CastRemoteDisplayManager != null &&
          CastRemoteDisplayManager.RemoteDisplayCamera != null) {
        CastRemoteDisplayManager.RemoteDisplayCamera.enabled = false;
      }
    }

    /**
     * Unity callback for when this game object is enabled. Called after Awake and before Start on
     * scene load.
     */
    void OnEnable() {
      Activate();
    }

    /**
     * Unity callback for when this object is disabled.
     */
    void OnDisable() {
      Deactivate();
    }

    /**
     * Unity callback for when the app gets paused/backgrounded, and resumed/foregrounded.
     */
    void OnApplicationPause(bool paused) {
      isApplicationPaused = paused;
      DiscardGeneratedTextureIfNeeded();
      UpdateRemoteDisplayTexture();
    }

    /**
     * Matches the Unity callback of the same name, but this is actually called by the
     * CastRemoteDisplayAudioListener. This is due to the fact that this callback should come from
     * the game object with the AudioListener script.
     * Note: On iOS, this callback won't trigger unless the AudioListener was attached to the game
     * object since scene load, that is why we need a custom script.
     */
    public void OnAudioFilterRead(float[] data, int channels) {
      if (castRemoteDisplayExtension != null) {
        castRemoteDisplayExtension.OnAudioFilterRead(data, channels);
      }
    }

    /**
     * Returns the list of cast devices.
     */
    public List<CastDevice> GetCastDevices() {
      return castDevices;
    }

    /**
     * Returns the last error encountered by the Cast Remote Display Plugin, or null if no error
     * has occurred.
     */
    public CastError GetLastError() {
      return lastError;
    }

    /**
     * Starts a remote display session on the cast device with the specified device ID.
     */
    public void SelectCastDevice(string deviceId) {
      if (castRemoteDisplayExtension != null) {
        castRemoteDisplayExtension.SelectCastDevice(deviceId);
        CacheSelectedDevice(deviceId);
      }
    }

    /**
     * Caches the device that is being connected to, so we can track the device name and ID.
     */
    private void CacheSelectedDevice(string deviceId) {
      foreach (CastDevice listDevice in castDevices) {
        if (listDevice.DeviceId == deviceId) {
          connectedCastDevice = listDevice;
          return;
        }
      }
    }

    /**
     * Returns the CastDevice of the receiver device this sender is currently casting to.
     */
    public CastDevice GetSelectedCastDevice() {
      if (connectedCastDevice != null) {
        return connectedCastDevice;
      }
      return null;
    }

    /**
     * Returns whether there is an active cast session.
     */
    public bool IsCasting() {
      return isCasting;
    }

    /**
     * Stops the current remote display session. This can be used in the middle of the game to let
     * the user stop and disconnect and later select another Cast device.
     */
    public void StopRemoteDisplaySession() {
      DiscardGeneratedTextureIfNeeded();
      connectedCastDevice = null;
      isCasting = false;
      if (castRemoteDisplayExtension != null) {
        castRemoteDisplayExtension.StopRemoteDisplaySession();
      }
    }

    /**
     * Notifies the static library that a new texture id should be used for remote display
     * rendering. It will select the #RemoteDisplayTexture property of the
     * #CastRemoteDisplayManager, if not set it will try to use the #RemoteDisplayCamera. If the
     * application is paused and the #RemoteDisplayPausedTexture is set, it will be selected.
     */
    public void UpdateRemoteDisplayTexture() {
      if (castRemoteDisplayExtension == null) {
        Debug.Log("Remote display is not activated. Can't update remote display texture or " +
            "camera.");
        return;
      }

      Texture texture = null;
      CastRemoteDisplayManager manager = CastRemoteDisplayManager;
      renderTextureGenerated = false;

      if (isApplicationPaused && manager.RemoteDisplayPausedTexture != null) {
        // Application paused and we have a paused texture to display.
        texture = manager.RemoteDisplayPausedTexture;
      } else {
        if (manager.RemoteDisplayTexture != null) {
          // The user explicitly set a texture to use.
          texture = manager.RemoteDisplayTexture;
        } else if (manager.RemoteDisplayCamera != null) {
          // We are using a camera. If it has a target render texture set, use that.
          manager.RemoteDisplayCamera.enabled = true;
          manager.RemoteDisplayCamera.gameObject.SetActive(true);
          if (manager.RemoteDisplayCamera.targetTexture == null) {
            // Otherwise create our own.
            RenderTexture renderTexture = new RenderTexture(
                (int) manager.Configuration.ResolutionDimensions.x,
                (int) manager.Configuration.ResolutionDimensions.y,
                24, RenderTextureFormat.ARGB32);
            manager.RemoteDisplayCamera.targetTexture = renderTexture;
            renderTextureGenerated = true;
          }
          if (!manager.RemoteDisplayCamera.targetTexture.IsCreated()) {
            manager.RemoteDisplayCamera.targetTexture.Create();
          }
          texture = manager.RemoteDisplayCamera.targetTexture;
        }
      }

      if (texture == null) {
        Debug.LogError("No texture or camera set. Can't cast to remote display.");
        return;
      }
      castRemoteDisplayExtension.SetRemoteDisplayTexture(texture);
      remoteDisplayTextureUpdated = true;
    }

    /**
     * Updates the current remote audio listener. We need to attach a component to the game object
     * marked as the remote audio listener. It is possible to swap audio listeners at runtime.
     * We disable the attached component on the object that is no longer active. Either parameter
     * can be null.
     */
    public void UpdateAudioListener(AudioListener previouslistener, AudioListener newlistener) {
      CastRemoteDisplayAudioInterceptor previousInterceptor =
          previouslistener == null ? null :
          previouslistener.GetComponent<CastRemoteDisplayAudioInterceptor>();
      CastRemoteDisplayAudioInterceptor newInterceptor =
          newlistener == null ? null :
          newlistener.GetComponent<CastRemoteDisplayAudioInterceptor>();

      // If we have a valid listener that doesn't have an interceptor yet, add it.
      if (newInterceptor == null && newlistener != null) {
        newInterceptor = newlistener.gameObject.AddComponent<CastRemoteDisplayAudioInterceptor>();
      }

      if (previousInterceptor != null && previousInterceptor != newInterceptor) {
        previousInterceptor.enabled = false;
      }

      if (newInterceptor != null) {
        newInterceptor.SetCastRemoteDisplayExtensionManager(this);
        newInterceptor.enabled = true;
      }
    }


    /**
     * Sets everything up and starts discovery on the native extension.
     */
    private void Activate() {
      Debug.Log("Activating Cast Remote Display.");
      if (CastRemoteDisplayManager == null) {
        Debug.LogError("FATAL: CastRemoteDisplayManager script not found.");
        return;
      }

      if (CastRemoteDisplayManager.CastAppId == null ||
          CastRemoteDisplayManager.CastAppId.Equals("")) {
        Debug.LogError("FATAL: CastRemoteDisplayManager needs a CastAppId");
        return;
      }

      if (CastRemoteDisplayManager.Configuration == null) {
        Debug.LogError("FATAL: CastRemoteDisplayManager has null configuration");
        return;
      }

#if UNITY_ANDROID && !UNITY_EDITOR
    castRemoteDisplayExtension = new CastRemoteDisplayAndroidExtension(this);
#elif UNITY_IOS && !UNITY_EDITOR
    castRemoteDisplayExtension = new CastRemoteDisplayiOSExtension(this);
#elif UNITY_EDITOR
      CastRemoteDisplaySimulator displaySimulator =
        UnityEngine.Object.FindObjectOfType<CastRemoteDisplaySimulator>();
      if (displaySimulator) {
        castRemoteDisplayExtension = new CastRemoteDisplayUnityExtension(this, displaySimulator);
      }
#endif

      if (castRemoteDisplayExtension != null) {
        castRemoteDisplayExtension.Activate();
#if !UNITY_IOS && !UNITY_EDITOR
        // Non iOS platforms can render using RenderRemoteDisplayCoroutine.
        StartCoroutine(RenderRemoteDisplayCoroutine());
#endif
        // Update the list of cast devices in case we missed updates from the native lib while this
        // object was deactivated.
        UpdateCastDevicesFromNativeCode();
      } else {
        Debug.LogWarning("Disabling the CastRemoteDisplayManager because the platform is not " +
                       "Android or iOS, and no simulator is found.");
      }
    }

    /**
     * Terminates the current remote display session (if any) and stops discovery on the native
     * extension.
     */
    private void Deactivate() {
      Debug.Log("Deactivating Cast Remote Display.");
      connectedCastDevice = null;
      isCasting = false;
      StopRemoteDisplaySession();
      DiscardGeneratedTextureIfNeeded();

      if (castRemoteDisplayExtension != null) {
        castRemoteDisplayExtension.Deactivate();
      }
      castRemoteDisplayExtension = null;
      StopAllCoroutines();
      castDevices.Clear();
    }

    /**
     * Callback called from the native plugins when the remote display session starts.
     */
    public void _callback_OnRemoteDisplaySessionStart(string deviceId) {
      if (castRemoteDisplayExtension != null) {
        Debug.Log("Remote display session started.");
        castRemoteDisplayExtension.OnRemoteDisplaySessionStart(deviceId);
      }

      UpdateRemoteDisplayTexture();
      isCasting = true;
      onRemoteDisplaySessionStartCallback();
    }

    /**
     * Callback called from the native plugins when the remote display session ends.
     */
    public void _callback_OnRemoteDisplaySessionEnd(string dummy) {
      if (castRemoteDisplayExtension != null) {
        Debug.Log("Remote display session stopped.");
        castRemoteDisplayExtension.OnRemoteDisplaySessionStop();
      }
      isCasting = false;
      onRemoteDisplaySessionEndCallback();
    }

    /**
     * Callback called from the native plugins when the list of available cast devices has changed.
     */
    public void _callback_OnCastDevicesUpdated(string dummy) {
      UpdateCastDevicesFromNativeCode();

      onCastDevicesUpdatedCallback();
    }

    /**
     * Callback called from the native plugins when the SDK throws an error.
     */
    public void _callback_OnCastError(string rawErrorString) {
      string errorString = extractErrorString(rawErrorString);
      CastErrorCode errorCode = extractErrorCode(rawErrorString);
      lastError = new CastError(errorCode, errorString);
      isCasting = false;
      onErrorCallback();
    }

    /**
     * Pulls the error code out of the raw error string that gets sent from the SDK.
     */
    private CastErrorCode extractErrorCode(string rawErrorString) {
      string[] splitString = rawErrorString.Split(':');
      int errorInt = -1;
      bool foundCode = Int32.TryParse(splitString[0], out errorInt);
      if (splitString.Length < 2 || !foundCode) {
        Debug.LogError("Error string malformed: " + rawErrorString);
        return CastErrorCode.ErrorCodeMalformed;
      } else {
        return (CastErrorCode) errorInt;
      }
    }

    /**
     * Pulls the error string out of the raw error string that gets sent from the SDK.
     */
    private string extractErrorString(string rawErrorString) {
      string[] splitString = rawErrorString.Split(':');
      if (splitString.Length < 2) {
        Debug.LogError("Error string malformed: " + rawErrorString);
        return "Error string malformed: " + rawErrorString;
      } else {
        return splitString[1];
      }
    }

    private void UpdateCastDevicesFromNativeCode() {
      castDevices.Clear();
      if (castRemoteDisplayExtension != null) {
        castDevices = castRemoteDisplayExtension.GetCastDevices(ref this.connectedCastDevice);
      } else {
        Debug.Log("Can't update the list of cast devices because there is no extension object.");
      }
    }

    private void DiscardGeneratedTextureIfNeeded() {
      var manager = CastRemoteDisplayManager;
      if (manager.RemoteDisplayCamera != null) {
        manager.RemoteDisplayCamera.enabled = false;
        if (renderTextureGenerated) {
          if (manager.RemoteDisplayCamera.targetTexture != null) {
            manager.RemoteDisplayCamera.targetTexture.DiscardContents();
            manager.RemoteDisplayCamera.targetTexture = null;
          }
          renderTextureGenerated = false;
        }
      }
    }

    private void MaybeRenderFrame() {
      if (castRemoteDisplayExtension != null
          && (!isApplicationPaused || remoteDisplayTextureUpdated) && isCasting) {
        castRemoteDisplayExtension.RenderFrame();
        remoteDisplayTextureUpdated = false;
      }
    }


#if UNITY_EDITOR
    /**
     * The Unity simulator must render in OnGUI in order to output the simulated output to the
     *  screen.
     */
    public void OnGUI() {
      MaybeRenderFrame();
    }

#elif UNITY_IOS
    /**
     * iOS Metal graphics API will only render in Update().
     */
    private void Update() {
      MaybeRenderFrame();
    }

#else
    /**
     * Coroutine that will call render on the native plugin at the end of every frame.
     */
    private IEnumerator RenderRemoteDisplayCoroutine() {
      var waitObject = new WaitForEndOfFrame();
      while (gameObject.activeInHierarchy && enabled) {
        yield return waitObject;
        MaybeRenderFrame();
      }
      yield return 0;
    }
#endif
  }

  /**
   * Target delays supported by a remote display session. See CastRemoteDisplayConfiguration for
   * details about using target delay.
   */
  public enum CastRemoteDisplayTargetDelay {
    /**
     * Specifies the minimum target delay for the session. Remote display will have the least time
     * to encode, transmit, retransmit, decode, and render on the TV.
     */
    Minimum = 1,

    /**
     * Specifies a low target delay for the session. Remote display will have less time to encode,
     * transmit, retransmit, decode, and render on the TV.
     */
    Low = 2,

    /**
     * Specifies a normal target delay for the session. Remote display is optimized for this delay.
     */
    Normal = 3,

    /**
     * Specifies a high target delay for the session. Remote display will have more time to encode,
     * transmit, retransmit, decode, and render on the TV. This is useful for smoother rendering on
     * the TV at the cost of higher latency and more memory to buffer and encode.
     */
    High = 4
  };
}
