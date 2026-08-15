import xml.etree.ElementTree as ET
import os

svg_path = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\DualShock 4 Controller Images\Default Theme\Theme SVG\DS4 V2 VSC SVG.svg"
tree = ET.parse(svg_path)
root = tree.getroot()

print("viewBox:", root.attrib.get('viewBox'))
print("width/height:", root.attrib.get('width'), root.attrib.get('height'))

for elem in root.iter():
    lbl = elem.attrib.get('{http://www.inkscape.org/namespaces/inkscape}label', '')
    eid = elem.attrib.get('id', '')
    tag = elem.tag.split('}')[-1]
    if any(k in lbl.lower() or k in eid.lower() for k in ['button', 'dpad', 'd-pad', 'bumper', 'trigger', 'l1', 'l2', 'r1', 'r2', 'triangle', 'circle', 'cross', 'square', 'share', 'option', 'home', 'stick']):
        print(f"Tag: {tag:10s} ID: {eid:25s} Label: {lbl}")
