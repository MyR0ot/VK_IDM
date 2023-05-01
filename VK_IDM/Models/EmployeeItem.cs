using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_IDM.Models
{
    [JsonConverter(typeof(CaseInsensitiveConverter<EmployeeItem>))]
    public class EmployeeItem
    {
        /// <summary>
        /// Идентификатор записи в базе данных
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Уникальный логин сотрудника
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Уникальный идентификатор сотрудника
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Должность сотрудника
        /// </summary>
        public string Specialization { get; set; }

        /// <summary>
        /// Дата найма
        /// </summary>
        public DateTime DateHired { get; set; }


        /// <summary>
        /// Дата увольнения
        /// </summary>
        public DateTime? DateFired { get; set; }
    }
}
