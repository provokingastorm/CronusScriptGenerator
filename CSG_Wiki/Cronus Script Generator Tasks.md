Quick, un-prioritized, wishlist of changes to make. No guarantee of implementation.
## New Features
 - Add a different view from the input sequence text box that displays an Xbox 360 controller that the user can press instead of entering button input. 
 - [[Task - Implement Input Sequence Text Box|Sanitize button sequence input.]]
	 - Prevent the user from entering a comma without an input sequence between.
	 - Force the input sequence to be upper-case. 
	 - Add error handling red text label whenever a bad input is attempted by the user.
 - Read in a text file containing a button sequence.
 - Add ability to generate multiplayer boost scripts.
 - [[Task - Implement Fixed Repeat Sequences|Add numbered text box to repeat input sequence.]]
 - Add number input box to add script start delay.
 - Add button to reset the button sequences back to scratch.
 - Add an undo and a redo button.
 - Add a dropbox to select between a Xbox, PC and Playstation script.

## Bug Fixes
 - On last button sequence, don't add wait time.

## Configuration Changes
 - Add configuration option to change the input delay between buttons.
 - Add configuration option to change the default script start and script stop inputs. 
 - Add configuration option to change the header comment of the script.

## Visual Changes
- Add a human-readable script summary.
- Move "Generate Script" to the bottom of the UI.
- Move time delay text box and label below input sequence text box.