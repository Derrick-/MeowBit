MeowBit
=======
[Censorship-Resistant Web for Windows](http://meowbit.com/press-release/)

http://meowbit.com
## Description
MeowBit is Free Software for Windows with the goal to allow you to effortlessly view Dot-Bit websites as registered in the [Namecoin](http://namecoin.info) blockchain.

[Dot-Bit](http://meowbit.com/what-is-dot-bit/) is a new top-level domain that is not controlled by any government or corporation.

Anyone can [register a Dot-Bit domain](http://meowbit.com/how-to-register-dot-bit-domains/) in minutes using the inexpensive cryptocurrency, [Namecoin](http://namecoin.info).

Dot-Bit domains are extremely resistant to being shut down or hijacked by governments, corporations or criminals.

With MeowBit you can navigate on any browser from the regular web to Dot-Bit sites and back, effortlessly and securely.

## Installation, Getting Help & Tips
* Requirements
 * Windows 7 or 8+ (for now)
 * .Net 4.5 (.net 4.0 release planned for Windows XP compatability)
 * Namecoin Wallet software running. [Get it here](http://namecoin.com).
 
* Installation
 * Run MeowBitSetup.exe
 * Ensure Namecoin wallet is installed and running, and the blockchain is up-to-date.
 * Run MeowBit for the first time to configure the service and connection to the namecoin client.
 * After installation, MeowBit may display a button that says "Install Service", "Set Auto Start", or "Start Service".
 * Once MeowBit is running and all status indicators are green, you should be able to browse dot-bit sites.
 * You may need to restart your browser the first time you use MeowBit.
 * [List of working dot-bit websites](http://meowbit.com/list-of-working-dot-bit-websites/)
 
* Getting Help
 * Please leave a clear and detailed comment on our [help forum](http://meowbit.com/forums/) if you have any issues with any aspect of running MeowBit. We like bug reports.

* Tips
 * If you were viewing Dot-Bit domains before MeowBit using the clunky and insecure alternate-DNS servers workaround, change back to your old default DNS servers before installing MeowBit.
 * MeowBit resolves .bit domains using the namecoin blockchain on your PC. Namecoin must run long enough for the blockchain to be downloaded from other wallets on the network. There are prominent status indiators that show this progress.
 * MeowBit auto-starts at Windows start-up, this can be changed using the start-up folder. Info for Windows 7 and 8 here: http://windows.microsoft.com/en-us/windows/run-program-automatically-windows-starts#1TC=windows-7
 * To work seamlessly, you also should ccnfigure the Namecoin wallet to start at start-up. This option is avalable in the Namecoin Wallet settings.
 * If you get a message that says “Namecoin config updated. restart wallet”, restart wallet and wait a few minutes.
 * If you are upgrading from a previous version, uninstall the old version first. Click on desktop kitty to open MeowBit monitor. Stop Service. Go into system tray. Right-click on kitty, exit MeowBit. Go to Windows Start / Control Panel, Programs and Features / click MeowBit to uninstall. Then you can install new version.
 
## Version history:
* Next Version:
 * Settings window with logging (dis/en)ablement, available from tool tray context menu
 * Open Log Folder, Latest Log File, or Copy Log contents buttons 
 * Sounds to verify log copy
 * Bug Fix: Resolution fail on unexpected domain info format.
 * Internal structural improvements of domain resolution
	
* v0.3.5179.694 : March 7, 2014
 * Better windows management via tray icon
 * Add help sub menu with website links
 * Fix crashed caused by duplicate config keys
 * Unmapped sub-domains default to their parent.

* v0.2.0.0 : March 4, 2014
  * Initial Release
