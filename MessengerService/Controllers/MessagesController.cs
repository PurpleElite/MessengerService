using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessengerService.Models;
using System.Net.Mail;

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

        [HttpGet("{start:datetime?}/{end:datetime?}")]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessages(DateTime? start = null, DateTime? end = null)
        {
            return await GetMessagesHelper(start, end);
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

        [HttpGet("recipients")]
        public async Task<ActionResult<IEnumerable<string>>> GetUsers()
        {
            return await _context.Messages.Select(x => x.RecipientAddress).Distinct().ToListAsync();
        }

        [HttpGet("recipients/{userAddress}/{start:datetime?}/{end:datetime?}")]
        public async Task<ActionResult<IEnumerable<Message>>> GetUserMessages(string userAddress, DateTime? start, DateTime? end)
        {
            return await GetMessagesHelper(start, end, userAddress);
        }

        [HttpGet("recipients/{userAddress}/unread/{start:datetime?}/{end:datetime?}")]
        public async Task<ActionResult<IEnumerable<Message>>> GetUserMessagesUnreadTimeframe(string userAddress, DateTime? start, DateTime? end)
        {
            return await GetMessagesHelper(start, end, userAddress, true);
        }

        [HttpPatch("mark-read")]
        public async Task<ActionResult<IEnumerable<Guid>>> MarkRead(params Guid[] idList)
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

            return await messages.Select(x => x.ID).ToListAsync();
        }

        [HttpPatch("mark-unread")]
        public async Task<ActionResult<IEnumerable<Guid>>> MarkUnread(params Guid[] idList)
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

            return await messages.Select(x => x.ID).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Message>> SendMessage(string content, string recipientAddress, string senderAddress)
        {
            if (!EmailIsValid(recipientAddress) || !EmailIsValid(senderAddress))
            {
                return BadRequest("Invalid email address received.");
            }

            var message = new Message
            {
                ID = Guid.NewGuid(),
                RecipientAddress = recipientAddress,
                SenderAddress = senderAddress,
                Content = content,
                SentTimestamp = DateTime.UtcNow,
            };
            _context.Entry(message).State = EntityState.Added;
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMessage), new { id = message.ID }, message);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<IEnumerable<Guid>>> DeleteMessage(Guid id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            _context.Entry(message).State = EntityState.Deleted;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete]
        public async Task<ActionResult<IEnumerable<Guid>>> DeleteMessages(Guid[] idList)
        {
            var messages = _context.Messages.Where(x => idList.Contains(x.ID));
            if (messages.Count() == 0)
            {
                return NotFound();
            }
            var foundIds = await messages.Select(x => x.ID).ToListAsync();

            foreach (var message in messages)
            {
                _context.Entry(message).State = EntityState.Deleted;
            }
            await _context.SaveChangesAsync();

            return foundIds;
        }

        private async Task<ActionResult<IEnumerable<Message>>> GetMessagesHelper(DateTime? start = null, DateTime? end = null, string? userAddress = null, bool? unread = null, bool markAsRead = false)
        {
            var messages = _context.Messages.Where(x =>
                (userAddress == null || x.RecipientAddress == userAddress)
                && (unread == null || (unread == true && x.ReadTimestamp == null) || (unread == false && x.ReadTimestamp != null))
                && (start == null || x.SentTimestamp >= start)
                && (end == null || x.SentTimestamp <= end))
                .OrderByDescending(x => x.SentTimestamp);

            // Extra code for if we want messages to automatically be marked as read as soon as they're retrieved. Currently markAsRead is always false.
            if (markAsRead)
            {
                foreach (var message in messages)
                {
                    message.ReadTimestamp = DateTime.UtcNow;
                    _context.Entry(message).State = EntityState.Modified;
                }
                await _context.SaveChangesAsync();
            }

            return await messages.ToListAsync();
        }

        private static bool EmailIsValid(string address)
        {
            if (MailAddress.TryCreate(address, out var parsedEmail))
            {
                return address == parsedEmail.Address;
            }
            else
            {
                return false;
            }
        }
    }
}
