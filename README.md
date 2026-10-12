# Checkmate — Unity Android Chess Prototype

Unity 6 source project for a stylized 3D chess board and landscape match HUD.

## Current scope

This revision is an offline, same-device chess prototype. It includes procedural 3D board/pieces, legal move filtering, check/checkmate/stalemate, castling, en passant, automatic queen promotion, simplified draw detection, and touch/mouse input. It is **not online multiplayer yet**; player cards are placeholders.

## Open and build

1. Install Unity 6 `6000.0.40f1` with Android Build Support, SDK/NDK, and OpenJDK.
2. Open this repository as a Unity project and let packages resolve.
3. Run **Checkmate → Tests → Run Chess Rule Smoke Tests** in the Editor.
4. Run **Checkmate → Build → Build Android APK** to create `Builds/Android/Checkmate.apk`.

The CI workflow is intended to build an APK, run a hosted emulator launch smoke test, and publish a tagged release. It requires correctly configured Unity license secrets. See `GITHUB_SETUP.md`.

## Known gaps

Online matchmaking/networking and server-side validation, real player accounts/profiles, promotion choice, chess clocks, full draw-claim handling, PGN/move history, audio/haptics, and physical-device testing are not complete. See `QA_AND_ROADMAP.md` for the backlog and verification status.
