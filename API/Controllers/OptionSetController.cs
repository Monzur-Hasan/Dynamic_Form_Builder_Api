using BLL.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Dynamic_Form_Builder_Api.Controllers
{    
    [ApiController, Route("api/[controller]")]
    public class OptionSetController : ControllerBase
    {
        private readonly IOptionRepository _repo;

        public OptionSetController(IOptionRepository repo)
        {
            _repo = repo;
        }

        [HttpGet, Route("GetOptionSets")]
        public async Task<IActionResult> GetOptionSetsAsync()
        {
            return Ok(await _repo.GetOptionSetsAsync());
        }

        [HttpGet, Route("GetValues{id}")]
        public async Task<IActionResult> GetValuesAsync(int id)
        {
            return Ok(await _repo.GetOptionValuesAsync(id));
        }
    }
}
