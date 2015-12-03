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

namespace Google.Cast.RemoteDisplay.UI {
  /**
   * Encapsulates the sprite outlets to organize assets required throughout the UI.
   */
  [System.Serializable]
  public class CastUISprites {
    /**
     * Sprite for the "casting" status.
     */
    public Sprite casting;

    /**
     * Sprite for the "not casting" status.
     */
    public Sprite notCasting;
  }

  /**
   * Functions for the various children of the Default UI to callback to this class
   */
  public delegate void UICallback();

  /**
   * The default UI for Cast functionality.  Handles cast selection, display, starting/ending Cast
   * sessions, basic error display, etc.
   */
  public class CastDefaultUI : MonoBehaviour {

    private const string FIRST_TIME_CAST_SHOWN = "firstTimeCastShown";

    /**
     * Container for the start/stop casting button.
     */
    public CastButtonFrame castButtonFrame;

    /**
     * The selection list UI element.
     */
    public CastListDialog castListDialog;

    /**
     * Dialog for displaying errors from the Remote Display Manager.
     */
    public CastErrorDialog errorDialog;

    /**
     * Dialog for displaying the "first time cast" information.
     */
    public FirstTimeCastDialog firstTimeCastDialog;

    /**
     * Dialog for disconnecting a cast device.
     */
    public CastDisconnectDialog castDisconnectDialog;

    /**
     * Controls the enabling of the default first time cast UI.  This will automatically turn on
     * the dialog if a cast device is discovered and the user has not seen it yet.
     */
    public bool enableFirstTimeCastUI = false;

    /**
     * Outlet for the sprites needed by various Cast UI components.
     */
    public CastUISprites castUISprites;

    /**
     * Outlet for the animator needed by the cast button.
     */
    public Animator connectingAnimator;

    /**
     * Dark background when showing dialogs.
     */
    public GameObject darkMask;

    /**
     * Tracks whether the UI Controller has performed initialization
     */
    private bool isInitialized;

    /**
     * Tracks whether the app is casting.
     */
    private bool isCasting = false;

    /**
     * Returns CastDefaultUI singleton. There should only be one CastDefaultUI at a time - this
     * allows easy programatic calling of public functionality.
     */
    public static CastDefaultUI GetInstance() {
      return instance;
    }
    private static CastDefaultUI instance = null;

    /**
     * If a default UI already exists, destroy the new one.
     */
    void Awake() {
      if (instance) {
        Debug.LogWarning("CastDefaultUI: Duplicate UI controller found - destroying.");
        DestroyImmediate(gameObject);
        return;
      } else {
        instance = this;
        DontDestroyOnLoad(gameObject);
      }
    }

    /**
     * Checks for the required remote display manager, and shows the default UI.
     */
    void Start() {
      if (instance != this) {
        return;
      }

      if (!CastRemoteDisplayManager.GetInstance()) {
        Debug.LogError("DebugCastUIController ERROR: No CastRemoteDisplayManager found!");
        Destroy(gameObject);
        return;
      }

      GetComponent<Canvas>().enabled = true;
      Initialize();
    }

    /**
     * When the UI is enabled, listen to the relevant events for the UI.
     */
    void OnEnable() {
      Initialize();
    }

    /**
     * When the UI is disabled, stop listening to the relevant events.
     */
    void OnDisable() {
      Uninitialize();
    }

    /**
     * Listens to the cast manager notifications, and sets up the start
     * state for the UI.
     */
    public void Initialize() {
      CastRemoteDisplayManager manager = CastRemoteDisplayManager.GetInstance();
      if (!isInitialized && manager) {
        manager.CastDevicesUpdatedEvent.AddListener(OnCastDevicesUpdated);
        manager.RemoteDisplaySessionStartEvent.AddListener(OnRemoteDisplaySessionStart);
        manager.RemoteDisplaySessionEndEvent.AddListener(OnRemoteDisplaySessionEnd);

        isInitialized = true;

        castButtonFrame.castButtonTappedCallback = OnCastButtonTapped;
        castListDialog.closeButtonTappedCallback = OnCloseCastList;
        errorDialog.okayButtonTappedCallback = OnConfirmErrorDialog;
        castDisconnectDialog.disconnectButtonTappedCallback = OnDisconnectButtonTapped;
        castDisconnectDialog.closeButtonTappedCallback = OnCloseDisconnectDialog;

        castButtonFrame.UiSprites = castUISprites;
        castButtonFrame.ConnectingAnimator = connectingAnimator;
        castListDialog.CastButtonFrame = castButtonFrame;

        HideAll();
        castListDialog.PopulateList(manager);
        castButtonFrame.Show();
      }
    }

