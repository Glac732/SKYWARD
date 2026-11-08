# SKYWARD

SKYWARD is an original Unity 6.3 flight-simulator project. The current development target is a single-engine trainer and one runway environment. It is an experimental game prototype, not certified flight-training software.

## Open and play

1. Open this repository folder in Unity **6000.3.0f1**.
2. Allow the editor to import the project and resolve packages.
3. Select **SKYWARD → Create / Rebuild Demo Scene**.
4. Open `Assets/Scenes/SKYWARD_Demo.unity`.
5. Press Play.

## Trainer controls

| Input | Action |
|---|---|
| W / S | Pitch |
| A / D | Roll |
| Q / E | Rudder |
| Left Shift / Left Ctrl | Increase / decrease throttle |
| Space | Wheel brakes |
| B | Parking brake toggle |
| G | Landing gear toggle |
| F | Cycle flap settings |
| X | Engine toggle |
| C | Cockpit / chase camera |
| Right mouse + move | Camera look |
| Mouse wheel | Chase camera zoom |
| Esc | Pause |

## Current implementation focus

- Body-axis aerodynamic model with dynamic pressure, lift, induced/profile drag, post-stall lift loss, sideslip force, control moments and rate damping.
- Propeller thrust decreases with airspeed.
- Three WheelColliders with suspension, friction, nose-wheel steering and brake torque.
- Runway, threshold and touchdown-zone markings, taxiway, apron, runway lights, hangar and tower.
- Live telemetry: indicated airspeed, ground speed, altitude above runway, vertical speed, heading, pitch, bank, angle of attack, sideslip, lift coefficient, throttle, fuel, gear, flaps and flight phase.

## Verification status

Unity Editor is required to compile and run the scene and Unity Test Framework tests. No Unity Editor or C# compiler is bundled in this repository. Source-level checks are not a substitute for an in-editor take-off/landing test. Aircraft coefficients are estimates for game development and are not validated real-aircraft data.
