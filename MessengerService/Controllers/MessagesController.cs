using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessengerService.Models;

namespace MessengerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly MessengerDbContext _context;

        public MessagesController(MessengerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessages()
        {
            return await _context.Messages.ToListAsync();
        }

        [HttpGet("timeframe")]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessagesTimeframe(DateTime? start, DateTime? end)
        {
            return await _context.Messages.Where(x => x.SentTimestamp >= start && x.SentTimestamp <= end).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Message>> GetMessage(Guid id)
        {
            var message = await _context.Messages.FindAsync(id);

            if (message == null)
            {
                return NotFound();
            }

            return message;
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<string>>> GetUsers()
        {
            return await _context.Messages.Select(x => x.RecipientAddress).Distinct().ToListAsync();
        }

        [HttpGet("users/{userAddress}")]
        public async Task<ActionResult<IEnumerable<Message>>> GetUserMessages(string userAddress)
        {
            return await _context.Messages.Where(x => x.RecipientAddress == userAddress).ToListAsync();
        }

        [HttpGet("users/{userAddress}/timeframe")]
        public async Task<ActionResult<IEnumerable<Message>>> GetUserMessagesTimeframe(string userAddress, DateTime? start, DateTime? end)
        {
            return await _context.Messages.Where(x => x.RecipientAddress == userAddress && x.SentTimestamp >= start && x.SentTimestamp <= end).ToListAsync();
        }

        [HttpGet("users/{userAddress}/unread")]
        public async Task<ActionResult<IEnumerable<Message>>> GetUserMessagesUnread(string userAddress)
        {
            return await _context.Messages.Where(x => x.RecipientAddress == userAddress && x.ReadTimestamp == null).ToListAsync();
        }

        [HttpGet("users/{userAddress}/unread/timeframe")]
        public async Task<ActionResult<IEnumerable<Message>>> GetUserMessagesUnreadTimeframe(string userAddress, DateTime? start, DateTime? end)
        {
            return await _context.Messages.Where(x => x.RecipientAddress == userAddress && x.ReadTimestamp == null && x.SentTimestamp >= start && x.SentTimestamp <= end).ToListAsync();
        }

        [HttpPatch("mark-read")]
        public async Task<IActionResult> MarkRead(params Guid[] idList)
        {
            var messages = _context.Messages.Where(x => idList.Contains(x.ID));

            if (messages.Count() == 0)
            {
                return NotFound();
            }

            foreach (var message in messages)
            {
                message.ReadTimestamp = DateTime.UtcNow;
                _context.Entry(message).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("mark-unread")]
        public async Task<IActionResult> MarkUnread(params Guid[] idList)
        {
            var messages = _context.Messages.Where(x => idList.Contains(x.ID));

            if (messages.Count() == 0)
            {
                return NotFound();
            }

            foreach (var message in messages)
            {
                message.ReadTimestamp = null;
                _context.Entry(message).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            //TODO: Return a report of message IDs not found?
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Message>> SendMessage(string content, string recipientAddress)
        {
            //TODO: Require that recipient address matches a specification (Email address?)
            var message = new Message
            {
                ID = Guid.NewGuid(),
                RecipientAddress = recipientAddress,
                Content = content,
                SentTimestamp = DateTime.UtcNow,
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMessage), new { id = message.ID }, message);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(Guid id)
        {
            return await DeleteMessages([id]);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMessages(Guid[] idList)
        {
            var messages = _context.Messages.Where(x => idList.Contains(x.ID));
            if (messages.Count() == 0)
            {
                return NotFound();
            }

            _context.Messages.RemoveRange(messages);
            await _context.SaveChangesAsync();


            //TODO: Return a report of message IDs not found?
            return NoContent();
        }

        private bool MessageExists(Guid id)
        {
            return _context.Messages.Any(e => e.ID == id);
        }
    }
}
