using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Payments.Domain.Payers;

namespace SilverbridgeWeb.Modules.Payments.Application.Payers.CreatePayer;

internal sealed class CreatePayerCommandHandler(IPayerRepository payerRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreatePayerCommand>
{
    public async Task<Result> Handle(CreatePayerCommand request, CancellationToken cancellationToken)
    {
        if (await payerRepository.GetAsync(request.UserId, cancellationToken) is not null)
        {
            return Result.Success();
        }

        payerRepository.Insert(Payer.Create(request.UserId, request.Email, request.FirstName, request.LastName));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
