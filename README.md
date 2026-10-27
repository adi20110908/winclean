# Winclean

A single-file Windows maintenance tool. One batch file, no installation, no
dependencies — double click it, approve the UAC prompt, and pick an option
from the menu. It cleans temporary files, throws out old Windows Update
downloads, repairs the system image, updates your apps, checks drivers,
health-checks your disks and RAM, and can lock a folder behind a password.

Latest version: **v4.5.0**

If the latest version is not working, use an older one:
https://github.com/adi20110908/winclean/tree/older-versions

---

## Requirements

| | |
|---|---|
| OS | Windows 10 / Windows 11 |
| Rights | Administrator — it asks for them at startup (UAC prompt) |
| Tools | Everything it uses ships with Windows: `Dism`, `sfc`, `cleanmgr`, `chkdsk`, `pnputil`, `icacls`, `mdsched`, `choice` |
| Optional | `winget` (App Installer) for options `5` and `D` — if it is missing, those options say so and skip instead of failing |

## Quick start

1. Download `winclean.bat`.
2. Double click it.
3. Press **Yes** on the UAC prompt (it relaunches itself with admin rights
   automatically — no need to "run as administrator" by hand).
4. Press the key of the option you want. **No Enter needed**, one keypress
   starts the action.

```
  +==================================================================+
    WINCLEAN v4.5.0   -   MAIN MENU
    user: Adi   free: 28.90 GB
  +==================================================================+

    CLEANING
      1  Temp files                user + system + public temp
      2  Browser cache             Chrome / Edge / Firefox cache only
      3  Old Windows Update files  update cache + component store
      4  Disk cleanup              cleanmgr, protected categories kept
      5  Recycle Bin               empty it, asks you first
      6  Repair system files       DISM RestoreHealth + SFC
      7  Winget app updates        upgrade every package
      8  Analyze disk              measure everything, delete nothing
      9  Deep clean + repair       1 + 3 + 4 + 6

    TOOLS
      A  Folder locker             lock / unlock with password
      B  Disk health               chkdsk
      C  RAM check                 Windows memory diagnostic
      D  Network fix               flush DNS / reset Winsock
      E  Restore point             snapshot before big changes
      F  System info               build, hardware, free space

    OTHER
      G  Driver check / update     list, back up, Windows Update
      H  Change color
      I  Change username
      J  Recommended apps
      K  Show log
      R  Run everything, then exit
      S  Restart winclean
      0  Exit

  select >
```

Every screen has the same framed header (version, screen name, your
username and the free space on the system drive) and every operation ends
with an optional rating prompt — press
**Enter** to skip it, nothing is sent anywhere, the rating is only written
to the local log. After the prompt you are back at the main menu; winclean
closes only when you press `0` (or after `R`, which is meant to quit).

---

## What each option does

### Cleaning

