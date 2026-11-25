# Codebound main-menu scene v2: a working corner of a Codebound factory, animated as a seamless loop.
# Run inside Blender 5.x:
#   exec(open(r"<repo>/Codebound/Tools/art/menu_scene.py").read()); build()
#   render_still(path) / render_loop(path)
# Original geometry only (no external assets). Everything that moves repeats exactly every LOOP frames.
import bpy, math, random
from mathutils import Vector

random.seed(11)
C = bpy.context
LOOP = 144          # 6 s at 24 fps
FPS = 24
M = {}


# ================================================================== scene helpers
def clear():
    for o in list(bpy.data.objects):
        bpy.data.objects.remove(o, do_unlink=True)
    for coll in (bpy.data.meshes, bpy.data.materials, bpy.data.lights, bpy.data.cameras, bpy.data.curves, bpy.data.actions):
        for b in list(coll):
            if b.users == 0:
                coll.remove(b)


def bsdf(m):
    return next(n for n in m.node_tree.nodes if n.type == "BSDF_PRINCIPLED")


def mat_metal(name, color, rough=0.45, metallic=1.0, grain=3.0, var=0.25):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    nt = m.node_tree
    b = bsdf(m)
    b.inputs["Metallic"].default_value = metallic
    tex = nt.nodes.new("ShaderNodeTexNoise"); tex.inputs["Scale"].default_value = grain; tex.inputs["Detail"].default_value = 6
    rr = nt.nodes.new("ShaderNodeMapRange")
    rr.inputs["To Min"].default_value = rough * (1 - var); rr.inputs["To Max"].default_value = min(1, rough * (1 + var))
    nt.links.new(tex.outputs["Fac"], rr.inputs["Value"]); nt.links.new(rr.outputs["Result"], b.inputs["Roughness"])
    dirt = nt.nodes.new("ShaderNodeTexNoise"); dirt.inputs["Scale"].default_value = 1.4; dirt.inputs["Detail"].default_value = 8
    ramp = nt.nodes.new("ShaderNodeValToRGB")   # grime: darker in the noise valleys
    ramp.color_ramp.elements[0].position = 0.35; ramp.color_ramp.elements[0].color = (color[0] * 0.55, color[1] * 0.55, color[2] * 0.55, 1)
    ramp.color_ramp.elements[1].position = 0.62; ramp.color_ramp.elements[1].color = (*color, 1)
    nt.links.new(dirt.outputs["Fac"], ramp.inputs["Fac"]); nt.links.new(ramp.outputs["Color"], b.inputs["Base Color"])
    return m


def mat_emit(name, color, strength):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    b = bsdf(m)
    b.inputs["Base Color"].default_value = (*color, 1)
    b.inputs["Emission Color"].default_value = (*color, 1)
    b.inputs["Emission Strength"].default_value = strength
    return m


def mat_glass(name, tint):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    b = bsdf(m)
    b.inputs["Base Color"].default_value = (*tint, 1)
    b.inputs["Roughness"].default_value = 0.08
    b.inputs["Transmission Weight"].default_value = 0.9
    b.inputs["IOR"].default_value = 1.45
    return m


def mat_stripes(name, a, b_, scale=5.0):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    nt = m.node_tree; b = bsdf(m)
    tc = nt.nodes.new("ShaderNodeTexCoord")
    wave = nt.nodes.new("ShaderNodeTexWave"); wave.wave_type = "BANDS"; wave.bands_direction = "DIAGONAL"
    wave.inputs["Scale"].default_value = scale; wave.inputs["Distortion"].default_value = 0
    nt.links.new(tc.outputs["Object"], wave.inputs["Vector"])
    gt = nt.nodes.new("ShaderNodeMath"); gt.operation = "GREATER_THAN"; gt.inputs[1].default_value = 0.5
    nt.links.new(wave.outputs["Fac"], gt.inputs[0])
    mix = nt.nodes.new("ShaderNodeMix"); mix.data_type = "RGBA"
    mix.inputs[6].default_value = (*a, 1); mix.inputs[7].default_value = (*b_, 1)
    nt.links.new(gt.outputs["Value"], mix.inputs["Factor"]); nt.links.new(mix.outputs[2], b.inputs["Base Color"])
    b.inputs["Roughness"].default_value = 0.6
    return m


def mat_concrete():
    m = bpy.data.materials.new("Concrete")
    m.use_nodes = True
    nt = m.node_tree; b = bsdf(m)
    n1 = nt.nodes.new("ShaderNodeTexNoise"); n1.inputs["Scale"].default_value = 0.5; n1.inputs["Detail"].default_value = 12
    ramp = nt.nodes.new("ShaderNodeValToRGB")
    ramp.color_ramp.elements[0].color = (0.014, 0.015, 0.017, 1); ramp.color_ramp.elements[1].color = (0.05, 0.05, 0.05, 1)
    nt.links.new(n1.outputs["Fac"], ramp.inputs["Fac"]); nt.links.new(ramp.outputs["Color"], b.inputs["Base Color"])
    n2 = nt.nodes.new("ShaderNodeTexNoise"); n2.inputs["Scale"].default_value = 0.28; n2.inputs["Detail"].default_value = 3
    r2 = nt.nodes.new("ShaderNodeMapRange")
    r2.inputs["From Min"].default_value = 0.44; r2.inputs["From Max"].default_value = 0.54
    r2.inputs["To Min"].default_value = 0.85; r2.inputs["To Max"].default_value = 0.04
    nt.links.new(n2.outputs["Fac"], r2.inputs["Value"]); nt.links.new(r2.outputs["Result"], b.inputs["Roughness"])
    return m


