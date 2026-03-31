# FEMBOY!!

「曲を作ったけど、動画制作がだるすぎる!!!」<br>
という人向けに簡単に動画が作成できて作曲に集中することができるツールです。<br>

This tool helps you quickly create simple videos from your music so you can focus on composing instead of video production.

- [FEMBOY!!](#femboy)
  - [対応OS](#対応os)
  - [Supported OS](#supported-os)
  - [前提インストールツール](#前提インストールツール)
  - [Prerequisite](#prerequisite)
  - [使い方](#使い方)
    - [Windowsの場合](#windowsの場合)
    - [Usage (Windows)](#usage-windows)
    - [Ubuntuの場合](#ubuntuの場合)
    - [Usage (Ubuntu)](#usage-ubuntu)
  - [title.jsonの書き方](#titlejsonの書き方)
  - [title.json format](#titlejson-format)
  - [その他](#その他)
  - [Other](#other)


## 対応OS

- Windows 11(10もおそらく動くと思う)
- Ubuntu(Ubuntu派生ディストリビューションも動くと思う)
- Mac OSはまだ未定(開発マシンとして所有していないので、検証ができないです…)

## Supported OS

- Windows 11 (Windows 10 likely works as well)
- Ubuntu (and Ubuntu-derived distributions)
- macOS: not officially supported yet (no test machine available)

## 前提インストールツール

ffmpegをダウンロードしてインストールしてください。<br>
https://ffmpeg.org/download.html

とはいいつつも、ツール実行の時点で入ってなかったらインストールコマンドが表示されます。

## Prerequisite

Please install ffmpeg before running this tool:
https://ffmpeg.org/download.html

If ffmpeg is not found when you run the tool, it will display installation commands appropriate for your OS.

## 使い方

### Windowsの場合

ダウンロードしたファイルダブルクリックしてください。<br>
実行されるとMusicフォルダが作成されるので、そこに音楽ファイルを入れてください。<br>

再度実行すると次はPicフォルダが作成されるので、次はそこに画像を入れてください。

二回目移行はこの処理は発生しません。(自分で画像と音楽を入れ替える必要はあります。)

### Usage (Windows)

Double-click the downloaded executable. The first run will create a `Music` folder—place your audio files there.
On the second run the tool will create a `Pic` folder—place your image files there.
After both folders exist, you can replace images and audio files as needed and run the tool again.

### Ubuntuの場合

ダウンロードしたファイルに以下のコマンドで権限を付与してください。<br>
```bash
chmod +x Femboy
```
そのあとに`./Femboy`と入力してください。<br>

実行されるとMusicフォルダが作成されるので、そこに音楽ファイルを入れてください。<br>

再度実行すると次はPicフォルダが作成されるので、次はそこに画像を入れてください。

二回目移行はこの処理は発生しません。(自分で画像と音楽を入れ替える必要はあります。)

### Usage (Ubuntu)

Make the downloaded file executable:

```bash
chmod +x Femboy
```

Then run:

```bash
./Femboy
```

The tool will create a `Music` folder on first run—put your audio files there. On the second run it will create a `Pic` folder—put images there. After both folders exist, the tool won’t create them again and you can swap files manually.

## title.jsonの書き方

```json
{
    "SoundInfo": {
        "ComposerName": "user", -> ユーザー名を書いてください
        "SongName": "song" -> 曲名を書いてください
    }
}
```

これを設定すると動画に曲名と作曲者名が設定されます。<br>
なお、<br>
```json
{
    "SoundInfo": {
        "ComposerName": "",
        "SongName": ""
    }
}
```
とすると曲名もユーザー名も設定されません。

## title.json format

```json
{
    "SoundInfo": {
        "ComposerName": "user",
        "SongName": "song"
    }
}
```

Set these fields to include the composer name and song title in the generated video. If you leave them empty:

```json
{
    "SoundInfo": {
        "ComposerName": "",
        "SongName": ""
    }
}
```

then the composer and song name will not be displayed in the video.

## その他

本ソフトはフリーソフトです。

このソフトを使用したことで問題が発生しても当方では責任は取れませんのでご了承ください。

## Other

This software is provided as freeware.

The author is not responsible for any damage or issues caused by using this software.