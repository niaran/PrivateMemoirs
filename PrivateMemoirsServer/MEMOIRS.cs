namespace PrivateMemoirs
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MEMOIRS
    {
        [Key]
        public long MEMOIR_ID { get; set; }

        public Guid USER_GUID { get; set; }

        [Column(TypeName = "text")]
        [Required]
        public string MEMOIR_TEXT { get; set; }

        [Column(TypeName = "text")]
        [Required]
        public string MEMOIR_TITLE { get; set; }

        public DateTime MEMOIR_DATE_RECORD { get; set; }

        public DateTime MEMOIR_DATE_CHANGE { get; set; }

        public virtual USERS USERS { get; set; }
    }
}
