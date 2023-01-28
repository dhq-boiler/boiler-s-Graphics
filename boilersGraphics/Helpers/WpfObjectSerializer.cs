// WPF オブジェクトをシリアル化または逆シリアル化するクラス
//
// WPF オブジェクトとは、XAML 構文で表現可能なオブジェクトと考えてよい。たとえば、Background プロパティの
// Brush オブジェクトなどのグラフィックスオブジェクトだけでなく、Window や Control も含む。

using System.IO;
using System.Text;
using System.Windows.Markup;
using System.Xml;

//namespace emanual.Wpf.Utility
namespace boilersGraphics.Helpers;

public class WpfObjectSerializer
{
    //---------------------------------------------------------------------------------------------
    // 指定の WPF オブジェクトをシリアル化し、指定のファイルに出力する
    // obj : シリアル化するオブジェクト（オブジェクトの Name プロパティ）
    // fileName: 出力ファイル名（文字コードは UTF-8）
    public static void Serialize(object obj, string fileName)
    {
        var text = Serialize(obj);

        var writer = new StreamWriter(fileName, false, Encoding.UTF8);

        try
        {
            writer.WriteLine(text);
        }
        finally
        {
            writer.Close();
        }
    }

    //---------------------------------------------------------------------------------------------
    // 指定の WPF オブジェクトをシリアル化する
    // obj : シリアル化するオブジェクト（オブジェクトの Name プロパティ）
    public static string Serialize(object obj)
    {
        var settings = new XmlWriterSettings();

        // 出力時の条件
        settings.Indent = true;
        settings.NewLineOnAttributes = false;

        // XML バージョン情報の出力を抑制する
        settings.ConformanceLevel = ConformanceLevel.Fragment;

        var sb = new StringBuilder();
        XmlWriter writer = null;
        XamlDesignerSerializationManager manager = null;

        try
        {
            writer = XmlWriter.Create(sb, settings);
            manager = new XamlDesignerSerializationManager(writer);
            manager.XamlWriterMode = XamlWriterMode.Expression;

            XamlWriter.Save(obj, manager);
        }
        finally
        {
            if (writer != null)
                writer.Close();
        }

        return sb.ToString();
    }

    //---------------------------------------------------------------------------------------------
    // 指定のファイルから XAML 文を読み込んで逆シリアル化し、WPF オブジェクトを返す
    // fileName : XAML 文を保持するファイル名
    // encoding : 文字のエンコーディング法（null のとき、UTF-8）
    // 戻り値   : WPF オブジェクト（オブジェクトの内容に応じて型キャストする）
    public static object Deserialize(string fileName, Encoding encoding)
    {
        var text = string.Empty;
        StreamReader reader = null;

        if (encoding == null)
            reader = new StreamReader(fileName, Encoding.UTF8);
        else
            reader = new StreamReader(fileName, encoding);

        try
        {
            text = reader.ReadToEnd();
        }
        finally
        {
            if (reader != null)
                reader.Close();
        }

        var obj = Deserialize(text);

        return obj;
    }

    //---------------------------------------------------------------------------------------------
    // 指定の XAML 文を読み込んで逆シリアル化し、WPF オブジェクトを返す
    // xamlText : XAML 文
    // 戻り値   : WPF オブジェクト（オブジェクトの内容に応じて型キャストする）
    public static object Deserialize(string xamlText)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xamlText);
            var obj = XamlReader.Load(new XmlNodeReader(doc));
            return obj;
        }
        catch (XmlException)
        {
            return null;
        }
    }
} // end of WpfObjectSerializer class
// end of namespace