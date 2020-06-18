using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using TLR.Services;
using TLR.Views;

namespace TLR
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