| Key | Option | What actually happens |
|---|---|---|
| `1` | Temp files | Deletes the contents of `%TEMP%`, `%SystemRoot%\Temp` and `%PUBLIC%\temp` (files with `del /f /q`, leftover directories with `rd /s /q`). Files that are in use are skipped, they are not an error. |
| `2` | Browser cache | Empties every browser profile's cache folders: Chrome and Edge `Cache`, `Code Cache`, `GPUCache`, Firefox `cache2` and `startupCache`. **Cache only** — passwords, cookies, history, bookmarks and site data stay where they are; the browser rebuilds the cache on its next start. If Chrome, Edge or Firefox is still running, winclean says so and asks before continuing, because files in use cannot be deleted anyway. |
| `3` | Old Windows Update files | Stops `wuauserv` and `bits`, empties `SoftwareDistribution\Download` (the folder where every downloaded update copy lives), clears the Delivery Optimization cache (`Delete-DeliveryOptimizationCache`), runs `Dism /Online /Cleanup-Image /StartComponentCleanup` to compact the component store, then restarts both services. |
| `4` | Disk cleanup | Writes `StateFlags0001 = 2` into every **safe** `VolumeCaches` profile in the registry, then runs `cleanmgr /sagerun:1`. This is the step most "disk cleanup" scripts forget — without the flags, `cleanmgr` silently does nothing. It no longer flags *everything*: `Recycle Bin`, `Previous Installations`, `Windows ESD installation files`, `User file versions` and `Downloads` are skipped, any flag an older version already wrote to them is removed, and each one is printed as `kept:` so you can see what was protected. |
| `5` | Recycle Bin | Shows how much is in the bin right now, asks `y/n`, then runs `Clear-RecycleBin -Force`. No undo — this is the one option that destroys files you might still want. |
| `6` | Repair system files | `Dism /Online /Cleanup-Image /RestoreHealth` (re-fetches broken system files from Windows Update) followed by `sfc /scannow`. If DISM fails, SFC is **skipped** — it needs a healthy component store — and DISM's error code is printed instead of pretending success. Expect 5–15 minutes. |
| `7` | Winget app updates | `winget upgrade --all --accept-package-agreements --accept-source-agreements`, so it never stops and waits for you to accept a licence. |
| `8` | Analyze disk | Measures and prints a table of MB and file counts for user temp, system temp, public temp, Windows Update downloads, the Delivery Optimization cache, the recycle bin, the Windows Error Reporting archive and the caches of Chrome, Edge and Firefox, plus the free space on the system drive. **Deletes nothing** — it is the preview of what the other options would free. |
| `9` | Deep clean + repair | Runs `1 + 3 + 4 + 6` back to back with a progress counter, then returns to the menu. |

### Tools

| Key | Option | What actually happens |
|---|---|---|
| `A` | Folder locker | Lock: hides the folder and applies an inherited `icacls /deny` for the `Everyone` SID, so nothing can be opened inside it. Unlock: asks for the password, removes the deny ACE and unhides. Refuses drive roots, `%SystemRoot%`, `System32`, `SysWOW64`, `%APPDATA%`, your user profile and winclean's own folder. |
| `B` | Disk health | Asks for a drive letter, then either checks it read-only (`chkdsk C:`) or checks and fixes (`chkdsk C: /F`). On `C:` Windows will offer to run the fix on the next boot. |
| `C` | RAM check | Starts the Windows Memory Diagnostic (`mdsched.exe`). Choose *Restart now* or *Schedule a restart* in the window it opens; the test runs at the next boot. |
| `D` | Network fix | Troubleshooting, not cleanup: flush the DNS resolver cache (`ipconfig /flushdns`, no restart needed) or reset the Winsock catalog (`netsh winsock reset`, needs a restart). The Winsock reset warns first that VPN and antivirus software with a network filter may have to be reinstalled. Both, in that order, is the third choice. |
| `E` | Restore point | `Checkpoint-Computer -Description 'winclean vX.Y.Z' -RestorePointType MODIFY_SETTINGS`. If it fails, winclean says why instead of staying silent: system protection switched off for the drive, or one was already created in the last 24 hours (Windows allows only one a day). |
| `F` | System info | Reads the registry for Windows product name, `DisplayVersion`, build number and UBR (shown as decimal, not `0x...`), edition; then CPU, RAM, free and total disk space, and where winclean keeps its folder and log. Read-only. |

### Other

| Key | Option | What actually happens |
|---|---|---|
| `G` | Driver check / update | Counts the driver packages in the driver store (`pnputil /enum-drivers` piped through `findstr`, so it works on a localized Windows too), then offers: open **Windows Update** (where driver updates actually come from), list every installed driver, or **back up the drivers** — `pnputil /export-driver *` into `Desktop\winclean-drivers` (falling back to Documents, then `%TEMP%`), which is your rollback if a driver update breaks something. |
| `H` | Change color | 15 presets (black background, one foreground color each). Saved, so it is used again on the next start. |
| `I` | Change username | Renames the greeting in the header. Saved. |
| `J` | Recommended apps | `winget install` for Java and LocalSend. Not sponsored, just a short useful list — edit the list in `:opt_apps` if you want other apps. |
| `K` | Show log | Pages through the all-time log. |
| `R` | Run everything, then exit | The five cleaning steps in order (temp, Windows Update files, disk cleanup, repair, winget), then quits. This is the "set it and forget it" mode. |
| `S` | Restart winclean | Starts a fresh instance and closes this one. |
| `0` | Exit | Quits. |

