using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LFE.DataTokens
{
    public class Input
    {
        public string key { get; set; }
        public string frameRate { get; set; }
        public string resolution { get; set; }
        public string aspectRatio { get; set; }
        public string interlaced { get; set; }
        public string container { get; set; }
    }

    public class Output
    {
        public string id { get; set; }
        public string presetId { get; set; }
        public string key { get; set; }
        public string rotate { get; set; }
        public string segmentDuration { get; set; }
        public string status { get; set; }
        public string statusDetail { get; set; }
        public string duration { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string thumbnailPattern { get; set; }
    }

    public class Playlist
    {
        public string name { get; set; }
        public string format { get; set; }
        public List<string> outputKeys { get; set; }
        public string status { get; set; }
    }

    public class UserMetadata
    {
        public string bcId { get; set; }
    }

    public class Message
    {
        public string state { get; set; }
        public string version { get; set; }
        public string jobId { get; set; }
        public string pipelineId { get; set; }
        public Input input { get; set; }
        public string outputKeyPrefix { get; set; }
        public List<Output> outputs { get; set; }
        public List<Playlist> playlists { get; set; }
        public UserMetadata userMetadata { get; set; }
    }

    public class TranscoderResponseMessage
    {
        public string Type { get; set; }
        public string MessageId { get; set; }
        public string TopicArn { get; set; }
        public string Subject { get; set; }
        public Message Message { get; set; }
        public string Timestamp { get; set; }
        public string SignatureVersion { get; set; }
        public string Signature { get; set; }
        public string SigningCertURL { get; set; }
        public string UnsubscribeURL { get; set; }
    }
}
