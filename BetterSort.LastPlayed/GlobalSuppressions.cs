// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BetterSort.LastPlayed.Test")]
// Unity overrides null comparison. So this can cause issue.
[assembly: SuppressMessage("Style", "IDE0029:Use coalesce expression", Justification = "<Pending>")]

namespace System.Runtime.CompilerServices {

  internal class IsExternalInit { }
}
