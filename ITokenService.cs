namespace UserAPI
{
    public interface ITokenService
    {
        public string GenerateToken(int userId, string role);

    }
}
