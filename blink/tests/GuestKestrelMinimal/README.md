# Minimal Kestrel middleware control

This ordinary ASP.NET Core service uses WebHostBuilder, Kestrel and one
middleware handler for health, echo and HTTP shutdown. It avoids minimal API
route-delegate construction while preserving real TCP request/response behavior.
The fixture explicitly allows 30 minutes for request headers: the initial
default-timeout translated attempt returned HTTP 408 during cold interpreted
work. This remains a finite application setting; execution keeps independent
instruction, lifetime, memory, storage, descriptor and thread bounds.
PublishAot/PublishReadyToRun remain false for the guest app. The host consumer
may be NativeAOT; every guest instruction still executes inside Blink.

The runner retains a new IL image and exact source/binary hashes, validates the
three endpoints under native Linux, then uses an already-built public Probe.
It is a separate control; original CoreClrRuntime service failures are retained
and not overwritten or counted as passing.
