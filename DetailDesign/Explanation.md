- [1. 設計解説資料](#1-設計解説資料)
  - [1.1. 概要について](#11-概要について)
    - [1.1.1. \[do command\]](#111-do-command)
    - [1.1.2. \[is insall ffmpeg\]](#112-is-insall-ffmpeg)
    - [1.1.3. \[Check Resource\]](#113-check-resource)
    - [1.1.4. \[is resource avaible\]](#114-is-resource-avaible)
    - [1.1.5. \[Select upload site\]](#115-select-upload-site)
      - [1.1.5.1. \[text to image for X\]](#1151-text-to-image-for-x)
        - [1.1.5.1.1. \[resize to resource image for x\]](#11511-resize-to-resource-image-for-x)
      - [1.1.5.2. \[text to image for Youtube\]](#1152-text-to-image-for-youtube)
        - [1.1.5.2.1. \[resize to resource image for Youtube\]](#11521-resize-to-resource-image-for-youtube)
    - [1.1.6. \[Create visualizer resource\]](#116-create-visualizer-resource)
      - [1.1.6.1. \[audio\_wav\_encoder\]](#1161-audio_wav_encoder)
      - [1.1.6.2. \[wave\_visualizer\_generator\]](#1162-wave_visualizer_generator)
    - [1.1.7. \[Mix image for text\]](#117-mix-image-for-text)
    - [1.1.8. \[Expoer Video\]](#118-expoer-video)

# 1. 設計解説資料

## 1.1. 概要について

DetailDesign/DetailDesign.webpに書かれている各処理について、処理内容などを記載します。

### 1.1.1. [do command]

バイナリを実行することで処理が実行される。

### 1.1.2. [is insall ffmpeg]

ffmpegがコンピューターにインストールされているか確認する。

- `Windows`の場合
  - installed -> 次の処理に進む
  - not installed -> ffmpegをインストールするコマンドをポップアップで提示する。
    - `winget install ffmpeg`を実行してください。
- `Ubuntu`の場合
  - installed -> 次の処理に進む
  - not installed -> ffmpegをインストールするコマンドをポップアップで提示する。
    - `sudo apt install ffmpeg`を実行してください。

### 1.1.3. [Check Resource]

PicフォルダとMusicフォルダにリソースが格納されていることを確認する。

### 1.1.4. [is resource avaible]

- Picフォルダにリソースが格納されていないとき
  - 画像ファイルが格納されていないので、格納してください。とポップアップで提示する。
- Musicフォルダにリソースが格納されていないとき
  - 音楽ファイルが格納されていないので、格納してください。とポップアップで提示する。
- PicフォルダもMusicフォルダもリソースが格納されていないとき
  - 音楽も画像も格納してください。とポップアップで提示する。

### 1.1.5. [Select upload site]

アップロードサイトをXかYoutubeか選択して以降の処理を分岐する。<br>
ポップアップで`Which your upload site is X or Youtube?`が表示される。<br>

- Xを選んだ場合
  - Xとして内部フラグを立てる。
  - その次に`text to image for X`を実行する。
- Youtubeを選んだ場合
  - Youtubeとして内部フラグを立てる。
  - その次に`text to image for Youtube`を実行する。

#### 1.1.5.1. [text to image for X]

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

##### 1.1.5.1.1. [resize to resource image for x]

Picフォルダに格納された画像ファイルを縦の長さをフィットさせて、横の長さをトリミングするようにして、切り出すようにする。<br>
**サイズは720x1280**

しかし、サイズ的にぴったりの画像が格納されていたら特にカットせずそのまま使われる。<br>
中央揃えで行われる。

#### 1.1.5.2. [text to image for Youtube]

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

##### 1.1.5.2.1. [resize to resource image for Youtube]

Picフォルダに格納された画像ファイルを横の長さをフィットさせて、横の長さをトリミングするようにして、切り出すようにする。<br>
**サイズは1920x1080**

しかし、サイズ的にぴったりの画像が格納されていたら特にカットせずそのまま使われる。<br>
中央揃えで行われる。

### 1.1.6. [Create visualizer resource]

オーディオビジュアライザー用の連番画像を生成するための処理群<br>
ffmpegに連番画像群を動画のような扱いで指定するオプションがあるようなので、それを用いて実装する。

#### 1.1.6.1. [audio_wav_encoder]

格納された音声ファイルを解析できるようにwavに変換する処理。ffmpegを使っても良いと思う。

#### 1.1.6.2. [wave_visualizer_generator]

格納されたwavファイルを用いて、連番画像を作成する処理。<br>
当然だが、音声を波形化した透過PNGで書き出す。

透過した連番はffmepgで動画素材のように指定できるようようです。

### 1.1.7. [Mix image for text]

透過PNG化したテキスト画像と、トリミングされたPicフォルダの画像を組み合わせる。<br>
合成できたら一時的に作成したテキストimageは削除して、`Complete`フォルダに組み合わせられた画像を格納する。

webpを検討したが、ユーザー側に追加のコンポーネントインストールが必要になる可能性があることと、そもそもエンコード速度という観点からして、デコードのオーバーヘッドが発生する可能性があるということなので、jpegで内部的に処理して、画像をエンコードしたほうがシンプルかつ早いと思われる。(枯れた技術なので)

### 1.1.8. [Expoer Video]

Musicフォルダに格納された音楽ファイルと`Complete`フォルダに格納された画像ファイルを組み合わせてffmpegを使って動画を作成する。

その際に`[Select upload site]`で設定したフラグを読み取る。

- フラグがXの場合
  - 音楽ファイルが2分以上か確認
    - 2分以上だった場合は2分に制限する。
    - 2分未満だった場合はそのまま使う。
- フラグがYoutubeの場合
  - 動画タイムは設定せず、音楽の時間に合わせる。

H265形式のmp4で書き出す<br>
書き出し後は、`Complete`フォルダに格納された画像ファイルを削除し、動画だけにする。

- オーディオビジュアライザーを乗せる際の対応。
  - 現行の書き出しに追加で連番画像を動画ファイルのように指定して、書き出しを行わないといけない。