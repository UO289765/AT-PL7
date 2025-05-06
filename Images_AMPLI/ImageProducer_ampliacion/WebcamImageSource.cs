using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMessageLib;
using OpenCvSharp;
namespace ImageProducer_ampliacion
{
    class WebcamImageSource : IImageSource
    {
        private int id = 1;
        private VideoCapture capture;

        public WebcamImageSource()
        {
            capture = new VideoCapture(0);
            if (!capture.IsOpened())
                throw new Exception("No se pudo abrir la webcam.");
        }

        public ImageMessage GetNextImage()
        {
            using var frame = new Mat();
            capture.Read(frame);

            if (frame.Empty())
                throw new Exception("No se pudo capturar la imagen de la webcam."); //coge los frames y los pasa a base64

            var buffer = frame.ToBytes(".jpg");
            var base64 = Convert.ToBase64String(buffer);

            return new ImageMessage
            {
                seqn = id++,
                Payload = base64
            };
        }
    }
}
