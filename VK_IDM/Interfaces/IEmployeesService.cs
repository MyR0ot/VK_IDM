using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VK_IDM.Models;

namespace VK_IDM.Interfaces
{
    public interface IEmployeesService
    {
        void ResolveFinalData(string dbFilePath, string updFilePath, string finalDataFilePath);
    }
}
