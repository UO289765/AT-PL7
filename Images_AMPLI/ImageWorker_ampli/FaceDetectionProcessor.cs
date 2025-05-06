using ImageMessageLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;


namespace ImageWorker_ampli
{
    public class FaceDetectionProcessor : IImageProcessor
    {
        private CascadeClassifier faceCascade;

        public FaceDetectionProcessor()
        {
            faceCascade = new CascadeClassifier("haarcascade_frontalface_default.xml"); //Modelo para al detección de caras
           
        }

        public string Process(string imagePayload) //Procesar las imagenes
        {
            byte[] imageBytes = Convert.FromBase64String(imagePayload);
            using var mat = Cv2.ImDecode(imageBytes, ImreadModes.Color);

            var faces = faceCascade.DetectMultiScale(mat, 1.1, 4);

            foreach (var face in faces)
            {
                Cv2.Rectangle(mat, face, Scalar.Red, 2); //Añadir rectángulo rojo en la cara detectada
            }

            var resultBuffer = mat.ToBytes(".jpg");
            return Convert.ToBase64String(resultBuffer);
        }
    }
}