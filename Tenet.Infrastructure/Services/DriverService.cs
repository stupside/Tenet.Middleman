using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Tenet.Application.Helpers;
using Tenet.Application.Services;
using Tenet.Domain.Entities;
using Tenet.Infrastructure.Helpers;
using Tenet.Persistence.Contexts;

namespace Tenet.Infrastructure.Services
{
    public class DriverService : IDriverService
    {
        private readonly IClient _client;
        private readonly Context _context;
        private readonly IDataProtector _protector;

        public DriverService(Context context, IDataProtectionProvider provider, IClient client)
        {
            _context = context;
            _protector = provider.CreateProtector(nameof(DriverInstance));
            _client = client;
        }

        public async Task DeleteOldinstances(string key, string ip)
        {
            IEnumerable<DriverInstance> olds = await _context.DriverInstances.Where(m => m.Key == key || m.Ip == ip)
                .ToListAsync();
            _context.DriverInstances.RemoveRange(olds);

            await _context.SaveChangesAsync();
        }

        public async Task<string> CreateInstance(int pid, string key, string ip)
        {
            if (pid == 0 || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(ip))
                return null;

            // We delete old instances
            await DeleteOldinstances(key, ip);

            Driver driver = await _context.Drivers.SingleOrDefaultAsync(m => m.Pid == pid);

            if (driver == null)
                return null;

            var entity = await _context.DriverInstances.AddAsync(new DriverInstance()
            {
                DriverId = driver.Id,
                Key = key,
                Expiry = DateTime.UtcNow.AddSeconds(driver.Expiry),
                Ip = ip
            });
            await _context.SaveChangesAsync();

            return ToInstance(entity.Entity.Id, ip, key);
        }

        public async Task<(string encrypted, string iv, string hash)> GetInstance(string key, string ip, string ist)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(ip))
                return (null, null, null);

            (string oid, string oip, string okey) = FromInstance(ist);
            if (!okey.Equals(key) || !oip.Equals(ip))
                return (null, null, null);

            DriverInstance instance = await _context.DriverInstances
                .Include(m => m.Driver)
                .ThenInclude(m => m.DriverContent)
                .SingleOrDefaultAsync(m => m.Key == key && m.Ip == ip && m.Id == oid);

            if (instance == null)
                return (null, null, null);

            if (instance.Expiry <= DateTime.UtcNow)
            {
                _context.DriverInstances.Remove(instance);
                await _context.SaveChangesAsync();
                return (null, null, null);
            }

            string iv = Encryption.iv_key();
            string encrypted = Encryption.Encrypt(instance.Driver.DriverContent.Bytes, instance.Driver.Secret, iv);

            string response = string.Concat(new string[] { encrypted, iv });
            return (encrypted, iv, Encryption.GetSha256Hash(response));
        }

        public async Task<IEnumerable<DriverInstance>> GetInstances()
        {
            return await _context.DriverInstances.ToListAsync();
        }

        private string ToInstance(string id, string ip, string key)
        {
            string joined = string.Join('.', new string[]{ id, ip, key });

            return _protector.Protect(joined);
        }

        private (string id, string ip, string key) FromInstance(string instance)
        {
            try
            {
                instance = _protector.Unprotect(instance);
            }
            catch
            {
                return (string.Empty, String.Empty, String.Empty);
            }

            string[] values = instance.Split('.');
            return (values[0], values[1], values[2]);
        }
    }
}
