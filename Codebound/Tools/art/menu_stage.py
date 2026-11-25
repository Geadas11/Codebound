# Codebound main-menu stage for Unity: the factory of menu_scene.py rebuilt as a real 3D scene that the game animates.
# Run inside Blender 5.x:
#   exec(open(r"<repo>/Codebound/Tools/art/menu_stage.py").read()); build_stage(); export_stage()
# Static geometry is merged per material (few draw calls); moving parts are exported separately with clear names, and
# empties named Mark_* tell the game where things are (camera poses, terminal screen corners, belt ends, arm bases).
import bpy, math, os
from mathutils import Vector

exec(open(os.path.join(os.path.dirname(__file__) if "__file__" in dir() else r"C:\Users\carlos\Desktop\Jogo_CODEFACT\Codebound\Tools\art", "menu_scene.py"), encoding="utf-8").read())

OUT = r"C:\Users\carlos\Desktop\Jogo_CODEFACT\Codebound\Assets\Codebound\Resources\MenuStage\menu_factory.fbx"
MOVING = []   # names of objects that must stay separate (animated in Unity)


def mark(name, loc):
    e = empty("Mark_" + name, loc)
    return e


def keep(o):
    MOVING.append(o.name)
    return o


# ------------------------------------------------------------------ parts the game animates
def belt_unity(name, p0, p1, z0, z1, kinds, housing=True, avoid_y=()):
    """Belt frame only: the game moves the items (Mark_Belt_<name>_A/B ends; kinds in the marker name)."""
    a = Vector((p0[0], p0[1], z0)); b = Vector((p1[0], p1[1], z1))
    d = b - a; L = d.length
    yaw = math.atan2(d.y, d.x); pitch = math.asin(max(-1, min(1, d.z / L)))
    rot = (0, -pitch, yaw)
    mid = (a + b) / 2
    side = Vector((-math.sin(yaw), math.cos(yaw), 0))
    keep(box("BeltSurface_" + name, tuple(mid + Vector((0, 0, 0.6))), (L, 1.0, 0.05), M["rubber"], 0.0, rot))
    for s in (-1, 1):
        box(name + "Rail", tuple(mid + side * 0.58 * s + Vector((0, 0, 0.66))), (L, 0.1, 0.24), M["paint"], 0.02, rot)
        box(name + "RailStripe", tuple(mid + side * 0.64 * s + Vector((0, 0, 0.66))), (L, 0.02, 0.12), M["hazard"], 0.0, rot)
    n = max(2, int(L / 2.2))
    for i in range(n + 1):
        p = a + d * (i / n)
        if any(abs(p.y - ay) < 1.0 for ay in avoid_y): continue   # no leg standing on the belt we cross
        h = p.z + 0.55
        for s in (-1, 1):
            box(name + "Leg", tuple(Vector((p.x, p.y, h / 2)) + side * 0.5 * s), (0.1, 0.1, h), M["dark"], 0)
    k = 0; t = 0.4
    while t < L - 0.3:   # rollers under the belt edge: the game turns them at belt speed
        p = a + d.normalized() * t
        r = keep(cyl("Roller_%s_%d" % (name, k), tuple(p + Vector((0, 0, 0.53))), 0.075, 1.08, M["steel"], rot=(math.radians(90), 0, yaw), verts=10, smooth=False))
        t += 1.1; k += 1
    mark("Belt_%s_A_%s" % (name, "-".join(kinds)), tuple(a + Vector((0, 0, 0.63))))
    mark("Belt_%s_B" % name, tuple(b + Vector((0, 0, 0.63))))
    if housing:
        u = d.normalized()
        ends = {"start": ((a, -1),), "end": ((b, 1),)}.get(housing, ((a, -1), (b, 1)))
        for p, face in ends:
            c = p + u * face * 0.6
            box(name + "Portal", tuple(c + Vector((0, 0, 0.95))), (1.6, 1.7, 1.9), M["paintDark"], 0.06, (0, 0, yaw))
            box(name + "PortalFrame", tuple(c - u * face * 0.81 + Vector((0, 0, 1.0))), (0.12, 1.5, 1.4), M["paint"], 0.02, (0, 0, yaw))
            box(name + "PortalLed", tuple(c - u * face * 0.83 + Vector((0, 0, 1.78))), (0.05, 0.9, 0.06), M["amber"], 0, (0, 0, yaw))


