using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Codebound
{
    /// <summary>
    /// What player C# may touch. Every symbol and type the program refers to is checked against an allow-list of
    /// namespaces, minus types that reach outside the program (reflection handles, environment, console, app domains).
    /// A member is refused if its declaring type, its own type, its parameters or its generic arguments are refused,
    /// so obj.GetType() (returns System.Type) and ex.TargetSite (System.Reflection) are blocked too.
    /// Unsafe code is off at compile time. The OS process limits (Job Object) are the second layer.
    /// </summary>
    public static class Sandbox
    {
        static readonly HashSet<string> AllowedNamespaces = new HashSet<string>
        {
            "System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Text.RegularExpressions", "System.Text.Json.Nodes", "Codebound",
        };

        static readonly HashSet<string> DeniedTypes = new HashSet<string>
        {
            "System.Type", "System.Activator", "System.AppDomain", "System.AppContext", "System.Environment", "System.Console",
            "System.RuntimeTypeHandle", "System.RuntimeMethodHandle", "System.RuntimeFieldHandle", "System.ModuleHandle",
            "System.TypedReference", "System.ArgIterator", "System.MarshalByRefObject", "System.ResolveEventHandler",
            "System.AppDomainSetup", "System.LoaderOptimizationAttribute", "System.STAThreadAttribute", "System.MTAThreadAttribute",
            "Codebound.Proto", "Codebound.Host", "Codebound.Sandbox",
        };

        /// <summary>Null when the program only uses allowed things, otherwise the first violation with its line.</summary>
        public static string Check(Compilation compilation, SyntaxTree tree)
        {
            var model = compilation.GetSemanticModel(tree);
            var player = compilation.Assembly;
            foreach (var node in tree.GetRoot().DescendantNodesAndSelf())
            {
                string bad = null;
                if (node is MethodDeclarationSyntax md && md.Modifiers.Any(SyntaxKind.ExternKeyword)) bad = "extern methods";
                else if (node is LocalFunctionStatementSyntax lf && lf.Modifiers.Any(SyntaxKind.ExternKeyword)) bad = "extern methods";
                else if (node.IsKind(SyntaxKind.MakeRefExpression) || node.IsKind(SyntaxKind.RefTypeExpression) || node.IsKind(SyntaxKind.RefValueExpression) || node.IsKind(SyntaxKind.ArgListExpression))
                    bad = "__makeref/__reftype/__refvalue/__arglist";
                else if (node is ExpressionSyntax || node is TypeSyntax || node is AttributeSyntax || node is ConstructorInitializerSyntax)
                {
                    var info = model.GetSymbolInfo(node);
                    foreach (var s in info.Symbol != null ? new[] { info.Symbol } : info.CandidateSymbols.ToArray())
                        if (!Allowed(s, player)) { bad = Describe(s); break; }
                    if (bad == null && node is ExpressionSyntax)
                    {
                        var ti = model.GetTypeInfo(node);
                        if (!Allowed(ti.Type, player)) bad = Describe(ti.Type);
                        else if (!Allowed(ti.ConvertedType, player)) bad = Describe(ti.ConvertedType);
                    }
                }
                else if (node is VariableDeclaratorSyntax || node is ParameterSyntax || node is BaseMethodDeclarationSyntax || node is PropertyDeclarationSyntax || node is ForEachStatementSyntax)
                {
                    var d = model.GetDeclaredSymbol(node);
                    if (!Allowed(d, player)) bad = Describe(d);
                }
                if (bad != null)
                    return $"line {node.GetLocation().GetLineSpan().StartLinePosition.Line + 1}: '{bad}' is not available on Codebound computers " +
                           "(allowed: System basics, collections, LINQ, text, regex, JsonNode and the game API)";
            }
            return null;
        }

        static string Describe(ISymbol s) => s?.ToDisplayString() ?? "?";

        static bool Allowed(ISymbol s, IAssemblySymbol player)
        {
            switch (s)
            {
                case null: return true;
                case ITypeSymbol t: return Allowed(t, player);
                case IMethodSymbol m:
                    if (SymbolEqualityComparer.Default.Equals(m.ContainingAssembly, player)) return m.Parameters.All(p => Allowed(p.Type, player)) && Allowed(m.ReturnType, player);
                    return Allowed(m.ContainingType, player) && Allowed(m.ReturnType, player) && m.Parameters.All(p => Allowed(p.Type, player))
                           && m.TypeArguments.All(a => Allowed(a, player));
                case IPropertySymbol p: return Allowed(p.ContainingType, player) && Allowed(p.Type, player);
                case IFieldSymbol f: return Allowed(f.ContainingType, player) && Allowed(f.Type, player);
                case IEventSymbol e: return Allowed(e.ContainingType, player) && Allowed(e.Type, player);
                case ILocalSymbol l: return Allowed(l.Type, player);
                case IParameterSymbol p: return Allowed(p.Type, player);
                case IAliasSymbol a: return Allowed(a.Target, player);
                case INamespaceSymbol n: return n.IsGlobalNamespace || AllowedNamespaces.Contains(n.ToDisplayString()) || AllowedNamespaces.Any(x => x.StartsWith(n.ToDisplayString() + "."))
                                                || SymbolEqualityComparer.Default.Equals(n.ContainingAssembly, player);
                default: return true;   // labels, range variables, discards
            }
        }

        static bool Allowed(ITypeSymbol t, IAssemblySymbol player)
        {
            switch (t)
            {
                case null: return true;
                case IArrayTypeSymbol a: return Allowed(a.ElementType, player);
                case IPointerTypeSymbol _: case IFunctionPointerTypeSymbol _: return false;
                case IDynamicTypeSymbol _: return false;
                case ITypeParameterSymbol _: return true;
                case INamedTypeSymbol n:
                    if (n.IsTupleType) return n.TupleElements.All(e => Allowed(e.Type, player));
                    if (!n.TypeArguments.All(a => Allowed(a, player))) return false;
                    if (SymbolEqualityComparer.Default.Equals(n.ContainingAssembly, player)) return true;
                    if (n.ContainingType != null) return Allowed(n.ContainingType, player);
                    string ns = n.ContainingNamespace?.ToDisplayString() ?? "";
                    return AllowedNamespaces.Contains(ns) && !DeniedTypes.Contains(ns + "." + n.Name);
                default: return t.TypeKind != TypeKind.Error;   // unresolved types are reported by the compiler anyway
            }
        }
    }
}
