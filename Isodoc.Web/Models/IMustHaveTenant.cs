namespace Isodoc.Web.Models;

public interface IMustHaveTenant
{
    Guid ClientId { get; set; }
}
