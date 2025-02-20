using System;
using ProtonDrive.App.Windows.Dialogs;

namespace ProtonDrive.App.Windows.Views.Main.Computers;

internal sealed class RemoveSyncFolderConfirmationViewModel : ConfirmationDialogViewModelBase
{
    private static readonly string ContentText =
        Resources.Strings.Main_MyComputer_Folders_RemoveConfirmation_Title + Environment.NewLine +
        Resources.Strings.Main_MyComputer_Folders_RemoveConfirmation_Message_1 + Environment.NewLine +
        Resources.Strings.Main_MyComputer_Folders_RemoveConfirmation_Message_2;

    public RemoveSyncFolderConfirmationViewModel()
        : base(message: ContentText)
    {
        ConfirmButtonText = Resources.Strings.Main_MyComputer_Folders_RemoveConfirmation_Button_Confirm;
    }

    public void SetFolderName(string folderName)
    {
        Message = string.Format(ContentText, folderName);
    }
}
