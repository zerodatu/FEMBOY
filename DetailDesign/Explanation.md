# 設計解説資料

## 概要について

DetailDesign/DetailDesign.webpに書かれている各処理について、処理内容などを記載します。

### do command

バイナリを実行することで処理が実行される。

### is insall ffmpeg

ffmpegがコンピューターにインストールされているか確認する。

- `Windows`の場合
  - installed -> 次の処理に進む
  - not installed -> ffmpegをインストールするコマンドをポップアップで提示する。
    - `winget install ffmpeg`を実行してください。
- `Ubuntu`の場合
  - installed -> 次の処理に進む
  - not installed -> ffmpegをインストールするコマンドをポップアップで提示する。
    - `sudo apt install ffmpeg`を実行してください。

### Check Resource

PicフォルダとMusicフォルダにリソースが格納されていることを確認する。

### is resource avaible

- Picフォルダにリソースが格納されていないとき
  - 画像ファイルが格納されていないので、格納してください。とポップアップで提示する。
- Musicフォルダにリソースが格納されていないとき
  - 音楽ファイルが格納されていないので、格納してください。とポップアップで提示する。
- PicフォルダもMusicフォルダもリソースが格納されていないとき
  - 音楽も画像も格納してください。とポップアップで提示する。

### Select upload site

アップロードサイトをXかYoutubeか選択して以降の処理を分岐する。

- Xを選んだ場合
