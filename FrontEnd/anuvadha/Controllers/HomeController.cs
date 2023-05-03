using anuvadha.Models;
using Google.Cloud.Translation.V2;
using Microsoft.AspNetCore.Mvc;
using NAudio.Wave;
using System.Net.Http.Headers;
using System.Text.Json;

namespace anuvadha.Controllers
{
    //[CustomExceptionFilter]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string GOOGLE_TRANSLATION_API_KEY = "AIzaSyBh7oFZYJbqLw47GdiGUCK_miV-18DvfCI";
        private static string S2T_API_URL = "http://127.0.0.1:3000/upload";
        private static string CLONING_API_URL = "http://127.0.0.1:5000/upload";
        private static string T2S_AI4_BH = "https://tts-api.ai4bharat.org/";


        private static string progress;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }



        public async Task<IActionResult> tToS(string text, string languageCode, string RoboticFilePath)
        {
            var client = new HttpClient();
            SpeechToTextRequestModel reqContent = new SpeechToTextRequestModel()
            {
                input = new()
                {
                    new ReqInput() { source = text }
                },
                config = new()
                {
                    gender = "male", //female
                    language = new()
                    {
                        sourceLanguage = languageCode
                    }
                }

            };
            string stringCOntent = JsonSerializer.Serialize(reqContent);
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(T2S_AI4_BH),
                Content = new StringContent(stringCOntent)
                {
                    Headers = {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };

            using (var response = await client.SendAsync(request))
            {

                response.EnsureSuccessStatusCode();
                var resString = await response.Content.ReadAsStringAsync();

                SpeechToTextResponceModel responceModel = Newtonsoft.Json.JsonConvert.
                    DeserializeObject<SpeechToTextResponceModel>(resString);

                var base64String = responceModel.audio[0].audioContent;
                byte[] binaryData = Convert.FromBase64String(base64String);

                System.IO.File.WriteAllBytes(RoboticFilePath, binaryData);
                _logger.LogInformation("Robotic Voice generated at " + RoboticFilePath);
                return Ok();
            }
        }

        private async Task<string> TransScript(string fileName, string AudioPath)
        {
            using (HttpClient client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), S2T_API_URL))
                {
                    var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(AudioPath));
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav");
                    var formData = new MultipartFormDataContent();
                    formData.Add(fileContent, "audio", "" + fileName);
                    request.Content = formData;

                    using (var response = await client.SendAsync(request))
                    {
                        response.EnsureSuccessStatusCode();

                        var responseContent = await response.Content.ReadAsStringAsync();

                        var TranscriptedText = JsonSerializer.Deserialize<InputText>(responseContent);
                        string transcript = TranscriptedText.message;
                        return transcript;

                    };

                };
            };
        }

        private string[] TranslateText(string text, string targetLanguage)
        {
            string[] result = new string[2];
            using (var client = TranslationClient.CreateFromApiKey(GOOGLE_TRANSLATION_API_KEY))
            {
                var response = client.TranslateText(text, targetLanguage);
                result[0] = response.TranslatedText;
                result[1] = response.OriginalText;
                return result;
            };


        }

        public async Task<IActionResult> Clone(string robotic, string original, string clonedAudioFilePath)
        {

            //cloning
            var client = new HttpClient();
            using (var request = new HttpRequestMessage(new HttpMethod("POST"),
                CLONING_API_URL))
            {
                var RoboticFileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(robotic));
                var OriginaltFileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(original));

                RoboticFileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav");
                OriginaltFileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav");

                var formData = new MultipartFormDataContent();
                formData.Add(RoboticFileContent, "audio", "robotic.wav");
                formData.Add(OriginaltFileContent, "audio", "original.wav");

                request.Content = formData;
                var response = await client.SendAsync(request);

                var clonedAudioStream = await response.Content.ReadAsStreamAsync();

                WaveFileReader waveFileReader = new WaveFileReader(clonedAudioStream);
                WaveFileWriter.CreateWaveFile(clonedAudioFilePath, waveFileReader);

                _logger.LogInformation("cloned voice audio file saved at " + clonedAudioFilePath);

                return Ok();

            }
        }


        public IActionResult Index()
        {
            return View();
        }
       
        
        [HttpPost]
        public async Task<IActionResult> PlayerAsync(IFormFile PostedAudioFile, string languages)
        {

            progress = "Uploading File...";


            string filenamePrefix = PostedAudioFile.FileName.Replace(".wav", "") + "_" + languages;
            string RootDirectory = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\AudioFiles\\{filenamePrefix}\\");

            string OriginalfileName = filenamePrefix + "_Original.wav";

            string RoboticFileName = filenamePrefix + "_Translated_robotic.wav";
            string RoboticFilePath = Path.Combine(RootDirectory, RoboticFileName);

            string clonedFileName = filenamePrefix + "_Translated_cloned.wav";
            string clonedAudioFilePath = Path.Combine(RootDirectory, clonedFileName);

            if (!Directory.Exists(RootDirectory))
            {
                Directory.CreateDirectory(RootDirectory);
            }

            //saving original
            string OriginalAudioFilePath = Path.Combine(RootDirectory, OriginalfileName);
            using (FileStream stream = new FileStream(OriginalAudioFilePath, FileMode.Create))
            {
                PostedAudioFile.CopyTo(stream);
            }

            _logger.LogInformation("FileUploaded : " + OriginalAudioFilePath);
            //------------------------------------------------------------------

            progress = "File Has been uploaded, Extracting text from the audio file...";
            //>>>transcript
            _logger.LogInformation(">>>>>Transcripting");
            string transcript = await TransScript(OriginalfileName, OriginalAudioFilePath);
            _logger.LogInformation("TransScription : " + transcript);
            //------------------------------------------------------------------
         

            progress = "Text Extraction done, Translating into target language...";
            //>>>Translate
            //translations[0] -->translatied text
            //translations[1] -->Original Text
            _logger.LogInformation(">>>>>Translating to !" + languages);
            string TargetLanguage = languages;
            var translations = TranslateText(transcript, TargetLanguage);
            string TranslatedText = translations[0];
            _logger.LogInformation("Translation done !");


            progress = "Translation Done, Generating audio from translated text...";
            //>>>textToSpeech --robotic
            _logger.LogInformation(">>>>>Generating speech from transcript!");
            var RoboticAudioFileBytes = await tToS(TranslatedText, languages, RoboticFilePath);

            //>>>CloneVoice
            progress = "Speech generated, Trying to clone into original voice, this might take while...";
            _logger.LogInformation(">>>>>Cloning voice!");
            var clonedAudioStream = await Clone(RoboticFilePath, OriginalAudioFilePath, clonedAudioFilePath);

            progress = "Coning done!!, Generating result...." ;

            Thread.Sleep(3000);

            progress = "loading ...";

            ViewBag.RoboticTranslated = "data:audio/wav;base64," +
                Convert.ToBase64String(
                    System.IO.File.ReadAllBytes(RoboticFilePath));

            ViewBag.OriginalFile = "data:audio/wav;base64," +
                Convert.ToBase64String(System.IO.File.ReadAllBytes(OriginalAudioFilePath));

            ViewBag.ClonedAudio = "data:audio/wav;base64," +
                Convert.ToBase64String(System.IO.File.ReadAllBytes(clonedAudioFilePath));

            ViewBag.TranslatedText = TranslatedText;
            ViewBag.Transcript = transcript;

            return View("temp");

        }

       
        public IActionResult Dummy()
        {
            return View();
        }

        public IActionResult Dummy2()
        {
            return View();
        }

        public IActionResult GetStatus()
        {
        //    Console.WriteLine($"progress  ------------------------------- {progress}");
            //        return Json(progress);
            string v = progress;
            return Ok(v);
        }

    }
}