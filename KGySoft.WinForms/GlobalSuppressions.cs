// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "Decided individually")]
[assembly: SuppressMessage("Style", "IDE0034:Simplify 'default' expression", Justification = "Should remain if helps readability")]
[assembly: SuppressMessage("Style", "IDE0056:Use index operator", Justification = "Cannot be used because it is not supported in every targeted platform")]
[assembly: SuppressMessage("Style", "IDE0057:Use range operator", Justification = "Cannot be used because it is not supported in every targeted platform")]
[assembly: SuppressMessage("Style", "IDE0090:Use 'new(...)'", Justification = "Decided individually")]
[assembly: SuppressMessage("Style", "IDE0130:Namespace does not match folder structure", Justification = "False alarm, Namespace Provider property is set to false to for folders that are not namespace providers")]
[assembly: SuppressMessage("Style", "IDE0270:Null check can be simplified (if null check)", Justification = "Decided individually. Sometimes it looks cleaner to have a separate validation block.")]
[assembly: SuppressMessage("Style", "IDE0300:Use collection expression for array'", Justification = "Decided individually")]
[assembly: SuppressMessage("Style", "IDE0305:Use collection expression for fluent", Justification = "Decided individually")]
[assembly: SuppressMessage("Interoperability", "SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time", Justification = "WinForms is not AOT compatible anyway")]
[assembly: SuppressMessage("Performance", "SYSLIB1045:Convert to 'GeneratedRegexAttribute'.", Justification = "Not every targeted platform supports it, and it's actually not even always faster than Compiled regex - see https://sam-lau.com/benchmark-net-regular-expression-source-generators/")]
