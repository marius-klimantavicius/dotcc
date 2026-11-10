# Native GLFW desktop control

`workflow.c` creates a hidden native GLFW/OpenGL CUI app, builds the managed
consumer's same label/button/checkbox/slider/input workload for four frames,
reads the actual GL framebuffer, quits and destroys the app. It rejects software
fallback. It links the matching native control only; managed products never load
that native library or native GLFW.

The recipe's `native` suite builds/runs this control after the nine selected
native CTest entries and the shaping oracle. When `consumer` runs in the same
invocation, all requested managed variants capture RGBA and compare against the
native image. The explicit GL tolerance is one channel value; all four observed
Linux variants were byte-identical: 1,228,800 RGBA bytes at 640×480, 33,023 pixels
differing from the corner background.

Run with the full qualification prerequisites and configured external work root:

```bash
LIBGL_ALWAYS_SOFTWARE=1 xvfb-run -a -s '-screen 0 1280x800x24 +extension GLX' \
  bash cui/scripts/test.sh --suite native --suite consumer --form all --mode all
```

The current native compile command uses the documented Linux GLFW development
toolchain. Actual Windows desktop/native-control qualification remains open.
