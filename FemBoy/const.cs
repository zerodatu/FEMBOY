using System;
using System.Collections.Concurrent;
using System.IO;

/// <summary>
/// 定数の定義クラス
/// </summary>
public class Const
{
    public readonly string PIC_DIR = Path.Combine(Environment.CurrentDirectory, "Pic");     // PicDirPath
    public readonly string MUSIC_DIR = Path.Combine(Environment.CurrentDirectory, "Music"); // MusicDirPath
    public readonly string TITLE_DIR = Path.Combine(Environment.CurrentDirectory, "Title"); // TitleDirPath
    public readonly string[] AUDIO_FORMAT = { "*.mp3", "*.wav", "*.ogg", "*.m4a", "*.flac", "*.aac", "*.wma", "*.opus" };
    public readonly string[] IMG_FORMAT = { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif", "*.webp", "*.tiff", "*.tif" };
    
    // いずれ他のアップロードサイトの選択肢が発生したときのため
    public const int UPLOAD_X = 0;
    public const int UPLOAD_YOUTUBE = 1;

}