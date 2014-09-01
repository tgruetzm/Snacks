Snacks:
----------------------------
Snacks is a Kerbal Space Program mod that adds a simple life support system based on the resource snacks.  Go ahead stuff the cupola snack modules full, just be careful of Jeb he sometimes doesn't share.

Requirements:
----------------------------
Snacks requires the use of module manager v2.2.1.  The latest version is provided in the release package.

v0.3.1 Alpha Features:
----------------------------
-Fixed the bug where the supply window would show incorrect data after loading a new save file.

-The supply window is now updated after certain game events(eva, docking/undocking, game load).  This should result in the supply window allways being accurate for the current situation.

-Planets in the supply window are now sorted by their referenceBodyIndex, instead of showing up in odd orders at times. Ships under the body are sorted by their supply level percent.

v0.3 Alpha Features:
----------------------------
-Supply Window!  Now you can actually tell how low on snacks your Kerbals are across the solar system.  The window is visible in the flight and space center scenes.  The data is a cache from the last snack time, so if you undock/eva that vessel won't be in the list until the next snack time.

-The Supply Window shows all vessels with crew.  The vessels are listed by the body they are around.  Vessels with greater than 50% supply are shown in green, 50%-25% yellow and under 25% red.  Tool tip shows the number of crew and an estimated number of Kerbin days the supply might last.

-Reduced the number of snacks in a day to 1.  Now Kerbals eat on average every 6 hours.

-Added mealsPerDay to snacks.cfg.  Are your Kerbal's extra hungry?  well if you want you can feed them more or less depending on your tastes.

-Added 10 snacks to the Probodyne RoveMate.


v0.2 Alpha Features:
----------------------------
-Snacks are added as a resource to every module that can carry crew 50 snacks per capacity.

-Snack capacity of non-command modules is 200 snacks for each capacity. Some modules may have more or less depending on built in snack compartments, or lack of space!
The idea is that non-command modules are the main means to transport snacks. I expect two hitchhikers and a command module would support a crew of 3 on a manned round trip mission to duna.

-Kerbals randomly consume snacks on average every 3 hours, 1/4 snack each time. This averages to 1/2 snack per Kerbin day or 2 snacks per Earth day. You cannot predict with certainty how many will be required for a given mission. There are a few extra twists here as well. 
Some Kerbals, especially courageous ones may sneak an extra snack at snack time, others may forget to eat if their stupidity is to high.

-Kerbals on EVA take 1 snack with them. This should be enough for about 2 Kerbin days. Plan accordingly.

-If you deprive a Kerbal of snacks, the next time he attempts to eat snacks, your reputation will go down. Each subsequent time the Kerbal is deprived your reputation decreases.
Reputation decrease is based on your current reputation. If you are a reputable space agency, your reputaion will drop faster than a less reputable agency. The exact amount is configurable in snacks.cfg(repLossPercent). The formula is percent(.25% default) * number of hungry kerbals * current Reputation


