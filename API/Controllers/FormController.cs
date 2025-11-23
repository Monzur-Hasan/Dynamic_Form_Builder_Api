using BLL.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;


namespace Dynamic_Form_Builder_Api.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class FormController : ControllerBase
    {
        private readonly IFormRepository _repo;

        public FormController(IFormRepository repo)
        {
            _repo = repo;
        }

    
        [HttpGet, Route("GetForm{id}")]
        public async Task<IActionResult> GetFormAsync(int id)
        {
            var form = await _repo.GetFormWithFieldsAsync(id);
            if (form == null) return NotFound();

            return Ok(form);
        }

        [HttpPost, Route("Save")]
        public async Task<IActionResult> SaveAsync([FromBody] FormDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Title required");

            bool exists = await _repo.IsTitleExistsAsync(dto.Title);
            if (exists)
                return Conflict("Duplicate title");

            int formId = await _repo.SaveFormAsync(dto.Title, dto.Fields);
            return Ok(new { formId });
        }

        [HttpPost, Route("Update")]
        public async Task<IActionResult> UpdateAsync([FromBody] FormDto dto)
        {
            bool ok = await _repo.UpdateFormAsync(dto);
            return ok ? Ok() : BadRequest("Update failed");
        }

        [HttpDelete, Route("Delete{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            bool ok = await _repo.DeleteFormAsync(id);
            return ok ? Ok() : BadRequest("Delete failed");
        }
    }

}
