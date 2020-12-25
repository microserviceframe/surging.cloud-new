namespace Surging.Cloud.ApiGateWay.OAuth
{
    public enum ValidateResult
    {
        Success,

        TokenExpired,

        SignatureError,

        TokenFormatError
    }
}
