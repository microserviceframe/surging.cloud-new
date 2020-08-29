namespace Surging.Core.ApiGateWay.OAuth
{
    public enum ValidateResult
    {
        Success,

        TokenExpired,

        SignatureError,

        TokenFormatError
    }
}
