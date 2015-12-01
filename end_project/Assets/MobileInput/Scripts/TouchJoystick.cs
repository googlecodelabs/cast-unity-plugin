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

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnitySampleAssets.CrossPlatformInput;

/**
 * A joystick control that you can anchor by touching the mobile device screen.
 * Based on standard assets TouchPad.
 */
namespace UnityStandardAssets.CrossPlatformInput
{
  [RequireComponent(typeof(Image))]
  public class TouchJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    /**
     * Joystick image.
     */
    public Texture joystickTexture;

    /**
     * Ring image.
     */
    public Texture ringTexture;

    /**
     * The name given to the horizontal axis for the cross platform input.
     */
    public string horizontalAxisName = "Horizontal";

    /**
     * The name given to the vertical axis for the cross platform input.
     */
    public string verticalAxisName = "Vertical";

    /**
     * X sensitivity in pixels.
     */
    public float xSensitivity = 1f;

    /**
     * Y sensitivity in pixels.
     */
    public float ySensitivity = 1f;

    /**
     * Reference to the joystick in the cross platform input.
     */
    CrossPlatformInputManager.VirtualAxis horizontalVirtualAxis;

    /**
     * Reference to the joystick in the cross platform input.
     */
    CrossPlatformInputManager.VirtualAxis verticalVirtualAxis;

    /**
     * Touch dragging state.
     */
    bool isDragging;

    /**
     * Touch event id.
     */
    int touchId = -1;

    /**
     * Current touch position.
     */
    Vector2 currentPos = new Vector2(0, 0);

    /**
     * Touch down position.
     */
    Vector2 downPos = new Vector2(0, 0);

    /**
     * Ring position.
     */
    Vector2 ringPos = new Vector2(0, 0);

    /**
     * Diff between down and current touch positions.
     */
    Vector2 currentToDownPos = new Vector2(0, 0);

    /**
     * Radius of the joystick image in pixels.
     */
    float joystickRadius;

    /**
     * Radius of the ring image in pixels.
     */
    float ringRadius;


    #if !UNITY_EDITOR
    /**
     * Center of game object.
     */
    private Vector3 center;

    /**
     * Image game object.
     */
    private Image image;
    #else
    /**
     * Previous mouse location.
     */
    Vector3 previousMouse;
    #endif

    /**
     * Desired 720p screen resolution.
     */
    private int screenResolutionWidth = 1280;
    private int screenResolutionHeight = 720;

    /**
     * Enable the game object.
     */
    void Start() {
      // Set screen resolution.
      Screen.SetResolution(screenResolutionWidth, screenResolutionHeight, true);
    }

    /**
     * Enable the game object.
     */
    void OnEnable() {
      CreateVirtualAxes ();
      #if !UNITY_EDITOR
      image = GetComponent<Image>();
      center = image.transform.position;
      #endif
      // Pre-calculate some values to size the graphics.
      joystickRadius = screenResolutionHeight / 8;
      ringRadius = joystickRadius * 1.75f;
    }

    /**
     * Create the virtual axes for the joystick.
     */
    void CreateVirtualAxes() {
      horizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
      verticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
    }

    /**
     * Update the virtual axes for the joystick.
     */
    void UpdateVirtualAxes(Vector3 value) {
      value = value.normalized;
      horizontalVirtualAxis.Update(value.x);
      verticalVirtualAxis.Update(value.y);
    }

    /**
     * Handle mouse down event.
     */
    public void OnPointerDown(PointerEventData data) {
      isDragging = true;
      touchId = data.pointerId;
      #if !UNITY_EDITOR
      // Set center position of the ring as the touch position.
      center = data.position;
      #endif
      downPos = data.position;
      ringPos = downPos;
    }

    /**
     * Update handler.
     */
    void Update() {
      if (!isDragging) {
        return;
      }
      if (Input.touchCount >= touchId + 1 && touchId != -1) {
        #if !UNITY_EDITOR
        Vector2 pointerDelta = new Vector2(Input.touches [touchId].position.x - center.x,
                                             Input.touches [touchId].position.y - center.y)
                                             .normalized;
        pointerDelta.x *= xSensitivity;
        pointerDelta.y *= ySensitivity;
        currentPos.x = Input.touches [touchId].position.x;
        currentPos.y = Input.touches [touchId].position.y;
        #else
        Vector2 pointerDelta;
        pointerDelta.x = Input.mousePosition.x - previousMouse.x;
        pointerDelta.y = Input.mousePosition.y - previousMouse.y;
        previousMouse = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
        currentPos.x = Input.mousePosition.x;
        currentPos.y = Input.mousePosition.y;
        #endif
        UpdateVirtualAxes(new Vector3(pointerDelta.x, pointerDelta.y, 0));
      }
    }

    /**
     * Handle mouse up event.
     */
    public void OnPointerUp(PointerEventData data) {
      isDragging = false;
      touchId = -1;
      UpdateVirtualAxes(Vector3.zero);
    }

    /**
     * Disable the game object.
     */
    void OnDisable() {
      if (CrossPlatformInputManager.VirtualAxisReference(horizontalAxisName) != null)
        CrossPlatformInputManager.UnRegisterVirtualAxis(horizontalAxisName);

      if (CrossPlatformInputManager.VirtualAxisReference(verticalAxisName) != null)
        CrossPlatformInputManager.UnRegisterVirtualAxis(verticalAxisName);
    }

    /**
     * Update the GUI. Draw the joystick image within the ring image boundary.
     */
    void OnGUI() {
      if (!isDragging) {
        return;
      }

      // Make the outer ring follow the joystick movements.
      currentToDownPos = downPos - currentPos;
      if (currentToDownPos.magnitude > ringRadius/2) {
        ringPos = currentPos + Vector2.ClampMagnitude(currentToDownPos, ringRadius/2);
        downPos = ringPos;
      }

      // Draw the outer ring.
      GUI.DrawTexture(new Rect(ringPos.x - ringRadius, Screen.height - ringPos.y - ringRadius,
                                 ringRadius * 2, ringRadius * 2), ringTexture, ScaleMode.ScaleToFit, true);

      // Draw the joystick.
      GUI.DrawTexture(new Rect(currentPos.x - joystickRadius, Screen.height - currentPos.y -
                                 joystickRadius, joystickRadius * 2, joystickRadius * 2), joystickTexture,
                                 ScaleMode.ScaleToFit, true);
    }
  }
}
