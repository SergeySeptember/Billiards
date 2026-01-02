using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Billiards.Core.Entities.DB;

namespace Billiards.Data.Repositories
{
    public interface IPlayerRepository
    {
        Task<Player> AddAsync(string name, CancellationToken ct = default);
        Task<List<Player>> GetAllAsync(CancellationToken ct = default);
    }
}
