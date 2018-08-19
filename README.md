# Synthy
An experimental synthesizer in unity, definetly not optimized, but it works.
Contributions and imporovements are welcome!

Video example: https://streamable.com/ehrll
![Example](https://media.discordapp.net/attachments/85593628650504192/480146029484965898/unknown.png)

### Requirements
Unity 2018.2.3f1

### Packages
Incremental compiler 0.0.42

### Example usage
To play a track file
1. Create empty game object
2. Add Player to it
3. Assign the Track file
4. Toggle play when in play mode

To play single notes
1. Create empty game object
2. Add Synthesizer to it
3. Enable keyboard input

To play a single note from code, the Synthesizer component has a `Play(note, duration)` method.
