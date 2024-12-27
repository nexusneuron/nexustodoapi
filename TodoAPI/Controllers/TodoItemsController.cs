using Microsoft.AspNetCore.Mvc;
using TodoAPI.Interfaces;
using TodoAPI.Models;

namespace TodoAPI.Controllers
{
    #region snippetErrorCode
    public enum ErrorCode
    {
        TodoItemNameAndNotesRequired,
        TodoItemIDInUse,
        RecordNotFound,
        CouldNotCreateItem,
        CouldNotUpdateItem,
        CouldNotDeleteItem
    }
    #endregion

    #region snippetDI
    [ApiController]
    [Route("api/[controller]")]
    public class TodoItemsController : ControllerBase
    {
        private readonly ITodoRepository _todoRepository;
        private readonly ApplicationDbContext _context;

        public TodoItemsController(ITodoRepository todoRepository, ApplicationDbContext context)
        {
            _todoRepository = todoRepository;
            _context = context;
        }
        #endregion

        #region snippet
        [HttpGet]
        public IActionResult List()
        {
            //return Ok(_todoRepository.All);
            return Ok(_context.TodoItems.ToList());
        }
        #endregion

        #region snippetCreate
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]TodoItem item)
        {
            try
            {
                if (item == null || !ModelState.IsValid)
                {
                    return BadRequest(ErrorCode.TodoItemNameAndNotesRequired.ToString());
                }
                bool itemExists = _todoRepository.DoesItemExist(item.ID);
                if (itemExists)
                {
                    return StatusCode(StatusCodes.Status409Conflict, ErrorCode.TodoItemIDInUse.ToString());
                }
                //_todoRepository.Insert(item);

                //item.ID = new Guid().ToString();
                _context.TodoItems.Add(item);
                await _context.SaveChangesAsync();
                //_context.SaveChanges();
            }
            catch (Exception)
            {
                return BadRequest(ErrorCode.CouldNotCreateItem.ToString());
            }
            return Ok(item);
        }
        #endregion

        #region snippetEdit
        [HttpPut]
        public IActionResult Edit([FromBody] TodoItem item)
        {
            try
            {
                if (item == null || !ModelState.IsValid)
                {
                    return BadRequest(ErrorCode.TodoItemNameAndNotesRequired.ToString());
                }
                var existingItem = _todoRepository.Find(item.ID);
                if (existingItem == null)
                {
                    return NotFound(ErrorCode.RecordNotFound.ToString());
                }
                _todoRepository.Update(item);
            }
            catch (Exception)
            {
                return BadRequest(ErrorCode.CouldNotUpdateItem.ToString());
            }
            return NoContent();
        }
        #endregion
        
        #region snippetDelete
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            try
            {
                var item = _todoRepository.Find(id);
                if (item == null)
                {
                    return NotFound(ErrorCode.RecordNotFound.ToString());
                }
                _todoRepository.Delete(id);
            }
            catch (Exception)
            {
                return BadRequest(ErrorCode.CouldNotDeleteItem.ToString());
            }
            return NoContent();
        }
        #endregion
    }
}
