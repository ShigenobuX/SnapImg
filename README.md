# SnapImg

SnapImg は Windows 向けの画像フォーマット変換アプリです。WPF と .NET 8 で動作し、実行ファイル名は `snapimg.exe` です。

## 主な機能

- SVG のラスタライズと、HEIC / HEIF / AVIF / JPG / PNG / WEBP / BMP / TIFF / GIF / ICO の読み込み
- JPG / PNG / WEBP / AVIF / BMP への変換
- 複数ファイルの一括変換、フォルダ追加、ドラッグ＆ドロップ
- JPEG / WEBP の画質設定、Exif 情報の保持または削除
- 幅・高さを指定したリサイズとアスペクト比の維持
- 画像プロパティとサムネイルの表示
- Windows エクスプローラーの右クリックメニュー、クリップボードコピー

SVG は 96 dpi を基準に読み込み、ピクセル画像へ変換します。必要な出力寸法はリサイズ設定で指定できます。PNG は透明背景を保持し、JPG は透明部分を白で塗りつぶします。

## 必要な環境

- Windows 10 / 11
- .NET 8 SDK（ビルドする場合）

画像処理には NuGet パッケージ `Magick.NET-Q8-AnyCPU` を使用しています。

## ビルド

```powershell
cd C:\work\SnapImg
dotnet restore .\imgconv.csproj
.\build.bat
```

Release ビルドは `bin\Release\net8.0-windows\snapimg.exe` に生成されます。配布用の自己完結ファイル一式を作るには次を実行します。

```powershell
.\build_publish.bat
```

配布時は `publish-folder` 内のファイル一式が必要です。インストーラーを作る場合は Inno Setup 7 をインストールしてから `build_installer.bat` を実行してください。

## 基本操作

1. 「ファイルを追加」または「フォルダを追加」で画像を登録します。
2. 出力形式や画質、Exif、リサイズ、保存先を設定します。
3. 「変換を開始」を押します。

SVG は PNG / JPG のほか、WEBP / AVIF / BMP にも変換できます。JPG では透明部分が白背景になります。

詳しい操作は [ユーザーマニュアル](USER_MANUAL.md) を参照してください。

## コマンドライン

```powershell
.\snapimg.exe --convert-to png "C:\Images\illustration.svg"
.\snapimg.exe --convert-to jpg "C:\Images\photo.heic"
.\snapimg.exe --register-menu
.\snapimg.exe --unregister-menu
```

入力形式は SVG、HEIC / HEIF、AVIF、JPG / JPEG、PNG、WEBP、BMP、TIFF、GIF、ICO。出力形式は `jpg`、`png`、`webp`、`avif`、`bmp` です。

## 設定ファイル

ウィンドウサイズは `%LocalAppData%\imgconv\settings.json` に保存されます。
