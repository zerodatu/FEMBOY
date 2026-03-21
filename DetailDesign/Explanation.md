# 設計解説資料

## 概要について

DetailDesign/DetailDesign.webpに書かれている各処理について、処理内容などを記載します。

### [do command]

バイナリを実行することで処理が実行される。

### [is insall ffmpeg]

ffmpegがコンピューターにインストールされているか確認する。

- `Windows`の場合
  - installed -> 次の処理に進む
  - not installed -> ffmpegをインストールするコマンドをポップアップで提示する。
    - `winget install ffmpeg`を実行してください。
- `Ubuntu`の場合
  - installed -> 次の処理に進む
  - not installed -> ffmpegをインストールするコマンドをポップアップで提示する。
    - `sudo apt install ffmpeg`を実行してください。

### [Check Resource]

PicフォルダとMusicフォルダにリソースが格納されていることを確認する。

### [is resource avaible]

- Picフォルダにリソースが格納されていないとき
  - 画像ファイルが格納されていないので、格納してください。とポップアップで提示する。
- Musicフォルダにリソースが格納されていないとき
  - 音楽ファイルが格納されていないので、格納してください。とポップアップで提示する。
- PicフォルダもMusicフォルダもリソースが格納されていないとき
  - 音楽も画像も格納してください。とポップアップで提示する。

### [Select upload site]

アップロードサイトをXかYoutubeか選択して以降の処理を分岐する。<br>
ポップアップで`Which your upload site is X or Youtube?`が表示される。<br>

- Xを選んだ場合
  - Xとして内部フラグを立てる。
  - その次に`text to image for X`を実行する。
- Youtubeを選んだ場合
  - Youtubeとして内部フラグを立てる。
  - その次に`text to image for Youtube`を実行する。

#### [text to image for X]

Xサイズに合うように、jsonファイルを読み込んで、作曲者名や曲名を読み込んで透過pngを作成する関数

**読み込むjson形式**

| key          |              value |
| :----------- | -----------------: |
| ComposerName |   string (ex.Zero) |
| SongName     | string (ex.Femboy) |

**実際の形式**

```tree
.
└── SoundInfo -> ClassName(全体を包むクラス名)
    ├── ComposerName -> KeyName(キー名、ここに文字列を入れる)
    └── SongName -> KeyName(キー名、ここに文字列を入れる)
```

#### [text to image for Youtube]

Youtubeサイズに合うように、jsonファイルを読み込んで、作曲者名や曲名を読み込んで透過pngを作成する関数

**読み込むjson形式**

| key          |              value |
| :----------- | -----------------: |
| ComposerName |   string (ex.Zero) |
| SongName     | string (ex.Femboy) |

**実際の形式**

```tree
.
└── SoundInfo -> ClassName(全体を包むクラス名)
    ├── ComposerName -> KeyName(キー名、ここに文字列を入れる)
    └── SongName -> KeyName(キー名、ここに文字列を入れる)
```

####
