using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.ValueObjects
{
    public record CustomerId
    {
        public Guid Value { get; init; }

        private CustomerId() { }
        private CustomerId(Guid value) => Value = value;

        public static CustomerId Create() => new(Guid.NewGuid());
        public static CustomerId Create(Guid value) => new(value);
        public override string ToString() => Value.ToString();
    }

}
