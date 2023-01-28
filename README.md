# Kugelblitz Version 2
## Mission Settings
  
### General Structure
It is possible to set the timer and quirks for multiple groups as follows:  
`[Kugelblitz]:<group>,<group>,<group>`  
  
### Group Structure
`<group>` in the structure above can be of the form `<pacing>;<preset>` or just `<preset>`. If the latter is used, the pacing will default to the value in the modsettings. This is 2.5 by default.  

#### Pacing
Pacing is the amount of time between pulses on the module during submission phase.
`<pacing>` can be between 0.25 and 10, with at most three digits after the decimal point.  

#### Preset
Preset is how many and which quirks may appear in a group.
`<preset>` is a group of seven characters out of `+?-` with optionally one or two digits before it.
- `+` means that quirk must be present if there are enough Kugelblitz modules available.
- `?` means that quirk can be present if there are Kugelblitz modules available to take that slot.
- `-` means that quirk will never appear in that groups.  
  
Quirks are in ROYGBIV order.

- No digits in front means the bounds for the group size are only limited by the which quirks may appear.
- One digit in front means the bounds for the group size are exactly that value.
- Two digits in front means the bounds for the group size are the two digits given as lower and upper bound respectively.

### Example
`[Kugelblitz]:4.2;4---+??+,28???----,1-------,2;???????` will generate the following, given the right number of Kugelblitz modules being present:
- A group of 4 Kugelblitz modules with a green quirk, a violet quirk and either the blue or the indigo quirk. This group pulses every 4.2 seconds during submission.
- A group of anywhere between 2 and 4 Kugelblitz modules, with choice between the red, orange and yellow quirks. This group pulses with whatever pacing is set by the player in modsettings. Note that it is not a group of up to 8 modules, because the set itself indicates that only 3 different quirks are allowed at most.
- A group of exactly one module, meaning no quirks. This group also uses the pacing set in modsettings.
- A group with complete freedom of quirk selection. During submission phase, the group will pulse every 2 seconds.
