namespace Chaos.Client.Definitions;

/// <summary>
///     A panel that can receive an inventory item dragged out of the inventory HUD while it is open. Implemented by
///     popups/panels (Exchange, Market, equipment) and registered with <c>WorldScreen</c>, which routes an inventory
///     drop to the first target whose <see cref="AcceptsInventoryDrop" /> returns true and runs the networking action
///     paired with it at registration (so all <c>Game.Connection.*</c> calls stay in <c>WorldScreen</c>).
/// </summary>
public interface IInventoryDropTarget
{
    /// <summary>
    ///     True if this target is currently eligible AND the screen point falls within its drop zone. Implementations
    ///     own their own visibility/geometry/eligibility rules (e.g. reject the gold bag, require a specific tab).
    /// </summary>
    bool AcceptsInventoryDrop(byte slot, int screenX, int screenY);
}
