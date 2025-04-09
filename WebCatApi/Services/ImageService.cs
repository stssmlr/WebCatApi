﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using WebCatApi.Abstract;

namespace WebCatApi.Services;

public class ImageService : IImageService
{
    private readonly string _imgPath;
    private readonly IConfiguration _config;

    public ImageService(IConfiguration config)
    {
        _config = config;
        _imgPath = Path.GetFullPath(config["ImagesDir"] ?? throw new ArgumentException("ImagesDir configuration is missing"));

        Directory.CreateDirectory(_imgPath);

        if (!IsDirectoryWritable(_imgPath))
        {
            throw new UnauthorizedAccessException($"No write access to directory: {_imgPath}");
        }
    }

    private static bool IsDirectoryWritable(string dirPath)
    {
        try
        {
            using var fs = File.Create(
                Path.Combine(dirPath, Path.GetRandomFileName()),
                1,
                FileOptions.DeleteOnClose);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> SaveImageAsync(IFormFile image)
    {
        using MemoryStream ms = new();
        await image.CopyToAsync(ms);
        return await SaveImageAsync(ms.ToArray());
    }

    public async Task<List<string>> SaveImagesAsync(IEnumerable<IFormFile> images)
    {
        var resultTasks = images.AsParallel().Select(x => SaveImageAsync(x));
        return [.. (await Task.WhenAll(resultTasks.ToArray()))];
    }

    public async Task<string> SaveImageAsync(string base64)
    {
        if (base64.Contains(','))
        {
            base64 = base64.Split(',')[1];
        }
        var bytes = Convert.FromBase64String(base64);
        return await SaveImageAsync(bytes);
    }

    public async Task<string> SaveImageAsync(byte[] bytes)
    {
        string imageName = $"{Path.GetRandomFileName()}.webp";

        var tasks = Sizes
            .AsParallel()
            .Select(s => SaveImageAsync(bytes, imageName, s))
            .ToArray();

        await Task.WhenAll(tasks);
        return imageName;
    }

    private async Task SaveImageAsync(byte[] bytes, string name, int size)
    {
        string imagePath = Path.Combine(_imgPath, $"{size}_{name}");

        using var image = Image.Load(bytes);
        try
        {
            image.Mutate(imageProcessingContext =>
            {
                imageProcessingContext.Resize(new ResizeOptions
                {
                    Size = new Size(Math.Min(image.Width, size), Math.Min(image.Height, size)),
                    Mode = ResizeMode.Max
                });
            });
            await image.SaveAsync(imagePath, new WebpEncoder());
        }
        catch (Exception e)
        {
            DeleteImageIfExists(imagePath);
            throw new Exception(e.Message);
        }
    }

    public async Task<List<string>> SaveImagesAsync(IEnumerable<byte[]> bytesArrays)
    {
        var resultTasks = bytesArrays.AsParallel().Select(x => SaveImageAsync(x));
        return [.. (await Task.WhenAll(resultTasks.ToArray()))];
    }

    public async Task<byte[]> LoadBytesAsync(string name) => await File.ReadAllBytesAsync(Path.Combine(_imgPath, name));

    public void DeleteImage(string nameWithFormat) => Sizes.AsParallel()
        .ForAll(x => File.Delete(Path.Combine(_imgPath, $"{x}_{nameWithFormat}")));

    public void DeleteImages(IEnumerable<string> images) => images.AsParallel().ForAll(x => DeleteImage(x));

    public void DeleteImageIfExists(string nameWithFormat)
    {
        Sizes.AsParallel().ForAll(x =>
        {
            var path = Path.Combine(_imgPath, $"{x}_{nameWithFormat}");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        });
    }

    public void DeleteImagesIfExists(IEnumerable<string> images)
    {
        images.AsParallel().ForAll(x => DeleteImageIfExists(x));
    }

    public async Task<string> SaveImageFromUrlAsync(string imageUrl)
    {
        using var httpClient = new HttpClient();
        var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
        return await SaveImageAsync(imageBytes);
    }

    private List<int> Sizes
    {
        get
        {
            List<int> sizes = _config.GetRequiredSection("ImageSizes").Get<List<int>>()
               ?? throw new Exception("Error read image sizes");
            if (sizes.Count == 0)
            {
                throw new Exception("Image sizes not initializing");
            }
            return sizes;
        }
    }
}