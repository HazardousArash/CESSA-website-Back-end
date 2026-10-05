namespace Cessa.Application.Common.Interfaces
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(Stream content, string fileName, CancellationToken cancellationToken = default);
    }
}