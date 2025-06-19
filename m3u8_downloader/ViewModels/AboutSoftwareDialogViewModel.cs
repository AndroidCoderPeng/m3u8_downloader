using System;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace m3u8_downloader.ViewModels
{
    public class AboutSoftwareDialogViewModel : BindableBase, IDialogAware
    {
        public string Title => "关于";

        public event Action<IDialogResult> RequestClose
        {
            add { }
            remove { }
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }
    }
}