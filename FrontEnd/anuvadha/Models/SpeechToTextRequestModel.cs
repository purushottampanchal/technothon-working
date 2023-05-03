namespace anuvadha.Models
{
    public class SpeechToTextRequestModel
    {
        public List<ReqInput> input { get; set; }
        public ReqConfig config { get; set; }
    }

    public class ReqConfig
    {
        public string gender { get; set; }
        public ReqLanguage language { get; set; }
    }

    public class ReqInput
    {
        public string source { get; set; }
    }

    public class ReqLanguage
    {
        public string sourceLanguage { get; set; }
    }

}
