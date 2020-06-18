using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;
using TLR.Models;

namespace TLR.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class NewItemPage : ContentPage
    {
        private static readonly HttpClient client = new HttpClient();

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


            string connectionString = "DefaultEndpointsProtocol=https;AccountName=machinelearnin5244646737;AccountKey=6YB4jwp+Ek/wVKwBVIvrLunrFID9Pyxm9pe0IZ/5ViHadGKlFLANKLIhpvXbI7FkjuK0714Bx8yoaEjpThLOvQ==;EndpointSuffix=core.windows.net";

            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            //Create a unique name for the container
            string containerName = "predict";

            // Create the container and return a container client object
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(Path.GetFileName(photo.AlbumPath));

            Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

            using FileStream uploadFileStream = File.OpenRead(photo.Path);
            await blobClient.UploadAsync(uploadFileStream, true);
            uploadFileStream.Close();

            output.Text = blobClient.Uri.ToString();

            var values = new Dictionary<string, string> {};

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync("https://modelpythonapiv2.azurewebsites.net/predict?imageUrl="+ blobClient.Uri.ToString(), content);

            var responseString = await response.Content.ReadAsStringAsync();

            output.Text = responseString;
        }
    }
} 