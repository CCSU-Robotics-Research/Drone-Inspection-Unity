# Drone Inspection Unity

Unity MRTK3 / OpenXR project for the HoloLens 2 end of the drone inspection system:
* Streams the user's head orientation (roll, pitch, yaw) over UDP to the ground station for gimbal teleoperation
* Displays the drone camera feed int he user's FOV, including the ground station's live AI defect annotations (red heatmap from DINOv3 and boxes from YOLO)
* Provides buttons in the FOV that toggle recording and live AI annotation on/off on the ground station

The ground station repo does all the video processing and AI work, while the Unity scene displays the video feedback and controls the gimbal with your head. In other words, it's as if you are sitting on the drone.

## Components

* [HeadOrientationSender.cs](Assets/Scripts/GimbalTeleoperation/HeadOrientationSender.cs) - Reads the head pose and sends "roll,pitch,yaw" UDP datagrams to the ground station. Destination IP, port, send rate, and axis inversions are serialized on the component in MainScene
* [VideoStreamReceiver.cs](Assets/Scripts/Video/VideoStreamReceiver.cs) - Connects to the ground station's video stream, receives frames on a background thread, an shows the newest frmae on teh VideoDisplay quad. Reconnects automatically if the stream drops momentarily
* [FrameAssembler.cs](Assets/Scripts/Video/FrameAssembler.cs) - Parses the video stream into complte JPEG frames. Mirrors the parser in the ground station repo
* [GroundStationCommander.cs](Assets/Scripts/Video/GroundStationCommander.cs) - Sends commands from the FOV buttons to the ground station
* [MainScene.unity](Assets/Scenes/MainScene.unity) - The HoloLens FOV. Contains the MRTK XR rig (MainCamera), the DebugCanvas with the roll/pitch/yaw readout, the VideoDisplay quad, the RecordButton and AnnotateButton, and the HeadOrientationSender and GroundStationCommander components
* [Tests/](Assets/Tests/) - Automated EditMode tests for FrameAssembler.cs.

## Unity Setup

1. Install Unity 2022.3.62f1 via Unity Hub. This is the latest version of Unity that supports HoloLens.
2. Clone this repo and open the project in Unity Hub. All the dependencies will load automatically, which needs a few minutes.
3. Install Visual Studio. Include the "Game development with Unity" workload and set Visual Studio as the external editor in Unity (with Edit > Preferences > External Tools).
4. For HoloLens hand input, enable the Microsoft Hand Interaction Profile under Edit > Project Settings > XR Plug-in Management > OpneXR (Standalone tab), then run the OpenXR project validation and choose Fix All.

IMPORTANT - Working with Scripts:
* Open scripts by double-clicking them in Unity's Project window. Unity recompiles automatically when you save; do not build them from Visual Studio.
* Rename, move, and delete scripts inside Unity's Project window and not in FileExplorer. Unity tracks each script through its `.meta` file, and doing this outsie Unity can disconnect scripts from the objects and buttons that use them.

## Usage

### Running the Full System

1. HoloLens and PC should be on the same network. Start the Holographic Remoting app on the HoloLens and note the IP it displays.
2. Configure Holographic Remoting in Unity with that IP.
3. In the ground station repository root, run `python launch.py`. This starts gimbal teleoperation and the camera feed with live AI annotation.
4. Press Play in Unity.

In the FOV:
* Your head-tracked motion drives the gimbal.
* The video panel shows the camera feed with an FPS counter showing its parsing latency on the ground station in the bottom left corner. *This does not include the latency of wireless transmission from the ground station to HoloLens*
* The record button toggles recording raw video footage for offline analysis, with a flashing indicator on the bottom right
* The AI button toggles live annotation. You should see a red heatmap where DINOv3 senses defects and bounding boxes where YOLO senses defects.

### Running One Component at a Time

Gimbal teleopration and the video feed can run individually:
* **Gimbal teleoperation:** In the ground station repository, navigate to `gimbal_teleoperation/`, activate the venv with `.\.venv\Scripts\Activate.ps1`, and run `python main.py`. Then press Play in Unity to see the HoloLens control the gimbal.
* **Video Feed:** In the ground station repository, navigate to `camera_vision/`, activate the venv, and run `python main.py`. To start the feed with annotation on, include the `--annotate` argument. Then press Play in Unity to see the camera feed in the HoloLens.

The video also appears in Unity's Game view without Holographic Remoting, which is useful fo rtesting, and for screen-recording what the headset shows.

### Simulating Hands Without the HoloLens

In Play mode, MRTK's input simulator provides virtual hands. Hold Space or Left Shift to raise a hand, and click to press a button.

_The connection settings live on each component in the Inspector panel. For video tuning, quality, and AI settings, refer to the ground station repository._

## Protocols

All connections run over localhost, since Holographic Remoting runs the app on the PC technically but mirrors it to HoloLens:
* Port 5005 (UDP) - head orientation, sent as "roll,pitch,yaw" text which is parsed by the ground station
* Port 5010 (TCP) - video stream from the ground dstation
* Port 5011 (UDP) - commands to the ground station for toggling AI annotation and video recording

For more details on the protocols, refer to the ground station repository.

## Testing

There are unit tests written in `Assets/Tests/` for the FrameAssembler. To run these in Unity, navigate to Window > General > Test Runner > EditMode > Run All.
