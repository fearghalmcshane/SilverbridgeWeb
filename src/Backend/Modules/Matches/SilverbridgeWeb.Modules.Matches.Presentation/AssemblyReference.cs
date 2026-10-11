using System.Reflection;

namespace SilverbridgeWeb.Modules.Matches.Presentation;

public sealed class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}
