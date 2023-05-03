namespace anuvadha.Models
{
    public class ResAudio
    {
        public string audioContent { get; set; }
    }



    public class ResConfig
    {
        public ResLanguage language { get; set; }
        public string audioFormat { get; set; }
        public string encoding { get; set; }
        public int samplingRate { get; set; }
    }



    public class ResLanguage
    {
        public string sourceLanguage { get; set; }
    }



    public class SpeechToTextResponceModel
    {
        public List<ResAudio> audio { get; set; }
        public ResConfig config { get; set; }
    }
}
