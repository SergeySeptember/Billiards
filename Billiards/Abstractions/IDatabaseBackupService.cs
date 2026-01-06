using Billiards.ModelAndDto;

namespace Billiards.Abstractions;

public interface IDatabaseBackupService
{
    Task<BilliardsBackupDto> BuildBackupAsync();
    Task RestoreBackupAsync(BilliardsBackupDto backup);
}