# Youtube GameBar Overlay
![Youtube GameBar Logo](https://github.com/MarconiGRF/YoutubeGameBarOverlay/blob/master/Assets/SplashScreen.scale-200.png)  
An Extension developed on top of Windows' Xbox Game Bar SDK, aimed to be used as a pinned window while playing, making it possible to enjoy the video while gaming. 

## Setting up
* 1: Clone this repository.  
* 2: Clone the [VideoUI](https://github.com/MarconiGRF/YoutubeGameBarVideoUI) repository, it is the core of videos playback on this project.  
* 3: Build the VideoUI (Instructions on its readme), copy the content of `YoutubeGameBarVideoUI/dist/YoutubeGameBarVideoUI` inside `YoutubeGameBarOverlay/VideoUI`.  
* 4: Rename the .env_sample to .env and update the necessary environment variables, which currently are:  
    *  YTGBSS_ADDRESS (The IP of a running [YTGBSS](https://github.com/MarconiGRF/YoutubeGameBarSearchServer) instance.)
    *  YTGBO_SOURCE_MAIL_ADDRESS (The port of the previous YTGBSS instance.)
    *  YTGBO_DESTINATION_MAIL_ADDRESS (The destination e-mails where the feedbacks will go)
    *  YTGBO_SMTP_SERVER_ADDRESS (The SMTP server address to be used to send feedback e-mails)
    *  YTGBO_SMTP_USER (The SMTP Server's user login)
    *  YTGBO_SMTP_PASSWORD (The SMTP Server's user password)
* 5: Open the `YoutubeGameBarOverlay.sln` on Visual Studio.  
* 6: Add the VideoUI files to the project
    * On VS' Team explorer, right click `VideoUI` Folder, then `Add...` > `Existing Item...`
    * Select all files inside VideoUI/
* 7: Update/install the NuGet Packages.  
* 8: You're ready to contribute!

## Development tips
### Debugging with Game Bar
Once that this overlay is intended to be used on Xbox Game Bar, and you're probably going to need debugging it inside Game Bar, the application should not be launched as a normal UWP App:  

* 1: With the solution opened, right-click the project `YoutubeGameBarOverlay (Universal Windows)` and go to `Properties` and then `Debug`.  
* 2: Check the `Do not launch, but debug my code when it starts` option.  
* 3: Save the Project Properties.  
* 4: Use the Visual Studio's `Build` Menu Bar option, then select `Deploy Solution.`  
* 5: Once the process of deploy is succeed, press F5 to start debugging on Visual Studio (notice that it will **not** start the Overlay automatically).  
* 6: Now go ahead to Xbox Game Bar (Windows + G), using the `Widget Menu` option select `Youtube GameBar Overlay`.  
* 7: Pin the window and go back to Visual Studio.  
* 8: Since step 7 Visual Studio should have already attached to the Overlay's process and you are now able to debug it properly.

Everytime you change the code you'll need to redeploy to debug (Steps 4-7).