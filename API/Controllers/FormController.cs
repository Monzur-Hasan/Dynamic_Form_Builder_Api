using BLL.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using Shared.Models.Pagination;


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

        [HttpPost, Route("SaveForm")]
        public async Task<IActionResult> SaveFormAsync([FromBody] FormDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Title required");

            bool exists = await _repo.IsTitleExistsAsync(dto.Title);
            if (exists)
                return Conflict("Duplicate title");

            int formId = await _repo.SaveFormAsync(dto.Title, dto.Fields);
            return Ok(new { formId });
        }
             
        [HttpPost,Route("GetFormsPaged")]
        public async Task<IActionResult> GetFormsPagedAsync([FromBody] DataTableRequest req)
        {
            if (req == null)
                return BadRequest("Invalid request");

            var result = await _repo.GetFormsPagedAsync(req);

            return Ok(new
            {
                data = result.Data,                 // rows
                recordsTotal = result.RecordsTotal, // total rows in DB
                recordsFiltered = result.RecordsFiltered // after search
            });
        }


        [HttpPost, Route("UpdateForm")]
        public async Task<IActionResult> UpdateFormAsync([FromBody] FormDto dto)
        {
            bool ok = await _repo.UpdateFormAsync(dto);
            return ok ? Ok() : BadRequest("Update failed");
        }

        [HttpDelete, Route("DeleteForm{id}")]
        public async Task<IActionResult> DeleteFormAsync(int id)
        {
            bool ok = await _repo.DeleteFormAsync(id);
            return ok ? Ok() : BadRequest("Delete failed");
        }
    }

}
