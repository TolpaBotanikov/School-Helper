//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SchoolHelperDb
{
    using System;
    using System.Collections.Generic;
    
    public partial class HelperProblem
    {
        public int Id { get; set; }
        public int HelperId { get; set; }
        public int ProblemId { get; set; }
    
        public virtual Problem Problem { get; set; }
        public virtual User User { get; set; }
    }
}
