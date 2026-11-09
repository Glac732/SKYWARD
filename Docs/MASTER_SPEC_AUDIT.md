# SKYWARD — Master Specification Audit

**Audit date:** 2026-11-08  
**Specification reviewed:** Project SKYWARD, 48 sections, 2,221 lines  
**Branch reviewed:** `main`  
**Reviewed revision at time of audit:** `65ecbae4d707a4aaa1e7288e7e0338b6bf390a7f` (README/status update; preceding code fixes are in the same branch history)  
**Overall result:** **NOT RELEASE READY — SOURCE PROTOTYPE ONLY**

## 1. Executive verdict

The current repository remains an early Unity source prototype, not the complete application requested by the master specification. Since the first audit, source additions have introduced a runtime front end, aircraft performance presets, flight-school lessons, mission challenges, career statistics, three procedural airport layouts, weather effects and Android touch controls.

It still does **not** provide a polished, finished, full flight simulator. The aircraft presets share one generated airframe, airports are variations on one runway layout, and there are no production aircraft/environment assets or verified online services. Unity compilation, playmode tests and platform builds remain unverified.

The prompt explicitly requires that features are not represented as available unless implemented and tested. This report therefore marks only source-visible systems as implemented; it does not infer that they work at runtime.

## 2. Scope actually present in the repository

| Area | Current evidence | Audit status |
|---|---|---|
| Unity project foundation | Unity 6000.3.0f1 project metadata, package manifest, C# scripts, assembly definitions | Present; Editor import/compile unverified |
| Aircraft flight model | Rigidbody forces and moments, estimated lift/drag/thrust, stall approximation, control moments | Prototype source present; runtime behaviour unverified |
| Trainer configuration | One `AircraftConfig` with trainer-like geometry/performance parameters | One configuration only |
| Ground handling | Three WheelColliders, spring/damper/friction setup, steering and braking code | Source present; runtime behaviour unverified |
| Aircraft controls | Legacy keyboard input, throttle, engine/gear/flaps/brakes, public control methods | Partial; no remapping or device abstraction |
| Flight telemetry | IAS, ground speed, AGL estimate, vertical speed, heading, pitch, bank, AoA, sideslip, fuel, flap/gear and phase values | Partial live values; definitions/accuracy require runtime validation |
| Cameras | Cockpit and chase toggle, mouse look, chase zoom | Partial; other requested views/features absent |
| Environment | One procedural runway/taxiway/apron, buildings, lights and primitive distant hills | Prototype only |
| HUD/cockpit | Screen overlay and seven textual world-space instrument readouts | Rudimentary; not production UI or a real cockpit |
| Persistence | Small PlayerPrefs profile with backup key, version field and basic sanitisation | Partial; not full profile/progression system |
| Tests | EditMode and PlayMode test source files | 22 cases authored; none run in this environment |
| CI | GitHub Actions source checks plus a new Unity test/build workflow | Source checks passed; Unity workflow requires configured licence and has not yet been proven successful |
| Packaging | Unity project source only | No executable release artifacts |

## 3. Corrections made during this audit

The following targeted changes were committed to `main`:

