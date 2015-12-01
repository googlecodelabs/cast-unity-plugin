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
using System.Collections.Generic;

namespace Google.Cast.RemoteDisplay.Internal {
  /**
   * Extension for debugging the remote display plugin in Unity.
   */
  public class CastRemoteDisplayUnityExtension : ICastRemoteDisplayExtension {
    private CastRemoteDisplayExtensionManager extensionManager;
    private CastRemoteDisplaySimulator displaySimulator;
    private Texture remoteDisplayTexture;
    private Texture2D screenTexture;
    private Material material;

    public CastRemoteDisplayUnityExtension(CastRemoteDisplayExtensionManager extensionManager,
        CastRemoteDisplaySimulator displaySimulator) {
      this.extensionManager = extensionManager;
      this.displaySimulator = displaySimulator;
      this.displaySimulator.DisplayExtension = this;
    }

    public void OnEnable() {
    }

    public void Activate() {
      material = new Material(Shader.Find("Unlit/Texture"));
      material.hideFlags = HideFlags.HideAndDontSave;
    }

    public void Deactivate() {
      GameObject.DestroyImmediate(material);
    }

    public void RenderFrame() {
      if (extensionManager.IsCasting()
          && remoteDisplayTexture != null
          && displaySimulator.simulateRemoteDisplay
          && material != null) {
        Rect rect = new Rect(Screen.width * displaySimulator.remoteDisplayRect.xMin,
            Screen.height * displaySimulator.remoteDisplayRect.yMin,
            Screen.width * displaySimulator.remoteDisplayRect.width,
            Screen.height * displaySimulator.remoteDisplayRect.height);

        GUI.DrawTexture(rect, remoteDisplayTexture, ScaleMode.ScaleToFit, false);
      }
    }

    public void UpdateDevices() {
      extensionManager._callback_OnCastDevicesUpdated("dummy");
    }

    public void ThrowError(CastErrorCode errorCode) {
      string rawErrorString = (int) errorCode + ": There was a fake error thrown by the " +
        "simulator - " + errorCode.ToString();
      extensionManager._callback_OnCastError(rawErrorString);
    }

    public void OnAudioFilterRead(float[] data, int channels) {
    }

    public void OnRemoteDisplaySessionStart(string deviceName) {
    }

    public void OnRemoteDisplaySessionStop() {
    }

    public List<CastDevice> GetCastDevices(ref CastDevice connectedCastDevice) {
      foreach (CastDevice castDevice in displaySimulator.castDevices) {
        if (connectedCastDevice != null &&
            castDevice.DeviceId == connectedCastDevice.DeviceId) {
          connectedCastDevice = castDevice;
        }
      }
      return new List<CastDevice>(displaySimulator.castDevices);
    }

    public void SelectCastDevice(string deviceId) {
      extensionManager._callback_OnRemoteDisplaySessionStart(deviceId);
    }

    public void SetRemoteDisplayTexture(Texture texture) {
      remoteDisplayTexture = texture;
    }

    public void StopRemoteDisplaySession() {
      extensionManager._callback_OnRemoteDisplaySessionEnd("dummy");
    }
  }
}
