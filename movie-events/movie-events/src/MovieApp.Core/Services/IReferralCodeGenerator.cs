namespace MovieApp.Core.Services;

public interface IReferralCodeGenerator
{
    string Generate(string username, int userId);
}
