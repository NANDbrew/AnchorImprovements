# Anchor Improvements

**Features**
- Anchors can now be picked up and set by hand. Still subject to rope length, so make sure you've let out enough line.
- Anchor winches now slow under tension like other winches
- Resizes the anchor and winch on the brig (this is purely visual, I thought they were too small)
- Adds text to winch showing how much rope is currently out.
- Fixes anchors rolling away downhill.
- Anchor winches to behave the same on all ships.
- Anchors can now set again after being pulled loose, instead of staying loose until lifted and dropped again.


**Options**
 - Simple physics
	- Anchor will stay set unless overpowered until boat is within 45 degrees of it, instead of releasing as soon as you start winching.
	- Anchor rope is now slightly stretchy to reduce the chance of jolts breaking the anchor loose.
	- Normal Sailwind rope length (50 yards).

 - Complex Physics
	- Holding power is now based on scope (angle from anchor to ship), resulting in more holding power at low angles and less at steep angles.
	- Extended rope to allow for realistic scope (150 yards).
	- Rope stretches more at low angles and long distances to imitate real scope.
	- Changed how anchor setting works. Anchor now sets more quickly when pulled than when stationary (it will still set with a slack line, just more slowly).