1. **Sideslip force direction:** corrected the local lateral aerodynamic force so positive local-X sideslip produces a restoring force rather than reinforcing the slip.
2. **Independent control axes:** replaced combined-vector magnitude clamping with per-axis clamping. Pitch, roll and yaw no longer reduce each other when used together.
3. **Regression coverage:** added a restoring lateral-force test and a control-axis clamping test.
4. **uGUI assembly dependency:** added `UnityEngine.UI` to the runtime assembly references, which the HUD and cockpit UI require.
5. **Runtime font lookup:** replaced the legacy `Arial.ttf` built-in font request with `LegacyRuntime.ttf`.
6. **URP setup:** added URP and uGUI package dependencies and an Editor setup routine that creates/assigns a Universal Render Pipeline asset when the demo scene is generated.
7. **Player scene registration:** made the generated demo scene register in Editor Build Settings so it is included in a later player build.
8. **Material reuse:** cached procedural airport and trainer materials and used a URP Lit shader with a Standard fallback.
9. **Wheel mesh alignment:** corrected the procedural wheel mesh orientation and its WheelCollider visual pose alignment.
10. **Flight-time save accounting:** changed pause/quit persistence to add only newly elapsed session time, preventing repeated save events from counting the same flight seconds twice.
11. **Build automation:** added explicit Unity Editor build entry points that generate the URP demo scene and build Windows x64 and Android ARM64 players, with build-result/error checks.
12. **Unity CI pipeline:** added a GitHub Actions workflow to run EditMode/PlayMode tests, build both player targets, upload ZIP/APK artifacts and preserve test reports. It requires an activated Unity licence in the repository's `UNITY_LICENSE` Actions secret.
13. **Startup smoke test:** added a PlayMode test for creation of the aircraft, three landing wheels, airport, main camera, HUD, cockpit instruments and configured render-pipeline asset.
14. **Graphics diagnostics:** added an offscreen 960×540 render capture, non-blank frame check and 120-frame timing sample; the screenshot is saved into the Unity test artifacts when the runner has a graphics device.
15. **Runtime front end:** added main menu navigation, aircraft presets, flight-school lessons, mission board, career statistics and basic achievement flags.
16. **Airport/environment variants:** added three procedural airport layouts plus cloud clusters, lighting, fog, wind and rain effects.
17. **Aircraft/cockpit visuals:** replaced the trainer fuselage and wing primitives with generated meshes and added a basic cockpit dashboard, yoke, seat and instrument faces.
18. **Mobile/audio foundations:** added Android touch controls, throttle-responsive procedural engine audio and procedural wind/weather ambience.
19. **Regression coverage:** added airport/weather environment smoke coverage and expanded startup checks for custom meshes and engine audio.

These are source edits. They have not yet been validated by importing/compiling/running the project inside Unity.

## 4. Master prompt coverage review

The following table follows the master prompt's 48-section structure. “Partial” means a limited prototype or source scaffold exists, not that the entire section has been completed.

