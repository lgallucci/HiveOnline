using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace HiveServer
{
    // Clients pin the SHA-256 of the certificate's public key, so as long as the same key is used the
    // certificate can be re-issued or the server recreated without updating any client.
    internal static class TlsCertificate
    {
        // HIVE_TLS=1 enables TLS. The certificate comes from HIVE_CERT_BASE64 (base64 PFX) or HIVE_CERT_PATH
        // (PFX file, default hive-server.pfx), with HIVE_CERT_PASSWORD if set. If neither exists a
        // self-signed certificate is created at the path, which is only suitable for testing.
        public static X509Certificate2? LoadFromEnvironment()
        {
            if (Environment.GetEnvironmentVariable("HIVE_TLS") != "1")
                return null;

            var password = Environment.GetEnvironmentVariable("HIVE_CERT_PASSWORD") ?? string.Empty;
            var base64 = Environment.GetEnvironmentVariable("HIVE_CERT_BASE64");
            X509Certificate2 certificate;

            if (!string.IsNullOrWhiteSpace(base64))
            {
                certificate = X509CertificateLoader.LoadPkcs12(Convert.FromBase64String(base64), password);
            }
            else
            {
                var path = Environment.GetEnvironmentVariable("HIVE_CERT_PATH") ?? "hive-server.pfx";
                if (!File.Exists(path))
                {
                    Create(path, password);
                    Console.WriteLine($"Created self-signed certificate at {path}");
                }

                certificate = X509CertificateLoader.LoadPkcs12FromFile(path, password);
            }

            Console.WriteLine($"TLS enabled. Public key pin: {Pin(certificate)}");
            return certificate;
        }

        // hive-server --generate-cert [file]: creates a long-lived key and prints the pin to put in the client.
        public static void Generate(string path)
        {
            var password = Environment.GetEnvironmentVariable("HIVE_CERT_PASSWORD") ?? string.Empty;
            if (File.Exists(path))
            {
                Console.WriteLine($"{path} already exists; refusing to overwrite it.");
                return;
            }

            var certificate = Create(path, password);
            Console.WriteLine($"Wrote {path}. Keep it secret and backed up.");
            Console.WriteLine($"Public key pin for the client (DefaultServerKeyPin / HIVE_TLS_PIN): {Pin(certificate)}");
        }

        public static string Pin(X509Certificate2 certificate) =>
            Convert.ToHexString(SHA256.HashData(certificate.PublicKey.ExportSubjectPublicKeyInfo()));

        private static X509Certificate2 Create(string path, string password)
        {
            using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            var request = new CertificateRequest("CN=HiveServer", key, HashAlgorithmName.SHA256);
            request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(
                new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));

            using var created = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(30));
            File.WriteAllBytes(path, created.Export(X509ContentType.Pfx, password));
            return X509CertificateLoader.LoadPkcs12FromFile(path, password);
        }
    }
}
