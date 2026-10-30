using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Payments.Domain.Payers;

namespace SilverbridgeWeb.Modules.Payments.Application.Payers.UpdatePayer;

internal sealed class UpdatePayerCommandHandler(IPayerRepository payerRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdatePayerCommand>
{
    public async Task<Result> Handle(UpdatePayerCommand request, CancellationToken cancellationToken)
    {
        Payer? payer = await payerRepository.GetAsync(request.UserId, cancellationToken);

        // Users registered before Payments existed are unknown here, so there is nothing to update.
        if (payer is null)
        {
            return Result.Success();
        }

        payer.UpdateName(request.FirstName, request.LastName);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
