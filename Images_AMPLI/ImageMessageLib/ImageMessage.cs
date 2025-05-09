﻿namespace ImageMessageLib
{
    public class ImageMessage
    {
        public int seqn { get; set; }
        public string Payload { get; set; }

        public string Serialize() => $"{seqn}:{Payload}"; //serializa la imagen

        public static ImageMessage Deserialize(string raw)//Deserializa la magen
        {
            var parts = raw.Split(':');
            return new ImageMessage
            {
                seqn = int.Parse(parts[0]),
                Payload = parts[1]
            };
        }
    }

    public interface IImageProcessor //Interfaces para hacerlo intercambiable
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

