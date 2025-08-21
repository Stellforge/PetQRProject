using SimpleProject.Domain.Dtos;
using SimpleProject.Domain;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing;
using System;

namespace SimpleProject.Services;
public interface IFileService : IServiceBase, IScopedService
{
    Task<Result<string>> SaveFile(Stream file, string filePath, bool isUri = false);

    Task<Result> DeleteFile(string filePath, bool isUri = false, IEnumerable<ResizeConfig>? configs = null);
    Task<Result<List<string>>> SaveImage(Stream file, string path, IEnumerable<ResizeConfig>? configs = null, bool isUri = false);

    Task<Result<List<object?[]>>> GetFiles(string path);
    Task<Result> RenameFile(string path, string name);
    Task<Result> CreateFolder(string path, string name);

    Task<Result<string>> MapPath(string uri);
    Task<Result<string>> GetUri(string path);
}

public class FileService : ServiceBase, IFileService
{
    private readonly List<string> _imageExtensions = [".JPG", ".JPEG", ".JPE", ".BMP", ".GIF", ".PNG"];

    public FileService(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public async Task<Result<string>> SaveFile(Stream file, string path, bool isUri = false)
    {
        try
        {
            var filePath = path;
            if (isUri)
            {
                var uriPathResult = await MapPath(path);
                if (uriPathResult.HasError)
                {
                    return uriPathResult;
                }
                filePath = uriPathResult.Data;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("filePath");
            }

            var uriResult = await GetUri(filePath);
            if (uriResult.HasError)
            {
                return new Result<string>(uriResult);
            }

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return new Result<string>() { Data = uriResult.Data };
        }
        catch (Exception ex)
        {
            return new Result<string>(await _logService.LogException(ex));
        }
    }

    public async Task<Result> DeleteFile(string path, bool isUri = false, IEnumerable<ResizeConfig>? configs = null)
    {
        try
        {
            var filePath = path;
            if (isUri)
            {
                var uriPathResult = await MapPath(path);
                if (uriPathResult.HasError)
                {
                    return uriPathResult;
                }
                filePath = uriPathResult.Data;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("filePath");
            }

            if (configs != null && configs.Any())
            {
                var firstConfig = configs.First();
                if (!string.IsNullOrEmpty(firstConfig.Suffix))
                {
                    filePath = filePath[..filePath.LastIndexOf('.')].TrimEnd(firstConfig.Suffix.ToCharArray()) + filePath[filePath.LastIndexOf('.')..];
                }

                foreach (var config in configs)
                {
                    var imagePath = filePath;
                    if (!string.IsNullOrEmpty(config.Suffix))
                    {
                        imagePath = filePath[..filePath.LastIndexOf('.')] + config.Suffix + filePath[filePath.LastIndexOf('.')..];
                    }
                    File.Delete(imagePath);
                }
            }
            else
            {
                File.Delete(filePath);
            }

            return new Result();
        }
        catch (Exception ex)
        {
            return new Result(await _logService.LogException(ex));
        }
    }

    public async Task<Result<List<string>>> SaveImage(Stream file, string path, IEnumerable<ResizeConfig>? configs = null, bool isUri = false)
    {
        try
        {
            var filePath = path;
            if (isUri)
            {
                var uriPathResult = await MapPath(path);
                if (uriPathResult.HasError)
                {
                    return new Result<List<string>>(uriPathResult);
                }
                filePath = uriPathResult.Data;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("filePath");
            }

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var extension = Path.GetExtension(Path.GetFileName(filePath));
            if (!_imageExtensions.Any(a => string.Equals(a, extension, StringComparison.OrdinalIgnoreCase)))
            {
                return new Result<List<string>>("Geçersiz dosya tipi");
            }

            var imageLinks = new List<string>();
            if (configs != null && configs.Any())
            {
                foreach (var config in configs)
                {
                    var imagePath = filePath;
                    if (!string.IsNullOrEmpty(config.Suffix))
                    {
                        imagePath = filePath[..filePath.LastIndexOf('.')] + config.Suffix + filePath[filePath.LastIndexOf('.')..];
                    }
                    SaveImageWithSize(file, imagePath, config);

                    var uriResult = await GetUri(imagePath);
                    if (uriResult.HasError)
                    {
                        return new Result<List<string>>(uriResult);
                    }
                    imageLinks.Add(uriResult.Data ?? "");
                }
            }
            else
            {
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                var uriResult = await GetUri(filePath);
                if (uriResult.HasError)
                {
                    return new Result<List<string>>(uriResult);
                }
                imageLinks.Add(uriResult.Data ?? "");
            }
            return new Result<List<string>>() { Data = imageLinks };
        }
        catch (Exception ex)
        {
            return new Result<List<string>>(await _logService.LogException(ex));
        }
    }

    public async Task<Result<List<object?[]>>> GetFiles(string path)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
            path = string.IsNullOrEmpty(path) ? "/media" : path;
            if (!path.StartsWith("/media"))
            {
                path = "/media/" + path.TrimStart('/');
            }

            var pathResult = await MapPath(path);
            if (pathResult.HasError)
            {
                return new Result<List<object?[]>>(pathResult);
            }

            var currentPath = pathResult.Data!;
            var list = new List<object?[]>();

            path = path[("media".Length + 1)..];
            if (!string.IsNullOrEmpty(path))
            {
                var url = PathCombine([.. SkipLast(path.Split('/'))]);
                list.Add([
                    "dir",
                    "..",
                    url,
                    null,
                    null,
                ]);
            }

            var folders = Directory.GetDirectories(currentPath);
            foreach (var item in folders)
            {
                var folderName = item.Split(Path.DirectorySeparatorChar).Last();
                list.Add([
                    "dir",
                    folderName,
                    PathCombine(path, folderName),
                    null,
                    null,
                ]);
            }

            var files = Directory.GetFiles(currentPath);
            foreach (var item in files)
            {
                var fileName = item.Split(Path.DirectorySeparatorChar).Last();
                var fileInfo = new FileInfo(item);
                list.Add([
                    _imageExtensions.Any(a => string.Equals(a, fileInfo.Extension, StringComparison.OrdinalIgnoreCase)) ? "img" : "file",
                    fileInfo.Name,
                    PathCombine(path, fileInfo.Name),
                    fileInfo.Length.ToString(),
                    fileInfo.CreationTime == DateTime.MinValue ? null : fileInfo.CreationTime,
                ]);
            }

            return new Result<List<object?[]>>() { Data = list };
        }
        catch (Exception ex)
        {
            if (!isTransactional)
            {
                return new Result<List<object?[]>>(await _logService.LogException(ex));
            }
            throw;
        }
    }

    public async Task<Result> RenameFile(string path, string name)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
            path = string.IsNullOrEmpty(path) ? "/upload" : path;
            if (!path.StartsWith("/upload"))
            {
                path = "/upload/" + path.TrimStart('/');
            }

            var pathResult = await MapPath(path);
            if (pathResult.HasError)
            {
                return new Result(pathResult);
            }

            var currentPath = pathResult.Data!;

            if (File.Exists(currentPath))
            {
                var dir = Path.GetDirectoryName(currentPath);
                File.Move(currentPath, Path.Combine(dir!, name));
            }
            else if (Directory.Exists(currentPath) && path != "/upload")
            {
                var dir = string.Join(Path.DirectorySeparatorChar, SkipLast(currentPath.Split(Path.DirectorySeparatorChar)));
                Directory.Move(currentPath, Path.Combine(dir, name));
            }
            else
            {
                return new Result("Dosya yolu bulanadı");
            }
            return new Result();
        }
        catch (Exception ex)
        {
            if (!isTransactional)
            {
                return new Result(await _logService.LogException(ex));
            }
            throw;
        }
    }

    public async Task<Result> CreateFolder(string path, string name)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
            path = string.IsNullOrEmpty(path) ? "/upload" : path;
            if (!path.StartsWith("/upload"))
            {
                path = "/upload/" + path.TrimStart('/');
            }

            var pathResult = await MapPath(path);
            if (pathResult.HasError)
            {
                return new Result(pathResult);
            }

            var currentPath = Path.Combine(pathResult.Data!, name);
            if (!Directory.Exists(currentPath))
            {
                Directory.CreateDirectory(currentPath);
            }
            return new Result();
        }
        catch (Exception ex)
        {
            if (!isTransactional)
            {
                return new Result(await _logService.LogException(ex));
            }
            throw;
        }
    }

    public async Task<Result<string>> MapPath(string uri)
    {
        try
        {
            uri ??= string.Empty;
            uri = uri.TrimStart('~');

            if (_userAccessor == null)
            {
                throw new ArgumentException("IUserAccessor");
            }

            if (uri.TrimStart('/').StartsWith("upload", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(AppSettings.Current.UploadPath))
                {
                    uri = uri.TrimStart('/')[6..].TrimStart('/');
                    return new Result<string>() { Data = Path.Combine(AppSettings.Current.UploadPath, uri.Replace('/', Path.DirectorySeparatorChar)) };
                }
            }

            if (!string.IsNullOrEmpty(_userAccessor.WebRootPath))
            {
                return new Result<string>() { Data = Path.Combine(_userAccessor.WebRootPath, uri.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)) };
            }
            else
            {
                return new Result<string>() { Data = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, uri.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)) };
            }
        }
        catch (Exception ex)
        {
            return new Result<string>(await _logService.LogException(ex));
        }
    }

    public async Task<Result<string>> GetUri(string path)
    {
        try
        {
            if (_userAccessor == null)
            {
                throw new ArgumentException("IUserAccessor");
            }

            if (!string.IsNullOrEmpty(AppSettings.Current.UploadPath))
            {
                if (path.StartsWith(AppSettings.Current.UploadPath, StringComparison.OrdinalIgnoreCase))
                {
                    return new Result<string>() { Data = "/upload/" + path.Replace(AppSettings.Current.UploadPath, "", StringComparison.OrdinalIgnoreCase).Replace(Path.DirectorySeparatorChar, '/').TrimStart('/') };
                }
            }

            if (!string.IsNullOrEmpty(_userAccessor.WebRootPath))
            {
                return new Result<string>() { Data = "/" + path.Replace(_userAccessor.WebRootPath, "", StringComparison.OrdinalIgnoreCase).Replace(Path.DirectorySeparatorChar, '/').TrimStart('/') };
            }
            else
            {
                return new Result<string>() { Data = "/" + path.Replace(AppDomain.CurrentDomain.BaseDirectory, "", StringComparison.OrdinalIgnoreCase).Replace(Path.DirectorySeparatorChar, '/').TrimStart('/') };
            }
        }
        catch (Exception ex)
        {
            return new Result<string>(await _logService.LogException(ex));
        }
    }

    private static void SaveImageWithSize(Stream file, string path, ResizeConfig config)
    {
#pragma warning disable CA1416
        long quality = 100L;
        if ((file.Length / 1024) > config.LowQualitySize)
        {
            quality = 80L;
        }

        Bitmap? targetImg = null;
        try
        {
            using var image = (Bitmap)Image.FromStream(file);

            var isPng = ".png".Equals(Path.GetExtension(path), StringComparison.OrdinalIgnoreCase);

            if (image.Width > config.Width || image.Height > config.Height)
            {
                var widthRatio = (float)config.Width / image.Width;
                var heightRatio = (float)config.Height / image.Height;
                var ratio = Math.Min(widthRatio, heightRatio);
                int width = (int)(image.Width * ratio);
                int height = (int)(image.Height * ratio);

                targetImg = new Bitmap(width, height);
                targetImg.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using var graphics = Graphics.FromImage(targetImg);
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                Clear(graphics, isPng, config);

                using var wrapMode = new ImageAttributes();
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, new Rectangle(0, 0, width, height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }
            else
            {
                var ratio = Math.Min((float)config.Width / image.Width, (float)config.Height / image.Height);
                var newWidth = (int)(image.Width * ratio);
                var newHeight = (int)(image.Height * ratio);
                var left = (int)((config.Width - newWidth) / 2f);
                var top = (int)((config.Height - newHeight) / 2f);

                targetImg = new Bitmap(config.Width, config.Height);
                targetImg.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                using (var graphics = Graphics.FromImage(targetImg))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    Clear(graphics, isPng, config);

                    graphics.DrawImage(image, left, top, newWidth, newHeight);
                }
            }

            if (isPng)
            {
                targetImg.Save(path, ImageFormat.Png);
            }
            else
            {
                ImageCodecInfo codec = ImageCodecInfo.GetImageEncoders().First(c => c.MimeType == "image/jpeg");
                EncoderParameters parameters = new(3);
                parameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                parameters.Param[1] = new EncoderParameter(Encoder.ScanMethod, (int)EncoderValue.ScanMethodInterlaced);
                parameters.Param[2] = new EncoderParameter(Encoder.RenderMethod, (int)EncoderValue.RenderProgressive);
                targetImg.Save(path, codec, parameters);
            }
        }
        catch
        {

            throw;
        }
        finally
        {
            targetImg?.Dispose();
        }
#pragma warning restore CA1416
    }

    private static void Clear(Graphics graphics, bool isPng, ResizeConfig config)
    {
#pragma warning disable CA1416

        if (!string.IsNullOrEmpty(config.BackGround))
        {
            graphics.Clear(ColorTranslator.FromHtml(config.BackGround));
        }
        else
        {
            if (isPng)
            {
                graphics.Clear(Color.Transparent);
            }
            else
            {
                graphics.Clear(Color.White);
            }
        }
#pragma warning restore CA1416
    }

    private static string PathCombine(params string?[] paths)
    {
        if (paths != null)
        {
            paths = [.. paths.Where(a => !string.IsNullOrEmpty(a))];
        }
        if (paths == null || paths.Length == 0)
        {
            return "/media";
        }
        return "/media/" + string.Join("/", paths.Select(a => a?.Trim('/')));
    }
    private static IEnumerable<T?> SkipLast<T>(IEnumerable<T> source)
    {
        T? previous = default;
        bool first = true;
        foreach (T element in source)
        {
            if (!first)
            {
                yield return previous;
            }
            previous = element;
            first = false;
        }
    }

}
