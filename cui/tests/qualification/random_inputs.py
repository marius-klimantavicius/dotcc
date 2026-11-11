"""Recorded libc random inputs, keyed by original source call site.

C permits either evaluation order for the rand calls on the two sides of a
mutation assignment. Call-site queues preserve the actual mutation/drag inputs
even when the native compiler chooses a different evaluation order.
"""
import json
from pathlib import Path
import re


def expected_groups(text):
    """Inventory the selected upstream entrypoint, including newly added cases."""
    start = re.search(r"(?m)^int (?:main|cui_qualification_cui_tests)\(", text)
    if not start:
        raise RuntimeError("Missing upstream test_main entry")
    names = re.findall(r"(?m)^    (test_\w+)\(\);$", text[start.start():])
    if not names or len(set(names)) != len(names):
        raise RuntimeError("Missing or duplicate upstream test groups")
    return names


def adapt(text):
    lines = text.splitlines(keepends=True)
    sites = []
    for number, line in enumerate(lines, 1):
        ordinal = [0]
        def replace(match):
            index = ordinal[0]
            ordinal[0] += 1
            sites.append(dict(line=number, site=index))
            return f"cui_qualification_rand({number},{index})"
        line = re.sub(r"\brand\(\)", replace, line)
        lines[number-1] = re.sub(r"\bsrand\(", "cui_qualification_srand(", line)
    if len(sites) != 13:
        raise RuntimeError(f"Expected 13 upstream random call sites, got {len(sites)}")
    text = "".join(lines)
    anchor = "static int g_failed, g_checks;"
    if text.count(anchor) != 1:
        raise RuntimeError("Missing upstream test_main counter anchor")
    text = text.replace(anchor, "int cui_qualification_rand(int line,int site);\nvoid cui_qualification_srand(unsigned seed);\n" + anchor)
    start = re.search(r"(?m)^int (?:main|cui_qualification_cui_tests)\(", text)
    if not start:
        raise RuntimeError("Missing upstream test_main entry")
    body = text[start.start():]
    body = re.sub(r"(?m)^(    )(test_\w+)\(\);$", r'\1\2(); printf("QCOUNT \2 %d\\n",g_checks);', body)
    return text[:start.start()] + body, sites


def overrides(path):
    rows = []
    for name, result, parameters, target in (
        ("cui_qualification_rand", "int", ["int", "int"], "Next"),
        ("cui_qualification_srand", "void", ["unsigned int"], "Seed")):
        rows.append(dict(name=name, signature=dict(returnType=result, parameterTypes=parameters),
                         target=dict(kind="managedMethod", method="global::Managed.Forms.QualificationRandom." + target)))
    path.write_text(json.dumps(dict(version=1, functionOverrides=rows)))


def configure(directory):
    (directory / "QualificationRandom.cs").write_text('''using System;
using System.IO;
namespace Managed.Forms;
internal static class QualificationRandom {
    private static uint seed=1;
    private static BinaryWriter? trace;
    public static void Seed(uint value) { seed=value; Cui.Libc.srand(value); }
    public static int Next(int line,int site) {
        int value=Cui.Libc.rand();
        string? path=Environment.GetEnvironmentVariable("CUI_QUALIFICATION_RANDOM_TRACE");
        if(!string.IsNullOrEmpty(path)) {
            trace ??= new BinaryWriter(File.Create(path));
            trace.Write(seed); trace.Write(line); trace.Write(site); trace.Write(value); trace.Flush();
        }
        return value;
    }
}
''')


def groups(text):
    return [(name,int(count)) for name,count in re.findall(r"QCOUNT (test_\w+) (\d+)",text)]
