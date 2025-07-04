using System.Collections.Generic;

namespace MovieManagement.Models;

public class PaymentViewModel
{
    public CartItem CartItem { get; set; } = null!;
    public List<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
}

public class PaymentMethod
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
} 