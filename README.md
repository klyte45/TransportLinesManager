# CSMod-TLM - Transport Lines Manager Mod

#IMPORTANT: THIS IS A ALMOST COMPLETE VERSION TO WORK WITH THE VERSION 1.7 OF THE GAME.

Last Steam description below: (v5.6.1)

#Works with Natural Distasters update (1.6.*)

-----------------------------------------------------------------------------------------------------------------------------
#If you don't downloaded this mod from Steam Workshop, I can't help you. 

I can't ensure the downloaded version is the same from here.
-----------------------------------------------------------------------------------------------------------------------------
#NOTE ABOUT IMPROVED PUBLIC TRANSPORT [IPT] 

This mod still compatible with IPT in version 5.0.0, and the compatibility mode will not be affected as I thought before. So, still compatible with IPT.
The asset selection and others functions are incompatible with IPT.
-----------------------------------------------------------------------------------------------------------------------------

A shortcut to manage all city's public transports lines. And now it add additional line categories!

In 3.0, we already could change the name, the number and the color of the line, also see its statics direct on this panel. Now is possible categorize the lines for a better passenger transport otimization.

#How does this work

The tabs of lines detail window now have a different form for each line type:

- Hexagons are regular buses lines
- Circles are train lines
- Squares are metro/subway lines
- Diamonds are ship lines. (Since 4.3)
- Trapezes are tram lines. (Snowfall/Since 4.5)
- Pentagons are airplane lines. (Since 5.1)

And more tabs are added:

- The asterisk tab contains all configurations about the prefixes (prices, budgets, models...)
- The factory icon tabs are tabs which contains lists for configure the vehicle depots - here you can set which line group (prefix) will be spawned in each depot/station.

#IPT Overridden Functions:
- You can choose which vehicle models will be spawned in each line (by line prefix in the asterisk tab; since 5.0)
- You can see where in the line are each vehicle in a graphic view, after accessing the line detail menu (since 4.2)
- You can select how many vehicles will be used in a line. (in the line detail view; since 5.1)
- You can set an multiplier for certain lines budget  (by line prefix in asterisk tab; Since 5.2)
- You can set the ticket price from 0.05 up to 40 C  (by line prefix in asterisk tab; since 5.3)
  (Game defaults: Bus= 1; Tram, Train, Metro = 2; Ship = 5; Airplane = 10)

#So, this mod overrides all main IPT functions. See Important Notes below for the AVO)

#AND MORE!
- Since 5.5, you can set the budget by time of the day, in 3 hours length groups!
- You can see all integrations of lines in each station in the linear view - in the line detail panel;
- Since 5.4 you can edit stop names directly from linear view, just click on the labels and set its name. Works for buses and trams too!
- You can see which lines serves around a building by clicking on it - with shortcuts for the line detail view;
- You can set the internal number of lines and format how it's shown as you want too.
- You can modify many line at once putting they in the same prefix and editing its prefix options on asterisk menu (includes budget, fares and models)
- TLM have an more friendly graphical view of the city lines, in the linear view or exporting a map with the city lines (this last function is in alpha)

#Languages
- English
- Portuguese
- Korean - by Toothless http://steamcommunity.com/id/lshst 
- German- by weoiss http://steamcommunity.com/profiles/76561198067363272 

#NEW AT 5.6

Added support for select metro models for each prefix.

#NEW AT 5.6.1
 NEW Added german support (thanks weoiss!)

 Fixed Lines with 0% budget are seen by the cims as enabled - now they will be disabled (temporary, if using per hour budget, and reactivated when the budget is raised again)
  NOTE: This fix allows create express lines in the rush hours!

 Fixed Unprefixed lines numbering form is respected now.


#Important Notes
- Due the overriding done in the Tram, Ship, Bus and Passenger Train system, the option of enable/disable vehicles from these categories inside the Advanced Vehicle Options is useless for public city lines. The configuration in the TLM override it. But all other functions, like coloring, set capacity and more of AVO are still working. The model selection for external vehicles (like regional passenger ships and trains) still workin in AVO and alike mods.
- Due the overriding done in the Tram, Ship, Bus and Passenger Train system, the IPT enqueue for these categories will fail: the model of the bus enqueued could not be the selected in IPT. Use the compatibility mode for IPT to avoid it.
- No new incompatibilities found, except the listed above.


#Known Bugs
- The auto naming in the line creation was disabled for a while. It wasn't working properly...
- Rename depots are not working in the TLM listing and in depot details windows.
- Bus stations and terminals are wrongly listed as depots.

See the complete changelog at http://steamcommunity.com/sharedfiles/filedetails/changelog/408875519

#(Alpha) Transport map exporter
When active, a Draw Map! button will appear mod options menu. When clicked, the game generates a map with the metro and train lines like the linear map style. 
The html files with the map can be found at Transport Lines Manager folder inside game root folder (steamapps/common/Cities Skylines).

#Next steps:
- Develop the transport map exporter
- Improve interface (suggestions are welcome)

#Other notes
- Since 4.0, this mod uses the Sebastian Schöner's Detour code. (https://github.com/sschoener/cities-skylines-detour)

#Reported incompatible mods:
- Slow Speed (TLM 1.1.1 > *)
- Service Vehicle Selector (TLM 5.0+)

If you like my work, you can help me https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=U9EM9Z4YXEMTC making a donation
