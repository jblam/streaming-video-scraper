using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JBlam.NetflixScrape.Core.Models
{
    public enum PlaybackState
    {
        Waiting,
        Paused,
        Playing
    }
    public class WatchModel
    {
        /// <summary>
        /// Gets or sets the title of the currently viewed show
        /// </summary>
        public string ShowTitle { get; set; }
        /// <summary>
        /// Gets or sets the show run time
        /// </summary>
        [JsonConverter(typeof(TimeSpanToSecondsConverter))]
        public TimeSpan Duration { get; set; }
        /// <summary>
        /// Gets or sets the current playback position time
        /// </summary>
        [JsonConverter(typeof(TimeSpanToSecondsConverter))]
        public TimeSpan Position { get; set; }
        /// <summary>
        /// Gets or sets the current playback state
        /// </summary>
        public PlaybackState PlaybackState { get; set; }
    }
    internal class TimeSpanToSecondsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(DateTime);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var totalSeconds = (double)reader.Value;
            return TimeSpan.FromSeconds(totalSeconds);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((TimeSpan)value).TotalSeconds);
        }
    }
}
