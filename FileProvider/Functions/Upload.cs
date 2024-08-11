using Data.Contexts;
using Data.Entities;
using FileProvider.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FileProvider.Functions;

public class Upload(ILogger<Upload> logger, FileService fileService)
{
    private readonly ILogger<Upload> _logger = logger;
    private readonly FileService _fileService;

    [Function("Upload")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        try
        {
            if (req.Form.Files["file*"] is IFormFile file)
            {
                var fileEntity = new FileEntity
                {
                    FileName = _fileService.SetFileName(file),
                    ContentType = file.ContentType,
                    ContainerName = "profiles"
                };

                await _fileService.SetBlobContainerAsync(fileEntity.ContainerName);
                fileEntity.FilePath = await _fileService.UploadFileAsync(file, fileEntity);

                await _fileService.SaveToDatabaseAsync(fileEntity);
                return new OkObjectResult(fileEntity);
            }
                
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
       
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}
