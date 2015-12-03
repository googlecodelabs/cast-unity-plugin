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
   * Dialog to show the first time cast information.  Only to be shown once, on the first time a
   * Cast device is detected.
   */
  public class FirstTimeCastDialog : MonoBehaviour {
    /**
     * Shows the first time cast dialog.
     */
    public void Show() {
      gameObject.SetActive(true);
    }

    /**
     * Hides the first time cast dialog.
     */
    public void Hide() {
      gameObject.SetActive(false);
    }
  }
}
