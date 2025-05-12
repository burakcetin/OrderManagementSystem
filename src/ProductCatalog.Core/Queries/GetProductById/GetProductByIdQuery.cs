using MediatR;
using ProductCatalog.Core.Models;

namespace ProductCatalog.Core.Queries.GetProductById
{
    public class GetProductByIdQuery : IRequest<ProductDto>
    {
        public Guid ProductId { get; set; }
    }
}
