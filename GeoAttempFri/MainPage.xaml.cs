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
using MappingUtilities.Geofencing;
using Windows.ApplicationModel.Background; 



// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace GeoAttempFri
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
   
    public sealed partial class MainPage : Page
    {
        //Create Geofence Monitor and create Geolocator
        GeofenceMonitor _monitor = GeofenceMonitor.Current;
        Geolocator geo = null;
        private const string taskName = "BlogFeedBackgroundTask";
        private const string taskEntryPoint = "BackgroundTasks.BlogFeedBackgroundTask";

        public MainPage()
        {
          
           this.InitializeComponent();
           this.NavigationCacheMode = NavigationCacheMode.Required;
           //Monitor state change.
           _monitor.GeofenceStateChanged += MonitorOnGeofenceStateChanged;

          //Create locations for Fence areas in GMIT
           BasicGeoposition tourismAndArts2 = new BasicGeoposition { Latitude = 53.278892, Longitude = -9.011625 };
           BasicGeoposition sportHall = new BasicGeoposition { Latitude = 53.278044, Longitude = -9.012473 };
           BasicGeoposition gmitLibrary = new BasicGeoposition { Latitude = 53.277628, Longitude = -9.009860 };
           BasicGeoposition scienceDepartment = new BasicGeoposition { Latitude = 53.278763, Longitude = -9.010220 };
           BasicGeoposition tourismAndArts = new BasicGeoposition { Latitude = 53.278519, Longitude = -9.010423 };
           BasicGeoposition engineering = new BasicGeoposition { Latitude = 53.278256, Longitude = -9.010466 };
           BasicGeoposition businessAndHum = new BasicGeoposition { Latitude = 53.279296, Longitude = -9.010987 };
           BasicGeoposition frontFootEntrance = new BasicGeoposition { Latitude = 53.277499,  Longitude = -9.010820 };
           BasicGeoposition receptionEntrance = new BasicGeoposition { Latitude = 53.279177, Longitude = -9.009941 };
           //Create GeoFences
           Geofence TourismAndArts2 = new Geofence("Your in Block C, Arts And Tourism Department 400-300", new Geocircle(tourismAndArts2, 25));
           Geofence SportHall = new Geofence("Your near the GMIT Sports Hall", new Geocircle(sportHall, 50));
           Geofence GmitLibrary = new Geofence("Your near the Library", new Geocircle(gmitLibrary, 30));
           Geofence ScienceDepartment = new Geofence("Your in Block B, Science Department 400-300", new Geocircle(scienceDepartment, 18));
           Geofence ArtsAndTourismDepartment = new Geofence("Your in Block C, Arts And Tourism Department 600-500", new Geocircle(tourismAndArts, 18));
           Geofence Engineering = new Geofence("Your in Block D, Engineering Department 800-700", new Geocircle(engineering, 18));
           Geofence BusinessAndHum = new Geofence("Your in Block A, Business and Humanities 200-300", new Geocircle(businessAndHum , 30));
           Geofence FrontFootEntrance = new Geofence("Your near the front entrance walkway, near bus", new Geocircle(frontFootEntrance, 18));
           Geofence ReceptionEntrance = new Geofence("Your near the entrance near the Reception 941-1000", new Geocircle(receptionEntrance, 25));
           //Add the Fences to the monitor
           try
           {
               _monitor.Geofences.Add(TourismAndArts2);
               _monitor.Geofences.Add(SportHall);
               _monitor.Geofences.Add(GmitLibrary);
               _monitor.Geofences.Add(ScienceDepartment);
               _monitor.Geofences.Add(ArtsAndTourismDepartment);
               _monitor.Geofences.Add(Engineering);
               _monitor.Geofences.Add(FrontFootEntrance);
               _monitor.Geofences.Add(BusinessAndHum);
               _monitor.Geofences.Add(ReceptionEntrance);
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
            this.RegisterBackgroundTask();
            MyMap.MapServiceToken = "ApxazuNjNEBsZrppTMoLTXgXn8WXwMongiF9BQAFIH1xC_zvbUn4HO8IeSPf-zgk";
            geo = new Geolocator();
            geo.DesiredAccuracyInMeters = 20;

            try
            {
                //Get a handle on location, and add image overlay to postion 
                Geoposition geoposition = await geo.GetGeopositionAsync(maximumAge: TimeSpan.FromMinutes(20),timeout: TimeSpan.FromSeconds(20));
                MapIcon icon = new MapIcon();
                icon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Images/GMITMap1.jpg"));
                icon.ZIndex = 1;
                icon.Location = new Geopoint(new BasicGeoposition() { Latitude = 53.278481, Longitude = -9.010477 });
                icon.NormalizedAnchorPoint = new Point(0.5, 0.5);
                MyMap.MapElements.Add(icon);
                await MyMap.TrySetViewAsync(icon.Location, 18D, 0, 0, MapAnimationKind.Bow);
                myProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                mySlider.Value = MyMap.ZoomLevel;
                MyMap.ZoomLevel = 17;
            }
            catch(UnauthorizedAccessException)
            {
                new MessageDialog("Cant get your location, check if location is enabled");
            }
        }

       private async void RegisterBackgroundTask()
       {
           var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
            if( backgroundAccessStatus == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity ||
                backgroundAccessStatus == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity )
            {
                foreach( var task in BackgroundTaskRegistration.AllTasks )
                {
                    if( task.Value.Name == taskName )
                    {
                        task.Value.Unregister( true );
                    }
                }

                BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder();
                taskBuilder.Name = taskName;
                taskBuilder.TaskEntryPoint = taskEntryPoint;
                taskBuilder.SetTrigger( new TimeTrigger( 15, false ) );
                var registration = taskBuilder.Register();
            }
        }

       private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
           //create slider control
            if (MyMap != null)
                MyMap.ZoomLevel = e.NewValue;
        }

       private async void DisplayGPS_Click(object sender, RoutedEventArgs e)
       {
           //Displays the coordinates of the current location to then user
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
           //Adds a new Geofence, no used at moment but could be implemented to add custom fences by the user
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
           //Monitor the change in state of a fence, send message when fence 
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
           //Add the location icon to map, used to place icons on map to track location
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
               MyMap.ZoomLevel = 17;
               myProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
           }
           catch (UnauthorizedAccessException)
           {
               new MessageDialog("Location service is turned off!");
           }   
       }

       private void ShowGeofence_Click(object sender, RoutedEventArgs e)
       {
           DrawGeofences();
       }

       private void DrawGeofences()
       {
           //Draw semi transparent purple circles for every fence
           var color = Colors.Red;
           color.A = 80;

           // Note GetFenceGeometries is a custom extension method
           foreach (var pointlist in GeofenceMonitor.Current.GetFenceGeometries())
           {
               var shape = new MapPolygon
               {
                   FillColor = color,
                   StrokeColor = color,
                   Path = new Geopath(pointlist.Select(p => p.Position)),
               };
               MyMap.MapElements.Add(shape);
           }
       }

       private async void BackToGMIT_Click(object sender, RoutedEventArgs e)
       {
         //Button to return to image overlay
           geo = new Geolocator();
           geo.DesiredAccuracyInMeters = 20;

           try
           {
               Geoposition geoposition = await geo.GetGeopositionAsync(maximumAge: TimeSpan.FromMinutes(20), timeout: TimeSpan.FromSeconds(20));
               MapIcon icon = new MapIcon();
               icon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Images/GMITMap1.jpg"));
               icon.ZIndex = 1;
               icon.Location = new Geopoint(new BasicGeoposition() { Latitude = 53.278481, Longitude = -9.010477 });
               icon.NormalizedAnchorPoint = new Point(0.5, 0.5);
               MyMap.MapElements.Add(icon);
               await MyMap.TrySetViewAsync(icon.Location, 18D, 0, 0, MapAnimationKind.Bow);
               myProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
               mySlider.Value = MyMap.ZoomLevel;
               MyMap.ZoomLevel = 17;
           }
           catch (UnauthorizedAccessException)
           {
               new MessageDialog("Cant get your location, check if location is enabled");
           }
           
       }

    }
}

      
    

