using System;
using System.Threading;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/yandex_api_key_title",
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.KeyboardHidden |
                               Android.Content.PM.ConfigChanges.Orientation |
                               Android.Content.PM.ConfigChanges.ScreenSize)]
    public class YandexKeyActivity : AppCompatActivity, View.IOnTouchListener
    {
        private Java.Lang.Object _clipboardManager;
        private InputMethodManager _imm;
        private Timer _clipboardCheckTimer;
        private string _oldYandexApiKey;
        private View _contentView;
        private LinearLayout _layoutYandexKey;
        private Button _buttonYandexApiKeyCreate;
        private Button _buttonYandexApiKeyGet;
        private Button _buttonYandexApiKeyPaste;
        private EditText _editTextYandexApiKey;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SetContentView(Resource.Layout.yandex_key_select);

            _clipboardManager = GetSystemService(ClipboardService);
            _imm = (InputMethodManager)GetSystemService(InputMethodService);
            _contentView = FindViewById<View>(Android.Resource.Id.Content);

            SetResult(Android.App.Result.Canceled);

            _oldYandexApiKey = ActivityCommon.YandexApiKey ?? string.Empty;

            _layoutYandexKey = FindViewById<LinearLayout>(Resource.Id.layoutYandexKey);
            _layoutYandexKey.SetOnTouchListener(this);

            _editTextYandexApiKey = FindViewById<EditText>(Resource.Id.editTextYandexApiKey);
            _editTextYandexApiKey.Text = _oldYandexApiKey;

            _buttonYandexApiKeyCreate = FindViewById<Button>(Resource.Id.buttonYandexKeyCreate);
            _buttonYandexApiKeyCreate.SetOnTouchListener(this);
            _buttonYandexApiKeyCreate.Click += (sender, args) =>
            {
                StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"https://tech.yandex.com/keys/get/?service=trnsl")));
            };

            _buttonYandexApiKeyGet = FindViewById<Button>(Resource.Id.buttonYandexKeyGet);
            _buttonYandexApiKeyGet.SetOnTouchListener(this);
            _buttonYandexApiKeyGet.Click += (sender, args) =>
            {
                StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"https://tech.yandex.com/keys/")));
            };

            _buttonYandexApiKeyPaste = FindViewById<Button>(Resource.Id.buttonYandexKeyPaste);
            _buttonYandexApiKeyPaste.SetOnTouchListener(this);
            _buttonYandexApiKeyPaste.Click += (sender, args) =>
            {
                ClipboardManager clipboardManagerNew = _clipboardManager as ClipboardManager;
                if (clipboardManagerNew != null)
                {
                    if (clipboardManagerNew.HasPrimaryClip && clipboardManagerNew.PrimaryClipDescription.HasMimeType(ClipDescription.MimetypeTextPlain))
                    {
                        ClipData.Item item = clipboardManagerNew.PrimaryClip.GetItemAt(0);
                        if (!string.IsNullOrWhiteSpace(item?.Text))
                        {
                            _editTextYandexApiKey.Text = item.Text.Trim();
                        }
                    }
                }
                else
                {
#pragma warning disable 618
                    Android.Text.ClipboardManager clipboardManagerOld = _clipboardManager as Android.Text.ClipboardManager;
#pragma warning restore 618
                    if (!string.IsNullOrWhiteSpace(clipboardManagerOld?.Text))
                    {
                        _editTextYandexApiKey.Text = clipboardManagerOld.Text.Trim();
                    }
                }
            };
            UpdateDisplay();
        }

        protected override void OnResume()
        {
            base.OnResume();
            UpdateDisplay();
            if (_clipboardCheckTimer == null)
            {
                _clipboardCheckTimer = new Timer(state =>
                {
                    UpdateDisplay();
                }, null, 1000, 1000);
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (_clipboardCheckTimer != null)
            {
                _clipboardCheckTimer.Dispose();
                _clipboardCheckTimer = null;
            }
        }

        public override void OnBackPressed()
        {
            if (StoreYandexKey((sender, args) =>
            {
                base.OnBackPressed();
            }))
            {
                base.OnBackPressed();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            HideKeyboard();
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (StoreYandexKey((sender, args) =>
                    {
                        Finish();
                    }))
                    {
                        Finish();
                    }
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    HideKeyboard();
                    break;
            }
            return false;
        }

        private void UpdateDisplay()
        {
            bool pasteEnable = false;
            ClipboardManager clipboardManagerNew = _clipboardManager as ClipboardManager;
            if (clipboardManagerNew != null)
            {
                if (clipboardManagerNew.HasPrimaryClip && clipboardManagerNew.PrimaryClipDescription.HasMimeType(ClipDescription.MimetypeTextPlain))
                {
                    ClipData.Item item = clipboardManagerNew.PrimaryClip.GetItemAt(0);
                    if (!string.IsNullOrWhiteSpace(item?.Text))
                    {
                        pasteEnable = true;
                    }
                }
            }
            else
            {
#pragma warning disable 618
                Android.Text.ClipboardManager clipboardManagerOld = _clipboardManager as Android.Text.ClipboardManager;
#pragma warning restore 618
                if (!string.IsNullOrWhiteSpace(clipboardManagerOld?.Text))
                {
                    pasteEnable = true;
                }
            }
            _buttonYandexApiKeyPaste.Enabled = pasteEnable;
        }

        private bool StoreYandexKey(EventHandler handler)
        {
            string newYandexApiKey = _editTextYandexApiKey.Text.Trim();
            if (string.Compare(_oldYandexApiKey, newYandexApiKey, StringComparison.Ordinal) == 0)
            {
                return true;
            }
            new AlertDialog.Builder(this)
                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                {
                    ActivityCommon.YandexApiKey = newYandexApiKey;
                    SetResult(Android.App.Result.Ok);
                    handler?.Invoke(sender, args);
                })
                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                {
                    handler?.Invoke(sender, args);
                })
                .SetCancelable(true)
                .SetMessage(Resource.String.translate_store_key)
                .SetTitle(Resource.String.alert_title_question)
                .Show();
            return false;
        }

        private void HideKeyboard()
        {
            _imm?.HideSoftInputFromWindow(_contentView.WindowToken, HideSoftInputFlags.None);
        }
    }
}