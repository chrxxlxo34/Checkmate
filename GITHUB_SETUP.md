# Checkmate GitHub Actions setup

The repository is `chrxxlxo34/Checkmate`. The workflow at `.github/workflows/android-release.yml` builds with Unity `6000.0.40f1`, runs a hosted Android emulator launch smoke test, and publishes the APK when a version tag such as `v0.1.0` is pushed.

## Unity licensing

Open **Settings → Secrets and variables → Actions** and configure the secrets required by the current GameCI Unity licensing method. This workflow expects `UNITY_LICENSE`, `UNITY_EMAIL`, and `UNITY_PASSWORD`. Use the official guide at <https://game.ci/docs/github/activation> and never commit license data, passwords, or tokens into the repository.

## Build flow

1. Push a commit to `main` to trigger the workflow.
2. Open the repository's **Actions** tab and inspect the run.
3. After successful build and emulator smoke test, download the `Checkmate-Android-APK` artifact.
4. To publish a GitHub Release, push a new version tag, for example:

```bash
git tag v0.1.0
git push origin v0.1.0
```

The release job only runs for a `v*.*.*` tag and only after both build and emulator smoke-test jobs pass.

## Scope and limitations

A green build only confirms compilation and the configured launch smoke test. It does not establish that online multiplayer works or replace hands-on gameplay testing on a physical Android device. Online multiplayer is not implemented in the current source.
