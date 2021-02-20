using System.Collections.Generic;
using System.Threading.Tasks;
using Tenet.Domain.Entities;

namespace Tenet.Application.Services
{
    public interface IDriverService
    {
        Task<IEnumerable<DriverInstance>> GetInstances();
        Task DeleteOldInstances(string key);
        Task<string> CreateInstance(int pid, string key);
        Task<(string encrypted, string iv, string hash)> GetInstance(string key, string ist);
    }
}
