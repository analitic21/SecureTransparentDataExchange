using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.Enums;

namespace SecureTransparentDataExchange.Services.StateMachines
{
    /// <summary>
    /// Controls allowed ShipmentStatus transitions
    /// </summary>
    public sealed class ShipmentStateMachine
    {
        private static readonly Dictionary<ShipmentStatus, HashSet<ShipmentStatus>> Transitions =
            new()
            {
                [ShipmentStatus.Created] = new()
                {
                    ShipmentStatus.PaymentCompleted,
                    ShipmentStatus.Canceled
                },

                [ShipmentStatus.PaymentCompleted] = new()
                {
                    ShipmentStatus.Processing
                },

                [ShipmentStatus.Processing] = new()
                {
                    ShipmentStatus.ReadyForPickup,
                    ShipmentStatus.Canceled
                },

                [ShipmentStatus.ReadyForPickup] = new()
                {
                    ShipmentStatus.InTransit
                },

                [ShipmentStatus.InTransit] = new()
                {
                    ShipmentStatus.Delivered,
                    ShipmentStatus.Returned
                }
            };

        public bool CanMoveTo(ShipmentStatus from, ShipmentStatus to)
        {
            return Transitions.TryGetValue(from, out var allowed)
                   && allowed.Contains(to);
        }
    }
}
