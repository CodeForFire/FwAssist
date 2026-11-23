#!/usr/bin/env bash
# Renders docs/play/feature-graphic-1024x500.png, the Play listing's feature graphic.
#
# The same horizontal lockup as Lagebuch's (packaging/logo/build-logo-assets.sh there), so the
# two listings read as one family: the mark beside the wordmark, on the app background, set in
# the app's own fonts (Oswald for the wordmark, Barlow for the tagline).
#
# The mark is the launcher icon below API 26 (Resources/drawable/ic_launcher.xml): a signal disc
# with the white four-tile glyph. The glyph's path is read from ic_launcher_foreground.xml rather
# than copied here, so a new launcher glyph changes the banner on the next run.
#
# Play crops this asset to several ratios, so the lockup stays in the middle with nothing near an
# edge, and it rejects alpha outright, hence -alpha off and PNG24. -strip and the excluded
# date/time chunks keep a re-run byte-identical, so an unchanged graphic never shows up in a diff.
#
# Needs ImageMagick 7 (magick) and rsvg-convert.
set -euo pipefail

repo_root=$(cd "$(dirname "$0")/.." && pwd)
fonts="$repo_root/src/FwAssist.Ui/Assets/Fonts"
foreground="$repo_root/src/FwAssist.App.Android/Resources/drawable/ic_launcher_foreground.xml"
out="$repo_root/docs/play/feature-graphic-1024x500.png"

# Theme/Tokens.axaml: AppBgColor, SignalDeepColor (= @color/icon_signal), SignalColor, TextSecondaryColor.
app_bg="#0B0E13"
disc="#C71E12"
accent="#FF3D2E"
tagline_ink="#9AA7B8"

glyph=$(sed -n 's/.*android:pathData="\([^"]*\)".*/\1/p' "$foreground")
test -n "$glyph" || { echo "No pathData in $foreground" >&2; exit 1; }

t=$(mktemp -d)
trap 'rm -rf "$t"' EXIT

# The disc and the glyph at the proportions of ic_launcher.xml: a 24-unit disc, the glyph at 0.6.
cat > "$t/mark.svg" <<EOF
<svg xmlns="http://www.w3.org/2000/svg" width="300" height="300" viewBox="0 0 24 24">
  <circle cx="12" cy="12" r="12" fill="$disc"/>
  <g transform="translate(4.8 4.8) scale(0.6)">
    <path fill="#FFFFFF" fill-rule="evenodd" d="$glyph"/>
  </g>
</svg>
EOF
rsvg-convert -w 300 -h 300 "$t/mark.svg" -o "$t/mark.png"

magick -background none -fill "#FFFFFF" -font "$fonts/Oswald-SemiBold.ttf" \
    -pointsize 112 -kerning 3 label:"FWASSIST" +repage "PNG32:$t/word.png"
magick -background none -fill "$tagline_ink" -font "$fonts/Barlow-Regular.ttf" \
    -pointsize 30 label:"Kleine Helfer für die Feuerwehr" +repage "PNG32:$t/tag.png"
magick -size 110x6 "xc:$accent" +repage "PNG32:$t/rule.png"
magick -size 1x18 "xc:none" +repage "PNG32:$t/gap.png"

# Each piece is its own file and appended, as in Lagebuch: -append takes its width from the first
# image's canvas, so assembling it in one command can crop the wider line.
magick "$t/word.png" "$t/gap.png" "$t/rule.png" "$t/gap.png" "$t/tag.png" \
    -background none -gravity west -append +repage "PNG32:$t/text.png"

magick -size 1024x500 "xc:$app_bg" \
    \( "$t/mark.png" "$t/text.png" -background none -gravity center +smush 48 \) \
    -gravity center -compose over -composite \
    -alpha remove -alpha off \
    -strip -define png:exclude-chunks=date,time \
    "PNG24:$out"

magick identify -format '%f %wx%h %[channels] %B bytes\n' "$out"
