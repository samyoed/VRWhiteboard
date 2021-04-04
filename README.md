## VRWhiteboard

# Whiteboard:

    For the whiteboard, we've been implementing the "Paint in 3D" package available in the asset store.
    The made-from-scratch whiteboard prefab is in Assets/Prefabs, however it is significantly less performant
    and less polished compared to the current implementation however it still is an option.

    The current implementation of the whiteboard is in the 'Current Whiteboard' Game Object. Simply pick up one
    of the three markers located on the stand with your grip button and write on the whiteboard as if you would
    write on a real life whiteboard. The cylinder on the stand will be your eraser, you would also use it as you 
    would on a real whiteboard. 

    On the left side you will see some text on the top saying 'Current Board" with a number below it, as well as 
    some buttons. The 'Board 1', 'Board 2', ... buttons are for swapping between boards,sort of like a tabbing 
    system so you're able to switch between boards while being able to keep what you want on a board. So for 
    example, if you were to write on 'Board 1', and then click on the 'Board 2' button, the system would switch you
    to 'Board 2', showing 'Board 2' but hiding 'Board 1'. Then if you click the 'Board 1' button again, it will show
    the contents of 'Board 1'. Clicking the 'Clear Board' button will clear the current board, leaving the current
    board like how it started.

# Snapshot Camera & Input Manager:

    For taking pictures in the application. Instructions are attached to the camera as you pick it up. Pictures 
    are saved into memory. Location is in SnapshotCamera.cs

    SnapshotCamera.cs is where the camera is coded. The function OnPostRender() is where most of the work is done.

    All the non-standard inputs are done in InputManager.cs, documentation about non-standard inputs can be found
    here: https://docs.unity3d.com/2020.2/Documentation/Manual/xr_input.html

# Current Networking Implementation and Status:

    There is very basic multiplayer enabled at the moment where multiple people are able to log into the experience
    at the same time. Once in the experience, players should be able to see avatars that represent the other players.
    When a player moves, their position will be updated for everyone else as well, so users will be able to see basic
    synced movement across the experience, however, positional movements without teleporting (like walking around room-scale)
    will not be synced. Rigged player models have also not been mapped to the player's hands and bodies. The markers
    and eraser are synced as well and in theory, when someone draws something on the whiteboard, everyone else should
    also see what is drawn, however, due to inconsistencies in connectivity, this syncing of drawings is not particularly
    reliable. There is also the fact that once someone joins the room after the board is drawn on, the previous drawings
    will not be synced.

# Wrist Menu:

    The menu is a canvas stored on the left hand. Implemented in Menu.cs

    We've also tried to implement a gallery feature to view the snapshots captured by the camera, potentially also
    having additional uses such as file management. Here was the package we used: https://github.com/yasirkula/UnityNativeGallery
    However, the implementation was not fully successful. This feature was left on hold and implementation is also in 
    Menu.cs


# Google Drive Integration/Web Browser Integration Status:

    The API used is here for the Google Drive Integration: https://github.com/Elringus/UnityGoogleDrive

    The issue at the moment regarding the Google Drive integration was that every time we wanted to authenticate a Google
    account for use within the application, it would take us out of the experience and attempt to authenticate within the
    web browser. However, when it tries to do that, it first closes out of the experience so the web browser isn't able
    to send the information back to the web browser (this is what we were assuming the problem was) because the experience
    was able to work during testing within Unity itself. Our solution was to implement an in-app web browser and authenticate
    through that, however, after trying out a multitiude of web browsers, we weren't able to find any that were reliable enough
    for use in VR.

The overall scripts should be relatively straight forward to follow. If there are any questions regarding any specific scripts,
please email me at shl638@nyu.edu


