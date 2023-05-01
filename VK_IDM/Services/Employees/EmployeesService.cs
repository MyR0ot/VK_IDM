using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VK_IDM.Interfaces;
using VK_IDM.Models;

namespace VK_IDM.Services.Employees
{
    public class EmployeesService : IEmployeesService
    {
        private readonly IMapper _mapper;
        private readonly IDataLoaderService _dataLoader;

        public EmployeesService(
            IMapper mapper,
            IDataLoaderService dataLoader
            )
        {
            _mapper = mapper;
            _dataLoader = dataLoader;
        }

        public void ResolveFinalData(string dbFilePath, string updFilePath, string finalDataFilePath)
        {
            var dbEmployees = _dataLoader.ParseStatesFromJsonFile<IDM_EmployeeItem>(dbFilePath);
            var updEmployees = _dataLoader.ParseStatesFromJsonFile<EmployeeItem>(updFilePath);
            var finalEmployees = GetFinalEmployeesData(dbEmployees, updEmployees);

            _dataLoader.SerializeToJsonFile<IDM_EmployeeItem>(finalEmployees, finalDataFilePath);
        }

        private List<IDM_EmployeeItem> GetFinalEmployeesData(List<IDM_EmployeeItem> dbEmployees, List<EmployeeItem> updEmployees)
        {
            var dbData = new List<IDM_EmployeeItem>(dbEmployees)
                .GroupBy(s => s.GetHashCode())
                .ToDictionary(
                    g => g.Key,
                    g => g.First()
                );

            var updData = new List<IDM_EmployeeItem>(updEmployees.Select(e => _mapper.Map<IDM_EmployeeItem>(e)))
                .GroupBy(s => s.GetHashCode())
                .ToDictionary(
                    g => g.Key,
                    g => g.First()
                );


            foreach (var updKey in updData.Keys)
            {
                if (dbData.ContainsKey(updKey)) // в базе уже есть запись с заданными (personId, date_hired)
                {
                    dbData[updKey] = updData[updKey];
                }
                else // в базе нет сотрудника c заданным date_hired
                {
                    var newEntity = updData[updKey];
                    if (newEntity.DateFired.HasValue) // уволенных не создаем 
                        continue;

                    dbData.Add(updKey, newEntity);
                }
            }

            return dbData
                .Select(s => s.Value)
                .OrderBy(o => o.PersonId)
                .ThenBy(o => o.DateHired)
                .ToList();
        }


    }
}
