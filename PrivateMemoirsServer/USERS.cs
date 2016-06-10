namespace PrivateMemoirs
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class USERS
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public USERS()
        {
            MEMOIRS = new HashSet<MEMOIRS>();
        }

        [Key]
        public Guid USER_GUID { get; set; }

        [Required]
        [StringLength(50)]
        public string USER_LOGIN { get; set; }

        [Required]
        [StringLength(64)]
        public string USER_HASH { get; set; }

        public DateTime REGISTRATION_DATE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MEMOIRS> MEMOIRS { get; set; }
    }
}