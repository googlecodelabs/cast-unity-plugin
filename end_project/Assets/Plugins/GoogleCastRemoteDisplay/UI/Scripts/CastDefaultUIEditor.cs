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

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Google.Cast.RemoteDisplay.UI {
  /**
   * Custom editor for the CastDefaultUI.
   */
  [CustomEditor(typeof(CastDefaultUI))]
  public class CastDefaultUIEditor : Editor {
    public override void OnInspectorGUI() {
      DrawDefaultInspector();

      if (GUILayout.Button("Reset First Time Flag")) {
        PlayerPrefs.SetInt("firstTimeCastShown", 0);
      }
    }
  }
}

#endif
