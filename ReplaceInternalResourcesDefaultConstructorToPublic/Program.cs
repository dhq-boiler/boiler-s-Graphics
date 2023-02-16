

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReplaceInternalResourcesDefaultConstructorToPublic;

var targetFileName = args[1];

Tool.ReplaceInternalToPublicOnDefaultConstructor(targetFileName);