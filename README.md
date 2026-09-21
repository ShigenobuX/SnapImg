<<<<<<< HEAD
# SnapImg
=======
# SnapImg

Windows 向けの画像フォーマット変換アプリです。WPF と .NET 8 で動作し、生成される実行ファイル名は `snapimg.exe` です。

## 主な機能

- HEIC / HEIF、PNG、JPG / JPEG、WEBP、BMP、TIFF、GIF、ICO の読み込み
- JPG、PNG、WEBP、BMP への変換
- 複数ファイルの一括変換
- ファイル・フォルダの追加、ドラッグ＆ドロップ
- JPEG / WEBP の画質設定
- Exif 情報の保持または削除
- 幅・高さを指定したリサイズ、アスペクト比の維持
- 保存先フォルダの指定
- 同名ファイルへの連番付与
- Windows エクスプローラーの右クリックメニューへの登録
- コマンドラインからの変換

## 必要な環境

- Windows 10 / 11
- .NET 8 SDK（ビルドする場合）

画像処理には NuGet パッケージ `Magick.NET-Q8-AnyCPU` を使用しています。

## ビルド

PowerShell でプロジェクトのフォルダへ移動し、次を実行します。

```powershell
cd C:\work\SnapImg
dotnet restore .\imgconv.csproj
dotnet build .\imgconv.csproj -c Release
```

ビルド後の実行ファイル：

```text
C:\work\SnapImg\bin\Release\net8.0-windows\snapimg.exe
```

バッチファイルも利用できます。

```powershell
.\build.bat
```

ビルドは Release 構成で行われ、`bin\Release\net8.0-windows\snapimg.exe` に生成されます。

依存ランタイムを含む配布用ファイル一式を作成する場合：

```powershell
.\build_publish.bat
```

`publish-folder` フォルダ内のファイルをすべて配布してください。`snapimg.exe` だけを取り出して配布することはできません。WPF と画像処理ライブラリが使用するネイティブ DLL も必要です。

## GUI の使い方

1. `snapimg.exe` を起動します。
2. 「ファイルを追加」または「フォルダを追加」を押します。
3. 変換形式、画質、Exif、リサイズ、保存先を設定します。
4. 「変換を開始」を押します。

画像ファイルやフォルダを、画面上のファイル一覧へドラッグ＆ドロップして追加することもできます。

## 右クリックメニュー

画面右上の「設定」ボタンから登録できます。「はい」を選ぶと JPG / PNG / WEBP / BMP の登録形式を選択できます。「いいえ」を選ぶと登録を解除します。

Windows 11 では「その他のオプションを確認」に表示される場合があります。

## コマンドライン

```powershell
.\snapimg.exe --convert-to jpg "C:\path\to\image.heic"
.\snapimg.exe --convert-to png "C:\path\to\photos"
.\snapimg.exe --register-menu
.\snapimg.exe --unregister-menu
```

対応する出力形式は `jpg`、`png`、`webp`、`bmp` です。

## 設定ファイル

ウィンドウサイズは次の場所に保存されます。

```text
C:\Users\<ユーザー名>\AppData\Local\imgconv\settings.json
```

## インストーラー作成

Inno Setup 7 をインストールした後、まず配布用ファイルを作成します。

```powershell
.\build_publish.bat
```

`build_publish.bat` は `publish-folder` に配布用ファイルを生成します。続いて `build_installer.bat` を実行します。

```powershell
.\build_installer.bat
```

Inno Setup の場所は次の候補から自動的に検索されます。

- `C:\Program Files\Inno Setup 7\ISCC.exe`
- `C:\Program Files (x86)\Inno Setup 7\ISCC.exe`
- `%LocalAppData%\Programs\Inno Setup 7\ISCC.exe`

インストーラーは `InstallerOutput` フォルダに生成されます。

## プロジェクト構成

- `imgconv.csproj`：.NET 8 WPF プロジェクト
- `MainWindow.xaml` / `MainWindow.xaml.cs`：画面と操作処理
- `ImageConverterService.cs`：画像変換・リサイズ
- `ContextMenuService.cs`：右クリックメニュー登録・解除
- `CommandLine.cs`：コマンドライン処理
- `App.xaml` / `App.xaml.cs`：アプリケーション起動処理
- `main.py`：旧 Python 版のソース（C# 版の実行には使用しません）
>>>>>>> e995c48 (Initial SnapImg C# WPF application)
