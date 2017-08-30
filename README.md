# RetroRoom
A Unity plugin that allow creation of 2D game using screensize rooms

For technical and easiness, 8/16bit era video game used sometime a world structure made from "room" that were the size of the screen (or a multiple of that) in which the camera is locked, and used transition from one to another (think Zelda/Metroid)

![zelda dungeon map](http://i.imgur.com/PR0bneW.png)
_note how everything fit into a grid of 8:7 square (the snes resolution)_

That plugin allow the creation of such game throught : 

- A fixed resolution setting, that allow to define the base resolution of your game
- a Room manager that allow to create rooms (that can be of multiple "screen" size, e.g a 2x3 room)
- A 2D camera script that will setup an orthographic camera & its screen rect so that pixel perfection is respected with maximum level of zoom (rest of screen is padded with black for now, support for border decoration possible later). That script also handle keeping the camera in room bounds & have functions to transition for one room to another when gameplay ask for it (e.g. when character enter a trigger at edge of the room etc...)

