

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Data.Contexts;
using Data.Entities;
using FileProvider.Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FileProvider.Services;

public class FileService(ILogger<FileService> logger, DataContext context, BlobServiceClient client, BlobContainerClient container)
{
    private readonly ILogger<FileService> _logger = logger;
    private readonly DataContext _context = context;
    private BlobServiceClient _client = client;
    private BlobContainerClient _container = container;

    public async Task SetBlobContainerAsync(string containerName)
    {
        _container = _client.GetBlobContainerClient(containerName);
        await _container.CreateIfNotExistsAsync();
    }

    public string SetFileName(IFormFile file)
    {
        var newFileName = $"{Guid.NewGuid()}_{file.FileName}";
        return file.Name;
    }

    public async Task<string> UploadFileAsync(IFormFile file, FileEntity fileEntity)
    {
        BlobHttpHeaders headers = new BlobHttpHeaders()
        {
            ContentType = file.ContentType,
        };

        var blobClient = _container.GetBlobClient(fileEntity.FileName);
        using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, headers);

        return blobClient.Uri.ToString();
    }

    public async Task SaveToDatabaseAsync(FileEntity fileEntity)
    {
        _context.Add(fileEntity);
        await _context.SaveChangesAsync();
    }

}
