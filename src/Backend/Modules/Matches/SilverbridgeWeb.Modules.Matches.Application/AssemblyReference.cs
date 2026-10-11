using System.Reflection;

namespace SilverbridgeWeb.Modules.Matches.Application;

public sealed class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}