def materials():
    M["steel"] = mat_metal("Steel", (0.42, 0.44, 0.47), 0.36)
    M["dark"] = mat_metal("DarkSteel", (0.06, 0.065, 0.075), 0.5)
    M["graphite"] = mat_metal("Graphite", (0.13, 0.14, 0.155), 0.45, 0.7)
    M["paint"] = mat_metal("SafetyOrange", (0.9, 0.3, 0.04), 0.48, 0.25)
    M["paintDark"] = mat_metal("PaintGrey", (0.2, 0.21, 0.23), 0.55, 0.3)
    M["copper"] = mat_metal("Copper", (0.85, 0.45, 0.28), 0.32)
    M["rubber"] = mat_metal("Rubber", (0.018, 0.018, 0.02), 0.8, 0.0)
    M["grate"] = mat_metal("Grating", (0.16, 0.17, 0.18), 0.55)
    M["crate"] = mat_metal("Crate", (0.5, 0.33, 0.15), 0.75, 0.0)
    M["ore"] = mat_metal("IronOre", (0.28, 0.33, 0.38), 0.55, 0.8)
    M["plate"] = mat_metal("IronPlate", (0.6, 0.62, 0.65), 0.3)
    M["coil"] = mat_metal("CopperCoil", (0.9, 0.48, 0.26), 0.3)
    M["glow"] = mat_emit("FurnaceGlow", (1.0, 0.34, 0.05), 30.0)
    M["inner"] = mat_emit("MachineInner", (1.0, 0.5, 0.15), 8.0)
    M["amber"] = mat_emit("AmberLed", (1.0, 0.5, 0.08), 14.0)
    M["green"] = mat_emit("GreenLed", (0.35, 1.0, 0.45), 10.0)
    M["cyan"] = mat_emit("CyanLed", (0.25, 0.75, 1.0), 9.0)
    M["red"] = mat_emit("RedLed", (1.0, 0.12, 0.05), 12.0)
    M["screen"] = mat_emit("ScreenBg", (0.02, 0.045, 0.055), 1.2)
    M["code"] = mat_emit("CodeText", (1.0, 0.62, 0.22), 7.0)
    M["codeDim"] = mat_emit("CodeDim", (0.4, 0.7, 0.75), 4.0)
    M["lamp"] = mat_emit("LampTube", (1.0, 0.75, 0.48), 45.0)
    M["glass"] = mat_glass("Glass", (0.6, 0.75, 0.8))
    M["hazard"] = mat_stripes("Hazard", (0.92, 0.5, 0.03), (0.025, 0.025, 0.025), 3.0)
    M["concrete"] = mat_concrete()
    M["sky"] = mat_emit("WindowSky", (0.3, 0.45, 0.7), 2.5)
    M["window"] = mat_emit("MachineWindow", (1.0, 0.5, 0.16), 1.3)
    M["wall"] = mat_metal("WallPanel", (0.035, 0.038, 0.045), 0.6, 0.4, 2.0, 0.1)


def obj_mat(o, m):
    o.data.materials.clear(); o.data.materials.append(m)
    return o


def bevel(o, w=0.03, seg=2):
    md = o.modifiers.new("Bevel", "BEVEL"); md.width = w; md.segments = seg; md.limit_method = "ANGLE"
    return o


def set_parent(o, p):
    mw = o.matrix_world.copy()
    o.parent = p
    o.matrix_world = mw


def box(name, loc, size, m, bw=0.03, rot=(0, 0, 0), parent=None):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc, rotation=rot)
    o = C.active_object; o.name = name; o.scale = size
    bpy.ops.object.transform_apply(scale=True)
    if bw > 0: bevel(o, bw)
    obj_mat(o, m)
    if parent: set_parent(o, parent)
    return o


def cyl(name, loc, r, depth, m, rot=(0, 0, 0), verts=32, bw=0.0, parent=None, smooth=True):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=r, depth=depth, location=loc, rotation=rot)
    o = C.active_object; o.name = name
    obj_mat(o, m)
    if bw > 0: bevel(o, bw)
    if smooth and verts >= 16: bpy.ops.object.shade_smooth()
    if parent: set_parent(o, parent)
    return o


def local(o, parent, loc):
    """Attach o to parent with a local position (for animated hierarchies)."""
    o.parent = parent
    o.matrix_parent_inverse.identity()
    o.location = loc
    return o


def torus(name, loc, R, r, m, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_torus_add(major_radius=R, minor_radius=r, location=loc, rotation=rot, major_segments=48, minor_segments=12)
    o = C.active_object; o.name = name; obj_mat(o, m); bpy.ops.object.shade_smooth()
    return o


def sphere(name, loc, r, m, scale=(1, 1, 1)):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=r, location=loc, segments=32, ring_count=16)
    o = C.active_object; o.name = name; o.scale = scale; obj_mat(o, m); bpy.ops.object.shade_smooth()
    return o


def empty(name, loc, parent=None):
    e = bpy.data.objects.new(name, None)
    C.collection.objects.link(e)
    if parent: e.parent = parent
    e.location = loc
    return e


def tube(name, pts, r, m):
    """A pipe or cable through points (bevelled curve, smooth bends)."""
    cu = bpy.data.curves.new(name, "CURVE"); cu.dimensions = "3D"
    cu.bevel_depth = r; cu.bevel_resolution = 4; cu.use_fill_caps = True
    sp = cu.splines.new("BEZIER"); sp.bezier_points.add(len(pts) - 1)
    for i, p in enumerate(pts):
        bp = sp.bezier_points[i]; bp.co = p; bp.handle_left_type = bp.handle_right_type = "AUTO"
    o = bpy.data.objects.new(name, cu); C.collection.objects.link(o); o.data.materials.append(m)
    return o


