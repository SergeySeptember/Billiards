using Billiards.DataBase.Entities;

namespace Billiards.Abstractions;

public interface IPlayerRepository
{
    Task<Player> AddAsync(string name);
    Task<List<Player>> GetAllAsync();
    Task DeleteAllAsync();
    Task<bool> DeletePlayerAsync(string name);
}