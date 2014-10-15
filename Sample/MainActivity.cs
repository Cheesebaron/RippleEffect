using Android.App;
using Android.Graphics;
using Android.Views;
using Android.OS;
using DK.Ostebaronen.FloatingActionButton;

namespace Sample
{
    [Activity(Label = "Sample", MainLauncher = true, Icon = "@drawable/ic_launcher", Theme = "@style/AppTheme")]
    public class MainActivity : Activity
    {
        private Fab _fab;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            _fab = FindViewById<Fab>(Resource.Id.button_floating_action);
            _fab.FabColor = Color.Purple;
            _fab.FabDrawable = Resources.GetDrawable(Resource.Drawable.ic_profil_plus);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_settings)
                return true;

            return base.OnOptionsItemSelected(item);
        }
    }
}

