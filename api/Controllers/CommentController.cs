
using api.Dtos.Comment;
using api.Extensions;
using api.Interfaces;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/comment")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _commentRepo;
        private readonly IStockRepository _stockRepo;
        private readonly UserManager<AppUser> _userManager;
        

        public CommentController(ICommentRepository commentRepo, 
            IStockRepository stockRepo, UserManager<AppUser> userManager)
        {
            _commentRepo = commentRepo;
            _stockRepo = stockRepo;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(){
            if(!ModelState.IsValid)
                return BadRequest();

            var comments = await _commentRepo.GetAllAsync();
            var commentDto = comments.Select(comment => comment.ToCommentDto());
            return Ok(commentDto);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id){
            if(!ModelState.IsValid)
                return BadRequest();

            var comment = await _commentRepo.GetByIdAsync(id);

            if(comment == null)
                return NotFound();

            return Ok(comment.ToCommentDto());
        }

        [HttpPost("{stockId:int}")]
        public async Task<IActionResult> Create([FromRoute] int stockId, [FromBody] CreateCommentRequestDto commentDto){
            if(!ModelState.IsValid)
                return BadRequest();

            if(!await _stockRepo.StockExists(stockId))
                return BadRequest("Stock does not exist");

            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);
                
            var commentModel = commentDto.ToCommentFromCreate(stockId);
            commentModel.AppUserId = appUser.Id;
            await _commentRepo.CreateAsync(commentModel);
            return CreatedAtAction(nameof(GetById), new {id = commentModel.Id}, commentModel.ToCommentDto());
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCommentRequestDto updateDto){
            if(!ModelState.IsValid)
                return BadRequest();

            var comment = await _commentRepo.UpdateAsync(id, updateDto.ToCommentFromUpdate());

            if(comment == null)
                return NotFound("Comment not found");

            return Ok(comment.ToCommentDto());
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id){
            if(!ModelState.IsValid)
                return BadRequest();

            var comment = await _commentRepo.DeleteAsync(id);

            if(comment == null)
                return NotFound("Comment not found");

            return NoContent();
        }
    }
}