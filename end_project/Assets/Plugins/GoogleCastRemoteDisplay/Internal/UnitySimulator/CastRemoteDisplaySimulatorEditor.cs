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


namespace Google.Cast.RemoteDisplay.Internal {
  /**
   * Custom editor for the CastRemoteDisplaySimulator.
   */
  [CustomEditor(typeof(CastRemoteDisplaySimulator))]
  public class CastRemoteDisplaySimulatorEditor : Editor {

    protected static bool openFoldout = true;
    protected static int selected = 0;
    protected static CastErrorCode errorCode = CastErrorCode.NoError;

    public override void OnInspectorGUI() {
      CastRemoteDisplaySimulator simulator = (CastRemoteDisplaySimulator) target;

      serializedObject.Update();
      EditorGUILayout.PropertyField(serializedObject.FindProperty("simulateRemoteDisplay"), true);
      EditorGUILayout.PropertyField(serializedObject.FindProperty("remoteDisplayRect"), true);
      EditorGUILayout.PropertyField(serializedObject.FindProperty("castDevices"), true);
      serializedObject.ApplyModifiedProperties();

      if (Application.isPlaying) {
        // Update the list of devices.
        if (GUILayout.Button("Update devices")) {
          simulator.UpdateDevices();
        }
        EditorGUILayout.Space();

        // Throwing errors.
        errorCode = (CastErrorCode) EditorGUILayout.EnumPopup("Throw Error", errorCode);
        if (errorCode != CastErrorCode.NoError) {
          if (GUILayout.Button("Throw")) {
            simulator.ThrowError(errorCode);
          }
        }
      }
    }
  }
}

#endif
