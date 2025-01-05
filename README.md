# Spawn Memory
A simple system for respawning the player at their last position. For VRchat/Udon. 

# Usage
Add the package to your Packages folder, or place the script and Udon program asset into your project. 

Then place the script onto a GameObject. 

You can adjust these options;

- **Save Interval**: How often, in seconds, the player position is saved. Due to the nature of Udon Persistence, saving the player position requires synchronising it to other users, so it is recommended to adjust this depending on network load. 

- **Save Time Limit Minutes**: How long the player position should be retained for. In other words, if the player leaves the world, their saved position will be forgotten after this interval. This is useful for hangout worlds, where players who momentarily disconnect will rejoin in the same place rather than at spawn, but people who last visited the world a few days ago won't find themselves somewhere unfamiliar. 

You can also see the debug values for the synched position, rotation, and last save time. 

When the player rejoins the world, the script checks their last save time against the current time, and moves them to the saved position if it's within the limit. 

# Notes
This script is provided as a reference for using persistence player objects. When making it, I ran into a bunch of gotchas that the final script covers and tries to make obvious. 

First, every player gets their own player object, and each one does the same script execution as any other, so scripting that only executes locally for each player needs to be gated by object ownership. 

Second, `OnPlayerRestored` is fired after the player has spawned in, so they'll see themselves spawn at the regular spawn. I recommend using this in combination with a system that blanks the player's view until `OnNetworkReady` is called. 

