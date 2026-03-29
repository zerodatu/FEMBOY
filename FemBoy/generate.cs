using System;
using System.Linq.Expressions;

public class Generate
{
    public static bool GenerateMain(int select)
    {
        if (select == Const.UPLOAD_X)
        {
            Console.WriteLine("X.com用の動画生成処理に入ります。");
        }
        else if (select == Const.UPLOAD_YOUTUBE)
        {
            Console.WriteLine("Youtube.com用の動画生成処理に入ります。");
        }
        return true;
    }
}