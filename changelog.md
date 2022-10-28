# Changelog

v1.1.0.0
 - added: ffmpeg and some other stuff for testing live TV in this app
 - moved: to .Net4.8
 - fixed: many bugs
 - changed: Methode to local password encryption (DVBViewer)

v1.0.5.5
 - fixed: Recordings, lose the last selected series if Button "Get all Recordings" is pressed a snd. time.
 - added: Recordings, added buttons for refresh the series and the channels. It exist a issue, 
   after one of this buttons is pressed a second time: The selected series or channel is lost. 
   I dont know why, it could be a bug in .Net.

v1.0.5.4
 - Updated all async methodes to run in the correct context

v1.0.5.3
 - fixed crash on editing series of a recording and series is null or becomes null
 - fixed crash on editing channel of a recording and channel is null or becomes null
 - fixed must do a second click on a menuitem after first settings are saved
 - added null checks for series and channels
 - added changelog.md for this projekt