Title: iCanScript Release Notes
Author: Michel Launier
Version: v2.0.5
CSS: Github.css


#  iCanScript Release Notes [Back to top]

<!-- PDF: <<[front-matter/pdf.md] -->
<!-- HTML: <<[front-matter/html.md] -->
![](images/iCanScript-logo_512x512.png)

## iCanScript v2.0.7 ##

_April 20, 2015_

V2.0.7 resolves two major issue introduced in release v2.0.6.  Sorry for the inconvenience.

### List of Changes

- **<span style="color: red">\[FIXED\]</span>** Enumeration null exception on data upgrade of visual script.
- **<span style="color: red">\[FIXED\]</span>** Invalid error generated on _get_ accessor of fields.

* * *

## iCanScript v2.0.6 ##

_April 20, 2015_

V2.0.6 is a development release of iCanScript2.

### List of Changes

- **<span style="color: blue">\[IMPROVED\]</span>** Add the _'Self'_ port on all function nodes.
- **<span style="color: red">\[FIXED\]</span>** Disabled nodes are now displayed with half intensity in editor mode.


* * *

## iCanScript v2.0.5 ##

_April 19, 2015_

V2.0.5 is a development release of iCanScript2.

### List of Changes

- **<span style="color: blue">\[IMPROVED\]</span>** Automatically generated type-cast now uses the most specialized type instead of the common base type.
- **<span style="color: red">\[FIXED\]</span>** Fix code generation when multiple enables exists on a single node.


* * *

## iCanScript v2.0.4 ##

_April 17, 2015_

V2.0.4 is a development release of iCanScript2.

### List of Changes

- **<span style="color: blue">\[IMPROVED\]</span>** Performance of panning canvas.
- **<span style="color: blue">\[IMPROVED\]</span>** Improve name of trigger port in generated code.
- **<span style="color: blue">\[IMPROVED\]</span>** Restructuring of the dynamic menu for future support of variable & nested type creation.
- **<span style="color: blue">\[IMPROVED\]</span>** Remove illegal option to add _enable/trigger_ ports on a Unity event handler.
- **<span style="color: blue">\[IMPROVED\]</span>** Remove unused menu item _'+ Iterator'_ from contextual menu.
- **<span style="color: red">\[FIXED\]</span>** Fix code generation when _Target_ port is connected to a package input port that is initialized to _Owner_.


* * *

## iCanScript v2.0.3 ##

_April 16, 2015_

V2.0.3 is a development release of iCanScript2.

V2.0.3 introduces the concept of the script _Owner_ which is used to configure input ports to designate the game object on which the script is installed.


### List of Changes

- **<span style="color: green">\[NEW\]</span>** Default _Target_ port value to script _Owner_ if not connected and of type _Transform_ or _GameObject_.
-  **<span style="color: blue">\[IMPROVED\]</span>** Prefix all constructors with the keyword _New_ in the library.
- **<span style="color: red">\[FIXED\]</span>** Opening the port/node editor now requires a double click.
- **<span style="color: red">\[FIXED\]</span>** Remove generation of extra function parameters when input port is connected to a variable defined outside of the function.
- **<span style="color: red">\[FIXED\]</span>** Generate upcast when the _Target_ port when needed.

* * *


## iCanScript v2.0.2 ##

_April 15, 2015_

V2.0.2 is a development release of iCanScript2.

### List of Changes

- **<span style="color: green">\[NEW\]</span>** Error display when a function node cannot find its runtime code. 
- **<span style="color: blue">\[IMPROVED\]</span>** In the contextual menu, rename menu item _**"+ Out Instance Port"**_ to _**"+ Add Self Port"**_.
- **<span style="color: blue">\[IMPROVED\]</span>** In the contextual menu, rename menu item _**"+ Enable Port"**_ to _**"+ Add Enable Port"**_.
- **<span style="color: blue">\[IMPROVED\]</span>** In the contextual menu, rename menu item _**"+ Trigger Port"**_ to _**"+ Add Trigger Port"**_.
- **<span style="color: red">\[FIXED\]</span>** Prepend namespace to generated type names when type exists in more than one namespace.

* * *

## iCanScript v2.0.1 ##
 _April 14, 2015_

V2.0.1 is a development release of iCanScript2.

### List of Changes
- **<span style="color: red">\[FIXED\]</span>** Generation of the _"using"_ directive is now dependent on the content of the visual script.
- **<span style="color: blue">\[IMPROVED\]</span>** Addition of the Unity _"Awake()"_ event handler in the dynamic menu.

* * *

## iCanScript v2.0.0 ##
 _March 20, 2015_

V2.0.0 is the initial development release of iCanScript2.
