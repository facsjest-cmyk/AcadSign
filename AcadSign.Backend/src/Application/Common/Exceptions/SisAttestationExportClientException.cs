namespace AcadSign.Backend.Application.Common.Exceptions;

public class SisAttestationExportClientException : Exception
{
    public SisAttestationExportClientException(string message)
        : base(message)
    {
    }

    public SisAttestationExportClientException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
