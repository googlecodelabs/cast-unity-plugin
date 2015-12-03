Google Cast Remote Display Unity Plugin
---------------------------------------

The Google Cast Remote Display Unity Plugin enables Unity applications to render
graphics and audio to a Google Cast device, such as a Chromecast, using the
Google Cast Remote Display API.

For more information about Google Cast, see:
https://developers.google.com/cast/

Requirements

* Unity 5
* Google Cast capable device (e.g. Chromecast, Android TV)
* Android device with KitKat+
* iOS device with iOS 8+

Before you start

1. Although the Unity Plugin hides a lot of the details of the Google Cast
   Remote Display API, it is suggested you review the video and documentation
   at https://developers.google.com/cast/docs/remote

2. You must set up an application ID as described at
   https://developers.google.com/cast/docs/remote#registration
   and you can use the same application ID for your Android and iOS app.

3. Make sure your Unity development machine already has the appropriate Android
   and iOS development tools installed (e.g. Android SDK, XCode, etc).

Unity Setup

1. Open a scene in Unity.

2. Select "Assets > Import Package > Custom Package"

3. Choose the Google Cast Remote Display Unity Plugin package.

4. Verify that these files and folders were added:
   a. Assets / Plugins / GoogleCastRemoteDisplay
   b. Assets / Plugins / Android / cast_remote_display_unity3d_lib.aar
   c. Assets / Plugins / iOS / google-cast-remote-display_lib

5. Add the CastRemoteDisplayManager.prefab from Assets / Plugins /
   GoogleCastRemoteDisplay to your scene hierarchy. This must be at the top
   level of your scene hierarchy.

6. Make sure your scene has the following:
   a. A main camera for showing graphics on the phone.
   b. A remote display camera for showing graphics on the TV.

7. Skip this step if you are using Unity 5.3+, this is deprecated in Unity 5.3+:

   Create a new game object, which will function as the audio listener in your
   scene. Add the script component, CastRemoteDisplayAudioListener.cs from
   Assets / Plugins / GoogleCastRemoteDisplay. This audio listener will be
   used to send audio to the TV.

   Important: You should only have one enabled audio listener in your scene at
   a time. Multiple enabled audio listeners will cause audio problems.

8. Select the CastRemoteDisplayManager in your hierarchy and update these
   fields:
   a. For Remote Display Camera, select the remote display camera in your scene
      that you want to show graphics on the TV (you do not need to specify a
      main camera - but the main camera should NOT be the same as the remote
      display camera).
   b. For Remote Audio Listener, select the game object with
      CastRemoteDisplayAudioListener.cs that you set up in step #7.
   c. For Cast App Id, enter the remote display application ID that you
      registered.
   d. If you want to display a static texture while the game is backgrounded on
      the phone or tablet, set a texture to the RemoteDisplayPausedTexture
      field.
   e. For Configuration, you can use the defaults.
      a. Resolution affects the texture used by Remote Display Camera.
      b. Frame Rate, Target Delay, Disable Adaptive Bitrate are only supported
         on iOS.

9. If you want try testing your game right away with a simple pre-built UI to
   select a cast device, follow these steps:
   a. Add the CastDefaultUI.prefab from Assets / Plugins /
      GoogleCastRemoteDisplay to your scene hierarchy.

      Important: This must be at the top level of your scene hierarchy.

   b. Verify that your scene hierarchy has a Unity EventSystem, otherwise the UI
      will not work.

10. If you want to try simulating remote display within the Unity editor,
    follow these steps (it’s easier to use the pre-built UI from step 9
    beforehand, but not required):
    a. Add the CastRemoteDisplaySimulator.prefab from Assets / Plugins /
       GoogleCastRemoteDisplay to your scene hierarchy.

       Important: This must be at the top level of your scene hierachy.

    b. Update the Cast Devices section with the CastRemoteDisplaySimulator in
       your scene (e.g. change Size to “1” and fill in dummy values for Device
       ID, Device Name, and Status.
    c. Press the play button to run your scene within Unity and then press the
       Cast button on the top right corner (or make the appropriate call on the
       CastRemoteDisplayManager) to start simulating remote display.

11. Follow the Android specific steps for building and deployment on Android
    (there are no additional steps for iOS) those devices.


Android Setup Steps

The Android portion of the plugin depends on these Android libraries:

* Media Router
* Google Play Services 8.3+
* App Compat

You can get the latest versions of these dependencies from your Android SDK
installation folder.

1. Place these dependencies in Assets / Plugins / Android. This folder should
   now have these files:
   a. cast_remote_display_unity3d_lib.aar
   b. appcompat
   c. google-play-services_lib
   d. mediarouter

2. Update your AndroidManifest.xml in Assets / Plugins / Android and don’t
   forget to update your package name:
   a. Remote display requires Api level 19, but your App can still set a lower
      API level if it doesn’t require cast functionality to work.
   b. Copy the Bundle Identifier string under “Identification”  (e.g.
      com.Unity3D.StarTrooper)

Unity Scripting API

There are two key classes, found under Assets / Plugins /
GoogleCastRemoteDisplay:

1. Google.Cast.RemoteDisplay.CastRemoteDisplayManager

The entry point in Unity for interacting with the Android and iOS native
implementations of Google Cast Remote Display.  Behaves as a singleton and
there should only be one. Important methods include:

* GetInstance: Returns the instance of the class, or null if no object has
  been created.

* RemoteDisplayCamera: The Camera that should be displayed in the remote
  display. Can be set at runtime. Ignored if RemoteDisplayTexture is set.

* RemoteDisplayTexture: The texture that should be displayed in the remote
  display. Can be set at runtime.

* RemoteDisplayPausedTexture: The texture that should be displayed in the
  remote display if the game gets backgrounded on the phone or tablet. Can be
  set at runtime.

* GetCastDevices: this returns a list of CastDevice structs, which includes
  the:
    * deviceId: use this to select cast devices using the scripting API
    * deviceName: the name of the cast device  to show in your GUI
    * status: the current status text of the cast device to show in your GUI

* SelectCastDevice: this should only be called after the OnCastDevicesUpdated
  method is called in the connectivity listener. Use the deviceId field of the
  CastDevice struct from GetCastDevices.

* GetSelectedCastDevice: Returns the device we are currently casting to, or
  null if no cast session is currently active.

* IsCasting : Whether there is an active cast session. This will be set to true
  from the moment the RemoteDisplaySessionStartEvent fires and until the
  session ends.

* StopRemoteDisplaySession: Stops casting.

* GetLastError: Returns the last error encountered by the plugin, or null if
  no error has occurred.

* Published events:
    * CastDevicesUpdatedEvent: fired when the list of available cast devices
      has been updated
    * RemoteDisplaySessionStartEvent:  fired when a cast session starts
    * RemoteDisplaySessionEndEvent: fired when a cast session ends
    * RemoteDisplayErrorEvent: fired when an error is encountered

All event listeners must take a parameter of type CastRemoteDisplayManager and
return void.

2. Google.Cast.RemoteDisplay.UI.CastDefaultUI (under UI / Scripts)

The entry point for an example of a UI for selecting cast devices. This is not
meant to be an API, but an example you can use for testing purposes, or use as
a reference to implement your own custom game UI.
