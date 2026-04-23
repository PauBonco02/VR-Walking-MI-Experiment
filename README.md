## VR-Walking-Experiment
This repository provides all the Unity files for a Walking and Turning simulation environment in VR, used for EEG-based motor imagery (MI) analysis and classification in our paper [EEG-based Forward Movement and Turning MI Classification with and without Action Observation in Virtual Reality](https://ieeexplore.ieee.org/document/11435097). For a short explanation of this project, please check out [my personal website](https://pauboncompte.me/projects/bci-walk/).

## Key scripts:
- **LSLOutlet.cs:** This script is in charge of creating an LSL Outlet accessible from any script and in any scene. 
- **GridMovement.cs:** This script automates the standard MI trial structure, making it follow a specific chronological sequence as specified in our paper. It first creates a randomized sequence of movements (Forward Movement, Left and Right Turning). It then uses Coroutines to provide smooth movement and rotations for each action trial. It also communicates with **LSLOutlet.cs** to push markers corresponding to each state (S for standing, F for forward, L for left, R for right) into the data stream, so that the action events are aligned with the EEG data.
- **GridChunk:** This script generates the forest grid by joining 4 different forest chunk models in randomized rotations to increase variability and provide a naturalistic, engaging virtual setting. 

## Scenes:
- Menu: This scene serves as the initial hub. From there, you can click the button "Start" and will go to the experiment scene.
- ExpGrid: This is the scene where the experiment is carried out. After all the Walking and Turning samples, you will be sent back to the Menu, where you can reenter the experiment again (in case you want to separate it into shorter subsessions as done in our paper).

## How to personalize the experiment settings
You can personalize the experiment via the GridMovement.cs script attached to the XR Rig (inside Pivot). The Inspector allows you to modify action durations, wait times, sample counts per session, and instruction audio files.

If you have any questions, issues, or would simply like to know more about the project, please feel free to open an issue or reach out.
