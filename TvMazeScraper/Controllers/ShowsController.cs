using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TvMazeScraper.Service;

namespace TvMazeScraper.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShowsController : Controller
    {
        private IShowsService ShowsService { get; }

        public ShowsController(IShowsService showsService) => ShowsService = showsService;
        
        [HttpGet()]
        public async Task<IActionResult> Get([FromQuery(Name = "page")] int page)
        {
            if (page < 0) return new BadRequestObjectResult(new { Error = "Wrong page" });
            try
            {
                return Json(await ShowsService.Get(page));
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { Error = e.Message });
            }
        }
    }
}
