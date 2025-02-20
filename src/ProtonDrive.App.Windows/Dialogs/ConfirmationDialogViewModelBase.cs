using CommunityToolkit.Mvvm.Input;
using ProtonDrive.App.Windows.Views;

namespace ProtonDrive.App.Windows.Dialogs;

public abstract class ConfirmationDialogViewModelBase : IDialogViewModel
{
    protected ConfirmationDialogViewModelBase(string message)
    {
        Message = message;

        ConfirmAndCloseCommand = new RelayCommand<IClosableDialog>(ConfirmAndClose);
    }

    public string Title { get; protected set; } = Resources.Strings.Main_Window_Title;
    public string Message { get; protected set; }

    public string ConfirmButtonText { get; protected set; } = Resources.Strings.Dialog_Button_Ok;
    public string CancelButtonText { get; protected set; } = Resources.Strings.Dialog_Button_Cancel;

    public RelayCommand<IClosableDialog> ConfirmAndCloseCommand { get; }

    private static void ConfirmAndClose(IClosableDialog? dialog)
    {
        if (dialog is null)
        {
            return;
        }

        dialog.DialogResult = true;
        dialog.Close();
    }
}
