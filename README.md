# Introduction
This project is an extension of the DH2323 animation track lab 3. The original implementation focused on tank movement, shooting mechanics, and camera control. In this extended version for the project, additional work was performed to investigate how different movement smoothing and interpolation techniques affect perceived responsiveness and game feel.

The project focuses on three main areas: turret rotation, tank movement, and camera movement. Several implementations were created and compared using interpolation techniques such as SLERP and movement smoothing. The purpose of the project was to investigate how relatively small implementation changes can influence player experience and immersion.

Below is a project blog detailing my journey with the implementation.

# DH2323 Project Blog

## Rotation Modes

It is now possible to change between Instant and SLERP rotation modes:

### Instant Rotation

With instant rotation, the turret immediately snaps toward the target direction. Rotation speed does not matter because the orientation changes instantly every frame.

The downside is that rapid mouse movement, especially when the cursor is very close to the center of the tank, can cause the turret to rotate extremely fast and feel unnatural.

[Instant Rotation Video][RotVid1]

### SLERP Rotation

To improve this behavior, I implemented Spherical Linear Interpolation (SLERP).

SLERP is a method of smoothly interpolating between two rotations represented by quaternions. Instead of instantly snapping to the target rotation, the turret gradually rotates toward it over time along the shortest rotational path.

This gives the turret a much more natural and responsive feeling.

A configurable rotation-speed variable was added as well:

High values make the behavior almost identical to instant rotation.  
Lower values create slower, heavier turret movement.

[SLERP Rotation Video][RotVid2]

### SQUAD: First try

Next, I wanted to experiment with SQUAD (Spherical quadrangle interpolation) to compare the results with SLERP.

While SLERP interpolates smoothly between two rotations, SQUAD extends this idea to create smoother transitions across multiple rotations. This is especially useful for animation systems where rotational continuity is important.

Current problem: the turret seems to have a few fixed places where it wants to be. It basically flickers all the time. 

[SQUAD flickering video][SQUADFlickVid]

### SQUAD: It works\!

Quaternions can represent the same rotation using opposite signs. If neighboring quaternions are not aligned to the same hemisphere before interpolation, the interpolation may suddenly flip directions, causing jittering and flickering.

After ensuring that all quaternions used by SQUAD were on the same hemisphere with an if statement, the interpolation became stable and smooth. It currently looks like this approach is unnecessarily complicated for the purpose of rotating a turret compared to SLERP.

[SQUAD Rotation Video][RotVid4]

## Tank Movement\!

The tank can now switch between three different movement modes:

- [Direct Movement][DirectMovementVid]  
  - Pressing W immediately sets the tank to maximum speed.  
- [Acceleration][AccelMovementVid]
  - The tank gradually accelerates toward its maximum speed.  
- [Acceleration & Deceleration][AccelDeaccelMovementVid]
  - The tank accelerates normally, but now also decelerates more naturally when movement input stops.  
  - This gives the movement a heavier and more grounded feeling.

## Camera

- For the camera, I want the following quality of life updates:  
  - [FOV zooms out when moving][CamVid1], giving the illusion of action  
  - [The camera moves around][CamVid2], making the tank feel more dynamic.  
  - There is no indicator of charging time for the user. Im thinking [zooming out the camera][CamVid3] should do the trick. 

 

[RotVid1]: videos/instantRotation.mp4

[RotVid2]: videos/SLERPRotation.mp4

[SQUADFlickVid]: videos/Flicker_SQUAD.mp4

[RotVid4]: videos/SQUADRotation.mp4

[DirectMovementVid]: videos/instantMovementt.mp4

[AccelMovementVid]: videos/AccelMovementn.mp4

[AccelDeaccelMovementVid]: videos/AccelDeaccelMovement.mp4

[CamVid1]: MoveZoomOut.mp4

[CamVid2]: CameraLookAhead.mp4

[CamVid3]: shootZoomOut.mp4
