- [設計解説資料](#設計解説資料)
  - [概要について](#概要について)
    - [\[do command\]](#do-command)
    - [\[is insall ffmpeg\]](#is-insall-ffmpeg)
    - [\[Check Resource\]](#check-resource)
    - [\[is resource avaible\]](#is-resource-avaible)
    - [\[Select upload site\]](#select-upload-site)
      - [\[text to image for X\]](#text-to-image-for-x)
        - [\[resize to resource image for x\]](#resize-to-resource-image-for-x)
      - [\[text to image for Youtube\]](#text-to-image-for-youtube)
        - [\[resize to resource image for Youtube\]](#resize-to-resource-image-for-youtube)
    - [\[Mix image for text\]](#mix-image-for-text)
    - [\[Expoer Video\]](#expoer-video)

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

##### [resize to resource image for x]

Picフォルダに格納された画像ファイルを縦の長さをフィットさせて、横の長さをトリミングするようにして、切り出すようにする。<br>
**サイズは720x1280**

しかし、サイズ的にぴったりの画像が格納されていたら特にカットせずそのまま使われる。<br>
中央揃えで行われる。

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

##### [resize to resource image for Youtube]

Picフォルダに格納された画像ファイルを横の長さをフィットさせて、横の長さをトリミングするようにして、切り出すようにする。<br>
**サイズは1920x1080**

しかし、サイズ的にぴったりの画像が格納されていたら特にカットせずそのまま使われる。<br>
中央揃えで行われる。

### [Mix image for text]

透過PNG化したテキスト画像と、トリミングされたPicフォルダの画像を組み合わせる。<br>
合成できたら一時的に作成したテキストimageは削除して、`Complete`フォルダに組み合わせられた画像を格納する。

webpを検討したが、ユーザー側に追加のコンポーネントインストールが必要になる可能性があることと、そもそもエンコード速度という観点からして、デコードのオーバーヘッドが発生する可能性があるということなので、jpegで内部的に処理して、画像をエンコードしたほうがシンプルかつ早いと思われる。(枯れた技術なので)

### [Expoer Video]

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