def arm_unity(name, loc, yaw_deg, phase):
    """Static base; the arm itself is assembled and animated by the game from the Proto_Arm* parts."""
    x, y, z = loc
    cyl(name + "Base", (x, y, 0.18), 0.6, 0.36, M["dark"], verts=24, bw=0.02)
    box(name + "BasePlate", (x, y, 0.02), (1.7, 1.7, 0.04), M["hazard"], 0)
    for k in range(4):   # anchor bolts
        a = k * math.pi / 2 + math.pi / 4
        cyl(name + "Bolt", (x + math.cos(a) * 0.72, y + math.sin(a) * 0.72, 0.06), 0.06, 0.08, M["steel"], verts=8)
    mark("Arm_%s_yaw%d_ph%d" % (name, yaw_deg, phase), (x, y, 0.36))


def assembler_extras(name, loc, s):
    """Two hydraulic presses on the front face (rods move in the game) and a status lamp the game recolours."""
    x, y, _ = loc
    for k, dx in enumerate((-1.05, 1.05)):
        cx, cy = x + dx * s, y - 2.2 * s
        cyl(name + "PistonBody", (cx, cy, 1.0 * s), 0.17 * s, 0.9 * s, M["dark"], verts=16, bw=0.01)
        cyl(name + "PistonCap", (cx, cy, 1.47 * s), 0.2 * s, 0.06 * s, M["paint"], verts=16)
        box(name + "PistonBracket", (cx, cy + 0.18 * s, 2.45 * s), (0.3 * s, 0.36 * s, 0.12 * s), M["steel"], 0.01)
        rod = cyl("PistonRod_%s_%d" % (name, k), (cx, cy, 1.95 * s), 0.07 * s, 0.9 * s, M["steel"], verts=16)
        origin_to(rod, (cx, cy, 1.5 * s)); keep(rod)
    lamp = box("Status_" + name, (x, y - 2.06 * s, 2.28 * s), (0.62 * s, 0.06, 0.14 * s), M["status"], 0)
    keep(lamp)


def furnace_extras(name, loc, s):
    x, y, _ = loc
    keep(box("Status_" + name, (x, y - 1.82 * s, 2.86 * s), (0.5 * s, 0.05, 0.1 * s), M["status"], 0, (math.radians(-12), 0, 0)))
    mark("Mouth_" + name, (x, y - 1.55 * s, 1.15 * s))


def origin_to(o, p):
    """Moves the object origin to p without moving its geometry (the joint the game rotates about)."""
    mw = o.matrix_world.copy()
    off = mw.inverted() @ Vector(p)
    o.data.transform(__import__("mathutils").Matrix.Translation(-off))
    o.location = p


def arm_prototypes():
    """One of each arm part, origin at its joint, standing along +Z; parked far away (the game clones them)."""
    X = 300.0
    yel = M["armYellow"]
    parts = [
        ("Proto_ArmTurret", lambda: cyl("t", (X, 0, 0.2), 0.45, 0.4, yel, verts=24, bw=0.02), (X, 0, 0)),
        ("Proto_ArmTurretCap", lambda: cyl("t", (X, 4, 0.44), 0.32, 0.1, M["dark"], verts=24), (X, 4, 0)),
        ("Proto_ArmJoint", lambda: cyl("t", (X, 8, 0), 0.26, 0.62, M["dark"], rot=(0, math.radians(90), 0), verts=24, bw=0.02), (X, 8, 0)),
        ("Proto_ArmUpper", lambda: box("t", (X, 12, 0.85), (0.32, 0.32, 1.7), yel, 0.06), (X, 12, 0)),
        ("Proto_ArmFore", lambda: box("t", (X, 16, 0.7), (0.24, 0.24, 1.4), yel, 0.05), (X, 16, 0)),
        ("Proto_ArmWrist", lambda: cyl("t", (X, 20, 0.08), 0.17, 0.22, M["dark"], verts=16), (X, 20, 0)),
        ("Proto_ArmFinger", lambda: box("t", (X, 24, 0.18), (0.06, 0.2, 0.36), M["steel"], 0.01), (X, 24, 0)),
        ("Proto_ArmLed", lambda: box("t", (X, 28, 0.0), (0.08, 0.03, 0.06), M["amber"], 0), (X, 28, 0)),
        ("Proto_ArmCable", lambda: cyl("t", (X, 32, 0.8), 0.04, 1.6, M["rubber"], verts=8), (X, 32, 0)),
    ]
    for name, make, pivot in parts:
        o = make(); o.name = name
        origin_to(o, pivot)
        keep(o)


