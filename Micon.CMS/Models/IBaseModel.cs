using System;

namespace Micon.CMS.Models
{
    public interface IBaseModel
    {
        Guid Id { get; set; }
        Guid TenantId { get; set; }
        DateTimeOffset Modified { get; set; }
        DateTimeOffset Created { get; set; }
    }
}
