using MotorcycleRental.Domain.Enums;

namespace MotorcycleRental.Application.DTOs.Driver
{
    public record CreateDriverRequest(
        string Identifier,
        string Name,
        string CNPJ,
        DateTime BirthDate,
        string CNHNumber,
        CNHType CNHType
    );
}