# VRStarterKit

## Outcome messages

Success and failure messages come from `StringConstants.cs`. The result frame
measures and wraps each message at a readable font size, expands to fit it, and
moves the banner and buttons to keep them separated. The current French messages
fit without scrolling. Messages longer than the canvas can accommodate use a
masked scrolling area with large **Haut** and **Bas** buttons. The frame shrinks
again for shorter messages; scrolling resets whenever the result changes.

## Player registration CSV

Pressing Register with a valid name and email first submits the registration to
the configured Google Apps Script endpoint. After its JSON response confirms
`status: success`, the app appends one row to `registrations.csv` and starts gameplay.
Columns: `RegisteredAtUtc,Name,Email`. The CSV date is UTC.
Every popup opening clears both fields. Cancelling does not save.
If saving fails, the form keeps the entered details and allows another attempt.
Repeated clicks during submission do not create duplicate rows.

## Google Sheets submission

The URL is configured in `RegistrationSheetClient.Endpoint`. The POST body is
UTF-8 JSON with `name`, `email`, and `dateTime` (device local time, `yyyy-MM-dd HH:mm`).
Unity follows Google's redirect and requires a JSON `status` of `success` before
continuing. Android Internet permission is enabled in Player Settings.

Registration requires Internet. No connection, a connection error, or a timeout
keeps the form open with `Connexion Internet requise`, retains the entered details,
and permits retry. A server error also keeps the form open. While submitting, the
fields, Register and Cancel are disabled to prevent overlapping requests.

If Google confirms saving but the local CSV fails, retry completes only the local
save; it does not resend the already confirmed Google submission. Admin entry
continues to open the local admin menu without uploading an admin registration.
The admin table still displays this device's CSV, rather than the entire Google Sheet.

The endpoint has no documented idempotency key. If a request reaches Google but
its response is lost, a manual retry may create another Google row. Exactly-once
submission in that situation requires server-side deduplication.

## Quest system keyboard

The custom keyboard is replaced by the Quest system keyboard. Select either
registration field with the controller ray to type; the email field requests
the email keyboard layout. Finish text entry, then press Register.

The Android build hook adds `oculus.software.overlay_keyboard` to Unity's generated
manifest automatically. The installed Oculus XR plugin enables the keyboard
overlay and focus awareness. No manual scene configuration is needed.

Build and install the updated Android APK to test the keyboard directly on Quest
3S. Unity Play mode through Link/Air Link runs on Windows and uses your physical
PC keyboard; it does not display the standalone Quest system keyboard overlay.

## Admin registration viewer

Enter name `admin` and email `admin@admin.com`, then press Register. Leading and
trailing spaces and letter case are ignored. Both values must match. This opens
an admin menu with **Start** and **View registrations**. Admin entry does not add
a player registration to the CSV.

**View registrations** displays all registrations saved on this device, newest
first, with name, email, and local date/time. Scroll with the controller ray or
use the large **Up** and **Down** buttons. **Refresh** reloads the file; **Back**
returns to the admin menu. An empty file shows "No registrations yet"; a read
failure shows a message and permits retry. **Log out** returns to a blank
registration form. **Start** launches the normal gameplay flow.

Ordinary players still save their details and start gameplay immediately. The
CSV records registrations, rather than completed playthroughs. The viewer reads
the local device file, so Quest registrations appear on Quest and Unity Play
mode registrations appear on the PC.

## CSV location and download

The file is stored under Unity's `Application.persistentDataPath`, on the device
running the app. The Unity Console logs the exact path after a successful save.
With the current project settings:

- Unity Play mode / Quest Link on this PC:
  `C:\Users\PC\AppData\LocalLow\Yudiz\Techpal\registrations.csv`
- Installed Quest APK:
  `/storage/emulated/0/Android/data/com.Yudiz.Techpal/files/registrations.csv`

Registrations append across app restarts. PC and Quest maintain separate files.
Installing an update with the same application ID preserves app data; uninstalling
the app or clearing its data removes it. Existing PlayerPrefs values are no longer
loaded into the fields; they are not historical registration records.

To copy the Quest file with Android Platform Tools, connect the headset by USB,
enable developer mode, and accept the USB debugging prompt in the headset. Run:

```powershell
adb devices
adb pull "/sdcard/Android/data/com.Yudiz.Techpal/files/registrations.csv" "$env:USERPROFILE\Downloads\registrations.csv"
```

Open the downloaded CSV in Excel. If Excel displays one column, use Data > From
Text/CSV, select UTF-8 encoding and a comma delimiter. The file uses UTF-8 with a
BOM, quoted fields, and escapes commas, quotes and line breaks. Formula-like
values are prefixed with an apostrophe for safe spreadsheet import.

Writing this app-specific file requires no extra storage permission. USB debugging
is needed for the `adb` download method. Use the updated Android build on Quest;
playing through Link writes to the PC file.
