using DesktopScaffold.Core.Application.Abstractions;

namespace DesktopScaffold.Tests.Fakes;

public sealed class TestLogger : IAppLogger
{
    public List<string> Lines { get; } = new();

    public void Info(string message) => Lines.Add("INFO " + message);
    public void Warn(string message) => Lines.Add("WARN " + message);
    public void Error(string message, Exception? ex = null) => Lines.Add("ERROR " + message + (ex is null ? "" : " :: " + ex.Message));
}

