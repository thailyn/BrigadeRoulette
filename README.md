Brigade Roulette
================
This simple program is a tool to be used with the Blood Brothers game on iOS
and Android.

One of the tasks in Blood Brothers is to put your familiars in a brigade.  A
brigade can have different formations, where each differs in how many
locations in the brigade are in each vertical position (here, called "front",
"center", and "rear").  There are benefits and costs to putting each familiar
in each vertical position (and thus to each brigade formation).

This program searches for the best brigade formation and vertical position for
the familiars you want to put into a brigade, given an estimate of each
familiar's win percent in each vertical position (how you determine those
numbers is up to you).  Only brigades with five positions are examined.

While these brigades are described as having five positions, they can support
up to twice that number, where the second set of five make up the "reserve",
the familiars who join a battle after the corresponding non-reserve familiar
has died (for Blood Clashes).

All permutations of familiar locations are examined, with the assumption that
the the first five familiars are non-reserve familiars and the following five
are reserve familiars.  The assumption here is that some familiars are better
suited to being in the reserve, while others are not, but this would have to
be determined outside of this program.

Input
=====
This program reads from a file called "input.csv" that must be located in the
same directory as the exe file.  If it is not present, the program will crash.
The file should be in CSV (comma-separated values) format, where each row in
the file represents one familiar.  The first column should be the name of the
familiar (this has no effect on the program aside from allowing you to keep
track of each familiar in the output).  The following columns are for the
familiar's win percentage in each vertical position: front, then center, then
rear.  Technically, any number of vertical positions are supported, ordered in
decreasing order according to that vertical position's damage multiplier.
However, since Blood Brothers has only three positions, more positions are
meaningless and will probably lead the program to work incorrectly (or maybe
ignore positions after the first three).

See the sample input.csv file in the root of the repo for an example of a
working (albeit meaningless) input file.  The win percentages therein were
graciously provided by Wolfram Alpha ("30 random numbers from 0 to 100").

Requirements
============
Brigade Roulette requires .NET 4.5 to run.  Additional libraries (e.g., Entity
Framework) are distributed with the executable.

This program uses an SQLite database file to get the available brigade
formations.  It must be located in a "schema" subfolder next to the exe file.
A suitable version of the file can be generated with the SQL files for the
Phlebotomist schema (which is transitively a dependency through the
Phlebotomist model .NET project), or is available as part of releases.
