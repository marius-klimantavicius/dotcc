"""Guarded fork libc declaration compatibility; algorithms stay upstream."""
from pathlib import Path
from campaigns.recipes import json_file


def prepare_input(ctx, source):
    policy = json_file(ctx.root / "config/libc-adaptations.json")
    original = Path(source) / policy["source"]
    text = original.read_text()
    for adaptation in policy["adaptations"]:
        if text.count(adaptation["before"]) != 1:
            raise RuntimeError("Fork libc adaptation requires one exact declaration context")
        text = text.replace(adaptation["before"], adaptation["after"], 1)
    # Only private authored inputs are written; acquired/configured trees remain intact.
    staged = ctx.work / "pglitec-source.c"
    staged.write_text('#line 1 "' + original.as_posix().replace('"', '\\"') + '"\n' + text)
    wrapper = ctx.work / "pglitec-input.c"
    wrapper.write_text('#include <stdbool.h>\n#include <stdlib.h>\n#include "' + staged.as_posix() + '"\n')
    ctx.receipt["pglite_libc_adaptations"] = {"source": str(original), "input": str(wrapper),
                                             "policy": "config/libc-adaptations.json",
                                             "adaptations": policy["adaptations"]}
    return wrapper
