**Want to see what I'm working on?**<br />
Visit the <a href='https://trello.com/#board/multiple-clipboards/4f2d46929b7c4ed7365b0b15'>Project Road Map</a>.
<br />
**Questions? Comments? Suggestions?**<br />
Post them on the <a href='http://groups.google.com/group/multiple-clipboards-discussion'>project's discussion group</a>.


&lt;hr/&gt;


<h2>Release Candidate coming in the foreseeable future</h2>
<p>In the meantime, check out the latest trunk build (rev 167).  I have spent a lot of time making sure that the application works well in Windows 8, as well as porting the source code over to Visual Studio 2012 Express, and creating a new installer using the WiX Toolset.  As far as I know the build is stable, but I have not had enough time to adequately test it, so I do not feel comfortable calling this beta 3.  But, if you want Win 8 support then this is the way to go!</p>


&lt;hr/&gt;


<h2>Multiple Clipboards 2.0 Beta has been released!</h2>
<p>At this point the application is stable, all major features have been implemented and tested, and all known bugs have been fixed.  Features may still be added before the final release.</p>
<h4>Release Notes</h4>
<ul>
<li>Lots of bug fixes.  See road map complete list.</li>
<li>Improved installer</li>
<li>Dramatically improved user feedback</li>
</ul>
<h4>Quick Start Guide</h4>
<ol>
<li>Download and run the <a href='http://multiple-clipboards.googlecode.com/files/MultipleClipboardsInstaller-2.0-beta2.msi'>installer</a></li>
<li>Follow the instructions in the install wizard</li>
<li>That's it!  The application will launch when the installer is done.</li>
</ol>
<h4>Known Issues</h4>
<p>On some computers with a 64 bit version of windows installed the main application window may take a very long time (10-20s) to appear.  This is caused by the 64 bit .NET assembly cache being out of date.  To fix this problem open up a command prompt, navigate to "C:\Windows\Microsoft.NET\Framework64\v4.0.30319", and run "NGEN update".  For more details about this problem see <a href='http://connect.microsoft.com/VisualStudio/feedback/details/725108/very-slow-start-of-64bit-wpf-applications-after-windows-update-security-fix-of-net4'>this article</a>.</p>


&lt;hr/&gt;


<h2>Multiple Clipboards 2.0 Alpha has been released!</h2>
<p>This is an alpha release of the application.  It is functional, but not yet complete.</p>
<h4>Release Notes</h4>
<ul>
<li>Brand new user interface</li>
<li>All features of version 1 of the app remain</li>
<li>All clipboard formats are now preserved.  If you copy HTML it will paste as HTML.</li>
<li>The cut operation now works on all data types</li>
<li>No more duplicate entries in the clipboard history (for real this time, updated 01/26/2012)</li>
<li>New Clipboard Inspector tab that shows the current contents of all clipboards</li>
</ul>
<h4>Quick Start Guide</h4>
<ol>
<li>Download and run the <a href='http://multiple-clipboards.googlecode.com/files/MultipleClipboardsInstaller-2.0-alpha2.msi'>installer</a></li>
<li>Follow the instructions in the install wizard</li>
<li>Launch the application from the shortcut placed in Start -> All Programs -> Multiple Clipboards</li>
<li>Configure additional clipboards and settings as you see fit.  All changes take effect immediately.</li>
<li>Close the main window when done.  The application will continue to run silently in the system tray.</li>
<li>Right clicking on the tray icon will show you all the recent clipboard history.  Clicking an item from this menu will automatically place it on the system clipboard.</li>
</ol>


&lt;hr/&gt;


<h2><u>Version 2 on the way!</u></h2>
<p>The project lives!  After a couple years of using this software, and learning a great deal about software development in general, I decided it was time for a significant overhaul.  Version 2 has been in the works for a few months now and is functional at this point, although nowhere near complete.  I have re-structured the subversion repository and added the trunk for version 2 for anyone who is interested.</p>
<h3>What's Next?</h3>
<p>Soon I will post an alpha version for those who are interested.  Then I need to finish the product.  Being a side project, this will be done when it's done, and not before.  After that, who knows!  Eventually I want to turn this into a more traditional open source project and see where people take it.</p>


&lt;hr/&gt;


<h2><u>About Multiple Clipboards v1</u></h2>
<p>I wrote this application to make my life easier.  I often found myself in a situation where I needed to copy and paste two different blocks of text multiple times.  This resulted in a lot of unwanted window switching, confusion, mistakes, and wasted time.  I often thought that things would be much easier if Windows had multiple clipboards to work with.  Now it does!</p>
<h2><u>Installation</u></h2>
<p>To install Multiple Clipboards just download and extract the Multiple Clipbaords zip file, run setup.exe, and follow the instructions.  The installer will place a shortcut to Multiple Clipboards in the Startup folder on your start menu so the program will run automatically when you start your computer.  There is no need to reboot your machine after the installation, but you will need to manually click the shortcut to launch the application the first time after installing.</p>
<h2><u>Basic Usage</u></h2>
<p>Use the main program interface to assign whatever hotkeys you want to up to 10 additional clipboards.  By default there will be one additional clipboard with the following hotkeys:</p>
<table cellpadding='0' border='0' cellspacing='0'>
<blockquote><tr>
<blockquote><td>Cut</td>
<td>-</td>
<td>Windows + X</td>
</blockquote></tr>
<tr>
<blockquote><td>Copy</td>
<td>-</td>
<td>Windows + C</td>
</blockquote></tr>
<tr>
<blockquote><td>Paste</td>
<td>-</td>
<td>Windows + V</td>
</blockquote></tr>
</table>
<p>In addition to providing additional clipboards Multiple Clipboards is also a fully featured clipboard history manager.  By default it will store the previous 20 items that have been placed on any clipboard, including the default Windows clipboard.  The number of historical items that the program keeps track of can be set on the settings tab of the Multiple Clipboards dialog.</p>
<p>Historical clipboard items can be retrieved in two ways:<br>
<ol>
<blockquote><li>Right click on the Multiple Clipboards icon in the system tray and expand the Clipboard History menu item.  Click the item that you wish to use and it will be automaticall placed on the default Windows clipboard.</li>
<li>Right click on the Multiple Clipboards icon in the system tray, expand the Clipboard History menu item, and click "View Detailed History".  This will display a grid of all the previous clipboard items.  Select the clipboard that you wish to place the data on from the dropdown.  Then select the item that you wish to use in the grid by clicking on the row.  Finally, click the "Place Selected Row on Selected Clipboard" button.</li>
</blockquote></ol>
</p>
<h2><u>Error Handling</u></h2>
<p>I highly doubt that you will get the program to crash.  Critical errors that will prevent the application from working at all will give you a message box and a quick description.  All errors will log detailed information about the error, including the stack trace, to a file "errorLog.txt" in the common application data folder.  There is a shortcut to this file on the applications tray icon right click menu.  If you encounter strange problems, or a clipboard operation simply not working, check the log.</p>
<h2><u>Updates / Support / Source Code</u></h2>
<p>Like I said, I wrote this program for me.  It is provided as is with absolutely no guarantee.  The complete source code as well as any updated versions of the program can be in the downloads section of this page.</p>
<h2><u>Known Bugs</u></h2>
<ul>
<li>The "cut" operation only works correctly for text</li>
<li>The additional clipboard treats all text as plain text.  That is, if you copy a section of a website and paste it into Word you will lose all the formatting.  Use this to your advantage.  Ever try to copy code from a website into Visual Studio?  Annoying!</li>
</ul>