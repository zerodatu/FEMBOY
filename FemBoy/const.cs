using System;
using System.Collections.Concurrent;
using System.Data;
using System.IO;

/// <summary>
/// 定数の定義クラス
/// </summary>
public class Const
{
    public readonly string PIC_DIR = Path.Combine(Environment.CurrentDirectory, "Pic");     // PicDirPath
    public readonly string MUSIC_DIR = Path.Combine(Environment.CurrentDirectory, "Music"); // MusicDirPath
    public readonly string[] AUDIO_FORMAT = { "*.mp3", "*.wav", "*.ogg", "*.m4a", "*.flac", "*.aac", "*.wma", "*.opus" };
    public readonly string[] IMG_FORMAT = { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif", "*.webp", "*.tiff", "*.tif" };
    public const int INIT_INT = 0;  // int系で共通利用する初期化の値
    // いずれ他のアップロードサイトの選択肢が発生したときのため
    public const int UPLOAD_X = 1;
    public const int UPLOAD_YOUTUBE = 2;

}