namespace Web_Ban_Dong_Ho.Abstractions
{
    public class IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string passwordHash, string inputPassword);
    }
}
