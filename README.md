MeowBit
=======
###[Censorship-Resistant Web for Windows](http://meowbit.com/press-release/)
* Website: http://meowbit.com
* Source: https://github.com/Derrick-/MeowBit

### v0.5 : April 1, 2014

## Description
MeowBit is Free Software for Windows with the goal to allow you to effortlessly view Dot-Bit websites as registered in the [Namecoin](http://namecoin.info) blockchain.

[Dot-Bit](http://meowbit.com/what-is-dot-bit/) is a new top-level domain that is not controlled by any government or corporation.

Anyone can [register a Dot-Bit domain](http://meowbit.com/how-to-register-dot-bit-domains/) in minutes using the inexpensive cryptocurrency, [Namecoin](http://namecoin.info).

Dot-Bit domains are extremely resistant to being shut down or hijacked by governments, corporations or criminals.

With MeowBit you can navigate on any browser from the regular web to Dot-Bit sites and back, effortlessly and securely.

## Installation, Getting Help & Tips
* Requirements
 * Windows 7 or 8+ only. (See also: [XP Version](https://github.com/Derrick-/MeowBit/tree/xp))
 * .Net 4.5
 * Namecoin Wallet software running. [Get it here](http://namecoin.com).

* Installation
 * Run MeowBitSetup.exe
 * Ensure Namecoin wallet is installed and running, and the blockchain is up-to-date.
 * Run MeowBit for the first time to configure the service and connection to the namecoin client.
 * After installation, MeowBit may display a button that says "Install Service", "Set Auto Start", or "Start Service".
 * Once MeowBit is running and all status indicators are green, you should be able to browse dot-bit sites.
 * You may need to restart your browser the first time you use MeowBit.
 * [List of working dot-bit websites](http://meowbit.com/list-of-working-dot-bit-websites/)

### Known Issues
 * You may not be able to view dot-bit sites via MeowBit when connected to a VPN service. Results vary, this will be addressed in a future version.

### Getting Help
 * Please leave a clear and detailed comment on our [help forum](http://meowbit.com/forums/) if you have any issues with any aspect of running MeowBit. We like bug reports.

### Tips
 * If you were viewing Dot-Bit domains before MeowBit using the clunky and insecure alternate-DNS servers workaround, change back to your old default DNS servers before installing MeowBit.
 * MeowBit resolves .bit domains using the namecoin blockchain on your PC. Namecoin must run long enough for the blockchain to be downloaded from other wallets on the network. There are prominent status indicators that show this progress.
 * MeowBit auto-starts at Windows start-up, this can be changed by removing the MeowBit shortcut from the start-up folder. Info for Windows 7 and 8 here: http://windows.microsoft.com/en-us/windows/run-program-automatically-windows-starts#1TC=windows-7
 * To work seamlessly, you also should configure the Namecoin wallet to start at start-up. This option is available in the Namecoin Wallet settings.
 * If you get a message that says “Namecoin config updated. restart wallet”, restart wallet and wait a few minutes.
 * If you are upgrading from a previous version, uninstall the old version first. Click on desktop kitty to open MeowBit monitor. Stop Service. Go into system tray. Right-click on kitty, exit MeowBit. Go to Windows Start / Control Panel, Programs and Features / click MeowBit to uninstall. Then you can install new version.
 
## Version History:
### v0.5 : April 1, 2014
 * Pre-Windows 8 local nameserver function now restores DNS to use DHCP if the IP Address is obtained via DHCP
 * Setup installs and starts dotBitNs service: More reliable then previous 'self-install' method.
 * Setup installs Start menu icon
 * Fixed System.Net.Http and other .Net framework version compatibility problems
 * Automatic configuration of namecoin.config file is less intrusive; preserves comments.
 * Namecoin config manager will search running processes to see if a custom data path is configured. <- Thanks Ian Freeman
  * Task tray toggle (double click) now works to show window when hidden via minimize
  * Settings window: Added some persistent settings:
   * Minimize To Tray
   * Minimize On Close
   * Start Minimized
   * Start On Login
 * Moved Version info to new tab in settings window
 * Remove StartUp shortcut from installer, it was being configured for all users, but new config option is per user.
  * In order to have MeowBit autostart, the user will have to explicitly set that in Settings window.
 * Start menu folder with Uninstall shortcut.
 * dotBitNs Service will now try to detect running Namecoin client and find authorization information from config file
 * Any configuration information that is found by the service is now saved so that it can connect to Namecoin client without the need for MeowBit monitor to be running.
  * Dot-Bit specification implemented: Empty sub-domain name ("") may be used to configure the domain. Entries specified here have lower precedence.

### v0.4 : March 14, 2014
 * Settings window with logging (dis/en)ablement, available from tool tray context menu
 * Open Log Folder, Latest Log File, or Copy Log contents buttons 
 * Sounds to verify log copy
 * Bug Fix: Resolution fail on unexpected domain info format.
 * Internal structural improvements of domain resolution
 * Fixed repeating Logging dis/enabled messages
 * Added README.md (this file)
 * Typo in log output "Determining"... Thanks MWD.
 * Query product info from blockchain using query api.
 * Setting page displays current version, and latest version from blockchain
 * Version info on Settings page offers link to download page if out of date.
 * Extended default timeout for Namecoin client API calls to 5 seconds
 * System status interval extended to 6 seconds (RpcConnectorTimeout * 1.2)
	
### v0.3 : March 7, 2014
 * Better windows management via tray icon
 * Add help sub menu with website links
 * Fix crashed caused by duplicate config keys
 * Unmapped sub-domains default to their parent.

### v0.2 : March 4, 2014
 * Initial Release

### Credits
#### MeowBit Team
 * [Michael W Dean](http://www.michaelwdean.com/) for product conception and mass marketing
 * [Derrick Slopey](https://github.com/Derrick-) for app design, programming and product conception

#### Thanks to
 * [Appamatto](https://bitcointalk.org/?topic=1790.0) and [Aaron Swartz](http://www.aaronsw.com/weblog/uncensor) for BitDNS and [SquareZooko](http://www.aaronsw.com/weblog/squarezooko) proposals.
 * Moxie Marlinspike and Mike Kazantsev for Convergence.
 * phelix and the Namecoin Marketing and Development Fund.
 * khal and vinced for namecoind.
 * [Jeremy Rand](http://veclabs.bit/): Convergence for Namecoin
