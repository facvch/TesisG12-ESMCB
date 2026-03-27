namespace BlazorFrontEnd.Auth
{
    /// <summary>
    /// In-memory token store that survives across HTTP calls within the same circuit.
    /// Populated after login; used by JwtAuthorizationMessageHandler as fallback
    /// when localStorage (JS interop) is not available during SSR.
    /// </summary>
    public class TokenStorageService
    {
        public string? Token { get; private set; }

        public void SetToken(string token) => Token = token;
        public void ClearToken() => Token = null;
    }
}
