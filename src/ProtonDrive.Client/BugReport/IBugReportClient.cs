using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProtonDrive.Client.BugReport;

public interface IBugReportClient
{
    Task SendAsync(BugReportBody report, IReadOnlyCollection<BugReportAttachment> attachments, CancellationToken cancellationToken);
}
