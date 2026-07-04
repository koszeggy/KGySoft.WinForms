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

[assembly: SuppressMessage("Style", "IDE0028:Collection initialization can be simplified", Justification = "Decided individually")]
[assembly: SuppressMessage("Style", "IDE0057:Use range operator", Justification = "Cannot be used because it is not supported in every targeted platform")]
[assembly: SuppressMessage("Style", "IDE0305:Use collection expression for fluent", Justification = "Decided individually")]
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Event handlers don't follow regular naming rules")]
