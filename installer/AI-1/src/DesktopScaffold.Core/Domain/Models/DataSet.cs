namespace DesktopScaffold.Core.Domain.Models;

public sealed class DataSet
{
    public IReadOnlyList<DataRow> Rows { get; }

    public DataSet(IReadOnlyList<DataRow> rows)
    {
        Rows = rows ?? throw new ArgumentNullException(nameof(rows));
    }

    public int Count => Rows.Count;
}

