"""Convert flat SVG pictograms into Avalonia DrawingImage resources (the Kennzeichnungen pictures).

Usage: svg2xaml.py <credits.json> <out.axaml> <key>[!white]=<file.svg> [...]

credits.json maps each SVG file name to {"page": <Commons page URL>, "licence": <licence>}; the
pair is written above its picture. Every picture is scaled into a 0..100 box (aspect kept,
centred) and clipped to it, so all pictures share one size. Transforms and arcs are flattened into
the path data, so the XAML needs no SVG library at run time. Needs svgelements (pip, MIT); run it
from a virtual environment, it is a tool for whoever adds a picture, not a build step.
"""
import json
import os
import sys
from xml.sax.saxutils import quoteattr

from svgelements import SVG, Shape, Path, Matrix, Color

BOX = 100.0


def fmt(x):
    s = f"{x:.2f}".rstrip("0").rstrip(".")
    return "0" if s in ("-0", "") else s


def colour(c):
    if c is None or c.value is None:
        return None
    c = Color(c)
    if c.alpha == 0:
        return None
    if c.alpha == 255:
        return "#%02X%02X%02X" % (c.red, c.green, c.blue)
    return "#%02X%02X%02X%02X" % (c.alpha, c.red, c.green, c.blue)


def path_data(path):
    parts = []
    for seg in path.segments():
        name = type(seg).__name__
        if name == "Move":
            parts.append(f"M{fmt(seg.end.x)},{fmt(seg.end.y)}")
        elif name == "Line":
            parts.append(f"L{fmt(seg.end.x)},{fmt(seg.end.y)}")
        elif name == "Close":
            parts.append("Z")
        elif name == "CubicBezier":
            parts.append(
                f"C{fmt(seg.control1.x)},{fmt(seg.control1.y)} {fmt(seg.control2.x)},{fmt(seg.control2.y)} {fmt(seg.end.x)},{fmt(seg.end.y)}")
        elif name == "QuadraticBezier":
            parts.append(f"Q{fmt(seg.control.x)},{fmt(seg.control.y)} {fmt(seg.end.x)},{fmt(seg.end.y)}")
        elif name == "Arc":
            # Arcs become cubics: a transformed arc can turn into an ellipse at an angle, which
            # the mini-language can carry, but cubics avoid every flag-ordering question.
            for cubic in seg.as_cubic_curves():
                parts.append(
                    f"C{fmt(cubic.control1.x)},{fmt(cubic.control1.y)} {fmt(cubic.control2.x)},{fmt(cubic.control2.y)} {fmt(cubic.end.x)},{fmt(cubic.end.y)}")
        else:
            raise ValueError(name)
    return " ".join(parts)


CREDITS = {}


def credit(file):
    entry = CREDITS[os.path.basename(file)]
    return f"{entry['page']} ({entry['licence']})"


def convert(key, file):
    # "key!white" lays a white diamond under the label: some files leave the white parts of an ADR
    # label transparent, which on the app's dark theme reads as grey or black.
    underlay = key.endswith("!white")
    key = key.removesuffix("!white")
    svg = SVG.parse(file, reify=True)
    # svgelements maps the viewBox onto the viewport (width/height, "pt" turned into px), so the
    # reified shapes are in viewport units. Only without a width does the viewBox stand alone.
    vb = svg.viewbox
    if "width" in svg.values and "height" in svg.values:
        x0, y0, w, h = 0, 0, svg.width, svg.height
    elif vb is not None:
        x0, y0, w, h = vb.x, vb.y, vb.width, vb.height
    else:
        x0, y0, w, h = 0, 0, svg.width, svg.height
    scale = BOX / max(w, h)
    fit = Matrix(f"translate({fmt((BOX - w * scale) / 2)},{fmt((BOX - h * scale) / 2)}) scale({scale}) translate({-x0},{-y0})")
    lines = [f"  <!-- {credit(file)} -->", f'  <DrawingImage x:Key="{key}">', f'    <DrawingGroup ClipGeometry="M0,0 H{fmt(BOX)} V{fmt(BOX)} H0 Z">']
    count = 0
    if underlay:
        lines.append('      <GeometryDrawing Brush="#FFFFFF" Geometry="F1 M50,1 L99,50 L50,99 L1,50 Z" />')
    for e in svg.elements():
        if not isinstance(e, Shape):
            continue
        if e.values.get("visibility") == "hidden" or e.values.get("display") == "none":
            continue
        fill = colour(e.fill)
        stroke = colour(e.stroke)
        if fill is None and stroke is None:
            continue
        p = Path(e) * fit
        p.reify()
        d = path_data(p)
        if not d:
            continue
        rule = "F0 " if e.values.get("fill-rule") == "evenodd" else "F1 "
        attrs = f' Geometry={quoteattr(rule + d)}'
        if fill is not None:
            attrs = f' Brush="{fill}"' + attrs
        if stroke is not None and e.stroke_width:
            width = e.stroke_width * scale
            join = {"round": "Round", "bevel": "Bevel"}.get(e.values.get("stroke-linejoin"), "Miter")
            cap = {"round": "Round", "square": "Square"}.get(e.values.get("stroke-linecap"), "Flat")
            lines.append(f"      <GeometryDrawing{attrs}>")
            lines.append(f'        <GeometryDrawing.Pen><Pen Brush="{stroke}" Thickness="{fmt(width)}" LineJoin="{join}" LineCap="{cap}" /></GeometryDrawing.Pen>')
            lines.append("      </GeometryDrawing>")
        else:
            lines.append(f"      <GeometryDrawing{attrs} />")
        count += 1
    lines += ["    </DrawingGroup>", "  </DrawingImage>"]
    return "\n".join(lines), count


def main():
    credits, out, pairs = sys.argv[1], sys.argv[2], sys.argv[3:]
    with open(credits, encoding="utf-8") as f:
        CREDITS.update(json.load(f))
    body = []
    for pair in pairs:
        key, file = pair.split("=", 1)
        xaml, count = convert(key, file)
        print(f"{key}: {count} drawings", file=sys.stderr)
        body.append(xaml)
    head = ('<ResourceDictionary xmlns="https://github.com/avaloniaui"\n'
            '                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">\n')
    with open(out, "w", encoding="utf-8", newline="\n") as f:
        f.write(head + "\n\n".join(body) + "\n</ResourceDictionary>\n")


if __name__ == "__main__":
    main()
