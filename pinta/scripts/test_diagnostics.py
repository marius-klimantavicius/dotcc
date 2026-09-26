"""Keep process exit status distinct from the interpreter's PintaException."""
import signal


def failure_details(status, output):
    details = []
    if status < 0:
        try:
            name = signal.Signals(-status).name
        except ValueError:
            name = f"signal {-status}"
        details.append(f"Process terminated by {name} (exit {status}; not a Pinta error code)")
    for line in output.splitlines():
        if (line.endswith("  FAIL") or line.startswith("!    ") or
                ("Assertion" in line and "failed" in line)):
            details.append(line)
    if status and not details:
        details.append(f"Process exit {status}; no Pinta error diagnostic was emitted (see full log)")
    return details


def print_details(details, prefix="  "):
    for line in details:
        print(prefix + line, flush=True)
