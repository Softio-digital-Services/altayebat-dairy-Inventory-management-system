using System;

namespace InventorySystem.Helpers
{
    public static class GlobalEvents
    {
        // Define events for data changes
        public static event Action OnInventoryUpdated;
        public static event Action OnCustomersUpdated;
        public static event Action OnSuppliersUpdated;
        public static event Action OnOrdersUpdated;

        // Methods to safe-trigger events
        public static void RaiseInventoryUpdated() => OnInventoryUpdated?.Invoke();
        public static void RaiseCustomersUpdated() => OnCustomersUpdated?.Invoke();
        public static void RaiseSuppliersUpdated() => OnSuppliersUpdated?.Invoke();
        public static void RaiseOrdersUpdated() => OnOrdersUpdated?.Invoke();
    }
}
