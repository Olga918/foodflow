using FoodFlow.Enums;
using System.ComponentModel.DataAnnotations;

namespace FoodFlow.ViewModels
{
    public class CheckoutViewModel
    {
        [Required]
        [Display(Name = "Order type")]
        public OrderType OrderType { get; set; } = OrderType.Pickup;

        [Required]
        [Phone]
        [RegularExpression(@"^\+?[0-9\s\-\(\)]{10,20}$", ErrorMessage = "Enter a valid phone number.")]
        [Display(Name = "Contact phone")]
        public string ContactPhone { get; set; } = string.Empty;

        [Display(Name = "Delivery address")]
        public string? DeliveryAddress { get; set; }

        [Required]
        [Display(Name = "Payment method")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        [Display(Name = "I checked order details and agree to proceed with payment.")]
        public bool ConfirmChecklist { get; set; }
    }
}
