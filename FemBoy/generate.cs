using System;
using System.Linq.Expressions;
using System.Text.Json;

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
        return true;
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