---

## Where your data lives

Nothing is written next to the script — everything goes to your profile:

```
%APPDATA%\winclean\
├── colormode.cfg      chosen color code
├── colorprofile.cfg   chosen color name
├── user.cfg           chosen username
├── locker.pw          folder locker password
└── logs\
    ├── winclean.log            all-time log (every run appends)
    └── winclean-latest.log     this session only, rewritten at start
```

Delete that folder to reset winclean to defaults; it recreates it on the
next start.

## The folder locker, honestly

It is an access-permission lock, **not encryption**:

* The password only gates winclean's unlock screen, and it is stored in
  plain text in `locker.pw`.
* The folder is hidden and denied through its ACL, so a normal double click
  fails — but an administrator can always undo it by hand, for example:

  ```
  icacls "C:\path\to\folder" /remove:d *S-1-1-0
  ```

* The deny is applied with the `Everyone` **SID** (`*S-1-1-0`) instead of the
  group name, so it also works on a Hungarian or any other localized
  Windows, where the group is not called "Everyone".
* Keep at least one unlocked copy of anything important. If you lock a
  folder and forget the password, the command above is your way out.

## Notes and limits

* Options `3` and `6` are slow on purpose — DISM and SFC do real work.
  Leave the window open.
* Option `4` opens the real Disk Cleanup window and waits for it to finish.
* Option `1` cannot delete files currently in use by a running program.
* Option `8` deletes nothing — run it first to see what is really there.
* Option `2` never touches passwords, cookies or history, but close the
  browsers anyway; winclean warns and asks if they are still running.
* Option `5` is the only one that destroys data you might want back, and
  it always asks before doing it.
* Option `4` deliberately leaves the recycle bin, the previous Windows
  installation, the Windows ESD files and user file versions alone — use
  Windows' own Disk Cleanup window if you want those removed too.
* Option `E` can only make one restore point every 24 hours.
* The header shows free space on the system drive, and any screen that
  actually freed something ends with a `freed: ... (free space now: ...)`
  line, so you can see what an option really accomplished.
* `R` runs everything including the slow steps, so expect 15–30 minutes.

## Version history

| Version | |
|---|---|
| **v4.5.0** | New options: browser cache cleanup (Chrome / Edge / Firefox, cache only), recycle bin with a confirmation, **Analyze disk** (MB and file counts for every target, deletes nothing), network fix (DNS flush / Winsock reset), restore point, system info, and driver backup. Disk cleanup now protects user data — recycle bin, previous installation, Windows ESD files, user file versions and downloads are kept instead of flagged — and the header plus every cleanup screen show free space and how much was freed. A failed DISM now skips SFC and prints the error code instead of carrying on. |
| v4.4.1 | Fixed the app quitting right after the rating prompt: every option now hands control back to the main menu explicitly, instead of depending on call/return bookkeeping. |
| v4.4.0 | Rewritten around shared subroutines (964 → 580 lines). Menu moved to single-key `choice` input with grouped sections; boot no longer runs from `System32`; `cleanmgr` profiles are configured before `sagerun`; repair uses `DISM /RestoreHealth`; `mdsched` is called from `System32`; user input can no longer break the command line. Added the old Windows Update cleanup, a working folder locker and a real driver check. |
| v4.3.1.1.1.1b | Previous release. |
| older | See the [`older-versions`](https://github.com/adi20110908/winclean/tree/older-versions) branch. |
