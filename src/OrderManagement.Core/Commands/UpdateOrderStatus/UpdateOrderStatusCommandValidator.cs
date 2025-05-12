using FluentValidation;
using OrderManagement.Core.Entities;

namespace OrderManagement.Core.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
    {
        public UpdateOrderStatusCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("Order ID is required");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Status value is invalid");
        }
    }
}
