using Newtonsoft.Json;

namespace WinBox.Model
{
    public class Error
    {
        [JsonProperty(PropertyName = "error")]
        public string Message { get; set; }     
    }
}