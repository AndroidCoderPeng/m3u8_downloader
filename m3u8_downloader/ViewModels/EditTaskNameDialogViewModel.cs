using System;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace m3u8_downloader.ViewModels
{
    public class EditTaskNameDialogViewModel : BindableBase, IDialogAware
    {
        public string Title => "修改下载任务标题";

        public event Action<IDialogResult> RequestClose;

        private string _taskName = string.Empty;

        public string TaskName
        {
            set
            {
                _taskName = value;
                RaisePropertyChanged();
            }
            get => _taskName;
        }

        public DelegateCommand DialogUpdateCommand { set; get; }
        public DelegateCommand DialogCancelCommand { set; get; }

        public EditTaskNameDialogViewModel()
        {
            DialogUpdateCommand = new DelegateCommand(delegate
            {
                var dialogParameters = new DialogParameters
                {
                    { "TaskName", _taskName }
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
            TaskName = parameters.GetValue<string>("TaskName");
        }
    }
}