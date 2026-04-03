using RevPay.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevPay.Infrastructure.Payments;

public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IEnumerable<IPaymentGateway> _gateways;

    public PaymentGatewayFactory(IEnumerable<IPaymentGateway> gateways)
    {
        _gateways = gateways;
    }

    public IPaymentGateway GetGateway(string providerName)
        => _gateways.FirstOrDefault(g =>
            g.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase))
            ?? throw new NotSupportedException($"Gateway '{providerName}' not supported.");
}
