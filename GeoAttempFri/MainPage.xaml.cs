using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;
using Windows.Devices.Geolocation.Geofencing;
using Windows.UI.Popups;
using Windows.Storage.Streams;  

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace GeoAttempFri
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
   
    public sealed partial class MainPage : Page
    {
        Geolocator geo = null;
        private Uri imgSource = new Uri("ms-appx:///Images/GMITMap.jpg");
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            MyMap.MapServiceToken = "asdfds-asdsadsafsfdsas";
            geo = new Geolocator();
            geo.DesiredAccuracyInMeters = 20;

            try
            {
                Geoposition geoposition = await geo.GetGeopositionAsync(maximumAge: TimeSpan.FromMinutes(20),timeout: TimeSpan.FromSeconds(20));
                MapIcon icon = new MapIcon();
                icon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Images/me.png"));
                icon.Title = "This is Me";
                icon.Location = new Geopoint(new BasicGeoposition() {Latitude = geoposition.Coordinate.Point.Position.Latitude, Longitude = geoposition.Coordinate.Point.Position.Longitude});
                icon.NormalizedAnchorPoint = new Point(0.5, 0.5);
                MyMap.MapElements.Add(icon);
                await MyMap.TrySetViewAsync(icon.Location, 18D, 0, 0, MapAnimationKind.Bow);
                myProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                mySlider.Value = MyMap.ZoomLevel;
            }
            catch(UnauthorizedAccessException)
            {
                new MessageDialog("Cant get your location, check if location is enabled");
            }
           
        }

        // Slider Control  
        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (MyMap != null)
                MyMap.ZoomLevel = e.NewValue;
        }  

       private void addFence(string uniqueId, Geocircle geoCircle)
       {
           string geofenceId = uniqueId;
           geofenceId = uniqueId + "WithDefalts";
           Geofence geofenceBasic = new Geofence(geofenceId, geoCircle);

           geofenceId = uniqueId + "WithSingleUse";
           Boolean useOnce = true;
           Geofence geofenceSingleUse = new Geofence(geofenceId,geoCircle, MonitoredGeofenceStates.Entered | MonitoredGeofenceStates.Removed, useOnce);
   
       }

       private async void btnGetLocation(object sender, RoutedEventArgs e)
        {
            if (geo == null)
            {
                geo = new Geolocator();
            }

            Geoposition pos = await geo.GetGeopositionAsync();
            textLatitude.Text = "Latitude: " + pos.Coordinate.Point.Position.Latitude.ToString();
            textLongitude.Text = "Longitude: " + pos.Coordinate.Point.Position.Longitude.ToString();
            textAccuracy.Text = "Accuracy: " + pos.Coordinate.Accuracy.ToString();
        }





       private async void LocateMe_Click(object sender, RoutedEventArgs e)
       {
           myProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
           geo = new Geolocator();
           geo.DesiredAccuracyInMeters = 50;

           try
           {
               Geoposition geoposition = await geo.GetGeopositionAsync(
                   maximumAge: TimeSpan.FromMinutes(5),
                   timeout: TimeSpan.FromSeconds(10));
               await MyMap.TrySetViewAsync(geoposition.Coordinate.Point, 18D);
               mySlider.Value = MyMap.ZoomLevel;
               myProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
           }
           catch (UnauthorizedAccessException)
           {
               new MessageDialog("Location service is turned off!");
           }   
        
       }

      
    }
}
