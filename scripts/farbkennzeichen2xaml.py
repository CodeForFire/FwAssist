"""Draw the Rohrleitung and Gasflasche pictures of the Kennzeichnungen helper as XAML.

Usage: farbkennzeichen2xaml.py <out.axaml>

Unlike the pictograms (scripts/svg2xaml.py), these markings are nothing but coloured shapes, so
they are drawn here from the sources' colour tables and owe no licence to anyone. The colour
*meaning* comes from the sources named below; the sRGB values are approximations of the RAL
shades, chosen for this drawing. The screen says every colour in words as well, so nothing
depends on how close a phone display gets to a RAL card.
"""
import sys

# EN 1089-3:2011, Anhang A, Tabelle A.1 (RAL 840 HR).
EN1089 = {
    "gelb": "#F3DA0B",          # RAL 1018 Zinkgelb
    "rot": "#AF2B1E",           # RAL 3000 Feuerrot
    "hellblau": "#3B83BD",      # RAL 5012 Lichtblau
    "gruen": "#57A639",         # RAL 6018 Gelbgrün ("leuchtendes Grün")
    "kastanienbraun": "#642424",  # RAL 3009 Oxidrot
    "weiss": "#F4F4F4",         # RAL 9010 Reinweiß
    "blau": "#0E294B",          # RAL 5010 Enzianblau
    "dunkelgruen": "#287233",   # RAL 6001 Smaragdgrün
    "schwarz": "#0A0A0A",       # RAL 9005 Tiefschwarz
    "grau": "#8F8F8C",          # RAL 7037 Staubgrau
    "braun": "#6F4F28",         # RAL 8008 Olivbraun
    "tuerkis": "#3F888F",       # RAL 5018 Türkisblau
}

# TRGS 201 Anhang 3 names the colours of DIN 2403; drawn here in the RAL signal colours.
DIN2403 = {
    "gruen": "#237F52",   # RAL 6032 Signalgrün
    "rot": "#A72920",     # RAL 3001 Signalrot
    "grau": "#969992",    # RAL 7004 Signalgrau
    "gelb": "#F9A800",    # RAL 1003 Signalgelb
    "orange": "#D4652F",  # RAL 2010 Signalorange
    "violett": "#924E7D",  # RAL 4008 Signalviolett
    "braun": "#6C3B2A",   # RAL 8002 Signalbraun
    "blau": "#154889",    # RAL 5005 Signalblau
    "weiss": "#F4F4F4",   # RAL 9003 Signalweiß
    "schwarz": "#282828",  # RAL 9004 Signalschwarz
}

# The app's dark background swallows a black shoulder or label; a light outline keeps the shape.
OUTLINE = "#C8CDD4"
INDUSTRIAL_BODY = "#5C636B"  # IGV: industrial cylinders grey, never white
VALVE = "#9AA3AD"

# Rohrleitungen, TRGS 201 Anhang 3, Tabelle 2: key, group colour, extra colour, lettering colour.
ROHR = [
    ("rohr-1", "gruen", None, "weiss"),
    ("rohr-2", "rot", None, "weiss"),
    ("rohr-3", "grau", None, "schwarz"),
    ("rohr-4", "gelb", "rot", "schwarz"),
    ("rohr-5", "gelb", "schwarz", "schwarz"),
    ("rohr-6", "orange", None, "schwarz"),
    ("rohr-7", "violett", None, "weiss"),
    ("rohr-8", "braun", "rot", "weiss"),
    ("rohr-9", "braun", "schwarz", "weiss"),
    ("rohr-0", "blau", None, "weiss"),
]

# Gasflaschen, EN 1089-3: key, shoulder colours, layout (rings or quadrants), body colour.
FLASCHE = [
    ("flasche-gelb", ["gelb"], "ring", INDUSTRIAL_BODY),
    ("flasche-rot", ["rot"], "ring", INDUSTRIAL_BODY),
    ("flasche-hellblau", ["hellblau"], "ring", INDUSTRIAL_BODY),
    ("flasche-gruen", ["gruen"], "ring", INDUSTRIAL_BODY),
    ("flasche-gelb-rot", ["gelb", "rot"], "ring", INDUSTRIAL_BODY),
    ("flasche-gelb-hellblau", ["gelb", "hellblau"], "ring", INDUSTRIAL_BODY),
    ("flasche-acetylen", ["kastanienbraun"], "ring", INDUSTRIAL_BODY),
    ("flasche-sauerstoff", ["weiss"], "ring", INDUSTRIAL_BODY),
    ("flasche-lachgas", ["blau"], "ring", INDUSTRIAL_BODY),
    ("flasche-argon", ["dunkelgruen"], "ring", INDUSTRIAL_BODY),
    ("flasche-stickstoff", ["schwarz"], "ring", INDUSTRIAL_BODY),
    ("flasche-kohlendioxid", ["grau"], "ring", INDUSTRIAL_BODY),
    ("flasche-helium", ["braun"], "ring", INDUSTRIAL_BODY),
    ("flasche-luft", ["weiss", "schwarz"], "quadrant", INDUSTRIAL_BODY),
    ("flasche-sauerstoff-helium", ["weiss", "braun"], "ring", INDUSTRIAL_BODY),
    ("flasche-sauerstoff-kohlendioxid", ["weiss", "grau"], "ring", INDUSTRIAL_BODY),
    ("flasche-sauerstoff-lachgas", ["weiss", "blau"], "ring", INDUSTRIAL_BODY),
    ("flasche-stickstoffmonoxid", ["tuerkis"], "ring", EN1089["weiss"]),
    ("flasche-medizinisch", ["weiss"], "ring", EN1089["weiss"]),
]


