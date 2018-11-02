using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TvMazeScraper.Service;
using TvMazeScraper.Service.Model;

namespace TvMazeScraper.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShowsController : Controller
    {
        private IShowsService ShowsService { get; }

        public ShowsController(IShowsService showsService) => ShowsService = showsService;
        
        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Json(await ShowsService.Get());
        }
    }
}
