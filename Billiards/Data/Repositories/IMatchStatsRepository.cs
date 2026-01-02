using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Billiards.Core.Entities.DB;

namespace Billiards.Data.Repositories;

public interface IMatchStatsRepository
{
    Task AddAsync(MatchStats match, CancellationToken ct = default);
}