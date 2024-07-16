using BookGuestAjax.Models;

namespace BookGuestAjax.Services
{
    public interface IRepository
    {
        Task<User> GetUserByIdAsync(int userId);
        Task<User> GetUserByNameAndPasswordAsync(string name, string password);
        Task<User> GetUserByNameAsync(string name);
        Task AddUserAsync(User user);
        Task SaveChangesAsync();

        Task<List<Message>> GetAllMessagesAsync();
        Task<List<Message>> GetMessagesByUserIdAsync(int userId);
        Task AddMessageAsync(Message message);
    }
}