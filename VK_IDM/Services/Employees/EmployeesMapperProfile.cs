using AutoMapper;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_IDM.Services.Employees
{
    public class EmployeesMapperProfile : Profile
    {
        public EmployeesMapperProfile()
        {
            CreateMap<Models.EmployeeItem, Models.IDM_EmployeeItem>()
                .ForMember(m => m.Disabled, opt => opt.MapFrom(f => f.DateFired.HasValue));
        }
    }

}
