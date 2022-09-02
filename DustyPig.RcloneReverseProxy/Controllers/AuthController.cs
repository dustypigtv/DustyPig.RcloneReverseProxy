using Krypto.WonderDog;
using Krypto.WonderDog.Symmetric;
using Microsoft.AspNetCore.Mvc;

namespace DustyPig.RcloneReverseProxy.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        static readonly Key _key = new Key(GetEnvironmentVariable("DUSTY_PIG_RCLONE_KEY"));
        static readonly ISymmetric _aes = SymmetricFactory.CreateAES();

        static readonly string[] ALLOWED_EXTENSIONS = new string[] { ".mp4", ".jpg", ".bif", ".srt", ".m3u8", ".ts", ".webvtt" };

        static string GetEnvironmentVariable(string name)
        {
            string ret = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);

            if (string.IsNullOrWhiteSpace(ret))
                ret = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);

            if (string.IsNullOrWhiteSpace(ret))
                ret = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);

            return ret;
        }


        [HttpGet]
        public IActionResult Get()
        {
            //Don't catch exceptions - if it fails, let nginx deny the request


            //Get the query params
            var query = Request.Headers["X-Original-URI"][0];
            query = query.Substring(query.IndexOf("?") + 1);
 
            var queryParams = query.Split('&', StringSplitOptions.RemoveEmptyEntries).Select(item =>
            {
                var parts = item.Split('=');
                return new KeyValuePair<string, string>(parts[0], parts[1]);
            }).ToList();

            //Get the file param
            var encFile = queryParams.First(item => item.Key.Equals("file", StringComparison.CurrentCultureIgnoreCase)).Value;

            //Restore the correct base64
            encFile = encFile.Replace("_", "/");

            var decFile = _aes.Decrypt(_key, encFile);

            //Make sure the file is allowed
            var ext = Path.GetExtension(encFile).ToLower();
            if (!ALLOWED_EXTENSIONS.Any(item => item == ext))
                throw new Exception("Invalid file");


            Response.Headers.Add("rclone_path", decFile);

            return Ok();
        }
    }
}