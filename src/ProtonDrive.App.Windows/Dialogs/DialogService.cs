using ProtonDrive.App.Windows.Services;
using ProtonDrive.App.Windows.Views;

namespace ProtonDrive.App.Windows.Dialogs;

internal sealed class DialogService : IDialogService
{
    private readonly App _app;

    public DialogService(App app)
    {
        _app = app;
    }

    public ConfirmationResult ShowConfirmationDialog(ConfirmationDialogViewModelBase dataContext)
    {
        var confirmationDialog = new ConfirmationDialogWindow
        {
            DataContext = dataContext,
            Owner = _app.MainWindow,
        };

        var result = confirmationDialog.ShowDialog();

        if (result is null)
        {
            return ConfirmationResult.Cancelled;
        }

        return result.GetValueOrDefault() ? ConfirmationResult.Confirmed : ConfirmationResult.Cancelled;
    }

    public void Show(IDialogViewModel dataContext)
    {
        var dialog = new DialogWindow
        {
            DataContext = dataContext,
            Owner = _app.MainWindow,
        };

        dialog.Show();
    }

    public void ShowDialog(IDialogViewModel dataContext)
    {
        var dialog = new DialogWindow
        {
            DataContext = dataContext,
            Owner = _app.MainWindow,
        };

        dialog.ShowDialog();
    }
}
