"""Summarise decompiled Entity::save functions into a Markdown table (read-only over artifacts/decomp/entity_save).
For each class, in first-occurrence order: serialised sub-objects (X::save / operator<<), labelled fields (TagWriter
strings) and raw writes (bytes). Usage: python -I entity_save_summary.py <decomp dir> <entity_hierarchy.txt> <out.md>
"""
import re, sys, os

src, hier, out = sys.argv[1:]
parent, cls_re = {}, re.compile(r"^(\s*)([A-Z]\w+) \(")
stack = []
for line in open(hier, encoding="utf-8"):
    m = cls_re.match(line)
    if not m:
        continue
    depth = len(m.group(1)) // 2
    stack[depth:] = [m.group(2)]
    parent[m.group(2)] = stack[depth - 1] if depth else None

CALL = re.compile(r"([A-Za-z_][\w<>,]*?)__(save\w*|operator<<<[^>(]+>?|saveSpaceOptimized<[^(]+>|saveUnitNumber)\(")
TAG = re.compile(r"\"([a-z][a-z0-9]*(?:-[a-z0-9]+)+|[a-z]{3,})\"")  # kebab-case labels used by TagWriter
WRITE = re.compile(r"\+ 0x50\) \+ 8\)\)\s*\([^;]*?,\s*(?:0x)?([0-9a-f]+)\)\s*;")


def uniq(xs):
    seen, r = set(), []
    for x in xs:
        if x not in seen:
            seen.add(x); r.append(x)
    return r


def ancestors(c):
    while parent.get(c):
        c = parent[c]; yield c


code_by_cls = {}  # a class can have 2 overloads (map save + another); merge them
for fn in sorted(os.listdir(src)):
    if fn.endswith(".c") and not fn.startswith("ApplicationVersion"):
        cls = fn.split("__save@")[0]
        code_by_cls[cls] = code_by_cls.get(cls, "") + open(os.path.join(src, fn), encoding="utf-8", errors="replace").read()
rows = []
for cls, code in sorted(code_by_cls.items()):
    calls = uniq(f"{a}::{b}" for a, b in CALL.findall(code) if not (a == cls and b == "save"))
    anc = {f"{a}::save" for a in ancestors(cls)}
    base = [c.split("::")[0] for c in calls if c in anc]
    subs = [c.replace("MapSerialiser::operator<<<", "<<").replace("Serialiser::", "") for c in calls if c not in anc]
    tags = uniq(TAG.findall(code))
    nraw = len(WRITE.findall(code))
    b = "nunca gravada (aborta)" if "logAndAbortOrThrow" in code and not calls else ("✔ " + base[0] if base else "—")
    rows.append((cls, parent.get(cls) or "—", b, ", ".join(subs) or "—", ", ".join(tags) or "—", nraw))
assert len(rows) >= 130, len(rows)
with open(out, "w", encoding="utf-8") as f:
    f.write("# Formato de `save()` por classe de entidade (gerado)\n\n")
    f.write("Gerado por `scripts/entity_save_summary.py` a partir de `artifacts/decomp/entity_save/*.c` (Ghidra, sem tipos). "
            "**Base** = `save` do antepassado que é chamado primeiro (formato = antepassado + campos abaixo). **Sub-objetos** = serializadores chamados, por ordem de 1.ª ocorrência. "
            "**Campos etiquetados** = nomes `TagWriter` (só gravados como texto em modo diagnóstico; indicam o campo seguinte). **Escritas diretas** = nº de escritas brutas (u8/u16/u32/u64/double).\n\n")
    f.write("| Classe | Mãe | Base | Sub-objetos serializados | Campos etiquetados | Escritas diretas |\n|---|---|---|---|---|---:|\n")
    for r in rows:
        f.write("| " + " | ".join(str(x).replace("|", "\\|") for x in r) + " |\n")
print(len(rows), "classes ->", out)
