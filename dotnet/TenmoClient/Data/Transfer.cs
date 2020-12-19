using System;
using System.Collections.Generic;
using System.Text;

namespace TenmoClient.Data
{
    public class Transfer
    {
        public int TransferId { get; set; }
        public int TransferTypeId { get; set; }
        public int TransferStatusId { get; set; }
        public int AccountFrom { get; set; }
        public int AccountTo { get; set; }
        public decimal Amount { get; set; }
        public string AccountFromName { get; set; }
        public string AccountToName { get; set; }
        public int UserId { get; set; }
        public string TransferStatusName { get; set; }

        public Transfer()
        {

        }

    }
    
}
