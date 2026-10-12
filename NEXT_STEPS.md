# Next milestone checklist

## Build the Android prototype
1. Open this project in Unity 6 `6000.0.40f1` with Android Build Support.
2. Allow Package Manager to resolve dependencies.
3. Run **Checkmate → Tests → Run Chess Rule Smoke Tests**.
4. Run **Checkmate → Build → Build Android APK**.
5. Install the generated `Builds/Android/Checkmate.apk` on an Android device and test touch controls.

## Release gates
- [ ] Unity compilation and rule smoke tests pass.
- [ ] Android APK builds successfully.
- [ ] APK installs and launches in the CI emulator.
- [ ] Manual touch/gameplay testing on a physical Android device.
- [ ] Choose and implement a backend before advertising online multiplayer.
- [ ] Add matchmaking, authenticated player identity, server-side legal move validation, reconnect handling, and abuse prevention before online release.

The current project is a local prototype, not a completed online multiplayer release.
