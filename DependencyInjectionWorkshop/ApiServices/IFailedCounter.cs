namespace DependencyInjectionWorkshop.ApiServices
{
    public interface IFailedCounter
    {
        void Add(string account);

        bool CheckAccountIsLocked(string account);

        int Get(string account);

        void Reset(string account);
    }
}