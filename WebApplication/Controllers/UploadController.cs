using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using WebApplication.Filters;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class UploadController :
        ControllerBase
    {
        [HttpPost("[action]")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> AsFormFile(
            [FromForm] AsFormFileRequestDto dto)
        {
            await dto.File.CopyToAsync(Stream.Null);
            return Ok();
        }

        [HttpPost("[action]")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> AsStream()
        {
            var request = HttpContext.Request;
            
            if (!request.HasFormContentType
                || !MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeader)
                || string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            {
                return new UnsupportedMediaTypeResult();
            }

            var reader = new MultipartReader(mediaTypeHeader.Boundary.Value, request.Body);
            var section = await reader.ReadNextSectionAsync();

            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                    out var contentDisposition);

                if (hasContentDispositionHeader
                    && contentDisposition!.DispositionType.Equals("form-data")
                    && !string.IsNullOrEmpty(contentDisposition.FileName.Value))
                {
                    await section.Body.CopyToAsync(Stream.Null);

                    return Ok();
                }

                section = await reader.ReadNextSectionAsync();
            }

            return BadRequest("No files data in the request.");
        }

        public sealed record AsFormFileRequestDto(
            IFormFile File);
    }
}