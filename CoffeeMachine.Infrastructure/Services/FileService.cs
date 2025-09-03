using CoffeeMachine.Core.Application.Repository;
using CoffeeMachine.Core.Common.Results;

namespace CoffeeMachine.Infrastructure.Services;

public class FileService : IFileService
{
    public async Task<Result<string>> ReadFileAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
                return Result.Failure<string>("File path cannot be null or empty");

            if (!File.Exists(filePath))
                return Result.Failure<string>($"File not found: {filePath}");

            var content = await File.ReadAllTextAsync(filePath);
            return Result.Success(content);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>($"Error reading file: {ex.Message}");
        }
    }

    public async Task<Result> WriteFileAsync(string filePath, string content)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
                return Result.Failure("File path cannot be null or empty");

            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(filePath, content);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error writing file: {ex.Message}");
        }
    }

    public async Task<Result<bool>> FileExistsAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
                return Result<bool>.Failure<bool>("File path cannot be null or empty");

            var exists = File.Exists(filePath);
            return Result<bool>.Success(exists);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure<bool>($"Error checking file existence: {ex.Message}");
        }
    }

    public async Task<Result> DeleteFileAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
                return Result.Failure("File path cannot be null or empty");

            if (!File.Exists(filePath))
                return Result.Failure($"File not found: {filePath}");

            File.Delete(filePath);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deleting file: {ex.Message}");
        }
    }

    public async Task<Result<FileInfo>> GetFileInfoAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
                return Result<FileInfo>.Failure<FileInfo>("File path cannot be null or empty");

            if (!File.Exists(filePath))
                return Result<FileInfo>.Failure<FileInfo>($"File not found: {filePath}");

            var fileInfo = new FileInfo(filePath);
            return Result<FileInfo>.Success(fileInfo);
        }
        catch (Exception ex)
        {
            return Result<FileInfo>.Failure<FileInfo>($"Error getting file info: {ex.Message}");
        }
    }

    public async Task<Result> CreateDirectoryAsync(string directoryPath)
    {
        try
        {
            if (string.IsNullOrEmpty(directoryPath))
                return Result.Failure("Directory path cannot be null or empty");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error creating directory: {ex.Message}");
        }
    }

    public async Task<Result<List<string>>> GetFilesInDirectoryAsync(string directoryPath, string searchPattern = "*.*")
    {
        try
        {
            if (string.IsNullOrEmpty(directoryPath))
                return Result<List<string>>.Failure<List<string>>("Directory path cannot be null or empty");

            if (!Directory.Exists(directoryPath))
                return Result<List<string>>.Failure<List<string>>($"Directory not found: {directoryPath}");

            var files = Directory.GetFiles(directoryPath, searchPattern);
            return Result<List<string>>.Success(new List<string>(files));
        }
        catch (Exception ex)
        {
            return Result<List<string>>.Failure<List<string>>($"Error getting files in directory: {ex.Message}");
        }
    }

    public async Task<Result<string>> ReadAllTextAsync(string filePath)
    {
        return await ReadFileAsync(filePath);
    }

    public async Task<Result> WriteAllTextAsync(string filePath, string content)
    {
        return await WriteFileAsync(filePath, content);
    }

    public async Task<Result<byte[]>> ReadAllBytesAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
                return Result<byte[]>.Failure<byte[]>("File path cannot be null or empty");

            if (!File.Exists(filePath))
                return Result<byte[]>.Failure<byte[]>($"File not found: {filePath}");

            var bytes = await File.ReadAllBytesAsync(filePath);
            return Result<byte[]>.Success(bytes);
        }
        catch (Exception ex)
        {
            return Result<byte[]>.Failure<byte[]>($"Error reading file bytes: {ex.Message}");
        }
    }

    public async Task<Result> WriteAllBytesAsync(string filePath, byte[] data)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
                return Result.Failure("File path cannot be null or empty");

            if (data == null)
                return Result.Failure("Data cannot be null");

            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllBytesAsync(filePath, data);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error writing file bytes: {ex.Message}");
        }
    }
}