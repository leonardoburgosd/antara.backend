using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AntaraSoft.Api.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : Controller
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardController(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardDto>> ObtenerDashboardAsync()
        {
            try
            {
                return Ok(await _dashboardRepository.ObtenerDashboard());
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo obtener la información del dashboard." });
            }
        }
    }
}

