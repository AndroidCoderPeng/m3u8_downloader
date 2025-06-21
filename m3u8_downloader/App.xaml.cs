using System.Windows;
using m3u8_downloader.Dialogs;
using m3u8_downloader.Pages;
using m3u8_downloader.Service;
using m3u8_downloader.ViewModels;
using m3u8_downloader.Views;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Regions;

namespace m3u8_downloader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            var mainWindow = Container.Resolve<MainWindow>();
            mainWindow.Loaded += delegate
            {
                var regionManager = Container.Resolve<IRegionManager>();
                regionManager.RequestNavigate("ContentRegion", "DownloadTaskPage");
            };
            return mainWindow;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //Data
            containerRegistry.Register<IAppDataService, AppDataServiceImpl>();
            
            //Navigation
            containerRegistry.RegisterForNavigation<DownloadTaskPage, DownloadTaskPageViewModel>();
            containerRegistry.RegisterForNavigation<FinishedTaskPage, FinishedTaskPageViewModel>();
            containerRegistry.RegisterForNavigation<AboutSoftwarePage>();
            
            //Dialog
            containerRegistry.RegisterDialog<EditTaskNameDialog, EditTaskNameDialogViewModel>();
        }
    }
}