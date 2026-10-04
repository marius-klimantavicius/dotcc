"""Complete shared Managed.Runtime provider supersets without rewriting providers."""
import json
import subprocess

from .source_format import format_sources


def complete_runtime_partials(root, contracts: str, providers: dict[str, str]) -> dict[str, str]:
    # Reuse the SDK/source-identity cache and build lock. The empty formatting
    # request builds/locates the tool without processing the generated program.
    _, metadata = format_sources(root, {})
    tool = root / "artifacts/source-formatter" / metadata["sourceSha256"] / "lib/Edg.SourceFormatter.dll"
    completed = subprocess.run(
        ["dotnet", str(tool), "--complete-partials"],
        input=json.dumps({"contracts": contracts, "providers": providers}),
        capture_output=True, text=True,
    )
    if completed.returncode:
        raise RuntimeError("runtime partial completion failed:\n" + completed.stdout + completed.stderr)
    response = json.loads(completed.stdout)
    if response.get("diagnostics"):
        raise RuntimeError("runtime partial completion returned diagnostics: " + json.dumps(response["diagnostics"]))
    files = response["files"]
    for name, text in files.items():
        if isinstance(text, list) and all(isinstance(chunk, str) for chunk in text):
            files[name] = "".join(text)
        elif not isinstance(text, str):
            raise RuntimeError("runtime partial completion returned invalid source: " + name)
    return files
