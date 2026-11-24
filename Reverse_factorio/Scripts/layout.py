"""Print a type's members from artifacts/pdb/pdb_layouts.txt with hex offsets. Usage: python -I layout.py <layouts> <Type> [Type...]"""
import re, sys
want = set(sys.argv[2:]); cur = None
for line in open(sys.argv[1], encoding="utf-8"):
    if line.startswith("== "):
        m = re.match(r"== \w+ (.+) size=(\d+)", line); cur = m and m.group(1) in want and m.group(1)
        if cur: print(f"== {cur} size=0x{int(m.group(2)):x}")
    elif cur:
        m = re.match(r"\s+member \+(\d+)\s+(.*)", line)
        print(f"   +0x{int(m.group(1)):x}  {m.group(2)}" if m else line.rstrip())
