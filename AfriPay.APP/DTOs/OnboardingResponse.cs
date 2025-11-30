using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.APP.DTOs
{
    public class OnboardingResponse
    {
        public Guid OnboardingId { get; set; }
        public string RequestReference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? CustomerReference { get; set; }
        public string? AccountNumber { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
