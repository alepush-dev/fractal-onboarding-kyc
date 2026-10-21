namespace onboardingKycApi.Service
{
    // Se lanza cuando el archivo es una imagen válida, pero la IA no pudo leer sus datos
    // (imagen borrosa, oscura, cortada o que no es un documento de identidad)
    public class ImagenIlegibleException : Exception
    {
        public ImagenIlegibleException(string message) : base(message) { }
    }
}
