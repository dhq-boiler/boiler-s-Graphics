using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplaceInternalResourcesDefaultConstructorToPublic
{
    public static class Tool
    {
        public static void ReplaceInternalToPublicOnDefaultConstructor(string targetFileName)
        {
            //ソースコードを読み込みます。
            var sourceCode = File.ReadAllText(targetFileName);

            //SyntaxTree オブジェクトを作成します。
            SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceCode);

            //SyntaxTree から SyntaxNode を取得します。
            SyntaxNode root = tree.GetRoot();

            //SyntaxNode からクラスの宣言を取得します。
            ClassDeclarationSyntax classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            //クラスの宣言から、デフォルトコンストラクタの宣言を取得します。
            ConstructorDeclarationSyntax constructorDeclaration = classDeclaration.DescendantNodes().OfType<ConstructorDeclarationSyntax>().FirstOrDefault(cd => cd.ParameterList.Parameters.Count == 0);
            
            // アクセス修飾子トークンを取得
            SyntaxToken accessModifierToken = constructorDeclaration.Modifiers.First();

            // public アクセス修飾子トークンを作成 (トリビアを含む)
            SyntaxToken publicModifierToken = SyntaxFactory.Token(
                accessModifierToken.LeadingTrivia, // 前の空白やコメントを含める
                SyntaxKind.PublicKeyword,
                accessModifierToken.TrailingTrivia // 後の空白やコメントを含める
            );

            // public アクセス修飾子を持つ新しいコンストラクタ宣言を作成
            ConstructorDeclarationSyntax newConstructorDeclaration =
                constructorDeclaration.WithModifiers(SyntaxFactory.TokenList(publicModifierToken));

            //SyntaxNode からデフォルトコンストラクタの宣言を削除し、新しいデフォルトコンストラクタの宣言を追加します。
            ClassDeclarationSyntax newClassDeclaration = classDeclaration.ReplaceNode(constructorDeclaration, newConstructorDeclaration);
            SyntaxNode newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);

            //SyntaxNode から更新されたソースコードを取得します。
            string newSourceCode = newRoot.ToFullString();

            //ソースコードを書き込みます。
            File.WriteAllText(targetFileName, newSourceCode);
        }
    }
}
