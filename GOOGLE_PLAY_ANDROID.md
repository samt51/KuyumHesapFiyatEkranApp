# Salih Çetinkaya Android yayını

- Paket: `com.salihcetinkaya.kuyumhesap`
- Sürüm: `1.0.9`, sürüm kodu: `15` (Play Console'da görülen önceki en yüksek kod: `7`).
- Google Play derleme hedefi: `net10.0-android36.0`.
- Apple ve diğer marka derlemelerinde `GooglePlayRelease` kullanılmaz.

## Derleme

Projeye özel .NET 10 ve `maui-android` iş yükü `.play-release/dotnet` altında tutulur.
Android SDK ve JDK konumları `.play-release/android-sdk` ve `.play-release/jdk` olarak varsayılır.
Gerekirse `SALIH_DOTNET`, `SALIH_ANDROID_SDK`, `SALIH_JAVA_SDK` değişkenleriyle değiştirilebilir.

```bash
bash scripts/publish-salih-google-play.sh
```

Derleme çıktısındaki paketler yayın anahtarıyla imzalanıp doğrulanmadan yüklenmemelidir.

## Yeni yükleme anahtarı

Anahtar proje dışında `~/Documents/AndroidKeys/salih-cetinkaya-upload-20260916.jks` dosyasındadır.
Alias: `salih-upload`. Parolayı kaynak koda veya komut geçmişine yazmayın.
Sertifikanın SHA-256 parmak izi:

```text
2E:BD:9C:9B:F1:FA:33:A7:D1:BA:0D:5A:1D:A5:93:54:06:E6:3C:2F:B3:E4:8C:1F:80:6E:40:6C:B9:27:05:24
```

16 Eylül 2026'da yeni yükleme anahtarı için Play Console sıfırlama talebi gönderildi.
18 Eylül 2026 Play Console ekranında bekleyen talep kalktı ve yeni sertifikanın SHA-1 parmak izi yerel sertifikayla eşleşti. Yeni yükleme anahtarı tanımlandı.
Üretime erişim ve incelemeye gönderim ayrıca Play Console'dan tamamlanır.

## İmzalama ve doğrulama

```bash
bash scripts/sign-salih-google-play.command
```

Bu komut yeni anahtar parolasını gizli ister; parolayı diske veya komut geçmişine yazmaz.
İmzalı dosya `Builds/SalihCetinkaya/Android/1.0.9/SalihCetinkaya_1.0.9_15_GooglePlay.aab` konumuna yazılır.

18 Eylül 2026 doğrulamaları:
- Release AAB derlemesi tamamlandı; bundletool validate başarılı.
- Manifest: paket `com.salihcetinkaya.kuyumhesap`, sürüm `1.0.9`, kod `15`, hedef API `36`.
- Paket ARM64 (`arm64-v8a`) içerir; 32 bit ARM ve x86 cihazları içermez.
- 108 yerel kütüphanenin ELF LOAD segmentleri 16 KB hizalaması için kontrol edildi.
- Android cihaz/emülatör üzerinde çalışma testi henüz yapılmadı; yayımlamadan önce dahili test önerilir.
