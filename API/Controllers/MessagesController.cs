using System.Reflection.Metadata.Ecma335;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;
        public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
        {
            this._mapper = mapper;
            this._messageRepository = messageRepository;
            this._userRepository = userRepository;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();
            if (username.ToLower() == createMessageDto.RecipientUsername.ToLower())
                return BadRequest("You cannot message yourself");

            var sender = await _userRepository.GetUserByUsernameAsync(username);
            var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) return NotFound();

            if(string.IsNullOrWhiteSpace(createMessageDto.Content)) return BadRequest("No message content sent");

            var newMessage = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            _messageRepository.AddMessage(newMessage);
            if (await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(newMessage));

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();
            var messages = await _messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUsername = User.GetUsername();
            return Ok(await _messageRepository.GetMessageThread(currentUsername, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id) {
            var userId = User.GetUserId();

            var message = await _messageRepository.GetMessage(id);

            if (message == null) return NotFound("Message not found");
            if (message.Sender.Id != userId && message.Recipient.Id != userId)
                return Unauthorized();

            if (message.Sender.Id == userId) message.SenderDeleted = true;
            if (message.Recipient.Id == userId) message.RecipientDeleted = true;
            if (message.RecipientDeleted && message.SenderDeleted) _messageRepository.DeleteMessage(message);

            if (await _messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting the message");
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpDelete("all/{username}")]
        public async Task<ActionResult> DeleteUserMessages(string username)
        {
            var user = await _userRepository.GetMemberByUsernameAsync(username);
            if (user == null) return BadRequest("No user found");
            var messages = await _messageRepository.GetAllMessagesFromUser(username);
            if (messages == null) return NotFound("No messages have been sent from this user");

            _messageRepository.DeleteMessages(messages.ToArray());

            if (await _messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Error deleting user's messages");
        }
    }
}