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

namespace Google.Cast.RemoteDisplay.UI {
  /**
   * The button in a CastListDialog for selecting a Cast device.
   */
  public class CastListButton : MonoBehaviour {
    /**
     * The clickable button used to select a Cast device.
     */
    public Button button;
    /**
     * The label with the Cast device name.
     */
    public Text nameLabel;
    /**
     * The label with the Cast device status information.
     */
    public Text statusLabel;
    /**
     * The icon displayed by the button.
     */
    public RawImage icon;
  }
}
