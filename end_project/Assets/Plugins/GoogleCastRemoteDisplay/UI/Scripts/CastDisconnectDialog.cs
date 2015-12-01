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

namespace Google.Cast.RemoteDisplay.UI {
  /**
   * The dialog for displaying the disconnect dialog.
   */
  public class CastDisconnectDialog : MonoBehaviour {

    /**
     * Outlet for the disconnect button.
     */
    public Button disconnectButton;

    /**
     * The current cast device name.
     */
    public Text deviceName;

    /**
     * The callback for disconnecting and closing the dialog.
     */
    public UICallback disconnectButtonTappedCallback;

    /**
     * The callback for closing the disconnect dialog.
     */
    public UICallback closeButtonTappedCallback;

    /**
     * Set the cast device name for the dialog title.
     */
    public void SetDeviceName(string name) {
      deviceName.text = name;
    }

    /**
     * Triggers the callback for closing the disconnect dialog.  Set as the OnClick function for
     * DisconnectButton.
     */
    public void OnDisconnectButtonTapped() {
      disconnectButtonTappedCallback();
    }

    /**
     * Triggers the callback for closing the disconnect dialog.  Set as the OnClick function for
     * CloseButton.
     */
    public void OnCloseButtonTapped() {
      closeButtonTappedCallback();
    }
  }
}
