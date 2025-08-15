namespace Micon.CMS.Services
{
    public interface IAuthorityScanService
    {
        public HashSet<string> Authority { get; }
        public void Scan();
    }
}
