using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using boilersGraphics;

[assembly: InternalsVisibleTo("boilersGraphics.Test")]

// アセンブリに関する一般情報は以下の属性セットをとおして制御されます。
// アセンブリに関連付けられている情報を変更するには、
// これらの属性値を変更してください。
[assembly: AssemblyTitle("boiler's Graphics")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("boiler's Graphics")]
[assembly: AssemblyCopyright("Copyright ©dhq_boiler 2018-2023")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// ComVisible を false に設定すると、このアセンブリ内の型は COM コンポーネントから
// 参照できなくなります。COM からこのアセンブリ内の型にアクセスする必要がある場合は、
// その型の ComVisible 属性を true に設定してください。
[assembly: ComVisible(false)]

//ローカライズ可能なアプリケーションのビルドを開始するには、
//.csproj ファイルの <UICulture>CultureYouAreCodingWith</UICulture> を
//<PropertyGroup> 内部で設定します。たとえば、
//ソース ファイルで英語を使用している場合、<UICulture> を en-US に設定します。次に、
//下の NeutralResourceLanguage 属性のコメントを解除します。下の行の "en-US" を
//プロジェクト ファイルの UICulture 設定と一致するよう更新します。

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //テーマ固有のリソース ディクショナリが置かれている場所
    //(リソースがページ、
    //またはアプリケーション リソース ディクショナリに見つからない場合に使用されます)
    ResourceDictionaryLocation.SourceAssembly //汎用リソース ディクショナリが置かれている場所
    //(リソースがページ、
    //アプリケーション、またはいずれのテーマ固有のリソース ディクショナリにも見つからない場合に使用されます)
)]


// AssemblyVersion / AssemblyFileVersion / AssemblyInformationalVersion are
// produced by the SDK's GenerateAssemblyInfo target from the AssemblyVersion
// / FileVersion / InformationalVersion MSBuild properties, which the
// SetGitDerivedVersion target in boilersGraphics.csproj populates from
// `git rev-list` / `git rev-parse`. This avoids the wpftmp obj/-sharing
// issue that GitInfo's source generator hit, while still embedding the
// commit count / branch / sha into the assembly metadata.