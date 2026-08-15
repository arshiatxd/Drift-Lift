import xml.etree.ElementTree as ET
import os

svg_path = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox 360 Controller Images\Default Theme\Theme SVG\Xbox 360 VSCView - Black.svg"
tree = ET.parse(svg_path)
root = tree.getroot()

print("viewBox:", root.attrib.get('viewBox'))

# Search for groups and labels
for elem in root.iter():
    lbl = elem.attrib.get('{http://www.inkscape.org/namespaces/inkscape}label', '')
    eid = elem.attrib.get('id', '')
    tag = elem.tag.split('}')[-1]
    if any(k in lbl.lower() or k in eid.lower() for k in ['button', 'stick', 'd-pad', 'dpad', 'bumper', 'trigger', 'guide', 'start', 'back', 'a', 'b', 'x', 'y']):
        print(f"Tag: {tag:10s} ID: {eid:25s} Label: {lbl}")