| Master spec section | Required outcome | Current status |
|---|---|---|
| 1. Role and mission | Actual playable commercial-quality flight simulator | **Not met** — repository is an experimental source prototype |
| 2. Game identity | Original polished aviation identity and release name clearance | **Partial** — working name only; no commercial name/trademark clearance |
| 3. Development philosophy | Functional systems, honest feature states, measured performance | **Partial** — source structure and test intent exist; runtime validation and measurements absent |
| 4. Technical architecture | Modular engine, simulation, world, UI, services, tests | **Partial** — small modular C# layout; most modules do not exist |
| 5. Visual direction | Premium aviation-realism art, natural sky, PBR environment | **Not met** — primitive geometry/materials; no production art or visual review |
| 6. Aircraft collection | Multiple meaningfully distinct aircraft, preview and selection | **Partial** — three physics/livery presets on one procedural airframe; no separate detailed aircraft models |
| 7. Flight physics | Stable force/moment model and aircraft-specific behaviour | **Partial** — one estimated trainer model; no Unity validation or validated aircraft data |
| 8. Flight control system | Keyboard, mouse, touch, controller, joystick, rebinding | **Partial** — legacy keyboard/mouse plus Android touch overlay; no remapping or joystick/gamepad support |
| 9. Take-off, flight and landing | Complete preflight, taxi, take-off, free flight, approach, landing results | **Partial** — core controller and WheelCollider source; complete flight loop unverified; no preflight/results flow |
| 10. Camera system | Eight camera modes, transitions, collision avoidance and settings | **Partial** — chase and cockpit only |
| 11. Cockpit/instruments | Detailed 3D cockpit and aircraft-appropriate functional gauges | **Partial** — generated cockpit dashboard, seat, yoke, instrument faces and live text instruments; not an aircraft-authentic detailed cockpit |
| 12. Flight HUD | Navigation, objectives, configurable professional HUD | **Partial** — basic telemetry panel; navigation, objective logic and customisation absent |
| 13. World/environment | Scalable streamed world with LOD/culling and rich biomes | **Partial** — three generated airport surroundings with coastal/highland/desert variations; no streaming, LOD or world-scale biomes |
| 14. Airport system | Functional airport catalog and selection | **Partial** — three selectable procedural airport variants sharing a common runway layout; no geographically accurate airport catalog |
| 15. Weather/atmosphere | Visual and aerodynamic weather presets, rain/fog/cloud/wind | **Partial** — menu presets, wind, cloud clusters, fog and rain particles; not a full meteorological simulation |
| 16. Lighting/sky | Realistic sky, clouds, atmospheric effects, day/night | **Partial** — procedural cloud clusters, directional sun, ambient sky tint and fog; no time-of-day cycle or production sky |
| 17. Aircraft animations | State-driven surfaces, flaps, gear, controls, lights and doors | **Partial** — propeller/wheels plus input-driven flaps, ailerons, elevators and rudder; no detailed gear/door/light animation |
| 18. Sound | Spatial aircraft/environmental sound and volume groups | **Partial** — procedural throttle-responsive engine loop and wind/weather ambience; no authored spatial sound library or full volume groups |
| 19. Game modes | Free flight, training, landing challenge, time trial, career | **Partial** — free flight, four lessons, six missions and basic career page; no full time-trial/career progression |
| 20. Missions/objectives | Real objective state machine, progress, reward, failure | **Partial** — six telemetry-based mission completion checks; no full objective sequencing, rewards or failure/results flow |
| 21. Player/profile/progression | Full profile, XP, levels, records, unlocks | **Partial** — small local profile and totals only |
| 22. Aircraft customisation | Real 3D livery/customisation preview and save | **Not met** |
| 23. Main menu | Premium 3D menu with working navigation | **Partial** — runtime uGUI main menu with working navigation; not a polished 3D front end |
| 24. Startup/loading | Robust startup stages and honest progress/error recovery | **Not met** |
| 25. UI design system | Complete professional UI components and navigation | **Not met** — basic runtime-generated uGUI text |
| 26. Settings | Graphics, audio, controls, gameplay, accessibility persisted | **Partial** — weather, master volume and basic quality controls; no control mapping/accessibility suite |
| 27. Mobile | Touch controls, responsive UI, platform profiling | **Partial** — Android touch-control overlay and scalable canvas; no device build or hardware profiling |
| 28. Desktop/laptop | Player build, display options, controller support and hardware profiles | **Not met** — no build artifacts or measured hardware support |
| 29. Multiplayer | Real sessions, authentication, sync/reconnect/server validation | **Not implemented** — correctly not exposed as available |
| 30. Backend/database | Secure online services and migrations if online features are built | **Not implemented** — no backend; local prototype does not require it |
| 31. Save system | Full versioned profile, atomic writes, recovery and migrations | **Partial** — PlayerPrefs JSON + backup key; limited schema and no migration pipeline |
| 32. Performance/FPS | Measured frame-time, 1% lows, memory, loading, thermal tests | **Not met** — no profiler or target-device measurements |
| 33. Graphics quality | Very Low through Ultra with real rendering changes | **Not met** — no graphics-quality UI/profiles |
| 34. Anti-crash/error handling | Recovery and logging for failures | **Partial** — limited missing-config error and save fallback; no complete error strategy |
| 35. Automatic updates | Versioned content manifests, verified downloads and rollback | **Not implemented** |
| 36. Downloadable release | Windows/macOS/Linux/Android/iOS distributables and install docs | **Not met** — no player binaries, installers or APK/AAB |
| 37. Accessibility/localisation | Language and unit options, scalable/accessibility UI | **Not implemented** |
| 38. First-time experience | Interactive onboarding and tutorial stages | **Not implemented** |
| 39. Flight results/statistics | Honest flight summary from accumulated simulation data | **Partial** — touchdown telemetry, landing score/debrief and basic career totals; no full flight report or export |
| 40. Achievements | Data-validated unlocks and rewards | **Partial** — local first landing, five landings, soft touchdown and ten-hour flags; no reward/unlock economy |
| 41. Quality assurance | Editor tests, full gameplay/device/build/performance matrix | **Partial** — 22 test cases authored but unrun; no gameplay/device test evidence |
| 42. Anti-placeholder | Missing systems honestly disabled and documented | **Partial** — README/report disclose prototype; basic primitives and limited prototype UI remain |
| 43. Code quality | Maintainable, tested, efficient release code | **Partial** — scripts separated; still uncompiled/unreviewed in Unity and many systems absent |
| 44. Development workflow | Implement/build/run/test/inspect/fix each milestone | **Not met** — no Unity Editor available for those runtime steps |
| 45. Project management | Progress, risks, tests and releases tracked | **Partial** — this audit provides status; no full release milestone tracking |
| 46. Final delivery | Complete source, playable build, assets and documentation | **Not met** — only source repository and setup guidance |
| 47. Final quality review | Professional expert review and fixes verified | **Not met** — no rendered build/device session to review |
| 48. Ultimate directive | Proudly presentable, genuinely playable simulator | **Not met yet** |

