using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageMessageLib;

namespace CodeTest
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void SerializeDeserialize_ShouldPreserveData()
        {
            // Crea una instancia de ImageMessage con datos de ejemplo
            var original = new ImageMessage
            {
                seqn = 42,
                Payload = "base64encodedimage"
            };

            // Serializa y luego deserializa el mensaje
            var serialized = original.Serialize();
            var deserialized = ImageMessage.Deserialize(serialized);

            // Verifica que los datos se mantengan iguales después del ciclo
            Assert.AreEqual(original.seqn, deserialized.seqn);
            Assert.AreEqual(original.Payload, deserialized.Payload);
        }
        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Deserialize_InvalidFormat_ShouldThrowException()
        {
            // Simula un formato de string incorrecto (sin el ":")
            var invalidString = "42base64encodedimage";

            // Intenta deserializar y espera que lance una excepción
            ImageMessage.Deserialize(invalidString);
        }
    }
}
