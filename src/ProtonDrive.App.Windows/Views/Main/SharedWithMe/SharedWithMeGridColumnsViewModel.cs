using ProtonDrive.App.Windows.Views.Shared.Sorting;

namespace ProtonDrive.App.Windows.Views.Main.SharedWithMe;

internal sealed class SharedWithMeGridColumnsViewModel
{
    private const string PermissionsColumnTooltip = "Permissions granted to you by the owner";

    public GridColumnViewModel PermissionsColumn { get; } = new(
        name: nameof(SharedWithMeItemViewModel.IsReadOnly),
        displayName: "Permissions",
        tooltip: PermissionsColumnTooltip);

    public GridColumnViewModel NameColumn { get; } = new(name: nameof(SharedWithMeItemViewModel.Name), displayName: "Name");
    public GridColumnViewModel SharedByColumn { get; } = new(name: nameof(SharedWithMeItemViewModel.InviterDisplayName), displayName: "Shared by");
    public GridColumnViewModel SharedOnColumn { get; } = new(name: nameof(SharedWithMeItemViewModel.SharingLocalDateTime), displayName: "Shared on");
    public GridColumnViewModel EnableSyncColumn { get; } = new(name: nameof(SharedWithMeItemViewModel.IsSyncEnabled), displayName: "Sync");

    public void ClearSorting()
    {
        PermissionsColumn.ClearSorting();
        NameColumn.ClearSorting();
        SharedByColumn.ClearSorting();
        SharedOnColumn.ClearSorting();
        EnableSyncColumn.ClearSorting();
    }
}
