using UnityEngine.Networking;

public class CustomCertificateHandler : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true; // Ini mengabaikan validasi sertifikat (gunakan hanya untuk debugging)
    }
}
