namespace PaymentService.API.Service
{
    public interface IPayOsSignature
    {
        bool Verify(byte[] body, string signatureHex);
    }
}
