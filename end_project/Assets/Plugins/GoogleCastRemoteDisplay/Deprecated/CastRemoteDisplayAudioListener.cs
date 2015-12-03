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

namespace Google.Cast.RemoteDisplay {
  /**
   * Ensures that an AudioListener has the required components to make sure they are not added at
   * runtime. This component should be added in the Unity editor and not at runtime.
   *
   * @deprecated Do not use if you are developing with Unity 5.3+.
   *
   * This component is required because Unity had a bug on iOS platforms where the OnAudioFilterRead
   * callback was not being called unless the component was present at scene initialization time.
   *
   * INSTRUCTIONS: Attach this component to the game object that will act as the audio listener for
   * the remote display and assign it as the RemoteAudioListener of the CastRemoteDisplayManager.
   * This component is marked as DontDestroyOnLoad, so make sure to place/parent it correctly when
   * loading scenes. This won't work if you attach this component at runtime.
   *
   * In Unity 5.3+ this bug was fixed and the CastRemoteDisplayManager will attach the required
   * component at runtime. Simply set the RemoteAudioListener property to any AudioListener.
   */
  [RequireComponent(typeof(AudioListener))]
  [RequireComponent(typeof(CastRemoteDisplayAudioInterceptor))]
  public class CastRemoteDisplayAudioListener : MonoBehaviour {
    private void Awake() {
      DontDestroyOnLoad(this);
    }
  }
}
