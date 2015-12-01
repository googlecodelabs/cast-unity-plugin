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

using Google.Cast.RemoteDisplay.Internal;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Google.Cast.RemoteDisplay {
  /**
   * Entry point to the Google Cast Remote Display API for Unity. Add only one of these to your
   * scene as a top level game object.
   */
  public class CastRemoteDisplayManager : MonoBehaviour {

    /**
     * Fired when the list of available cast devices has been updated. Call GetCastDevices() on the
     * CastRemoteDisplayManager to get the actual list.
     */
    public CastRemoteDisplayEvent CastDevicesUpdatedEvent;

    /**
     * Fired when the remote display session starts. Call GetSelectedDevice() on the
     * CastRemoteDisplayManager to get the name of the selected cast device.
     */
    public CastRemoteDisplayEvent RemoteDisplaySessionStartEvent;

    /**
     * Fired when the remote display session ends.
     */
    public CastRemoteDisplayEvent RemoteDisplaySessionEndEvent;

    /**
     * Fired when the remote display session encounters an error. When this event is fired, the
     * game object that owns this component will be disabled. Call GetLastError() to get information
     * about the error that was fired.
     */
    public CastRemoteDisplayEvent RemoteDisplayErrorEvent;

    /**
     * There should only be one DisplayManager in the scene at a time, this instance enforces that.
     */
    public static CastRemoteDisplayManager GetInstance() {
      return instance;
    }
    private static CastRemoteDisplayManager instance = null;

    /**
     * Used to render graphics on the remote display. Only used if RemoteDisplayTexture is not
     * set.
     */
    public Camera RemoteDisplayCamera {
      get {
        return remoteDisplayCamera;
      }
      set {
        // Needed because the unity editor can call this multiple times.
        if (value == remoteDisplayCamera) {
          return;
        }
        remoteDisplayCamera = value;
        if (extensionManager != null) {
          // Must be called after the new value has been set.
          extensionManager.UpdateRemoteDisplayTexture();
        }
      }
    }
    [SerializeField]
    private Camera remoteDisplayCamera;

    /**
     * Used to render graphics on the remote display. If this is set, RemoteDisplayCamera will not
     * be used.
     */
    public Texture RemoteDisplayTexture {
      get {
        return remoteDisplayTexture;
      }
      set {
        // Needed because the unity editor can call this multiple times.
        if (value == remoteDisplayTexture) {
          return;
        }
        remoteDisplayTexture = value;
        if (extensionManager != null) {
          // Must be called after the new value has been set.
          extensionManager.UpdateRemoteDisplayTexture();
        }
      }
    }
    [SerializeField]
    private Texture remoteDisplayTexture;

    /**
     * Used to render graphics on the remote display when the application is paused or
     * backgrounded.
     */
    public Texture RemoteDisplayPausedTexture {
      get {
        return remoteDisplayPausedTexture;
      }
      set {
        remoteDisplayPausedTexture = value;
      }
    }
    [SerializeField]
    private Texture remoteDisplayPausedTexture;

    /**
     * Used to play audio on the remote display.
     */
    public AudioListener RemoteAudioListener {
      get {
        return remoteAudioListener;
      }
      set {
        if (extensionManager) {
          extensionManager.UpdateAudioListener(remoteAudioListener, value);
        }
        remoteAudioListener = value;
      }
    }
    [SerializeField]
    private AudioListener remoteAudioListener;

    /**
     * The remote display application ID.
     */
    public string CastAppId {
      get {
        return castAppId;
      }
      set {
        castAppId = value;
      }
    }
    [SerializeField]
    private string castAppId = "";

    /**
     * The configuration used to set up a remote display session when a cast device is selected.
     * See SelectCastDevice().
     */
    public CastRemoteDisplayConfiguration Configuration {
      get {
        return configuration;
      }
      set {
        if (value == null) {
          throw new System.ArgumentException("Configuration cannot be set to null");
        }
        configuration = value;
      }
    }
    [SerializeField]
    private CastRemoteDisplayConfiguration configuration = new CastRemoteDisplayConfiguration();

    private CastRemoteDisplayExtensionManager extensionManager;

    private void Awake() {
      if (instance && instance != this) {
        Debug.LogWarning("Second CastRemoteDisplayManager detected - destroying. Please make " +
          "sure the appropriate configuration gets migrated to the singleton DisplayManager if " +
          "this is intended.");
        DestroyImmediate(gameObject);
        return;
      } else {
        instance = this;
        DontDestroyOnLoad(gameObject);
      }

      extensionManager = gameObject.AddComponent<CastRemoteDisplayExtensionManager>();
      // Notify the extension manager that a new audio listener was added so that it can attach the
      // required script to it.
      extensionManager.UpdateAudioListener(null, remoteAudioListener);
      extensionManager.SetEventHandlers(fireCastDevicesUpdatedEvent,
          fireRemoteDisplaySessionStartEvent, fireRemoteDisplaySessionEndEvent,
          fireErrorEvent);
    }

    /**
     * Selects a cast device for remote display.
     */
    public void SelectCastDevice(string deviceId) {
      extensionManager.SelectCastDevice(deviceId);
    }

    /**
     * Returns the list of available cast devices for remote display.
     */
    public IList<CastDevice> GetCastDevices() {
      return extensionManager.GetCastDevices().AsReadOnly();
    }

    /**
     * Returns the CastDevice selected for remote display.
     */
    public CastDevice GetSelectedCastDevice() {
      return extensionManager.GetSelectedCastDevice();
    }

    /**
     * Returns whether there is an active cast session. This will be set to true from the moment
     * the RemoteDisplaySessionStartEvent() fires and until the session ends.
     */
    public bool IsCasting() {
      return extensionManager.IsCasting();
    }

    /**
     * Stops the current remote display session. This can be used in the middle of the game to let
     * the user stop and disconnect and later select another Cast device.
     */
    public void StopRemoteDisplaySession() {
      extensionManager.StopRemoteDisplaySession();
    }

    /**
     * Returns the last error encountered by the Cast Remote Display Plugin, or null if no error
     * has occurred.
     */
    public CastError GetLastError() {
      return extensionManager.GetLastError();
    }

    /**
     * Used to allow internal classes to fire events published by this class.
     */
    private void fireCastDevicesUpdatedEvent() {
      if (CastDevicesUpdatedEvent != null) {
        CastDevicesUpdatedEvent.Invoke(this);
      }
    }

    /**
     * Used to allow internal classes to fire events published by this class.
     */
    private void fireRemoteDisplaySessionStartEvent() {
      if (RemoteDisplaySessionStartEvent != null) {
        RemoteDisplaySessionStartEvent.Invoke(this);
      }
    }

    /**
     * Used to allow internal classes to fire events published by this class.
     */
    private void fireRemoteDisplaySessionEndEvent() {
      if (RemoteDisplaySessionEndEvent != null) {
        RemoteDisplaySessionEndEvent.Invoke(this);
      }
    }

    /**
     * Used to allow internal classes to fire events published by this class.
     */
    private void fireErrorEvent() {
      CastError error = GetLastError();
      if (error == null) {
        Debug.LogError("Got an error callback but no error was found");
        return;
      }
      Debug.LogError("Remote display error. ErrorCode: " + error.ErrorCode +
          " errorMessage: " + error.Message);

      // Always disable the manager in case of an error.
      gameObject.SetActive(false);

      if (RemoteDisplayErrorEvent != null) {
        RemoteDisplayErrorEvent.Invoke(this);
      }
    }
  }

  /**
   * Error codes for Cast Remote Display.
   */
  public enum CastErrorCode {
    /**
     * The default error code when there are no errors.
     */
    NoError = 0,

    /**
     * Thrown when the device OS is unsupported. Remote Display requires a minimum of iOS 8 or
     * Android KitKat. (4.4)
     */
    RemoteDisplayUnsupported = 1,

    /**
     * Thrown when the version of Google Play Services needs to be updated.
     */
    GooglePlayServicesUpdateRequired = 2,

    /**
     * Thrown when the Remote Display configuration was rejected by the cast device.
     */
    RemoteDisplayConfigurationRejected = 3,

    /**
     * Thrown when the error message is malformed. Indicates an internal Cast error.
     */
    ErrorCodeMalformed = 1000,
  };

  /**
   * Frame rates expected by the remote display session from the game. The video
   * encoder on the device and TV will expect the game to provide frames at this
   * frame rate. If the game exceeds this frame rate, then it is possible for
   * stutters and slowdowns to occur.
   */
  public enum CastRemoteDisplayFrameRate {
    /**
     * Specifies 15 frames per second coming from the game.
     */
    Fps15 = 15,

    /**
     * Specifies 24 frames per second coming from the game.
     */
    Fps24 = 24,

    /**
     * Specifies 25 frames per second coming from the game.
     */
    Fps25 = 25,

    /**
     * Specifies 30 frames per second coming from the game.
     */
    Fps30 = 30,

    /**
     * Specifies 60 frames per second coming from the game.
     */
    Fps60 = 60,
  };

  /**
   * TV resolutions supported by a remote display session.
   */
  public enum CastRemoteDisplayResolution {
    /**
     * Specifies 848x480 for the session.
     */
    Resolution480p = 480,

    /**
     * Specifies 1280x720 for the session.
     */
    Resolution720p = 720,

    /**
     * Specifies 1920x1080 for the session.
     */
    Resolution1080p = 1080,
  };

  /**
   * Represents a cast device.
   */
  [System.Serializable]
  public class CastDevice {
    /**
     * The ID of the device. This value must be passed when selecting a cast device to start the
     * remote display session.
     */
    public string DeviceId {
      get {
        return deviceId;
      }
    }
    [SerializeField]
    private string deviceId;

    /**
     * Name of the device. This should be used when populating a list of devices in the UI.
     */
    public string DeviceName {
      get {
        return deviceName;
      }
    }
    [SerializeField]
    private string deviceName;

    /**
     * The current status of the device.
     */
    public string Status {
      get {
        return status;
      }
    }
    [SerializeField]
    private string status;

    private CastDevice() {}

    /**
     * Constructor for CastDevice.
     */
    public CastDevice(string deviceId, string deviceName, string status) {
      this.deviceId = deviceId;
      this.deviceName = deviceName;
      this.status = status;
    }
  }

  /**
   * Represents an error that triggered by the Cast Remote Display Plugin.
   */
  [System.Serializable]
  public class CastError {
    /**
     * The error code.
     */
    public CastErrorCode ErrorCode {
      get {
        return errorCode;
      }
    }
    [SerializeField]
    private CastErrorCode errorCode;

    /**
     * Developer-readable error message.
     */
    public string Message {
      get {
        return message;
      }
    }
    [SerializeField]
    private string message;

    /**
     * Returns user-readable error message title text.
     */
    public string ReadableErrorTitle {
      get {
        switch (errorCode) {
          case CastErrorCode.RemoteDisplayUnsupported:
            return "Remote Display Unsupported";
          case CastErrorCode.GooglePlayServicesUpdateRequired:
            return "Play Services Update Required";
          case CastErrorCode.RemoteDisplayConfigurationRejected:
            return "Remote Display Config Rejected";
          default:
            return "Unknown Error";
        }
      }
    }

    /**
     * Returns user-readable error message body text.
     */
    public string ReadableErrorBody {
      get {
        switch (errorCode) {
          case CastErrorCode.RemoteDisplayUnsupported:
            return "Your mobile device is not supported for this game. Please play using any of " +
                "the following:\n\nAndroid 4.4+\niOS 8+ and (iPad Mini 2+, iPad 3+, iPhone 5+)";
          case CastErrorCode.GooglePlayServicesUpdateRequired:
            return "Google Play Services requires an update.";
          case CastErrorCode.RemoteDisplayConfigurationRejected:
            return "Remote display configuration rejected by Cast device.";
          default:
            return "Unknown Error";
        }
      }
    }

    private CastError() {}

    /**
     * Constructor for CastError.
     */
    public CastError(CastErrorCode errorCode, string message) {
      this.errorCode = errorCode;
      this.message = message;
    }
  }

  /**
   * Specifies the remote display configuration to set up a session.
   */
  [System.Serializable]
  public class CastRemoteDisplayConfiguration {
    /**
     * The TV resolution to use for the remote display session. Higher the resolution, the more
     * bandwidth required.
     * This field is fully supported on iOS. Android and Unity simulator use this to change the
     * camera texture resolution - using a custom render texture instead of a camera will not use
     * the resolution set here.
     */
    public CastRemoteDisplayResolution Resolution {
      get {
        return resolution;
      }
    }
    [SerializeField]
    [Tooltip("iOS fully supported. Changes camera resolution on Android and Unity simulator.")]
    private CastRemoteDisplayResolution resolution;

    /**
     * The frame rate expected by the remote display session from the game. The
     * video encoder on the device and the TV will expect the game to provide
     * frames at this rate. If the game exceeds this frame rate, then it is
     * possible for stutters and slowdowns to occur. Slower devices and
     * bandwidth conditions might slow down actual framerate on the TV.
     * This field is iOS-only. Ignored by Android and Unity simulator.
     */
    public CastRemoteDisplayFrameRate FrameRate {
      get {
        return frameRate;
      }
    }
    [SerializeField]
    [Tooltip("Expected frame rate from the game. See docs for details. iOS-only. Ignored by Android and Unity simulator.")]
    private CastRemoteDisplayFrameRate frameRate;

    /**
     * Whether to disable adaptive video bitrate. If true, use a fixed bitrate set to
     * 3 Mbps. The default is false.
     * This is an experimental feature.
     * This field is iOS-only. Ignored by Android and Unity simulator.
     */
    public bool DisableAdaptiveVideoBitrate {
      get {
        return disableAdaptiveVideoBitrate;
      }
    }
    [SerializeField]
    [Tooltip("Experimental and iOS-only. Ignored by Android and Unity simulator.")]
    private bool disableAdaptiveVideoBitrate;

    private static readonly Dictionary<CastRemoteDisplayResolution, Vector2> resolutionMap =
      new Dictionary<CastRemoteDisplayResolution, Vector2> {
      { CastRemoteDisplayResolution.Resolution480p, new Vector2(848, 480) },
      { CastRemoteDisplayResolution.Resolution720p, new Vector2(1280, 720) },
      { CastRemoteDisplayResolution.Resolution1080p, new Vector2(1920, 1080) }
      };

    /**
     * Returns the current resolution width and height as a 2-D vector.
     */
    public Vector2 ResolutionDimensions {
      get {
        return resolutionMap[resolution];
      }
    }

    /**
     * Creates default remote display configuration to set up a session.
     */
    public CastRemoteDisplayConfiguration() {
      frameRate = CastRemoteDisplayFrameRate.Fps60;
      resolution = CastRemoteDisplayResolution.Resolution720p;
      disableAdaptiveVideoBitrate = false;
    }
  }

  /**
   * Used to allow the events fired by CastRemoteDisplayManager to be serializable in the inspector.
   */
  [System.Serializable]
  public class CastRemoteDisplayEvent : UnityEvent<CastRemoteDisplayManager> { }
}
