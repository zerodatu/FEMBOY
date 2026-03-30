using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Fonts;

/// <summary>
/// 動画生成処理を管理するクラスです。
/// </summary>
public class Generate
{
    /// <summary>
    /// ユーザーが選択したアップロードサイトに応じた動画生成処理のエントリポイントです。
    /// </summary>
    /// <param name="select">
    /// ユーザーが選択したアップロードサイトを表す整数値。
    /// </param>
    /// <returns>
    /// 動画生成処理が正常に完了した場合は true、何らかの理由で処理が失敗した場合は false を返します。
    /// </returns>
    public static bool GenerateMain(int select)
    {
        Dictionary<string, string> map = new Dictionary<string, string>();
        switch (select)
        {
            case Const.UPLOAD_X:
                Console.WriteLine("X.com用の動画生成処理に入ります。");
                map = GetTitleInfo();
                break;
            case Const.UPLOAD_YOUTUBE:
                Console.WriteLine("Youtube.com用の動画生成処理に入ります。");
                map = GetTitleInfo();
                break;
        }

        // jsonから取得した曲のタイトル、アーティスト名、BPMをコンソールに表示する
        foreach (var item in map)
        {
            Console.WriteLine($"{item.Key}: {item.Value}");
        }
        Console.WriteLine("-------------------");

        // テキストから透過PNGを生成
        CreateTextOverlay(map, select);

        return true;
    }

    /// <summary>
    /// 曲情報のマップからImageSharpを使用して透過PNG画像を生成します。
    /// </summary>
    /// <param name="map">曲情報（Title, Artist, BPMなど）</param>
    /// <param name="select">アップロードサイトの識別子</param>
    static void CreateTextOverlay(Dictionary<string, string> map, int select)
    {
        // tmpフォルダの作成
        string tmpDir = Path.Combine(Environment.CurrentDirectory, "tmp");
        if (!Directory.Exists(tmpDir))
        {
            Directory.CreateDirectory(tmpDir);
        }

        // 解像度の設定
        int width = (select == Const.UPLOAD_X) ? 1080 : 1920;
        int height = (select == Const.UPLOAD_X) ? 1920 : 1080;

        // テキストの構築
        string title = map.GetValueOrDefault("Title", "Unknown Title");
        string artist = map.GetValueOrDefault("Artist", "Unknown Artist");
        string bpm = map.GetValueOrDefault("BPM", "0");
        string text = $"{title}{Environment.NewLine}{artist}{Environment.NewLine}BPM: {bpm}";

        // フォントの設定 (システムフォントから選択。Arialがない場合は最初に見つかったものを使用)
        if (!SystemFonts.Collection.TryGet("Arial", out var family))
        {
            family = SystemFonts.Collection.Families.FirstOrDefault();
        }
        Font font = family.CreateFont(50, FontStyle.Bold);

        using (Image<Rgba32> image = new Image<Rgba32>(width, height))
        {
            image.Mutate(ctx =>
            {
                if (select == Const.UPLOAD_X)
                {
                    // X.com用: 左側に配置
                    var options = new RichTextOptions(font)
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Origin = new PointF(50, height / 2)
                    };
                    ctx.DrawText(options, text, Color.White);
                }
                else
                {
                    // Youtube用: 下側に中央揃えで配置
                    var options = new RichTextOptions(font)
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Origin = new PointF(width / 2, height - 100)
                    };
                    ctx.DrawText(options, text, Color.White);
                }
            });

            string outputPath = Path.Combine(tmpDir, "overlay.png");
            image.SaveAsPng(outputPath);
            Console.WriteLine($"Overlay image created: {outputPath}");
        }
    }

    /// <summary>
    /// title.jsonから曲のタイトル、アーティスト名、BPMを取得する関数
    /// </summary>
    /// <returns>
    /// Dictionary<string, string>: title.jsonから取得した曲のタイトル、アーティスト名、BPMを格納した辞書
    /// </returns>
    static Dictionary<string, string> GetTitleInfo()
    {
        string formatPath = Path.Combine(Environment.CurrentDirectory, "title.json");
        string jsonString = File.ReadAllText(formatPath);
        var param = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonString);
        return param?["SoundInfo"] ?? new Dictionary<string, string>();
    }
}