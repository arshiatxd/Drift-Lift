import cv2
import numpy as np
from PIL import Image
import os

base_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox Wireless Controller Images\Default Theme"
template_dir = os.path.join(base_dir, r"Template\Xbox Series X Controller\Black")
press_dir = os.path.join(base_dir, r"Theme Assets\Xbox Series X Active Presses")

# Let's check GIMP XCF files if any, or SVG files to see how the active presses are exported!
# In Inkscape / VSCView, active presses are exported from the full 1534x954 canvas or from objects.
# Let's inspect the active press SVG:
svg_press = os.path.join(base_dir, r"Theme Assets\Xbox Series X Active Presses\Xbox Series X Active Press.svg")
with open(svg_press, 'r', encoding='utf-8') as f:
    svg_content = f.read()

print("SVG Press file size:", len(svg_content))

# Let's check if each button in the SVG has path data or bounding box
import xml.etree.ElementTree as ET
tree = ET.parse(svg_press)
root = tree.getroot()

for elem in root.iter():
    lbl = elem.attrib.get('{http://www.inkscape.org/namespaces/inkscape}label', '')
    eid = elem.attrib.get('id', '')
    tag = elem.tag.split('}')[-1]
    if lbl:
        print(f"Label: {lbl:25s} Tag: {tag:10s} ID: {eid}")
