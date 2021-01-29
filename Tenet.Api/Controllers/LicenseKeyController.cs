using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AspNetCore.Proxy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tenet.Application.Services;
using Tenet.Domain.Configuration;
using Tenet.Domain.Entities;

namespace Tenet.Api.Controllers
{
    [Route("/api")]
    [ApiController]
    public class LicenseKeyController : Controller
    {
        private readonly IDriverService _drivers;
        private readonly EndpointConfiguration _endpoints;
        private readonly ILicenseKeyService _licenses;

        public LicenseKeyController(ILicenseKeyService licenses, IDriverService drivers, IOptions<EndpointConfiguration> endpoints)
        {
            _licenses = licenses;
            _drivers = drivers;
            _endpoints = endpoints.Value;

            if (string.IsNullOrEmpty(_endpoints.Auth))
                throw new System.Exception(nameof(_endpoints.Auth));
        }

        [HttpGet("stream")]
        public async Task<IEnumerable<DriverInstance>> Instances()
            => await _drivers.GetInstances();

        [HttpPost("auth")]
        public Task Authenticate([Required] string token)
            => this.HttpProxyAsync($"{_endpoints.Auth}/api/auth?token={token}", _licenses.Process());

        [HttpPost("stream")]
        public async Task<string> Stream()
        {
            string ist = HttpContext.Request.Headers["TN-IST"];
            string key = HttpContext.Request.Headers["TN-KEY"];

            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? HttpContext.Request.Headers["CF-Connecting-IP"].ToString();

            (string encrypted, string iv, string hash) = await _drivers.GetInstance(key, ip, ist);

            Response.Headers["i"] = iv;
            Response.Headers["h"] = hash;

            return encrypted;
        }
    }
}
