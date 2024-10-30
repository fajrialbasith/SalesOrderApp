using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesOrderApp.Models
{
    [Table("COM_CUSTOMER")]
    public class Customer
    {
        [Key]
        public int COM_CUSTOMER_ID { get; set; }
        public string CUSTOMER_NAME { get; set; }
    }
}
