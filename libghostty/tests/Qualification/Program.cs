using Core = Managed.GhosttyVt.Native.GhosttyVt;

// Tests are in a separate nondefault profile; the scalar delivered library
// contains no test entrypoints or fixed transcripts.
var oracle = Core.dotcc_oracle_main();
if (oracle != 0) return oracle;
var boundary = Core.dotcc_boundary_main();
if (boundary != 0) return boundary;
return Core.dotcc_abi_main();
