# Salih home lifecycle regression

Run `dotnet run --project tests/SalihHomeLifecycle` from the repository root.

This executable compiles the production home page with real MAUI controls.
Network data and platform theme preferences are replaced with in-memory fixtures.
It rebuilds the home page 30 times, alternating themes, and verifies that the
price table is never left in two layouts. It also checks cached and empty prices.
Before the fix, the first rebuild fails the ownership assertion.

The calculator regression compiles the production calculator, tracks its actual
stateful controls, and verifies single layout ownership through 30 theme updates.
Before the calculator fix, this fails on the amount `Entry`. On the API 35
emulator, the first test APK also crashed on Calculator → Home → Calculator with
`java.lang.IllegalStateException: The specified child already has a parent`.
Clearing `Page.Content` did not remove controls from nested layouts; the calculator
now removes those controls from their previous layouts before reusing them.

This is a managed visual-tree regression test, not an Android device test.
Before release, repeatedly navigate Home → Prices → Home on Android, including
rapid taps, theme changes, and background/resume. Check `adb logcat` for crashes.

Validation in this workspace: Android Release AAB build succeeded using
`-p:Brand=SalihCetinkaya -p:GooglePlayRelease=true -f net10.0-android36.0`.
The generated Release APK was installed on the Pixel 7 API 35 emulator; Prices
and the returned Home screen were verified through UI Automator, with an empty
Android crash log. This does not replace physical-device testing or Play upload
signing. Version metadata remains 1.0.9 (15); a new Play upload needs a higher
version code and the registered upload key.

For the full tab matrix, launch the app on a 1080x2400 API 35 emulator, then run:

```powershell
./tests/SalihHomeLifecycle/AndroidNavigation.ps1 -Adb 'path/to/adb.exe'
```

The script locates tabs by text and bounds, verifies destination headings, and
requires the same process throughout all 20 directed pairs. It stops on failure.
Test2 uses build overrides `ApplicationDisplayVersion=1.0.10-test2` and
`ApplicationVersion=16`, retaining the previous test signing key for in-place
updates. These overrides do not change the Apple release metadata.

Test2 validation: all 20 directed tab pairs passed on Pixel 7 / Android API 35,
with the same application process throughout and an empty crash log afterward.
The installed manifest reported version `1.0.10-test2`, code `16`. APK signature
verification passed. The Release build completed with 0 errors and 140 existing
warnings (including obsolete APIs and XAML binding warnings).
