using Microsoft.Practices.ServiceLocation;
using GalaSoft.MvvmLight.Ioc;

namespace TorrentHardLinkHelper.ViewModels
{
    /// <summary>
    ///     This class contains static references to all the view models in the
    ///     application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        ///     Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            var container = new SimpleIoc();
            container.Register<MainViewModel>();

            //var builder = new ContainerBuilder();
            //builder.RegisterType<MainViewModel>();

            //IContainer container = builder.Build();
            //var locator = new AutofacServiceLocator(container);

            ServiceLocator.SetLocatorProvider(() => container);
        }

        public MainViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}