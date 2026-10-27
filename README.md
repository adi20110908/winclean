# Winclean

Winclean is a portable Windows maintenance app with a searchable installed-app manager and a set of built-in Windows cleanup and system tools. It is distributed as a single `.exe`; there is no installer.

Latest version: **v5.3.1**

[Download Winclean.exe](https://github.com/adi20110908/winclean/releases/latest) · [Browse the source](https://github.com/adi20110908/winclean)

## Interface

Winclean opens on **Maintenance**. **Tools** is the other top-level tab; the app manager is not a separate main page. Its three sections are always available in one consistent tab bar:

- **System tools** — Windows troubleshooting, repair and information tools.
- **App manager** — searchable installed applications, uninstall and optional reviewed leftovers cleanup.
- **Preferences** — accent-color chooser, display name, local rating and restart action.

The interface, app list and custom dialogs use a dark theme. The accent chooser shows color swatches for the 15 named colors from the original batch version. Display name and accent are saved for the current Windows user under `%LOCALAPPDATA%\Winclean`.

## App manager

The **Tools → App manager** section lists registered desktop apps and removable Store apps for the current Windows account. Search by name, publisher or app type. Select an app to see its publisher, version, source and install location, then choose **Uninstall selected** or double-click its row.

- Every uninstall is confirmed first. Desktop apps launch their registered interactive `.exe` uninstaller; Windows Installer products use `msiexec /x`. Winclean strips common silent switches rather than intentionally requesting a silent uninstall.
- Store apps are removed only from the current account. Framework packages and protected Windows components are not offered for removal.
- Script/command interpreters, missing uninstallers and unsupported commands are blocked. Use **Windows Settings → Apps → Installed apps** for those entries.
- After uninstall, Winclean refreshes the inventory. A separate optional deep-clean scan is offered only if the app no longer appears.

### Deep-clean review

The scan looks for likely app-specific leftovers: a Store package-data directory, an exact app-name directory under known AppData roots, a conservatively matched install folder, the app's uninstall registration and an exact-name registry settings key. **Nothing is selected automatically.** Review each full path; app data may contain personal files and registry names may match unrelated software. Cancel the review to leave everything untouched.

- Selected folders are moved to the Windows Recycle Bin. Folders containing junctions or symbolic links are excluded.
- Registry keys are exported before removal to `%LOCALAPPDATA%\Winclean\Backups`. If export fails, the key is not removed. Local-machine changes may prompt for administrator approval.
- Matching is conservative and cannot identify every vendor-specific leftover or guarantee complete removal.

## Maintenance

The **Maintenance** page contains these actions:

- **Temporary files** — clean current-user, Windows and Public temp locations. Files in use are skipped; linked folders are not traversed. Windows/Public locations may request administrator approval. Disk analysis reports all three temp locations and the Recycle Bin as read-only sizes.
- **Browser cache** — clears cache folders for Chrome, Edge and Firefox. Cookies, passwords, history and bookmarks are untouched. Close browsers first to avoid active cache files.
- **Windows Update downloads** — requests UAC approval, stops update services that were running, clears downloaded updates and the Delivery Optimization cache, runs DISM component cleanup, then attempts to restart those services.
- **Disk Cleanup** — prepares the Windows Disk Cleanup profile while excluding the Recycle Bin, previous Windows installations, ESD setup files, user file versions and Downloads; then runs `cleanmgr`.
- **Recycle Bin** — prompts before permanently emptying it. This cannot be undone.
- **Disk analysis** — reports sizes for user/system/public temp, Recycle Bin, Windows Update downloads, Delivery Optimization, Windows Error Reporting and common browser caches without deleting anything.
- **Component cleanup** — runs DISM `/StartComponentCleanup`; this is not system repair.
- **System file repair** (under **Tools → System tools**) — runs DISM `/RestoreHealth`, then SFC `/scannow` if DISM succeeds.
- **Winget app updates** — runs `winget upgrade --all` with package/source agreement flags.
- **Recommended apps** — offers LocalSend or Oracle Java Runtime Environment through `winget`.
- **Deep clean + repair** — confirmation-gated sequence of temp cleanup, Windows Update cleanup, Disk Cleanup and system repair.
- **Run everything, then exit** — the same sequence plus winget app updates, then closes Winclean after success (as in the batch menu). A failed step stops the sequence and leaves Winclean open.

Long operations ask for confirmation and stop at the first failed step. Windows utilities may open their own windows or display UAC prompts.

## Tools → System tools

- **Disk health** — choose a drive and run a read-only CHKDSK report, or explicitly opt in to `/F` repair (may require a restart).
- **Folder locker** — applies a reversible Windows ACL deny rule to a selected personal folder. A salted PBKDF2 password verifier and the original access-control descriptor are stored locally so Winclean can restore the permissions after the password check. This is not encryption; an administrator can still access the data. Keep a separate backup and keep Winclean's lock metadata intact until the folder is unlocked.
- **Memory diagnostic** — opens Windows Memory Diagnostic; testing requires a restart.
- **Restore point** — asks Windows to create a restore point, subject to System Protection settings and Windows limits.
- **System information** — shows Windows, CPU, memory and disk details.
- **Network repair** — choose Flush DNS, Reset Winsock or Do both. Winsock reset requires a separate warning, can affect VPN/security network filters and usually requires a restart.
- **Drivers** — open Windows Update, list driver packages or export a driver backup to `Desktop\winclean-drivers`.
- **Activity log** — view locally recorded actions and open the log directory.

Winclean uses Windows' built-in utilities. Administrator approval is requested only for operations that need it. Read each confirmation and UAC prompt before continuing.

## Preferences, data and privacy

- **Accent color** — 15 batch-compatible color choices; saved locally.
- **Display name** — changes the welcome label; saved locally.
- **Rate Winclean** — records the score in the local activity log only. It is never transmitted.
- **Restart Winclean** — closes and starts the app again when no operation is active.

Winclean does not send telemetry or create an account. It reads installed-app information from Windows uninstall registry entries and the current account's Store package list. Preferences, activity logs, folder-lock metadata and ACL backups are kept under `%LOCALAPPDATA%\Winclean`. Deleting the folder-lock metadata while a folder is locked can prevent restoration of its original permissions.

## Download and use

1. Download `Winclean.exe` from the [latest GitHub Release](https://github.com/adi20110908/winclean/releases/latest).
2. Run it — no installer, service or files beside the executable are required.
3. Start with **Maintenance** or open **Tools → App manager** to manage installed apps. Inside Tools, **System tools**, **App manager** and **Preferences** are always available in the same tab bar.

## Build from source

Winclean is written in C# with Windows Forms and built using the Windows .NET Framework C# compiler. No external packages are needed.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
```

The script creates `dist\Winclean.exe`. GUI source: [`src/Winclean.cs`](src/Winclean.cs). The original command-line implementation remains available in [`winclean.bat`](winclean.bat).

## Open source

Winclean is released under the [MIT License](LICENSE). Contributions and issue reports are welcome.

## Version history

| Version | Changes |
|---|---|
| **v5.3.1** | Fixed a startup crash in dark-theme application and verified the Maintenance/Tools navigation, app manager, and all 15 accent swatches on Windows. |
| v5.2.0 | Reorganized the dark-themed interface with Maintenance and Tools; restored preferences and batch-era tools. |
| v5.1.0 | Added the portable Windows GUI as the primary app: searchable installed-app list, confirmed interactive uninstall, opt-in reviewed leftovers cleanup with registry exports and Recycle Bin recovery, and Cleanup/Tools tabs. |
| v4.5.0 | Added disk analysis, browser cache cleanup, Recycle Bin handling, network/restore/system information tools and driver backup to the batch utility. |
| v4.4.1 | Fixed returning to the main menu after the rating prompt. |
| v4.4.0 | Reworked the batch utility into shared subroutines and a grouped single-key menu. |
| Older versions | See the [`older-versions` branch](https://github.com/adi20110908/winclean/tree/older-versions). |