def item_prototypes():
    X = 320.0
    crate = box("Proto_Item_crate", (X, 0, 0.25), (0.5, 0.5, 0.5), M["crate"], 0.03)
    band = box("CrateBand", (X, 0, 0.25), (0.52, 0.52, 0.08), M["dark"], 0)
    bpy.ops.object.select_all(action="DESELECT"); band.select_set(True); crate.select_set(True); C.view_layer.objects.active = crate
    bpy.ops.object.convert(target="MESH"); bpy.ops.object.join()
    crate = C.view_layer.objects.active; crate.name = "Proto_Item_crate"
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=0.26, location=(X, 4, 0.2))
    ore = C.active_object; obj_mat(ore, M["ore"]); ore.scale = (1, 0.8, 0.7); ore.name = "Proto_Item_ore"
    bpy.ops.object.transform_apply(scale=True)
    plate = box("Proto_Item_plate", (X, 8, 0.04), (0.6, 0.45, 0.08), M["plate"], 0.01)
    coil = cyl("Proto_Item_coil", (X, 12, 0.22), 0.22, 0.36, M["coil"], rot=(math.radians(90), 0, 0), verts=24)
    for o, piv in ((crate, (X, 0, 0)), (ore, (X, 4, 0)), (plate, (X, 8, 0)), (coil, (X, 12, 0))):
        origin_to(o, piv); keep(o)


