using BLL.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using Shared.Models.Pagination;

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

        [HttpPost("GetPagedOptionSets")]
        public async Task<IActionResult> GetPagedOptionSetsAsync([FromBody] DataTableRequest req)
        {
            var result = await _repo.GetPagedOptionSetsAsync(req);

            return Ok(new
            {
                data = result.Data,
                recordsTotal = result.TotalCount,
                recordsFiltered = result.FilteredCount
            });
        }


        [HttpGet, Route("GetOptionSet{id}")]
        public async Task<IActionResult> GetOptionSetAsync(int id)
        {
            var data = await _repo.GetOptionSetAsync(id);
            if (data == null) return NotFound();

            return Ok(data);
        }

        [HttpPost("CreateOptionSet")]
        public async Task<IActionResult> CreateOptionSetAsync([FromBody] OptionSetDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest("Name is required.");

            try
            {
                await _repo.CreateOptionSetAsync(model.Name);
                return Ok(new { message = "Saved successfully" });
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        
        [HttpPut("UpdateOptionSet{id}")]
        public async Task<IActionResult> UpdateOptionSetAsync(int id, [FromBody] OptionSetDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest("Name is required.");

            try
            {
                bool update = await _repo.UpdateOptionSetAsync(id, model.Name);
                if (!update) return BadRequest("Update failed.");

                return Ok(new { message = "Updated successfully" });
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpDelete("DeleteOptionSet{id}")]
        public async Task<IActionResult> DeleteOptionSetAsync(int id)
        {
            try
            {
                bool deleted = await _repo.DeleteOptionSetAsync(id);
                if (!deleted) return BadRequest("Delete failed.");

                return Ok(new { message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }


        [HttpPost("AddOptionValue{setId}")]
        public async Task<IActionResult> AddOptionValueAsync(int setId, [FromBody] OptionDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Value))
                return BadRequest("Value is required.");

            try
            {
                bool added = await _repo.AddOptionValueAsync(setId, model.Value);
                if (!added) return BadRequest("Insert failed.");

                return Ok(new { message = "Value added successfully" });
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPost("UpdateValue{valueId}")]
        public async Task<IActionResult> UpdateValueAsync(int valueId, [FromBody] OptionDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Value))
                return BadRequest("Value is required.");

            try
            {
                bool updated = await _repo.UpdateOptionValueAsync(valueId, model.Value, model.OptionId);
                if (!updated) return BadRequest("Update failed.");

                return Ok(new { message = "Updated successfully" });
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpDelete("DeleteValue{valueId}")]
        public async Task<IActionResult> DeleteValueAsync(int valueId)
        {
            try
            {
                bool deleted = await _repo.DeleteOptionValueAsync(valueId);
                if (!deleted) return BadRequest("Delete failed.");

                return Ok(new { message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}