    /**
     * Unlistens to the cast manager notifications.
     */
    public void Uninitialize() {
      CastRemoteDisplayManager manager = CastRemoteDisplayManager.GetInstance();
      if (isInitialized && manager) {
        manager.CastDevicesUpdatedEvent.RemoveListener(OnCastDevicesUpdated);
        manager.RemoteDisplaySessionStartEvent.RemoveListener(OnRemoteDisplaySessionStart);
        manager.RemoteDisplaySessionEndEvent.RemoveListener(OnRemoteDisplaySessionEnd);
        isInitialized = false;
      }
    }

    /**
     * Resets the UI to hidden, so the proper elements can be shown.
     */
    private void HideAll() {
      castButtonFrame.Hide();
      castListDialog.Hide();
      errorDialog.gameObject.SetActive(false);
      firstTimeCastDialog.Hide();
      castDisconnectDialog.gameObject.SetActive(false);
      darkMask.SetActive(false);
    }

    /**
     * When the list of devices updates, update the list. Called when the list of
     * devices updates.
     */
    public void OnCastDevicesUpdated(CastRemoteDisplayManager manager) {
      if (enableFirstTimeCastUI) {
        ShowFirstTimeCastDialogIfNeeded();
      }
      castListDialog.PopulateList(manager);
    }

    /**
     * Shows the first time cast dialog, if it hasn't yet been shown.
     */
    public void ShowFirstTimeCastDialogIfNeeded() {
      bool firstTimeCastShown = PlayerPrefs.GetInt(FIRST_TIME_CAST_SHOWN) == 0 ? false : true;
      if (!firstTimeCastShown && !isCasting) {
        HideAll();
        firstTimeCastDialog.Show();
        darkMask.SetActive(true);
        PlayerPrefs.SetInt(FIRST_TIME_CAST_SHOWN, 1);
      }
    }

    /**
     * Closes the list of devices, sets up the casting display.
     */
    public void OnRemoteDisplaySessionStart(CastRemoteDisplayManager manager) {
      isCasting = true;
      HideAll();
      castButtonFrame.ShowCasting();
    }

    /**
     * Cleans up display when the session is over.
     */
    public void OnRemoteDisplaySessionEnd(CastRemoteDisplayManager manager) {
      isCasting = false;
      castButtonFrame.ShowNotCasting();
    }

    /**
     * Callback when the user taps close button.
     */
    public void OnCloseCastList() {
      HideAll();
      castButtonFrame.ShowNotCasting();
    }

    /**
     * Either stop casting or open the list of detected cast devices.
     */
    public void OnCastButtonTapped() {
      HideAll();
      darkMask.SetActive(true);
      if (isCasting) {
        CastDevice selectedDevice = CastRemoteDisplayManager.GetInstance().GetSelectedCastDevice();
        if (selectedDevice != null) {
          castDisconnectDialog.SetDeviceName(selectedDevice.DeviceName);
        }
        castDisconnectDialog.gameObject.SetActive(true);
      } else {
        CastError error = CastRemoteDisplayManager.GetInstance().GetLastError();
        if (error == null) {
          darkMask.SetActive(true);
          castListDialog.Show();
        } else {
          errorDialog.SetError(error);
          errorDialog.gameObject.SetActive(true);
        }
      }
    }

    /**
     * Called when the error dialog is confirmed.
     */
    public void OnConfirmErrorDialog() {
      HideAll();
      castButtonFrame.ShowNotCasting();
    }

    /**
     * Called when the first time dialog is confirmed.
     */
    public void OnConfirmFirstTimeDialog() {
      HideAll();
      castButtonFrame.ShowNotCasting();
    }

    /**
     * Called when the learn more button is pressed.
     */
    public void OnConfirmLearnMore() {
      HideAll();
      castButtonFrame.ShowNotCasting();
      Application.OpenURL("http://www.chromecast.com/tv");
    }

    /**
     * Called when the disconnect button is pressed.
     */
    public void OnDisconnectButtonTapped() {
      HideAll();
      CastRemoteDisplayManager.GetInstance().StopRemoteDisplaySession();
    }

    /**
     * Callback when the user taps close button.
     */
    public void OnCloseDisconnectDialog() {
      HideAll();
      castButtonFrame.ShowCasting();
    }
  }
}
