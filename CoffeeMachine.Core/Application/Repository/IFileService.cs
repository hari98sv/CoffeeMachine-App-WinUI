using CoffeeMachine.Core.Common.Results;

namespace CoffeeMachine.Core.Application.Repository;

public interface IFileService
{
    Task<Result<string>> ReadFileAsync(string filePath);
    Task<Result> WriteFileAsync(string filePath, string content);
    Task<Result<bool>> FileExistsAsync(string filePath);
    Task<Result> DeleteFileAsync(string filePath);
    Task<Result<FileInfo>> GetFileInfoAsync(string filePath);
    Task<Result> CreateDirectoryAsync(string directoryPath);
    Task<Result<List<string>>> GetFilesInDirectoryAsync(string directoryPath, string searchPattern = "*.*");
    Task<Result<string>> ReadAllTextAsync(string filePath);
    Task<Result> WriteAllTextAsync(string filePath, string content);
    Task<Result<byte[]>> ReadAllBytesAsync(string filePath);
    Task<Result> WriteAllBytesAsync(string filePath, byte[] data);
}