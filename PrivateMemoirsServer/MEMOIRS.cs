//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PrivateMemoirs
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class MEMOIRS
    {
        [Key]
        public long MEMOIR_ID { get; set; }
        public System.Guid USER_GUID { get; set; }
        public string MEMOIR_TEXT { get; set; }
        public string MEMOIR_TITLE { get; set; }
        public System.DateTime MEMOIR_DATE_RECORD { get; set; }
        public System.DateTime MEMOIR_DATE_CHANGE { get; set; }
    
        public virtual USERS USERS { get; set; }
    }
}