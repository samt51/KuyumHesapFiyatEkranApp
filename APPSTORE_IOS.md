# iOS App Store Build

This MAUI project is prepared for App Store builds from a Mac with Xcode 26 or newer.

## Mac mini setup

1. Install Xcode 26 or newer from Apple.
2. Install the latest .NET SDK that supports `net9.0-ios`.
3. Install or update MAUI workloads:

```bash
sudo dotnet workload install maui
dotnet workload update
```

4. In Apple Developer, create an iOS App Store provisioning profile for:

```text
com.aslankuyumculuk.kuyumhesap
```

5. Install the Apple Distribution certificate and provisioning profile on the Mac.

## Archive

From the repository root on the Mac:

```bash
export CODESIGN_KEY="Apple Distribution: Company Name (TEAMID)"
export CODESIGN_PROVISION="Provisioning Profile Name"
bash scripts/publish-ios-appstore.sh
```

The build output is created under:

```text
KuyumHesapFiyatEkran.UI/bin/Release/net9.0-ios/ios-arm64/publish
```

Upload the generated archive/package with Xcode Organizer or Apple's Transporter app.