# ------------------------------------------------------------------ the terminal station (navigation hub)
def terminal_station(loc, yaw_deg):
    """A raised control station: console desk with keys and dials, a heavy framed 16:9 monitor under a visor,
    side cabinets with status lights, cables into the floor. The game draws on Mark_Screen_* (the glass)."""
    x, y, _ = loc
    yaw = math.radians(yaw_deg)
    R = __import__("mathutils").Matrix.Rotation(yaw, 4, "Z")

    def at(lx, ly, lz):   # local (right, forward-to-viewer is -y, up) → world
        v = R @ Vector((lx, ly, lz))
        return (x + v.x, y + v.y, v.z)

    def rot(rx=0.0):
        return (math.radians(rx), 0, yaw)

    # raised plinth with hazard edge and a step
    box("TermPlinth", at(0, 0.2, 0.15), (4.6, 3.2, 0.3), M["dark"], 0.04, rot())
    box("TermPlinthEdge", at(0, -1.42, 0.16), (4.6, 0.16, 0.3), M["hazard"], 0.0, rot())
    box("TermStep", at(0, -1.75, 0.07), (2.2, 0.5, 0.14), M["grate"], 0.01, rot())
    # desk: lower cabinet and an angled control deck
    box("TermDesk", at(0, -0.25, 0.72), (3.0, 1.0, 0.85), M["paintDark"], 0.05, rot())
    box("TermDeskTrim", at(0, -0.76, 0.98), (3.0, 0.04, 0.08), M["paint"], 0.01, rot())
    box("TermDeck", at(0, -0.45, 1.2), (3.1, 0.95, 0.07), M["graphite"], 0.02, rot(-14))
    for r in range(3):   # keyboard: rows of keys
        for c in range(14):
            box("TermKey", at(-0.92 + c * 0.13, -0.62 + r * 0.12, 1.19 + r * 0.03), (0.1, 0.09, 0.03), M["dark"], 0.005, rot(-14))
    for k in range(4):   # dials and buttons
        cyl("TermDial", at(0.98 + (k % 2) * 0.22, -0.6 + (k // 2) * 0.2, 1.22 + (k // 2) * 0.05), 0.07, 0.06, M["steel"], rot(-14 + 90 - 90), verts=16)
    for k, mat in enumerate((M["green"], M["amber"], M["red"])):
        box("TermButton", at(1.42, -0.62 + k * 0.12, 1.2 + k * 0.03), (0.08, 0.08, 0.04), mat, 0.005, rot(-14))
    # monitor: heavy frame, bezel bolts, visor, status light bar, swing arms
    mz = 2.15
    box("TermMonitorBack", at(0, 0.22, mz), (2.5, 0.28, 1.55), M["dark"], 0.06, rot(-6))
    box("TermBezel", at(0, 0.06, mz), (2.42, 0.08, 1.47), M["paintDark"], 0.04, rot(-6))
    keep(box("TerminalScreen", at(0, 0.015, mz), (2.16, 0.02, 1.215), M["screen"], 0.0, rot(-6)))
    for sx in (-1.12, 1.12):
        for sz in (-0.66, 0.66):
            cyl("TermBolt", at(sx, 0.01, mz + sz), 0.03, 0.03, M["steel"], rot(-6 + 90), verts=8)
    box("TermVisor", at(0, -0.2, mz + 0.86), (2.62, 0.6, 0.08), M["paint"], 0.02, rot(16))
    keep(box("TermLightBar", at(0, -0.42, mz + 0.82), (2.2, 0.05, 0.05), M["amber"], 0, rot(16)))
    for sx in (-1.32, 1.32):
        box("TermArm", at(sx, 0.25, mz - 0.1), (0.14, 0.14, 1.9), M["steel"], 0.02, rot())
        box("TermArmFoot", at(sx, 0.25, 1.2), (0.3, 0.3, 0.12), M["dark"], 0.02, rot())
    # side cabinets with status LEDs and vents
    for side in (-1, 1):
        cx = side * 2.0
        box("TermCabinet", at(cx, 0.35, 1.15), (0.75, 0.9, 1.7), M["dark"], 0.04, rot())
        box("TermCabinetTop", at(cx, 0.35, 2.03), (0.8, 0.95, 0.06), M["paint"], 0.01, rot())
        for k in range(5):
            led = box("TermCabLed", at(cx, -0.11, 1.75 - k * 0.16), (0.42, 0.02, 0.05), M["cyan"] if (k + side) % 3 else M["amber"], 0, rot())
            keep(led); led.name = "Blink_Led_%d_%d" % (side + 1, k)
        for k in range(6):
            box("TermVent", at(cx, -0.11, 0.95 - k * 0.08), (0.5, 0.02, 0.03), M["graphite"], 0, rot())
    # cables from the back into the floor
    for k, off in enumerate((-0.6, 0.0, 0.6)):
        tube("TermCable", [at(off, 0.4, mz - 0.4), at(off * 1.2, 1.1, 1.0), at(off * 1.4, 1.6, 0.05)], 0.045, M["rubber"])
    # where the game reads the screen and the two camera poses
    for nm, lx, lz in (("TL", -1.08, 0.6075), ("TR", 1.08, 0.6075), ("BL", -1.08, -0.6075), ("BR", 1.08, -0.6075)):
        tz = mz + lz * math.cos(math.radians(-6))
        ty = 0.0 - lz * math.sin(math.radians(-6))
        mark("Screen_" + nm, at(lx, ty - 0.005, tz))
    mark("Cam_Terminal", at(0, -1.75, mz + 0.06))
    mark("Cam_Terminal_Look", at(0, 0.0, mz))
    light("SPOT", "TermKeyLight", at(0, -2.4, 4.6), 900, (1.0, 0.72, 0.45), 0.4, rot=(math.radians(38), 0, yaw), spot=48)


def cargo(name, loc, yaw_deg, size=(1.6, 1.1, 0.9)):
    """Industrial cargo case: ribs, orange end caps, a hazard band (foreground depth)."""
    x, y, z = loc
    r = (0, 0, math.radians(yaw_deg))
    w, d, h = size
    box(name, (x, y, h / 2 + z), (w, d, h), M["paintDark"], 0.05, r)
    for s in (-1, 1):
        ox, oy = math.cos(r[2]) * s * (w / 2 - 0.12), math.sin(r[2]) * s * (w / 2 - 0.12)
        box(name + "Cap", (x + ox, y + oy, h / 2 + z), (0.22, d + 0.04, h + 0.04), M["paint"], 0.03, r)
    for k in (-0.25, 0, 0.25):
        box(name + "Rib", (x + math.cos(r[2]) * k * w, y + math.sin(r[2]) * k * w, h / 2 + z), (0.06, d + 0.03, h - 0.1), M["dark"], 0.01, r)
    box(name + "Band", (x, y, h * 0.8 + z), (w * 0.6, d + 0.02, 0.08), M["hazard"], 0, r)


def emblem(loc, size, facing_deg):
    """The Codebound emblem on the right wall: a gear ring around a 'C' with a code chevron."""
    x, y, z = loc
    cu = bpy.data.curves.new("EmblemCurve", "CURVE"); cu.dimensions = "2D"; cu.fill_mode = "BOTH"; cu.extrude = 0.08
    def poly(pts):
        sp = cu.splines.new("POLY"); sp.points.add(len(pts) - 1)
        for i, (px, py) in enumerate(pts): sp.points[i].co = (px, py, 0, 1)
        sp.use_cyclic_u = True
    teeth = 12; outer = []
    for i in range(teeth * 4):
        a = i / (teeth * 4) * math.tau
        rr = 1.0 if (i % 4) in (1, 2) else 0.84
        outer.append((math.cos(a) * rr, math.sin(a) * rr))
    poly(outer)
    poly([(math.cos(a) * 0.68, math.sin(a) * 0.68) for a in [i / 48 * math.tau for i in range(48)]])   # hole
    # the C: a thick arc open to the right
    arc = []
    for i in range(33):
        a = math.radians(40 + i * (280 / 32)); arc.append((math.cos(a) * 0.55, math.sin(a) * 0.55))
    for i in range(33):
        a = math.radians(320 - i * (280 / 32)); arc.append((math.cos(a) * 0.32, math.sin(a) * 0.32))
    poly(arc)
    o = bpy.data.objects.new("WallEmblem", cu); C.collection.objects.link(o); o.data.materials.append(M["paint"])
    o.scale = (size, size, size); o.location = (x, y, z)
    o.rotation_euler = (math.radians(90), 0, math.radians(facing_deg))
    # backing plate and a chevron '>' as the code mark
    box("EmblemPlate", (x + 0.08, y, z), (0.06, size * 2.5, size * 2.5), M["graphite"], 0.04, (0, 0, 0))


def wall_details():
    """The right wall: ribbed panels, a sector sign, conduit, the emblem with its own light."""
    for k in range(10):
        box("WallRib", (19.6, -2 + k * 4.0, 6.0), (0.25, 0.3, 12.0), M["dark"], 0.02)
    for k in range(5):
        box("WallPanel", (19.62, 0 + k * 8.0, 2.2), (0.12, 3.6, 3.4), M["paintDark"], 0.03)
        box("WallPanelStripe", (19.55, 0 + k * 8.0, 0.45), (0.06, 3.6, 0.3), M["hazard"], 0)
    emblem((19.5, 6.0, 7.5), 2.0, -90)
    text("SectorSign", "SECTOR 03 // SMELTING", (19.45, 1.5, 4.6), (math.radians(90), 0, math.radians(-90)), 0.55, M["steel"], align="CENTER")
    light("SPOT", "EmblemLight", (16.5, 6.0, 11.5), 2200, (1.0, 0.68, 0.38), 0.3, rot=(math.radians(-50), 0, math.radians(-90)), spot=45)
    tube("WallConduit", [(19.4, -6, 9.8), (19.4, 20, 9.8)], 0.12, M["steel"])


def build_stage():
    clear()
    materials()
    M["armYellow"] = mat_metal("ArmYellow", (0.92, 0.62, 0.06), 0.42, 0.3)
    hall()
    wall_details()
    assembler_mk2("AsmHero", (9.6, 8.6, 0), 1.25, "A-03", 0)
    assembler_mk2("AsmB", (-7.6, 11.0, 0), 0.95, "A-02", 9)
    assembler_mk2("AsmFar", (-4.0, 38, 0), 1.0, "A-07", 17)
    for nm in ("AsmHero", "AsmB", "AsmFar"):
        for part in ("Fan", "RingPivot", "Beacon"):
            o = bpy.data.objects.get(nm + part)
            if o: keep(o)
    for nm in ("AsmHeroFan", "AsmBFan", "AsmFarFan", "AsmHeroRingPivot", "AsmBRingPivot", "AsmFarRingPivot"):
        o = bpy.data.objects.get(nm)
        if o:
            for ch in o.children: keep(ch)
    M["status"] = mat_emit("StatusLamp", (1.0, 1.0, 1.0), 6.0)
    assembler_extras("AsmHero", (9.6, 8.6, 0), 1.25)
    assembler_extras("AsmB", (-7.6, 11.0, 0), 0.95)
    assembler_extras("AsmFar", (-4.0, 38, 0), 1.0)
    furnace("FurnA", (-1.2, 22, 0), 1.0, 1)
    furnace("FurnB", (3.4, 24, 0), 1.0, 2)
    furnace("FurnC", (8.0, 26, 0), 1.0, 3)
    for nm, lc in (("FurnA", (-1.2, 22, 0)), ("FurnB", (3.4, 24, 0)), ("FurnC", (8.0, 26, 0))):
        furnace_extras(nm, lc, 1.0)
    terminal_station((3.2, 0.9, 0), -26)
    belt_unity("Front", (-17, 3.6), (6.8, 7.0), 0.0, 0.0, ("crate", "ore"), housing="start")
    belt_unity("Mid", (-12, 17.0), (11, 17.0), 0.0, 0.0, ("plate", "coil"))
    belt_unity("High", (-18, 30.0), (16, 30.0), 3.6, 3.6, ("crate", "plate"), housing="end")
    belt_unity("Incline", (-1.2, 8.8), (-1.2, 29.3), 0.0, 3.6, ("ore", "coil"), housing=False, avoid_y=(17.0, 30.0))
    arm_unity("ArmA", (-2.1, 7.7, 0), 0, 0)
    arm_unity("ArmB", (-3.5, 14.4, 0), 0, 0)
    arm_unity("ArmC", (3.4, 19.8, 0), 0, 0)
    tanks((15.0, 42, 0))
    mezzanine()
    crane()
    for nm in ("Trolley", "CraneSweepPivot", "CraneSweep"):
        o = bpy.data.objects.get(nm)
        if o: keep(o)
    for ch in bpy.data.objects["Trolley"].children_recursive: keep(ch)
    mark("Crane_A", (-3.0, 16, 10.3)); mark("Crane_B", (3.0, 16, 10.3))
    # foreground depth: cargo at the left and right of the overview
    cargo("CargoL1", (-3.6, -1.6, 0), 12)
    cargo("CargoL2", (-3.3, -1.5, 0.9), 18, (1.2, 0.9, 0.7))
    cargo("CargoR1", (7.2, -0.6, 0), -20, (1.8, 1.2, 1.0))
    arm_prototypes()
    item_prototypes()
    # the overview the menu starts from
    mark("Cam_Overview", (-2.2, -5.6, 2.1))
    mark("Cam_Overview_Look", (3.6, 9.0, 1.7))
    camera()
    cam = bpy.data.objects["MenuCam"]
    cam.location = (-2.2, -5.6, 2.1)
    d = Vector((3.6, 9.0, 1.7)) - Vector((-2.2, -5.6, 2.1))
    cam.rotation_euler = d.to_track_quat("-Z", "Y").to_euler()
    world()
    for n in ("HeroKey", "HeroBack"):   # re-aim the hero lights at the new hero position
        o = bpy.data.objects.get(n)
    for o in bpy.data.objects:   # labels and signs become meshes
        if o.type == "FONT":
            bpy.ops.object.select_all(action="DESELECT"); o.select_set(True); C.view_layer.objects.active = o
            bpy.ops.object.convert(target="MESH")


def export_stage(path=OUT):
    """Merges static meshes per material (modifiers applied), gives them 1 m box-projected UVs, exports FBX."""
    bpy.ops.object.select_all(action="DESELECT")
    moving = set(MOVING)
    for o in list(bpy.data.objects):
        if o.type in ("CURVE", "FONT") and o.name not in moving:
            o.select_set(True)
    if C.selected_objects:
        C.view_layer.objects.active = C.selected_objects[0]
        bpy.ops.object.convert(target="MESH")
    # hanging cables: pivot at the ceiling end, kept apart (they sway); pipes kept apart (they hum)
    for o in list(bpy.data.objects):
        if o.name in moving or o.type not in ("MESH",): continue
        if o.name.endswith("Cable") and o.name.startswith("Asm") or o.name.startswith("MezzPipe"):
            top = max((o.matrix_world @ v.co for v in o.data.vertices), key=lambda v: v.z)
            bpy.ops.object.select_all(action="DESELECT"); o.select_set(True); C.view_layer.objects.active = o
            if o.modifiers: bpy.ops.object.convert(target="MESH")
            origin_to(o, top if o.name.endswith("Cable") else o.matrix_world @ o.data.vertices[0].co)
            o.name = "Sway_" + o.name; moving.add(o.name); MOVING.append(o.name)
    # whole machines, belts and the station: one object each (multi-material) so the game can shake them
    for prefix in ("AsmHero", "AsmB", "AsmFar", "FurnA", "FurnB", "FurnC", "Front", "Mid", "High", "Incline", "Term", "ArmA", "ArmB", "ArmC"):
        parts = [o for o in bpy.data.objects if o.type == "MESH" and o.name.startswith(prefix) and o.name not in moving
                 and not o.name.startswith(("Proto_", "BeltSurface_", "Roller_", "PistonRod_", "Status_", "Sway_"))
                 and not (o.parent is not None and o.parent.name in moving)]
        if not parts: continue
        bpy.ops.object.select_all(action="DESELECT")
        for o in parts:
            if o.animation_data: o.animation_data_clear()
            o.select_set(True)
        C.view_layer.objects.active = parts[0]
        bpy.ops.object.convert(target="MESH")
        bpy.ops.object.join()
        j = C.view_layer.objects.active
        j.parent = None
        bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.select_all(action="SELECT"); bpy.ops.uv.cube_project(cube_size=1.0); bpy.ops.object.mode_set(mode="OBJECT")
        vs = [j.matrix_world @ v.co for v in j.data.vertices]
        base = Vector(((min(v.x for v in vs) + max(v.x for v in vs)) / 2, (min(v.y for v in vs) + max(v.y for v in vs)) / 2, min(v.z for v in vs)))
        origin_to(j, base)
        j.name = "Grp_" + prefix; moving.add(j.name); MOVING.append(j.name)
    groups = {}
    for o in list(bpy.data.objects):
        if o.type != "MESH" or o.name in moving or o.parent is not None and o.parent.name in moving: continue
        if o.name.startswith("Proto_") or o.name.startswith("BeltSurface_") or o.name == "TerminalScreen": continue
        mat = o.data.materials[0].name if o.data.materials else "none"
        groups.setdefault(mat, []).append(o)
    for mat, objs in groups.items():
        bpy.ops.object.select_all(action="DESELECT")
        for o in objs:
            if o.animation_data: o.animation_data_clear()
            o.select_set(True)
        C.view_layer.objects.active = objs[0]
        bpy.ops.object.convert(target="MESH")          # applies bevels
        bpy.ops.object.join()
        j = C.view_layer.objects.active; j.name = "Static_" + mat
        bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.select_all(action="SELECT")
        bpy.ops.uv.cube_project(cube_size=1.0)
        bpy.ops.object.mode_set(mode="OBJECT")
    for o in bpy.data.objects:
        if o.animation_data: o.animation_data_clear()
        if o.type == "MESH" and o.modifiers:
            bpy.ops.object.select_all(action="DESELECT"); o.select_set(True); C.view_layer.objects.active = o
            bpy.ops.object.convert(target="MESH")
    os.makedirs(os.path.dirname(path), exist_ok=True)
    bpy.ops.object.select_all(action="DESELECT")
    bpy.ops.export_scene.fbx(filepath=path, use_selection=False, object_types={"MESH", "EMPTY", "LIGHT"},
                             apply_scale_options="FBX_SCALE_ALL", axis_forward="-Z", axis_up="Y",
                             use_mesh_modifiers=True, mesh_smooth_type="FACE", bake_anim=False, add_leaf_bones=False)
    return path
