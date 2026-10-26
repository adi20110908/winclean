# Winclean

A single-file Windows maintenance tool. One batch file, no installation, no
dependencies — double click it, approve the UAC prompt, and pick an option
from the menu. It cleans temporary files, throws out old Windows Update
downloads, repairs the system image, updates your apps, checks drivers,
health-checks your disks and RAM, and can lock a folder behind a password.

Latest version: **v4.4.1**

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
    WINCLEAN v4.4.1   -   MAIN MENU
    user: Adi
  +==================================================================+

    CLEANING
      1  Temp files                user + system + public temp
      2  Old Windows Update files  SoftwareDistribution + DISM
      3  Disk cleanup              cleanmgr, all profiles
      4  Repair system files       DISM RestoreHealth + SFC
      5  Winget app updates        upgrade every package
      6  Driver check / update     pnputil + Windows Update

    TOOLS
      7  Folder locker             lock / unlock with password
      8  Disk health               chkdsk
      9  RAM check                 Windows memory diagnostic
      A  Deep clean + repair       1 + 2 + 3 + 4

    OTHER
      B  Change color
      C  Change username
      D  Recommended apps
      E  Show log
      R  Run everything, then exit
      S  Restart winclean
      0  Exit

  select >
```

Every screen has the same framed header (version, screen name, your
username) and every operation ends with an optional rating prompt — press
**Enter** to skip it, nothing is sent anywhere, the rating is only written
to the local log. After the prompt you are back at the main menu; winclean
closes only when you press `0` (or after `R`, which is meant to quit).

---

## What each option does

### Cleaning

| Key | Option | What actually happens |
|---|---|---|
| `1` | Temp files | Deletes the contents of `%TEMP%`, `%SystemRoot%\Temp` and `%PUBLIC%\temp` (files with `del /f /q`, leftover directories with `rd /s /q`). Files that are in use are skipped, they are not an error. |
| `2` | Old Windows Update files | Stops `wuauserv` and `bits`, empties `SoftwareDistribution\Download` (the folder where every downloaded update copy lives), runs `Dism /Online /Cleanup-Image /StartComponentCleanup` to compact the component store, then restarts both services. |
| `3` | Disk cleanup | Writes `StateFlags0001 = 2` into **every** `VolumeCaches` profile in the registry, then runs `cleanmgr /sagerun:1`. This is the step most "disk cleanup" scripts forget — without the flags, `cleanmgr` silently does nothing. |
| `4` | Repair system files | `Dism /Online /Cleanup-Image /RestoreHealth` (re-fetches broken system files from Windows Update) followed by `sfc /scannow`. Expect 5–15 minutes. |
| `5` | Winget app updates | `winget upgrade --all --accept-package-agreements --accept-source-agreements`, so it never stops and waits for you to accept a licence. |
| `6` | Driver check / update | Counts the driver packages in the driver store (`pnputil /enum-drivers`), then offers: open **Windows Update** (where driver updates actually come from), or list every installed driver. |

### Tools

| Key | Option | What actually happens |
|---|---|---|
| `7` | Folder locker | Lock: hides the folder and applies an inherited `icacls /deny` for the `Everyone` SID, so nothing can be opened inside it. Unlock: asks for the password, removes the deny ACE and unhides. Refuses drive roots, `%SystemRoot%`, `System32`, `SysWOW64`, `%APPDATA%`, your user profile and winclean's own folder. |
| `8` | Disk health | Asks for a drive letter, then either checks it read-only (`chkdsk C:`) or checks and fixes (`chkdsk C: /F`). On `C:` Windows will offer to run the fix on the next boot. |
| `9` | RAM check | Starts the Windows Memory Diagnostic (`mdsched.exe`). Choose *Restart now* or *Schedule a restart* in the window it opens; the test runs at the next boot. |
| `A` | Deep clean + repair | Runs `1 + 2 + 3 + 4` back to back with a progress counter, then returns to the menu. |

### Other

| Key | Option | What actually happens |
|---|---|---|
| `B` | Change color | 15 presets (black background, one foreground color each). Saved, so it is used again on the next start. |
| `C` | Change username | Renames the greeting in the header. Saved. |
| `D` | Recommended apps | `winget install` for Java and LocalSend. Not sponsored, just a short useful list — edit the list in `:opt_apps` if you want other apps. |
| `E` | Show log | Pages through the all-time log. |
| `R` | Run everything, then exit | The five cleaning steps in order, then quits. This is the "set it and forget it" mode. |
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

* Options `2` and `4` are slow on purpose — DISM and SFC do real work.
  Leave the window open.
* Option `3` opens the real Disk Cleanup window and waits for it to finish.
* Option `1` cannot delete files currently in use by a running program.
* `R` runs everything including the slow steps, so expect 15–30 minutes.

## Version history

| Version | |
|---|---|
| **v4.4.1** | Fixed the app quitting right after the rating prompt: every option now hands control back to the main menu explicitly, instead of depending on call/return bookkeeping. |
| v4.4.0 | Rewritten around shared subroutines (964 → 580 lines). Menu moved to single-key `choice` input with grouped sections; boot no longer runs from `System32`; `cleanmgr` profiles are configured before `sagerun`; repair uses `DISM /RestoreHealth`; `mdsched` is called from `System32`; user input can no longer break the command line. Added the old Windows Update cleanup, a working folder locker and a real driver check. |
| v4.3.1.1.1.1b | Previous release. |
| older | See the [`older-versions`](https://github.com/adi20110908/winclean/tree/older-versions) branch. |
