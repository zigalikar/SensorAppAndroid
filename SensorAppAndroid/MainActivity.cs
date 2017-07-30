using System;
using System.Collections.Generic;

using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;

using Android.Hardware; // namespace for SensorManager - acceleration, orientation
using Android.Locations; // namespace for LocationManager

namespace SensorAppAndroid
{
    [Activity(Label = "SensorAppAndroid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, ISensorEventListener, ILocationListener
    {
        SensorManager sensorManager;

        // Accelerometer
        TextView accelText; // XYZ acceleration textview
        TextView accelLastUp; // last updated textview

        // Location
        LocationManager locManager;
        Location currLoc; // current location
        TextView locText; // location textview
        TextView providerText; // provider textview
        TextView locLastUp; // last updated textview
        String locProvider = String.Empty; // location provider name

        // Orientation
        TextView orientationText; // XYZ orientation textview
        TextView orientationLastUp; // last updated textview
        float[] gravity = new float[3]; // for storing accelerometer values from sensor
        float[] geomagnetic = new float[3]; // for storing geomagnetic values from sensor

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Sensors initialization
            sensorManager = (SensorManager) GetSystemService(Context.SensorService);

            /* ============================
             *          ACCELEROMETER
             * ============================ */
            // SetContentView - sets this activity view
            // Resource.Layout.Main - Resources/layout/Main.axml
            SetContentView(Resource.Layout.Main);
           
            // Getting TextViews
            accelText = FindViewById<TextView>(Resource.Id.accel_text);
            accelLastUp = FindViewById<TextView>(Resource.Id.accel_lastUp);

            /* ============================
             *          GPS LOCATION
             * ============================ */
            // Getting TextViews
            locText = FindViewById<TextView>(Resource.Id.loc_text);
            providerText = FindViewById<TextView>(Resource.Id.loc_provider);
            locLastUp = FindViewById<TextView>(Resource.Id.loc_lastUp);

            // Initializing location manager
            locInit();

            /* ============================
             *          ORIENTATION
             * ============================ */
             // Getting TextViews
            orientationText = FindViewById<TextView>(Resource.Id.orientation_text);
            orientationLastUp = FindViewById<TextView>(Resource.Id.orientation_lastUp);
        }

        private void locInit()
        {
            // Gets service for location manager
            locManager = (LocationManager) GetSystemService(LocationService);

            // creates new criteria - sets accuracy to fine (for longitude/latitude location)
            Criteria locCriteria = new Criteria { Accuracy = Accuracy.Fine };

            // LocationManager.GetBestProvider(criteria, enabledOnly) - returns best available location provider according to criteria
            // criteria - previously created criteria
            // enabledOnly = true - returns only currently enabled providers
            String availableLocProvider = locManager.GetBestProvider(locCriteria, true);
            if (availableLocProvider != null)
            {
                locProvider = availableLocProvider;
                providerText.Text = "Provider: " + locProvider;
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            // SensorManager.RegisterListener(listener, sensor, rateUs)
            // listener - ISensorEventListener object (this [activity implements from ISensorEventListener])
            // sensor - what sensor to register to
            // rateUs - the rate events are delivered at - can be received faster or slower
            sensorManager.RegisterListener(this, sensorManager.GetDefaultSensor(SensorType.Accelerometer), SensorDelay.Ui);
            sensorManager.RegisterListener(this, sensorManager.GetDefaultSensor(SensorType.MagneticField), SensorDelay.Ui);

            // LocationManager.RequestLocationUpdates
            locManager.RequestLocationUpdates(locProvider, 0, 0, this);
        }

        protected override void OnPause()
        {
            base.OnPause();

            // UnregisterListener(listener, sensor)
            // listener - what SensorEventListener to unregister from
            // sensor - what sensor to unregister from
            sensorManager.UnregisterListener(this);

            // LocationManager.RemoveUpdates(ILocationListener)
            locManager.RemoveUpdates(this);
        }

        public void OnSensorChanged(SensorEvent e) // when sensor changes values
        {
            if (e.Sensor.Type == SensorType.Accelerometer)
            {
                // Save accelerometer values for further use
                gravity[0] = e.Values[0];
                gravity[1] = e.Values[1];
                gravity[2] = e.Values[2];

                // Update textviews
                accelText.Text = string.Format("X: {0:f}{3}Y: {1:f}{3}Z: {2:f}", e.Values[0], e.Values[1], e.Values[2], System.Environment.NewLine);
                accelLastUp.Text = "Last updated: " + DateTime.Now.ToLocalTime();
            }

            if (e.Sensor.Type == SensorType.MagneticField)
            {
                // Save geomagnetic values for further use
                geomagnetic[0] = e.Values[0];
                geomagnetic[1] = e.Values[1];
                geomagnetic[2] = e.Values[2];
            }

            // R, I matrices
            float[] R = new float[9];
            float[] I = new float[9];
            bool result = SensorManager.GetRotationMatrix(R, I, gravity, geomagnetic); // tries to get the rotation matrix
            if (result)
            {
                // GetOrientation requires an array of 3 floats to return values
                float[] orientation = new float[3];
                SensorManager.GetOrientation(R, orientation);

                // Update textviews
                orientationText.Text = string.Format("Azimuth (-Z): {0:f} rad{3}Pitch (-X): {1:f} rad{3}Roll (Y): {2:f} rad", orientation[0], orientation[1], orientation[2], System.Environment.NewLine);
                orientationLastUp.Text = "Last updated: " + DateTime.Now.ToLocalTime();
            }
        }

        public void OnLocationChanged(Location loc) // when location manager changes locations
        {
            currLoc = loc;
            if (currLoc == null)
            {
                // Unable to locate device
                locText.Text = "Unable to locate.";
            }
            else
            {
                // Update textviews
                locText.Text = String.Format("Latitude: {0:f}{2}Longitude: {1:f}", currLoc.Latitude, currLoc.Longitude, System.Environment.NewLine);
                locLastUp.Text = "Last updated: " + DateTime.Now.ToLocalTime();
            }
        }
        
        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy) { } // unused

        public void OnProviderDisabled(String provider) { } // unused
        public void OnProviderEnabled(String provider) { } // unused
        public void OnStatusChanged(String provider, Availability status, Bundle extras) { } // unused
    }
}