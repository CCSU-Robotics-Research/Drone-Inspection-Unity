# Drone Inspection Unity

Unity MRTK3 / OpenXR project for the HoloLens end:
* Streams the user's head orientation (roll, pitch, yaw) over UDP to the ground station for gimbal teleoperation.
* Displays the camera feed from the drone in the user's FOV.

## Components

* [HeadOrientationSender.cs](Assets/Scripts/GimbalTeleoperation/HeadOrientationSender.cs) - Reads the head pose and sends "roll,pitch,yaw" UDP datagrams to the ground station. Destination IP, port, and send rate ar serialized on the component in MainScene.
* [VideoStreamReceiver.cs](Assets/Scripts/Video/VideoStreamReceiver.cs) - Connects to video stream from ground
* [FrameAssembler.cs](Assets/Scripts/Video/FrameAssembler.cs) - Parser for the video stream to receive the video feed from Python.
* [MainScene.unity](Assets/Scenes/MainScene.unity) - The HoloLens FOV shown, contains the Main camera, DebugCanvas for RPY transmission, VideoDisplay quad, and HeadOrientationSender.
* [Tests/](Assets/Tests/) - Automated EditMode tests for FrameAssembler.cs.

## Unity Setup

1. Install Unity 2022.3.62f1 via Unity Hub. This is the latest version of Unity that supports HoloLens.
2. Clone this repo and open the project in Unity Hub. All the dependencies will load automatically and needs a few minutes.
3. Install Visual Studio. Include the "Game development with Unity" workload and set Visual Studio as the extenral editor in Unity (with Edit > Preferences > External Tools).
4. When opening C# scripts, use the Unity Editor. All changes are automatically reflected in meta files. Unity will compile scripts automatically as they are edited; do not build them from Visual Studio.

## Usage

Both gimbal teleoperation and video feed can run simultaneously, or one at a time.

### Gimbal Teleoperation:
1. HoloLens and PC should be on the same network. Start the Holographic Remoting app on the HoloLens and note the IP it displays.
2. Configure Holographic Remoting in Unity with that IP.
3. In the ground station repository, navigate to `gimbal_teleoperation/`, source the venv with `.\.venv\Scripts\Activate.ps1` and then run `python main.py`.
4. Press Play in Unity. Head motion should drive the gimbal to work.

### Video Feed
1. In the ground station repository, navigate to `camera_vision/`, source the venv with `.\.venv\Scripts\Activate.ps1`, and run `python main.py`.
2. Press Play in Unity. The VideoDisplay quad should show the feed. Even without Holographic Remoting, the video should be visible in the Game view.

_For reference, the connection settings live on the VideoDisplay quad's Video Stream Receiver component in the Inspector panel. Unity will display whatever it receives. For video tuning and quality, refer to the ground station repository._

## Testing

There are unit tests written in `Assets/Tests/` for the FrameAssembler. To run these in Unity, navigate to Window > General > Test Runner > EditMode > Run All.
