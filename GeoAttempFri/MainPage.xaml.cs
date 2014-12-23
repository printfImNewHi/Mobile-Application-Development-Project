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
            this.InitialiseGeoFence();

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
        // Slider control to set zoom
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

       private void Track_Click(object sender, RoutedEventArgs e)
       {
          
          
       }

       
        
       private async Task InitialiseGeoFence()
       {
           var geoMon = GeofenceMonitor.Current;
           var GeoId1 = "Test1";
           var GeoId2 = "Test2";
           var location = await new Geolocator().GetGeopositionAsync(TimeSpan.FromMinutes(5),TimeSpan.FromSeconds(3));
           try{
           geoMon.GeofenceStateChanged += (sender, args) =>
               {
                   var geoReport = geoMon.ReadReports();
                   foreach(var geofenceStateChangeReport in geoReport)
                   {
                       var id = geofenceStateChangeReport.Geofence.Id;
                       var newState = geofenceStateChangeReport.NewState;

                       if(id==GeoId1 && newState==GeofenceState.Entered)
                       {
                            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>new MessageDialog("Test 1 Geofence enabled").ShowAsync());
                       }
                       else if (id == GeoId2 && newState == GeofenceState.Entered)
                       {
                           Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => new MessageDialog("Test 2 Geofence enabled").ShowAsync());
                       }
                   }
               };

           var test1 = new BasicGeoposition()
           {
               Latitude = 53.860284,
               Longitude = -9.316087
           };

           var test2 = new BasicGeoposition()
           {
               Latitude = 53.858468,
               Longitude = -9.309102
           };

           var geo1 = new Geofence(GeoId1, new Geocircle(test1, 200), MonitoredGeofenceStates.Entered , false, TimeSpan.FromSeconds(10));
           var geo2 = new Geofence(GeoId1, new Geocircle(test2, 200), MonitoredGeofenceStates.Entered , false, TimeSpan.FromSeconds(10));
           geoMon.Geofences.Add(geo1);
           geoMon.Geofences.Add(geo2);
           }
           catch (Exception e)
           {
            Debug.WriteLine(e);
            // geofence already added to system
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
