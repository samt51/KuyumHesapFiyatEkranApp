# Salih Cetinkaya 1.0.10 (16)

- Package: `com.salihcetinkaya.kuyumhesap`
- Target: existing closed-testing `alpha` track; do not stop the test or replace testers.
- Fixes: Home price-table ownership and repeated Calculator navigation crashes.
- Verification: 30 Home rebuilds and 30 Calculator theme updates passed. All 20
  directed bottom-tab pairs passed on the API 35 emulator with an empty crash log.
- Release notes: `Yenilikler.txt`.
- ARM64 Release publish succeeded. `bundletool validate` passed for
  `SalihCetinkaya_1.0.10_16_UNSIGNED.aab`; manifest confirms version 1.0.10,
  version code 16, target SDK 36, and the package ID above.
- SHA-256: `805480B5599C594C970EB565293DF323C19A018555BDC0DEA1491B31E0690363`.
- `jarsigner -verify` confirms the bundle is unsigned; it is not ready to upload.

## Upload key reset required

The upload certificate in the previous 1.0.9 (15) AAB has SHA-256:

`2E:BD:9C:9B:F1:FA:33:A7:D1:BA:0D:5A:1D:A5:93:54:06:E6:3C:2F:B3:E4:8C:1F:80:6E:40:6C:B9:27:05:24`

The available Windows keystore has a different certificate:

`82:E8:7F:F9:AD:E2:4D:A9:21:76:3E:72:C8:A1:0F:55:21:74:44:48:41:A8:76:FF:4F:18:20:04:D3:46:25:5A`

The user authorized continuing with the available Windows upload key because
the original key remains on the old computer. No additional private key was
generated. The Windows key is RSA 2048-bit, alias `salih-upload`, valid until
2053-11-12; private key and password remain outside the repository.

- `upload_certificate.pem`: public certificate for the Play upload-key reset request.
- `SalihCetinkaya_1.0.10_16_UPLOAD_RESET_REQUIRED.aab`: signed using that same key.
- `jarsigner -verify` passed; the bundle certificate SHA-256 matches the PEM.
- The self-signed certificate warning is expected for this upload certificate.

In Play Console, open Play app signing, find Upload key certificate, request an
upload key reset, and attach `upload_certificate.pem`. Explain that the previous
upload key is on the old computer and is unavailable on the current computer.
Wait until Play confirms the new upload certificate is active before uploading
the signed AAB to the existing alpha track. Keep the current test running.

No reset request or Play Console upload has been submitted by the assistant.
Official instructions: https://support.google.com/googleplay/android-developer/answer/9842756
