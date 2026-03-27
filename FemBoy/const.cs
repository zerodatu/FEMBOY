using System;
using System.Collections.Concurrent;
using System.IO;
public class Const
{
    public readonly string PIC_DIR = Path.Combine(Environment.CurrentDirectory, "Pic");     // PicDirPath
    public readonly string MUSIC_DIR = Path.Combine(Environment.CurrentDirectory, "Music"); // MusicDirPath
    public readonly string TITLE_DIR = Path.Combine(Environment.CurrentDirectory, "Title"); // TitleDirPath
    public readonly string[] AUDIO_FORMAT = { "*.mp3", "*.wav", "*.ogg", "*.m4a", "*.flac", "*.aac", "*.wma", "*.opus" };

}