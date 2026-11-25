# Codebound logotype: original chamfered industrial letterforms, extruded steel with an amber edge.
# Run inside Blender: exec(open(r"<repo>/Codebound/Tools/art/menu_logo.py").read()); render_logo(path)
import bpy, math

W, H, T = 6.0, 8.0, 1.75     # letter cell and stroke
CH = 1.3                      # chamfer of outer corners
GAP = 1.25


def chamfer_rect(x0, y0, x1, y1, c, corners="tl tr br bl"):
    pts = []
    if "bl" in corners: pts += [(x0, y0 + c), (x0 + c, y0)]
    else: pts += [(x0, y0)]
    if "br" in corners: pts += [(x1 - c, y0), (x1, y0 + c)]
    else: pts += [(x1, y0)]
    if "tr" in corners: pts += [(x1, y1 - c), (x1 - c, y1)]
    else: pts += [(x1, y1)]
    if "tl" in corners: pts += [(x0 + c, y1), (x0, y1 - c)]
    else: pts += [(x0, y1)]
    return pts


def letter(ch):
    """List of closed polygons (outer first, holes after); even-odd fill makes holes."""
    t, w, h, c = T, W, H, CH
    ci = c * 0.55
    if ch == "O":
        return [chamfer_rect(0, 0, w, h, c), chamfer_rect(t, t, w - t, h - t, ci)]
    if ch == "D":
        return [chamfer_rect(0, 0, w, h, c * 1.6, "tr br"), chamfer_rect(t, t, w - t, h - t, ci * 1.4, "tr br")]
    if ch == "C":
        return [[(c, 0), (w, 0), (w, t), (t + ci, t), (t, t + ci), (t, h - t - ci), (t + ci, h - t), (w, h - t), (w, h), (c, h), (0, h - c), (0, c)]]
    if ch == "E":
        m = h / 2
        return [[(c, 0), (w, 0), (w, t), (t, t), (t, m - t / 2), (w - 0.9, m - t / 2), (w - 0.9, m + t / 2), (t, m + t / 2),
                 (t, h - t), (w, h - t), (w, h), (c, h), (0, h - c), (0, c)]]
    if ch == "B":
        m = h / 2
        outer = [(0, 0), (w - c, 0), (w, c), (w, m - 0.5), (w - 0.6, m), (w, m + 0.5), (w, h - c), (w - c, h), (0, h)]
        top = chamfer_rect(t, m + t / 2, w - t, h - t, ci, "tr br")
        bot = chamfer_rect(t, t, w - t, m - t / 2, ci, "tr br")
        return [outer, top, bot]
    if ch == "U":
        return [[(0, h), (t, h), (t, t + ci), (t + ci, t), (w - t - ci, t), (w - t, t + ci), (w - t, h), (w, h), (w, c), (w - c, 0), (c, 0), (0, c)]]
    if ch == "N":
        return [[(0, 0), (t, 0), (t, h - 3.6), (w - t, 0), (w, 0), (w, h), (w - t, h), (w - t, 3.6), (t, h), (0, h)]]
    raise ValueError(ch)


def make_curve(name, polys, ox, mat):
    cu = bpy.data.curves.new(name, "CURVE")
    cu.dimensions = "2D"
    cu.fill_mode = "BOTH"
    cu.extrude = 0.55
    cu.bevel_depth = 0.12
    cu.bevel_resolution = 3
    for poly in polys:
        sp = cu.splines.new("POLY")
        sp.points.add(len(poly) - 1)
        for i, (x, y) in enumerate(poly):
            sp.points[i].co = (x + ox, y, 0, 1)
        sp.use_cyclic_u = True
    o = bpy.data.objects.new(name, cu)
    o.data.materials.append(mat)
    bpy.context.collection.objects.link(o)
    return o


def mat(name, color, metallic, rough, emit=None, strength=0):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True
    b = next(n for n in m.node_tree.nodes if n.type == "BSDF_PRINCIPLED")
    b.inputs["Base Color"].default_value = (*color, 1)
    b.inputs["Metallic"].default_value = metallic
    b.inputs["Roughness"].default_value = rough
    if emit:
        b.inputs["Emission Color"].default_value = (*emit, 1)
        b.inputs["Emission Strength"].default_value = strength
    return m


def build_logo():
    for o in list(bpy.data.objects):
        bpy.data.objects.remove(o, do_unlink=True)
    steel = mat("LogoSteel", (0.78, 0.76, 0.72), 1.0, 0.28)
    amber = mat("LogoAmber", (0.95, 0.42, 0.06), 0.0, 0.35, (1.0, 0.38, 0.04), 0.9)
    dark = mat("LogoDark", (0.04, 0.045, 0.05), 0.8, 0.5)
    x = 0.0
    for i, ch in enumerate("CODEBOUND"):
        make_curve("L_" + ch + str(i), letter(ch), x, steel)
        x += W + GAP
    total = x - GAP
    # amber underline bar, split like a stencil, and a dark backing plate for depth
    bpy.ops.mesh.primitive_cube_add(size=1, location=(total * 0.5, -1.25, 0.2))
    bar = bpy.context.active_object; bar.scale = (total, 0.45, 0.35); bar.data.materials.append(amber)
    for k in (0.25, 0.5, 0.75):
        bpy.ops.mesh.primitive_cube_add(size=1, location=(total * k, -1.25, 0.25))
        notch = bpy.context.active_object; notch.scale = (0.5, 0.5, 0.42); notch.data.materials.append(dark)
    # camera (orthographic, front) and studio lights
    cd = bpy.data.cameras.new("LogoCam"); cd.type = "ORTHO"; cd.ortho_scale = total + 3
    cam = bpy.data.objects.new("LogoCam", cd); cam.location = (total / 2, 3.2, 40); cam.rotation_euler = (0, 0, 0)
    bpy.context.collection.objects.link(cam); bpy.context.scene.camera = cam
    for name, loc, energy, color, rot in (("Key", (total * 0.2, 14, 12), 9000, (1.0, 0.92, 0.85), (math.radians(-40), math.radians(-15), 0)),
                                          ("Warm", (total * 0.9, -6, 8), 7000, (1.0, 0.5, 0.18), (math.radians(45), math.radians(25), 0)),
                                          ("Top", (total * 0.5, 30, 6), 5000, (0.6, 0.75, 1.0), (math.radians(-75), 0, 0))):
        ld = bpy.data.lights.new(name, "AREA"); ld.energy = energy; ld.color = color; ld.size = 12
        lo = bpy.data.objects.new(name, ld); lo.location = loc; lo.rotation_euler = rot
        bpy.context.collection.objects.link(lo)
    w = bpy.context.scene.world
    nt = w.node_tree
    for n in list(nt.nodes):   # drop the scene's fog: the logo is rendered clean
        if n.type == "PRINCIPLED_VOLUME": nt.nodes.remove(n)
    bg = next(n for n in nt.nodes if n.type == "BACKGROUND")
    bg.inputs["Color"].default_value = (0.35, 0.33, 0.3, 1); bg.inputs["Strength"].default_value = 0.6
    return total


def render_logo(path, width=2400):
    total = build_logo()
    sc = bpy.context.scene
    sc.render.resolution_x = width
    sc.render.resolution_y = int(width * (H + 3.6) / (total + 3))
    sc.render.film_transparent = True
    sc.render.image_settings.file_format = "PNG"
    sc.render.image_settings.color_mode = "RGBA"
    sc.render.filepath = path
    try: sc.view_settings.look = "AgX - Medium High Contrast"
    except TypeError: pass
    sc.view_settings.exposure = 0.0
    bpy.ops.render.render(write_still=True)
