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
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;
using Windows.Devices.Geolocation.Geofencing;
using Windows.UI.Popups;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.UI;  

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace GeoAttempFri
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
   
    public sealed partial class MainPage : Page
    {
        GeofenceMonitor _monitor = GeofenceMonitor.Current;
        Geolocator geo = null;
        private Uri imgSource = new Uri("ms-appx:///Images/GMITMap.jpg");
        public MainPage()
        {
           this.InitializeComponent();
           this.NavigationCacheMode = NavigationCacheMode.Required;
         
           _monitor.GeofenceStateChanged += MonitorOnGeofenceStateChanged;

           BasicGeoposition pos = new BasicGeoposition { Latitude = 53.860221, Longitude = -9.316038 };
           BasicGeoposition pos1 = new BasicGeoposition { Latitude = 53.861204, Longitude = -9.312986 };


           Geofence fence = new Geofence("Home", new Geocircle(pos, 50));
           Geofence fence1 = new Geofence("Home50", new Geocircle(pos1, 50));

           try
           {
               _monitor.Geofences.Add(fence);
           }
           catch (Exception)
           {
               //geofence already added to system
           }
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
    
       private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (MyMap != null)
                MyMap.ZoomLevel = e.NewValue;
        }

       private async void DisplayGPS_Click(object sender, RoutedEventArgs e)
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

       private void NewGeofence(String key, Geopoint position)
       {
           var oldFence = GeofenceMonitor.Current.Geofences.Where(p => p.Id == key).FirstOrDefault();
           if(oldFence != null)
           {
               GeofenceMonitor.Current.Geofences.Remove(oldFence);
           }
           Geocircle geocircle = new Geocircle(position.Position, 25);
           bool singleUse = false;

           MonitoredGeofenceStates mask = 0;
           mask |= MonitoredGeofenceStates.Entered;

           var geofence = new Geofence(key, geocircle, mask, singleUse, TimeSpan.FromSeconds(1));
           GeofenceMonitor.Current.Geofences.Add(geofence);
       }

       private void MonitorOnGeofenceStateChanged(GeofenceMonitor sender, object args)
        {
            var geoReports = GeofenceMonitor.Current.ReadReports();
            foreach (var geofenceStateChangeReport in geoReports)
            {
                var id = geofenceStateChangeReport.Geofence.Id;
                var newState = geofenceStateChangeReport.NewState.ToString();
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    new MessageDialog(newState + " : " + id)
                        .ShowAsync());
            }
        }

       private async void LocateMe_Click(object sender, RoutedEventArgs e)
       {
           myProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
           geo = new Geolocator();
           geo.DesiredAccuracyInMeters = 50;

           try
           {
               Geoposition geoposition = await geo.GetGeopositionAsync( maximumAge: TimeSpan.FromMinutes(5),timeout: TimeSpan.FromSeconds(10));
               MapIcon icon = new MapIcon();
               icon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Images/me.png"));
               icon.Title = "This is Me";
               icon.Location = new Geopoint(new BasicGeoposition() { Latitude = geoposition.Coordinate.Point.Position.Latitude, Longitude = geoposition.Coordinate.Point.Position.Longitude });
               icon.NormalizedAnchorPoint = new Point(0.5, 0.5);
               MyMap.MapElements.Add(icon);
               await MyMap.TrySetViewAsync(geoposition.Coordinate.Point, 18D);
               mySlider.Value = MyMap.ZoomLevel;
               myProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
           }
           catch (UnauthorizedAccessException)
           {
               new MessageDialog("Location service is turned off!");
           }   
       }

       private void ShowGeofence_Click(object sender, RoutedEventArgs e)
       {

       }
    }
}

      
    

