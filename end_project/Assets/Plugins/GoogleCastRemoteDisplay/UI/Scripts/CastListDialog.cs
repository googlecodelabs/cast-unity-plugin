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
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Google.Cast.RemoteDisplay.UI {
  /**
   * The dialog for displaying/selecting the list of Cast devices.
   */
  public class CastListDialog : MonoBehaviour {

    private const int SEARCH_TIMEOUT_SECONDS = 5;
    private delegate void Action();

    /**
     * Outlet for the title field.
     */
    [Tooltip("Default: Title")]
    public Text titleText;

    /**
     * Outlet for the "Searching/Connecting" text.
     */
    [Tooltip("Default: StatusText")]
    public Text statusText;

    /**
     * Prefab for the cast list elements.
     */
    [Tooltip("Default: CastListElementPrefab")]
    public GameObject listElementPrefab;

    /**
     * GameObject that contains the "searching for cast devices" state for the dialog.
     */
    [Tooltip("Default: CastListDialog->SearchingElements")]
    public GameObject searchingElements;

    /**
     * GameObject that contains the "list not found" state for the dialog.
     */
    [Tooltip("Default: CastListDialog->ListNotFoundElements")]
    public GameObject listNotFoundElements;

    /**
     * GameObject that contains the "list found" state for the dialog.
     */
    [Tooltip("Default: CastListDialog->ListFoundElements")]
    public GameObject listFoundElements;

    /**
     * GameObject that contains (and formats) the list of cast buttons.
     */
    [Tooltip("Default: CastListDialog->ListFoundElements->ScrollView->ContentPanel")]
    public GameObject contentPanel;

    /**
     * The callback for closing the cast list.
     */
    public UICallback closeButtonTappedCallback;

    /**
     * Currently displayed list of buttons - one for each cast device.
     */
    private List<GameObject> currentButtons = new List<GameObject>();

    /**
     * Tracks whether the game is currently casting, to maintain state.
     */
    private bool connecting = false;

    /**
     * A reference to the cast button for animating the icon.
     */
    public CastButtonFrame CastButtonFrame {
      set {
        castButtonFrame = value;
      }
    }
    private CastButtonFrame castButtonFrame;

    /**
     * Shows the list of cast devices, or shows the "searching for cast devices" state.
     */
    public void Show() {
      gameObject.SetActive(true);
      ResetDialog();
    }

    private void ResetDialog() {
      titleText.text = "Cast To";
      if (currentButtons.Count == 0) {
        statusText.text = "Finding Devices";
        searchingElements.SetActive(true);
        listNotFoundElements.SetActive(false);
        listFoundElements.SetActive(false);
        StartCoroutine(ShowNotFoundCoroutine());
      } else {
        searchingElements.SetActive(false);
        listNotFoundElements.SetActive(false);
        listFoundElements.SetActive(true);
      }
    }

    private IEnumerator ShowNotFoundCoroutine() {
      float startTime = Time.realtimeSinceStartup;
      while (Time.realtimeSinceStartup < startTime + SEARCH_TIMEOUT_SECONDS) {
        yield return null;
      }
      if (currentButtons.Count == 0) {
        ShowNotFoundState();
      }
    }

    /**
     * Hides the list of cast devices.
     */
    public void Hide() {
      connecting = false;
      gameObject.SetActive(false);
    }

    /**
     * Shows the dialog when no devices have been found.
     */
    private void ShowConnectingState() {
      titleText.text = "";
      statusText.text = "Connecting";
      searchingElements.SetActive(true);
      listNotFoundElements.SetActive(false);
      listFoundElements.SetActive(false);
      connecting = true;
    }

    /**
     * Shows the dialog when no devices have been found.
     */
    private void ShowNotFoundState() {
      if (gameObject.activeInHierarchy && currentButtons.Count == 0) {
        titleText.text = "No Device Found";
        searchingElements.SetActive(false);
        listNotFoundElements.SetActive(true);
        listFoundElements.SetActive(false);
      }
    }

    /**
     * Populates the list of casts with devices, and sets up callbacks for selecting said devices.
     */
    public void PopulateList(CastRemoteDisplayManager manager) {
      foreach (var button in currentButtons) {
        Destroy(button);
      }
      currentButtons.Clear();

      IList<CastDevice> devices = manager.GetCastDevices();
      foreach (CastDevice listDevice in devices) {
        GameObject newButton = Instantiate(listElementPrefab) as GameObject;
        CastListButton button = newButton.GetComponent<CastListButton>();
        button.nameLabel.text = listDevice.DeviceName;
        button.statusLabel.text = listDevice.Status;
        string deviceId = listDevice.DeviceId;
        button.button.onClick.AddListener(() => {
          manager.SelectCastDevice(deviceId);
          castButtonFrame.ShowConnecting();
          this.ShowConnectingState();
        });
        newButton.transform.SetParent(contentPanel.transform, false);
        currentButtons.Add(newButton);
      }

      if (!connecting) {
        ResetDialog();
      }
    }

    /**
     * Triggers the callback for closing the cast list. Set as the OnClick function for
     * CloseButton.
     */
    public void OnCloseButtonTapped() {
      closeButtonTappedCallback();
    }
  }
}
