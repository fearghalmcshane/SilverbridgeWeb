using System.Reflection;

namespace SilverbridgeWeb.Modules.Matches.Domain;

public sealed class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}
