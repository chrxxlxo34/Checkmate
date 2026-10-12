# Checkmate QA review and improvement backlog

## Present in source (not yet Unity-verified)

- Procedural 3D 8×8 chessboard and geometric pieces.
- Legal move filtering, check/checkmate/stalemate, castling, en passant, automatic queen promotion, threefold repetition, 50-move counter, and basic insufficient-material checks.
- Mouse/touch picking, UI tap blocking, legal destination highlights, selected-square highlight.
- Landscape HUD, placeholder player cards, move counter, new game and board flip.
- Editor smoke tests and an Android build entry point.
- GitHub Actions build, emulator launch smoke test, and tagged-release workflow.

## 25 remaining work items

### Multiplayer and identity
1. Choose a multiplayer backend and deployment model.
2. Add player authentication and secure session handling.
3. Build player profiles with avatars, usernames, and connection state.
4. Implement private rooms and invite/join codes.
5. Implement matchmaking and queue cancellation.
6. Synchronize turns and match state over a network protocol.
7. Validate every online move authoritatively on the server.
8. Handle disconnects, reconnects, timeouts, and abandoned matches.
9. Add basic abuse prevention, rate limits, and server-side result records.

### Chess and match flow
10. Add a promotion chooser for queen, rook, bishop, or knight.
11. Complete FIDE draw adjudication, claims, and acceptance/rejection UX.
12. Add chess clocks with authoritative online timing.
13. Add algebraic move history and navigation.
14. Add PGN import/export.
15. Add resign, draw offer/response, rematch, and result summary.
16. Save local match history and settings safely.

### UI and presentation
17. Replace placeholder player cards with real profile data and ratings.
18. Add last-move indicators, check warnings, and captured-piece trays.
19. Replace primitive pieces with optimized cohesive models and tuned lighting/materials.
20. Add accessible settings for audio, haptics, orientation, color-blind highlights, and text sizing.
21. Improve safe-area, notch, and small-screen layouts.

### Reliability and release quality
22. Run the Unity compile/build and fix all compiler or package issues.
23. Expand rule tests with edge cases and perft/randomized validation.
24. Profile and test touch, lifecycle, performance, and memory on physical Android devices.
25. Add crash diagnostics, version metadata, and repeatable release verification.

## Verification status

- Unity compilation and editor tests have not yet been run in this workspace because Unity is not installed here.
- No APK is currently available from this source-only workspace.
- CI requires valid Unity licensing secrets before it can produce a build.
- Online multiplayer, accounts, matchmaking, and server-authoritative validation are not implemented. This is an explicit backlog, not a claim of completion.
