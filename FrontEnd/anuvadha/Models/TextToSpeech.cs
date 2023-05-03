namespace anuvadha.Models
{
    public class TextToSpeech
    {
    }

    public class Audio
    {
        public string audioContent { get; set; }
    }



    public class Config
    {
        public Language language { get; set; }
        public string audioFormat { get; set; }
        public string encoding { get; set; }
        public int samplingRate { get; set; }
    }



    public class Language
    {
        public string sourceLanguage { get; set; }
    }



    public class Root
    {
        public List<Audio> audio { get; set; }
        public Config config { get; set; }
    }
}
