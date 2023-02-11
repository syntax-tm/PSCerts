using System.Collections.Generic;

namespace PSCerts.Summary;

public class CertSummary
{
    public List<CertSummaryItem> Items { get; set; }
    public int Count => Items.Count;
    public bool IsEmpty => Count == 0;

    public CertSummary()
    {
        Items = new ();
    }
}