def light(kind, name, loc, energy, color, size=0.5, rot=(0, 0, 0), spot=None):
    ld = bpy.data.lights.new(name, kind); ld.energy = energy; ld.color = color
    if kind in ("POINT", "SPOT"): ld.shadow_soft_size = size
    if kind == "AREA": ld.size = size
    if kind == "SPOT" and spot: ld.spot_size = math.radians(spot); ld.spot_blend = 0.5
    o = bpy.data.objects.new(name, ld); o.location = loc; o.rotation_euler = rot
    C.collection.objects.link(o)
    return o


def text(name, body, loc, rot, size, m, align="LEFT"):
    bpy.ops.object.text_add(location=loc, rotation=rot)
    t = C.active_object; t.name = name; t.data.body = body; t.data.size = size; t.data.align_x = align
    t.data.materials.append(m)
    return t


# ================================================================== animation (everything periodic in LOOP)
def fcurves(action):
    try:
        fcs = action.fcurves
        if fcs is not None: return list(fcs)
    except (AttributeError, TypeError): pass
    out = []   # Blender 5 layered actions
    for layer in action.layers:
        for strip in layer.strips:
            for bag in strip.channelbags:
                out.extend(bag.fcurves)
    return out


def interp(o, mode, data=False):
    ad = (o.data if data else o).animation_data
    if ad and ad.action:
        for fc in fcurves(ad.action):
            for kp in fc.keyframe_points: kp.interpolation = mode


def key_loc_linear(o, p0, p1):
    o.location = p0; o.keyframe_insert("location", frame=0)
    o.location = p1; o.keyframe_insert("location", frame=LOOP)
    interp(o, "LINEAR")


def spin(o, axis, turns):
    o.rotation_euler[axis] = 0; o.keyframe_insert("rotation_euler", index=axis, frame=0)
    o.rotation_euler[axis] = math.tau * turns; o.keyframe_insert("rotation_euler", index=axis, frame=LOOP)
    interp(o, "LINEAR")


