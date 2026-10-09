# VRStarterKit

## Player registration CSV

Pressing Register with a valid name and email appends one row to `registrations.csv`
before gameplay starts. Columns: `RegisteredAtUtc,Name,Email`. The date is UTC.
Every popup opening clears both fields and resets Shift. Cancelling does not save.
If saving fails, the form keeps the entered details and allows another attempt.
Repeated clicks during submission do not create duplicate rows.

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
