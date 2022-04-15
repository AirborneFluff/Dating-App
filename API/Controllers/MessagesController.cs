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
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public MessagesController(IMapper mapper, IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();
            if (username.ToLower() == createMessageDto.RecipientUsername.ToLower())
                return BadRequest("You cannot message yourself");

            var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) return NotFound();

            if (string.IsNullOrWhiteSpace(createMessageDto.Content)) return BadRequest("No message content sent");

            var newMessage = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            _unitOfWork.MessageRepository.AddMessage(newMessage);
            if (await _unitOfWork.Complete()) return Ok(_mapper.Map<MessageDto>(newMessage));

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();
            var messages = await _unitOfWork.MessageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

            return messages;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var userId = User.GetUserId();

            var message = await _unitOfWork.MessageRepository.GetMessage(id);

            if (message == null) return NotFound("Message not found");
            if (message.Sender.Id != userId && message.Recipient.Id != userId)
                return Unauthorized();

            if (message.Sender.Id == userId) message.SenderDeleted = true;
            if (message.Recipient.Id == userId) message.RecipientDeleted = true;
            if (message.RecipientDeleted && message.SenderDeleted) _unitOfWork.MessageRepository.DeleteMessage(message);

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Problem deleting the message");
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpDelete("all/{username}")]
        public async Task<ActionResult> DeleteUserMessages(string username)
        {
            var user = await _unitOfWork.UserRepository.GetMemberByUsernameAsync(username, true);
            if (user == null) return BadRequest("No user found");
            var messages = await _unitOfWork.MessageRepository.GetAllMessagesFromUser(username);
            if (messages == null) return NotFound("No messages have been sent from this user");

            _unitOfWork.MessageRepository.DeleteMessages(messages.ToArray());

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Error deleting user's messages");
        }
    }
}