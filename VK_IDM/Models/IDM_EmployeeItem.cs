using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_IDM.Models
{
    [JsonConverter(typeof(CaseInsensitiveConverter<IDM_EmployeeItem>))]
    public class IDM_EmployeeItem
    {
        /// <summary>
        /// Уникальный логин сотрудника
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Уникальный идентификатор сотрудника
        /// </summary>
        [JsonProperty("personId")]
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

        /// <summary>
        /// статус блокировки доступов
        /// </summary>
        public bool Disabled { get; set; }


        public override int GetHashCode() => new
        {
            PersonId,
            DateHired
        }.GetHashCode();
    }
}
