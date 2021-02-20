using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tenet.Application.Helpers;
using Tenet.Application.Services;
using Tenet.Domain.Entities;
using Tenet.Infrastructure.Helpers;
using Tenet.Persistence.Contexts;

namespace Tenet.Infrastructure.Services
{
    public class DriverService : IDriverService
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IClient _client;
        private readonly Context _context;
        private readonly ILogger<DriverService> _logger;
        private readonly IDataProtector _protector;

        public DriverService(Context context, IDataProtectionProvider provider, IClient client, IHttpContextAccessor accessor, ILogger<DriverService> logger)
        {
            _context = context;
            _protector = provider.CreateProtector(nameof(DriverInstance));
            _client = client;
            _accessor = accessor;
            _logger = logger;
        }

        public async Task DeleteOldInstances(string key)
        {
            IEnumerable<DriverInstance> olds = await _context.DriverInstances.Where(m => m.Key == key || m.Ip == CurrentIp())
                .ToListAsync();
            _context.DriverInstances.RemoveRange(olds);

            _logger.LogInformation("[DeleteOldInstances] Ip: {Ip} and Key: {Key}", CurrentIp(), key);
            
            await _context.SaveChangesAsync();
        }

        public async Task<string> CreateInstance(int pid, string key)
        {
            if (pid == 0 || string.IsNullOrEmpty(key))
                return null;

            // We delete old instances
            await DeleteOldInstances(key);

            Driver driver = await _context.Drivers.SingleOrDefaultAsync(m => m.Pid == pid);

            if (driver == null)
                return null;

            var entity = await _context.DriverInstances.AddAsync(new DriverInstance()
            {
                DriverId = driver.Id,
                Key = key,
                Expiry = DateTime.UtcNow.AddSeconds(driver.Expiry),
                Ip = CurrentIp()
            });
            await _context.SaveChangesAsync();

            _logger.LogInformation("[CreateInstance] Ip: {Ip} Key: {Key} Driver: {Driver}", CurrentIp(), key, driver.Name);

            
            return ToInstance(entity.Entity.Id, key);
        }

        public async Task<(string encrypted, string iv, string hash)> GetInstance(string key, string ist)
        {
            if (string.IsNullOrEmpty(key))
                return (null, null, null);

            (string oid, string oip, string okey) = FromInstance(ist);
            if (!okey.Equals(key) || !oip.Equals(CurrentIp()))
            {
                _logger.LogCritical("[GetInstance][Tempered] Ip: {Ip} and Key: {Key}", CurrentIp(), key);
                return (null, null, null);
            }

            DriverInstance instance = await _context.DriverInstances
                .Include(m => m.Driver)
                .ThenInclude(m => m.DriverContent)
                .SingleOrDefaultAsync(m => m.Key == key && m.Ip == oip && m.Id == oid);

            if (instance == null)
            {
                _logger.LogCritical("[GetInstance][NotFound] Ip: {Ip} and Key: {Key}", CurrentIp(), key);
                return (null, null, null);
            }

            if (instance.Expiry <= DateTime.UtcNow)
            {
                _context.DriverInstances.Remove(instance);
                await _context.SaveChangesAsync();
                
                _logger.LogWarning("[GetInstance][Expired] Ip: {Ip} and Key: {Key}", CurrentIp(), key);
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

        private string ToInstance(string id, string key)
        {
            string joined = string.Join('|', new string[]{ id, CurrentIp(), key });

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

            string[] values = instance.Split('|');
            return (values[0], values[1], values[2]);
        }

        private string CurrentIp()
        {
            string ip = _accessor.HttpContext.GetRemoteIPAddress(false)
                .ToString();
            return ip;
        }
    }
}
