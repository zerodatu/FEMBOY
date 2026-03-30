using System;
using Microsoft.VisualBasic;

/// <summary>
/// ユーザーからの入力によるアップロード先の選択処理を管理するクラスです。
/// </summary>
public class UserSelect
{
    /// <summary>
    /// ユーザーに動画のアップロード先サイト（X または YouTube）を選択させます。
    /// 最大3回まで入力を受け付け、有効な番号が入力されたかどうかを判定します。
    /// </summary>
    /// <returns>
    /// サイトが正常に選択された場合は true、3回連続で入力に失敗した場合は false を返します。
    /// </returns>
    public static bool SelectUser()
    {
        int count = Const.INIT_INT;  // whileカウント用
        int select_num = Const.INIT_INT;
        while (count < 3)
        {
            Console.WriteLine("アップロードサイトを選択してください。" + Environment.NewLine +
            "Please enter the upload site." + Environment.NewLine +
            "- X.com -> " + Const.UPLOAD_X + Environment.NewLine +
            "- Youtube.com -> " + Const.UPLOAD_YOUTUBE);

            Console.Write("Input: ");
            string? input = Console.ReadLine();

            Console.WriteLine("-------------------");

            if (
                (string.IsNullOrEmpty(input)) ||
                (input.ToString() != Const.UPLOAD_X.ToString() &&
                input.ToString() != Const.UPLOAD_YOUTUBE.ToString()))
            {
                Console.WriteLine("アップロードサイト番号を入力してください。" + Environment.NewLine +
                "Please enter the upload site number.");
                count++;
                continue;
            }
            else
            {
                select_num = int.Parse(input!);
                break;
            }


        }

        if (
            select_num != Const.UPLOAD_X &&
            select_num != Const.UPLOAD_YOUTUBE)
        {
            return false;
        }
        else if (select_num == Const.UPLOAD_X || select_num == Const.UPLOAD_YOUTUBE)
        {
            Shared.select_site_num = select_num;

            switch (select_num)
            {
                case Const.UPLOAD_X:
                    Console.WriteLine("あなたはX.comを選択しました。");
                    Console.WriteLine("You selected X.com.");
                    Console.WriteLine("-------------------");
                    break;
                case Const.UPLOAD_YOUTUBE:
                    Console.WriteLine("あなたはYoutube.comを選択しました。");
                    Console.WriteLine("You selected Youtube.com.");
                    Console.WriteLine("-------------------");
                    break;
            }

            return true;
        }
        else
        {
            return false;
        }


    }

}