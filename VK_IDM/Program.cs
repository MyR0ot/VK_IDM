using AutoMapper;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VK_IDM.Interfaces;
using VK_IDM.Services;
using VK_IDM.Services.Employees;

namespace VK_IDM
{
    class Program
    {

        static void Main(string[] args)
        {
            // TODO: настроить DI
            IMapper mapper = new MapperConfiguration (cfg => { cfg.AddProfile(new EmployeesMapperProfile()); }).CreateMapper();
            IEmployeesService employeesService = new EmployeesService(mapper, new DataLoaderService());

            employeesService.ResolveFinalData(
                $"{Directory.GetCurrentDirectory()}\\Resources\\StartData.json",
                $"{Directory.GetCurrentDirectory()}\\Resources\\Employees.json",
                $"{Directory.GetCurrentDirectory()}\\Resources\\FinalData.json"
                );
        }
    }
}
