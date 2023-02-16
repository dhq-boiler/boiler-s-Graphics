

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReplaceInternalResourcesDefaultConstructorToPublic;

var targetFileName = args[0];

Tool.ReplaceInternalToPublicOnDefaultConstructor(targetFileName);