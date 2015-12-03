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

#if UNITY_ANDROID

using UnityEngine;
using System;
using System.Collections.Generic;


namespace Google.Cast.RemoteDisplay.Internal {
  /**
   * Android-specific implementation to extend Unity to support Google Cast Remote Display.
   */
  public class CastRemoteDisplayAndroidExtension : ICastRemoteDisplayExtension {

    private static string ANDROID_BRIDGE_CLASS_NAME = "com.google.cast.unityplugin.UnityBridge";

    private static string DEVICE_LIST_MARKER_DONE = "DONE";
    private static string DEVICE_LIST_MARKER_NOT_DONE = "NOT_DONE";

    private static string NATIVE_START_SCAN = "_native_startScan";
    private static string NATIVE_TEARDOWN_REMOTE_DISPLAY = "_native_teardownRemoteDisplay";
    private static string NATIVE_CALL_RENDER_FRAME = "_native_renderFrame";
    private static string NATIVE_SET_REMOTE_DISPLAY_TEXTURE = "_native_setRemoteDisplayTexture";
    private static string NATIVE_GET_CAST_DEVICES = "_native_getCastDevices";
    private static string NATIVE_SELECT_CAST_DEVICE = "_native_selectCastDevice";
    private static string NATIVE_STOP_REMOTE_DISPLAY_SESSION = "_native_stopRemoteDisplaySession";

    // Max number of JNI calls for a single call to #GetCastDevices.
    private static int MAX_NUMBER_OF_JNI_CALLS_FOR_CAST_DEVICES = 10;

    private CastRemoteDisplayExtensionManager extensionManager;

    private AndroidJavaClass bridge;

    public CastRemoteDisplayAndroidExtension(CastRemoteDisplayExtensionManager extensionManager) {
      this.extensionManager = extensionManager;
    }

    public void Activate() {
      Debug.Log("RemoteDisplayController started.");
      bridge = new AndroidJavaClass(ANDROID_BRIDGE_CLASS_NAME);
      if (bridge != null) {
        // Start scanning for cast media routes. The second parameter is the name of the game
        // object that should get callbacks from the Android side.
        bridge.CallStatic(NATIVE_START_SCAN, extensionManager.CastRemoteDisplayManager.CastAppId,
            extensionManager.name);
      } else {
        Debug.LogError("Couldn't initialize the Android Remote Display native library. " +
            "Couldn't not find class " + ANDROID_BRIDGE_CLASS_NAME);
      }
    }

    public void Deactivate() {
      if (bridge != null) {
        bridge.CallStatic(NATIVE_TEARDOWN_REMOTE_DISPLAY);
        bridge.Dispose();
      }
    }

    public void RenderFrame() {
      if (bridge != null) {
        bridge.CallStatic(NATIVE_CALL_RENDER_FRAME);
      }
    }

    public void OnAudioFilterRead(float[] data, int channels) {
    }

    public void OnRemoteDisplaySessionStart(string deviceId) {
    }

    public void SetRemoteDisplayTexture(Texture texture) {
      IntPtr texturePointer = texture.GetNativeTexturePtr();
      if (texturePointer == IntPtr.Zero) {
        Debug.LogError("Couldn't obtain native pointer for the remote display texture.");
        return;
      }
      if (bridge != null) {
        Debug.Log("Setting texture with ID: " + texturePointer.ToInt64());
        bridge.CallStatic(NATIVE_SET_REMOTE_DISPLAY_TEXTURE, texturePointer.ToInt64());
      }
    }

    public void OnRemoteDisplaySessionStop() {
    }

    public List<CastDevice> GetCastDevices(ref CastDevice connectedCastDevice) {
      List<CastDevice> devices = new List<CastDevice>();
      using (AndroidJavaClass bridge = new AndroidJavaClass(ANDROID_BRIDGE_CLASS_NAME)) {

        string statusString = DEVICE_LIST_MARKER_NOT_DONE;
        int numberOfJniCalls = 0;

        // The static library will respond with a partial device list.
        // The first string indicates whether the list is complete or not, after that, each
        // device is represented by 3 strings: The order is deviceId, deviceName and status.
        // We must call the method mutiple time to get the entire list.
        while (statusString != DEVICE_LIST_MARKER_DONE) {
          using (AndroidJavaObject returnedArray =
              bridge.CallStatic<AndroidJavaObject>(NATIVE_GET_CAST_DEVICES)) {
            if (returnedArray != null && returnedArray.GetRawObject().ToInt32() != 0) {
              string[] deviceInfoStrings =
                  AndroidJNIHelper.ConvertFromJNIArray<string[]>(returnedArray.GetRawObject());
              statusString = deviceInfoStrings[0];
              int i = 1;
              while (i < deviceInfoStrings.Length - 2) {
                // Creates a CastDevice with ID, Name, and Status.
                CastDevice device = new CastDevice(deviceInfoStrings[i++],
                    deviceInfoStrings[i++],
                    deviceInfoStrings[i++]);
                devices.Add(device);
                if (connectedCastDevice != null &&
                    connectedCastDevice.DeviceId == device.DeviceId) {
                  connectedCastDevice = device;
                }
              }
            } else {
              Debug.LogError("Couldn't retrieve list of Cast Devices.");
              StopRemoteDisplaySession();
              devices.Clear();
              return devices;
            }
          }
          numberOfJniCalls++;
          if (numberOfJniCalls >= MAX_NUMBER_OF_JNI_CALLS_FOR_CAST_DEVICES) {
            Debug.LogError("Couldn't retrieve the full list of cast devices. JNI call limit " +
                "exceeded.");
            break;
          }
        }
      }
      return devices;
    }

    public void SelectCastDevice(string deviceId) {
      if (bridge != null) {
        bridge.CallStatic(NATIVE_SELECT_CAST_DEVICE, deviceId);
      }
    }

    public void StopRemoteDisplaySession() {
      if (bridge != null) {
        bridge.CallStatic(NATIVE_STOP_REMOTE_DISPLAY_SESSION);
      }
    }
  }
}
#endif