## 5. Visual quality assessment

The current procedural trainer is assembled from primitive Unity meshes (capsule, cubes, spheres and cylinders). The airport is largely cubes, spheres and simple materials. The cockpit strip is text on a world-space Canvas. This is suitable as a blockout or engineering prototype, but it is **not** at the premium aviation-realism standard described in the prompt.

Before the game is shown as a polished product, it needs a real aircraft mesh with correct proportions and materials, a designed cockpit/interior, readable integrated instruments, detailed terrain/airport assets, a proper sky/cloud solution, lighting review, aircraft animation, and original/licensed engine/ambient audio. The actual Unity Game view must be inspected at representative desktop and mobile aspect ratios.

## 6. Test and build evidence

### Source-level checks

The most recent GitHub Actions source-validation run at audit time completed successfully. It checks project JSON/package/assembly metadata, expected source files, selected startup setup invariants and C# delimiter balance.

That result only establishes that these source-level checks passed. It is **not** evidence that Unity compiled the project, that the scene loads, that any Unity Test Framework test passed, that take-off or landing works, or that a player build exists.

### Unity test inventory

Current authored test source contains:

- 11 aerodynamic EditMode tests.
- 2 save-system EditMode tests.
- 6 aircraft/ground/telemetry PlayMode tests, plus 1 full-session startup smoke test and 1 graphics/frame-timing smoke test.
- **21 authored test cases in total.**

**Execution status: NOT RUN.** Unity Editor and C# compiler tooling were unavailable in the development environment. Test names and assertions are not presented as passing evidence.

### Platform build inventory

| Target | Deliverable status |
|---|---|
| Windows | No EXE/MSI/installer generated |
| Android | No APK/AAB generated |
| iOS | No Xcode project/player generated |
| macOS | No app bundle generated |
| Linux | No player package generated |

## 7. Required release gate — do not skip

The project should not be represented to friends or distributed as a finished SKYWARD game until all applicable checks below have passed in Unity:

1. Open with Unity **6000.3.0f1**; let packages resolve; inspect Package Manager and Console.
2. Run **SKYWARD → Create / Rebuild Demo Scene**; verify the URP assets are created and assigned and the demo scene appears enabled in Build Settings.
3. Wait for compilation; resolve all errors and actionable warnings.
4. Run all EditMode and PlayMode tests; capture the actual test report.
5. Complete an uninterrupted flight: parking brake release, taxi/steering, take-off, climb, turns, stall recovery, descent, runway approach, touchdown, rollout and braking.
6. Verify instruments against Rigidbody/environment state and inspect wheel/gear/propeller behaviour visually.
7. Inspect graphics at 16:9 and a narrow aspect ratio; replace blockout art before a public demo.
8. Build a Windows development player and smoke-test on target laptop hardware; measure FPS/frame time and inspect logs.
9. Implement and separately test mobile UI/controls before claiming Android/iOS support.
10. Only after successful checks, create versioned release artifacts and publish a release with install/update instructions.

Unity Editor and platform build modules, appropriate hardware and an available Unity license are prerequisites for those checks.

## 8. Download guidance

The GitHub **Code → Download ZIP** archive provides the current Unity source project. It is not a ready-to-run Windows or mobile application. Open the extracted folder as a Unity project and follow the setup instructions in the root README.

## 9. Final audit decision

**Current release decision: NO-GO.**

The focused engineering fixes in this audit improve correctness and project setup, but they do not close the large feature, art, platform and verification gaps in the 48-section master specification. The most honest next milestone is not broad feature expansion: it is to import this exact revision in Unity, compile, run and debug the complete first-flight loop, and then replace the blockout presentation with production-quality art before calling it a demo.

No platform build, runtime test, graphic inspection or complete flight has been claimed as verified in this report.
