using System;
using System.Linq.Expressions;

public class Generate
{
    public static bool GenerateMain(int select)
    {
        switch (select)
        {
            case Const.UPLOAD_X:
                Console.WriteLine("X.com用の動画生成処理に入ります。");
                break;
            case Const.UPLOAD_YOUTUBE:
                Console.WriteLine("Youtube.com用の動画生成処理に入ります。");
                break;
        }
        return true;
    }
}