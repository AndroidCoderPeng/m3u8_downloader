using System;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace m3u8_downloader.ViewModels
{
    public class EditFileNameDialogViewModel : BindableBase, IDialogAware
    {
        public string Title => "修改下载文件标题";

        public event Action<IDialogResult> RequestClose;

        private string _fileName = string.Empty;

        public string FileName
        {
            set
            {
                _fileName = value;
                RaisePropertyChanged();
            }
            get => _fileName;
        }

        public DelegateCommand DialogUpdateCommand { set; get; }
        public DelegateCommand DialogCancelCommand { set; get; }

        public EditFileNameDialogViewModel()
        {
            DialogUpdateCommand = new DelegateCommand(delegate
            {
                var dialogParameters = new DialogParameters
                {
                    { "FileName", _fileName }
                };
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK, dialogParameters));
            });

            DialogCancelCommand = new DelegateCommand(delegate
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
            });
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
            FileName = parameters.GetValue<string>("FileName");
        }
    }
}