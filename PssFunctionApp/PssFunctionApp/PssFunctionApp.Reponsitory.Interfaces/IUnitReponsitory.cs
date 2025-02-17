using PssFunctionApp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PssFunctionApp.Reponsitory.Interfaces
{
    public interface IUnitReponsitory
    {
        Task<Unit> getUnitById(string unitId);
    }
}
