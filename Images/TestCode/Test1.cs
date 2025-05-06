using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageMessageLib;

namespace TestCode
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void Serialize_ShouldReturnCorrectFormat()
        {
            // Crea una imagen con ID y Payload conocidos
            var img = new ImageMessage { Id = 42, Payload = "imagenCodificada123" };

            // Serializa la imagen
            var result = img.Serialize();

            // Verifica que la cadena tenga el formato esperado "Id:Payload"
            Assert.AreEqual("42:imagenCodificada123", result);
        }

        [TestMethod]
        public void Deserialize_ShouldCreateCorrectImageMessage()
        {
            // Cadena serializada simulando una imagen
            var raw = "17:base64imagen";

            // Convierte la cadena en un objeto ImageMessage
            var result = ImageMessage.Deserialize(raw);

            // Verifica que los valores sean correctos
            Assert.AreEqual(17, result.Id);
            Assert.AreEqual("base64imagen", result.Payload);
        }

        [TestMethod]
        [ExpectedException(typeof(System.FormatException))]
        public void Deserialize_ShouldThrowIfIdIsNotInteger()
        {
            // Cadena con un ID no numérico
            var raw = "notanumber:algo";

            // Debe lanzar una excepción al convertir el ID
            ImageMessage.Deserialize(raw);
        }

        [TestMethod]
        [ExpectedException(typeof(System.IndexOutOfRangeException))]
        public void Deserialize_ShouldThrowIfMissingColon()
        {
            // Cadena sin separador ':' (formato incorrecto)
            var raw = "12345";

            // Debe lanzar una excepción por falta de partes
            ImageMessage.Deserialize(raw);
        }
    }
}

