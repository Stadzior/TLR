using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using TLR.Models;
using TLR.Services;

namespace TLR.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class NewItemPage : ContentPage
    {
        public Item Item { get; set; }

        public NewItemPage()
        {
            InitializeComponent();
        }

        async void Save_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "AddItem", Item);
            await Navigation.PopModalAsync();
        }

        async void Cancel_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
        private async void TakePhotoButton_Clicked(object sender, EventArgs e)
        {
            await ProcessPhotoAsync(true);
        }

        private async void PickPhotoButton_Clicked(object sender, EventArgs e)
        {
            await ProcessPhotoAsync(false);
        }

        private async Task ProcessPhotoAsync(bool useCamera)
        {
            await CrossMedia.Current.Initialize();
            if (useCamera ? !CrossMedia.Current.IsTakePhotoSupported : !CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Info", "Your phone doesn't support photo feature.", "OK");
                return;
            }

            var photo = useCamera ?
                await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions()) :
                await CrossMedia.Current.PickPhotoAsync();
            if (photo == null)
            {
                picture.Source = null;
                return;
            }

            picture.Source = ImageSource.FromFile(photo.Path);

            var service = DependencyService.Get<IPhotoDetector>();
            if (service == null)
            {
                await DisplayAlert("Info", "Not implemented the feature on your device.", "OK");
                return;
            }

            using (var s = photo.GetStream())
            {
                var result = await service.DetectAsync(s);
                output.Text = $"It looks like a {result}";
            }
        }
    }
} 