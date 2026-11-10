# Managed GLFW source

This is the supplied managed port of GLFW, copied into this campaign so a
consumer needs no sibling checkout. It is an altered C# implementation, not
the original GLFW distribution. The GLFW license is in `LICENSE.md`.

The source snapshot is recorded in `../../config/glfw-source.json`. It includes
the Windows, X11 and explicitly selected null platforms, plus their context
implementations. Wayland and Cocoa connectors are not implemented. A null
platform test does not establish working desktop windows or graphics.

The project inherits this repository's .NET target. Platform selection takes
place at runtime in `platform.cs`. No native GLFW library is required by this
port; its operating-system and graphics dependencies still apply.
