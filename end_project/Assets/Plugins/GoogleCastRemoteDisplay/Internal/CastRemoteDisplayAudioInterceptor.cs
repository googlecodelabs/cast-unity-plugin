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

namespace Google.Cast.RemoteDisplay.Internal {
  /**
   * Required to be set on the AudioListener intended to be used in the remote display. Can't
   * add [RequereComponent(typeof(AudioListener))] to avoid requiring developers to remove this
   * internal component before destroying an AudioListener.
   */
  public class CastRemoteDisplayAudioInterceptor : MonoBehaviour {

    private CastRemoteDisplayExtensionManager extensionManager;

    public void SetCastRemoteDisplayExtensionManager(
        CastRemoteDisplayExtensionManager extensionManager) {
      this.extensionManager = extensionManager;
    }

    /**
     * The Unity callback for when the AudioListener component on this object picks up audio every
     *  frame.
     */
    void OnAudioFilterRead(float[] data, int channels) {
      if (extensionManager != null) {
        extensionManager.OnAudioFilterRead(data, channels);
      }
    }
  }
}
