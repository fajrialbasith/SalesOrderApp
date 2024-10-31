using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesOrderApp.Models
{
    [Table("SO_ITEM")]
    public class Item
    {
        [Key]
        public int SO_ITEM_ID { get; set; }
        public int SO_ORDER_ID { get; set; }
        public string ITEM_NAME { get; set; }
        public int QUANTITY { get; set; }
        public float Price { get; set; }
    }
}
