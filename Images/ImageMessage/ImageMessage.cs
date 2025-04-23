

namespace ImageMessageLib
{
    public class ImageMessage
    {
        public int Id { get; set; }
        public string Payload { get; set; }

        public string Serialize() => $"{Id}:{Payload}";

        public static ImageMessage Deserialize(string raw)
        {
            var parts = raw.Split(':');
            return new ImageMessage
            {
                Id = int.Parse(parts[0]),
                Payload = parts[1]
            };
        }
    }
    public interface IImageProcessor
    {
        string Process(string imagePayload);
    }

    public interface IImageViewer
    {
        void Display(ImageMessage image);
    }

    public interface IImageSource
    {
        ImageMessage GetNextImage();
    }
}