def drawing(brush, geometry, pen=None):
    attrs = f'Brush="{brush}" ' if brush else ""
    if pen:
        return (f'      <GeometryDrawing {attrs}Geometry="{geometry}">\n'
                f'        <GeometryDrawing.Pen><Pen Brush="{pen[0]}" Thickness="{pen[1]}" LineJoin="Round" /></GeometryDrawing.Pen>\n'
                f"      </GeometryDrawing>")
    return f'      <GeometryDrawing {attrs}Geometry="{geometry}" />'


def image(key, parts, comment):
    body = "\n".join(parts)
    return (f"  <!-- {comment} -->\n"
            f'  <DrawingImage x:Key="{key}">\n'
            f'    <DrawingGroup ClipGeometry="M0,0 H100 V100 H0 Z">\n{body}\n'
            f"    </DrawingGroup>\n  </DrawingImage>")


def rohr(key, group, extra, letters):
    # A pipe run with its marking: the group colour as a band, the extra colour as a field at each
    # end of the band, and the flow arrow in the lettering colour.
    parts = [
        drawing("#3A424D", "M2,40 H98 V60 H2 Z", (OUTLINE, 1)),
        drawing(DIN2403[group], "M14,36 H86 V64 H14 Z", (OUTLINE, 1)),
    ]
    if extra:
        parts.append(drawing(DIN2403[extra], "M14,36 H24 V64 H14 Z"))
        parts.append(drawing(DIN2403[extra], "M76,36 H86 V64 H76 Z"))
    parts.append(drawing(DIN2403[letters], "M32,46 H58 V40 L70,50 L58,60 V54 H32 Z"))
    return image(key, parts, f"TRGS 201 Anhang 3: {group}{' + ' + extra if extra else ''}, Schrift {letters}")


SHOULDER = "M36,40 C36,24 42,18 50,18 C58,18 64,24 64,40 Z"
BODY = "M36,40 H64 V90 C64,94 61,96 58,96 H42 C39,96 36,94 36,90 Z"


def flasche(key, colours, layout, body):
    parts = [
        drawing(VALVE, "M45,6 H55 V19 H45 Z", (OUTLINE, 0.8)),
        drawing(body, BODY, (OUTLINE, 1)),
    ]
    first = EN1089[colours[0]]
    parts.append(drawing(first, SHOULDER))
    if len(colours) == 2:
        second = EN1089[colours[1]]
        if layout == "quadrant":
            # Seen from the side, two of the four quadrants: the right half in the second colour.
            parts.append(drawing(second, "M50,18 C58,18 64,24 64,40 H50 Z"))
        else:
            # Rings: the lower band of the shoulder in the second colour.
            parts.append(drawing(second, "M36,40 C36,35 36.6,31 37.7,29 H62.3 C63.4,31 64,35 64,40 Z"))
    parts.append(drawing(None, SHOULDER, (OUTLINE, 1)))
    comment = f"EN 1089-3: Schulter {' + '.join(colours)}{' (Quadranten)' if layout == 'quadrant' else ''}"
    return image(key, parts, comment)


def main():
    out = sys.argv[1]
    images = [rohr(*r) for r in ROHR] + [flasche(*f) for f in FLASCHE]
    head = ('<!-- Generated by scripts/farbkennzeichen2xaml.py: the Rohrleitung and Gasflasche pictures of\n'
            '     the Kennzeichnungen helper, drawn from the colour tables of TRGS 201 Anhang 3 (DIN 2403)\n'
            '     and EN 1089-3. Regenerate rather than edit by hand. -->\n'
            '<ResourceDictionary xmlns="https://github.com/avaloniaui"\n'
            '                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">\n')
    with open(out, "w", encoding="utf-8", newline="\n") as f:
        f.write(head + "\n\n".join(images) + "\n</ResourceDictionary>\n")


if __name__ == "__main__":
    main()
