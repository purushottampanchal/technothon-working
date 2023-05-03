using anuvadha.Models;
using Google.Cloud.Translation.V2;
using Microsoft.AspNetCore.Mvc;
using NAudio.Wave;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Microsoft.CognitiveServices.Speech;

namespace anuvadha.Controllers
{
    public class EmotionController : Controller
    {
        private readonly ILogger<EmotionController> _logger;


        public EmotionController(ILogger<EmotionController> logger)
        {
            _logger = logger;

        }

        private static string progress;

        private readonly string GOOGLE_TRANSLATION_API_KEY = "AIzaSyBh7oFZYJbqLw47GdiGUCK_miV-18DvfCI";
        private static string S2T_API_URL = "http://127.0.0.1:3000/upload";
        private static string CLONING_API_URL = "http://127.0.0.1:5000/upload";
        private static string T2S_SSML_API = "http://localhost:8008/text/";

        private static string SPEECH_KEY = "35d6cc66d94b4387b886ce8c18f0f952";
        private static string SPEECH_REGN = "eastus";

        public async Task<IActionResult> tToS(string text, string languageCode, string RoboticFilePath)
        {

            var client = TranslationClient.CreateFromApiKey(GOOGLE_TRANSLATION_API_KEY);
            var response = client.TranslateText(text, "en");

            HttpClient httpClient = new HttpClient();
            Data res = new Data();
            var apiResponce = await httpClient.GetAsync(T2S_SSML_API + response.TranslatedText);


            if (apiResponce.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string res1 = await apiResponce.Content.ReadAsStringAsync();
                try
                {
                    res = JsonConvert.DeserializeObject<Data>(res1);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                };



                Data translated_result = new Data();
                List<string> t = new List<string>();
                List<string> m = new List<string>();
                int index1 = 0;

                foreach (var i in res.text)
                {

                    var response1 = client.TranslateText(i, languageCode);
                    t.Add(response1.TranslatedText);
                    m.Add(res.emotion[index1]);
                    index1++;
                }
                translated_result.text = t;
                translated_result.emotion = m;

                int index = 0;
                string resultSSML = null;
                foreach (var i in translated_result.text)
                {
                    string tags = null;

                    switch (translated_result.emotion[index])
                    {
                        case "happy":
                            tags = string.Format("<prosody pitch=\"+10%\" volume=\"+30%\" range=\"high\">{0}</prosody>", i);
                            break;
                        case "sad":
                            tags = string.Format("<prosody volume=\"+30%\" range=\"high\" pitch=\"-5%\">{0}</prosody>", i);
                            break;
                        case "angry":
                            tags = string.Format("<prosody pitch=\"-10%\" rate=\"medium\" range=\"high\" volume=\"loud\">{0}</prosody>", i);
                            break;
                        case "love":
                            tags = string.Format("<prosody pitch=\"+10%\" rate=\"medium\">{0}</prosody>", i);
                            break;
                        case "neutral":
                            tags = string.Format("<prosody volume=\"medium\" rate=\"medium\">{0}</prosody>", i);
                            break;

                    }
                    resultSSML += tags;
                    index++;
                }

                string start = "";
                switch (languageCode)
                {
                    case "hi":
                        start = "<speak xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"http://www.w3.org/2001/mstts\" xmlns:emo=\"http://www.w3.org/2009/10/emotionml\" version=\"1.0\" xml:lang=\"hi-IN\"><voice name=\"hi-IN-MadhurNeural\">";
                        break;
                    case "bn":
                        start = "<speak xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"http://www.w3.org/2001/mstts\" xmlns:emo=\"http://www.w3.org/2009/10/emotionml\" version=\"1.0\" xml:lang=\"bn-IN\"><voice name=\"bn-IN-BashkarNeural\">";
                        break;
                    case "gu":
                        start = "<speak xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"http://www.w3.org/2001/mstts\" xmlns:emo=\"http://www.w3.org/2009/10/emotionml\" version=\"1.0\" xml:lang=\"gu-IN\"><voice name=\"gu-IN-NiranjanNeural\">";
                        break;
                    case "mr":
                        start = "<speak xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"http://www.w3.org/2001/mstts\" xmlns:emo=\"http://www.w3.org/2009/10/emotionml\" version=\"1.0\" xml:lang=\"mr-IN\"><voice name=\"mr-IN-ManoharNeural\">";
                        break;
                    case "ml":
                        start = "<speak xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"http://www.w3.org/2001/mstts\" xmlns:emo=\"http://www.w3.org/2009/10/emotionml\" version=\"1.0\" xml:lang=\"ml-IN\"><voice name=\"ml-IN-MidhunNeural\">";
                        break;
                    case "ne":
                        start = "<speak xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"http://www.w3.org/2001/mstts\" xmlns:emo=\"http://www.w3.org/2009/10/emotionml\" version=\"1.0\" xml:lang=\"ne-NP\"><voice name=\"ne-NP-SagarNeural\">";
                        break;
                    case "ta":
                        start = "<speak xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"http://www.w3.org/2001/mstts\" xmlns:emo=\"http://www.w3.org/2009/10/emotionml\" version=\"1.0\" xml:lang=\"ta-IN\"><voice name=\"ta-SG-AnbuNeural\">";
                        break;
                    case "te":
                        start = "<speak xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"http://www.w3.org/2001/mstts\" xmlns:emo=\"http://www.w3.org/2009/10/emotionml\" version=\"1.0\" xml:lang=\"te-IN\"><voice name=\"te-IN-MohanNeural\">";
                        break;
                    case "si":
                        start = "<speak xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"http://www.w3.org/2001/mstts\" xmlns:emo=\"http://www.w3.org/2009/10/emotionml\" version=\"1.0\" xml:lang=\"si-LK\"><voice name=\"si-LK-SameeraNeural\">";
                        break;
                }



                var speechConfig = SpeechConfig.FromSubscription(SPEECH_KEY, SPEECH_REGN);

                // The language of the voice that speaks.
                //speechConfig.SpeechSynthesisVoiceName = "en-US-JennyNeural";

                using (var speechSynthesizer = new SpeechSynthesizer(speechConfig, null))
                {
                    // Get text from the console and synthesize to the default speaker.
                    var ssml = start + resultSSML + "</voice></speak>";

                    var speechSynthesisResult = await speechSynthesizer.SpeakSsmlAsync(ssml);

                    var stream = AudioDataStream.FromResult(speechSynthesisResult);
                    await stream.SaveToWaveFileAsync(RoboticFilePath);
                }

                //_logger.LogInformation("Robotic Voice generated at " + RoboticFilePath);
                return Ok();

            }
            else
            {
                string msg = await apiResponce.Content.ReadAsStringAsync();
                _logger.LogError($"Error in t2s| {apiResponce.StatusCode} | {msg} ");
                return Ok();
            }

        }

        public IActionResult Index()
        {
            return View();
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

                        var TranscriptedText = System.Text.Json.JsonSerializer.Deserialize<InputText>(responseContent);
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



        [HttpPost]
        public async Task<IActionResult> PlayerAsync(IFormFile PostedAudioFile, string languages)
        {

            progress = "Uploading File...";

            string filenamePrefix = PostedAudioFile.FileName.Replace(".wav", "")+"_"+languages;
            string RootDirectory = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\AudioFiles\\{filenamePrefix}\\emotions");

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
            Thread.Sleep(1000);
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

            progress = "Coning done!!, Generating result....";
            Thread.Sleep(3000);

            progress = "Loading...";

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
         //   Console.WriteLine($"progress  ------------------------------- {progress}");
            //        return Json(progress);
            string v = progress;
            return Ok(v);
        }


    }
}
