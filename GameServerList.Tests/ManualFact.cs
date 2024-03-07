using System.Diagnostics;

namespace GameServerList.Tests;

public class ManualFactAttribute : FactAttribute
{
    public ManualFactAttribute()
    {
        if (!Debugger.IsAttached)
        {
            Skip = "Only running in interactive mode.";
        }
    }
}