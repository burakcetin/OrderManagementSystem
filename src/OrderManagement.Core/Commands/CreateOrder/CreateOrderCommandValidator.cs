using FluentValidation;

namespace OrderManagement.Core.Commands.CreateOrder
{
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required");
                
            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            RuleFor(x => x.CustomerName)
                .NotEmpty().WithMessage("Customer name is required")
                .MaximumLength(100).WithMessage("Customer name cannot exceed 100 characters");

            RuleFor(x => x.CustomerEmail)
                .NotEmpty().WithMessage("Customer email is required")
                .EmailAddress().WithMessage("A valid email address is required")
                .MaximumLength(100).WithMessage("Customer email cannot exceed 100 characters");

            RuleFor(x => x.ShippingAddress)
                .NotEmpty().WithMessage("Shipping address is required")
                .MaximumLength(255).WithMessage("Shipping address cannot exceed 255 characters");
        }
    }
}