def flicker_light(o, base, amount, step=6, seed=0):
    rnd = random.Random(seed)
    vals = [base * (1 - amount + amount * rnd.random()) for _ in range(LOOP // step)]
    for i, v in enumerate(vals + [vals[0]]):
        o.data.energy = v; o.data.keyframe_insert("energy", frame=i * step)


def blink(o, period, on_frac=0.55, phase=0):
    """Switches an emissive part on and off (via scale) with the given period in frames."""
    s = o.scale.copy()
    for f in range(0, LOOP + 1):
        on = ((f + phase) % period) < period * on_frac
        prev = ((f - 1 + phase) % period) < period * on_frac
        if f == 0 or on != prev or f == LOOP:
            o.scale = s if on else s * 0.0001
            o.keyframe_insert("scale", frame=f)
    interp(o, "CONSTANT")
    o.scale = s


# ================================================================== the hall
def hall():
    box("Floor", (0, 20, -0.05), (90, 100, 0.1), M["concrete"], 0)
    lane = mat_metal("LanePaint", (0.45, 0.27, 0.05), 0.7, 0.0)
    for x in (-3.6, 3.6):
        box("Lane", (x, 22, 0.003), (0.09, 70, 0.006), lane, 0)
    for (x, y, w, d) in ((6.6, 4.2, 5.4, 6.8), (-7.6, 8.6, 4.4, 4.8)):
        box("ZoneA", (x, y, 0.004), (w, 0.18, 0.008), M["hazard"], 0)
        box("ZoneB", (x, y + d, 0.004), (w, 0.18, 0.008), M["hazard"], 0)
    for y in range(-2, 70, 9):
        for x in (-14, 14):
            box("Column", (x, y, 8), (0.55, 0.55, 16), M["dark"], 0.02)
            box("ColumnWeb", (x, y, 8), (1.0, 0.14, 16), M["dark"], 0.01)
            box("ColumnGuard", (x, y, 0.7), (0.62, 0.62, 1.4), M["hazard"], 0.01)
            text("Bay", "B-%02d" % ((y + 2) // 9 + 1), (x + (0.36 if x < 0 else -0.36), y - 0.6, 3.4),
                 (math.radians(90), 0, math.radians(90 if x < 0 else -90)), 0.45, M["paintDark"], align="CENTER")
        box("Truss", (0, y, 14.6), (29, 0.45, 0.6), M["graphite"], 0.02)
        box("TrussLow", (0, y, 13.4), (29, 0.25, 0.25), M["graphite"], 0.01)
        for k in range(-7, 8):
            box("Lattice", (k * 2.0, y, 14.0), (0.1, 0.1, 1.5), M["graphite"], 0, rot=(0, math.radians(38 if k % 2 else -38), 0))
        for lx in (-7, 7):
            cyl("LampShade", (lx, y, 11.6), 0.6, 0.4, M["paintDark"], verts=24)
            cyl("LampTube", (lx, y, 11.38), 0.46, 0.05, M["lamp"], verts=24)
            box("LampCable", (lx, y, 12.6), (0.03, 0.03, 2.0), M["dark"], 0)
            light("SPOT", "LampLight", (lx, y, 11.3), 3600, (1.0, 0.63, 0.32), 0.3, spot=62)
    box("BackWall", (0, 70, 9), (64, 0.6, 18), M["wall"], 0)
    for x in range(-27, 29, 6):
        box("Window", (x, 69.6, 10), (4.2, 0.1, 8), M["sky"], 0)
        for k in (-1, 0, 1):
            box("Mullion", (x + k * 1.4, 69.5, 10), (0.08, 0.1, 8), M["dark"], 0)
        box("Transom", (x, 69.5, 10), (4.2, 0.1, 0.08), M["dark"], 0)
    box("SideWallR", (20, 32, 9), (0.6, 80, 18), M["wall"], 0)
    box("SideWallL", (-20, 32, 9), (0.6, 80, 18), M["wall"], 0)


# ================================================================== machines
def assembler_mk2(name, loc, s=1.0, label="A-03", phase=0):
    """Octagonal assembler: lit glass band with a spinning ring inside, a turning fan, ribs, status LEDs."""
    x, y, z = loc
    root = empty(name, (0, 0, 0))
    cyl(name + "Base", (x, y, 0.3 * s), 2.35 * s, 0.6 * s, M["dark"], verts=8, bw=0.05, parent=root, smooth=False)
    cyl(name + "Skirt", (x, y, 0.68 * s), 2.2 * s, 0.16 * s, M["hazard"], verts=8, parent=root, smooth=False)
    cyl(name + "Body", (x, y, 1.6 * s), 2.0 * s, 1.7 * s, M["paintDark"], verts=8, bw=0.06, parent=root, smooth=False)
    cyl(name + "GlassBand", (x, y, 2.75 * s), 1.92 * s, 0.6 * s, M["window"], verts=8, parent=root, smooth=False)
    for k in range(8):   # mullions over the lit band
        a = (k + 0.5) * math.tau / 8 + math.tau / 16
        box(name + "Mullion", (x + math.cos(a) * 1.9 * s, y + math.sin(a) * 1.9 * s, 2.75 * s), (0.08 * s, 0.4 * s, 0.62 * s), M["dark"], 0, rot=(0, 0, a))
    cyl(name + "Inner", (x, y, 2.75 * s), 1.55 * s, 0.5 * s, M["inner"], verts=24, parent=root)
    ringp = empty(name + "RingPivot", (x, y, 2.75 * s))
    ring = torus(name + "Ring", (0, 0, 0), 1.25 * s, 0.08 * s, M["steel"], rot=(math.radians(90), 0, 0))
    local(ring, ringp, (0, 0, 0))
    spin(ringp, 2, 1)
    cyl(name + "Top", (x, y, 3.25 * s), 2.0 * s, 0.4 * s, M["paint"], verts=8, bw=0.05, parent=root, smooth=False)
    sphere(name + "Dome", (x, y, 3.45 * s), 1.3 * s, M["steel"], (1, 1, 0.42))
    cyl(name + "FanHub", (x, y, 4.05 * s), 0.25 * s, 0.3 * s, M["dark"], verts=16)
    fan = empty(name + "Fan", (x, y, 4.05 * s))
    for k in range(4):
        a = k * math.pi / 2
        blade = box(name + "Blade", (0, 0, 0), (1.1 * s, 0.24 * s, 0.04 * s), M["graphite"], 0.01, rot=(math.radians(14), 0, a))
        local(blade, fan, (math.cos(a) * 0.6 * s, math.sin(a) * 0.6 * s, 0))
    spin(fan, 2, 6)
    for k in range(8):
        a = (k + 0.5) * math.tau / 8
        box(name + "Rib", (x + math.cos(a) * 2.0 * s, y + math.sin(a) * 2.0 * s, 1.95 * s), (0.12 * s, 0.12 * s, 2.5 * s), M["dark"], 0.01, rot=(0, 0, a))
    for k, mat in enumerate((M["green"], M["amber"], M["green"])):
        box(name + "Led", (x - 0.35 * s + k * 0.35 * s, y - 1.98 * s, 1.15 * s), (0.18 * s, 0.04, 0.1 * s), mat, 0)
    text(name + "Label", label, (x, y - 2.0 * s, 1.55 * s), (math.radians(90), 0, 0), 0.38 * s, M["steel"], align="CENTER")
    beacon = cyl(name + "Beacon", (x + 1.5 * s, y - 0.9 * s, 3.65 * s), 0.11 * s, 0.22 * s, M["amber"], verts=16)
    blink(beacon, 24, 0.5, phase)
    tube(name + "Pipe", [(x + 2.1 * s, y + 0.6 * s, 2.2 * s), (x + 2.9 * s, y + 0.6 * s, 2.0 * s), (x + 3.1 * s, y + 0.6 * s, 0.2)], 0.14 * s, M["copper"])
    tube(name + "Cable", [(x - 1.8 * s, y + 1.0 * s, 3.3 * s), (x - 2.6 * s, y + 1.4 * s, 4.6), (x - 3.4 * s, y + 1.6 * s, 13.2)], 0.05, M["rubber"])


def furnace(name, loc, s=1.0, seed=0):
    x, y, z = loc
    box(name + "Plinth", (x, y, 0.2 * s), (3.6 * s, 3.2 * s, 0.4 * s), M["dark"], 0.04)
    box(name + "Body", (x, y, 2.0 * s), (3.2 * s, 2.8 * s, 3.2 * s), M["paintDark"], 0.12)
    box(name + "Brick", (x, y - 1.2 * s, 1.75 * s), (2.3 * s, 0.5 * s, 2.2 * s), mat_metal("FireBrick" + name, (0.26, 0.1, 0.06), 0.85, 0.0, 8, 0.1), 0.05)
    for k in range(5):
        box(name + "Grill", (x - 0.8 * s + k * 0.4 * s, y - 1.47 * s, 1.3 * s), (0.22 * s, 0.05, 1.0 * s), M["glow"], 0.0)
    box(name + "Hood", (x, y - 1.45 * s, 2.55 * s), (2.9 * s, 0.7 * s, 0.35 * s), M["paint"], 0.05, rot=(math.radians(-12), 0, 0))
    cyl(name + "Stack", (x + 0.7 * s, y + 0.5 * s, 7.4 * s), 0.42 * s, 8.6 * s, M["steel"], verts=24)
    for k in range(3):
        cyl(name + "Band", (x + 0.7 * s, y + 0.5 * s, 4.5 * s + k * 2.2 * s), 0.46 * s, 0.16 * s, M["paint"] if k == 1 else M["dark"], verts=24)
    fire = light("POINT", name + "Fire", (x, y - 2.4 * s, 1.3 * s), 1500 * s, (1.0, 0.38, 0.08), 0.7)
    flicker_light(fire, 1500 * s, 0.45, 4, seed)


def tanks(loc):
    x, y, z = loc
    for (dx, dy, r, h) in [(0, 0, 1.7, 7.0), (3.9, 1.2, 1.35, 5.6), (1.6, 3.9, 1.45, 8.2)]:
        cyl("Tank", (x + dx, y + dy, h / 2), r, h, M["steel"], verts=48)
        sphere("TankDome", (x + dx, y + dy, h), r, M["steel"], (1, 1, 0.32))
        for k in range(4):
            cyl("TankBand", (x + dx, y + dy, 0.8 + k * h / 4), r * 1.02, 0.14, M["paint"] if k == 1 else M["dark"], verts=48)
        for k in range(int(h / 0.45)):
            box("Rung", (x + dx - r - 0.08, y + dy, 0.4 + k * 0.45), (0.04, 0.5, 0.04), M["dark"], 0)
        for side in (-1, 1):
            box("LadderRail", (x + dx - r - 0.08, y + dy + side * 0.26, h / 2), (0.05, 0.05, h), M["dark"], 0)
    tube("TankPipe", [(x + 1.7, y + 0.2, 1.4), (x + 2.6, y + 0.6, 1.4), (x + 2.7, y + 1.0, 1.4)], 0.2, M["paint"])


ARM_KEYS = [(0, (0.0, 0.95, 1.55, 0.6)), (12, (0.0, 0.35, 1.0, 0.65)), (30, (1.9, 0.35, 1.0, 0.65)), (40, (1.9, 0.9, 1.5, 0.6)),
            (50, (1.9, 0.35, 1.0, 0.65)), (64, (0.0, 0.35, 1.0, 0.65)), (72, (0.0, 0.95, 1.55, 0.6))]


def arm_pose(t):
    t %= 72
    for (t0, p0), (t1, p1) in zip(ARM_KEYS, ARM_KEYS[1:]):
        if t0 <= t <= t1:
            k = (t - t0) / (t1 - t0); k = k * k * (3 - 2 * k)
            return tuple(a + (b - a) * k for a, b in zip(p0, p1))
    return ARM_KEYS[0][1]


def robot_arm(name, loc, yaw, phase=0):
    """Articulated arm: picks from the belt, swings, places, returns; twice per loop."""
    x, y, z = loc
    cyl(name + "Base", (x, y, 0.18), 0.6, 0.36, M["dark"], verts=24, bw=0.02)
    box(name + "BasePlate", (x, y, 0.02), (1.6, 1.6, 0.04), M["hazard"], 0)
    turret = empty(name + "Turret", (x, y, 0.36))
    local(cyl(name + "TurretMesh", (0, 0, 0), 0.45, 0.4, M["paint"], verts=24), turret, (0, 0, 0.2))
    shoulder = empty(name + "Shoulder", (0, 0, 0.5), turret)
    local(cyl(name + "ShoulderJoint", (0, 0, 0), 0.28, 0.62, M["dark"], rot=(0, math.radians(90), 0), verts=24), shoulder, (0, 0, 0))
    local(box(name + "Upper", (0, 0, 0), (0.3, 0.3, 1.7), M["paint"], 0.05), shoulder, (0, 0, 0.85))
    elbow = empty(name + "Elbow", (0, 0, 1.7), shoulder)
    local(cyl(name + "ElbowJoint", (0, 0, 0), 0.22, 0.52, M["dark"], rot=(0, math.radians(90), 0), verts=24), elbow, (0, 0, 0))
    local(box(name + "Fore", (0, 0, 0), (0.22, 0.22, 1.4), M["paint"], 0.04), elbow, (0, 0, 0.7))
    wrist = empty(name + "Wrist", (0, 0, 1.4), elbow)
    local(cyl(name + "WristJoint", (0, 0, 0), 0.16, 0.2, M["dark"], verts=16), wrist, (0, 0, 0.05))
    for side in (-1, 1):
        local(box(name + "Finger", (0, 0, 0), (0.06, 0.18, 0.35), M["steel"], 0.01), wrist, (side * 0.12, 0, 0.3))
    local(box(name + "Led", (0, 0, 0), (0.08, 0.02, 0.05), M["amber"], 0), wrist, (0, -0.16, 0.12))
    for f in range(0, LOOP + 1, 4):
        yw, sh, el, wr = arm_pose(f + phase)
        turret.rotation_euler[2] = yaw + yw; turret.keyframe_insert("rotation_euler", index=2, frame=f)
        shoulder.rotation_euler[0] = sh; shoulder.keyframe_insert("rotation_euler", index=0, frame=f)
        elbow.rotation_euler[0] = el; elbow.keyframe_insert("rotation_euler", index=0, frame=f)
        wrist.rotation_euler[0] = wr; wrist.keyframe_insert("rotation_euler", index=0, frame=f)
    for o in (turret, shoulder, elbow, wrist): interp(o, "LINEAR")


def conveyor(name, p0, p1, z0, z1, spacing=1.25, kinds=("crate", "ore"), housing=True):
    """Belt from p0 to p1 (x, y), rising from z0 to z1. Items and chevrons move two spacings per loop."""
    a = Vector((p0[0], p0[1], z0)); b = Vector((p1[0], p1[1], z1))
    d = b - a; L = d.length; u = d.normalized()
    yaw = math.atan2(d.y, d.x); pitch = math.asin(max(-1, min(1, d.z / L)))
    rot = (0, -pitch, yaw)
    mid = (a + b) / 2
    side = Vector((-math.sin(yaw), math.cos(yaw), 0))
    box(name + "Belt", tuple(mid + Vector((0, 0, 0.6))), (L, 1.0, 0.05), M["rubber"], 0.0, rot)
    for s in (-1, 1):
        box(name + "Rail", tuple(mid + side * 0.58 * s + Vector((0, 0, 0.66))), (L, 0.1, 0.24), M["paint"], 0.02, rot)
    n = max(2, int(L / 2.2))
    for i in range(n + 1):
        p = a + d * (i / n)
        h = p.z + 0.55
        for s in (-1, 1):
            box(name + "Leg", tuple(Vector((p.x, p.y, h / 2)) + side * 0.5 * s), (0.1, 0.1, h), M["dark"], 0)
        cyl(name + "Roller", tuple(p + Vector((0, 0, 0.55))), 0.07, 1.05, M["steel"], rot=(math.radians(90), 0, yaw), verts=12)
    lift = Vector((0, 0, 0.62))
    period = spacing * 2
    t = -period
    while t < L:
        p = a + u * t + lift + Vector((0, 0, 0.01))
        ch = box(name + "Chev", tuple(p), (0.08, 0.75, 0.012), M["graphite"], 0, rot)
        key_loc_linear(ch, tuple(p), tuple(p + u * period))
        t += 0.42
    t = -period; i = 0
    while t < L:
        kind = kinds[i % 2]
        if kind == "crate":
            o = box(name + "Crate", (0, 0, 0), (0.5, 0.5, 0.5), M["crate"], 0.03, rot); h = 0.27
        elif kind == "ore":
            bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=0.26); o = C.active_object; obj_mat(o, M["ore"]); o.scale = (1, 0.8, 0.7); h = 0.2
        elif kind == "plate":
            o = box(name + "Plate", (0, 0, 0), (0.6, 0.45, 0.08), M["plate"], 0.01, rot); h = 0.06
        else:
            o = cyl(name + "Coil", (0, 0, 0), 0.22, 0.36, M["coil"], rot=(math.radians(90), 0, yaw), verts=24); h = 0.24
        o.name = name + kind
        p = a + u * t + lift + Vector((0, 0, h))
        key_loc_linear(o, tuple(p), tuple(p + u * period))
        t += spacing; i += 1
    if housing:   # items come out of and go into these portals ("start": the end feeds a machine)
        for p, face in (((a, -1),) if housing == "start" else ((a, -1), (b, 1))):
            c = p + u * face * 0.6
            box(name + "Portal", tuple(c + Vector((0, 0, 0.95))), (1.6, 1.7, 1.9), M["paintDark"], 0.06, (0, 0, yaw))
            box(name + "PortalFrame", tuple(c - u * face * 0.81 + Vector((0, 0, 1.0))), (0.12, 1.5, 1.4), M["paint"], 0.02, (0, 0, yaw))
            box(name + "PortalLed", tuple(c - u * face * 0.83 + Vector((0, 0, 1.78))), (0.05, 0.9, 0.06), M["amber"], 0, (0, 0, yaw))


def terminal(name, loc, yaw):
    """The Codebound computer whose program runs this line; the cursor blinks."""
    x, y, z = loc
    box(name + "Desk", (x, y, 0.55), (2.6, 1.15, 1.1), M["paintDark"], 0.05, (0, 0, yaw))
    box(name + "DeskTop", (x, y - 0.05, 1.12), (2.7, 1.25, 0.06), M["graphite"], 0.02, (math.radians(-6), 0, yaw))
    tx, ty = x + 1.7 * math.cos(yaw), y + 1.7 * math.sin(yaw)
    box(name + "Tower", (tx, ty, 1.1), (0.75, 1.0, 2.2), M["dark"], 0.04, (0, 0, yaw))
    for k in range(6):
        led = box(name + "Led", (tx + 0.51 * math.sin(yaw), ty - 0.51 * math.cos(yaw), 1.8 - k * 0.2), (0.4, 0.02, 0.05), M["cyan"] if k % 3 else M["amber"], 0, (0, 0, yaw))
        blink(led, 12 + 6 * k, 0.6, k * 5)
    mx, my, mz = x - 0.12 * math.sin(yaw) * -1, y + 0.18, 1.98
    box(name + "Monitor", (mx, my, mz), (2.35, 0.12, 1.4), M["dark"], 0.04, (math.radians(-8), 0, yaw))
    box(name + "Screen", (mx, my - 0.07, mz), (2.15, 0.02, 1.22), M["screen"], 0, (math.radians(-8), 0, yaw))
    code = ["// line 2: smelt what the drills send", "while (true) {", "  const ore = belt.take();", "  if (ore) furnace.insert(ore);",
            "  if (furnace.done()) arm.place(\"out\");", "  hud.set(\"rate\", furnace.rate());", "  sleep(1);", "}"]
    rx = math.radians(90 - 8)
    fwd = Vector((math.sin(yaw), -math.cos(yaw), 0))     # screen normal (towards the viewer)
    right = Vector((math.cos(yaw), math.sin(yaw), 0))
    base = Vector((mx, my, mz)) + fwd * 0.09 - right * 1.0
    for i, line in enumerate(code):
        p = base + Vector((0, 0, 0.48 - i * 0.12))
        text(name + "Code", line, tuple(p), (rx, 0, yaw), 0.075, M["codeDim"] if i == 0 else M["code"])
    cur = box(name + "Cursor", tuple(base + right * 0.08 + Vector((0, 0, 0.48 - len(code) * 0.12 + 0.035))), (0.05, 0.012, 0.08), M["code"], 0, (math.radians(-8), 0, yaw))
    blink(cur, 24, 0.55)
    light("AREA", name + "ScreenGlow", tuple(Vector((mx, my, mz)) + fwd * 1.0), 70, (1.0, 0.72, 0.45), 2.2, rot=(math.radians(90), 0, yaw))


def mezzanine():
    """Steel platform on the right: stairs, railings, pipe runs and a valve station."""
    x0, y0 = 12.0, 12
    box("MezzDeck", (x0, y0 + 6, 4.2), (6.0, 16, 0.14), M["grate"], 0.01)
    box("MezzEdge", (x0 - 3.0, y0 + 6, 4.05), (0.12, 16, 0.4), M["paint"], 0.01)
    for y in range(int(y0 - 1), int(y0 + 14), 3):
        box("MezzPost", (x0 - 2.95, y, 2.1), (0.22, 0.22, 4.2), M["dark"], 0.01)
        box("RailPost", (x0 - 2.95, y, 4.8), (0.06, 0.06, 1.1), M["paint"], 0)
    box("RailTop", (x0 - 2.95, y0 + 6, 5.35), (0.07, 16, 0.07), M["paint"], 0)
    box("RailMid", (x0 - 2.95, y0 + 6, 4.85), (0.05, 16, 0.05), M["paint"], 0)
    for k in range(14):
        box("Step", (x0 - 1.4, y0 - 2.0 - k * 0.34, 4.0 - k * 0.3), (1.6, 0.32, 0.06), M["grate"], 0.005)
    tube("StairRail", [(x0 - 2.25, y0 - 1.8, 5.0), (x0 - 2.25, y0 - 6.8, 0.9)], 0.04, M["paint"])
    for k, z in enumerate((5.6, 6.2, 6.8)):
        tube("MezzPipe", [(x0 + 2.5, y0 - 6, z), (x0 + 2.5, y0 + 4, z), (x0 + 1.0, y0 + 6, z + 1.2), (x0 - 6, y0 + 9, z + 2.4), (-14, y0 + 9, z + 2.4)],
             0.18 + 0.05 * k, M["copper"] if k == 1 else M["steel"])
    cyl("Valve", (x0 + 2.2, y0 - 2, 6.2), 0.35, 0.08, M["red"], rot=(0, math.radians(90), 0), verts=24)


def crane():
    for x in (-13.6, 13.6):
        box("CraneRail", (x, 24, 10.8), (0.45, 56, 0.45), M["dark"], 0.02)
    box("CraneBeam", (0, 16, 11.1), (27.6, 0.9, 1.0), M["paint"], 0.04)
    box("CraneBeamB", (0, 16, 10.45), (27.6, 0.5, 0.3), M["dark"], 0.02)
    trolley = empty("Trolley", (0, 16, 0))
    for o, p in ((box("TrolleyBox", (0, 0, 0), (1.5, 1.5, 0.9), M["graphite"], 0.04), (0, 0, 10.3)), (box("Cable", (0, 0, 0), (0.04, 0.04, 3.6), M["dark"], 0), (0, 0, 8.2)),
                 (box("HookBlock", (0, 0, 0), (0.6, 0.45, 0.75), M["paint"], 0.04), (0, 0, 6.3)), (box("Load", (0, 0, 0), (2.2, 1.6, 1.4), M["crate"], 0.04), (0, 0, 5.0)),
                 (box("LoadBand", (0, 0, 0), (2.24, 1.64, 0.2), M["dark"], 0.0), (0, 0, 5.0)), (cyl("CraneBeacon", (0, 0, 0), 0.13, 0.25, M["amber"], verts=16), (0.6, -0.5, 10.85))):
        local(o, trolley, p)
    for f, x in ((0, -3.0), (LOOP // 2, 3.0), (LOOP, -3.0)):
        trolley.location.x = x; trolley.keyframe_insert("location", index=0, frame=f)
    sweep = empty("CraneSweepPivot", (0.6, -0.5, 10.75), trolley)
    lt = light("SPOT", "CraneSweep", (0, 0, 0), 1200, (1.0, 0.45, 0.1), 0.05, spot=30)
    local(lt, sweep, (0, 0, 0)); lt.rotation_euler = (math.radians(65), 0, 0)
    spin(sweep, 2, 3)


def build():
    clear()
    materials()
    sc = C.scene
    sc.frame_start = 0; sc.frame_end = LOOP - 1; sc.render.fps = FPS
    hall()
    assembler_mk2("AsmHero", (4.6, 7.4, 0), 1.25, "A-03", 0)
    terminal("PC", (6.9, 2.4, 0), math.radians(-38))
    assembler_mk2("AsmB", (-7.6, 11.0, 0), 0.95, "A-02", 9)
    assembler_mk2("AsmFar", (-4.0, 38, 0), 1.0, "A-07", 17)
    furnace("FurnA", (-1.2, 22, 0), 1.0, 1)
    furnace("FurnB", (3.4, 24, 0), 1.0, 2)
    furnace("FurnC", (8.0, 26, 0), 1.0, 3)
    conveyor("BeltFront", (-17, 3.2), (2.5, 5.5), 0.0, 0.0, housing="start")
    conveyor("BeltMid", (-12, 17.0), (11, 17.0), 0.0, 0.0, kinds=("plate", "coil"))
    conveyor("BeltHigh", (-18, 30.0), (16, 30.0), 3.6, 3.6, kinds=("crate", "plate"), housing=False)
    conveyor("BeltIncline", (-2.5, 12.0), (-2.5, 29.4), 0.0, 3.6, kinds=("ore", "crate"), housing=False)
    robot_arm("ArmA", (0.6, 6.2, 0), math.radians(160), 0)
    robot_arm("ArmB", (-5.0, 15.3, 0), math.radians(10), 24)
    robot_arm("ArmC", (5.5, 15.3, 0), math.radians(-10), 48)
    tanks((15.0, 42, 0))
    mezzanine()
    crane()
    camera()
    world()


def camera():
    cd = bpy.data.cameras.new("MenuCam"); cd.lens = 19
    cd.dof.use_dof = True; cd.dof.focus_distance = 9.0; cd.dof.aperture_fstop = 5.6
    cam = bpy.data.objects.new("MenuCam", cd)
    cam.location = (-1.6, -4.4, 1.5)
    cam.rotation_euler = (math.radians(94), 0, math.radians(-18))
    C.collection.objects.link(cam); C.scene.camera = cam


def world():
    sc = C.scene
    w = sc.world or bpy.data.worlds.new("World"); sc.world = w
    w.use_nodes = True; nt = w.node_tree
    for n in list(nt.nodes):
        if n.type not in ("BACKGROUND", "OUTPUT_WORLD"): nt.nodes.remove(n)
    bg = next(n for n in nt.nodes if n.type == "BACKGROUND")
    bg.inputs["Color"].default_value = (0.006, 0.008, 0.012, 1); bg.inputs["Strength"].default_value = 1.0
    out = next(n for n in nt.nodes if n.type == "OUTPUT_WORLD")
    vol = nt.nodes.new("ShaderNodeVolumePrincipled")
    vol.inputs["Density"].default_value = 0.016; vol.inputs["Anisotropy"].default_value = 0.55
    vol.inputs["Color"].default_value = (0.8, 0.78, 0.76, 1)
    nt.links.new(vol.outputs[0], out.inputs["Volume"])
    box("Roof", (0, 30, 16.4), (42, 84, 0.4), M["wall"], 0)
    for x in range(-21, 23, 6):   # daylight shafts falling in through the back windows
        light("SPOT", "WindowShaft", (x, 68.6, 12.5), 26000, (0.62, 0.74, 1.0), 1.5, rot=(math.radians(-58), 0, math.radians(8 if x < 0 else -8)), spot=26)
    light("AREA", "Fill", (-2, -6, 6), 40, (0.45, 0.6, 1.0), 10, rot=(math.radians(70), 0, math.radians(-10)))
    light("AREA", "BackGlow", (2, 33, 3), 2200, (1.0, 0.45, 0.15), 16, rot=(math.radians(-90), 0, 0))
    light("SPOT", "HeroKey", (2.6, 2.0, 9.5), 5200, (1.0, 0.7, 0.42), 0.6, rot=(math.radians(-24), 0, math.radians(-30)), spot=40)
    light("AREA", "HeroBack", (5.0, 13.0, 5.0), 900, (0.55, 0.68, 1.0), 4, rot=(math.radians(-60), 0, 0))


def setup_render(width, height, samples, motion_blur=True):
    sc = C.scene
    try: sc.render.engine = "BLENDER_EEVEE"
    except TypeError: sc.render.engine = "BLENDER_EEVEE_NEXT"
    sc.render.resolution_x = width; sc.render.resolution_y = height; sc.render.resolution_percentage = 100
    ee = sc.eevee
    for attr, val in (("taa_render_samples", samples), ("use_shadows", True), ("use_raytracing", True), ("volumetric_tile_size", "4"),
                      ("volumetric_samples", 64), ("use_volumetric_shadows", True), ("volumetric_end", 95.0)):
        try: setattr(ee, attr, val)
        except (AttributeError, TypeError): pass
    try: sc.render.use_motion_blur = motion_blur
    except AttributeError: pass
    vs = sc.view_settings
    try: vs.view_transform = "AgX"
    except TypeError: pass
    try: vs.look = "AgX - Medium High Contrast"
    except TypeError: pass
    vs.exposure = 0.1


def render_still(path, width=3840, height=2160, samples=96, frame=0):
    setup_render(width, height, samples, False)
    sc = C.scene; sc.frame_set(frame)
    im = sc.render.image_settings
    try: im.media_type = "IMAGE"
    except (AttributeError, TypeError): pass
    im.file_format = "JPEG"; im.quality = 92
    sc.render.filepath = path
    bpy.ops.render.render(write_still=True)


def render_loop(path, width=2560, height=1440, samples=32):
    setup_render(width, height, samples)
    sc = C.scene
    im = sc.render.image_settings
    try: im.media_type = "VIDEO"
    except (AttributeError, TypeError): pass
    im.file_format = "FFMPEG"
    ff = sc.render.ffmpeg
    ff.format = "MPEG4"; ff.codec = "H264"; ff.constant_rate_factor = "HIGH"; ff.ffmpeg_preset = "GOOD"; ff.gopsize = 12
    ff.audio_codec = "NONE"
    sc.render.filepath = path
    sc.frame_start = 0; sc.frame_end = LOOP - 1
    bpy.ops.render.render(animation=True)
